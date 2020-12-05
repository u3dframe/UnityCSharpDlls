using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 类名 : 曲线 Curve 扩展脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-08-22 22:17
/// 功能 : 
/// </summary>
public class CurveEx : CurveBase
{
    static public new CurveEx GetInChild(GameObject gobj)
    {
        if (gobj)
        {
            CurveEx ret = gobj.GetComponent<CurveEx>();
            if (!ret)
            {
                ret = gobj.GetComponentInChildren<CurveEx>();
            }
            return ret;
        }
        return null;
    }

#if UNITY_EDITOR
    bool isRunning = false;
    void Update() {
        if(!isRunning)
            return;
        
        this.ReVal(Time.time,1);
    }

    [ContextMenu("Running")]
    void ED_Running(){
        this.isRunning = true;
    }
#endif
}
