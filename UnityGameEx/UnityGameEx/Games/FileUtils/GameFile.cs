using UnityEngine;
using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core
{
	/// <summary>
	/// 类名 : 游戏 路径
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2020-03-26 09:29
	/// 功能 : 
	/// </summary>
	public class GameFile : Kernel.Resources
	{
		// zip 压缩文件列表(将文件分包体大小来压缩,减小解压时所需内存)
		static public readonly string m_fpZipList = string.Concat (m_appContentPath,"ziplist.txt");
		static public readonly string m_fmtZip = string.Concat (m_appContentPath,"resource{0}.zip");
		
		static public string crcDataPath {
			get{
				return CRCClass.GetCRCContent (m_dirDataNoAssets);
			}
		}

		// 编辑模式
		static public bool isEditor{
			get{
				#if UNITY_EDITOR
				return true;
				#else
				return false;
				#endif
			}
		}

		// 编辑模式 -  加载原始资源 Original
		static public bool bLoadOrg4Editor = true;
		static public bool isLoadOrg4Editor{
			get{
#if UNITY_EDITOR
				return bLoadOrg4Editor;
#else
				return false;
#endif
			}
		}

		static public void AppQuit(){
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else
			Application.Quit ();
			#endif
		}

		static public void AppPause(){
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPaused = true;
			#endif
		}
		
		static private bool IsTextInCT(string fn){
			return fn.EndsWith(".csv") || fn.EndsWith(".minfo") || fn.IndexOf("protos/") != -1;
		}
		
		// 取得路径
		static public string GetFilePath(string fn){
#if UNITY_EDITOR
			if(IsTextInCT(fn)){
				return string.Format("{0}CsvTxts/{1}",m_appAssetPath,fn);
			}
#endif
			return string.Concat (m_dirRes, fn);
		}

		static public string GetStreamingFilePath(string fn){
			return string.Concat (m_appContentPath, fn);
		}

		static public string GetPath(string fn){
			string _fp = GetFilePath (fn);
			if (File.Exists (_fp)) {
				return _fp;
			}

			return GetStreamingFilePath (fn);
		}

		static public void DeleteFile(string fn,bool isFilePath){
			string _fp = isFilePath ? fn : GetFilePath (fn);
			DelFile(_fp);
		}

		static public void DeleteFile(string fn){
			DeleteFile (fn, false);
		}

		// 取得文本内容
		static public string GetText(string fn){
			string _fp = GetPath (fn);
			if (File.Exists (_fp)) {
				return File.ReadAllText (_fp);
			}

			string _suffix = Path.GetExtension (fn);
			int _ind_ = fn.LastIndexOf(_suffix);
			string _fnNoSuffix = fn.Substring(0, _ind_);
			TextAsset txtAsset = Resources.Load<TextAsset> (_fnNoSuffix); // 可以不用考虑释放txtAsset
			string _ret = "";
			if (txtAsset){
				_ret = txtAsset.text;
				Resources.UnloadAsset(txtAsset);
			}
			return _ret;
		}

		static public void WriteText(string fn,string content,bool isFilePath){
			string _fp = isFilePath ? fn : GetFilePath (fn);
			CreateText(_fp,content);
		}

		static public void WriteText(string fn,string content){
			WriteText(fn,content,false);
		}

		// 文件是否存在可读写文件里
		static public bool IsExistsFile(string fn,bool isFilePath){
			string _fp = isFilePath ? fn : GetFilePath (fn);
			return File.Exists (_fp);
		}

		// 取得文件流
		static public byte[] GetFileBytes(string fn){
			string _fp = GetPath (fn);
			if (File.Exists (_fp)) {
				return File.ReadAllBytes (_fp);
			}

			string _suffix = Path.GetExtension (fn);
			int _ind_ = fn.LastIndexOf(_suffix);
			string _fnNoSuffix = fn.Substring(0, _ind_);
			TextAsset txtAsset = Resources.Load<TextAsset> (_fnNoSuffix); // 可以不用考虑释放txtAsset
			byte[] _bts = null;
			if (txtAsset){
				_bts = txtAsset.bytes;
				UnLoadOne(txtAsset);
			}
			return _bts;
		}
        
		/// <summary>
        /// manifest的路径
        /// </summary>
        static public string m_fpABManifest{
            get{
                return GetPath(m_curPlatform);
            }
        }

#if UNITY_EDITOR
		static public void CreateFab(GameObject obj, string assetPath,bool isOnlyOne)
		{
			assetPath = Path2AssetsStart(assetPath);
			if(isOnlyOne)
				assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
			PrefabUtility.SaveAsPrefabAsset(obj,assetPath);
		}
#endif

	}
}