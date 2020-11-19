using UnityEngine;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

public class UnZipClass : Core.Kernel.UGameFile
{
	string m_strTargetDir = "";
	ZipFile m_zf = null;
	volatile public bool m_bFinished = false;
	byte[] buffer = null;
	IEnumerator m_zf_enumerator = null;

	ZipState zipState = new ZipState();
    public ZipState m_zipState { get { return zipState; } }
    public System.Exception error{ get; private set;}
    
	public UnZipClass(byte[] ZipData, string strTargetDir)
	{
		Init (new MemoryStream(ZipData), strTargetDir);
	}

    ~UnZipClass()
    {
		Close();
    }

	void Init(Stream data, string strTargetDir){
		try
		{
			m_strTargetDir = strTargetDir;
			m_zf = new ZipFile(data);
			m_zf_enumerator = m_zf.GetEnumerator();

			zipState.m_nZipedFileCount = 0;
			zipState.m_nAllFileCount = (int)m_zf.Count;
		} catch (System.Exception ex) {
			error = ex;
			Close ();
		}
	}

    public void Begin()
    {
        Thread thread = new Thread(new ThreadStart(UnZipThread));
        thread.IsBackground = true;
        thread.Start();
    }

    public void Close()
    {
		m_bFinished = true;
		buffer = null;
		m_zf_enumerator = null;

        ZipFile _zf = m_zf;
		m_zf = null;
        if (_zf != null) {
            _zf.IsStreamOwner = true;
            _zf.Close ();
		}
    }

    public void UnZipThread()
    {
		while (!m_bFinished)
        {
			if (m_zf_enumerator != null && m_zf_enumerator.MoveNext())
			{
				var entry = (ZipEntry)m_zf_enumerator.Current;
				DecodeEntry(entry);
				zipState.m_strCurFileName = entry.Name;
				zipState.m_nZipedFileCount++;
			}
			else
			{
				Close();
			}
        }
    }

	void ClearBuffer(){
		if (buffer == null) {
			buffer = new byte[512];
		} else {
			for (int i = 0; i < buffer.Length; i++) {
				buffer [i] = (byte)0;
			}
		}
	}

    void DecodeEntry(ZipEntry entry)
    {
        try
        {
			string _fn = string.Concat(m_strTargetDir,entry.Name);
            CreateFolder(_fn);
            if (_fn.EndsWith("/"))
                return;

			using(FileStream destStream = File.Create(_fn)){
				using(Stream sourceStrem = m_zf.GetInputStream(entry)){
					ClearBuffer();
					StreamUtils.Copy(sourceStrem,destStream,buffer);
				}
			}
        } catch (System.Exception ex) {
            error = ex;
			Close ();
        }
    }
}
