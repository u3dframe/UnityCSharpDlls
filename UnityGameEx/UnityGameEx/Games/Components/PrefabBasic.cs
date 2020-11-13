using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void DF_ElementForeach(int index,GameObject gobj);

/// <summary>
/// 类名 : Prefab 单元对象
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-08-04 00:10
/// 功能 : 缓存需要操作的对象
/// </summary>
[System.Serializable]
public class PrefabBasic : GobjLifeListener {
	static public new PrefabBasic Get(GameObject gobj,bool isAdd){
		return UtilityHelper.Get<PrefabBasic>(gobj,true);
	}

	static public new PrefabBasic Get(GameObject gobj){
		return Get(gobj,true);
	}
	
	/// <summary>
	/// 操作的对象
	/// </summary>
	[SerializeField]
	protected GameObject[] m_gobjs = new GameObject[0];

    /// <summary>
    /// key = name or Relative name (相对应自身对象);
    /// val = gobj;
    /// </summary>
    protected Dictionary<string,GameObject> m_dicName2Gobj = new Dictionary<string,GameObject>();

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
		
		GameObject tmp = null;
		string _tmpName = "";
		for(int i = 0; i < m_gobjs.Length;i++)
		{
			tmp = m_gobjs[i];
			if(tmp){
				_tmpName = tmp.name;
				if(!m_dicName2Gobj.ContainsKey(_tmpName)){
					m_dicName2Gobj.Add(_tmpName,tmp);
				}else{
					Debug.LogError(string.Format("the same name = [{0}] in gameObject.name = [{1}]",_tmpName,tmp.name));
				}
				
				_tmpName = GetRelativeName(tmp);
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
		
		UtilityHelper.RecursionName(trsf,ref refName);
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
			return m_dicName2Gobj[elName];
		}
		return null;
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
			_gobj = m_gobjs [i];
			if(null == _gobj) continue;
			if(cfCall != null) cfCall(i,_gobj);
		}
	}

    public void SetChildGobjs(GameObject[] gobjs)
    {
        this.m_gobjs = gobjs;
    }

    [ContextMenu("Re-Rmv Empty")]
    void ReSizeList()
    {
        UtilityHelper.Is_App_Quit = false;
        List<GameObject> list = new List<GameObject>();
        GameObject _gobj = null;
        for (int i = 0; i < m_gobjs.Length; ++i)
        {
            _gobj = m_gobjs[i];
            if (null == _gobj) continue;
            if (!list.Contains(_gobj))
            {
                list.Add(_gobj);
            }
        }
        m_gobjs = list.ToArray();
        list.Clear();
    }

    [ContextMenu("Re-Rmv Empty(This and Childs)")]
    void ReSizeListAll()
    {
        UtilityHelper.Is_App_Quit = false;
        PrefabBasic[] arrs = this.m_gobj.GetComponentsInChildren<PrefabBasic>(true);
        foreach (var item in arrs)
        {
            item.ReSizeList();
        }
    }

    [ContextMenu("Re-Bind Transform's First Childs")]
    void ReBindAllFirstChilds()
    {
        UtilityHelper.Is_App_Quit = false;
        int lens = this.m_trsf.childCount;
        m_gobjs = new GameObject[lens];
        for (int i = 0; i < lens; i++)
        {
            m_gobjs[i] = this.m_trsf.GetChild(i).gameObject;
        }
    }

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
}