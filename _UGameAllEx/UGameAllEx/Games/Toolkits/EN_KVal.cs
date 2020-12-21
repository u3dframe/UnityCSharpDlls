using UnityEngine;
using System.Collections.Generic;
namespace Core.Kernel.Beans
{

    /// <summary>
    /// 类名 : Key - Value 字符串读取
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2019-06-27 16:37
    /// 功能 : 
    /// 修改 : 2020-12-20 08:55
    /// </summary>
    public class EN_KVal
    {
        public string m_fname { get; private set; }
        Dictionary<string, string> m_dicVals = new Dictionary<string, string>();

        public bool ReLoad(string fname, bool isCsv, string sBeg = "lanuage/")
        {
            this.m_fname = fname;
            string fpath = fname;
            if (isCsv)
                fpath = UGameFile.ReSBegEnd(fpath, sBeg, ".csv");
            else
                fpath = UGameFile.ReSBegEnd(fpath, sBeg, ".properties");

            string val = UGameFile.curInstance.GetDecryptText(fpath).Trim();
            if (string.IsNullOrEmpty(val))
                return false;
            string[] _rows = UGameFile.SplitRow(val);
            if (UGameFile.IsNullOrEmpty(_rows))
                return false;

            m_dicVals.Clear();
            int lens = _rows.Length;
            char[] spt = isCsv ? UGameFile.m_cSpComma : UGameFile.m_cSpEqual;
            string[] _cols;
            string _k, _v;
            for (int i = 0; i < lens; i++)
            {
                _cols = UGameFile.Split(_rows[i], spt, true);
                if (_cols == null || _cols.Length < 1)
                    continue;
                _k = _cols[0];
                if (_cols.Length > 1)
                    _v = _cols[1].Replace("\\n", "\n");
                else
                    _v = "";

                // 判断下
                if (m_dicVals.ContainsKey(_k))
                    Debug.LogErrorFormat("==== KVal has same key = [{0}],val = [{1}] ", _k, _v);
                else
                    m_dicVals.Add(_k, _v);
            }
            lens = m_dicVals.Count;
            return lens > 0;
        }

        public bool Exists(string key)
        {
            return m_dicVals.ContainsKey(key);
        }

        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            if (Exists(key))
                return m_dicVals[key];
            return key;
        }

        public string Get(int key)
        {
            return Get(key.ToString());
        }

        public string FormatMore(string key, params object[] parameters)
        {
            if (Exists(key))
            {
                string _fmt = Get(key);
                // Debug.LogError(parameters[0].GetType());
                return string.Format(_fmt, parameters);
            }
            return key;
        }

        public string Format(string key, object obj1, object obj2 = null, object obj3 = null, object obj4 = null, object obj5 = null, object obj6 = null)
        {
            return FormatMore(key, obj1, obj2, obj3, obj4, obj5, obj6);
        }

        public string FormatMore(int key, params object[] parameters)
        {
            return FormatMore(key.ToString(), parameters);
        }

        public string Format(int key, object obj1, object obj2 = null, object obj3 = null, object obj4 = null, object obj5 = null, object obj6 = null)
        {
            return FormatMore(key, obj1, obj2, obj3, obj4, obj5, obj6);
        }
    }
}
