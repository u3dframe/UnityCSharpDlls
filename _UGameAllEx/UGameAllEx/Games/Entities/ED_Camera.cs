using UnityEngine;
using System;

/// <summary>
/// 类名 : Camera 数据脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-12-25 17:33
/// 功能 : 数据加载逻辑
/// </summary>
public class ED_Camera : ED_Animator
{
    static public new ED_Camera Builder(UnityEngine.Object uobj)
    {
        return Builder<ED_Camera>(uobj);
    }

    static public void GetUILocPos(Camera cmr,UnityEngine.Object src,Camera uiCmr,UnityEngine.Object uiParent,ref float posX,ref float posY)
    {
        if (null == cmr || null == uiCmr || uiCmr == cmr)
            return;
        Transform _trsf = UtilityHelper.ToTransform( src );
         if (null == _trsf)
            return;
        GameObject _uiDest = UtilityHelper.ToGObj( uiParent );
        Vector3 _pos = cmr.WorldToScreenPoint(_trsf.position);
        Vector2 _ret = UtilityHelper.ScreenPointToLocalPointInRectangleBy( _uiDest,uiCmr,_pos );
        posX = _ret.x;
        posY = _ret.y;
    }

    public Camera m_cmr { get; private set; }
    public float m_curFOV { get; private set; }

    // 跟随的平滑时间（类似于滞后时间）
    private float m_smoothFov = 0.3F;
    private float m_currVelocityFov = 0.0F;
    private float m_startFOV = 0;    
    private float m_toFOV = 0;
    private float m_lmtFOV = 0.01f;

    public ED_Camera() : base()
    {
    }

    override public void InitComp(string strComp, Action cfDestroy, Action cfShow, Action cfHide)
    {
        base.InitComp(strComp, cfDestroy, cfShow, cfHide);
    }

    override public void InitComp(Component comp, Action cfDestroy, Action cfShow, Action cfHide)
    {
        base.InitComp(comp, cfDestroy, cfShow, cfHide);
        this.m_cmr = this.m_comp as Camera;
        if (this.m_cmr != null)
        {
            this.m_startFOV = this.m_cmr.fieldOfView;
            this.m_curFOV = this.m_startFOV;
        }
    }

    override protected void On_Destroy(GobjLifeListener obj)
    {
        this.m_cmr = null;
        base.On_Destroy(obj);
    }

    override protected void _OnCurrUpdate()
    {
        if (this.m_cfUpdate != null)
        {
            float _difFOV = this.m_toFOV - this.m_curFOV; 
            if (_difFOV > m_lmtFOV * -1 && _difFOV < m_lmtFOV)
                this.m_cfUpdate -= _MoveFieldOfView;
            this.SetFieldOfView(this.m_curFOV);
        }

        base._OnCurrUpdate();
    }

    public void ToSmooth4Local(float toX, float toY, float toZ, float toFieldOfView,float smoothFov = 0f,float smoothPos = 0f, Action callFinish = null)
    {
        if (null == this.m_cmr)
        {
            this.ToSmoothPos(toX,toY,toZ,true,smoothPos,callFinish);
            return;
        }
        bool isChgPos = this.IsChgSmoothPos( toX,toY,toZ );
        bool isChgFOV = (this.m_toFOV != toFieldOfView);
        if(!isChgFOV && !isChgPos)
            return;
        
        this.m_isUpByLate = true;
        this.m_smoothFov = smoothFov;
        this.m_toFOV = toFieldOfView;
        this.m_curFOV = this.m_cmr.fieldOfView;
        float _difFOV = this.m_toFOV - this.m_curFOV;

        bool _isSmoonthFOV  = isChgFOV && (smoothFov > 0) && (_difFOV < m_lmtFOV * -1 || _difFOV > m_lmtFOV);
        bool _isSmoonth = this.IsSmoothPos(toX,toY,toZ,true,smoothPos,callFinish);
        if (_isSmoonth || _isSmoonthFOV)
        {
            if (this.m_curFOV != this.m_toFOV)
                this.m_cfUpdate += _MoveFieldOfView;
            this.StartCurrUpdate();
        }
        else
        {
            this.m_curFOV = this.m_toFOV;
            this.SetFieldOfView(this.m_toFOV);
            this.ExcuteCFUpdateEnd();
        }
    }

    private void _MoveFieldOfView()
    {
        this.m_curFOV = Mathf.SmoothDamp(this.m_curFOV, this.m_toFOV, ref m_currVelocityFov, m_smoothFov);
    }

    public void SetFieldOfView(float val)
    {
        if(UtilityHelper.IsNull(this.m_cmr))
            return;
        this.m_cmr.fieldOfView = val;
    }

    public void ToSmooth4LocXYZ(float val, float toFieldOfView, float smoothFov = 0f,float smoothPos = 0f, int xyz = 0, Action callFinish = null)
    {
        if(UtilityHelper.IsNull(this.m_gobj))
            return;
        Vector3 _to = this.GetCurrPos();
        float _x = _to.x;
        float _y = _to.y;
        float _z = _to.z;
        switch (xyz)
        {
            case 1:
                _y = val;
                break;
            case 2:
                _z = val;
                break;
            default:
                _x = val;
                break;
        }
        this.ToSmooth4Local(_x, _y, _z, toFieldOfView, smoothFov,smoothPos, callFinish);
    }

    public void ToSmooth4LocXYZStartAdd(float val, float toFieldOfView, float smoothFov = 0f,float smoothPos = 0f, int xyz = 0, Action callFinish = null)
    {
        if(UtilityHelper.IsNull(this.m_gobj))
            return;
        Vector3 _to = this.m_startLocPos;
        float _x = _to.x;
        float _y = _to.y;
        float _z = _to.z;
        switch (xyz)
        {
            case 1:
                _y += val;
                break;
            case 2:
                _z += val;
                break;
            default:
                _x += val;
                break;
        }
        this.ToSmooth4Local(_x, _y, _z, toFieldOfView, smoothFov,smoothPos, callFinish);
    }

    public void RebackStart(float smoothFov = 0f,float smoothPos = 0f, Action callFinish = null)
    {
        if(UtilityHelper.IsNull(this.m_gobj))
            return;
        Vector3 _to = this.m_startLocPos;
        this.ToSmooth4Local(_to.x, _to.y, _to.z, this.m_startFOV, smoothFov,smoothPos, callFinish);
    }

    public void GetUILocPos(UnityEngine.Object src,Camera uiCmr,UnityEngine.Object uiParent,ref float posX,ref float posY)
    {
        if(UtilityHelper.IsNull(this.m_gobj))
            return;
        GetUILocPos(this.m_cmr,src,uiCmr,uiParent,ref posX,ref posY);
    }
}
