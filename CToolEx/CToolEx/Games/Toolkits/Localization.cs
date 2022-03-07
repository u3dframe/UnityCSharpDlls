using UnityEngine;
using System.Collections.Generic;
using Core.Kernel;
using Core.Kernel.Beans;


/// <summary>
/// 类名 : 本地话
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-27 16:37
/// 功能 : key值对应相应的语言
/// 修订 : 2020-12-20 09:37
/// </summary>
static public class Localization
{
    static Dictionary<string, EN_KVal> mDicLgs = new Dictionary<string, EN_KVal>();
    static string mLanguage = null;
    static EN_KVal mCurr = null;
    static public System.Action onLocalize; // 语言改变时候的通知
    static public bool mIsLogErr = true;
    static public string mHeader = "lanuage/";

    /// <summary>
    /// Name of the currently active language.
    /// </summary>
    static public string language
    {
        get
        {
            if (string.IsNullOrEmpty(mLanguage))
                LoadAndSelect(mLanguage);
            return mLanguage;
        }
        set
        {
            if (mLanguage != value)
            {
                mLanguage = value;
                LoadAndSelect(mLanguage);
            }
        }
    }

    static private EN_KVal GetEntity(string key)
    {
        if (!string.IsNullOrEmpty(key) && mDicLgs.ContainsKey(key))
            return mDicLgs[key];
        return null;
    }

    static public bool ReLoad(string language, bool isCsv)
    {
        EN_KVal temp = GetEntity(language);
        if (temp == null)
        {
            temp = new EN_KVal();
            mDicLgs.Add(language, temp);
        }
        return temp.ReLoad(language, isCsv, mHeader);
    }

    static bool Load(string val, bool isCsv)
    {
        if (string.IsNullOrEmpty(val)) return false;
        if (mDicLgs.ContainsKey(val)) return true;
        return ReLoad(val, isCsv);
    }

    static void ReLangueInfo(string val)
    {
        if (mDicLgs.ContainsKey(val))
        {
            mCurr = mDicLgs[val];
            mLanguage = val;
            if (onLocalize != null) onLocalize();
        }
    }

    static public bool LoadAndSelect(string val, bool isCsv = true)
    {
        mCurr = null;
        string vRef = val;
        if (string.IsNullOrEmpty(vRef)) vRef = GameLanguage.strCurLanguage;
        if (Load(vRef, isCsv))
        {
            ReLangueInfo(vRef);
            return true;
        }
        if (mIsLogErr)
            Debug.LogErrorFormat("==== Localization not has language = [{0}],SrcLanuage = [{1}] ", vRef, val);
        return false;
    }

    static public bool IsHasCurr()
    {
        return mCurr != null;
    }

    static public bool Exists(string key)
    {
        return IsHasCurr() && mCurr.Exists(key);
    }

    static public string Get(string key, string tagName = null, bool isCsv = true)
    {
        if (string.IsNullOrEmpty(key)) return null;
        if (mLanguage == null) mLanguage = language;
        if (mLanguage == null)
        {
            if (mIsLogErr)
                Debug.LogError("No localization data present");
            return null;
        }

        if (!string.IsNullOrEmpty(tagName))
        {
            string _key = UGameFile.ReSBegEnd(tagName, mLanguage);
            if (Load(_key, isCsv))
            {
                EN_KVal _en = GetEntity(_key);
                if (_en != null && _en.Exists(key))
                    return _en.Get(key);
            }
        }

        if (Exists(key))
            return mCurr.Get(key);
        return key;
    }

    static public string Get(int key, string tagName = null, bool isCsv = true)
    {
        return Get(key.ToString(), tagName, isCsv);
    }

    static public string FormatMoreStr(string tagName, bool isCsv, string key, params object[] parameters)
    {
        string _fmt = Get(key, tagName, isCsv);
        if (!string.IsNullOrEmpty(_fmt) && !_fmt.Equals(key))
        {
            // Debug.LogError(parameters[0].GetType());
            return string.Format(_fmt, parameters);
        }
        return key;
    }

    static public string Format(string key, object obj1, object obj2 = null, object obj3 = null, object obj4 = null, object obj5 = null, object obj6 = null)
    {
        return FormatMoreStr(null, true, key, obj1, obj2, obj3, obj4, obj5, obj6);
    }

    static public string Format(int key, object obj1, object obj2 = null, object obj3 = null, object obj4 = null, object obj5 = null, object obj6 = null)
    {
        return FormatMoreStr(null, true, key.ToString(), obj1, obj2, obj3, obj4, obj5, obj6);
    }

    static public string SetOrFormat(string tagName, bool isCsv, string key, object obj1 = null, object obj2 = null, object obj3 = null, object obj4 = null, object obj5 = null, object obj6 = null)
    {
        if (obj1 != null)
            return FormatMoreStr(tagName, isCsv, key, obj1, obj2, obj3, obj4, obj5, obj6);
        return Get(key, tagName, isCsv);
    }

    static public string SetOrFormat(string tagName, bool isCsv, int key, object obj1 = null, object obj2 = null, object obj3 = null, object obj4 = null, object obj5 = null, object obj6 = null)
    {
        if (obj1 != null)
            return FormatMoreStr(tagName, isCsv, key.ToString(), obj1, obj2, obj3, obj4, obj5, obj6);
        return Get(key, tagName, isCsv);
    }
}
