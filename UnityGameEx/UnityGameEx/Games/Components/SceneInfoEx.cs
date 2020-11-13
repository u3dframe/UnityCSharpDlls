using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using LitJson;
using Core;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// 类名 : 场景参数
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-03-21 10:37
/// 功能 : 场景的雾效，摄像机参数等
/// 修改 ：2020-10-10 10:02
/// </summary>
// [ExecuteInEditMode]
public class SceneInfoEx : MonoBehaviour
{
	static public SceneInfoEx Get(GameObject gobj,bool isAdd){
		return UtilityHelper.Get<SceneInfoEx>(gobj,isAdd);
	}

	static public SceneInfoEx Get(GameObject gobj){
		return Get(gobj,true);
	}

	static string ReFname(string fname){
		if(string.IsNullOrEmpty(fname)) return "";
		if(!fname.StartsWith("maps/")) fname = "maps/" + fname;
		if(!fname.EndsWith(".minfo")) fname += ".minfo";
		return fname;
	}

	static readonly List<LightmapData> listLightMaps = new List<LightmapData>();
	static readonly Dictionary<int,SceneLightMapData> dicLminfos = new Dictionary<int,SceneLightMapData>();

	public string m_rootRelative = "Scene/";
	public string m_infoName = "";
	[HideInInspector]
	public string m_lmABName = "";
	int m_lightmapsMode = 0;
	bool m_isDebug = false; // 是否打印
	bool m_isDebugRLmap = false; // 是否打印

	string StrRight(string src,string rev){
		int _ind = src.LastIndexOf(rev);
		int _nlen = rev.Length;
		if(_ind >= 0){
			return src.Substring( _ind + _nlen );
		}
		return src;
	}
	
#if UNITY_EDITOR
	string m_fabName = "";
	string _fpInAsset4Gbox = "Assets/_Develop/Builds/groudbox/Excludes/gbox.prefab";

	JsonData NewJObj(){
		return LJsonHelper.NewJObj();
	}

	JsonData NewJArr(){
		return LJsonHelper.NewJArr();
	}

	[ContextMenu("Save Infos")]
    void _SaveInfos(){
		Scene scene = EditorSceneManager.GetActiveScene();
		string sname = scene.name;
		this.m_infoName = "sinfo_" + sname;
		this.m_fabName = "map_" + sname;

		JsonData jdRoot = NewJObj();
		// fog
		JsonData jdFog = _SaveFog();
		jdRoot["info_fog"] = jdFog;

		// 烘培 lightmap
		JsonData jdLm = NewJObj();
		jdRoot["info_lms"] = jdLm;

		jdLm["fp_lm"] = "lightmaps/" + sname;
		jdLm["lightmapsMode"] = (int)LightmapSettings.lightmapsMode;

		JsonData jdLmds = NewJArr();
		LightmapData[] _lmDatas = LightmapSettings.lightmaps;
		int _nLens =  -1;
		int needLoad = 0;
		if (_lmDatas != null && _lmDatas.Length > 0) {
            _nLens = _lmDatas.Length;
			LightmapData _lmTemp;
            for (int i = 0; i < _nLens; i++)
            {
				_lmTemp = _lmDatas[i];
				JsonData _jd = NewJObj();
				if(_lmTemp.lightmapColor != null){
					_jd["lightmapColor"] = _lmTemp.lightmapColor.name;
					needLoad++;
				}

				if(_lmTemp.lightmapDir != null){
					_jd["lightmapDir"] =  _lmTemp.lightmapDir.name;
					needLoad++;
				}

				if(_lmTemp.shadowMask != null){
					_jd["shadowMask"] =  _lmTemp.shadowMask.name;
					needLoad++;
				}

				jdLmds.Add(_jd);
            }
        }

		if(jdLmds.Count > 0){
			jdLm["lmDatas"] = jdLmds;
		}
		jdLm["n_need_load"] = needLoad;

		JsonData jdRLmds = _SaveRenderLightMaps(_nLens);
		if(jdRLmds.Count > 0){
			jdLm["rlmDatas"] = jdRLmds;
		}

		string _fname = ReFname(this.m_infoName);
		string _vc = jdRoot.ToJson();
		GameFile.WriteText(_fname,_vc);

		string fp = string.Format("{0}{1}{2}.prefab",GameFile.m_appAssetPath,"Scene/Builds/prefabs/maps/",this.m_fabName);
		GameFile.CreateFolder(fp);

		GameObject gobjBox = UtilityHelper.ChildRecursion(gameObject,"gbox");
		if(!gobjBox){
			GameObject _obj = AssetDatabase.LoadAssetAtPath(_fpInAsset4Gbox, typeof(GameObject)) as GameObject;
			if(_obj){
        		gobjBox = GameObject.Instantiate(_obj,transform,false) as GameObject;
				gobjBox.name = "gbox";
			}
		}
		UtilityHelper.SetLayerAll(gobjBox,"Ground");
		
		PrefabElement csEle = UtilityHelper.Get<PrefabElement>(gameObject,true);
		GameObject[] m_gobjs = new GameObject[2];
		m_gobjs[0] = UtilityHelper.ChildRecursion(gameObject,"MainCamera");
		m_gobjs[1] = gobjBox;
		csEle.SetChildGobjs(m_gobjs);
		GameFile.CreateFab(gameObject,fp,false);

		UnityEditor.AssetDatabase.Refresh();
	}

