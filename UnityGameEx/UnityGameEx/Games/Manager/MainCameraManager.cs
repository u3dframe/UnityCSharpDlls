using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// 类名 : 主摄像机管理脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2018-07-12 20:29
/// 功能 : 
/// </summary>
public class MainCameraManager : MgrMainCamera
{
	// 取得对象
	static public new MainCameraManager Get(UnityEngine.Object uobj, bool isAdd){
		return UtilityHelper.Get<MainCameraManager>(uobj, isAdd);
	}

	static public new MainCameraManager Get(UnityEngine.Object uobj)
    {
		return Get(uobj, true);
	}
	
    public PostProcessLayer m_postLayer { get; private set; }
    public PostProcessVolume m_postVolume { get; private set; }

    override protected void Awake()
    {
        base.Awake();

        if (m_camera){
            this.m_postLayer = UtilityHelper.Get<PostProcessLayer>(this.m_camera);
            this.m_postVolume = UtilityHelper.Get<PostProcessVolume>(this.m_camera);
        }
        this.EnablePPLayer(false);
    }

    public void EnablePPLayer(bool isEnabled)
    {
        if (this.m_postLayer == null)
            return;
        this.m_postLayer.enabled = isEnabled;
    }

    public void SetPPVolume(PostProcessProfile pppfile)
    {
        if (this.m_postVolume == null)
            return;
        this.m_postVolume.profile = pppfile;
        this.EnablePPLayer(pppfile != null);
    }
}
