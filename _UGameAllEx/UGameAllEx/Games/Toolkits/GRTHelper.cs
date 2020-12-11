using UnityEngine;
using System.Reflection;
using System;
using UObject = UnityEngine.Object;


/// <summary>
/// 类名 : RectTransform 公用帮助脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-12-09 09:33
/// 功能 : 
/// </summary>
public class GRTHelper : GHelper {

    static public Rect RViewport = new Rect(0, 0, 1, 1);

    static public Rect ToScreenPointRect(RectTransform rt,Camera cmr)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector2 v0 = RectTransformUtility.WorldToScreenPoint(cmr, corners[0]);
        Vector2 v1 = RectTransformUtility.WorldToScreenPoint(cmr, corners[2]);
        Rect rect = new Rect(v0, v1 - v0);
        return rect;
    }

    static public Rect ToScreenPointRect(GameObject gobj, Camera cmr)
    {
        RectTransform rt = ToRectTransform(gobj);
        if (IsNull(rt))
            return Rect.zero;
        return ToScreenPointRect(rt, cmr);
    }

    static public Rect ToScreenPointRect(Transform trsf, Camera cmr)
    {
        RectTransform rt = ToRectTransform(trsf);
        if (IsNull(rt))
            return Rect.zero;
        return ToScreenPointRect(rt, cmr);
    }

    static public bool Overlaps(RectTransform p1, RectTransform p2, Camera cmr)
    {
        Rect rect1 = ToScreenPointRect(p1, cmr);
        Rect rect2 = ToScreenPointRect(p2, cmr);
        return rect1.Overlaps(rect2);
    }

    static public bool Overlaps(GameObject p1, GameObject p2, Camera cmr)
    {
        Rect rect1 = ToScreenPointRect(p1, cmr);
        Rect rect2 = ToScreenPointRect(p2, cmr);
        return rect1.Overlaps(rect2);
    }

    static public bool Overlaps(Transform p1, Transform p2, Camera cmr)
    {
        Rect rect1 = ToScreenPointRect(p1, cmr);
        Rect rect2 = ToScreenPointRect(p2, cmr);
        return rect1.Overlaps(rect2);
    }

    static public bool IsInCamera(Camera cmr,Transform trsf,Vector4 v4Off)
    {
        if (null == trsf)
            return false;
        if (cmr == null || !cmr.gameObject.activeInHierarchy)
            return false;
        Rect _curV = new Rect(RViewport);
        _curV.x += v4Off.x;
        _curV.y += v4Off.y;
        _curV.height += v4Off.z;
        _curV.width += v4Off.w;
        bool _isClipPlane = !_curV.Equals(RViewport);
        Vector3 v3 = cmr.WorldToViewportPoint(trsf.position);
        bool isIn = _curV.Contains(v3);
        bool isIn2 = !_isClipPlane;
        if (_isClipPlane && v3.z >= cmr.nearClipPlane && v3.z <= cmr.farClipPlane)
            isIn2 = true;
        return isIn && isIn2;
    }

    static public bool IsInCamera(Camera cmr, Transform trsf)
    {
        return IsInCamera(cmr, trsf, Vector4.zero);
    }
}