	JsonData _SaveFog(){
		JsonData jdFog = NewJObj();
		jdFog["fog"] = RenderSettings.fog;
		jdFog["fogMode"] = (int)RenderSettings.fogMode;
		
		Color fColor = RenderSettings.fogColor;
		jdFog["fc_r"] = fColor.r.ToString();
		jdFog["fc_g"] = fColor.g.ToString();
		jdFog["fc_b"] = fColor.b.ToString();
		jdFog["fc_a"] = fColor.a.ToString();
		jdFog["fogDensity"] = RenderSettings.fogDensity.ToString();
		jdFog["fogStartDistance"] = RenderSettings.fogStartDistance.ToString();
		jdFog["fogEndDistance"] = RenderSettings.fogEndDistance.ToString();
		return jdFog;
	}

	JsonData _SaveRenderLightMaps(int nLmData){
		JsonData jdRLm = NewJObj();
		if(nLmData <= 0)
			return jdRLm;

		var _arrs = GetComponentsInChildren<Renderer>(true);
		int _nTemp = _arrs.Length;
		Renderer _render;
		GameObject _gobj;
		string _rrname,_rname;
		Vector4 _vec4;
		for (int i = 0; i < _nTemp; i++) {
			_render = _arrs[i];

			if( null == _render)
				continue;
			_gobj = _render.gameObject;
			if(!_gobj.isStatic || _render.lightmapIndex < 0 || nLmData <= _render.lightmapIndex)
				continue;
			
			JsonData _jd = NewJObj();
			_jd["lightmapIndex"] = _render.lightmapIndex;
			_jd["lightProbeUsage"] = (int)_render.lightProbeUsage;
			_vec4 = _render.lightmapScaleOffset;
			_jd["lmso_x"] = _vec4.x.ToString();
			_jd["lmso_y"] = _vec4.y.ToString();
			_jd["lmso_z"] = _vec4.z.ToString();
			_jd["lmso_w"] = _vec4.w.ToString();

			_rrname = UtilityHelper.RelativeName(_gobj);
			_rname = StrRight(_rrname,this.m_rootRelative);
			
			jdRLm[_rname] = _jd;
		}
		return jdRLm;
	}
#endif

	[ContextMenu("Load Infos")]
	public void LoadInfos ()
	{
		this._ReSetLightmap();

		string _fname = ReFname(this.m_infoName);
		string _vc = GameFile.GetText(_fname);
		if(string.IsNullOrEmpty(_vc))
			return;
		JsonData jdRoot = null;
		try{
			jdRoot = JsonMapper.ToObject<JsonData>(_vc);
		} catch {
		}

		if(jdRoot == null)
			return;
		
		// fog
		_LoadFog(LJsonHelper.ToJData(jdRoot,"info_fog"));

		// light map
		_LoadLightmap(LJsonHelper.ToJData(jdRoot,"info_lms"),_LoadRenderLightmap);
	}

