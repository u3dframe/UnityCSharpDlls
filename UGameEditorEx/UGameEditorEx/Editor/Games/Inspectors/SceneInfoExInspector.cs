using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using LitJson;
using Core;

/// <summary>
/// 类名 : 场景参数数据 的 自定义Inspector界面
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-03-21 10:37
/// 功能 : 雾效,烘培，prefab等
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(SceneInfoEx))]
public class SceneInfoExInspector : Editor
{
    SceneInfoEx m_obj;
	// string m_fabName = "";
	string _fpInAsset4Gbox = "Assets/_Develop/Builds/groudbox/Excludes/gbox.prefab";
	
    void OnEnable()
    {
        m_obj = target as SceneInfoEx;
    }
	
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
		if (GUILayout.Button("Save Infos - 保存【场景】信息",EG_Helper.ToOptionH(30))){
			_SaveInfos();
		}
    }

	JsonData NewJObj(){
		return LJsonHelper.NewJObj();
	}

	JsonData NewJArr(){
		return LJsonHelper.NewJArr();
	}

    void _SaveInfos(){
		UtilityHelper.Is_App_Quit = false;
        GameFile.CurrDirRes();
        Scene scene = EditorSceneManager.GetActiveScene();
		string sname = scene.name;
		m_obj.m_infoName = "sinfo_" + sname;
		string _fabName = "map_" + sname;

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

		string _fname = SceneInfoEx.ReFname(m_obj.m_infoName);
		string _vc = jdRoot.ToJson();        
		GameFile.WriteText(_fname,_vc);

		string fp = string.Format("{0}{1}{2}.prefab",GameFile.m_appAssetPath,"Scene/Builds/prefabs/maps/",_fabName);
		GameFile.CreateFolder(fp);

		GameObject gobjBox = UtilityHelper.ChildRecursion(m_obj.gameObject,"gbox");
		if(!gobjBox){
			GameObject _obj = AssetDatabase.LoadAssetAtPath(_fpInAsset4Gbox, typeof(GameObject)) as GameObject;
			if(_obj){
        		gobjBox = GameObject.Instantiate(_obj,m_obj.transform,false) as GameObject;
				gobjBox.name = "gbox";
			}
		}
		UtilityHelper.SetLayerAll(gobjBox,"Ground");
		
		PrefabElement csEle = UtilityHelper.Get<PrefabElement>(m_obj.gameObject,true);
		GameObject[] m_gobjs = new GameObject[2];
		m_gobjs[0] = UtilityHelper.ChildRecursion(m_obj.gameObject,"MainCamera");
		m_gobjs[1] = gobjBox;
		csEle.SetChildGobjs(m_gobjs);
		GameFile.CreateFab(m_obj.gameObject,fp,false);

		UnityEditor.AssetDatabase.Refresh();
		// m_obj.m_fabName = _fabName;
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

		var _arrs = m_obj.GetComponentsInChildren<Renderer>(true);
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
			_rname = GameFile.RightLast(_rrname,m_obj.m_rootRelative,false);
			
			jdRLm[_rname] = _jd;
		}

		var _arrs1 = m_obj.GetComponentsInChildren<Terrain>(true);
		Terrain _terrain;
		_nTemp = _arrs1.Length;
		for (int i = 0; i < _nTemp; i++) {
			_terrain = _arrs1[i];

			if( null == _terrain)
				continue;
			_gobj = _terrain.gameObject;
			if(!_gobj.isStatic || _terrain.lightmapIndex < 0 || nLmData <= _terrain.lightmapIndex)
				continue;
			
			JsonData _jd = NewJObj();
			_jd["lightmapIndex"] = _terrain.lightmapIndex;
			_vec4 = _terrain.lightmapScaleOffset;
			_jd["lmso_x"] = _vec4.x.ToString();
			_jd["lmso_y"] = _vec4.y.ToString();
			_jd["lmso_z"] = _vec4.z.ToString();
			_jd["lmso_w"] = _vec4.w.ToString();

			_rrname = UtilityHelper.RelativeName(_gobj);
			_rname = GameFile.RightLast(_rrname,m_obj.m_rootRelative,false);
			
			jdRLm[_rname] = _jd;
		}
		return jdRLm;
	}
}
