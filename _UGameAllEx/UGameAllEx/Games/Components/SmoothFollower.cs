using UnityEngine;

/// <summary>
/// 类名 : 平滑跟随者
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2015-05-12 09:29
/// 功能 : 
/// 修改 : 2020-07-12 20:35
/// </summary>
public class SmoothFollower : MonoBehaviour
{
	// 取得对象
	static public SmoothFollower Get(GameObject gobj,bool isAdd){
		SmoothFollower _r = gobj.GetComponent<SmoothFollower> ();
		if (isAdd && null == _r) {
			_r = gobj.AddComponent<SmoothFollower> ();
		}
		return _r;
	}

	static public SmoothFollower Get(GameObject gobj){
		return Get(gobj,true);
	}
	
	public bool isUpByLate = false;
	public bool isRunning = false;
    public Transform target;
	public float lookAtHeight = 0.0f;
	    
	public bool isLerpDistance = false;
	public float distance = 10.0f;
	public float distanceDamping = 1.8f;

	public bool isLerpHeight = false;
	public float height = 5.0f;
	public float heightDamping = 1.8f;
	
	public bool isLerpRotate = false;
	public float rotationDamping = 1.8f;
	
	Vector3 lookAtVector = Vector3.zero;
	float currentHeight = 0.0f;
	float wantedHeight = 0.0f;
	float currentDistance = 1.0f;
	Quaternion currentRotation;
	float wantedRotationAngle = 0.0f;
	float currentRotationAngle = 0.0f;
	float _dt = 0.0f;
	
	[Range(0.02f,3f)]
	public float offsetHeight4Call = 0.05f;
	
	[Range(0.02f,3f)]
	public float offsetWidth4Call = 0.05f;
	
	public System.Action callFinished = null;
	
	private Transform _trsf = null;
    private Vector3 _tPos = Vector3.zero;
    private Vector3 _fPos = Vector3.zero;

    void Update() {
		if(isUpByLate) return;
		_OnUpdate();
	}
	
	void LateUpdate() {
		if(!isUpByLate) return;
		_OnUpdate();
	}
	
	void _OnUpdate() {
		if (!isRunning || !target)
			return;
		if(_trsf == null) _trsf = transform;
		_dt = Time.deltaTime;
		currentHeight = _trsf.position.y;
		wantedHeight = target.position.y + height;
		if (isLerpHeight) {
			currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * _dt);
		} else {
			currentHeight = wantedHeight;
		}

		if (isLerpDistance) {
			currentDistance = Mathf.Lerp (currentDistance, distance, distanceDamping * _dt);
		} else {
			currentDistance = distance;
		}

		currentRotation = Quaternion.identity;
		if (isLerpRotate) {
			wantedRotationAngle = target.eulerAngles.y;
			currentRotationAngle = _trsf.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, rotationDamping * _dt);
			currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);
		}

        _tPos = target.position;
        _fPos = _tPos - currentRotation * Vector3.forward * currentDistance;
        _fPos.y = currentHeight;
        _trsf.position = _fPos;

		lookAtVector.x = 0;
		lookAtVector.z = 0;
		lookAtVector.y = lookAtHeight;
		lookAtVector += _tPos;
		_trsf.LookAt(lookAtVector);
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
		this.lookAtHeight = lookAtHeight;
		this.distance = distance;
		this.height = height;
		this.isLerpDistance = isLerpDistance;
		this.isLerpHeight = isLerpHeight;
		this.isLerpRotate = isLerpRotate;
	}

	public void ReSetDHL(float distance,float height,float lookAtHeight){
		this.distance = distance;
		this.height = height;
		this.lookAtHeight = lookAtHeight;
	}

	public void SetTarget(Transform target){
		this.target = target;
		if(!!target){
			this._OnUpdate();
		}
	}
}  