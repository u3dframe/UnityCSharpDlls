using System.Collections.Generic;
using System.IO;

namespace Core.Kernel
{

    /// <summary>
    /// 类名 : 路径工具
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2017-03-07 09:29
    /// 功能 : 
    /// </summary>
    public class EL_Path
    {
        static string[] m_ignoreFiles = {
            ".manifest",
            ".meta",
        };

        static bool _IsIgnoreFile(string fp)
        {
            for (int i = 0; i < m_ignoreFiles.Length; i++)
            {
                if (fp.Contains(m_ignoreFiles[i]))
                {
                    return true;
                }
            }
            return false;
        }

        static public void Init(string path)
        {
            instance.DoInit(path);
        }

        static public void Append(string path)
        {
            instance.Recursive(path);
        }

        static public void Clear()
		{
			instance.DoClear(); 
		}

        static public List<string> folders
        {
            get
            {
                return instance.m_folders;
            }
        }

        static public List<string> files
        {
            get
            {
                return instance.m_files;
            }
        }

        static EL_Path _instance = null;
        static public EL_Path instance
        {
            get
            {
                if (_instance == null)
                    _instance = builder;

                return _instance;
            }
        }

        static public EL_Path builder { get { return new EL_Path(); } }

        public bool m_isAllFiles = false;
        /// <summary>
        /// 文件夹地址
        /// </summary>
        public List<string> m_folders = new List<string>();

        /// <summary>
        /// 文件地址
        /// </summary>
        public List<string> m_files = new List<string>();

        /// <summary>
        /// 取得该路径下面的-所有文件，以及该下面的所以文件夹
        /// </summary>
        /// <param name="path">文件夹路径</param>
        public void Recursive(string path)
        {
            if(!Directory.Exists(path))
                return;
            
            string[] names = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);
            foreach (string filename in names)
            {
                if (!m_isAllFiles && _IsIgnoreFile(filename)) continue;
                m_files.Add(filename.Replace('\\', '/'));
            }
            foreach (string dir in dirs)
            {
                m_folders.Add(dir.Replace('\\', '/'));
                Recursive(dir);
            }
        }

        public void DoClear()
        {
            m_files.Clear();
            m_folders.Clear();
        }

        public EL_Path DoInit(string path,bool isAllFiles)
        {
            this.m_isAllFiles = isAllFiles;
            DoClear();
            Recursive(path);
			return this;
        }

        public EL_Path DoInit(string path){
            return this.DoInit(path,false);
        }
    }
}