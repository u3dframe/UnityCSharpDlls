using LitJson;
using Core.Kernel;
using Core.Kernel.Beans;
/// <summary>
/// 类名 : 统计时间的脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2021-01-06 20:03
/// 功能 : 
/// </summary>
public class ED_RecordTime : ED_Basic
{
    static ListDict<ED_RecordTime> _dic = new ListDict<ED_RecordTime>(false);
    static protected ED_RecordTime Builder(string tag)
    {
        ED_RecordTime _ret = GetCache<ED_RecordTime>();
        if(_ret == null){
            _ret = new ED_RecordTime();
        }
        _ret.Init(tag);
        return _ret;
    }

    static public ED_RecordTime GetOrNew(string tag)
    {
        if(_dic.ContainsKey(tag))
            return _dic.Get(tag);
        return Builder(tag);
    }
    
    static public ED_RecordTime objProgress{
        get{
            return GetOrNew("objProgress");
        }
    }
    
    public ED_RecordTime() : base()
    {
    }

    private string m_tag = null;
    private JsonData jroot = null;
    private JsonData jList = null;
    static System.DateTime d1970 = new System.DateTime(1970,1,1);
	static System.DateTime d1970_8;

    protected void Init(string tag){
		if(d1970_8 == null){
			d1970_8 = d1970.ToUniversalTime().AddHours(8);
		}
        if(jroot == null)
            jroot = LJsonHelper.NewJObj();
        if(jList == null)
            jList = LJsonHelper.NewJArr();

        jList.Clear();
        jroot.Clear();
        this.m_tag = tag;
        jroot["tag"] = tag;
        jroot["msg_list"] = jList;
        
        ED_RecordTime _old = _dic.Remove4Get(tag);
        if(_old != null){
            _old.ReBack();
        }
        _dic.Add(tag,this);
    }

    protected void ReBack(){
        jList.Clear();
        jroot.Clear();
        AddCache(this);
    }
    

    public ED_RecordTime AddMsg(object msg){
        if(msg != null){
            JsonData _cur = LJsonHelper.NewJObj();
            _cur["msg"] = new JsonData(msg);
            var dt8 = System.DateTime.UtcNow.AddHours(8);
            var _diff = dt8 - d1970_8;
            _cur["ms"] = (long)(_diff.TotalMilliseconds);
            _cur["time"] = dt8.ToString("yyyy-MM-dd HH:mm:ss");
            jList.Add(_cur);
        }
        return this;
    }

    public void EndMsg(object msg,bool isReBack = true){
        this.AddMsg(msg);
        string _json = this.jroot.ToJson();
        if(isReBack)
            this.ReBack();
        
        var dt8 = System.DateTime.UtcNow.AddHours(8);
        string _fp = string.Format("{0}../{1}_{2}.txt",UGameFile.m_dirRes,this.m_tag,dt8.ToString("MMddHHmmss"));
        UGameFile.WriteText(_fp,_json,true);
    }

    static public void SyDiffMS(string name){
        string _fp = string.Format("{0}../{1}.txt", UGameFile.m_dirRes,name);
        string _json = UGameFile.GetText4File(_fp);
        var jd = LJsonHelper.ToJData(_json);
        var jarr = jd["msg_list"];
        JsonData jIt = null,jItLast = null,jItFirst = null;
        int lens = jarr.Count;
        JsonData jNew = LJsonHelper.NewJArr();
        for (int i = 0; i < lens; i++)
        {
            if(jIt != null)
                jItLast = jIt;
            jIt = jarr[i];
            if(jItFirst == null)
                jItFirst = jIt;
            if(jItLast != null)
            {
                var jNew2 = LJsonHelper.NewJObj();
                jNew2["A"] = jItLast["msg"].ToJson();
                jNew2["B"] = jIt["msg"].ToJson();
                long _diffMs = (long)jIt["ms"] - (long)jItLast["ms"];
                jNew2["(B - A) = ms"] = _diffMs;
                jNew2["_diffMs"] = _diffMs;
                jNew.Add(jNew2);
            }
        }

        var jNew3 = LJsonHelper.NewJObj();
        jNew3["A"] = jItFirst["msg"].ToJson();
        jNew3["B"] = jIt["msg"].ToJson();
        long _diffMs1 = (long)jIt["ms"] - (long)jItFirst["ms"];
        jNew3["(B - A) = ms"] = _diffMs1;
        jNew3["_diffMs"] = _diffMs1;
        jNew3["_diffSec"] = _diffMs1 / 1000;
        jNew.Add(jNew3);

        var dt8 = System.DateTime.UtcNow.AddHours(8);
        _fp = string.Format("{0}../_diff_ms_{1}.txt",UGameFile.m_dirRes,dt8.ToString("MMddHHmmss"));
        _json = jNew.ToJson();
        UGameFile.WriteText(_fp,_json,true);
    }
}
