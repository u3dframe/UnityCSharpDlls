using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public delegate void DF_InpKeyState(string key, int state);
public delegate void DF_InpScale(bool isBig, float val);
public delegate void DF_InpVec2(Vector2 val);
public delegate void DF_InpRayHit(Transform hitTrsf);

/// <summary>
/// 类名 : Input 管理脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2018-07-13 21:29
/// 功能 : 
/// </summary>
public class InputBaseMgr : GobjLifeListener {
    /*
	static InputBaseMgr _instance;
	static public InputBaseMgr instance{
		get{
			if (IsNull(_instance)) {
				GameObject _gobj = GameMgr.mgrGobj;
				_instance = UtilityHelper.Get<InputBaseMgr>(_gobj,true);
			}
			return _instance;
		}
	}

#if UNITY_EDITOR
	static private System.Type _tpKeyCode = typeof(KeyCode);
	static private Dictionary<KeyCode, DF_InpKeyState> m_diCalls;
	static private void OnUpdate()
	{
		if (m_diCalls == null || m_diCalls.Count <= 0)
			return;
		var e = m_diCalls.GetEnumerator();
		while (e.MoveNext())
		{
			var current = e.Current;
			if (Input.GetKeyDown(current.Key))
			{
				// 按键按下的第一帧返回true
				current.Value(current.Key.ToString(), 1);
			}
			else if (Input.GetKeyUp(current.Key))
			{
				// 按键松开的第一帧返回true
				current.Value(current.Key.ToString(), 2);
			}
			else if (Input.GetKey(current.Key))
			{
				// 按键按下期间返回true
				current.Value(current.Key.ToString(), 3);
			}
		}
	}

	static private void RegKeyCode(string key, DF_InpKeyState callBack,bool isAppend)
	{
		if (m_diCalls == null) m_diCalls = new Dictionary<KeyCode, DF_InpKeyState>();

		KeyCode _code = EnumEx.Str2Enum<KeyCode>(_tpKeyCode,key);
		DF_InpKeyState _val;
		if (m_diCalls.ContainsKey(_code)) {
			if(isAppend){
				_val = m_diCalls[_code] + callBack;
				m_diCalls[_code] = _val;
			}
		}else{
			m_diCalls.Add(_code, callBack);
		}
	}
#endif

	static public void RegKeyCode(string key, DF_InpKeyState callBack){
#if UNITY_EDITOR
		RegKeyCode(key,callBack,false);
#endif
	}
    */

    private EventSystem _currEvt;
	// 单击到了UI上面
	public bool IsClickInUI{
		get{
			_currEvt = EventSystem.current;
			if(_currEvt == null) return false;
			if(Input.touchCount > 0) return _currEvt.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
			return _currEvt.IsPointerOverGameObject();
		}
	}
	
    public bool m_isRunning = true;
	public int m_nCanRay = 0;
	public float m_minScaleDis = 5;
	public float m_minSlideDis = 5;
	public DF_InpScale m_lfScale = null; // 缩放
	public DF_InpVec2 m_lfRotate = null; // 旋转
	public DF_InpVec2 m_lfSlide = null; // 滑动
	public DF_InpRayHit m_lfRayHit = null; // 单击到物体

    [SerializeField] float _noOpsTime = 0;
    public int m_fpsFrameRate { get; set; }
    protected bool m_isOpt { get; set; }
    [Range(0.1f,0.5f)] public float m_noOpsFpsRate = 0.2f;
    public float m_noOpsLmtSec = 5 * 60;

	private LayerMask _lay_mask = 1 << 0 | 1 << 1 | 1 << 4;
	
	float maxDistance = 0;
	bool isSingleFinger = false;
	int count = 0;
	Touch _t1,_t2;
	bool _isSlide = false;
	bool _isClick = false;
	Vector2 _v2T1,_v2T2;
	float _f1,_f2,_f3;

	Queue<RayScreenPointInfo> m_queue_pool = new Queue<RayScreenPointInfo>();
	Queue<RayScreenPointInfo> m_queue_sp_1 = new Queue<RayScreenPointInfo>();
	Queue<RayScreenPointInfo> m_queue_sp_2 = new Queue<RayScreenPointInfo>();
	
	override protected void OnCall4Awake(){
		this.maxDistance = Screen.height > Screen.width ? Screen.height : Screen.width;
		this.csAlias = "InpMgr";
	}

	override protected void OnClear() {
#if UNITY_EDITOR
		if(m_diCalls != null) m_diCalls.Clear();
#endif
		m_lfScale = null;
		m_lfRotate = null;
		m_lfSlide = null;
		m_lfRayHit = null;

		_ClearQueue(m_queue_sp_1);
		_ClearQueue(m_queue_sp_2);
		_ClearQueue(m_queue_pool);
	}
	