	void _LoadFog(JsonData jdFog) {
		if(jdFog == null)
			return;

		RenderSettings.fog = (bool)jdFog["fog"];
		int mode = (int)jdFog["fogMode"];
		RenderSettings.fogMode = (FogMode) mode;
		
		float _fv = 0f;
		Color fColor = Color.white;
		if(!float.TryParse(jdFog["fc_r"].ToString(),out _fv))
			_fv = 0.0f;
		fColor.r = _fv;

		if(!float.TryParse(jdFog["fc_g"].ToString(),out _fv))
			_fv = 0.0f;
		fColor.g = _fv;

		if(!float.TryParse(jdFog["fc_b"].ToString(),out _fv))
			_fv = 0.0f;
		fColor.b = _fv;

		if(!float.TryParse(jdFog["fc_a"].ToString(),out _fv))
			_fv = 0.0f;
		fColor.a = _fv;
		RenderSettings.fogColor = fColor;
		
		if(!float.TryParse(jdFog["fogDensity"].ToString(),out _fv))
			_fv = 0.0f;
		RenderSettings.fogDensity = _fv;

		if(!float.TryParse(jdFog["fogStartDistance"].ToString(),out _fv))
			_fv = 0.0f;
		RenderSettings.fogStartDistance = _fv;

		if(!float.TryParse(jdFog["fogEndDistance"].ToString(),out _fv))
			_fv = 0.0f;
		RenderSettings.fogEndDistance = _fv;

		if(m_isDebug){
			Debug.LogErrorFormat("====== fog = [{0}] = [{1}] = [{2}] = [{3}] = [{4}] = [{5}]",
				jdFog.ToJson(),RenderSettings.fog,
				RenderSettings.fogColor,RenderSettings.fogDensity,
				RenderSettings.fogStartDistance,RenderSettings.fogEndDistance
			);
		}
	}

	void _LoadLightmap(JsonData jdLm,System.Action<JsonData> cfCall) {
		if(jdLm == null)
			return;

		string _fp = jdLm["fp_lm"].ToString();
		string _fp_ab = _fp + GameFile.m_strLightmap;
		this.m_lmABName = _fp_ab;
	
		// LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
		// Debug.Log(LightmapSettings.lightmapsMode);
		this.m_lightmapsMode = (int)jdLm["lightmapsMode"];
		if(m_isDebug){
			Debug.LogErrorFormat("====== lightmap = [{0}] = [{1}]",
				LightmapSettings.lightmapsMode,
				this.m_lightmapsMode
			);
		}
		JsonData jdLmds = LJsonHelper.ToJData(jdLm,"lmDatas");
		if(jdLmds == null || jdLmds.Count <= 0)
			return;
		
		JsonData jdRLmds = LJsonHelper.ToJData(jdLm,"rlmDatas");
		int n_need_load = (int)jdLm["n_need_load"];
		int _nLens = jdLmds.Count;
		int _n_loaded = 0;
		JsonData _jd;
		string _asset_;
		for (int i = 0; i < _nLens; i++)
		{
			dicLminfos.Add(i,new SceneLightMapData());

			_jd = jdLmds[i];
			_asset_ = LJsonHelper.ToStr(_jd,"lightmapColor");
			if(!string.IsNullOrEmpty(_asset_)){
				_asset_ = _asset_ + GameFile.m_suffix_light;
				ResourceManager.LoadTexture(_fp_ab,_asset_,(tex2d,ext1,ext2) =>{
					int ind = (int)ext1;
					SceneLightMapData _item = dicLminfos[ind];
					_item.lightmapColor = tex2d;

					if(m_isDebug)
						Debug.LogErrorFormat("====== cor ==[{0}] =[{1}] =[{2}] = [{3}]",tex2d,ext1,ext2,dicLminfos.Count);
					
					_n_loaded++;
					if(_n_loaded >= n_need_load){
						if(cfCall != null){
							cfCall(jdRLmds);
						}
					}
				},i,_asset_);
			}

			_asset_ = LJsonHelper.ToStr(_jd,"lightmapDir");
			if(!string.IsNullOrEmpty(_asset_)){
				ResourceManager.LoadTexture(_fp_ab,_asset_,(tex2d,ext1,ext2) =>{
					int ind = (int)ext1;
					SceneLightMapData _item = dicLminfos[ind];
					_item.lightmapDir = tex2d;

					if(m_isDebug)
						Debug.LogErrorFormat("====== dir ==[{0}] =[{1}] =[{2}] = [{3}]",tex2d,ext1,ext2,dicLminfos.Count);
					
					_n_loaded++;
					if(_n_loaded >= n_need_load){
						if(cfCall != null){
							cfCall(jdRLmds);
						}
					}
				},i,_asset_);
			}

			_asset_ = LJsonHelper.ToStr(_jd,"shadowMask");
			if(!string.IsNullOrEmpty(_asset_)){
				ResourceManager.LoadTexture(_fp_ab,_asset_,(tex2d,ext1,ext2) =>{
					int ind = (int)ext1;
					SceneLightMapData _item = dicLminfos[ind];
					_item.shadowMask = tex2d;

					if(m_isDebug)
						Debug.LogErrorFormat("====== sm ==[{0}] =[{1}] =[{2}] = [{3}]",tex2d,ext1,ext2,dicLminfos.Count);
					
					_n_loaded++;
					if(_n_loaded >= n_need_load){
						if(cfCall != null){
							cfCall(jdRLmds);
						}
					}
				},i,_asset_);
			}
		}
	}

