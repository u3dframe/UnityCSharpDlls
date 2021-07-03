//#define USE_UPP
//#define USE_UPPCreate
using UnityEngine;
#if USE_UPP
using UnityEngine.Rendering.PostProcessing;
#endif

/// <summary>
/// 类名 : 主摄像机管理脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2018-07-12 20:29
/// 功能 : 
/// </summary>
public class MainCameraManager : CtrlCamera
{
	// 取得对象
	static public new MainCameraManager Get(UnityEngine.Object uobj, bool isAdd){
		return UtilityHelper.Get<MainCameraManager>(uobj, isAdd);
	}

	static public new MainCameraManager Get(UnityEngine.Object uobj)
    {
		return Get(uobj, true);
	}

#if USE_UPP
    private PostProcessLayer m_postLayer;
    private PostProcessVolume m_postVolume;
#endif

    override protected void Awake()
    {
        base.Awake();

        if (m_camera){
#if USE_UPP
            this.m_postLayer = UtilityHelper.Get<PostProcessLayer>(this.m_camera);
            this.m_postVolume = UtilityHelper.Get<PostProcessVolume>(this.m_camera);
#endif
        }
    }
    
#if USE_UPPCreate
    void DestroyPP()
    {
        PostProcessVolume _pVolume = m_postVolume;
        m_postVolume = null;
        if (_pVolume != null)
            Core.Kernel.UGameFile.UnLoadOne(_pVolume, true);

        PostProcessLayer _pLayer = m_postLayer;
        m_postLayer = null;
        if (_pLayer != null)
            Core.Kernel.UGameFile.UnLoadOne(_pLayer, true);
    }
    void CreatePP()
    {
        if (!this.m_camera)
            return;
        var _layer = this.m_camera.gameObject.layer;
        this.m_postLayer = UtilityHelper.Get<PostProcessLayer>(this.m_camera, true);
        this.m_postLayer.volumeLayer = _layer;
        this.m_postLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
        var _fastAntial = this.m_postLayer.fastApproximateAntialiasing;
        _fastAntial.fastMode = true;
        _fastAntial.keepAlpha = false;
        this.m_postLayer.stopNaNPropagation = true;
        this.m_postLayer.finalBlitToCameraTarget = false;

        this.m_postVolume = UtilityHelper.Get<PostProcessVolume>(this.m_camera, true);
        this.m_postVolume.isGlobal = true;
    }
#endif

    public void SetPPVolume(ScriptableObject sobj)
    {
#if USE_UPP
        if (this.m_postVolume == null)
            return;
        PostProcessProfile pppfile = null;
        if(sobj is PostProcessProfile)
            pppfile = sobj as PostProcessProfile;
        this.m_postVolume.profile = pppfile;
#endif
    }
}
