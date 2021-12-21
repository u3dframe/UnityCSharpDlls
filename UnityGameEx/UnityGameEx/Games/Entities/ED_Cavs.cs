using UnityEngine;
using System;
/// <summary>
/// 类名 : UGUI Canvas 数据脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2021-01-05 15:23
/// 功能 : 
/// </summary>
public class ED_Cavs : Core.Kernel.Beans.ED_Comp
{
    static public new ED_Cavs Builder(UnityEngine.Object uobj)
    {
        return Builder<ED_Cavs>(uobj);
    }
	
    CanvasEx[] m_cvsCurrs = null;
    public int m_nLens { get; private set; }
    
    public ED_Cavs() : base()
    {
    }

    override public void InitComp(string strComp, Action cfDestroy, Action cfShow, Action cfHide)
    {
        base.InitComp(strComp, cfDestroy, cfShow, cfHide);
    }

    override public void InitComp(Component comp, Action cfDestroy, Action cfShow, Action cfHide)
    {
        base.InitComp(comp, cfDestroy, cfShow, cfHide);
        Canvas[] _arrs = this.m_gobj.GetComponentsInChildren<Canvas>(true);
        int _lens = UtilityHelper.LensArrs(_arrs);
        this.m_nLens = _lens;
        this.m_cvsCurrs = null;
        if (this.m_nLens > 0)
		{
            this.m_cvsCurrs = new CanvasEx[this.m_nLens];
            Canvas _cvs = null;
            for (int i = 0; i < this.m_nLens; i++)
            {
                _cvs = _arrs[i];
                this.m_cvsCurrs[i] = CanvasEx.Get(_cvs.gameObject);
            }
        }
        this.ReInit(-1);
    }

    override protected void On_Destroy(GobjLifeListener obj)
    {
        this.m_cvsCurrs = null;
        base.On_Destroy( obj );
    }

    public void ReInit(int valBase)
    {
        if (this.m_cvsCurrs == null)
            return;
        CanvasEx _citem = null;
        for (int i = 0; i < this.m_nLens; i++)
        {
            _citem = this.m_cvsCurrs[i];
            if (_citem == null)
                continue;
            _citem.ReInit(valBase);
        }
    }

    public void ReBaseOrder(int valBase,bool isForce)
    {
        if (this.m_cvsCurrs == null)
            return;
        CanvasEx _citem = null;
        int _curVal = 0;
        for (int i = 0; i < this.m_nLens; i++)
        {
            _citem = this.m_cvsCurrs[i];
            if (_citem == null)
                continue;
            _curVal = valBase < 0 ? _citem.m_orderBase : valBase;
            isForce = isForce || valBase <= 0;
            _citem.ReBaseOrder(_curVal, isForce);
        }
    }
}
