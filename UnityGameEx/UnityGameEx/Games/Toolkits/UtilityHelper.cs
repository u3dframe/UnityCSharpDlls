using UnityEngine;
using UnityEngine.Playables;


/// <summary>
/// 类名 : 基础公用帮助脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-26 09:33
/// 功能 : 
/// </summary>
public class UtilityHelper : GHelper {
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

    static public Camera GetOrAddCamera(GameObject gobj){
		return Get<Camera>(gobj,true);
	}

	static public Camera GetOrAddCamera(Transform trsf){
		return Get<Camera>(trsf,true);
	}

	static public Animator GetOrAddAnimator(GameObject gobj){
		return Get<Animator>(gobj,true);
	}

	static public Animator GetOrAddAnimator(Transform trsf){
		return Get<Animator>(trsf,true);
	}

	static public PlayableDirector GetOrAddPlayableDirector(GameObject gobj)
	{
		return Get<PlayableDirector>(gobj, true);
	}

	static public PlayableDirector GetOrAddPlayableDirector(Transform trsf)
	{
		return Get<PlayableDirector>(trsf, true);
	}
}