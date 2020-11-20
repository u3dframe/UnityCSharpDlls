using System.IO;
using System.Collections.Generic;
using LitJson;

namespace Core.Kernel
{

	/// <summary>
	/// 类名 : 包体下载信息
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2018-06-05 15:35
	/// 功能 : 
	/// </summary>
	public class ApkIpaInfo{
		// 渠道
		public string m_channel;

		// 包体下载地址
		public string m_down_url;
	}

	/// <summary>
	/// 类名 : 版本配置
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-07 10:35
	/// 功能 : 1.多个version管理自身更新地址,2.只有一个version(多版本地址)
	/// 默认是多版本，各管各的信息
	/// </summary>
	public class CfgVersion  {        
		static public readonly string m_defFileName = "version.txt";

		protected static string URL_HEAD = "http://";

		// 资源版本号(yyMMddHHmmss)
		public string m_resVerCode = "";

		// 上一次资源版本号(yyMMddHHmmss)
		public string m_lastResVerCode = "";

		// 游戏版本号
		public string m_gameVerCode = "";

		// git,或者svn版本号
		public string m_svnVerCode = "";

		// 版本地址(放入Package文件里)
		public string m_urlVersion = "";

		// 文件列表地址
		public string m_urlFilelist = "";

		// file list 标识
		public string m_codeFilelist = "";

		// 大版本信息(判断整包更新)
		public string m_bigVerCode = "";

		// 下载apk,ipa文件地址 - 渠道列表
		public List<ApkIpaInfo> m_lApkIpa = new List<ApkIpaInfo>();

		// 下载apk,ipa文件地址 - 默认地址
		public string m_urlNewApkIpa = "";

		// 服务器入口地址(登录服务器,或者取得服务器列表)
		private string _m_urlSv = "";
		protected string[] _arrsSvs = null;
		protected int _indSvs = -1;
		public string m_urlSv{
			get{ return _m_urlSv; }
			set{
				if (string.IsNullOrEmpty(value)) {
					_arrsSvs = null;
				} else if (!value.Equals (_m_urlSv)) {
					_arrsSvs = UGameFile.SplitDivision(value,true);
					_indSvs = -1;
				}
				_m_urlSv = value;
			}
		}

		public string m_pkgVersion = ""; // 运行从cfgPacage中得到，打包得时候直接用
		public string m_pkgFilelist = "";
		public string m_pkgFiles = "";
		public string m_keyLua = ""; // lua encode key

		// 服务器列表
		public string urlSvlist{
			get{
				return UGameFile.GetUrl(_arrsSvs, m_urlSv, ref _indSvs);
			}
		}

		// 文件路径
		protected string m_filePath = "";
		public string m_content{ get; private set; }

		private string _kBigVerCode = "bigVersion";
		private string _kResVerCode = "resVersion";
		private string _kGameVerCode = "version";
		private string _kLastResVerCode = "lastResVersion";

		private string _kUrlNewApkIpa = "url_newdown";
		private string _kUrlNewApkIpa4Chn = "url_newdown_chn";

		private string _kUrlFilelist = "fl_url";
		private string _kPkgFilelist = "fl_pkg";
		private string _kCodeFilelist = "fl_code";

		private string _kPkgFiles = "fls_pkg";
		private string _kLua = "key_lua";

		
		private JsonData m_jsonData = null;

		private CfgVersion(){
            this.RefreshResVerCode();
            // this.RefreshBigVerCode ();
        }

        public void Load(string fn){
			this.m_filePath = UGameFile.curInstance.GetFilePath (fn);
			Init (UGameFile.curInstance.GetText (fn));
		}

		public void Init(string content){
            if (!string.IsNullOrEmpty(content)){
                this.m_content = content;
                _OnInit(this.m_content);
            }

            this.SyncByCfgPkg();
        }