    virtual protected void Update () {
        _OnUpdating();
        _OnUpdatingOpt();
    }

    void _OnUpdatingOpt()
    {
        if (m_fpsFrameRate <= 0)
            m_fpsFrameRate = Application.targetFrameRate;

        if(m_isOpt)
        {
            _noOpsTime = 0f;
            Application.targetFrameRate = m_fpsFrameRate;
            return;
        }

        _noOpsTime += Time.deltaTime;
        if(_noOpsTime >= this.m_noOpsLmtSec)
        {
            _noOpsTime -= this.m_noOpsLmtSec;
            int _fps = Mathf.CeilToInt(m_fpsFrameRate * m_noOpsFpsRate);
            _fps = Mathf.Max(_fps, 3);
            Application.targetFrameRate = _fps;
        }
    }

    void _OnUpdating()
    {
        m_isOpt = false;
        if (!m_isRunning)
            return;
#if UNITY_EDITOR
		OnUpdate();
#endif
        m_isOpt = IsClickInUI;
        if (!m_isOpt)
            return;

        if (Input.touchSupported)
        {
            _OnUpdateTouch();
        }
        else
        {
            _OnUpdateMouse();
        }
    }

    void FixedUpdate()
    {
		_ExcRaycastScreenPoint(m_queue_sp_1,5);
		_ExcRaycastScreenPoint(m_queue_sp_2,2);
	}
	
	public void Init(){}

	public InputBaseMgr InitAll(int layerMask,DF_InpScale cfScale,DF_InpVec2 cfRotate,DF_InpVec2 cfSlide,DF_InpRayHit cfRayHit){
		this.SetLayerMask(layerMask);
		this.m_lfScale = cfScale;
		this.m_lfRotate = cfRotate;
		this.m_lfSlide = cfSlide;
		this.m_lfRayHit = cfRayHit;
		return this;
	}

	public InputBaseMgr InitCall(DF_InpScale cfScale,DF_InpVec2 cfRotate,DF_InpVec2 cfSlide,DF_InpRayHit cfRayHit){
		return InitAll(this._lay_mask,cfScale,cfRotate,cfSlide,cfRayHit);
	}

    protected void _ClearQueue(Queue<RayScreenPointInfo> queue){
		if(queue == null || queue.Count <= 0) return;
		RayScreenPointInfo _rif;
		while(queue.Count > 0){
			_rif = queue.Dequeue();
			_rif.Clear();
		}
	}

	void _ExcRaycastScreenPoint(Queue<RayScreenPointInfo> queue,int nCount){
		if(queue == null || queue.Count <= 0) return;
		bool m_isBreak = false;
		while(queue.Count > 0){
			_ExcCast(queue.Dequeue());
			if(nCount != -1 && nCount != 0)
				nCount--;

			m_isBreak = nCount < -1 || nCount == 0;
			if(m_isBreak)
				break;
		}
	}

	void _ExcCast(RayScreenPointInfo rayInfo){
		rayInfo.DoCast();
		rayInfo.Clear();
		m_queue_pool.Enqueue(rayInfo);
	}

	RayScreenPointInfo borrow(){
		if(m_queue_pool.Count > 0)
			return m_queue_pool.Dequeue();
		
		return new RayScreenPointInfo();
	}

	public RayScreenPointInfo ReRayScreenPointInfo(float x,float y,float rayDisctance,LayerMask layerMask,DF_InpRayHit cfCall){
		RayScreenPointInfo rayInfo = borrow();
		rayInfo.m_pos.x = x;
		rayInfo.m_pos.y = y;
		if(rayDisctance > 0)
			rayInfo.m_rayDisctance = rayDisctance;
		rayInfo.m_layMask = layerMask;
		rayInfo.m_call = cfCall;
		return rayInfo;
	}

	void AddQueue(Queue<RayScreenPointInfo> queue,RayScreenPointInfo rayInfo){
		if(queue == null)
			queue = m_queue_sp_1;
		queue.Enqueue(rayInfo);
	}

	void SetLayerMask(LayerMask lmask){
		this._lay_mask = lmask;
	}

	public void SetLayerMask(int layerMask){
		LayerMask lmask = layerMask;
		this._lay_mask = lmask;
	}

	LayerMask GetLayerMask(params string[] layerNames){
		return LayerMask.GetMask(layerNames);
	}

	public void SetLayerMaskMore(params string[] layerNames){
		LayerMask _lm = GetLayerMask(layerNames);
		SetLayerMask(_lm);
	}

	public void SetLayerMaskBy(string nmLayer){
		SetLayerMaskMore(nmLayer);
	}

	void _ExcLFScroll(bool isBig,float val){
		if(m_lfScale != null){
			m_lfScale(isBig,val);
		}
	}
	
