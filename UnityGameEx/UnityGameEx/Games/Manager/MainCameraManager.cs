using UnityEngine;
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
	static public new MainCameraManager Get(GameObject gobj,bool isAdd){
		return UtilityHelper.Get<MainCameraManager>(gobj,isAdd);
	}

	static public new MainCameraManager Get(GameObject gobj){
		return Get(gobj,true);
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
    }
}
