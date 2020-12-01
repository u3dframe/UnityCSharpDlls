using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Core.Kernel
{
	/// <summary>
	/// 类名 : 文件列表配置
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-08 14:45
	/// 功能 : 
	/// </summary>
	public class CfgFileList  {

		static public readonly string m_defFileName = "filelist.txt";
		static public readonly string m_defFileName2 = "filelist2.txt"; // 需要下载的
		static public readonly string m_defFileName3 = "filelist3.txt"; // 已下载的
        
		public ResInfo m_manifest { get; private set;} // 当前的主要资源关系文件
        public ListDict<ResInfo> m_data { get; private set; }
        // 文件路径
        protected string m_filePath = "";
		public string m_content{ get; private set; }
        
		private CfgFileList(){
            this.m_data = new ListDict<ResInfo>(true);
        }
        
        public CfgFileList Load(string fn){
			this.m_filePath = UGameFile.curInstance.GetFilePath (fn);
			Init (UGameFile.GetText (fn));
			return this;
		}

		public CfgFileList LoadFP(string fp){
			this.m_filePath = fp;
			string _content = "";
			if (File.Exists (fp))
				_content = File.ReadAllText (fp);
			
			Init (_content);
			return this;
		}

		public void Init(string content){
			if(string.IsNullOrEmpty(content)){
				return;
			}

			Clear ();

			this.m_content = content;

            string[] _vals = UGameFile.SplitRow(content);
			for (int i = 0; i < _vals.Length; i++) {
                Add(new ResInfo(_vals[i]));
            }
		}

		public void Add(ResInfo info){
			if (info.m_size == 0)
				return;

            bool isOkey = this.m_data.Add(info.m_curName, info);
            if (isOkey)
            {
                if (info.isManifest)
                {
                    this.m_manifest = info;
                }
            }
            else if(info.m_size > 0 && !string.IsNullOrEmpty(info.m_compareCode))
            {
                ResInfo _old = this.GetInfo(info.m_curName);
                _old.CloneFromOther(info);             
            }
            else
            {
                Debug.LogErrorFormat("========== Filelist Add error has [{0}],[{1}],[{2}]", info.m_curName, info.m_size, info.m_compareCode);
            }
        }

        public ResInfo GetInfo(string key)
        {
            return this.m_data.Get(key);
        }

        public int GetDataCount()
        {
            return this.m_data.Count();
        }

        public List<ResInfo> GetList(bool isNew)
        {
            if (isNew)
                return new List<ResInfo>(this.m_data.m_list);
            return this.m_data.m_list;
        }

        public bool Remove(ResInfo info){
			if (info == null)
				return false;

			return Remove (info.m_curName);
		}

		public bool Remove(string key){
			return this.m_data.Remove(key);
        }

		public void Put2ListEnd(ResInfo info){
			if (info == null)
				return;

            this.m_data.Remove(info.m_curName);
			this.Add (info);
		}

		void ClearBuilder(System.Text.StringBuilder builder){
			if (builder == null) {
				return;
			}
			if (builder.Length > 0) {
				builder.Remove (0, builder.Length);
			}

			builder.Length = 0;
		}

		public void ToContent(){
            List<ResInfo> m_lFiles = this.GetList(true);
            if (m_lFiles.Count > 0) {
                int _count = m_lFiles.Count;
                System.Text.StringBuilder _build = new System.Text.StringBuilder ();
				for (int i = 0; i < _count; i++) {
					_build.Append (m_lFiles [i].ToString ()).Append ("\n");
				}
				this.m_content = _build.ToString ();
				ClearBuilder (_build);
			} else {
				this.m_content = "";
			}
		}

		public bool Save(){
			try {
				if (string.IsNullOrEmpty (this.m_filePath)) {
					this.m_filePath = UGameFile.curInstance.GetFilePath (m_defFileName);
				}
                List<ResInfo> m_lFiles = this.GetList(true);
                if (string.IsNullOrEmpty (this.m_content) && m_lFiles.Count <= 0){
					// UGameFile.curInstance.DeleteFile(this.m_filePath,true);
					return false;
				}

                UGameFile.CreateFolder (this.m_filePath);

				using (FileStream stream = new FileStream (this.m_filePath, FileMode.Create)) {
					using (StreamWriter writer = new StreamWriter (stream)) {
						if(string.IsNullOrEmpty(this.m_content)){
							for (int i = 0; i < m_lFiles.Count; i++) {
								writer.WriteLine (m_lFiles [i].ToString ());
							}
						}else{
							writer.Write (this.m_content);
						}
					}
				}
				return true;
			} catch{
			}
			return false;
		}

		public bool SaveByTContent(){
			ToContent ();
			return Save ();
		}

		/// <summary>
		/// 保存到默认路径
		/// </summary>
		public bool Save2Default(){
			this.m_filePath = "";
			return SaveByTContent ();
		}

		public void Clear(){
			this.m_content = "";
			this.m_data.Clear ();
		}

		public void CloneFromOther(CfgFileList other){
			this.Clear ();

			this.m_filePath = other.m_filePath;
			this.m_content = other.m_content;

			ResInfo _info;
            List<ResInfo> m_lFiles = other.GetList(true);
            for (int i = 0; i < m_lFiles.Count; i++) {
				_info = m_lFiles[i];
				this.Add (_info);
			}
		}

		public bool IsHas(string  realName){
			if (string.IsNullOrEmpty (realName))
				return true;

			return this.m_data.ContainsKey(realName);
		}

		public bool IsHas(ResInfo info){
			if (info == null)
				return true;
			return IsHas(info.m_curName);
		}

		/// <summary>
		/// 保存到已下载文件夹里面
		/// </summary>
		public void Save2Downed(ResInfo info){
			if (this.Remove (info)) {
				instanceDowned.Add (info);
				instanceDowned.SaveByTContent ();
			}
		}

		public void ReInitDowning(){
			if (_downing == null)
				return;
			
			_downing.Clear ();
            List<ResInfo> m_lFiles = instanceNeedDown.GetList(true);
            int lens = m_lFiles.Count;
			ResInfo _info = null, _info2 = null;
			for (int i = 0; i < lens; i++) {
				_info = m_lFiles [i];
                if (_info == null)
                    continue;

                _info2 = instanceDowned.GetInfo(_info.m_curName);

                if (_info2 == null || !_info.m_compareCode.Equals(_info2.m_compareCode)) {
					_downing.Add (_info);
				}
			}
		}

		// 用linq和lb表达式取得size最大的几个对象
		public List<ResInfo> GetList4MaxSize(int limit){
			limit = Mathf.Max (1, limit);
            var _slist = this.GetList(true);
			List<ResInfo> list = _slist.OrderByDescending (s=>s.m_size).ToList ();
			limit  = Mathf.Min (list.Count, limit);
			return list.GetRange(0,limit);
		}

        public CfgFileList LoadDefault()
        {
            return this.Load(m_defFileName);
        }

        static public CfgFileList Builder()
        {
            return new CfgFileList();
        }

        static public CfgFileList Builder(string fn)
        {
            return new CfgFileList().Load(fn);
        }

        static public CfgFileList BuilderFp(string fpath)
        {
            return new CfgFileList().LoadFP(fpath);
        }

        static public CfgFileList BuilderDefault()
        {
            return Builder(m_defFileName);
        }

        static CfgFileList _instance;
		/// <summary>
		/// 此单例在打包的时候用
		/// </summary>
		static public CfgFileList instance{
			get{ 
				if (_instance == null) {
					_instance = Builder();
				}
				return _instance;
			}
		}

		static CfgFileList _ndDown;
		static public CfgFileList instanceNeedDown{
			get{ 
				if (_ndDown == null) {
					_ndDown = Builder(m_defFileName2);
				}
				return _ndDown;
			}
		}

		static CfgFileList _downed;
		static public CfgFileList instanceDowned{
			get{ 
				if (_downed == null) {
					_downed = Builder(m_defFileName3);
				}
				return _downed;
			}
		}

		static CfgFileList _downing;
		static public CfgFileList instanceDown{
			get{
				if (_downing == null) {
					_downing = Builder();
					_downing.ReInitDowning ();
				}
				return _downing;
			}
		}

	}
}
