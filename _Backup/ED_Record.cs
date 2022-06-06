using LitJson;
using UnityEngine;
using System.Collections.Generic;
using SFile = System.IO.File;

/// <summary>
/// 类名 : 统计时间的脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2021-01-06 20:03
/// 功能 : 
/// </summary>
public class ED_Record
{
    static Dictionary<string,ED_Record> _dic = new Dictionary<string,ED_Record>(4);
    static protected ED_Record Builder(string tag)
    {
        ED_Record _ret = new ED_Record();
        _ret.Init(tag);
        return _ret;
    }

    static public ED_Record GetOrNew(string tag)
    {
        ED_Record _ret = null;
        if(_dic.TryGetValue(tag,out _ret))
            return _ret;
        return Builder(tag);
    }
    
    static public ED_Record objProgress{
        get{
            return GetOrNew("objProgress");
        }
    }

    static public ED_Record luaUsed{
        get{
            return GetOrNew("luaUsed");
        }
    }

    static public ED_Record luaNoUse{
        get{
            return GetOrNew("luaNoUse");
        }
    }
    
    public ED_Record() 
    {
    }

    private string m_tag = null;
    private JsonData jroot = null;
    private JsonData jList = null;
    static System.DateTime d1970 = new System.DateTime(1970,1,1);
	static System.DateTime d1970_8;

    static JsonData NewJObj(){
        var njobj = new JsonData();
        njobj.SetJsonType(JsonType.Object);
        return njobj;
    }

    static JsonData NewJArr(){
        var njobj = new JsonData();
        njobj.SetJsonType(JsonType.Array);
        return njobj;
    }

    protected void Init(string tag){
		if(d1970_8 == null){
			d1970_8 = d1970.ToUniversalTime().AddHours(8);
		}
        if(jroot == null){
            jroot = NewJObj();
        }
        if(jList == null)
        {
            jList = NewJArr();
        }

        jList.Clear();
        jroot.Clear();
        this.m_tag = tag;
        jroot["tag"] = tag;
        jroot["msg_list"] = jList;
        
        ED_Record _old = null;
        if(_dic.TryGetValue(tag,out _old))
        {
            _old.ReBack();
        }
        _dic.Add(tag,this);
    }

    protected void ReBack(){
        jList.Clear();
        jroot.Clear();
    }
    
    public ED_Record AddMsg(string msg){
        if(string.IsNullOrEmpty(msg)){
            return this;
        }
        JsonData _cur = NewJObj();
        _cur["msg"] = msg;
        var dt8 = System.DateTime.UtcNow.AddHours(8);
        var _diff = dt8 - d1970_8;
        _cur["ms"] = (long)(_diff.TotalMilliseconds);
        _cur["time"] = dt8.ToString("yyyy-MM-dd HH:mm:ss");
        jList.Add(_cur);
        return this;
    }

    public ED_Record AddMsg(long msg){
        JsonData _cur = NewJObj();
        _cur["msg"] = msg;
        var dt8 = System.DateTime.UtcNow.AddHours(8);
        var _diff = dt8 - d1970_8;
        _cur["ms"] = (long)(_diff.TotalMilliseconds);
        _cur["time"] = dt8.ToString("yyyy-MM-dd HH:mm:ss");
        jList.Add(_cur);
        return this;
    }

    public ED_Record AddMsg(object msg){
        if(msg == null){
            return this;
        }
        if(msg is string){
            return AddMsg((string)msg);
        }else if(msg is long){
            return AddMsg((long)msg);
        }

        JsonData _cur = NewJObj();
        string json = JsonMapper.ToJson(msg);
        _cur["msg"] = JsonMapper.ToObject(json);
        var dt8 = System.DateTime.UtcNow.AddHours(8);
        var _diff = dt8 - d1970_8;
        _cur["ms"] = (long)(_diff.TotalMilliseconds);
        _cur["time"] = dt8.ToString("yyyy-MM-dd HH:mm:ss");
        jList.Add(_cur);
        return this;
    }

    public void EndMsg(object msg,bool isReBack = true){
        this.AddMsg(msg);
        string _json = this.jroot.ToJson();
        if(isReBack)
            this.ReBack();
        
        var dt8 = System.DateTime.UtcNow.AddHours(8);
        string _dir = Application.dataPath + "/";
        string _fp = string.Format("{0}../../{1}_{2}.txt",_dir,this.m_tag,dt8.ToString("MMddHHmmss"));
        SFile.WriteAllText(_fp,_json);
        Debug.LogError(_fp);
    }

    static public void SyDiffMS(string name){
        string _dir = Application.dataPath + "/";
        string _fp = string.Format("{0}../../{1}.txt", _dir,name);
        string _json = SFile.ReadAllText( _fp );
        var jd = JsonMapper.ToObject(_json);
        var jarr = jd["msg_list"];
        JsonData jIt = null,jItLast = null,jItFirst = null;
        int lens = jarr.Count;
        JsonData jNew = NewJArr();
        for (int i = 0; i < lens; i++)
        {
            if(jIt != null)
                jItLast = jIt;
            jIt = jarr[i];
            if(jItFirst == null)
                jItFirst = jIt;
            if(jItLast != null)
            {
                var jNew2 = NewJObj();
                jNew2["A"] = jItLast["msg"].ToJson();
                jNew2["B"] = jIt["msg"].ToJson();
                long _diffMs = (long)jIt["ms"] - (long)jItLast["ms"];
                jNew2["(B - A) = ms"] = _diffMs;
                jNew2["_diffMs"] = _diffMs;
                jNew.Add(jNew2);
            }
        }

        var jNew3 = NewJObj();
        jNew3["A"] = jItFirst["msg"].ToJson();
        jNew3["B"] = jIt["msg"].ToJson();
        long _diffMs1 = (long)jIt["ms"] - (long)jItFirst["ms"];
        jNew3["(B - A) = ms"] = _diffMs1;
        jNew3["_diffMs"] = _diffMs1;
        jNew3["_diffSec"] = _diffMs1 / 1000;
        jNew.Add(jNew3);

        var dt8 = System.DateTime.UtcNow.AddHours(8);
        _fp = string.Format("{0}../../_diff_ms_{1}.txt",_dir,dt8.ToString("MMddHHmmss"));
        _json = jNew.ToJson();
        SFile.WriteAllText(_fp,_json);
		Debug.LogError(_fp);
    }
}
