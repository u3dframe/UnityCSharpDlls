using UnityEngine;
using System;

/// <summary>
/// 类名 : GameObject对象 生命周期 监听
/// 作者 : Canyon
/// 日期 : 2017-03-21 10:37
/// 功能 : this.enabled 不能在自身的 回调事件里面设置(只能通过外包设置)
/// </summary>
[System.Serializable]
public class GobjLifeListener : Core.Kernel.Beans.EU_Basic
{
    static public bool IsNull(UnityEngine.Object uobj)
    {
        return GHelper.IsNull(uobj);
    }

    static protected GameObject GetGobj(string name)
    {
        return GHelper.GetGobjNotDestroy(name);
    }

    static public GobjLifeListener Get(GameObject gobj, bool isAdd)
    {
        return GHelper.Get<GobjLifeListener>(gobj, isAdd);
    }

    static public GobjLifeListener Get(GameObject gobj)
    {
        return Get(gobj, true);
    }

    // 自身对象
    Transform _m_trsf;

    /// <summary>
    /// 自身对象
    /// </summary>
    public Transform m_trsf
    {
        get
        {
            if (IsNull(_m_trsf))
            {
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
        get
        {
            if (IsNull(_m_gobj))
            {
                _m_gobj = gameObject;
            }
            return _m_gobj;
        }
    }

    public int m_gobjID
    {
        get
        {
            if (this.m_gobj)
                return this.m_gobj.GetInstanceID();
            return 0;
        }
    }

    [HideInInspector] public object m_obj = null; // 主obj
    [HideInInspector] public object m_obj1 = null; // 子obj
    [HideInInspector] public string csAlias = ""; // CSharp 别名
                                                  // 是否是存活的
    private bool _isAlive = false;
    public bool isAlive { get { return _isAlive; } }
    bool _isAppQuit = false;
    public bool isAppQuit { get { return this._isAppQuit || GHelper.Is_App_Quit; } }

    /// <summary>
    /// 继承对象可实现的函数 (比代理事件快)
    /// </summary>
    virtual protected void OnCall4Awake() { }
    virtual protected void OnCall4Start() { }
    virtual protected void OnCall4Show() { }
    virtual protected void OnCall4Hide() { }
    virtual protected void OnCall4Destroy() { }
    virtual protected void OnClear() { }


    protected Action m_callAwake = null;
    protected Action m_callStart = null;
    protected Action m_callShow = null; // 显示
    protected Action m_callHide = null; // 隐藏
    protected event Core.DF_OnNotifyDestry m_onDestroy = null; // 销毁

    void Awake()
    {
        this._isAlive = true;
        OnCall4Awake();
        if (m_callAwake != null) m_callAwake();
        if (string.IsNullOrEmpty(this.csAlias))
        {
            this.csAlias = this.m_gobj.name;
        }
    }

    void Start()
    {
        OnCall4Start();
        if (m_callStart != null) m_callStart();
    }

    void OnEnable()
    {
        if (this.isAppQuit) return;
        OnCall4Show();
        if (m_callShow != null) m_callShow();
    }

    void OnDisable()
    {
        if (this.isAppQuit) return;
        OnCall4Hide();
        if (m_callHide != null) m_callHide();
    }

    void OnDestroy()
    {
        if (!this.isAppQuit)
        {
            OnCall4Destroy();
            _ExcDestoryCall();
        }
        _OnClear();
    }

    protected void OnApplicationQuit()
    {
        GHelper.Is_App_Quit = true;
        this._isAppQuit = true;
        _OnClear();
    }

    private void _OnClear()
    {
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

    void _ExcDestoryCall()
    {
        var _call = this.m_onDestroy;
        this.m_onDestroy = null;
        if (_call != null)
            _call(this);
    }

    public void DetroySelf()
    {
        GameObject.Destroy(this);
    }

    public void OnlyOnceCallDetroy(Core.DF_OnNotifyDestry call, bool isAdd = true)
    {
        if (call == null)
            return;
        this.m_onDestroy -= call;
        if (isAdd)
            this.m_onDestroy += call;
    }

    public void OnlyOnceCallAwake(Action call, bool isAdd = true)
    {
        if (call == null)
            return;
        this.m_callAwake -= call;
        if (isAdd)
            this.m_callAwake += call;
    }

    public void OnlyOnceCallStart(Action call, bool isAdd = true)
    {
        if (call == null)
            return;
        this.m_callStart -= call;
        if (isAdd)
            this.m_callStart += call;
    }

    public void OnlyOnceCallShow(Action call, bool isAdd = true)
    {
        if (call == null)
            return;
        this.m_callShow -= call;
        if (isAdd)
            this.m_callShow += call;
    }

    public void OnlyOnceCallHide(Action call, bool isAdd = true)
    {
        if (call == null)
            return;
        this.m_callHide -= call;
        if (isAdd)
            this.m_callHide += call;
    }

    public void LookAtV3(Vector3 v3Target)
    {
        this.m_trsf.LookAt(v3Target);
    }

    public void LookAtTrsf(Transform trsf)
    {
        this.m_trsf.LookAt(trsf);
    }

    protected Vector3 ToVec3(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }

    public void LookAt(float x, float y, float z)
    {
        Vector3 _v3 = ToVec3(x, y, z);
        this.m_trsf.LookAt(_v3);
    }

    public void LookAtDirction(float x, float y, float z)
    {
        Vector3 _pos = this.m_trsf.position;
        Vector3 _v3 = ToVec3(x, y, z);
        LookAtV3(_pos + _v3);
    }

    public void SetPos(float x, float y, float z)
    {
        Vector3 _v3 = ToVec3(x, y, z);
        this.m_trsf.position = _v3;
    }

    public void SetPosByAdd(float x, float y, float z)
    {
        // Vector3 v3 = this.m_trsf.position;
        // Vector3 _v3 = ToV3(x,y,z);
        // this.m_trsf.position = _v3 + v3;
        this.m_trsf.Translate(x, y, z, Space.World);
    }
}
