using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using LitJson;
using Core;
using Core.Kernel.Beans;

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
	string _fpInAsset4Gbox = "Assets/_Develop/Builds/groudbox/gbox.prefab";
    string _fpInAsset4Gbox2 = "Assets/_Develop/Builds/groudbox/Excludes/gbox.prefab";
    bool _isClearCmr = false;

    void OnEnable()
    {
        m_obj = target as SceneInfoEx;
    }
	
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        this._isClearCmr = GUILayout.Toggle(this._isClearCmr, "Clear Curr Cmr Note - 清除【摄像机预制体】", EG_Helper.ToOptionH(30));

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

        string _fpdir = "lightmaps/" + sname;
        jdLm["fp_lm"] = _fpdir;
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
            this._SaveReflectionProbe(scene,_fpdir,jdLm);
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

        GameObject _go_ = m_obj.gameObject;
        GameObject gobjBox = UtilityHelper.ChildRecursion(_go_, "gbox");
		if(!gobjBox){
            string _fpInAsset = _fpInAsset4Gbox;
            string _fp = GameFile.m_dirDataNoAssets + _fpInAsset;
            if(!GameFile.IsExistsFile(_fp,true))
                _fpInAsset = _fpInAsset4Gbox2;

            GameObject _obj = AssetDatabase.LoadAssetAtPath(_fpInAsset, typeof(GameObject)) as GameObject;
			if(_obj){
        		gobjBox = GameObject.Instantiate(_obj,m_obj.transform,false) as GameObject;
				gobjBox.name = "gbox";
			}
		}
		UtilityHelper.SetLayerAll(gobjBox,"Ground");

        string[] _nodes = {
            "MainCamera","gbox","Lights","Scene","Effects","Probes",
            "victory_hide","victory_show","victory_camera","victory_location"
        };
        PrefabElement _pe = this._ReBindNode4PrefabElement(_go_, _nodes);
        var _qs_ = UtilityHelper.Get<Core.Kernel.EU_GQScene>(_go_, true);
        if (!_qs_.m_rootLight)
            _qs_.m_rootLight = _pe.GetTrsfElement("Lights");

        if(this._isClearCmr)
        {
            Transform _trsf_ = _go_.transform;
            // 清除 camera
            _trsf_ = _pe.GetTrsfElement("victory_hide");
            _ClearChild(_trsf_, 0);
            _trsf_ = _pe.GetTrsfElement("victory_show");
            _ClearChild(_trsf_, 0);

            _pe.ReNodes();
        }
        
        GameFile.CreateFab(_go_, fp,false);
        UnityEditor.AssetDatabase.Refresh();
		// m_obj.m_fabName = _fabName;
	}

    void _ClearChild(Transform trsfRoot,int index)
    {
        if (!trsfRoot)
            return;
        Transform _trsf_ = trsfRoot.GetChild(index);
        if (_trsf_)
            GameObject.DestroyImmediate(_trsf_.gameObject);
    }

    PrefabElement _ReBindNode4PrefabElement(GameObject _go_, string[] _nodes)
    {
        PrefabElement csEle = UtilityHelper.Get<PrefabElement>(_go_, true);
        var list2 = new System.Collections.Generic.HashSet<GameObject>();
        foreach (Transform item in _go_.transform)
        {
            list2.Add(item.gameObject);
        }

        GameObject _gobj = null;
        foreach (var item in _nodes)
        {
            _gobj = UtilityHelper.ChildRecursion(this.m_obj.gameObject, item);
            if (null == _gobj) continue;
            list2.Add(_gobj);
        }

        var list = new System.Collections.Generic.List<GameObject>();
        list.AddRange(list2);
        GameObject[] gobjs = list.ToArray();
        csEle.SetChildGobjs(gobjs);
        return csEle;
    }

    void _SaveReflectionProbe(Scene scene, string rfp, JsonData jdLm)
    {
        if (jdLm == null)
            return;

        JsonData _jdRoot = NewJObj();
        GameObject[] _rgobjs = scene.GetRootGameObjects();
        var _list = NewJArr();
        ReflectionProbe[] _arrs = null;
        Texture _brake = null;
        JsonData _jdTemp = null;
        Vector3 _v3Temp;
        int _maxL = 0;
        string _key = null;
        foreach (var _gobj_ in _rgobjs)
        {
            _arrs = _gobj_.GetComponentsInChildren<ReflectionProbe>(true);
            if (_arrs == null || _arrs.Length <= 0)
                continue;
            foreach (var item in _arrs)
            {
                _brake = item.bakedTexture;
                if (_brake != null)
                {
                    _jdTemp = NewJObj();
                    _key = _brake.name;

                    _jdTemp["importance"] = item.importance;
                    _jdTemp["intensity"] = item.intensity.ToString();
                    _v3Temp = item.center;
                    _jdTemp["center_x"] = _v3Temp.x.ToString();
                    _jdTemp["center_y"] = _v3Temp.y.ToString();
                    _jdTemp["center_z"] = _v3Temp.z.ToString();
                    _v3Temp = item.size;
                    _jdTemp["size_x"] = _v3Temp.x.ToString();
                    _jdTemp["size_y"] = _v3Temp.y.ToString();
                    _jdTemp["size_z"] = _v3Temp.z.ToString();
                    _jdTemp["rp_exr"] = _key;
                    _jdTemp["g_name"] = item.name;
                    _v3Temp = item.transform.position;
                    _jdTemp["pos_x"] = _v3Temp.x.ToString();
                    _jdTemp["pos_y"] = _v3Temp.y.ToString();
                    _jdTemp["pos_z"] = _v3Temp.z.ToString();

                    _jdRoot[_key] = _jdTemp;
                    _list.Add(_key);
                    _maxL++;
                }
            }
        }
        bool isHasRP_Environment = (RenderSettings.defaultReflectionMode == UnityEngine.Rendering.DefaultReflectionMode.Skybox && RenderSettings.skybox != null) || (RenderSettings.customReflection != null);
        if (isHasRP_Environment)
        {
            _jdTemp = NewJObj();
            _jdTemp["defaultReflectionResolution"] = RenderSettings.defaultReflectionResolution;
            _jdTemp["reflectionBounces"] = RenderSettings.reflectionBounces;
            _jdTemp["reflectionIntensity"] = RenderSettings.reflectionIntensity.ToString();
            if (RenderSettings.customReflection != null)
                _key = RenderSettings.customReflection.name;
            else
                _key = "ReflectionProbe-" + _maxL;
            _jdTemp["rp_exr"] = _key;

            _jdRoot[_key] = _jdTemp;
            _jdRoot["environment"] = _key;
            _list.Add(_key);
        }
        int _lens = _list.Count;
        if (_lens > 0)
        {
            _jdRoot["lens"] = _lens;
            _jdRoot["list"] = _list;
            jdLm["reflections"] = _jdRoot;
        }
    }

    /*
    void _SaveLightProbes(Scene scene, string rfp, JsonData jdLm)
    { 
        string _fn = string.Format("{0}_probes.asset",scene.name);
        string _fp = string.Format("{0}{1}{2}/{3}", GameFile.m_appAssetPath, "Scene/Builds/", rfp,_fn);
        if(GameFile.IsFile(_fp))
            GameFile.DelFile(_fp);

        LightProbes _lprobes = LightmapSettings.lightProbes;
        bool _isHasProbes = _lprobes != null;
#if UNITY_2018 || UNITY_2017
        _isHasProbes = _isHasProbes && (_lprobes.count != 0 || _lprobes.cellCount != 0);
#else
        _isHasProbes = _isHasProbes && (_lprobes.bakedProbes != null || _lprobes.bakedProbes.Length != 0);
#endif
        if (_isHasProbes)
        {
            string _fpAsset = GameFile.Path2AssetsStart(_fp);
            Object _assetObj = null;
#if UNITY_2018 || UNITY_2017
            _assetObj = Instantiate<LightProbes>(_lprobes);
#else
            var _ldat = ScriptableObject.CreateInstance(typeof(LProbeData)) as LProbeData;
            _ldat.lightProbes = _lprobes.bakedProbes;
            _assetObj = _ldat;
#endif
            AssetDatabase.CreateAsset(_assetObj, _fpAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            jdLm["lprobes"] = _fn;
        }
    }
    */

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

        jdFog["haloStrength"] = RenderSettings.haloStrength.ToString();
        jdFog["flareStrength"] = RenderSettings.flareStrength.ToString();
        fColor = RenderSettings.subtractiveShadowColor;
        jdFog["ssc_r"] = fColor.r.ToString();
        jdFog["ssc_g"] = fColor.g.ToString();
        jdFog["ssc_b"] = fColor.b.ToString();
        jdFog["ssc_a"] = fColor.a.ToString();
        jdFog["flareFadeSpeed"] = RenderSettings.flareFadeSpeed.ToString();
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
        JsonData _jd = null;
        for (int i = 0; i < _nTemp; i++) {
			_render = _arrs[i];

			if( null == _render)
				continue;
			_gobj = _render.gameObject;
			if(!_gobj.isStatic || _render.lightmapIndex < 0 || nLmData <= _render.lightmapIndex)
				continue;
			
			_jd = NewJObj();
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
			
			_jd = NewJObj();
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
