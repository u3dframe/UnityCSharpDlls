using UnityEngine;
using System;
/// <summary>
/// 类名 : UIEffect 数据脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2021-01-04 15:03
/// 功能 : 
/// </summary>
public class ED_UIEffect : Core.Kernel.Beans.ED_Comp
{
    static public new ED_UIEffect Builder(UnityEngine.Object uobj)
    {
        return Builder<ED_UIEffect>(uobj);
    }
    
    public ParticleSystemEx m_csPsEx { get; private set; } // 粒子特效
    public RendererSortOrder m_csSortLayer { get; private set; } // Render渲染
    protected SpriteMask[] m_csSpriteMasks { get; private set; } // SpriteMask组件
    public float m_maxTime { get{ return m_csPsEx != null ? m_csPsEx.maxTime : 1f;} }

    public ED_UIEffect() : base()
    {
    }

    override public void InitComp(string strComp, Action cfDestroy, Action cfShow, Action cfHide)
    {
        base.InitComp(strComp, cfDestroy, cfShow, cfHide);
    }

    override public void InitComp(Component comp, Action cfDestroy, Action cfShow, Action cfHide)
    {
        if(comp == null)
            comp = PrefabElement.Get(this.m_gobj,false);
        base.InitComp(comp, cfDestroy, cfShow, cfHide);
        this.m_csPsEx = ParticleSystemEx.Get(this.m_gobj);
        this.m_csSortLayer = RendererSortOrder.Get(this.m_gobj).Init(false);
        this.m_csSpriteMasks = m_trsf.GetComponentsInChildren<SpriteMask>();
    }

    override protected void On_Destroy(GobjLifeListener obj)
    {
        this.m_csPsEx = null;
        this.m_csSortLayer = null;
        this.m_csSpriteMasks = null;

        base.On_Destroy(obj);
    }

    public void SetPars(UnityEngine.Object uobjParent,Vector3 locPos,Vector3 locAngle,Vector3 locScale)
    {
        if(UtilityHelper.IsNull(this.m_gobj))
            return;
        
        UtilityHelper.SetParent(this.m_trsf,uobjParent,true);
        this.m_trsf.localPosition = locPos;
        this.m_trsf.localEulerAngles = locAngle;
        this.m_trsf.localScale = locScale;

        CanvasEx _c_ = CanvasEx.GetInParent( this.m_trsf );
        if(_c_ != null)
        {
            int _s_id = _c_.m_sortingLayerID;
            this.m_csSortLayer.m_delay_excsort = -2;
            this.m_csSortLayer.m_nmLayer = _c_.m_sortingLayerName;
            this.m_csSortLayer.m_val_layer = _c_.m_curSortOrder;
            this.m_csSortLayer.ReRenderSorting();

            if(this.m_csSpriteMasks != null && this.m_csSpriteMasks.Length > 0)
            {
                SpriteMask _smask;
                int _maxLayer = 0;
                Transform _child;
                Renderer _rer = null;
                for (int i = 0; i < this.m_csSpriteMasks.Length; i++)
                {
                    _smask = this.m_csSpriteMasks[i];
                    _smask.sortingLayerID = _s_id;
                    _smask.frontSortingLayerID = _s_id;
                    _smask.backSortingLayerID = _s_id;

                    _child = _smask.transform.GetChild(0);
                    _rer = _child?.GetComponent<Renderer>();
                    _maxLayer = _rer ? _rer.sortingOrder : -9999999;
                    if(_maxLayer != -9999999)
                    {
                        _smask.sortingOrder = _maxLayer;
                        _smask.backSortingOrder = Mathf.Max(1, _maxLayer - 1);
                        _smask.frontSortingOrder = _maxLayer + 1;
                    }
                }
            }
        }
    }

    public void SetPars(UnityEngine.Object uobjParent,Vector3 locPos,Vector3 locScale)
    {
        this.SetPars(uobjParent,locPos,Vector3.zero,locScale);
    }

    public void SetPars(UnityEngine.Object uobjParent,Vector3 locPos)
    {
        this.SetPars(uobjParent,locPos,Vector3.zero,Vector3.one);
    }
}
