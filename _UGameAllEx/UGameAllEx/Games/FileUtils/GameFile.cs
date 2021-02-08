using UnityEngine;
using LitJson;
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

		static public bool IsInitLuaMgr{ get;set; }

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

        static void InitGameFile(System.Action callFunc = null)
        {
			if(m_init){
				if(callFunc != null)
					callFunc();
				return;
			}
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
			CloseEnCode();
			InitFdRoot(m_resFdRoot);
			if(Application.isPlaying){
				GameEntranceEx.Entrance(_OnCFError);
				string _url_push_log;
				_url_push_log =  "http://push.dianyue.com/";
				// _url_push_log =  "http://140.143.15.163:8085/";
				LogToNetHelper.shareInstance.Init(_url_push_log,"client_log");
			}

			CfgPackage.InitPackage(()=>{
				m_bk_url = CfgPackage.instance.m_urlVersion;
				if(callFunc != null)
					callFunc();
				EU_Bridge.SendAndCall("{\"cmd\":\"logLev\",\"logLev\":10}",null);
			});
        }

		static void InitFdRoot(string fdRoot){
			m_resFdRoot = fdRoot;
			m_fpZipList = string.Concat (m_appContentPath,"ziplist.txt");
			m_fmtZip = string.Concat (m_appContentPath,"resource{0}.zip");
		}

		static public void InitFirst(System.Action callFunc = null)
        {
			m_init = false;
            InitGameFile(callFunc);
#if UNITY_EDITOR
			CfgVersion.instance.m_urlVersion = m_url_editor;
#endif
        }

		static void _OnCFError(bool isException,string errMsg){
#if UNITY_EDITOR
			if(isException)
				AppPause();
#else
			LogToNetHelper.shareInstance.SendDefault(isException ? "Exception" : "Error",errMsg);
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
			return fn.EndsWith(".csv") || fn.EndsWith(".minfo") || fn.IndexOf("protos/") != -1 || fn.IndexOf("movies/") != -1;
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
		static public void VwFps(bool isShow)
		{
			GameMgr.Fps(isShow);
		}

		static private DelayExcute m_en = null;
		static void _CallDEx()
		{
			if(m_en != null)
				m_en.Init(0.08f,_CallDEx);
			m_en.Start();

			EU_Bridge.SendAndCall("{\"cmd\":\"phone_mem_cpu\"}",_CallPhone);
		}

		static void _CallPhone(string strData)
		{
			JsonData _json = LJsonHelper.ToJData(strData);
			if (_json == null)
				return;
			
			JsonData _jd = LJsonHelper.ToJDataByStrVal(_json,"data");
			if (_jd == null)
				return;
			MemDisplay _dis = GameMgr.GetMem(false);
			if(UtilityHelper.IsNull(_dis))
				return;
			
			if(IsInitLuaMgr)
				_dis.m_luaUseMem = LuaHelper.LuaMemroy() * 1024;
			
			_dis.m_outMemAll = LJsonHelper.ToLong( _jd,"am_total" );
			_dis.m_outMemFree = LJsonHelper.ToLong( _jd,"am_free" );
		}

		static public void VwMems(bool isShow)
		{
			GameMgr.Mem(isShow);

			if(m_en != null){
				m_en.Stop(true);
				m_en = null;
			}

			if(isShow)
			{
				if(m_en == null)
					m_en = new DelayExcute(0.08f,_CallDEx);
				_CallDEx();
			}
		}

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
			bool isBl = CfgVersion.IsCfgFile(fn) || CfgFileList.IsCfgFile(fn) || CfgMustFiles.IsCfgFile(fn);
			if(!isBl){
				ResInfo _info = CfgFileList.instance.GetInfo(fn);
				if(_info == null)
					Debug.LogErrorFormat("=== resinfo null,nm = [{0}]",fn);
				else
					fn = _info.m_resName;
			}
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

		override public Shader FindShader(string shaderName)
		{
			return ABShader.FindShader(shaderName);
		}
    }
}