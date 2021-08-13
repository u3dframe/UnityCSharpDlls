using UnityEngine;

/// <summary>
/// 类名 : 控制摄像机跟随
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2018-07-12 20:29
/// 功能 : 
/// </summary>
public class CtrlCamera : MonoBehaviour
{
	// 取得对象
	static public CtrlCamera Get(UnityEngine.Object uobj, bool isAdd){
        CtrlCamera _ret = UtilityHelper.Get<CtrlCamera>(uobj, isAdd);
        _ret?.Init();
        return _ret;
    }

	static public CtrlCamera Get(UnityEngine.Object uobj)
    {
		return Get(uobj, true);
	}
    static int _tget = 0;
    [SerializeField] Transform m_fwer;
    public Transform m_target;
    public Camera m_camera;
	public SmoothFollower m_follower { get;private set; }
    public AudioListener m_adoLner;
  
	[SerializeField] float m_offsetW = 300;
	[SerializeField] float m_offsetH = 300;
    public Skybox m_skybox { get; private set; }
    bool m_isInited = false;
	protected virtual void Awake(){
        this.Init();
    }

    protected void Init()
    {
        if (this.m_isInited)
            return;
        this.m_isInited = true;
        GameObject _gobj = this.gameObject;
        if (!m_adoLner)
            m_adoLner = _gobj.GetComponentInChildren<AudioListener>(true);

        if (!m_camera)
            this.m_camera = UtilityHelper.Get<Camera>(_gobj, false);
        if (!m_camera)
            return;

        if (!m_fwer)
            m_fwer = m_camera.transform;

        if (!m_target)
        {
            _gobj = new GameObject("__Target_" + (_tget++));
            this.m_target = _gobj.transform;
        }
        
        this.m_skybox = UtilityHelper.Get<Skybox>(this.m_camera, true);
        this.InitFollower(false);
    }

    protected void InitFollower(bool isRunning)
    {
        if (!m_fwer)
            return;
        this.m_follower = SmoothFollower.Get(this.m_fwer, true);
        this.m_follower.target = this.m_target;
        this.m_follower.isUpByLate = true;
        this.m_follower.isRunning = isRunning;
    }

	public void ReScreenRect(float ofW,float ofH){
		this.m_offsetW = ofW;
		this.m_offsetH = ofH;
	}
    
	public bool IsInCamera(Object uobj, Camera cmr = null)
    {
        if (cmr == null)
            cmr = this.m_camera;
        return GRTHelper.IsInCameraByScreenPoint(cmr, uobj, this.m_offsetW, this.m_offsetH);
	}

    public void SetSkybox(Material mat)
    {
        if (this.m_skybox == null)
            return;
        this.m_skybox.material = mat;
    }

    public void EnableAdoListener(bool isEnable)
    {
        if (this.m_adoLner)
            this.m_adoLner.enabled = isEnable;
    }
}