		protected virtual void _OnInit(string content){
            this.m_jsonData = LJsonHelper.ToJData(content);
			JsonData _jsonData = this.m_jsonData;
			if (_jsonData == null)
				return;
			this.m_bigVerCode = LJsonHelper.ToStr(_jsonData,_kBigVerCode);
			this.m_resVerCode = LJsonHelper.ToStr(_jsonData,_kResVerCode);
			this.m_gameVerCode = LJsonHelper.ToStr(_jsonData,_kGameVerCode);
            this.m_lastResVerCode = LJsonHelper.ToStr(_jsonData, _kLastResVerCode);

            this.m_urlNewApkIpa = LJsonHelper.ToStr(_jsonData,_kUrlNewApkIpa);
			if (_jsonData.Keys.Contains (_kUrlNewApkIpa4Chn)) {
				_ToList4ApkIpaChn (_jsonData [_kUrlNewApkIpa4Chn]);
			}

			this.m_urlFilelist = LJsonHelper.ToStr(_jsonData,_kUrlFilelist);
			this.m_pkgFilelist = LJsonHelper.ToStr(_jsonData,_kPkgFilelist);
			this.m_codeFilelist = LJsonHelper.ToStr(_jsonData,_kCodeFilelist);

			this.m_pkgFiles = LJsonHelper.ToStrDef(_jsonData,_kPkgFiles,"files");
			this.m_keyLua = LJsonHelper.ToStr(_jsonData,_kLua);
        }
		
		void _ToList4ApkIpaChn(JsonData jsonData){
			this.m_lApkIpa.Clear();

			string _tmpJson = jsonData.ToString();
			jsonData = LJsonHelper.ToJData(_tmpJson);
			if (jsonData == null || !jsonData.IsArray)
				return;
			for (int i = 0; i < jsonData.Count; i++) {
				_tmpJson = jsonData [i].ToJson ();
				AddApkIpa (LJsonHelper.ToObject<ApkIpaInfo> (_tmpJson));
			}
		}

        public string NowYMDHms()
        {
            return System.DateTime.Now.ToString("yyMMddHHmmss");
        }

		/// <summary>
		/// 刷新资源版本号
		/// </summary>
		public void RefreshResVerCode(){
            if (string.IsNullOrEmpty(this.m_bigVerCode))
                this.RefreshBigVerCode();

			this.m_resVerCode = NowYMDHms();
		}

		public void RefreshBigVerCode(){
			this.m_bigVerCode = NowYMDHms();
		}

		/// <summary>
		/// 加载默认的资源
		/// </summary>
		public CfgVersion LoadDefault(){
			Load (m_defFileName);
            return this;
		}

		/// <summary>
		/// 加载默认的资源 4 Editor
		/// </summary>
		public CfgVersion LoadDefault4EDT(){
			LoadDefault ();
			m_lastResVerCode = m_resVerCode;
			RefreshResVerCode ();
            return this;
		}
		
		public bool Save(){
			if (string.IsNullOrEmpty (this.m_filePath)) {
				this.m_filePath = UGameFile.curInstance.GetFilePath (m_defFileName);
			}
			
			UGameFile.CreateFolder (this.m_filePath);
		
			try {
				if(this.m_jsonData == null)
					this.m_jsonData = LJsonHelper.NewJObj();
				this.m_jsonData.Clear();
				
				this.m_jsonData[_kBigVerCode] = this.m_bigVerCode;
				this.m_jsonData[_kResVerCode] = this.m_resVerCode;
				this.m_jsonData[_kGameVerCode] = this.m_gameVerCode;
                this.m_jsonData[_kLastResVerCode] = this.m_lastResVerCode;

                this.m_jsonData[_kUrlNewApkIpa] = this.m_urlNewApkIpa;
				this.m_jsonData[_kUrlNewApkIpa4Chn] = LJsonHelper.ToJson(this.m_lApkIpa);

				this.m_jsonData[_kUrlFilelist] = this.m_urlFilelist;
				this.m_jsonData[_kPkgFilelist] = this.m_pkgFilelist;
				this.m_jsonData[_kCodeFilelist] = this.m_codeFilelist;

				this.m_jsonData[_kPkgFiles] = this.m_pkgFiles;
				this.m_jsonData[_kLua] = this.m_keyLua;
				
				File.WriteAllText (this.m_filePath, this.m_jsonData.ToJson());
				return true;
			} catch{
			}
			return false;
		}
		
		/// <summary>
		/// 保存到默认路径
		/// </summary>
		public bool SaveDefault(){
			this.m_filePath = "";
			return Save ();
		}

