﻿using UnityEngine;
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

        public string m_platformName = ""; // 平台标识 - 字符串
        public string m_platformID = ""; // 平台标识 - ID
        public string m_language = ""; // 基础语言类型
        public string m_urlVersion = ""; // 版本地址
        public string m_uprojVer = ""; // url proj -> uproj (可为空)
        public bool m_isSync2CfgVer = false; // 是否同步 CfgVerstion

        string _kPlatformName = "platform";
		string _kPlatformID = "platformID";
		string _kLanguage = "language";
		string _kUrlVersion = "url_ver";
		string _kUprojVer = "uproj_ver";
        string _kIsSync2CfgVer = "isSync2CfgVer";

        public bool m_isInit{ get; private set; }
        public string m_content { get; private set; }

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
			
			this.m_platformName = LJsonHelper.ToStr(_jsonData,_kPlatformName);
			this.m_platformID = LJsonHelper.ToStr(_jsonData,_kPlatformID);
			this.m_language = LJsonHelper.ToStr(_jsonData,_kLanguage);
			this.m_urlVersion = LJsonHelper.ToStr(_jsonData,_kUrlVersion);
			this.m_uprojVer = LJsonHelper.ToStr(_jsonData,_kUprojVer);
            this.m_isSync2CfgVer = LJsonHelper.ToBool(_jsonData, _kIsSync2CfgVer);
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

        public string ToJson()
        {
            JsonData jd = LJsonHelper.NewJObj();
            jd[_kPlatformName] = this.m_platformName;
            jd[_kPlatformID] = this.m_platformID;
            jd[_kLanguage] = this.m_language;
            jd[_kUrlVersion] = this.m_urlVersion;
            jd[_kUprojVer] = this.m_uprojVer;
            jd[_kIsSync2CfgVer] = this.m_isSync2CfgVer;
            return jd.ToJson();
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

        static public string EditorFPath()
        {
            if (UGameFile.m_isIOS)
                return string.Format("{0}/Plugins/iOS/assets/{1}", Application.dataPath, m_defFn);

            return string.Format("{0}/Plugins/Android/assets/{1}", Application.dataPath, m_defFn);
        }

        static public CfgPackage InitPackage(System.Action callBack)
        {
            if (UGameFile.m_isEditor)
            {
                string path = EditorFPath();
                string _data = UGameFile.GetText4File(path);
                instance.Init(_data);
                if (callBack != null)
                    callBack();
            }
            else
            {
                EU_Bridge.SendAndCall("{\"cmd\":\"getPackageInfo\",\"filename\":\"" + m_defFn + "\"}",(strData)=> {
                    instance.Init(strData);
                    if (callBack != null)
                        callBack();
                });
            }
            return instance;
        }

        static public void SaveEditor(string path)
        {
            if(string.IsNullOrEmpty(path))
                path = EditorFPath();

            string _cont = instance.ToJson();
            UGameFile.CreateText(path, _cont);
        }
	}
}
