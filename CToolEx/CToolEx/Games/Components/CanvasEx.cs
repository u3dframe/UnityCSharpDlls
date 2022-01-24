using UnityEngine;
using Core.Kernel;
using System.Collections.Generic;

/// <summary>
/// 类名 : Canvas 扩展
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-01-16 17:13
/// 功能 : 
/// </summary>
public class CanvasEx : GobjLifeListener
{
    static public new CanvasEx Get(UnityEngine.Object uobj, bool isAdd)
    {
        return GHelper.Get<CanvasEx>(uobj, isAdd);
    }

    static public new CanvasEx Get(UnityEngine.Object uobj)
    {
        return Get(uobj, true);
    }

    static public CanvasEx GetInParent(UnityEngine.Object uobj)
    {
        return GHelper.GetInParent<CanvasEx>(uobj,false);
    }

    static Dictionary<int, CanvasEx> m_caches = new Dictionary<int, CanvasEx>();
    static public CanvasEx GetInCache(int gobjID)
    {
        CanvasEx _r = null;
        m_caches.TryGetValue(gobjID, out _r);
        return _r;
    }

    static public void AddCache(CanvasEx entity)
    {
        if (null == entity || !entity)
            return;
        int _id = entity.m_gobjID;
        if (!m_caches.ContainsKey(_id))
            m_caches.Add(_id, entity);
    }

    static public void RemoveCache(int gobjID)
    {
        m_caches.Remove(gobjID);
    }

    static public PrefabBasic m_uiroot { get;set; }

    public bool m_isInited { get; private set; }
    public CanvasEx m_cvsRoot { get; private set; }
    public Canvas m_cvs;// { get; private set; }
    public int m_orderBase { get; private set; }
    public int m_sortingLayerID { get; private set; }
    public string m_sortingLayerName { get; private set; }

    public int m_sortOrder { get; private set; }
    public int m_curSortOrder;// { get; private set; }

    override protected void OnCall4Destroy()
    {
        int _gid = this.m_gobjID;
        RemoveCache(_gid);
        string _key1 = string.Format(MsgConst.MSL_Cvs_Destroy, _gid);
        Messenger.Brocast(_key1);
    }

    public CanvasEx Init()
    {
        if (this.m_isInited)
            return this;
        this.m_isInited = true;

        AddCache(this);
        
        this.m_cvs = GHelper.Get<Canvas>(this.m_gobj);
        bool _isNew = null == this.m_cvs;
        if (_isNew)
        {
            this.m_cvs = GHelper.Add<Canvas>(this.m_gobj, false);
            this.m_cvs.overrideSorting = true;
            this.m_cvs.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
        }

        int sortingOrder = this.m_orderBase;
        if(sortingOrder == 0)
            sortingOrder = this.m_cvs.sortingOrder;
        int _sid = 0;
        string _sname = null;

        if (_isNew)
            sortingOrder = Mathf.Max(sortingOrder, 1);

        if (this.m_orderBase == 0)
            this.m_orderBase = sortingOrder;
        _sid = this.m_cvs.sortingLayerID;
        _sname = this.m_cvs.sortingLayerName;

        sortingOrder = this.m_orderBase * 1000;

        CanvasEx csParent = null;
        if (!"Default".Equals(_sname))
        {
            int _lid = SortingLayer.NameToID(_sname);
            if (m_uiroot != null && m_uiroot && SortingLayer.IsValid(_lid))
            {
                csParent = m_uiroot.GetComponent4Element<CanvasEx>(_sname);
            }
        }

        if(csParent == null)
            csParent = GetInParent(this.m_trsf);

        if (csParent != null && csParent != this)
        {
            sortingOrder += csParent.m_sortOrder;
            _sid = csParent.m_sortingLayerID;
            _sname = csParent.m_sortingLayerName;
        }
        this.m_cvsRoot = csParent;

        this.m_sortingLayerID = _sid;
        this.m_sortingLayerName = _sname;
        this.m_sortOrder = sortingOrder;
        this.m_curSortOrder = sortingOrder;

        this.m_cvs.sortingLayerID = _sid;
        this.m_cvs.sortingOrder = sortingOrder;

        // string _key1 = string.Format(MsgConst.MSL_Cvs_ValChange, this.m_gobjID);
        // Messenger.Brocast<int, int>(_key1, sortingOrder, sortingOrder);
        return this;
    }

    [ContextMenu("Re - Init")]
    void _ReInit()
    {
        this.ReInit();
    }

    public void ReInit(int valBase = -1)
    {
        if (this.m_isInited)
        {
            CanvasEx csParent = GetInParent(this.m_trsf);
            if(csParent != null && this.m_cvsRoot != csParent)
            {
                this.m_cvsRoot = csParent;
                this.m_sortingLayerID = csParent.m_sortingLayerID;
                this.m_sortingLayerName = csParent.m_sortingLayerName;
                this.m_cvs.sortingLayerID = this.m_sortingLayerID;
            }
        }
        else
        { 
            this.Init();
        }
        valBase = valBase < 0 ? this.m_orderBase : valBase;
        this.ReBaseOrder(valBase, true);
    }

    public void ReBaseOrder(int valBase,bool isForce = false)
    {
        this.Init();
        isForce = isForce || !(this.m_orderBase == valBase || valBase < 0 || valBase > 31);
        if (!isForce)
            return;
        valBase = Mathf.Max(valBase, 0);
        valBase = Mathf.Min(valBase, 31);
        int _sort = valBase * 1000;
        if (this.m_cvsRoot != null && this.m_cvsRoot != this)
            _sort += this.m_cvsRoot.m_sortOrder;

        this.m_orderBase = valBase;
        this.m_sortOrder = _sort;
        this.m_cvs.sortingOrder = _sort;

        int _diff = _sort - this.m_curSortOrder;
        bool _isAdd = _diff > 0;
        if(_diff != 0)
            _diff = Mathf.Abs(_diff);
        this.ChgCurr(_isAdd, _diff);
    }

    // 改变当前值
    public void ChgCurr(bool isAdd, int val)
    {
        this.Init();

        int _last = this.m_curSortOrder;
        int _symbol = isAdd ? 1 : -1;
        int _cur = _last + (val * _symbol);
        if (_cur <= this.m_sortOrder)
            _cur = this.m_sortOrder;

        int _diff = _cur - _last;
        this.m_curSortOrder = _cur;

        string _key1 = string.Format(MsgConst.MSL_Cvs_ValChange, this.m_gobjID);
        Messenger.Brocast<int, int>(_key1, _cur, _diff);
    }
}
