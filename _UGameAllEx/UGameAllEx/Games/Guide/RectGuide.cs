using UnityEngine;
using UnityEngine.UI;

public class RectGuide : GuideBase
{
    protected float width;//镂空宽
    protected float height;//镂空高
    Vector2 imgpianyi = new Vector2(50, 50);
    float scalewidth;
    float scaleheight;
    public override void Guide(Canvas canvas, RectTransform target, out Vector3 v3)
    {
        base.Guide(canvas, target,out v3);
        //计算宽高
        width = (targetCorners[3].x - targetCorners[0].x) / 2;
        height = (targetCorners[1].y - targetCorners[0].y) / 2;
        material.SetFloat("_SliderX", width);
        material.SetFloat("_SliderY", height);
    }

    public override void Guide(Canvas canvas, RectTransform target, float scale, float time, RectTransform img)
    {
        Vector3 v3;
        this.Guide(canvas, target, out v3);

        scalewidth = width * scale;
        scaleheight = height * scale;
        material.SetFloat("_SliderX", scalewidth);
        material.SetFloat("_SliderY", scaleheight);

        this.time = time;
        isScaling = true;
        timer = 0;

        img.anchoredPosition = v3;
        img.sizeDelta = new Vector2(scalewidth + 55, scaleheight + 55) ;
    }

    protected override void Update()
    {
        base.Update();
        if (isScaling)
        {
            this.material.SetFloat("_SliderX", Mathf.Lerp(scalewidth, width, timer));
            this.material.SetFloat("_SliderY", Mathf.Lerp(scaleheight, height, timer));
        }
    }
}