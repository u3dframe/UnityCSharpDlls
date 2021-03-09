using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 类名 : Float4 类型扩展
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-10 10:55
/// 功能 : 
/// </summary>
[System.Serializable]
public class NumFloat4
{
	public NumFloat x = new NumFloat(0f); // r
	public NumFloat y = new NumFloat(0f); // g
	public NumFloat z = new NumFloat(0f); // b
	public NumFloat w = new NumFloat(0f); // a
	
	public NumFloat r { get{ return x; } }
	public NumFloat g { get{ return y; } }
	public NumFloat b { get{ return z; } }
	public NumFloat a { get{ return w; } }
	
	public NumFloat4(){
	}
	
	public NumFloat4(float v1,float v2,float v3,float v4){
        ReInit(v1,v2,v3,v4);
    }

    public NumFloat4(double v1,double v2,double v3,double v4){
        ReInit(v1,v2,v3,v4);
    }
	
	public void ReInit(float v1,float v2,float v3,float v4){
		x.ReInit(v1);
		y.ReInit(v2);
		z.ReInit(v3);
		w.ReInit(v4);
    }

    public void ReInit(double v1,double v2,double v3,double v4){
        x.ReInit(v1);
		y.ReInit(v2);
		z.ReInit(v3);
		w.ReInit(v4);
    }
}
