using LitJson;

/// <summary>
/// 类名 :  LitJson 帮助脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-26 09:33
/// 功能 : 
/// </summary>
public static class LJsonHelper {
	static public JsonData NewJObj(){
		JsonData jd = new JsonData();
		jd.SetJsonType(JsonType.Object);
		return jd;
	}

	static public JsonData NewJArr(){
		JsonData jd = new JsonData();
		jd.SetJsonType(JsonType.Array);
		return jd;
	}

    static public T ToObject<T>(string json)
    {
        T _ret = default(T);
        if (string.IsNullOrEmpty(json))
            return _ret;

        try
        {
            _ret = JsonMapper.ToObject<T>(json);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogErrorFormat("====== obj error = [{0}] = [{1}]", ex, json);
        }
        return _ret;
    }

    static public string ToJson(object obj)
    {
        if (obj == null)
            return null;
        return JsonMapper.ToJson(obj);
    }

    static public JsonData ToJData(string json) {
        if (string.IsNullOrEmpty(json))
            return null;

        JsonData _ret = null;
        try {
            _ret = JsonMapper.ToObject(json);
        } catch(System.Exception ex){
            UnityEngine.Debug.LogErrorFormat("====== json error = [{0}] = [{1}]", ex, json);
        }
        return _ret;
    }

    static public JsonData ToJData(JsonData jdRoot,string key){
		if(jdRoot == null || string.IsNullOrEmpty(key))
			return null;
		
		if(!jdRoot.IsObject || !jdRoot.ContainsKey(key))
			return null;
		
		return jdRoot[key];
	}

	static public JsonData ToJData(JsonData jdRoot,int index){
		if(jdRoot == null || index < 0)
			return null;
		
		if(!jdRoot.IsArray)
			return null;
		
		int count = jdRoot.Count;
		if(count <= index)
			return null;

		return jdRoot[index];
	}

	static public string ToStrDef(JsonData jdRoot,string key,string def){
		JsonData jd = ToJData(jdRoot,key);
		if (jd != null) {
			return jd.ToString();
		}

		return def;
	}

	static public string ToStrDef(JsonData jdRoot,int index,string def){
		JsonData jd = ToJData(jdRoot,index);
		if (jd != null) {
			return jd.ToString();
		}

		return def;
	}

	static public string ToStr(JsonData jdRoot,string key){
		return ToStrDef(jdRoot,key,"");
	}

	static public string ToStr(JsonData jdRoot,int index){
		return ToStrDef(jdRoot,index,"");
	}
}