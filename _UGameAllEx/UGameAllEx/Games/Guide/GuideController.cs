using UnityEngine;
using UnityEngine.UI;

public enum GuideType
{
    Rect,
    Circle,
}

[RequireComponent(typeof(RectGuide))]
[RequireComponent(typeof(CircleGuide))]
public class GuideController : MonoBehaviour, ICanvasRaycastFilter
{
    public RectTransform FImg;

    private CircleGuide circleGuide;
    private RectGuide rectGuide;
    public Material rectMat;
    public Material circleMat;
    private Image mask;
    private RectTransform target;
    UGUIEventListener uguiButton;

    DF_UGUIPos ClickEvt;
    private void Awake()
    {
        mask = transform.GetComponent<Image>();
        if (mask == null) { throw new System.Exception("mask初始化失败"); }
        if (rectMat == null || circleMat == null) { throw new System.Exception("材质未赋值"); }
        rectGuide = transform.GetComponent<RectGuide>();
        circleGuide = transform.GetComponent<CircleGuide>();
    }

    public void SetEvt(DF_UGUIPos cl)
    {
        ClickEvt = cl;
    }
    public void Guide(Canvas canvas, RectTransform target, int type, float scale, float time, RectTransform showtarget = null)
    {
        this.target = target;
        FImg.gameObject.SetActive(false);
        if (showtarget == null) { showtarget = target; }
        switch (type)
        {
            case (int)GuideType.Rect:
                mask.material = rectMat;
                rectGuide.Guide(canvas, showtarget, scale, time, FImg);
                FImg.gameObject.SetActive(true);
                break;
            case (int)GuideType.Circle:
                mask.material = circleMat;
                circleGuide.Guide(canvas, showtarget, scale, time, FImg);
                break;
        }
        uguiButton = target.GetComponent<UGUIEventListener>();
        if (uguiButton)
        {
            uguiButton.OnlyOnceCallClick(ClickEvt);
        }
    }
    //这里的方法代表是否镂空内容可被点击，返回false则可以，true则不可以
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (target == null) { return true; }
        bool IsRaycastLocationValid = !RectTransformUtility.RectangleContainsScreenPoint(target, sp, eventCamera);
        return IsRaycastLocationValid;
    }
}