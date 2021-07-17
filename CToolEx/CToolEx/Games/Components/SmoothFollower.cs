using UnityEngine;

/// <summary>
/// 类名 : 平滑跟随者
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2015-05-12 09:29
/// 功能 : 
/// 修改 : 2020-07-12 20:35
/// </summary>
public class SmoothFollower : SmoothLookAt
{
	// 取得对象
	static public new SmoothFollower Get(Object uobj,bool isAdd){
        return GHelper.Get<SmoothFollower>(uobj, isAdd);
	}

	static public new SmoothFollower Get(Object uobj)
    {
		return Get(uobj, true);
	}

    public bool isRunningFollow = false;
    public bool isBackDistance = true;
    public bool isLerpDistance = false;
    public float distance = 10.0f;
	public float distanceDamping = 1.8f;

	public bool isLerpHeight = false;
	public float height = 5.0f;
	public float heightDamping = 1.8f;

    public bool isSyncRotate = false;
    public bool isLerpRotate = false;
	public float rotationDamping = 1.8f;

    public bool isSmoothPos = false;
    Vector3 m_curPosVelocity = Vector3.zero;
    [Range(0.02f,5f)] public float m_posSmoothTime = 0.1f;

	float currentHeight = 0.0f;
	float wantedHeight = 0.0f;
    int _symbolDistance = 1;
    float currentDistance = 1.0f;
    Quaternion zeroRotation = Quaternion.identity;
    Quaternion currentRotation;
	float wantedRotationAngle = 0.0f;
	float currentRotationAngle = 0.0f;
	float _dt = 0.0f;
	
	[Range(0.02f,3f)] public float offsetHeight4Call = 0.05f;
	[Range(0.02f,3f)] public float offsetWidth4Call = 0.05f;
	public System.Action callFinished = null;
    private Vector3 _tPos = Vector3.zero;
    private Vector3 _fPos = Vector3.zero;

    override protected void _OnUpdate()
    {
        this._OnUpFollower();
        base._OnUpdate();
    }

    void _OnUpFollower()
    {
        if (!isRunningFollow || !target)
			return;

		_dt = Time.deltaTime;
		currentHeight = m_trsf.position.y;
		wantedHeight = target.position.y + height;
		if (isLerpHeight)
			currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * _dt);
		else
			currentHeight = wantedHeight;

		if (isLerpDistance)
			currentDistance = Mathf.Lerp (currentDistance, distance, distanceDamping * _dt);
		else 
			currentDistance = distance;
        
        _tPos = target.position;
        currentRotation = zeroRotation;
        if (isSyncRotate)
        {
            wantedRotationAngle = target.eulerAngles.y;
            if (isLerpRotate)
                currentRotationAngle = Mathf.LerpAngle(m_trsf.eulerAngles.y, wantedRotationAngle, rotationDamping * _dt);
            else
                currentRotationAngle = wantedRotationAngle;
            currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);
        }
        _symbolDistance = isBackDistance ? -1 : 1;
        _fPos = _tPos + currentRotation * Vector3.forward * currentDistance * _symbolDistance;
        _fPos.y = currentHeight;
        if(isSmoothPos)
            m_trsf.position = Vector3.SmoothDamp(m_trsf.position, _fPos, ref m_curPosVelocity, m_posSmoothTime);
        else
            m_trsf.position = _fPos;

		_CallEnd ();
	}

	// 执行回调
	void _CallEnd() {
		if (callFinished == null)
			return;
		
		if (currentHeight >= (wantedHeight - offsetHeight4Call) && currentDistance >= (distance - offsetWidth4Call)) {
			System.Action _call = callFinished;
			callFinished = null;
			_call ();
		}
	}

	public void DoStart(Transform target,float distance,float height,float lookAtHeight,bool isLerpDistance,bool isLerpHeight,bool isLerpRotate){
		this.ReSetPars( target,distance,height,lookAtHeight,isLerpDistance,isLerpHeight,isLerpRotate );
		this.isRunning = true;
    }

	public void ReSetPars(Transform target,float distance,float height,float lookAtHeight,bool isLerpDistance,bool isLerpHeight,bool isLerpRotate){
		this.target = target;
		this.m_lookAtHeight = lookAtHeight;
		this.distance = distance;
		this.height = height;
		this.isLerpDistance = isLerpDistance;
		this.isLerpHeight = isLerpHeight;
		this.isLerpRotate = isLerpRotate;
	}

	public void ReSetDHL(float distance,float height,float lookAtHeight){
		this.distance = distance;
		this.height = height;
        this.m_lookAtHeight = lookAtHeight;
	}

	public void SetTarget(Transform target){
		this.target = target;
		if(!!target){
			this._OnUpdate();
		}
	}

    bool _isRunning = false;
    public bool isRunning
    {
        get { return _isRunning; }
        set
        {
            this._isRunning = value;
            this.isRunningFollow = value;
            this.isRunningAt = value;
        }
    }
}  