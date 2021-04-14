using UnityEngine;

/// <summary>
/// 类名 : 主摄像机管理脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2018-07-12 20:29
/// 功能 : 
/// </summary>
public class MgrMainCamera : MonoBehaviour
{
	// 取得对象
	static public MgrMainCamera Get(UnityEngine.Object uobj, bool isAdd){
		return UtilityHelper.Get<MgrMainCamera>(uobj, isAdd);
	}

	static public MgrMainCamera Get(UnityEngine.Object uobj)
    {
		return Get(uobj, true);
	}
	
    public Transform m_target;
	public Camera m_camera;
	public SmoothFollower m_follower { get;private set; }
    public Skybox m_skybox { get; private set; }

	[SerializeField] float m_offsetW = 300;
	[SerializeField] float m_offsetH = 300;
	protected virtual void Awake(){
		if(m_camera){
			m_follower = SmoothFollower.Get(m_camera.gameObject);
			m_follower.target = m_target;
			m_follower.isUpByLate = true;
			m_follower.isRunning = true;
            this.m_skybox = UtilityHelper.Get<Skybox>(this.m_camera,true);
        }
    }

	public void ReScreenRect(float ofW,float ofH){
		this.m_offsetW = ofW;
		this.m_offsetH = ofH;
	}
    
	public bool IsInCamera(Camera cmr,Object uobj)
    {
        return GRTHelper.IsInCameraByScreenPoint(cmr, uobj, this.m_offsetW, this.m_offsetH);
	}

	public bool IsInCurCamera(Object uobj)
    {
		return IsInCamera(this.m_camera, uobj);
	}

    public void SetSkybox(Material mat)
    {
        if (this.m_skybox == null)
            return;
        this.m_skybox.material = mat;
    }
}
