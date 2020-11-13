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