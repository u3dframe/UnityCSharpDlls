using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 类名 : FlexibleGridLayoutGroup
/// 作者 : 赖永飞
/// 日期 : 2020-08-20 
/// 功能 : 自适应 Grid Layout Group
/// </summary>

public class FlexibleGridLayoutGroup : LayoutGroup
{
    public enum FitType
    {
        Horizontal,
        Vertical,
    }

    public FitType fitType;
    public Vector2 spacing;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        float initPos0 = fitType == FitType.Horizontal ? padding.left : padding.top;
        float currPos0 = initPos0;
        float currPos1 = fitType == FitType.Horizontal ? padding.top : padding.left;
        float maxCellSize = 0;
        for (int i = 0; i < rectChildren.Count; i++)
        {
            var item = rectChildren[i];
            var f = item.GetComponent<ContentSizeFitter>();
            if (f != null && f.isActiveAndEnabled)
            {
                f.SetLayoutHorizontal();
                f.SetLayoutVertical();
            }
            var itemWidth = item.rect.width;
            var itemHeight = item.rect.height;
            var s0 = fitType == FitType.Horizontal ? itemWidth : itemHeight;
            var s1 = fitType == FitType.Horizontal ? itemHeight : itemWidth;
            if (currPos0 + s0 > (fitType == FitType.Horizontal ? (rectTransform.rect.width - padding.right) : (rectTransform.rect.height - padding.bottom)))
            {
                currPos0 = initPos0;
                currPos1 += maxCellSize + (fitType == FitType.Horizontal ? spacing.y : spacing.x);
                maxCellSize = 0;
            }
            if (s1 > maxCellSize)
                maxCellSize = s1;
            SetChildAlongAxis(item, 0, fitType == FitType.Horizontal ? currPos0 : currPos1);
            SetChildAlongAxis(item, 1, fitType == FitType.Horizontal ? currPos1 : currPos0);
            currPos0 += s0 + (fitType == FitType.Horizontal ? spacing.x : spacing.y);
        }
    }

    public override void CalculateLayoutInputVertical()
    {
        
    }

    public override void SetLayoutHorizontal()
    {

    }

    public override void SetLayoutVertical()
    {

    }
}