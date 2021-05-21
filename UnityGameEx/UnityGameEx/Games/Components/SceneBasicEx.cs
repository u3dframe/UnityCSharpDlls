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
		fColor.r = LJsonHelper.ToFloat(jdFog,"fc_r");
		fColor.g = LJsonHelper.ToFloat(jdFog,"fc_g");
		fColor.b = LJsonHelper.ToFloat(jdFog,"fc_b");
		fColor.a = LJsonHelper.ToFloat(jdFog,"fc_a");
		float fogDensity = LJsonHelper.ToFloat(jdFog,"fogDensity");
        float fogStartDistance = LJsonHelper.ToFloat(jdFog,"fogStartDistance");
        float fogEndDistance = LJsonHelper.ToFloat(jdFog,"fogEndDistance");

        RenderSettingsEx.SetFog(fog, mode, fColor, fogDensity, fogStartDistance, fogEndDistance);


        if(LJsonHelper.IsHas(jdFog, "haloStrength"))
        {
            RenderSettings.haloStrength = LJsonHelper.ToFloat(jdFog, "haloStrength");
            RenderSettings.flareStrength = LJsonHelper.ToFloat(jdFog, "flareStrength");
            RenderSettings.flareFadeSpeed = LJsonHelper.ToFloat(jdFog, "flareFadeSpeed");
            fColor.r = LJsonHelper.ToFloat(jdFog, "ssc_r");
            fColor.g = LJsonHelper.ToFloat(jdFog, "ssc_g");
            fColor.b = LJsonHelper.ToFloat(jdFog, "ssc_b");
            fColor.a = LJsonHelper.ToFloat(jdFog, "ssc_a");
            RenderSettings.subtractiveShadowColor = fColor;
        }

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

			_vec4.x = LJsonHelper.ToFloat(_jd,"lmso_x");
			_vec4.y = LJsonHelper.ToFloat(_jd,"lmso_y");
			_vec4.z = LJsonHelper.ToFloat(_jd,"lmso_z");
			_vec4.w = LJsonHelper.ToFloat(_jd,"lmso_w");
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
			_vec4.x = LJsonHelper.ToFloat(_jd,"lmso_x");
			_vec4.y = LJsonHelper.ToFloat(_jd,"lmso_y");
			_vec4.z = LJsonHelper.ToFloat(_jd,"lmso_z");
			_vec4.w = LJsonHelper.ToFloat(_jd,"lmso_w");
			_terrain.lightmapScaleOffset = _vec4;
			// _gobj.isStatic = true;
		}
	}

    virtual protected void LoadLRP()
    {
        if (this.m_smInfo == null)
            return;
        this.m_smInfo.ReReflEnv();
        this.m_smInfo.ReReflProbe(this.m_trsf);
        this.m_smInfo.ReProbes();
    }
}