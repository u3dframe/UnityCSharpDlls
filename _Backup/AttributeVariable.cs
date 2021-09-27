using Toolkits;

/// <summary>
/// 类名 : 自动加密，防止属性被内存修改
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-10 10:45
/// 功能 : 
/// </summary>
[System.Serializable]
public class AttributeVariable
{
	static System.Type _tpString = typeof(string);
	static System.Type _tpBool = typeof(bool);
	static System.Type _tpInt = typeof(int);
	static System.Type _tpLong = typeof(long);
	static System.Type _tpFloat = typeof(float);
	static System.Type _tpDouble = typeof(double);
	private object curVal;
	private string _curCode;
	
	public AttributeVariable(){
	}

    public AttributeVariable(object val){
        ReSet(val);
    }
	
	public void ReSet(object val)
	{
		if(val != null)
		{
			System.Type _curType = val.GetType();
			if(_curType != _tpBool && _curType != _tpDouble && _curType != _tpFloat && _curType != _tpInt && _curType != _tpLong && _curType != _tpString)
			{
				throw new System.Exception("=== 类型不支持 ==");
				// 判断下内置类型 bool,int,long,float,double,string
			}
		}
		this.curVal = val;
		this._curCode = MD5Ex.encrypt(val);
	}
	
	bool _isValide(){
		if(this.curVal == null || string.IsNullOrEmpty(this._curCode))
			return false;
		string _code = MD5Ex.encrypt(this.curVal);
		if(!this._curCode.Equals(_code))
			throw new System.Exception("=== memory is changed,内存被修改了 ==");
		return true;
	}

	public string GetString(){
		if(!_isValide())
			return "";
		if(this.curVal is string)
			return (string) this.curVal;
		return this.curVal.ToString();
	}
	
	public bool GetBool(){
		if(!_isValide())
			return false;
		if(this.curVal is bool)
			return (bool) this.curVal;
		return false;
	}

	public int GetInt(){
		if(!_isValide())
			return 0;
		if(this.curVal is int)
			return (int) this.curVal;
		return 0;
	}

	public long GetLong(){
		if(!_isValide())
			return 0;
		if(this.curVal is long)
			return (long) this.curVal;
		else if(this.curVal is int)
			return (int) this.curVal;
		return 0;
	}

	public float GetFloat(){
		if(!_isValide())
			return 0;
		if(this.curVal is float)
			return (float) this.curVal;
		else if(this.curVal is long)
			return (long) this.curVal;
		else if(this.curVal is int)
			return (int) this.curVal;
		return 0;
	}

	public double GetDouble(){
		if(!_isValide())
			return 0;
		if(this.curVal is double)
			return (double) this.curVal;
		else if(this.curVal is float)
			return (float) this.curVal;
		else if(this.curVal is long)
			return (long) this.curVal;
		else if(this.curVal is int)
			return (int) this.curVal;
		return 0;
	}
}
