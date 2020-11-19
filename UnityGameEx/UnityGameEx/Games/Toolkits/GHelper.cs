using UnityEngine;
using System.Reflection;
using System;
using UObject = UnityEngine.Object;


/// <summary>
/// 类名 : Gamobje 公用帮助脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-26 09:33
/// 功能 : 泛型是不能被Tolua导成函数的
/// </summary>
public class GHelper {
	static public bool Is_App_Quit = false;
	static public readonly DateTime DT_Start = new DateTime(1970, 1, 1, 0, 0, 0);

	static public DateTime ToTZLoc{
		get{
            // return TimeZone.CurrentTimeZone.ToLocalTime(DT_Start);
            return TimeZoneInfo.ConvertTime(DT_Start, TimeZoneInfo.Local);
		} 
	}
	
	static public long TickStartUtc{
		get{
			return ToTZLoc.ToUniversalTime().Ticks;
		}
	}
	
	static public long TickStartLoc{
		get{
			return ToTZLoc.ToLocalTime().Ticks;
		}
	}

	/// <summary>
	/// getType
	/// </summary>
	/// <param name="classname"></param>
	/// <returns></returns>
	static public System.Type GetType(string classname) {
		Assembly assb = Assembly.GetExecutingAssembly();  //.GetExecutingAssembly();
		System.Type t = null;
		t = assb.GetType(classname);
		return t;
	}
	
