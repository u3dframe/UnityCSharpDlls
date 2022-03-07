namespace Core.Kernel
{
    /// <summary>
    /// 类名 : 字符串操作工具
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2016-12-23 15:20
    /// 功能 : 
    /// 修订 : 2020-12-18 17:05
    /// </summary>
    public class StrEx : ObjEx
    {
        static public readonly char[] m_cSpRow = "\r\n\t".ToCharArray();
        static public readonly char[] m_cSpComma = ",".ToCharArray();
        static public readonly char[] m_cSpEqual = "=".ToCharArray();
        static public readonly char[] m_cSpEmicolon = ";".ToCharArray();

        static public string ReUrlEnd(string url)
        {
            return UGameFile.ReFnPath(url,true);
        }

        static public string ReUrlTime(string url)
        {
            return string.Concat(url, "?time=", System.DateTime.Now.Ticks);
        }

        static public string ReUrlTime(string url, string fn)
        {
            url = ReUrlEnd(url);
            return string.Concat(url, fn, "?time=", System.DateTime.Now.Ticks);
        }

        static public string ReUrlTime(string url, string proj, string fn)
        {
            if (!string.IsNullOrEmpty(proj))
            {
                url = ReUrlEnd(url);
                url = string.Concat(url, proj);
            }
            return ReUrlTime(url, fn);
        }

        static System.Random sysRnd = new System.Random(System.Guid.NewGuid().GetHashCode());
        static public string GetUrl(string[] arrs, string defUrl, ref int index)
        {
            string _ret = defUrl;
            if (arrs != null && arrs.Length > 0)
            {
                int _lens = arrs.Length;

                if (index < 0)
                {
                    if (_lens > 1)
                    {
                        index = sysRnd.Next(_lens);
                    }
                    else
                    {
                        index = 0;
                    }
                }
                index %= _lens;
                _ret = arrs[index];
                index++;
            }
            return _ret;
        }

        /// <summary>
		/// 字符串分割
		/// </summary>
        static public string[] Split(string val, char[] spt, bool isRmEmpty)
        {
            if (string.IsNullOrEmpty(val) || spt == null || spt.Length <= 0)
                return null;
            System.StringSplitOptions _sp = System.StringSplitOptions.None;
            if (isRmEmpty) _sp = System.StringSplitOptions.RemoveEmptyEntries;
            return val.Split(spt, _sp);
        }

        /// <summary>
		/// 行分割 \r\n\t
		/// </summary>
        static public string[] SplitRow(string val)
        {
            return Split(val, m_cSpRow, true);
        }

        /// <summary>
		/// 常用分割 - 英文 逗号 ,
		/// </summary>
        static public string[] SplitComma(string val)
        {
            return Split(val, m_cSpComma, false);
        }

        /// <summary>
		/// 常用分割 - 英文 分号 ;
		/// </summary>
        static public string[] SplitDivision(string val,bool isRmEmpty)
        {
            return Split(val, m_cSpEmicolon,isRmEmpty);
        }

        /// <summary>
		/// 重写拼接字符串
		/// </summary>
        static public string ReSBegEnd(string src, string beg,string end = null)
        {
            if (string.IsNullOrEmpty(src))
                return "";

            if (!string.IsNullOrEmpty(beg) && !src.StartsWith(beg))
                src = string.Concat(beg, src);

            if (!string.IsNullOrEmpty(end) && !src.EndsWith(end))
                src = string.Concat(src, end);

            return src;
        }

        static public string ReSEnd(string src, string end)
        {
            return ReSBegEnd(src,null,end);
        }

    }
}
