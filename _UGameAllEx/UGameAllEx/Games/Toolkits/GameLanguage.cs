using UnityEngine;
using System;
// using System.Globalization;

/// <summary>
/// 类名 : Game Language
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2019-12-22 09:23
/// 功能 : 
/// </summary>
public static class GameLanguage
{
	public delegate SystemLanguage DF_LmtLanguage(SystemLanguage language);
	
	static public Type tpSysLanguage = typeof(SystemLanguage);
	static public DF_LmtLanguage m_cfLmtLanguage = null; // 初始化限定语言
	static public SystemLanguage m_curLanguage = SystemLanguage.Chinese;
	
	static public string keyLanguage
	{
		get
        {
            return "Language";
        }
	}
	
	// 系统的语言
    static public SystemLanguage systemLanguage
    {
        get
        {
            return Application.systemLanguage;
        }
    }
	
	// 用户选择语言
	static public SystemLanguage userSelectLanuage
	{
		get
        {
			string key = keyLanguage;
			if(PlayerPrefs.HasKey(key)){
				string _lg = PlayerPrefs.GetString(key);
				return EnumEx.Str2Enum<SystemLanguage>(tpSysLanguage,_lg);
			}
			return systemLanguage;
        }
	}

    // 是否是英文
    static public bool isEN
    {
        get
        {
            return SystemLanguage.English == m_curLanguage;
        }
    }

    // 是否是中文
    static public bool isCN
    {
        get
        {
            return SystemLanguage.Chinese == m_curLanguage;
        }
    }

    //设备系统的地区区域
    static public string curCountryBySystem
    {
		get
        {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
			// return EU_Bridge.getCountryCode();
			return "";
#else
			return RegionInfo.CurrentRegion.ToString();
#endif
        }
    }

	static public string strCurLanguage
    {
		get
        {
			return m_curLanguage.ToString();
        }
    }

    // 转为限定的语言(每新增一个必须添加)
    static public SystemLanguage ToLmtLanguage(SystemLanguage sLg){
		if(m_cfLmtLanguage != null) return m_cfLmtLanguage(sLg);
		
		switch (sLg)
		{
			case SystemLanguage.Chinese:
			case SystemLanguage.ChineseSimplified:
				sLg = SystemLanguage.Chinese;
				break;
			case SystemLanguage.ChineseTraditional: // 中 - 繁体
			case SystemLanguage.Japanese: // 日文
			case SystemLanguage.Korean: // 韩文
			case SystemLanguage.German: // 德文
			case SystemLanguage.French: // 法文
			case SystemLanguage.Portuguese: // 葡萄牙
			case SystemLanguage.Spanish: // 西班牙
			case SystemLanguage.Arabic: // 阿拉伯
				break;
			default:
				sLg = SystemLanguage.English;
				break;
		}
		return sLg;
	}
	
	static public SystemLanguage ReInitLanguage()
    {
		m_curLanguage = ToLmtLanguage(userSelectLanuage);
		return m_curLanguage;
    }
	
	static public void Init()
    {
		ReInitLanguage();
    }
	
	static public void Set(SystemLanguage sLg)
	{
		m_curLanguage = ToLmtLanguage(sLg);
		PlayerPrefs.SetString(keyLanguage, strCurLanguage);
	}
	
	static public bool Set(string language)
	{
		if(EnumEx.IsHas(tpSysLanguage,language))
		{
			SystemLanguage sLg = EnumEx.Str2Enum<SystemLanguage>(tpSysLanguage,language);
			Set(sLg);
			return true;
		}
		return false;
	}
}

