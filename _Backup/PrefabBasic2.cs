using System;
using UnityEngine;
using System.Collections.Generic;

// public delegate void DF_ElementForeach(int index,GameObject gobj);

/// <summary>
/// 类名 : Prefab 单元对象
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-08-04 00:10
/// 功能 : 缓存需要操作的对象
/// </summary>
[System.Serializable]
public class PrefabBasic2 : GobjLifeListener {
	static public new PrefabBasic2 Get(UnityEngine.Object uobj, bool isAdd){
		return GHelper.Get<PrefabBasic2>(uobj, true);
	}

	static public new PrefabBasic2 Get(UnityEngine.Object uobj)
    {
		return Get(uobj, true);
	}

    public enum GChildType
    {
        Trsf = 0,
        UIBtn,
        UIText,
        UIImage,
        UIRawIamge
    }

    [Serializable]
    public class GChild
    {
        public GChildType m_currType = GChildType.Trsf;
        public GameObject m_gobj = null;
        public string name { get { return m_gobj?.name; } }

        static public GChild Builder(GameObject gobj,GChildType gType)
        {
            if (!gobj)
                return null;
            GChild ret = new GChild();
            ret.m_currType = gType;
            ret.m_gobj = gobj;
            return ret;
        }

        static public GChild Builder(GameObject gobj)
        {
            return Builder(gobj, GChildType.Trsf);
        }
    }
	
	/// <summary>
	/// 操作的对象
	/// </summary>
	[SerializeField] protected GChild[] m_gobjs = new GChild[0];
    protected string[] _arrs_nodes = null;

    /// <summary>
    /// key = name or Relative name (相对应自身对象);
    /// val = gobj;
    /// </summary>
    protected Dictionary<string, GChild> m_dicName2Gobj = new Dictionary<string, GChild>();

    protected bool isInit = false;
    
    override protected void OnCall4Start(){
		this.Init();
	}
	
	void Init()
	{
		if(null == m_gobjs || m_gobjs.Length <= 0)
			return;
		
		if(isInit)
			return;
		isInit = true;

        GChild tmp = null;
		string _tmpName = "";
        bool _isEditor = Core.Kernel.UGameFile.m_isEditor;
        for (int i = 0; i < m_gobjs.Length;i++)
		{
			tmp = m_gobjs[i];
			if(tmp != null && tmp.m_gobj){
				_tmpName = tmp.name;
				if(!m_dicName2Gobj.ContainsKey(_tmpName)){
					m_dicName2Gobj.Add(_tmpName,tmp);
				}else if(_isEditor){
					Debug.LogError(string.Format("=== the same name in gameObject.name = [{0}]",_tmpName));
				}
				
				_tmpName = GetRelativeName(tmp.m_gobj);
				if(!m_dicName2Gobj.ContainsKey(_tmpName)){
					m_dicName2Gobj.Add(_tmpName,tmp);
				}
			}
		}
	}
	
	/// <summary>
	/// 取得自身对象下面的对象的相对路径name
	/// </summary>
	void GetRelativeName(Transform trsf,ref string refName)
	{
		if(trsf == m_trsf){
			if(string.IsNullOrEmpty(refName))
			{
				refName = "/";
			}
			return;
		}
		
		GHelper.RecursionName(trsf,ref refName);
	}
	
	/// <summary>
	/// 取得自身对象下面的对象的相对路径name
	/// </summary>
	string GetRelativeName(GameObject gobj)
	{
		string ret = "";
		GetRelativeName(gobj.transform,ref ret);
		return ret;
	}
	
	/// <summary>
	/// 取得自身对象下面的对象的相对路径name
	/// </summary>
	string GetRelativeName(Transform trsf)
	{
		string ret = "";
		GetRelativeName(trsf,ref ret);
		return ret;
	}
	
	/// <summary>
	/// 取得可操作的对象
	/// </summary>
	public GameObject GetGobjElement(string elName){
		if(string.IsNullOrEmpty(elName))
			return null;
		
		if("/".Equals(elName))
		{
			return m_gobj;
		}
		
		Init();

		if(m_dicName2Gobj.ContainsKey(elName)){
			return (m_dicName2Gobj[elName])?.m_gobj;
		}
		return null;
	}

    public Transform GetTrsfElement(string elName) {
        GameObject gobj = GetGobjElement(elName);
        return gobj?.transform;
    }
	
	/// <summary>
	/// 取得子对象的组件
	/// </summary>
	public T GetComponent4Element<T>(string elName) where T : Component
	{
		GameObject gobj = GetGobjElement(elName);
		if(IsNull(gobj)) return null;
		return gobj.GetComponent<T>();
	}
	
	/// <summary>
	/// 取得子对象的组件
	/// </summary>
	public Component GetComponent4Element(string elName,string comType)
	{
		GameObject gobj = GetGobjElement(elName);
		if(IsNull(gobj)) return null;
		return gobj.GetComponent(comType);
	}
	
	/// <summary>
	/// 取得子对象的组件
	/// </summary>
	public Component GetComponent4Element(string elName,Type comType)
	{
		GameObject gobj = GetGobjElement(elName);
		if(IsNull(gobj)) return null;
		return gobj.GetComponent(comType);
	}
	
