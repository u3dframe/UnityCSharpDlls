using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 类名 : UGUILocalize
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-27 16:37
/// 功能 : UGUI的文本本地化
/// </summary>
// [ExecuteInEditMode]
[RequireComponent(typeof(Text))]
[AddComponentMenu("UI/UGUILocalize")]
public class UGUILocalize : GobjLifeListener {
	// 取得对象
	static public new UGUILocalize Get(GameObject gobj,bool isAdd){
		return UtilityHelper.Get<UGUILocalize>(gobj,isAdd);
	}

	static public new UGUILocalize Get(GameObject gobj){
		return Get(gobj,true);
	}

	public string m_tagName{ get; private set; }
	public bool m_isCsv{ get; private set; }
	public string m_key = "";
	public Text m_text;
	bool m_isInit = false;
	bool m_isUseLocalize = true;
	bool m_isChg = false;
	object[] fmtPars = null;
	string _sval = "";
	string _language = "";
	
	public string m_textVal{
		get{
			if(m_text != null && !m_text.text.Equals(_sval))
				_sval = m_text.text;
			return _sval;
		}
	}

	override protected void OnCall4Awake()
	{
		Init();
		this.csAlias = "U_TLOC";
	}

	override protected void OnCall4Hide()
	{
		Localization.onLocalize -= _OnLocalize;
	}	

	override protected void OnCall4Show()
	{
		Init ();
		OnLocalize();
		Localization.onLocalize += _OnLocalize;
	}

	override protected void OnCall4Destroy()
	{
		Localization.onLocalize -= _OnLocalize;
	}

	void Init()
	{
		if(m_isInit)
			return;
		
		_language = Localization.language;
		m_isInit = true;
		m_isChg = !string.IsNullOrEmpty(m_key);
		m_text = gameObject.GetComponent<Text>();
	}

	void _OnLocalize(){
		this.m_isChg = !string.Equals(_language,Localization.language);
		_language = Localization.language;
		OnLocalize();
	}

	void OnLocalize()
	{
		if(!m_isChg || !m_isUseLocalize || !m_text) return;

		if(string.IsNullOrEmpty(m_key)){
			_sval = "";
		}else{
			if(fmtPars == null || fmtPars.Length <= 0) _sval = Localization.Get(this.m_key,this.m_tagName,this.m_isCsv);
			else _sval = Localization.FormatMoreStr(this.m_tagName,this.m_isCsv,this.m_key,fmtPars);
		}
		_SetTextVal(_sval);
	}

	void _SetTextVal(string val){
		this.m_isChg = false;
		_sval = (val == null) ? (m_key == null ? "" : m_key) : val;
		// Debug.LogErrorFormat("===[{0}] = [{1}]",m_key,_sval);
		m_text.text = _sval;
	}

	public void SetText(object key){
		string _key_ = key.ToString();
		if(string.IsNullOrEmpty(_key_))
			_key_ = "1";
		this.m_isChg = this.m_isChg || !string.Equals(_key_,this.m_key);
		this.m_key = _key_;
		this.m_isUseLocalize = true;
		OnLocalize();
	}

	public void SetUText(string val){
		if(!m_text)
			return;
		this.m_isUseLocalize = false;
		val =  (val == null) ? "" : val;
		_SetTextVal(val);
	}

	bool IsInPars(object v,params object[] pars){
		if(v == null)
			return true;
		int _l1 = pars.Length;
		for(int i = 0;i < _l1;i++){
			if(v == pars[i])
				return true;
		}
		return false;
	}

	bool IsChangePars(params object[] pars){
		if(pars == null && this.fmtPars == null) return false;
		if(pars == null && this.fmtPars != null) return true;
		if(pars != null && this.fmtPars == null) return true;
		int _l1 = pars.Length;
		int _l2 = this.fmtPars.Length;
		if(_l1 != _l2) return true;
		for(int i = 0;i < _l2;i++){
			if(!IsInPars(this.fmtPars[i],pars))
				return true;
		}
		return false;
	}

	void FormatMore(string key,params object[] pars){
		if(string.IsNullOrEmpty(key))
			key = "1";
		this.m_isChg = !string.Equals(key,this.m_key);
		this.m_key = key;
		this.m_isChg = this.m_isChg || IsChangePars(pars);
		this.fmtPars = pars;
		this.m_isUseLocalize = true;
		OnLocalize();
	}

	public Color GetColor(){
		if(m_text) return m_text.color;
		return Color.white;
	}

	public void SetColor(Color color){
		if(m_text != null) m_text.color = color;
	}

	public void Format(object key,object obj1,object obj2 = null,object obj3 = null,object obj4 = null,object obj5 = null,object obj6 = null)
	{
		if(key == null)
			key = "1";
		List<object> list = UtilityHelper.ToList(obj1,obj2,obj3,obj4,obj5,obj6);
		if(list == null || list.Count <= 0)
		{
			this.SetText(key.ToString());
			return;
		}
		this.FormatMore(key.ToString(),list.ToArray());
	}

	public void SetOrFormat(string tag,bool isCsv,object key,object obj1 = null,object obj2 = null,object obj3 = null,object obj4 = null,object obj5 = null,object obj6 = null)
	{
		bool isChg = !string.Equals(tag,this.m_tagName) || this.m_isCsv != isCsv;
		this.m_isChg = this.m_isChg || isChg;
		this.m_tagName = tag;
		this.m_isCsv = isCsv;
		this.Format(key,obj1,obj2,obj3,obj4,obj5,obj6);
	}
}