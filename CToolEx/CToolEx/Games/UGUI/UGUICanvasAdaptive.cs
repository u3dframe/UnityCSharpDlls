using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 类名 : UGUI Canvas 自适应
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-09-08 11:10
/// 功能 : 720 x 1280 ; 1080 x 1920 ; 1440 x 2560
/// </summary>
[RequireComponent(typeof(CanvasScaler),typeof(Canvas))]
public class UGUICanvasAdaptive : MonoBehaviour {

	/// <summary>
	/// 初始宽度
	/// </summary>
	[SerializeField]
	float m_standard_width = 1920f;

	/// <summary>
	/// 初始高度
	/// </summary>
	[SerializeField]
	float m_standard_height = 1080f;

	/// <summary>
	/// 当前设备宽度
	/// </summary>
	[SerializeField]
	float device_width = 0f;

	/// <summary>
	/// 当前设备高度
	/// </summary>
	[SerializeField]
	float device_height = 0f;

	/// <summary>
	/// UI画布的宽度
	/// </summary>
	[SerializeField]
	float cav_width = 0f;

	/// <summary>
	/// UI画布的高度
	/// </summary>
	[SerializeField]
	float cav_height = 0f;

	/// <summary>
	/// 是否权重缩放
	/// </summary>
	[SerializeField]
	bool m_isWeightZoom = false;

	/// <summary>
	/// 屏幕矫正比例
	/// </summary>
	float adjustor = 0f;

	CanvasScaler m_canvasScaler;
	RectTransform m_rtrsfSelf;

	[SerializeField]
	float _scaleHeight;
	[SerializeField]
	float _scaleWidth;
	[SerializeField]
	float _curScale;
	[SerializeField]
	float _ndScale;

	public float scaleHeight {get{ return _scaleHeight;} }
	public float scaleWidth {get{ return _scaleWidth;} }
	public float standardWidth{ get { return m_standard_width; } }
	public float standardHeight{ get { return m_standard_height; } }
	public float cavWidth{ get { return cav_width; } }
	public float cavHeight{ get { return cav_height; } }
	public float curScale{ get{return _curScale;}}
	public float needScale{get{return _ndScale;}}

    public Canvas m_cvs { get; private set; }
    public int m_sortingLayerID { get; private set; }
    public int m_sortOrder { get; private set; }
    public int m_curSortOrder { get; set; }

    void Awake(){
        this.m_cvs = this.gameObject.GetComponent<Canvas>();
        this.m_sortingLayerID = this.m_cvs.sortingLayerID;
        this.m_sortOrder = this.m_cvs.sortingOrder;
        this.m_curSortOrder = this.m_sortOrder;

        m_rtrsfSelf = transform as RectTransform;
		m_canvasScaler = gameObject.GetComponent<CanvasScaler> ();
		m_canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		m_canvasScaler.referenceResolution = new Vector2 (m_standard_width, m_standard_height);
		m_canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
		ReAdaptive ();
	}

	void ReAdaptive() {
		if (device_width != Screen.width && device_height != Screen.height) {
			device_width = Screen.width;
			device_height = Screen.height;

			float _zoomX = device_width / m_standard_width;
			float _zoomY = device_height / m_standard_height;
			// 宽高比
			float _standard_aspect = m_standard_width / m_standard_height;
			float _device_aspect = device_width / device_height;
			float _scale = _device_aspect / _standard_aspect;

			if (m_isWeightZoom) {
				if (_zoomX <= _zoomY) {
					adjustor = 1 - _scale;
				}
				adjustor = Mathf.Max (adjustor, 0);
				adjustor = Mathf.Min (adjustor, 1);
			} else {
				adjustor = (_zoomX <= _zoomY) ? 0 : 1;
			}

			m_canvasScaler.matchWidthOrHeight = adjustor;

			_scaleHeight = adjustor == 1 ? 1 : 1 / _scale;
			_scaleWidth = adjustor == 0 ? 1 : _scale;

			ReCalcCurrScale ();

			ReCavSize ();
		}
	}

	void ReCalcCurrScale(){
		float dh = _scaleHeight - 1;
		float dw = _scaleWidth - 1;
		float absh = Mathf.Abs (dh);
		float absw = Mathf.Abs (dw);
		if (absh >= absw) {
			_curScale = _scaleHeight;
		} else {
			_curScale = _scaleWidth;
		}
		_ndScale = 1 / _curScale;
	}

	void ReCavSize(){
		CancelInvoke ("_ReCavSize");
		Invoke("_ReCavSize",0.1f);
	}

	void _ReCavSize(){
		cav_width = m_rtrsfSelf.sizeDelta.x;
		cav_height = m_rtrsfSelf.sizeDelta.y;
	}

	void Update () {
		ReAdaptive ();
	}

	static public UGUICanvasAdaptive GetInParent(GameObject gobj){
		if(gobj){
			UGUICanvasAdaptive ret = gobj.GetComponent<UGUICanvasAdaptive> ();
			if (!ret) {
				ret = gobj.GetComponentInParent<UGUICanvasAdaptive> ();
			}
			return ret;
		}
		return null;
	}
}
