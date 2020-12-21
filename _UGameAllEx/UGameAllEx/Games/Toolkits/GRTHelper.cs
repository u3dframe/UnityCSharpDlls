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

    static public Rect ToScreenPointRect(UObject uobj, Camera cmr)
    {
        RectTransform rt = ToRectTransform(uobj);
        if (IsNull(rt))
            return Rect.zero;
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector2 v0 = RectTransformUtility.WorldToScreenPoint(cmr, corners[0]);
        Vector2 v1 = RectTransformUtility.WorldToScreenPoint(cmr, corners[2]);
        Rect rect = new Rect(v0, v1 - v0);
        return rect;
    }

    static public bool Overlaps(UObject uobj1, UObject uobj2, Camera cmr)
    {
        Rect rect1 = ToScreenPointRect(uobj1, cmr);
        Rect rect2 = ToScreenPointRect(uobj2, cmr);
        return rect1.Overlaps(rect2);
    }

    static public bool IsInCameraByViewPort(Camera cmr, UObject uobj, Vector4 v4Off)
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf))
            return false;
        if (IsNull(cmr) || !cmr.gameObject.activeInHierarchy)
            return false;
        Rect _viewport = new Rect(0, 0, 1, 1);
        Rect _curV = new Rect(_viewport);
        _curV.x += v4Off.x;
        _curV.y += v4Off.y;
        _curV.height += v4Off.z;
        _curV.width += v4Off.w;
        bool _isClipPlane = !_curV.Equals(_viewport);
        Vector3 v3 = cmr.WorldToViewportPoint(trsf.position);
        bool isIn = _curV.Contains(v3);
        bool isIn2 = !_isClipPlane;
        if (_isClipPlane && v3.z >= cmr.nearClipPlane && v3.z <= cmr.farClipPlane)
            isIn2 = true;
        return isIn && isIn2;
    }

    static public bool IsInCameraByViewPort(Camera cmr, UObject uobj)
    {
        return IsInCameraByViewPort(cmr, uobj, Vector4.zero);
    }

    static public bool IsInCameraByScreenPoint(Camera cmr, UObject uobj, float offW,float offH)
    {
        Transform trsf = ToTransform(uobj);
        if (IsNull(trsf))
            return false;
        if (IsNull(cmr) || !cmr.gameObject.activeInHierarchy)
            return false;
        if (offW <= 0) offW = 0;
        if (offH <= 0) offH = 0;
        bool _isClipPlane = offW < 1 && offH < 1;
        Rect _screen = new Rect(offW * -0.5f, offH * -0.5f,Screen.width + offW, Screen.height + offH);
        Vector3 v3 = cmr.WorldToScreenPoint(trsf.position);
        bool isIn = _screen.Contains(v3);
        bool isIn2 = !_isClipPlane;
        if (_isClipPlane && v3.z >= cmr.nearClipPlane && v3.z <= cmr.farClipPlane)
            isIn2 = true;
        return isIn && isIn2;
    }

    static public bool IsInCameraByScreenPoint(Camera cmr, UObject uobj)
    {
        return IsInCameraByScreenPoint(cmr,uobj,0,0);
    }
}