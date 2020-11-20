using UnityEngine;
using LitJson;

namespace Core.Kernel
{
	/// <summary>
	/// 类名 : 包体配置
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-21 10:35
	/// 功能 : 放在plugins下面 (android/assets里面cfg_xx.json里面)
	/// </summary>
	public class CfgPackage {
        static public readonly string m_defFn = "cfg_game_package.json";

        // 平台标识 - 字符串
        public string m_platformName = "";

		// 平台标识 - ID
		public string m_platformID = "";

		// 基础语言类型
		public string m_language = "";

		// 版本地址
		public string m_urlVersion = "";

		// url proj -> uproj (可为空)
		public string m_uprojVer = "";

		public string m_content{ get; private set; }

		const string m_kPlatformName = "platform";
		const string m_kPlatformID = "platformID";
		const string m_kLanguage = "language";
		const string m_kUrlVersion = "url_ver";
		const string m_kUprojVer = "uproj_ver";

		public bool m_isInit{ get; private set; }

		private CfgPackage(){
		}

		public CfgPackage Init(string content){
			if(!string.IsNullOrEmpty(content)){
			    this.m_isInit = true;
			    this.m_content = content;
			    _OnInit (this.m_content);
			}
			return this;
		}

		protected virtual void _OnInit(string content){
			JsonData _jsonData = LJsonHelper.ToJData(content);
			if (_jsonData == null)
				return;
			
			this.m_platformName = LJsonHelper.ToStr(_jsonData,m_kPlatformName);
			this.m_platformID = LJsonHelper.ToStr(_jsonData,m_kPlatformID);
			this.m_language = LJsonHelper.ToStr(_jsonData,m_kLanguage);
			this.m_urlVersion = LJsonHelper.ToStr(_jsonData,m_kUrlVersion);
			this.m_uprojVer = LJsonHelper.ToStr(_jsonData,m_kUprojVer);
		}

		public void CloneFromOther(CfgPackage other){
			this.m_platformName = other.m_platformName;
			this.m_platformID = other.m_platformID;
			this.m_language = other.m_language;
			this.m_urlVersion = other.m_urlVersion;
			this.m_uprojVer = other.m_uprojVer;
			this.m_content = other.m_content;
			this.m_isInit = other.m_isInit;
		}

        static CfgPackage _instance;
        static public CfgPackage instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Builder();
                }
                return _instance;
            }
        }

        static public CfgPackage Builder()
        {
            return new CfgPackage();
        }

        static public CfgPackage Builder(string content)
        {
            return new CfgPackage().Init(content);
        }

        static public CfgPackage InitPackage()
        {
            if (UGameFile.m_isEditor)
            {
                string path = string.Format("{0}/Plugins/Android/assets/{1}", Application.dataPath, m_defFn);
                string _data = null;
                if (UGameFile.m_isIOS)
                {
                    path = string.Format("{0}/Plugins/iOS/{1}", Application.dataPath, m_defFn);
                }
                _data = UGameFile.GetText4File(path);
                instance.Init(_data);
            }
            else
            {
                EU_Bridge.SendAndCall("{\"cmd\":\"getPackageInfo\",\"filename\":\"" + m_defFn + "\"}",(strData)=> {
                    instance.Init(strData);
                });
            }
            return instance;
        }
	}
}
