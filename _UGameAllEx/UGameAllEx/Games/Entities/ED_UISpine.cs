using UnityEngine;
using System;
using Spine.Unity;
/// <summary>
/// 类名 : UISpine 数据脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2021-01-06 20:03
/// 功能 : 
/// </summary>
public class ED_UISpine : ED_Animator
{
    static public new ED_UISpine Builder(UnityEngine.Object uobj)
    {
        return Builder<ED_UISpine>(uobj);
    }
    
    public SkeletonAnimation m_csSpine { get; private set; } // Render渲染
    public RendererSortOrder m_csSortLayer { get; private set; } // Render渲染

    public ED_UISpine() : base()
    {
    }

    override public void InitComp(Component comp, Action cfDestroy, Action cfShow, Action cfHide)
    {
        base.InitComp(comp, cfDestroy, cfShow, cfHide);

        this.m_csSpine = this.m_gobj.GetComponentInChildren<SkeletonAnimation>(true);
        this.m_csSortLayer = RendererSortOrder.Get(this.m_gobj).Init(false);
        this.m_csSortLayer.m_sType = RendererSortOrder.SortType.SLayer;
    }

    override protected void On_Destroy(GobjLifeListener obj)
    {
        this.m_csSpine = null;
        this.m_csSortLayer = null;

        base.On_Destroy(obj);
    }

    public void SetPars(UnityEngine.Object uobjParent,Vector3 locScale,Vector3 locPos)
    {
        if(UtilityHelper.IsNull(this.m_gobj))
            return;
        
        UtilityHelper.SetParent(this.m_trsf,uobjParent,true);
        RectTransform s = uobjParent as RectTransform;
        s.localScale = locScale;
        s.localPosition = locPos;
        //this.m_trsf.localScale = locScale;
        //this.m_trsf.localPosition = locPos;

        CanvasEx _c_ = CanvasEx.GetInParent( this.m_trsf );
        if(_c_ != null)
        {
            this.m_csSortLayer.m_delay_excsort = -2;
            this.m_csSortLayer.m_nmLayer = _c_.m_sortingLayerName;
            this.m_csSortLayer.m_val_layer = _c_.m_curSortOrder + 1;
            this.m_csSortLayer.ReRenderSorting();
        }
    }

    public void SetPars(UnityEngine.Object uobjParent,Vector3 locScale)
    {
        this.SetPars(uobjParent,locScale,Vector3.zero);
    }

}
