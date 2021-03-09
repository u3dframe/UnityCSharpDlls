using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 类名 : Float 数据的扩展
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-10 10:45
/// 功能 : 
/// </summary>
[System.Serializable]
public class NumFloat
{
    public long val = 0;
    public long valMul = 1;
    float valF = 0.0f;
    double valD = 0.0;
	
	public NumFloat(){
	}

    public NumFloat(float val){
        ReInit(val);
    }

    public NumFloat(double val){
        ReInit(val);
    }

    public NumFloat(long val,long valMul){
        ReInit(val,valMul);
    }

    (long,long) _ToZSAndMultiple(float fVal){
		long val = 0;
		long mul = 1;
		string _v = fVal.ToString();
		if(_v.IndexOf(".") != -1){
			string[] _arrs = _v.Split('.');
			int _n = _arrs[1].Length;
			for (int i = 0; i < _n; i++)
			{
				mul *= 10;
			}
		}
		val = (int) (fVal * mul);
		return (val,mul);
	}

    (long,long) _ToZSAndMultiple(double dVal){
		long val = 0;
		long mul = 1;
		string _v = dVal.ToString();
		if(_v.IndexOf(".") != -1){
			string[] _arrs = _v.Split('.');
			int _n = _arrs[1].Length;
			for (int i = 0; i < _n; i++)
			{
				mul *= 10;
			}
		}
		val = (int) (dVal * mul);
		return (val,mul);
	}

    public void ReInit(float fVal){
        this.valF = fVal;
        this.valD = fVal;
        (this.val,this.valMul) = _ToZSAndMultiple(fVal);
    }

    public void ReInit(double dVal){
        this.valD = dVal;
        this.valF = (float)(dVal);
        (this.val,this.valMul) = _ToZSAndMultiple(dVal);
    }

    public void ReInit(long val,long valMul){
        this.val = val;
        this.valMul = (valMul <= 0) ? 1 : valMul;
        this.valF = valFloat;
        this.valD = valDouble;
    }

    public float valFloat{
        get{ return ((float) this.val) / this.valMul; }
    }

    public double valDouble{
        get{ return ((double) this.val) / this.valMul; }
    }
}
