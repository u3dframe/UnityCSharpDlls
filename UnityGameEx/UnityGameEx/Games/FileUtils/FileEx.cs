using UnityEngine;
using System.IO;
using System;

/// <summary>
/// 类名 : 文件工具
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-03-07 09:29
/// 功能 :
/// </summary>
namespace Core.Kernel
{
	public class FileEx : PathEx
	{
		//  是否存在 目录
		static public bool IsFolder (string fn)
		{
			return Directory.Exists (fn);
		}

		//  是否存在 文件
		static public bool IsFile (string fn)
		{
			return File.Exists (fn);
		}

		// 拷贝文件
		static public void Copy (string srcFile, string destFile)
		{
			File.Copy (srcFile, destFile, true);
		}

		// 删除文件
		static public void DelFile (string fn)
		{
			if (File.Exists (fn))
				File.Delete (fn);
		}

		static public string GetFolder (string fp)
		{
			string _fd = GetFolderPath (fp);
			return ReplaceSeparator(_fd);
		}

		// 删除文件夹
		static public void DelFolder (string fp)
		{
			string _fd = GetFolder (fp);
			if (Directory.Exists (_fd)) {
				Directory.Delete (_fd,true);
			}
		}

		// 创建文件夹
		static public bool CreateFolder (string fn)
		{
			string folder = GetFolder (fn);
			if (IsFolder (folder)) {
				return true;
			}

			DirectoryInfo dicInfo = Directory.CreateDirectory (folder);
			return dicInfo.Exists;
		}

		// 创建文件
		static public bool CreateText (string fn, byte[] buffs)
		{
			CreateFolder(fn);
			File.WriteAllBytes (fn, buffs);
			return true;
		}

		static public bool CreateText (string fn, string contents)
		{
			CreateFolder(fn);
			File.WriteAllText (fn, contents);
			return true;
		}

		// 取得文件字节
		static public byte[] GetBytes4File (string fp)
		{
			if (IsFile (fp)) {
				return File.ReadAllBytes (fp);
			}
			return null;
		}

		// 取得文件内容
		static public string GetText4File (string fp)
		{
			if (IsFile (fp)) {
				return File.ReadAllText (fp);
			}
			return "";
		}

		// 取得目录下面的文件夹
		static public string[] GetFns4Folders (string fn)
		{
			try {
				return Directory.GetDirectories (fn);
			} catch (Exception ex) {
				Debug.LogError (ex);
			}
			return null;
		}

		// 取得目录下面的文件(只取得当前文件夹下面的文件)
		static public string[] GgetFns4Files (string fn)
		{
			try {
				return Directory.GetFiles (fn);
			} catch (Exception ex) {
				Debug.LogError (ex);
			}
			return null;
		}

		// 取得文件如果是文件夹，则返回文件夹路径
		static public string ReFnPath (string fn, bool isFolder = true)
		{
			if (string.IsNullOrEmpty (fn)) {
				fn = "";
			}
			fn = ReplaceSeparator(fn);
			int lastIndex = fn.LastIndexOf ("/");
			int lens = fn.Length;
			bool isEndSeparator = lens == (lastIndex + 1);
			if (isFolder && !isEndSeparator && lens > 0) {
				fn += "/";
			}
			return fn;
		}

		/// <summary>
		/// 统一路径格式
		/// </summary>
		static public string ReplaceSeparator(string path){
			return path.Replace ('\\', '/');
		}
	}
}
