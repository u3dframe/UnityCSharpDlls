using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GuideBase : MonoBehaviour
{
    protected Material material;//材质
    protected Vector3 center;//镂空中心
    protected RectTransform target;//被引导的目标对象
    protected Vector3[] targetCorners = new Vector3[4];//引导目标的边界

    protected float timer;//计时器，来达到动画匀速播放
    protected float time;//整体动画时间
    protected bool isScaling;//是否正在缩放
                             //虚方法，子类可以去重写，里面用来判断动画是否播放，如果播放，就按照既定的时间匀速完成
    protected virtual void Update()
    {
        if (isScaling)
        {
            timer += Time.unscaledDeltaTime * 1 / time;
            if (timer >= 1)
            {
                timer = 0;
                isScaling = false;
            }
        }
    }
    //这里是来获取目标物体的四个点来计算中心点，因为对于矩形或者圆形效果，他们面对的中心点是确定的
    public virtual void Guide(Canvas canvas, RectTransform target, out Vector3 v3)
    {
        material = GetComponent<Image>().material;
        this.target = target;
        //获取四个点的世界坐标
        target.GetWorldCorners(targetCorners);
        //世界坐标转屏幕坐标
        for (int i = 0; i < targetCorners.Length; i++)
        {
            targetCorners[i] = WorldToScreenPoints(canvas, targetCorners[i]);
        }
        //计算中心点
        center.x = targetCorners[0].x + (targetCorners[3].x - targetCorners[0].x) / 2;
        center.y = targetCorners[0].y + (targetCorners[1].y - targetCorners[0].y) / 2;
        //设置中心点
        material.SetVector("_Center", center);
        v3 = center;
    }
    //为了让子类继承的时候直接重写就可以，因为矩形和圆形的动画方式不一样，跟长宽或者半径有关
    public virtual void Guide(Canvas canvas, RectTransform target, float scale, float time, RectTransform img)
    {

    }
    //坐标的转换
    public Vector2 WorldToScreenPoints(Canvas canvas, Vector3 world)
    {
        //把世界转屏幕
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, world);
        Vector2 localPoint;
        //屏幕转局部坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPoint, canvas.worldCamera, out localPoint);
        return localPoint;
    }
}