	void _ExcLFRotate(Vector2 val){
		if(m_lfRotate != null){
			m_lfRotate(val);
		}
	}
	
	void _ExcLFSlide(Vector2 val){
		if(m_lfSlide != null){
			m_lfSlide(val);
		}
	}

	void _JugdeClick(Vector2 newPos){
		if((!isSingleFinger) || (!_isClick && !_isSlide))
			return;
						
		_v2T2 = newPos - _v2T1;
		_isSlide = (_v2T2.sqrMagnitude >= (m_minSlideDis * m_minSlideDis));
		_isClick = !_isSlide;
		
		if(_isSlide) {
			_isSlide = false;
			// left = 0, right = 1, up = 2, down = 3;
			// if (Mathf.Abs(_v2T2.y) <= Mathf.Abs(_v2T2.x)) {
			// 	_userInput = _v2T2.x < 0 ? 0 : 1;
			// } else {
			// 	_userInput = _v2T2.y > 0 ? 2 : 3;
			// }
			_ExcLFSlide(_v2T2);
		} else if (_isClick) {
			_isClick = false;
			if(m_nCanRay > 0){
				m_nCanRay--;
				return;
			}

			if(m_queue_sp_1.Count > 0)
				return;

			var _info = ReRayScreenPointInfo(newPos.x,newPos.y,0,this._lay_mask,m_lfRayHit);
			AddQueue(m_queue_sp_1,_info);
		}
	}

    protected void _OnUpdateTouch(){
		count = Input.touchCount;
        m_isOpt = count > 0;
        if (count <= 0) return;
		_t1 = Input.GetTouch(0);
		switch(count){
			case 1:
				isSingleFinger = true;
				switch (_t1.phase)
				{
					case TouchPhase.Began:
						_isClick = true;
						_isSlide = true;
						_v2T1 = _t1.position;
						break;
					case TouchPhase.Canceled:
						_isClick = false;
						_isSlide = false;
						break;
					case TouchPhase.Moved:
						_isClick = false;
						_isSlide = true;
						break;
					case TouchPhase.Ended:
						_JugdeClick(_t1.position);
						break;
				}
			break;
			case 2:
				_isClick = false;
				_isSlide = false;
				_t2 = Input.GetTouch(0);
				if ((isSingleFinger) || (_t1.phase == TouchPhase.Began || _t2.phase == TouchPhase.Began)){
					_v2T1 = _t1.position;
					_v2T2 = _t2.position;
					isSingleFinger = false;
				}
				if (_t1.phase == TouchPhase.Ended || _t2.phase == TouchPhase.Ended){
					_v2T1 = _v2T2 - _v2T1;
					_v2T2 = _t2.position - _t1.position;
					_f1 = _v2T1.sqrMagnitude;
					_f2 = _v2T2.sqrMagnitude;
					_f3 = (_f2 - _f1);
					_v2T1 = _v2T2 - _v2T1;
					_f2 = m_minScaleDis * m_minScaleDis;
					if(_f3 >= _f2){
						// 变大了
						_f1 = (_v2T1.magnitude / this.maxDistance);
						_ExcLFScroll(true,_f1);
					}else if(_f3 < -1 * _f2){
						// 缩小了
						_f1 = (_v2T1.magnitude / this.maxDistance);
						_ExcLFScroll(false,_f1);
					}else{
						// 旋转角度差值 _v2T1
						_ExcLFRotate(_v2T1);
					}
					_v2T1 = _t1.position;
					_v2T2 = _t2.position;
				}
			break;
		}
	}

    protected void _OnUpdateMouse(){
        if (Input.GetMouseButtonDown(0)){
			_isClick = true;
			_isSlide = true;
            m_isOpt = true;
            isSingleFinger = true;
			_v2T1 = Input.mousePosition;
		}

		if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)){
			isSingleFinger = false;
			_isClick = false;
			_isSlide = false;
		}
		
		if(_isSlide && Input.GetMouseButton(0)){
			_isClick = false;
		}
		
		if(Input.GetMouseButtonUp(0)){
            m_isOpt = true;
            _JugdeClick(Input.mousePosition);
		}
	}

	public void SendRaycast4ScreenPointBy(RayScreenPointInfo rayInfo,bool isImmediate){
		if(isImmediate){
			_ExcCast(rayInfo);
		}else{
			AddQueue(m_queue_sp_2,rayInfo);
		}
	}

	public void SendRaycast4ScreenPoint(float x,float y,float distance,int layerMask,DF_InpRayHit cfCall,bool isImmediate){
		LayerMask masks = layerMask;
		RayScreenPointInfo rayInfo = ReRayScreenPointInfo(x,y,distance,masks,cfCall);
		SendRaycast4ScreenPointBy(rayInfo,isImmediate);
	}
}