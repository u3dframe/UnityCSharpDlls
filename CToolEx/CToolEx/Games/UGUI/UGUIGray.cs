using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Core.Kernel;

/// <summary>
/// 类名 : UGUIGray 置灰
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2019-07-30 13:27
/// 功能 : 控制 UI 灰显示
/// </summary>
[AddComponentMenu("UI/UGUIGray")]
public class UGUIGray : GobjLifeListener {
	// 取得对象
	static public new UGUIGray Get(UnityEngine.Object uobj, bool isAdd){
		return GHelper.Get<UGUIGray>(uobj, isAdd);
	}

	static public new UGUIGray Get(UnityEngine.Object uobj)
    {
		return Get(uobj, true);
	}
	
	static Material _matGray;
	static Material matGray{
		get{
			if (_matGray == null)
            {
				Shader shader = UGameFile.SFindShader("Custom/ui_default_multifunctional");
				if(shader != null){
					_matGray = new Material(shader);
					_matGray.SetInt("_IsGray",1);
                	// _matGray.EnableKeyword("UI_GRAY_ON");
				}
			}
			return _matGray;
		}
	}

	override protected void OnCall4Awake()
    {
		this.csAlias = "U_Gray";
		_matGray = matGray;
    }
	
	override protected void OnCall4Start(){
		Init ();
		m_isGray = _m_isGray_2; // 避免没显示(没走生命周期的对象)调用了错误
	}

	override protected void OnCall4Destroy(){
		if (m_isNew && _m_matGrap)
			Destroy (_m_matGrap);
	}
	
	override protected void OnClear(){
		isInit = false;
		m_lExcludeNames.Clear();
		m_imgs = null;
		m_txts = null;
		m_txtCols = null;
		m_isGrayTxt = false;
		m_isNew = false;
		_m_matGrap = null;
	}
	
	bool isInit = false;

	// 渲染的 Image 对象集合
	Image[] m_imgs;

	// 渲染的 Text 对象集合
	public bool m_isGrayTxt{get;set;}
	Text[] m_txts;
	Color[] m_txtCols;
	
	// 排除 - 除去
	List<string> m_lExcludeNames = new List<string>();

	bool _m_isClearExceptNames;
	public bool m_isClearExceptNames{
		get{ return _m_isClearExceptNames; }
		set{
			_m_isClearExceptNames = value;
			if (_m_isClearExceptNames) {
				m_lExcludeNames.Clear ();
			}
		}
	}

	bool m_isNew = false;

	Material _m_matGrap;
	Material m_matGrap{
		get{
			if (_m_matGrap == null) {
				if (m_isNew) {
					_m_matGrap = new Material (matGray.shader);
					_m_matGrap.SetInt("_IsGray",1);
					// _m_matGrap.EnableKeyword("UI_GRAY_ON");
				} else {
					_m_matGrap = matGray;
				}
			} else{
				if (m_isNew && _m_matGrap == matGray) {
					_m_matGrap = new Material (matGray.shader);
					_m_matGrap.SetInt("_IsGray",1);
					// _m_matGrap.EnableKeyword("UI_GRAY_ON");
				}
			}
			return _m_matGrap;
		}
	}


	bool _m_isGray = false;
	bool _m_isGray_2 = false;
	public bool m_isGray {
		get
		{
			return _m_isGray;
		}

		set{
			_m_isGray_2 = value;
			if (isInit) {
				if (_m_isGray != _m_isGray_2)
				{
					_m_isGray = _m_isGray_2;
					if (_m_isGray)
					{
						Gray();
					}
					else
					{
						CancelGray();
					}
				}
			}
		}
	}

	bool _m_isRaycastTarget = false;
	public bool m_isRaycastTarget {
		get
		{
			return _m_isRaycastTarget;
		}

		set{
			_m_isRaycastTarget = value;
			if (_m_isRaycastTarget) {
				RaycastTarget ();
			} else {
				CancelRaycastTarget ();
			}
		}
	}

	void Init(){
		if (isInit)
			return;
		isInit = true;

		m_imgs = gameObject.GetComponentsInChildren<Image> (true);

		m_txts = gameObject.GetComponentsInChildren<Text> (true);
		if (m_txts != null && m_txts.Length > 0) {
			m_txtCols = new Color[m_txts.Length];
			int lens = m_txts.Length;
			for (int i = 0; i < lens; i++) {
				m_txtCols[i] = m_txts [i].color;
			}
		}
	}

	void GrayImg(bool isGray){
		if (m_imgs == null || m_imgs.Length <= 0)
			return;

		int lens = m_imgs.Length;
		MaskableGraphic graphic;
		for (int i = 0; i < lens; i++) {
			graphic = m_imgs [i];

			if (isContains (graphic.name))
				continue;
			
			graphic.material = isGray ? m_matGrap : null;
		}
	}

	void RaycastTargetImg(bool isRaycastTarget){
		if (m_imgs == null || m_imgs.Length <= 0)
			return;

		int lens = m_imgs.Length;
		MaskableGraphic graphic;
		for (int i = 0; i < lens; i++) {
			graphic = m_imgs [i];
			graphic.raycastTarget = isRaycastTarget;
		}
	}

	void GrayTxt(bool isGray){
		if (m_txts == null || m_txts.Length <= 0)
			return;
		
		if (!m_isGrayTxt)
			return;
		
		int lens = m_txts.Length;
		Text graphic;
		for (int i = 0; i < lens; i++) {
			graphic = m_txts [i];

			if (isContains (graphic.name))
				continue;
			
			graphic.color = isGray ? Color.gray : m_txtCols[i];
		}
	}

	void RaycastTargetTxt(bool isRaycastTarget){
		if (m_txts == null || m_txts.Length <= 0)
			return;

		int lens = m_txts.Length;
		MaskableGraphic graphic;
		for (int i = 0; i < lens; i++) {
			graphic = m_txts [i];
			graphic.raycastTarget = isRaycastTarget;
		}
	}

	[ContextMenu("Gray")]
	void Gray(){
		Init ();

		GrayImg (true);
		GrayTxt (true);
	}

	[ContextMenu("CancelGray")]
	void CancelGray(){
		GrayImg (false);
		GrayTxt (false);
	}

	void RaycastTarget(){
		Init ();
		
		RaycastTargetImg (true);
		RaycastTargetTxt (true);
	}

	void CancelRaycastTarget(){
		RaycastTargetImg (false);
		RaycastTargetTxt (false);
	}

	bool isContains(string name){
		int lens = m_lExcludeNames.Count;
		if (lens <= 0)
			return false;
		
		for (int i = 0; i < lens; i++) {
			if (m_lExcludeNames [i].Equals (name)) {
				return true;
			}
		}

		return false;
	}

	public void AddExceptName(string excludeName){
		if (isContains (excludeName))
			return;
		m_lExcludeNames.Add (excludeName);
	}

	public void IsGrayAll(bool isGray,bool isGrayTxt){
		this.m_isGrayTxt = isGrayTxt;
		this.m_isGray = isGray;
	}
}