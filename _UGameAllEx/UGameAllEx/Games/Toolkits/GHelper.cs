using UnityEngine;
using System.Reflection;
using System;
using UObject = UnityEngine.Object;


/// <summary>
/// 类名 : GameObject 公用帮助脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-26 09:33
/// 功能 : 泛型是不能被Tolua导成函数的
/// </summary>
public class GHelper : Core.Kernel.ObjEx
{
    static public bool Is_App_Quit = false;
    static public readonly DateTime DT_Start = new DateTime(1970, 1, 1, 0, 0, 0);

    static public DateTime ToTZLoc
    {
        get
        {
            // return TimeZone.CurrentTimeZone.ToLocalTime(DT_Start);
            return TimeZoneInfo.ConvertTime(DT_Start, TimeZoneInfo.Local);
        }
    }

    static public long TickStartUtc
    {
        get
        {
            return ToTZLoc.ToUniversalTime().Ticks;
        }
    }

    static public long TickStartLoc
    {
        get
        {
            return ToTZLoc.ToLocalTime().Ticks;
        }
    }

    /// <summary>
    /// getType
    /// </summary>
    /// <param name="classname"></param>
    /// <returns></returns>
    static public System.Type GetType(string classname)
    {
        Assembly assb = Assembly.GetExecutingAssembly();  //.GetExecutingAssembly();
        System.Type t = null;
        t = assb.GetType(classname);
        return t;
    }

