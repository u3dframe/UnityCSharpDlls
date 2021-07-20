using UnityEngine;
using UnityEngine.Playables;
using UObject = UnityEngine.Object;

/// <summary>
/// 类名 : 基础公用帮助脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-26 09:33
/// 功能 : 
/// </summary>
public class UtilityHelper : GHelper
{
    static public int SDWidth { get { return GameAppEx.sd_width; } }
    static public int SDHeight { get { return GameAppEx.sd_height; } }
    static public void ReResolution(float rate = 0.5f)
    {
        GameAppEx.ReResolution(rate);
    }

    static public void SetFrameRate(int frame)
    {
        GameAppEx.SetFrameRate(frame,true);
    }

    static public void SetFrameRateByRate(float rate)
    {
        GameAppEx.SetFrameRateByRate(rate);
    }

    static public RenderTextureFormat GetRTexFmt(RenderTextureFormat src)
    {
        if (src != RenderTextureFormat.Default && SystemInfo.SupportsRenderTextureFormat(src))
            return src;
        if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB565))
            src = RenderTextureFormat.RGB565;
        else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB4444))
            src = RenderTextureFormat.ARGB4444;
        else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32))
            src = RenderTextureFormat.ARGB32;
        else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
            src = RenderTextureFormat.ARGBHalf;
        else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat))
            src = RenderTextureFormat.ARGBFloat;
        else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
            src = RenderTextureFormat.Depth;
        else
            src = RenderTextureFormat.Default;
        return src;
    }

    static public Vector2 ScreenPointToLocalPointInRectangleBy(GameObject parent, Camera uiCamera, Vector2 screenPoint)
    {
        Vector2 _v2 = Vector2.zero;
        if (IsNull(parent)) return _v2;
        RectTransform _rect = ToRectTransform(parent);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, screenPoint, uiCamera, out _v2);
        return _v2;
    }

    static public void ScreenPointToLocalPointInRectangle(GameObject parent, Camera uiCamera, ref float pX, ref float pY)
    {
        Vector2 _screenPoint = new Vector2(pX, pY);
        Vector2 _v2 = ScreenPointToLocalPointInRectangleBy(parent, uiCamera, _screenPoint);
        pX = _v2.x;
        pY = _v2.y;
    }

    static public Vector3 ScreenPointToWorldPointInRectangleBy(GameObject parent, Camera uiCamera, Vector2 screenPoint)
    {
        Vector3 _v3 = Vector3.zero;
        if (IsNull(parent)) return _v3;
        RectTransform _rect = ToRectTransform(parent);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_rect, screenPoint, uiCamera, out _v3);
        return _v3;
    }

    static public void ScreenPointToWorldPointInRectangle(GameObject parent, Camera uiCamera, ref float pX, ref float pY, ref float pZ)
    {
        Vector2 _screenPoint = new Vector2(pX, pX);
        Vector3 _v3 = ScreenPointToWorldPointInRectangleBy(parent, uiCamera, _screenPoint);
        pX = _v3.x;
        pY = _v3.y;
        pZ = _v3.z;
    }

    static public Vector3 ToVec3ByDistance(Vector3 src,Vector3 dest,float distance)
    {
        Vector3 _ret = (dest - src).normalized;
        _ret = src + _ret * distance;
        return _ret;
    }

    static public Vector3 ToVec3ByDistance(UObject src, UObject dest, float distance)
    {
        Transform _src = ToTransform(src);
        Transform _dest = ToTransform(dest);
        if (_src == null || _dest == null)
            return Vector3.forward * distance;
        return ToVec3ByDistance(_src.position, _dest.position, distance);
    }

    static public Camera GetOrAddCamera(UObject uobj)
    {
        return Get<Camera>(uobj, true);
    }

    static public Animator GetOrAddAnimator(UObject uobj)
    {
        return Get<Animator>(uobj, true);
    }

    static public PlayableDirector GetOrAddPlayableDirector(UObject uobj)
    {
        return Get<PlayableDirector>(uobj, true);
    }

    static public CanvasGroup GetOrAddCanvasGroup(UObject uobj)
    {
        return Get<CanvasGroup>(uobj, true);
    }
}