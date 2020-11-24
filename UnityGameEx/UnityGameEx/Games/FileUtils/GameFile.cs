using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core
{
	using Core.Kernel;

	/// <summary>
	/// 类名 : 游戏 路径
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2020-03-26 09:29
	/// 功能 : 
	/// </summary>
	public class GameFile : Kernel.Resources
	{
        static private readonly GameFile instance = new GameFile();
		static private bool m_init = false;
		static public string crcDataPath { get{	return CRCClass.GetCRCContent (m_dirDataNoAssets); } }
		static public string m_bk_url = null;

#if UNITY_EDITOR
		static public readonly string m_url_editor = "http://192.168.1.30:8006/dykj";
#endif
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
                return instance.IsLoadOrg4Editor();
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

        static void InitGameFile()
        {
			if(m_init)
				return;
			m_init = true;

#if UNITY_EDITOR && UNITY_ANDROID
            m_emFpType = Kernel.ET_FPType.UNITY_EDITOR_ANDROID;
#elif UNITY_EDITOR && UNITY_IOS
			m_emFpType = Kernel.ET_FPType.UNITY_EDITOR_IOS;
#elif UNITY_ANDROID
			m_emFpType = Kernel.ET_FPType.UNITY_ANDROID;
#elif UNITY_IOS
			m_emFpType = Kernel.ET_FPType.UNITY_IOS;
#else
            m_emFpType = Kernel.ET_FPType.UNITY_EDITOR;
#endif
            curInstance = instance;
			InitFdRoot(m_resFdRoot);
			GameEntranceEx.Entrance(_OnCFError);
			// EncodeWordFile = EM_EnCode.None;
#if UNITY_EDITOR
			CfgPackage.InitPackage(()=>{
				m_bk_url = CfgPackage.instance.m_urlVersion;
				Debug.LogError(CfgPackage.instance.ToJson());
			});
#endif
        }

		static void InitFdRoot(string fdRoot){
			m_resFdRoot = fdRoot;
			m_fpZipList = string.Concat (m_appContentPath,"ziplist.txt");
			m_fmtZip = string.Concat (m_appContentPath,"resource{0}.zip");
		}

		static public void InitFirst()
        {
            InitGameFile();
#if UNITY_EDITOR
			CfgVersion.instance.m_urlVersion = m_url_editor;
#endif
        }

		static void _OnCFError(string errMsg){
#if UNITY_EDITOR
			AppPause();
#endif
		}

        static public string CurrDirRes()
        {
            InitGameFile();
			if(!string.IsNullOrEmpty(m_bk_url))
				CfgVersion.instance.m_urlVersion = m_bk_url;
            return m_dirRes;
        }

        static public bool IsTextInCT(string fn){
			return fn.EndsWith(".csv") || fn.EndsWith(".minfo") || fn.IndexOf("protos/") != -1;
		}

		static public bool IsUpdateUIRes (string resName)
		{
			return resName.EndsWith ("updateui.ui") || resName.EndsWith ("uiupdate.atlas") || resName.EndsWith ("update_bg.tex");
		}
		
		// 取得路径
		override public string GetFilePath(string fn){
#if UNITY_EDITOR
			if(IsTextInCT(fn)){
				return string.Format("{0}CsvTxts/{1}",m_appAssetPath,fn);
			}
#else
			ResInfo _rinfo = CfgFileList.instance.GetInfo(fn);
			if(_rinfo != null){
				fn = _rinfo.m_resName;
			}
#endif
            return base.GetFilePath(fn);
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

        override public bool IsLoadOrg4Editor()
        {
#if UNITY_EDITOR
				return bLoadOrg4Editor;
#else
            return false;
#endif
        }

        override public string GetPath(string fn)
        {
#if !UNITY_EDITOR
			ResInfo _info = CfgFileList.instance.GetInfo(fn);
			if(_info == null)
				Debug.LogErrorFormat("=== resinfo null,nm = [{0}]",fn);
			else
				fn = _info.m_resName;
#endif
            return base.GetPath(fn);
        }

        override public string GetDecryptText(string fn)
        {
#if UNITY_EDITOR
			return GetText(fn);
#else
            return base.GetDecryptText(fn);
#endif
        }
    }
}