    static public long GetTime()
    {
        TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - TickStartUtc);
        return (long)ts.TotalMilliseconds;
    }

    static public void ThrowError(string msg)
    {
        throw new Exception(msg);
    }

    static public bool IsNull(UObject uobj)
    {
        if (Is_App_Quit) return true;
        return null == uobj;
    }

    static public bool IsNoNull(UObject uobj)
    {
        if (Is_App_Quit) return false;
        return null != uobj;
    }

    // 在编辑模式下，这个函数有问题，即便为null对象，经过判断就不为空了
    static public bool IsNull(object obj)
    {
        return object.ReferenceEquals(obj, null);
    }

    static public bool IsNoNull(object obj)
    {
        return !IsNull(obj);
    }

    static public bool IsComponent(object obj)
    {
        if (IsNull(obj)) return false;
        return obj is Component;
    }

    static public bool IsCamera(object obj)
    {
        if (IsNull(obj)) return false;
        return obj is Camera;
    }

    static public bool IsTransform(object obj)
    {
        if (IsNull(obj)) return false;
        return obj is Transform;
    }

    static public bool IsGameObject(object obj)
    {
        if (IsNull(obj)) return false;
        return obj is GameObject;
    }

    static public GameObject ToGObj(UObject uobj)
    {
        if (IsNull(uobj)) return null;
        if (uobj is GameObject)
        {
            return uobj as GameObject;
        }
        else if (uobj is Component)
        {
            Component _c = uobj as Component;
            return _c.gameObject;
        }
        return null;
    }

    static public Transform ToTransform(UObject uobj)
    {
        if (IsNull(uobj)) return null;
        if (uobj is GameObject)
        {
            GameObject _g = uobj as GameObject;
            return _g.transform;
        }
        else if (uobj is Transform)
        {
            return uobj as Transform;
        }
        else if (uobj is Component)
        {
            Component _c = uobj as Component;
            return _c.transform;
        }
        return null;
    }

    static public RectTransform ToRectTransform(UObject uobj)
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return null;
        return trsf as RectTransform;
    }

    static public bool IsInParent(UObject uobj, UObject uobjParent)
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return false;
        Transform trsfParent = ToTransform(uobjParent);
        return trsf.parent == trsfParent;
    }

    static public bool IsInLayerMask(UObject uobj, LayerMask layerMask)
    {
        // 根据Layer数值进行移位获得用于运算的Mask值
        GameObject gobj = ToGObj(uobj);
        if (IsNull(gobj)) return false;
        int objLayerMask = 1 << gobj.layer;
        return (layerMask.value & objLayerMask) > 0;
    }

    static public bool IsInLayerMask(UObject uobj, int lMask)
    {
        LayerMask layerMask = lMask;
        return IsInLayerMask(uobj, layerMask);
    }

    static public T Get<T>(UObject uobj) where T : Component
    {
        GameObject gobj = ToGObj(uobj);
        if (IsNull(gobj)) return null;
        return gobj.GetComponent<T>();
    }

    static public T Get<T>(UObject uobj, bool isAdd) where T : Component
    {
        GameObject gobj = ToGObj(uobj);
        if (IsNull(gobj)) return null;
        T _r = gobj.GetComponent<T>();
        if (isAdd && IsNull(_r))
        {
            _r = gobj.AddComponent<T>();
        }
        return _r;
    }

    /// <summary>
    /// 搜索子物体组件
    /// </summary>
    static public T Get<T>(UObject uobj, string subnode) where T : Component
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return null;
        Transform sub = trsf.Find(subnode);
        return Get<T>(sub);
    }

    static public T Get<T>(UObject uobj, string subnode, bool isAdd) where T : Component
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return null;
        Transform sub = trsf.Find(subnode);
        return Get<T>(sub, isAdd);
    }

    static public T GetInParent<T>(UObject uobj,bool includeSelf) where T : Component
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return null;
        Transform _parent = includeSelf ? trsf : trsf.parent;
        T ret = _parent?.GetComponent<T>();
        if (!ret)
            ret = _parent?.GetComponentInParent<T>();
        return ret;
    }

    static public T GetInParentRecursion<T>(UObject uobj,bool includeSelf) where T : Component
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return null;
        Transform _parent = includeSelf ? trsf : trsf.parent;
        T ret = _parent?.GetComponent<T>();
        if (ret) return ret;
        _parent = includeSelf ? trsf : _parent;
        return GetInParentRecursion<T>(_parent.parent, true);
    }

    /// <summary>
    /// 添加组件
    /// </summary>
    static public T Add<T>(UObject uobj,bool isOnlyOne = true) where T : Component
    {
        GameObject gobj = ToGObj(uobj);
        if (IsNull(gobj))
            return null;
        if (isOnlyOne)
        {
            T[] ts = gobj.GetComponents<T>();
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] != null) GameObject.DestroyImmediate(ts[i]);
            }
        }
        return gobj.AddComponent<T>();
    }

    static public void DestroyAllChild(UObject uobj)
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return;
        GameObject gobjChild;
        while (true)
        {
            if (trsf.childCount <= 0)
                break;

            gobjChild = trsf.GetChild(0).gameObject;
            GameObject.DestroyImmediate(gobjChild);
        }
        trsf.DetachChildren();
    }

    /// <summary>
    /// 递归查找子对象
    /// </summary>
    static public Transform ChildRecursionTrsf(UObject uobj, string subnode)
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return null;
        if (trsf.name.Equals(subnode)) return trsf;
        int lens = trsf.childCount;
        Transform _ret = null;
        for (int i = 0; i < lens; i++)
        {
            _ret = ChildRecursionTrsf(trsf.GetChild(i), subnode);
            if (_ret != null)
                return _ret;
        }
        return null;
    }

    static public GameObject ChildRecursion(UObject uobj, string subnode)
    {
        Transform trsf = ChildRecursionTrsf(uobj, subnode);
        return trsf?.gameObject;
    }

    /// <summary>
    /// 查找子对象
    /// </summary>
    static public Transform ChildTrsf(UObject uobj, string subnode)
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return null;
        return trsf.Find(subnode);
    }

    static public GameObject Child(UObject uobj, string subnode)
    {
        Transform tf = ChildTrsf(uobj, subnode);
        if (IsNull(tf)) return null;
        return tf.gameObject;
    }

    /// <summary>
    /// 取平级对象
    /// </summary>
    static public GameObject Peer(UObject uobj, string subnode)
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return null;
        return Child(trsf.parent, subnode);
    }

    static public GameObject GetGobj(string name, bool isNew, bool isNoDestroy)
    {
        GameObject gobj = GameObject.Find(name);
        if (isNew && IsNull(gobj))
        {
            gobj = new GameObject(name);
        }
        if (isNoDestroy) GameObject.DontDestroyOnLoad(gobj);
        return gobj;
    }

    static public GameObject GetGobj(string name, bool isNew)
    {
        return GetGobj(name, isNew, false);
    }

    static public GameObject GetGobj(string name)
    {
        return GetGobj(name, true);
    }

    static public GameObject GetGobjNotDestroy(string name)
    {
        return GetGobj(name, true, true);
    }

    //设置子物体显示隐藏（不包括父物体本身）
    static public void SetChildActive(UObject uobj, bool isActive)
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return;
        int lens = trsf.childCount;
        GameObject _go_;
        for (int i = 0; i < lens; i++)
        {
            _go_ = trsf.GetChild(i).gameObject;
            _go_.SetActive(isActive);
        }
    }

    static public Transform GetParent(UObject uobj)
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return null;
        return trsf.parent;
    }

    /// <summary>
    /// 设置父节点
    /// </summary>
    static public void SetParent(UObject uobj, UObject uobjParent, bool isLocalZero)
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return;
        Transform trsfParent = ToTransform(uobjParent);
        trsf.SetParent(trsfParent, !isLocalZero);
        // trsf.parent = trsfParent;

        if (isLocalZero)
        {
            trsf.localPosition = Vector3.zero;
            trsf.localEulerAngles = Vector3.zero;
            trsf.localScale = Vector3.one;
        }
    }

    static public void SetParent(UObject uobj, UObject uobjParent)
    {
        SetParent(uobj, uobjParent, true);
    }

    static public void SetParentSyncLayer(UObject uobj, UObject uobjParent, bool isLocalZero)
    {
        GameObject gobj = ToGObj(uobj);
        GameObject gobjParent = ToGObj(uobjParent);
        SetParent(gobj, gobjParent, isLocalZero);
        if (IsNoNull(gobj) && IsNoNull(gobjParent))
        {
            int layer = gobjParent.layer;
            SetLayerBy(gobj, layer, true);
        }
    }

    static public GameObject Clone(UObject uobj, UObject uobjParent)
    {
        GameObject gobj = ToGObj(uobj);
        if (IsNull(gobj)) return null;
        Transform trsfParent = ToTransform(uobjParent);
        GameObject ret = GameObject.Instantiate(gobj, trsfParent, false) as GameObject;
        if (IsNoNull(trsfParent))
            SetParentSyncLayer(ret.transform, trsfParent, true);
        return ret;
    }

    static public GameObject Clone(UObject uobj)
    {
        Transform trsf = ToTransform(uobj);
        return Clone(trsf, trsf?.parent);
    }

    static public void SetLayer(UObject uobj, int layer)
    {
        GameObject gobj = ToGObj(uobj);
        if (IsNull(gobj)) return;
        gobj.layer = layer;
    }

    static public void SetLayer(UObject uobj, string nmLayer)
    {
        GameObject gobj = ToGObj(uobj);
        if (IsNull(gobj)) return;
        int layer = LayerMask.NameToLayer(nmLayer);
        gobj.layer = layer;
    }

    static public void SetLayerAll(UObject uobj, int layer)
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return;
        SetLayer(trsf, layer);

        int lens = trsf.childCount;
        for (int i = 0; i < lens; i++)
        {
            SetLayerAll(trsf.GetChild(i), layer);
        }
    }

    static public void SetLayerAll(UObject uobj, string nmLayer)
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf)) return;
        int layer = LayerMask.NameToLayer(nmLayer);
        SetLayerAll(trsf, layer);
    }

    static public void SetLayerBy(UObject uobj, string nmLayer, bool isAll)
    {
        if (isAll)
            SetLayerAll(uobj, nmLayer);
        else
            SetLayer(uobj, nmLayer);
    }

    static public void SetLayerBy(UObject uobj, int layer, bool isAll)
    {
        if (isAll)
            SetLayerAll(uobj, layer);
        else
            SetLayer(uobj, layer);
    }

    static public Vector3 ToVec3(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }

    static public void GetRectSize(UObject uobj, ref float w, ref float h)
    {
        w = 0; h = 0;
        RectTransform _r = ToRectTransform(uobj);
        if (IsNull(_r)) return;
        var v2 = _r.rect.size;
        w = v2.x;
        h = v2.y;
    }

    static public void SetRectSize(UObject uobj,float w,float h)
    {
        RectTransform _r = ToRectTransform(uobj);
        if (IsNull(_r)) return;
        Vector2 v2 = _r.rect.size;
        Vector2 v2n = new Vector2(w, h);
        Vector2 deltaSize = v2n - v2;
        _r.offsetMin = _r.offsetMin - new Vector2(deltaSize.x * _r.pivot.x, deltaSize.y * _r.pivot.y);
        _r.offsetMax = _r.offsetMax + new Vector2(deltaSize.x * (1f - _r.pivot.x), deltaSize.y * (1f - _r.pivot.y));
    }

    static public void SetSizeDelta(UObject uobj, float w, float h)
    {
        RectTransform _r = ToRectTransform(uobj);
        if (IsNull(_r)) return;
        _r.sizeDelta = new Vector2(w, h);
    }

    // Relative 相对
    static public void RecursionName(UObject uobj, ref string refName)
    {
        Transform trsf = ToTransform(uobj);
        if (!trsf)
            return;

        if (string.IsNullOrEmpty(refName))
            refName = trsf.name;
        else
            refName = trsf.name + "/" + refName;

        RecursionName(trsf.parent, ref refName);
    }

    static public string RelativeName(UObject uobj)
    {
        string ret = "";
        Transform trsf = ToTransform(uobj);
        RecursionName(trsf, ref ret);
        return ret;
    }

    static public Material NewMat(Material org)
    {
        if (IsNull(org))
            return null;
        return new Material(org);
    }

    static public Material NewMat(Shader org)
    {
        if (IsNull(org))
            return null;
        return new Material(org);
    }

    static public void SetMaxFrame(int maxFrame)
    {
        Application.targetFrameRate = maxFrame;
    }

    static public bool IsGLife(object obj)
    {
        if (IsNull(obj)) return false;
        return obj is GobjLifeListener;
    }

    static public bool IsElement(object obj)
    {
        if (IsNull(obj)) return false;
        return obj is PrefabBasic;
    }

    static public Color ToColor(float r,float g,float b,float a = 1)
    {
        r = r > 1 ? r / 255f : r;
        g = g > 1 ? g / 255f : g;
        b = b > 1 ? b / 255f : b;
        a = a > 1 ? a / 255f : a;
        return new Color(r, g, b, a);
    }

    /// <summary>
    /// 网络可用
    /// </summary>
    static public bool NetAvailable
    {
        get
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }

    /// <summary>
    /// 是否是无线
    /// </summary>
    static public bool IsWifi
    {
        get
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }
    }

    static public void Log(string str)
    {
        Debug.Log(str);
    }

    static public void LogWarning(string str)
    {
        Debug.LogWarning(str);
    }

    static public void LogError(string str)
    {
        Debug.LogError(str);
    }
}