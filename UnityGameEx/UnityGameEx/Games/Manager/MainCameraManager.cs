using UnityEngine;
using System.Collections;

/// <summary>
/// 类名 : 主摄像机管理脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-07-12 20:29
/// 功能 : 
/// </summary>
public class MainCameraManager : MonoBehaviour
{
	// 取得对象
	static public MainCameraManager Get(GameObject gobj,bool isAdd){
		return UtilityHelper.Get<MainCameraManager>(gobj,isAdd);
	}

	static public MainCameraManager Get(GameObject gobj){
		return Get(gobj,true);
	}
	
    public Transform m_target;
	public Camera m_camera;
	public SmoothFollower m_follower { get;private set; }
	
	float device_width = 0f;
	float device_height = 0f;
	[SerializeField] Rect screenRect = new Rect(0,0,0,0);
	[SerializeField] float m_offsetW = 300;
	[SerializeField] float m_offsetH = 300;
	void Awake(){
		this.device_width = Screen.width;
		this.device_height = Screen.height;
		this.ReSetSRectWH();
		if(m_camera){
			m_follower = SmoothFollower.Get(m_camera.gameObject);
			m_follower.target = m_target;
			m_follower.isUpByLate = true;
			m_follower.isRunning = true;
		}
	}

	public void ReScreenRect(float ofW,float ofH){
		this.m_offsetW = ofW;
		this.m_offsetH = ofH;
		this.ReSetSRectWH();
	}

	[ContextMenu("ReSet-ScreenRectWH")]
	void ReSetSRectWH(){
		this.screenRect.x = -0.5f * this.m_offsetW;
		this.screenRect.y = -0.5f * this.m_offsetH;
		this.screenRect.width = this.device_width + this.m_offsetW;
		this.screenRect.height = this.device_height + this.m_offsetH;
	}

	public bool IsInCamera(Camera cmr,Transform trsf){
		if(null == trsf)
			return false;
		if(cmr == null){
			cmr = this.m_camera;
		}
		if(cmr == null || !cmr.gameObject.activeInHierarchy){
			return false;
		}		
		Vector3 v3 = cmr.WorldToScreenPoint(trsf.position);
		bool isIn = this.screenRect.Contains(v3);
		return isIn;
	}

	public bool IsInCamera(Camera cmr,GameObject gobj){
		return IsInCamera(cmr,gobj?.transform);
	}

	public bool IsInCurCamera(Transform trsf){
		return IsInCamera(null,trsf);
	}

	public bool IsInCurCamera(GameObject gobj){
		return IsInCurCamera(gobj?.transform);
	}
}