	/// <summary>
	/// 设置元素显隐
	/// </summary>
	public void SetActive(string elName,bool isActive)
	{
		GameObject gobj = GetGobjElement(elName);
		if(gobj){
			gobj.SetActive(isActive);
		}
	}
	
	/// <summary>
	/// 是否包含元素
	/// </summary>
	public bool IsHasGobj(string elName)
	{
		GameObject gobj = GetGobjElement(elName);
		return !!gobj;
	}
	
	public void ForeachElement(DF_ElementForeach cfCall)
	{
        GameObject _gobj = null;
		for (int i = 0;i < m_gobjs.Length;++i)
		{
			_gobj = (m_gobjs [i])?.m_gobj;
			if(null == _gobj) continue;
			if(cfCall != null) cfCall(i,_gobj);
		}
	}

    public void SetChildGobjs(GameObject[] gobjs)
    {
        if(gobjs == null)
        {
            this.m_gobjs = null;
            return;
        }
        int lens = gobjs.Length;
        this.m_gobjs = new GChild[lens];
        for (int i = 0; i < lens; ++i)
            this.m_gobjs[i] = GChild.Builder(gobjs[i]);
    }

    [ContextMenu("Re Bind Nodes (重新绑定所需节点)")]
    public void ReNodes()
    {
        this.ReBindNodes(true);
    }

    virtual protected void ReBindNodes(bool isChild = false)
    {
        GHelper.Is_App_Quit = false;
        List<GChild> list = new List<GChild>();
        GChild _child = null;
        GameObject _gobj = null;
        int lens = m_gobjs.Length;
        for (int i = 0; i < lens; ++i)
        {
            _child = m_gobjs[i];
            if (null == _child || null == _child.m_gobj) continue;
            list.Add(_child);
        }

        bool _isHas = this._arrs_nodes != null && this._arrs_nodes.Length > 0;
        if (_isHas)
        {
            lens = _arrs_nodes.Length;
            for (int i = 0; i < lens; i++)
            {
                _gobj = GHelper.ChildRecursion(this.m_gobj, _arrs_nodes[i]);
                if (null == _gobj) continue;
                _child = GChild.Builder(_gobj);
                list.Add(_child);
            }
        }

        if(isChild)
        {
            lens = this.m_trsf.childCount;
            for (int i = 0; i < lens; i++)
            {
                _gobj = this.m_trsf.GetChild(i).gameObject;
                _child = GChild.Builder(_gobj);
                list.Add(_child);
            }
        }

        this.m_gobjs = list.ToArray();
        list.Clear();
    }

    [ContextMenu("Re-Rmv Empty")]
    protected void ReRmvEmpty()
    {
        GHelper.Is_App_Quit = false;
        List<GChild> list = new List<GChild>();
        GChild _gobj = null;
        for (int i = 0; i < m_gobjs.Length; ++i)
        {
            _gobj = m_gobjs[i];
            if (null == _gobj || null == _gobj.m_gobj) continue;
            list.Add(_gobj);
        }
        m_gobjs = list.ToArray();
        list.Clear();
    }

    [ContextMenu("Re-Rmv Empty(This and Childs)")]
    protected void ReRmvEmptyAll()
    {
        GHelper.Is_App_Quit = false;
        PrefabBasic2[] arrs = this.m_gobj.GetComponentsInChildren<PrefabBasic2>(true);
        foreach (var item in arrs)
        {
            item.ReRmvEmpty();
        }
    }

    [ContextMenu("Re-Bind Transform's First Childs")]
    protected void ReBindAllFirstChilds()
    {
        GHelper.Is_App_Quit = false;
        int lens = this.m_trsf.childCount;
        m_gobjs = new GChild[lens];
        GameObject _gobj = null;
        for (int i = 0; i < lens; i++)
        {
            _gobj = this.m_trsf.GetChild(i).gameObject;
            m_gobjs[i] = GChild.Builder(_gobj);
        }
    }

    [ContextMenu("Append Transform's First Childs")]
    protected void AppendAllFirstChilds()
    {
        GHelper.Is_App_Quit = false;
        GameObject __gobj = null;
        List<GameObject> list = new List<GameObject>();
        List<GChild> listChild = new List<GChild>();
        if(m_gobjs != null)
        {
            foreach (var item in m_gobjs)
            {
                if (item != null && item.m_gobj != null)
                {
                    list.Add(item.m_gobj);
                    listChild.Add(item);
                }
            }
        }

        int lens = this.m_trsf.childCount;
        for (int i = 0; i < lens; i++)
        {
            __gobj = this.m_trsf.GetChild(i).gameObject;
            if (!list.Contains(__gobj))
            {
                list.Add(__gobj);
                listChild.Add(GChild.Builder(__gobj));
            }
        }
        m_gobjs = listChild.ToArray();
    }

    /*
    [ContextMenu("PrintDicKeys")]
    void PrintDicKeys()
    {
        if (!isInit)
            return;

        foreach (var key in m_dicName2Gobj.Keys)
        {
            Debug.Log(key);
        }
    }
    */
}