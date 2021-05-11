using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using LitJson;
using Core;
using Core.Kernel;

/// <summary>
/// 类名 : 场景参数
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-03-21 10:37
/// 功能 : 场景的雾效，摄像机参数等
/// 修改 ：2020-10-10 10:02
/// </summary>
// [AddComponentMenu("Scene/BasicEx")]
public class SceneBasicEx : GobjLifeListener
{
	static public string ReFname(string fname){
        return SceneMapEx.ReFname(fname);
	}

	public string m_rootRelative = "Scene/";
	public string m_infoName = "";
    protected SceneMapEx m_smInfo { get; private set; }
	
    protected override void OnCall4Start()
    {
        base.OnCall4Start();
        this.LoadInfos();
    }

    protected override void OnClear()
    {
        this.m_smInfo = null;
        base.OnClear();
    }

    string StrRight(string src,string rev){
		return UGameFile.RightLast(src,rev,false);
	}
	
	[ContextMenu("Load Infos")]
	public void LoadInfos ()
	{
        this.m_smInfo = SceneMapEx.GetMSM(this.m_infoName);
        if (this.m_smInfo == null)
            return;

        this.m_smInfo.ReLightmap();

        this.LoadLRP();

        JsonData jdRoot = this.m_smInfo.m_mapJdRoot;
        if (jdRoot == null)
            return;

        // fog
        JsonData _jd = LJsonHelper.ToJData(jdRoot, "info_fog");
        this._LoadFog(_jd);
        _jd = LJsonHelper.ToJData(jdRoot, "info_lms", "rlmDatas");
        this._LoadRenderLightmap(_jd);
    }

	void _LoadFog(JsonData jdFog) {
		if(jdFog == null)
			return;

        bool fog = (bool)jdFog["fog"];
		int mode = (int)jdFog["fogMode"];
		Color fColor = Color.white;
		fColor.r = UtilityHelper.Str2Float(jdFog["fc_r"].ToString());
		fColor.g = UtilityHelper.Str2Float(jdFog["fc_g"].ToString());
		fColor.b = UtilityHelper.Str2Float(jdFog["fc_b"].ToString());
		fColor.a = UtilityHelper.Str2Float(jdFog["fc_a"].ToString());
		float fogDensity = UtilityHelper.Str2Float(jdFog["fogDensity"].ToString());
        float fogStartDistance = UtilityHelper.Str2Float(jdFog["fogStartDistance"].ToString());
        float fogEndDistance = UtilityHelper.Str2Float(jdFog["fogEndDistance"].ToString());

        RenderSettingsEx.SetFog(fog, mode, fColor, fogDensity, fogStartDistance, fogEndDistance);
	}
    
	void _LoadRenderLightmap(JsonData jdRLm) {
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

			_vec4.x = UtilityHelper.Str2Float(_jd["lmso_x"].ToString());
			_vec4.y = UtilityHelper.Str2Float(_jd["lmso_y"].ToString());
			_vec4.z = UtilityHelper.Str2Float(_jd["lmso_z"].ToString());
			_vec4.w = UtilityHelper.Str2Float(_jd["lmso_w"].ToString());
			_render.lightmapScaleOffset = _vec4;
			// _gobj.isStatic = true;
		}

		var _arrs1 = GetComponentsInChildren<Terrain>(true);
		_nTemp = _arrs1.Length;
		Terrain _terrain; 
		for (int i = 0; i < _nTemp; i++) {
			_terrain = _arrs1[i];
			if( null == _terrain)
				continue;
			_gobj = _terrain.gameObject;
			
			_rrname = UtilityHelper.RelativeName(_gobj);
			_rname = StrRight(_rrname,this.m_rootRelative);
			_jd = LJsonHelper.ToJData(jdRLm,_rname);

			if(_jd == null)
				continue;
			_gobj.isStatic = true;

			_terrain.lightmapIndex = (int)_jd["lightmapIndex"];
			_vec4.x = UtilityHelper.Str2Float(_jd["lmso_x"].ToString());
			_vec4.y = UtilityHelper.Str2Float(_jd["lmso_y"].ToString());
			_vec4.z = UtilityHelper.Str2Float(_jd["lmso_z"].ToString());
			_vec4.w = UtilityHelper.Str2Float(_jd["lmso_w"].ToString());
			_terrain.lightmapScaleOffset = _vec4;
			// _gobj.isStatic = true;
		}
	}

    virtual protected void LoadLRP()
    {
        if (this.m_smInfo == null)
            return;

        Cubemap _obj = this.m_smInfo.m_lrp;
        if (_obj == null)
            return;

        ReflectionProbe _rp = this.m_trsf.GetComponentInChildren<ReflectionProbe>(true);
        if (_rp == null)
            return;

        _rp.mode = ReflectionProbeMode.Custom;
        _rp.customBakedTexture = _obj;
        // _rp.importance = 1;
        // _rp.intensity = 1;
        // _rp.center;
        // _rp.size;
        _rp.gameObject.SetActive(true);
    }
}