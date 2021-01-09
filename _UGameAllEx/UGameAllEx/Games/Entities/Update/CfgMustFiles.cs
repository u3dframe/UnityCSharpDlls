using System.Collections.Generic;

namespace Core.Kernel
{
	/// <summary>
	/// 类名 : 包体 - 必须的资源文件
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2019-08-20 19:35
	/// 功能 : 用于判断哪些文件是必须下载
	/// </summary>
	public class CfgMustFiles  {
		static public readonly string m_defFn = "must_files.txt";
		public List<string> m_lFiles{ get; private set; }
		public List<string> m_lFolders{ get; private set; }

		public string m_content{ get; private set; }
		public bool m_isInit{ get; private set; }
        
		private CfgMustFiles(){
		}

		public void Init(string content){
			if(content == null){
				return;
			}
			if (!this.m_isInit) {
				this.m_isInit = true;
				this.m_lFiles = new List<string> ();
				this.m_lFolders = new List<string> ();
			}
			this.m_lFiles.Clear ();
			this.m_lFolders.Clear ();
			this.m_content = content;
			_OnInit (this.m_content);
		}

		protected void _OnInit(string content){
			if(string.IsNullOrEmpty(content)){
				return;
			}
			string[] _vals = content.Split ("\r\n\t".ToCharArray (), System.StringSplitOptions.RemoveEmptyEntries);
			int _lens = _vals.Length;
			string _str = null;
			for (int i = 0; i < _lens; i++) {
				_str = _vals [i];
				if (_str.IndexOf (".") == -1) {
					if (!m_lFolders.Contains (_str)) {
						m_lFolders.Add (_str);
					}
				} else {
					if (!m_lFiles.Contains (_str)) {
						m_lFiles.Add (_str);
					}
				}
			}
		}

		public void CloneFromOther(CfgMustFiles other){
			this.m_content = other.m_content;
			this.m_isInit = other.m_isInit;
			this.m_lFiles = other.m_lFiles;
			this.m_lFolders = other.m_lFolders;
		}

		public void Load(string fn){
			Init (UGameFile.GetText (fn));
		}

		/// <summary>
		/// 加载默认的资源
		/// </summary>
		public void LoadDefault(){
			Load (m_defFn);
		}

		/// <summary>
		/// 判断是否是必须文件
		/// </summary>
		public bool IsMust(string resName){
			if (!m_isInit || (m_lFiles.Count <= 0 && m_lFolders.Count <= 0))
				return false;
			
			if (string.IsNullOrEmpty (resName))
				return false;

			string _str = "";
			int lens = m_lFolders.Count;
			for (int i = 0; i < lens; i++) {
				_str = m_lFolders [i];
				if (resName.StartsWith (_str))
					return true;
				if (resName.Contains(_str))
					return true;
			}

			lens = m_lFiles.Count;
			for (int i = 0; i < lens; i++) {
				_str = m_lFiles [i];
				if (resName.Equals (_str))
					return true;
                if (resName.EndsWith(_str))
                    return true;
            }
			return false;
		}

        public bool IsHas(string resName) {
            return IsMust(resName);
        }

		static CfgMustFiles _instance;
		static public CfgMustFiles instance{
			get{ 
				if (_instance == null) {
					_instance = Builder();
				}
				return _instance;
			}
		}

        static public CfgMustFiles Builder()
        {
            return new CfgMustFiles();
        }
        
        static public CfgMustFiles Builder(string fn)
        {
            CfgMustFiles ret = Builder();
            if (!string.IsNullOrEmpty(fn))
            {
                string _cont = UGameFile.GetText(fn);
                ret.Init(_cont);

            }
            return ret;
        }

        static public CfgMustFiles BuilderFp(string fp)
        {
            CfgMustFiles ret = Builder();
            if (!string.IsNullOrEmpty(fp))
            {
                string _cont = UGameFile.GetText4File(fp);
                ret.Init(_cont);

            }
            return ret;
        }

        static public bool IsCfgFile(string fn)
        {
            if (string.IsNullOrEmpty(fn))
                return false;
            return fn.EndsWith(m_defFn);
        }
    }
}