	[ContextMenu("Clear Lightmap")]
	void _ReSetLightmap(){
		int _nLens = dicLminfos.Count;
		if(_nLens > 0){
			SceneLightMapData _item;
			for (int i = 0; i < _nLens; i++)
			{
				_item = dicLminfos[i];
				listLightMaps.Add(_item.ToLightmapData());
			}
		}
		LightmapSettings.lightmapsMode = (LightmapsMode)this.m_lightmapsMode;
		LightmapSettings.lightmaps = listLightMaps.ToArray();
		dicLminfos.Clear();
		listLightMaps.Clear();

		if(m_isDebug){
			Debug.LogErrorFormat("====== lightmap set = [{0}] = [{1}] = [{2}]",
				this.m_lightmapsMode,
				LightmapSettings.lightmapsMode,
				LightmapSettings.lightmaps.Length
			);
		}
	}

	void _LoadRenderLightmap(JsonData jdRLm) {
		this._ReSetLightmap();

		if(jdRLm == null)
			return;
		
		var _arrs = GetComponentsInChildren<Renderer>(true);
		int _nTemp = _arrs.Length;
		Renderer _render;
		GameObject _gobj;
		string _rrname,_rname;
		Vector4 _vec4 = Vector4.zero;
		JsonData _jd;
		int mode;
		float _fv = 0f;
		for (int i = 0; i < _nTemp; i++) {
			_render = _arrs[i];

			if( null == _render)
				continue;
			_gobj = _render.gameObject;
			
			_rrname = UtilityHelper.RelativeName(_gobj);
			_rname = StrRight(_rrname,this.m_rootRelative);
			_jd = LJsonHelper.ToJData(jdRLm,_rname);

			if(_jd == null)
				continue;
			_gobj.isStatic = true;

			_render.lightmapIndex = (int)_jd["lightmapIndex"];
			mode = (int)_jd["lightProbeUsage"];
			_render.lightProbeUsage = (LightProbeUsage) mode;

			if(!float.TryParse(_jd["lmso_x"].ToString(),out _fv))
				_fv = 0.0f;
			_vec4.x = _fv;

			if(!float.TryParse(_jd["lmso_y"].ToString(),out _fv))
				_fv = 0.0f;
			_vec4.y = _fv;
			
			if(!float.TryParse(_jd["lmso_z"].ToString(),out _fv))
				_fv = 0.0f;
			_vec4.z = _fv;

			if(!float.TryParse(_jd["lmso_w"].ToString(),out _fv))
				_fv = 0.0f;
			_vec4.w = _fv;
	
			_render.lightmapScaleOffset = _vec4;

			// _gobj.isStatic = true;
			if(m_isDebugRLmap){
				Debug.LogErrorFormat("====== r lightmap = [{0}] = [{1}] = [{2}]",
					_jd.ToJson(),_render.lightmapIndex,_render.lightmapScaleOffset
				);
			}
		}
	}

	void Awake() {
		this.LoadInfos();
	}
}