using System.IO;
/// <summary>
/// 类名 : 路径工具
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-03-07 09:29
/// 功能 : 
/// </summary>
namespace Core.Kernel
{
	public class PathEx : StrEx
    {
		// 取得文件分隔符"/"
		static public readonly char folderSeparator = Path.DirectorySeparatorChar;
        
		// 取得路径分隔符";"
		static public readonly char pathSeparator = Path.PathSeparator;

		// 统一分割符号
		static public string ReDirSeparator(string fp){
			return fp.Replace ('\\', '/');
		}

		// 文件的目录路径
		static public string GetFolderPath (string fn)
		{
			return Path.GetDirectoryName (fn);
		}

		// 扩展名 Suffix(包含.号)
		static public string GetSuffix (string fn)
		{
			return Path.GetExtension (fn);
		}

		static public string GetSuffixToLower (string fn)
		{
            string _ret = GetSuffix(fn);
            return _ret.ToLower ();
		}

		// 扩展名 Suffix(无.号)
		static public string GetSuffixNoPoint (string fn)
		{
			string _ret = GetSuffix(fn);
			if(string.IsNullOrEmpty(_ret) || !_ret.StartsWith("."))
				return _ret;
			return _ret.Substring(1);
		}

		static public string GetSuffixNoPointToLower (string fn)
		{
            string _ret = GetSuffixNoPoint(fn);
            return _ret.ToLower();
		}

		// 文件名字(含有扩展名)
		static public string GetFileName (string fn)
		{
			return Path.GetFileName (fn);
		}

        // 文件名字(不含扩展名)
        static public string GetFileNameNoSuffix(string fn)
        {
            return Path.GetFileNameWithoutExtension(fn);
        }

		static public string GetPathNoSuffix(string fn)
        {
			int nInd = fn.LastIndexOf(".");
			if(nInd < 0) return fn;
            return fn.Substring(0,nInd);
        }

		// 文件全路径
		static public string GetFullPath (string fn)
		{
			return Path.GetFullPath (fn);
		}

        static public bool IsHasPoint(string src)
        {
            return src.LastIndexOf(".") >= 0;
        }

        static public bool IsHasSuffix(string src)
        {
            return IsHasPoint(src);
        }
    }
}
