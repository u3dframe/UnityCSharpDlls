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
}