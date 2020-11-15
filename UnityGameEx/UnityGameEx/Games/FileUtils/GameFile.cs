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
        static public readonly GameFile instance = new GameFile();

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

        static public void InitFpType()
        {
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
        }

        static public string CurrDirRes()
        {
            InitFpType();
            return m_dirRes;
        }

        static private bool IsTextInCT(string fn){
			return fn.EndsWith(".csv") || fn.EndsWith(".minfo") || fn.IndexOf("protos/") != -1;
		}
		
		// 取得路径
		override public string GetFilePath(string fn){
#if UNITY_EDITOR
			if(IsTextInCT(fn)){
				return string.Format("{0}CsvTxts/{1}",m_appAssetPath,fn);
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
    }
}