		public bool IsNewDown(CfgVersion other){
			if (other == null)
				return false;
			
			if (string.IsNullOrEmpty (other.m_bigVerCode))
				return false;
			int v = other.m_bigVerCode.CompareTo (m_bigVerCode);
			return v > 0;
		}

		public bool IsUpdate(bool isCheckResUrl = false){
			if (string.IsNullOrEmpty (m_resVerCode))
				return false;
			
			// if (string.IsNullOrEmpty (m_urlVersion) || URL_HEAD.Equals(m_urlVersion) || m_urlVersion.IndexOf(URL_HEAD) != 0)
			// 	return false;

			if (string.IsNullOrEmpty (m_urlFilelist) || URL_HEAD.Equals(m_urlFilelist) || m_urlFilelist.IndexOf(URL_HEAD) != 0)
				return false;

			return true;
		}

		public bool IsUpdate4Other(CfgVersion other){
			if (other == null)
				return false;
			
			if (string.IsNullOrEmpty (other.m_resVerCode))
				return false;
			// A.CompareTo(B) 比较A在B的前-1,后1,或相同0
			int v = other.m_resVerCode.CompareTo (this.m_resVerCode);
			if (v > 0 && other.IsUpdate(true)) {
				return true;
			}

			return false;
		}

		public void CloneFromOther(CfgVersion other){
			this.m_content = other.m_content;

			this.m_bigVerCode = other.m_bigVerCode;
			this.m_resVerCode = other.m_resVerCode;
			this.m_gameVerCode = other.m_gameVerCode;
            this.m_lastResVerCode = other.m_lastResVerCode;

			this.m_urlNewApkIpa = other.m_urlNewApkIpa;
			this.m_lApkIpa = other.m_lApkIpa;

			this.m_urlFilelist = other.m_urlFilelist;
			this.m_pkgFilelist = other.m_pkgFilelist;
			this.m_codeFilelist = other.m_codeFilelist;

			this.m_pkgFiles = other.m_pkgFiles;
            this.m_keyLua = other.m_keyLua;

            this.m_urlVersion = other.m_urlVersion;
			this.m_pkgVersion = other.m_pkgVersion;
			this.m_urlSv = other.m_urlSv;
            this.m_svnVerCode = other.m_svnVerCode;
		}

		public ApkIpaInfo GetApkIpa(string channel){
			if (string.IsNullOrEmpty (channel))
				return null;

			int lens = m_lApkIpa.Count;
			ApkIpaInfo tmp;
			for (int i = 0; i < lens; i++) {
				tmp = m_lApkIpa [i];
				if (tmp.m_channel.Equals (channel))
					return tmp;
			}
			return null;
		}

		public string GetApkIpaDownUrl(string channel){
			ApkIpaInfo tmp = GetApkIpa (channel);
			if (tmp != null)
				return tmp.m_down_url;
			return "";
		}
		
		public void AddApkIpa(ApkIpaInfo one){
			if (one == null)
				return;
			ApkIpaInfo tmp = GetApkIpa (one.m_channel);
			if (tmp != null)
				return;
			
			m_lApkIpa.Add (one);
		}

        public void SyncByCfgPkg()
        {
            this.m_urlVersion = CfgPackage.instance.m_urlVersion; // 从包体里面获取
            this.m_pkgVersion = CfgPackage.instance.m_uprojVer; // 从包体里面获取
            bool _isSync = CfgPackage.instance.m_isSync2CfgVer;
            if (_isSync || string.IsNullOrEmpty(this.m_urlFilelist))
                this.m_urlFilelist = this.m_urlVersion;

            if (_isSync || string.IsNullOrEmpty(this.m_pkgFilelist))
            {
                this.m_pkgFilelist = this.m_pkgVersion;
                this.m_pkgFiles = string.Format("{0}files", UGameFile.ReUrlEnd(this.m_pkgFilelist));
            }
        }
	
        static public CfgVersion Builder()
        {
            return new CfgVersion();
        }
        
        static CfgVersion _instance;
		static public CfgVersion instance{
			get{ 
				if (_instance == null) {
					_instance = Builder();
				}
				return _instance;
			}
		}
	}
}
