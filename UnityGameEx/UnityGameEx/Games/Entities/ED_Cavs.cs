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

    class CavInfo
    {
        public UGUICanvasAdaptive m_rootCvs { get; private set; }
        public Canvas m_cvs { get; private set; }
        public bool m_isAuto { get; private set; }
        public int m_sortOrder { get; private set; }
        public int m_curSortOrder { get; private set; }

        public CavInfo(Canvas cav, UGUICanvasAdaptive root)
        {
            this.m_cvs = cav;
            this.m_rootCvs = root;
            if(root != null)
                cav.sortingLayerID = root.m_sortingLayerID;
            this.m_sortOrder = cav.sortingOrder;
            this.m_curSortOrder = this.m_sortOrder;
        }

        virtual public void Dispose()
        {
            if (this.m_isAuto)
                this.AutoSortOrder(true);

            this.m_rootCvs = null;
            this.m_cvs = null;
            this.m_sortOrder = 0;
            this.m_curSortOrder = 0;
            this.m_isAuto = false;
        }

        public bool IsEmpty()
        {
            return this.m_cvs == null || this.m_rootCvs == null;
        }

        public bool IsEmptyOrSameRoot()
        {
            return this.IsEmpty() || this.m_cvs == this.m_rootCvs.m_cvs;
        }
        
        public void AutoSortOrder(bool isBack = false)
        {
            if (this.IsEmptyOrSameRoot())
                return;
            this.m_isAuto = !isBack;
            int _symbol = isBack ? -1 : 1;
            int _root = this.m_rootCvs.m_curSortOrder;
            int _sort = _root + (this.m_sortOrder * _symbol);
            this.m_rootCvs.m_curSortOrder = _sort;
            this.m_curSortOrder = _sort;
            this.m_cvs.sortingOrder = _sort;
        }

        public void SetSortOrder(int sortOrder)
        {
            if (this.IsEmpty())
                return;

            if (this.m_isAuto)
                this.AutoSortOrder(true);

            this.m_curSortOrder = sortOrder;
            this.m_cvs.sortingOrder = sortOrder;
        }
    }
	
	public UGUICanvasAdaptive m_rootCvs { get; private set; }
    CavInfo[] m_cvsCurrs = null;
    
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
        this.m_rootCvs = UGUICanvasAdaptive.GetInParent(this.m_gobj);
        Canvas[] _arrs = this.m_gobj.GetComponentsInChildren<Canvas>(true);
        this.m_cvsCurrs = null;
        int _lens = UtilityHelper.LensArrs(_arrs);
        if (_lens > 0)
		{
            this.m_cvsCurrs = new CavInfo[_lens];
            Canvas _cvs = null;
            for (int i = 0; i < _lens; i++)
            {
                _cvs = _arrs[i];
                this.m_cvsCurrs[i] = new CavInfo(_cvs, this.m_rootCvs);
            }
        }
        this.AutoSortOrder();
    }

    override public void ClearComp()
    {
        CavInfo[] _arrs = this.m_cvsCurrs;
        this.m_rootCvs = null;
        this.m_cvsCurrs = null;

        int _lens = UtilityHelper.LensArrs(_arrs);
        if (_lens > 0)
        {
            CavInfo _info = null;
            for (int i = 0; i < _lens; i++)
            {
                _info = _arrs[i];
                if (_info != null)
                    _info.Dispose();
            }
        }
        base.ClearComp();
    }

    public void AutoSortOrder(bool isBack = false)
    {
        int _lens = UtilityHelper.LensArrs(this.m_cvsCurrs);
        if (_lens <= 0)
            return;

        CavInfo _info = null;
        for (int i = 0; i < _lens; i++)
        {
            _info = this.m_cvsCurrs[i];
            if (_info != null)
                _info.AutoSortOrder(isBack);
        }
    }

    public void SetSortOrder(int sortOrder)
    {
        int _lens = UtilityHelper.LensArrs(this.m_cvsCurrs);
        if (_lens <= 0)
            return;

        CavInfo _info = null;
        for (int i = 0; i < _lens; i++)
        {
            _info = this.m_cvsCurrs[i];
            if (_info != null)
                _info.SetSortOrder(sortOrder);
        }
    }
}
