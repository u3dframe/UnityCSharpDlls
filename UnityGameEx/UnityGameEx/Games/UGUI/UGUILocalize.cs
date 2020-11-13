using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 类名 : UGUILocalize
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-27 16:37
/// 功能 : UGUI的文本本地化
/// </summary>
[ExecuteInEditMode]
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
			if(fmtPars == null || fmtPars.Length <= 0) _sval = Localization.Get(m_key);
			else _sval = Localization.FormatMore(m_key,fmtPars);
		}
		_SetTextVal(_sval);
	}

	void _SetTextVal(string val){
		this.m_isChg = false;
		_sval = (val == null) ? (m_key == null ? "" : m_key) : val;
		// Debug.LogErrorFormat("===[{0}] = [{1}]",m_key,_sval);
		m_text.text = _sval;
	}

	public void SetText(string key){
		this.m_isChg = !string.Equals(key,this.m_key);
		this.m_key = key;
		this.m_isUseLocalize = true;
		OnLocalize();
	}

	public void SetText(int key){
		SetText(key.ToString());
	}

	public void SetUText(string val){
		if(!m_text)
			return;
		this.m_isUseLocalize = false;
		val =  (val == null) ? "" : val;
		_SetTextVal(val);
	}

	bool IsInPars(object v,params object[] pars){
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

	public void FormatMore(string key,params object[] pars){
		this.m_isChg = !string.Equals(key,this.m_key);
		this.m_key = key;
		this.m_isChg = this.m_isChg || IsChangePars(pars);
		this.fmtPars = pars;
		this.m_isUseLocalize = true;
		OnLocalize();
	}

	public void Format(string key,object obj1){
		FormatMore(key,obj1);
	}

	public void Format(string key,object obj1,object obj2){
		FormatMore(key,obj1,obj2);
	}

	public void Format(string key,object obj1,object obj2,object obj3){
		FormatMore(key,obj1,obj2,obj3);
	}

	public void Format(string key,object obj1,object obj2,object obj3,object obj4){
		FormatMore(key,obj1,obj2,obj3,obj4);
	}

	public void Format(string key,object obj1,object obj2,object obj3,object obj4,object obj5){
		FormatMore(key,obj1,obj2,obj3,obj4,obj5);
	}

	public void Format(string key,object obj1,object obj2,object obj3,object obj4,object obj5,object obj6){
		FormatMore(key,obj1,obj2,obj3,obj4,obj5,obj6);
	}

	public void FormatMore(int key,params object[] pars){
		FormatMore(key.ToString(),pars);
	}

	public void Format(int key,object obj1){
		FormatMore(key,obj1);
	}

	public void Format(int key,object obj1,object obj2){
		FormatMore(key,obj1,obj2);
	}

	public void Format(int key,object obj1,object obj2,object obj3){
		FormatMore(key,obj1,obj2,obj3);
	}

	public void Format(int key,object obj1,object obj2,object obj3,object obj4){
		FormatMore(key,obj1,obj2,obj3,obj4);
	}

	public void Format(int key,object obj1,object obj2,object obj3,object obj4,object obj5){
		FormatMore(key,obj1,obj2,obj3,obj4,obj5);
	}

	public void Format(int key,object obj1,object obj2,object obj3,object obj4,object obj5,object obj6){
		FormatMore(key,obj1,obj2,obj3,obj4,obj5,obj6);
	}

	public Color GetColor(){
		if(m_text) return m_text.color;
		return Color.white;
	}

	public void SetColor(Color color){
		if(m_text != null) m_text.color = color;
	}
}