	static public long GetTime() {
		TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - TickStartUtc);
		return (long)ts.TotalMilliseconds;
	}

	static public void ThrowError(string msg) {
		throw new Exception(msg);
	}

	static public bool IsNull(UObject uobj)
	{
		if(Is_App_Quit) return true;
		return null == uobj;
	}

	static public bool IsNoNull(UObject uobj)
	{
		if(Is_App_Quit) return false;
		return null != uobj;
	}
	
	// 在编辑模式下，这个函数有问题，即便为null对象，经过判断就不为空了
	static public bool IsNull(object obj)
	{
		return object.ReferenceEquals(obj,null);
	}

	static public bool IsNoNull(object obj)
	{
		return !IsNull(obj);
	}

	static public bool IsComponent(object obj) {
		if(IsNull(obj))	return false;
		return obj is Component;
	}

	static public bool IsCamera(object obj) {
		if(IsNull(obj))	return false;
		return obj is Camera;
	}

	static public bool IsTransform(object obj) {
		if(IsNull(obj))	return false;
		return obj is Transform;
	}

	static public bool IsGameObject(object obj) {
		if(IsNull(obj))	return false;
		return obj is GameObject;
	}

	static public bool IsInParent(Transform trsf, Transform trsfParent) {
		if(IsNull(trsf)) return false;
		return trsf.parent == trsfParent;
	}

	static public bool IsInParent(GameObject gobj, GameObject gobjParent) {
		return IsInParent(gobj?.transform,gobjParent?.transform);
	}

	static public bool IsInLayerMask(GameObject gobj, LayerMask layerMask) {
        // 根据Layer数值进行移位获得用于运算的Mask值
		if(IsNull(gobj)) return false;
        int objLayerMask = 1 << gobj.layer;
        return (layerMask.value & objLayerMask) > 0;
    }

	static public T Get<T>(GameObject go) where T : Component {
		if(IsNull(go)) return null;
		return go.GetComponent<T>();
	}

	static public T Get<T>(Transform trsf) where T : Component {
		if(IsNull(trsf)) return null;
		return trsf.GetComponent<T>();
	}

	static public T Get<T>(GameObject gobj,bool isAdd) where T : Component {
		if(IsNull(gobj)) return null;
		T _r = gobj.GetComponent<T> ();
		if (isAdd && IsNull(_r)) {
			_r = gobj.AddComponent<T> ();
		}
		return _r;
	}

	static public T Get<T>(Transform trsf,bool isAdd) where T : Component {
		if(IsNull(trsf)) return null;
		return Get<T>(trsf.gameObject,isAdd);
	}

	/// <summary>
	/// 搜索子物体组件-GameObject版
	/// </summary>
	static public T Get<T>(GameObject go, string subnode) where T : Component {
		if(IsNull(go)) return null;
		Transform sub = go.transform.Find(subnode);
		return Get<T>(sub);
	}

	/// <summary>
	/// 搜索子物体组件-Transform版
	/// </summary>
	static public T Get<T>(Transform trsf, string subnode) where T : Component {
		if(IsNull(trsf)) return null;
		Transform sub = trsf.Find(subnode);
		return Get<T>(sub);
	}

	static public T Get<T>(GameObject go, string subnode,bool isAdd) where T : Component {
		if(IsNull(go)) return null;
		Transform sub = go.transform.Find(subnode);
		return Get<T>(sub,isAdd);
	}

	static public T Get<T>(Transform trsf, string subnode,bool isAdd) where T : Component {
		if(IsNull(trsf)) return null;
		Transform sub = trsf.Find(subnode);
		return Get<T>(sub,isAdd);
	}

	/// <summary>
	/// 搜索子物体组件-Component版
	/// </summary>
	static public T Get<T>(Component go, string subnode) where T : Component {
		return go.transform.Find(subnode).GetComponent<T>();
	}

	static public T GetInParent<T>(GameObject gobj) where T : Component {
		if(IsNull(gobj)) return null;
		T ret = gobj.GetComponent<T> ();
		if (!ret) {
			ret = gobj.GetComponentInParent<T> ();
		}
		return ret;
	}

	static public T GetInParent<T>(Transform trsf) where T : Component {
		if(IsNull(trsf)) return null;
		return GetInParent<T>(trsf.gameObject);
	}

	static public T GetInParentRecursion<T>(Transform trsf) where T : Component {
		if(IsNull(trsf)) return null;
		T ret = trsf.GetComponent<T> ();
		if (ret != null) return ret;
		return GetInParentRecursion<T>(trsf.parent);
	}

	static public T GetInParentRecursion<T>(GameObject gobj) where T : Component {
		if(IsNull(gobj)) return null;
		return GetInParentRecursion<T>(gobj.transform);
	}

	/// <summary>
	/// 添加组件
	/// </summary>
	static public T Add<T>(GameObject go) where T : Component {
		if (IsNoNull(go)) {
			T[] ts = go.GetComponents<T>();
			for (int i = 0; i < ts.Length; i++) {
				if (ts[i] != null) GameObject.Destroy(ts[i]);
			}
			return go.AddComponent<T>();
		}
		return null;
	}

	/// <summary>
	/// 添加组件
	/// </summary>
	static public T Add<T>(Transform go) where T : Component {
		return Add<T>(go.gameObject);
	}

	/// <summary>
	/// 递归查找子对象
	/// </summary>
	static public Transform ChildRecursion(Transform trsf, string subnode) {
		if(IsNull(trsf)) return null;
		if(trsf.name.Equals(subnode)) return trsf;
		int lens = trsf.childCount;
		Transform _ret = null; 
		for(int i = 0; i < lens;i++){
			_ret = ChildRecursion(trsf.GetChild(i),subnode);
			if(_ret != null)
				return _ret;
		}
		return null;
	}

	static public GameObject ChildRecursion(GameObject gobj, string subnode) {
		if(IsNull(gobj)) return null;
		Transform trsf = ChildRecursion(gobj.transform,subnode);
		if(IsNull(trsf)) return null;
		return trsf.gameObject;
	}

	/// <summary>
	/// 查找子对象
	/// </summary>
	static public Transform ChildTrsf(Transform trsf, string subnode) {
		if(IsNull(trsf)) return null;
		return trsf.Find(subnode);
	}

	static public Transform ChildTrsf(GameObject gobj, string subnode) {
		if(IsNull(gobj)) return null;
		return ChildTrsf(gobj.transform,subnode);
	}
	
	static public GameObject Child(Transform trsf, string subnode) {
		Transform tf = ChildTrsf(trsf,subnode);
		if(IsNull(tf)) return null;
		return tf.gameObject;
	}

	/// <summary>
	/// 查找子对象
	/// </summary>
	static public GameObject Child(GameObject gobj, string subnode) {
		if(IsNull(gobj)) return null;
		return Child(gobj.transform, subnode);
	}

	/// <summary>
	/// 取平级对象
	/// </summary>
	static public GameObject Peer(Transform trsf, string subnode) {
		if(IsNull(trsf)) return null;
		return Child(trsf.parent,subnode);
	}
	
	/// <summary>
	/// 取平级对象
	/// </summary>
	static public GameObject Peer(GameObject gobj, string subnode) {
		if(IsNull(gobj)) return null;
		return Peer(gobj.transform, subnode);
	}

	static public GameObject GetGobj(string name,bool isNew,bool isNoDestroy) {
		GameObject gobj = GameObject.Find(name);
		if (isNew && IsNull(gobj)) {
			gobj = new GameObject(name);
		}
		if(isNoDestroy) GameObject.DontDestroyOnLoad (gobj);
		return gobj;
	}

	static public GameObject GetGobj(string name,bool isNew) {
		return GetGobj(name,isNew,false);
	}

	static public GameObject GetGobj(string name) {
		return GetGobj(name,true);
	}

	static public GameObject GetGobjNotDestroy(string name) {
		return GetGobj(name,true,true);
	}

	//设置子物体显示隐藏（不包括父物体本身）
	static public void SetChildActive(GameObject gobj, bool isActive)
	{
		SetChildActive(gobj?.transform,isActive);
	}

	static public void SetChildActive(Transform trsf, bool isActive)
	{
		if(IsNull(trsf)) return;
		int lens = trsf.childCount;
		GameObject _go_;
		for (int i = 0; i < lens; i++)
		{
			_go_ = trsf.GetChild(i).gameObject;
			_go_.SetActive(isActive);
		}
	}

	/// <summary>
	/// 设置父节点
	/// </summary>
	static public void SetParent(Transform trsf,Transform trsfParent,bool isLocalZero) {
		if(IsNull(trsf)) return;
		trsf.SetParent (trsfParent,!isLocalZero);
		// trsf.parent = trsfParent;
		
		if(isLocalZero){
			trsf.localPosition = Vector3.zero;
			trsf.localEulerAngles = Vector3.zero;
			trsf.localScale = Vector3.one;
		}
	}

	static public void SetParent(Transform trsf,Transform trsfParent) {
		SetParent(trsf,trsfParent,true); 
	}

	/// <summary>
	/// 设置父节点
	/// </summary>
	static public void SetParent(GameObject gobj, GameObject gobjParent,bool isLocalZero) {
		if(IsNull(gobj)) return;
		Transform trsf = gobj.transform;
		Transform trsfParent = null;
		if (gobjParent != null) trsfParent = gobjParent.transform;
		SetParent(trsf, trsfParent, isLocalZero);
	}

	static public void SetParent(GameObject gobj, GameObject gobjParent) {
		SetParent(gobj,gobjParent,true); 
	}

	static public void SetParentSyncLayer(Transform trsf,Transform trsfParent,bool isLocalZero) {
		SetParent(trsf,trsfParent,isLocalZero);
		if(IsNoNull(trsf) && IsNoNull(trsfParent)){
			int layer = trsfParent.gameObject.layer;
			SetLayerBy(trsf.gameObject,layer,true);
		}
	}

	static public void SetParentSyncLayer(GameObject gobj, GameObject gobjParent,bool isLocalZero) {
		SetParent(gobj,gobjParent,isLocalZero);
		if(IsNoNull(gobj) && IsNoNull(gobjParent)){
			int layer = gobjParent.layer;
			SetLayerBy(gobj,layer,true);
		}
	}

	static public GameObject Clone(GameObject gobj,Transform parent) {
		if(IsNull(gobj)) return null;
		GameObject ret = GameObject.Instantiate(gobj, parent, false) as GameObject;

		if(IsNoNull(parent)){
			SetParentSyncLayer(ret.transform,parent,true);
		}
		
		return ret;
	}

	static public GameObject Clone(GameObject gobj,GameObject gobjParent) {
		return Clone(gobj,gobjParent?.transform);
	}

	static public GameObject Clone(Transform trsf,Transform parent) {
		return Clone (trsf?.gameObject,parent);
	}

	static public GameObject Clone(GameObject gobj) {
		return Clone (gobj,gobj?.transform.parent);
	}

	static public GameObject Clone(Transform trsf) {
		return Clone(trsf?.gameObject);
	}

	static public RectTransform ToRectTransform(Transform trsf) {
		if(IsNull(trsf)) return null;
		return trsf as RectTransform;
	}

	static public RectTransform ToRectTransform(GameObject gobj) {
		return ToRectTransform(gobj?.transform);
	}

	static public void SetLayer(GameObject gobj,int layer){
		if(IsNull(gobj)) return;
		gobj.layer = layer;
	}

	static public void SetLayer(Transform trsf,int layer){
		if(IsNull(trsf)) return;
		SetLayer(trsf.gameObject,layer);
	}

	static public void SetLayer(GameObject gobj,string nmLayer){
		if(IsNull(gobj)) return;
		int layer = LayerMask.NameToLayer(nmLayer);
		gobj.layer = layer;
	}

	static public void SetLayer(Transform trsf,string nmLayer){
		if(IsNull(trsf)) return;
		SetLayer(trsf.gameObject,nmLayer);
	}

	static public void SetLayerAll(Transform trsf,int layer){
		if(IsNull(trsf)) return;
		SetLayer(trsf,layer);

		int lens = trsf.childCount;
		for (int i = 0; i < lens; i++)
		{
			SetLayerAll(trsf.GetChild(i),layer);
		}
	}

	static public void SetLayerAll(GameObject gobj,int layer){
		if(IsNull(gobj)) return;
		SetLayerAll(gobj.transform,layer);
	}

	static public void SetLayerAll(Transform trsf,string nmLayer){
		if(IsNull(trsf)) return;
		int layer = LayerMask.NameToLayer(nmLayer);
		SetLayerAll(trsf,layer);
	}

	static public void SetLayerAll(GameObject gobj,string nmLayer){
		if(IsNull(gobj)) return;
		int layer = LayerMask.NameToLayer(nmLayer);
		SetLayerAll(gobj,layer);
	}

	static public void SetLayerBy(GameObject gobj,string nmLayer,bool isAll){
		if(isAll){
			SetLayerAll(gobj,nmLayer);
		}else{
			SetLayer(gobj,nmLayer);
		}
	}

	static public void SetLayerBy(Transform trsf,string nmLayer,bool isAll){
		if(isAll){
			SetLayerAll(trsf,nmLayer);
		}else{
			SetLayer(trsf,nmLayer);
		}
	}

	static public void SetLayerBy(GameObject gobj,int layer,bool isAll){
		if(isAll){
			SetLayerAll(gobj,layer);
		}else{
			SetLayer(gobj,layer);
		}
	}

	static public void SetLayerBy(Transform trsf,int layer,bool isAll){
		if(isAll){
			SetLayerAll(trsf,layer);
		}else{
			SetLayer(trsf,layer);
		}
	}

	static public Vector3 ToVec3(float x,float y,float z){
		return new Vector3(x,y,z);
	}

	static public void GetRectSize(Transform trsf,ref float w,ref float h) {
		w = 0;h = 0;
		if(IsNull(trsf)) return;
		RectTransform _r = trsf as RectTransform;
		var v2 = _r.rect.size;
		w = v2.x;
		h = v2.y;
	}

	static public void GetRectSize(GameObject gobj,ref float w,ref float h) {
		w = 0;h = 0;
		if(IsNull(gobj)) return;
		GetRectSize(gobj.transform,ref w,ref h);
	}

	static public Vector2 ScreenPointToLocalPointInRectangleBy(GameObject parent,Camera uiCamera,Vector2 screenPoint) {
		Vector2 _v2 = Vector2.zero;
		if(IsNull(parent)) return _v2;
		RectTransform _rect = ToRectTransform(parent);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect,screenPoint,uiCamera,out _v2);
		return _v2;
	}

	static public void ScreenPointToLocalPointInRectangle(GameObject parent,Camera uiCamera,ref float pX,ref float pY) {
		Vector2 _screenPoint = new Vector2(pX,pY);
		Vector2 _v2 = ScreenPointToLocalPointInRectangleBy(parent,uiCamera,_screenPoint);
		pX = _v2.x;
		pY = _v2.y;
	}

	static public Vector3 ScreenPointToWorldPointInRectangleBy(GameObject parent,Camera uiCamera,Vector2 screenPoint) {
		Vector3 _v3 = Vector3.zero;
		if(IsNull(parent)) return _v3;
		RectTransform _rect = ToRectTransform(parent);
		RectTransformUtility.ScreenPointToWorldPointInRectangle(_rect,screenPoint,uiCamera,out _v3);
		return _v3;
	}

	static public void ScreenPointToWorldPointInRectangle(GameObject parent,Camera uiCamera,ref float pX,ref float pY,ref float pZ) {
		Vector2 _screenPoint = new Vector2(pX,pX);
		Vector3 _v3 = ScreenPointToWorldPointInRectangleBy(parent,uiCamera,_screenPoint);
		pX = _v3.x;
		pY = _v3.y;
		pZ = _v3.z;
	}

	// Relative 相对
	static public void RecursionName(Transform trsf,ref string refName)
	{
		if(!trsf){
			return;
		}

		if(string.IsNullOrEmpty(refName))
		{
			refName = trsf.name;
		} else {
			refName = trsf.name + "/" + refName;
		}

		RecursionName(trsf.parent,ref refName);
	}

	static public string RelativeName(Transform trsf){
		string ret = "";
		RecursionName(trsf,ref ret);
		return ret;
	}

	static public string RelativeName(GameObject gobj){
		string ret = "";
		if(gobj){
			RecursionName(gobj.transform,ref ret);
		}
		return ret;
	}
}