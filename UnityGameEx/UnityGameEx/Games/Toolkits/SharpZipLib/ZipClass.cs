using UnityEngine;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;

public class ZipClass : Core.Kernel.UGameFile
{
    // FileStream m_targetFile = null;
    ZipFile m_zipFile = null;

    List<string> m_listWaitAddFile = new List<string>();
    List<string> m_listEntryName = new List<string>();
    Object m_LockObject = new Object();

    ZipState zipState = new ZipState();
    public ZipState m_zipState { get { return zipState; } }
    public bool m_bClosed { get; private set; }
    public bool m_bFinished { get; private set; }
    public System.Exception error { get; private set; }

    public ZipClass(string strTargetPath)
    {
        DelFile(strTargetPath);
        CreateFolder(strTargetPath);

        FileStream m_targetFile = File.Create(strTargetPath);
        m_zipFile = ZipFile.Create(m_targetFile);
		m_zipFile.UseZip64 = UseZip64.Off;
    }

    ~ZipClass()
    {
        Close();
    }

    public void Close()
    {
        if (m_bClosed)
            return;

        m_bClosed = true;
        m_listEntryName.Clear();
        m_listWaitAddFile.Clear();
        // m_targetFile.Close();
        m_zipFile.Close();
    }

    public void AddDir(string strDirPath)
    {
        strDirPath = ReFnPath(strDirPath,true);

        if (!IsFolder(strDirPath))
        {
            Debug.LogError("添加失败，文件夹不存在 [" + strDirPath + "]");
            return;
        }

        string[] dirs = GetFns4Folders(strDirPath);
        if (!IsNullOrEmpty(dirs))
        {
            foreach (string dir in dirs)
            {
                AddDir(dir);
            }
        }
        
        string[] files = GgetFns4Files(strDirPath);
        if (!IsNullOrEmpty(files))
        {
            foreach (string _str in files)
            {
                AddFile(_str, _str);
            }
        }
    }

    public bool AddFile(string strFilePath, string strEnptyName = "")
    {
        strFilePath = ReplaceSeparator(strFilePath);
        if (!IsFile(strFilePath))
        {
            Debug.LogError("添加失败，文件不存在 [" + strFilePath + "]");
            return false;
        }

        lock (m_LockObject)
        {
            m_listWaitAddFile.Add(strFilePath);

            if(!string.IsNullOrEmpty(strEnptyName))
                strEnptyName = GetFileName(strEnptyName);

            m_listEntryName.Add(strEnptyName);
            zipState.m_nAllFileCount++;
            return true;
        }
    }

    public void Begin()
    {
        Thread thread = new Thread(new ThreadStart(ZipThread));
        thread.Start();
    }

    void ZipThread()
    {
        while (true)
        {
            m_bFinished = m_bFinished || m_listWaitAddFile.Count <= 0;
            if (m_bFinished)
            {
                Close();
                break;
            }

            try
            {
                string strFilePath = m_listWaitAddFile[0];
                string strEntryName = m_listEntryName[0];
                m_listWaitAddFile.RemoveAt(0);
                m_listEntryName.RemoveAt(0);

                m_zipFile.BeginUpdate();
                m_zipFile.Add(strFilePath, strEntryName);
                m_zipFile.CommitUpdate();

                zipState.m_strCurFileName = strEntryName;
                zipState.m_nZipedFileCount++;
            }
            catch (System.Exception e)
            {
                error = e;
                m_bFinished = true;
            }
        }
    }
}
