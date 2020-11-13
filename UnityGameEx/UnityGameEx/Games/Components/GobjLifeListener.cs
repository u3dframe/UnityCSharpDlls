using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// 类名 : GameObject对象 生命周期 监听
/// 作者 : Canyon
/// 日期 : 2017-03-21 10:37
/// 功能 : this.enabled 不能在自身的 回调事件里面设置(只能通过外包设置)
/// </summary>
[System.Serializable]
public class GobjLifeListener : MonoBehaviour,IUpdate {
	static public bool IsNull(UnityEngine.Object uobj)
	{
		return UtilityHelper.IsNull(uobj);
	}

	static protected GameObject GetGobj(string name)
	{
		return UtilityHelper.GetGobjNotDestroy(name);
	}

	static public GobjLifeListener Get(GameObject gobj,bool isAdd){
		return UtilityHelper.Get<GobjLifeListener>(gobj,isAdd);
	}

	static public GobjLifeListener Get(GameObject gobj){
		return Get(gobj,true);
	}

	// 接口函数
	[HideInInspector] public bool m_isOnUpdate = true;
	public bool IsOnUpdate(){ return this.m_isOnUpdate;} 
	virtual public void OnUpdate(float dt,float unscaledDt) {}	

	// 自身对象
	Transform _m_trsf;
	
	/// <summary>
	/// 自身对象
	/// </summary>
	public Transform m_trsf
	{
		get{
			if(IsNull(_m_trsf)){
				_m_trsf = transform;
			}
			return _m_trsf;
		}
	}
	
	GameObject _m_gobj;
	
	/// <summary>
	/// 自身对象
	/// </summary>
	public GameObject m_gobj
	{
		get{
			if(IsNull(_m_gobj)){
				_m_gobj = gameObject;
			}
			return _m_gobj;
		}
	}

	[HideInInspector] public object m_obj = null; // 主obj
	[HideInInspector] public object m_obj1 = null; // 子obj
	[HideInInspector] public string csAlias = ""; // CSharp 别名
	// 是否是存活的
	private bool _isAlive = false;
	public bool isAlive { get {return _isAlive;} }
	bool _isAppQuit = false;
	public bool isAppQuit { get{ return this._isAppQuit || UtilityHelper.Is_App_Quit; } }


	/// <summary>
	/// 继承对象可实现的函数 (比代理事件快)
	/// </summary>
	virtual protected void OnCall4Awake(){}
	virtual protected void OnCall4Start(){}
	virtual protected void OnCall4Show(){}
	virtual protected void OnCall4Hide(){}
	virtual protected void OnCall4Destroy(){}
	virtual protected void OnClear(){}

	
	public Action m_callAwake = null;
    public Action m_callStart = null;
    public Action m_callShow = null; // 显示
    public Action m_callHide = null; // 隐藏
    public event Core.DF_OnNotifyDestry m_onDestroy = null; // 销毁

    void Awake()
	{
		this._isAlive = true;
		OnCall4Awake();
		if(m_callAwake != null) m_callAwake ();
		if(string.IsNullOrEmpty(this.csAlias)){
			this.csAlias = this.m_gobj.name;
		}
	}

	void Start() {
		OnCall4Start ();
		if(m_callStart != null) m_callStart ();
	}

	void OnEnable()
	{
		if(this.isAppQuit) return;
		OnCall4Show ();
		if (m_callShow != null) m_callShow ();
	}

	void OnDisable()
	{
		if(this.isAppQuit) return;
		OnCall4Hide ();
		if (m_callHide != null) m_callHide ();
	}

	void OnDestroy(){
		if(!this.isAppQuit){
			OnCall4Destroy();
			_ExcDestoryCall();
		}
		_OnClear();
	}

	protected void OnApplicationQuit(){
		GHelper.Is_App_Quit = true;
		this._isAppQuit = true;
		_OnClear();
	}
	
	private void _OnClear(){
		this.m_isOnUpdate = false;
		this._isAlive = false;
		this.m_callAwake = null;
		this.m_callStart = null;
		this.m_callShow = null;
		this.m_callHide = null;
		this._m_gobj = null;
		this._m_trsf = null;

		OnClear();
	}

	void _ExcDestoryCall(){
		var _call = this.m_onDestroy;
		this.m_onDestroy = null;
		if (_call != null)
			_call (this);
	}

	public void DetroySelf(){
		GameObject.Destroy(this);
	}

	public void AddOnlyOnceDCall(Core.DF_OnNotifyDestry call){
		if(call == null)
			return;
		this.m_onDestroy -= call;
		this.m_onDestroy += call;
	}
}
