using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 类名 : UGUI界面展示模型
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-07-15 09:10
/// 功能 : 
/// </summary>
public class UGUIModel : PrefabBasic {
	static public new UGUIModel Get(GameObject gobj,bool isAdd){
		return UtilityHelper.Get<UGUIModel>(gobj,true);
	}

	static public new UGUIModel Get(GameObject gobj){
		return Get(gobj,true);
	}

    public SmoothFollower m_sfwer { get; private set; }
    public Transform m_wrap { get; private set; }
    public GameObject m_node { get; private set; }
    public bool m_isAutoScale = true; // 是否对象自动缩放
    public float m_childScaleTo1 { get; private set; }
    public float m_wrapLocalScale { get; private set; }
    public Camera m_camera { get; private set; }
    [SerializeField] RenderTexture _rtTarget;
    public RenderTexture m_rtTarget { get { return _rtTarget; } }
    [SerializeField] bool m_isUseRT = true;
    [SerializeField] bool m_isUseRTFmt = false;
    [SerializeField] int m_rtWidth = 1024;
    [SerializeField] int m_rtHeight = 1024;
    [SerializeField][Range(0, 8)] int m_rtdepth = 1;
    [SerializeField][Range(1, 16)] int m_rtAntiAliasing = 1;
    RawImage _imgRaw = null;
    public RawImage m_imgRaw { get; private set; }
    public float m_rawWidth { get; private set; }
    public float m_rawHeight { get; private set; }
    [SerializeField] Material _rawMat = null;
    public Material m_rawMat { get { return _rawMat; } } // EnableKeyword
    public string m_layerModel = "ModelUI";
    float sfwerD = 3.9f, sfwerH = 1.78f, sfwerL = 0.86f;

    protected override void OnCall4Awake()
    {
        base.OnCall4Awake();
        ReBindNodes();
    }

    protected override void ReBindNodes()
    {
        _arrs_nodes = new string[]{
            "Camera","ModelWrap"
        };
        base.ReBindNodes();
    }
    
    protected override void OnCall4Start()
    {
        base.OnCall4Start();
        this.m_node = GetGobjElement("ModelNode");
        this.m_wrap = GetTrsfElement("ModelWrap");
        float _nodeScale = 1;
        if (this.m_node)
            _nodeScale = this.m_node.transform.lossyScale.x;
        this.m_childScaleTo1 = 1 / _nodeScale;
        this._imgRaw = this.m_trsf.GetComponentInChildren<RawImage>(true);
        this.m_camera = this.m_trsf.GetComponentInChildren<Camera>(true);

        if (this.m_camera)
        {
            this.m_sfwer = SmoothFollower.Get(this.m_camera.gameObject);
            this.m_sfwer.target = this.m_wrap;
            this.m_sfwer.isUpByLate = true;
            this.m_sfwer.isRunning = true;
            this.ReSfwer(this.sfwerD,this.sfwerH,this.sfwerL);
        }
        this._ReRtex(this.m_rtWidth, this.m_rtHeight);
        this.ReRawImg(_imgRaw);
        this.SetModelLocalScale(this.m_wrapLocalScale);
    }

    protected override void OnCall4Destroy()
    {
        base.OnCall4Destroy();
        this._DestroyRt();
    }

    void _ReRtex(int w, int h)
    {
        if(this.m_rtWidth != w || this.m_rtHeight != h)
        {
            this.m_rtWidth = w;
            this.m_rtHeight = h;
            this._DestroyRt();
        }
        
        if (this.m_isUseRT && this.m_rtTarget == null)
        {
            if (m_isUseRTFmt)
            {
                RenderTextureFormat rtFmt = RenderTextureFormat.Default;
                if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB565))
                    rtFmt = RenderTextureFormat.RGB565;
                else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB4444))
                    rtFmt = RenderTextureFormat.ARGB4444;
                else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32))
                    rtFmt = RenderTextureFormat.ARGB32;
                else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
                    rtFmt = RenderTextureFormat.ARGBHalf;
                else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RG16))
                    rtFmt = RenderTextureFormat.RG16;
                this._rtTarget = new RenderTexture(this.m_rtWidth, this.m_rtHeight,this.m_rtdepth, rtFmt);
            }
            else
            {
                this._rtTarget = new RenderTexture(this.m_rtWidth, this.m_rtHeight, this.m_rtdepth);
            }
            this.m_rtTarget.antiAliasing = m_rtAntiAliasing;
            // this.ReMatMainTexture();
        }

        if (this.m_camera)
        {
            this.m_camera.targetTexture = this.m_rtTarget;
            // this.m_camera.clearFlags = CameraClearFlags.Depth;
        }
    }

    void _DestroyRt(bool isDestroy = true)
    {
        if (this._imgRaw != null)
            this._imgRaw.texture = null;

        if (this.m_imgRaw != null && this.m_imgRaw != this._imgRaw)
            this.m_imgRaw.texture = null;

        RenderTexture _cur = this.m_rtTarget;
        if (this.m_camera)
            this.m_camera.targetTexture = null;

        if (_cur != null && RenderTexture.active == _cur)
            RenderTexture.active = null;

        if (isDestroy && _cur)
        {
            this._rtTarget = null;
            GameObject.DestroyImmediate(_cur);
        }
    }

    public void ReRawImg(RawImage raw)
    {
        if (this._imgRaw != null)
        {
            this._imgRaw.enabled = false;
            this._imgRaw.texture = null;
        }
        
        this.m_imgRaw = raw;
        if (this.m_imgRaw)
        {
            this.m_imgRaw.texture = this.m_rtTarget;
            this.m_imgRaw.enabled = this.m_isUseRT;
            this.m_imgRaw.material = m_rawMat;
            this.ReRawWH(this.m_rawWidth, this.m_rawHeight);
        }
    }

    public void ReSet(int rtWidth,int rtHeight,bool isUseRt = true)
    {
        this.m_isUseRT = isUseRt;
        if (!isUseRt)
            this._DestroyRt();

        this._ReRtex(rtWidth, rtHeight);
        this.ReRawImg(this.m_imgRaw);
    }

    public void ReMatMainTexture()
    {
        if (this.m_rawMat)
        {
            if (this.m_rawMat.HasProperty("_MainTex"))
                this.m_rawMat.SetTexture("_MainTex", this.m_rtTarget);
            else
                this.m_rawMat.mainTexture = this.m_rtTarget;
        }
    }
    
    public void ReSetModelLayer(string modelLayer)
    {
        if (!string.IsNullOrEmpty(modelLayer))
            this.m_layerModel = modelLayer;
        UtilityHelper.SetLayerAll(this.m_node, this.m_layerModel);
    }

    public void ReSfwer(float distance,float height = 1.78f,float lookAtHeight = 0.86f)
    {
        if (distance == 0)
            return;

        this.sfwerD = distance;
        this.sfwerH = height;
        this.sfwerL = lookAtHeight;
        if (!this.m_sfwer)
            return;

        this.m_sfwer.distance = distance;
        this.m_sfwer.height = height;
        this.m_sfwer.lookAtHeight = lookAtHeight;

        float _abs = Mathf.Abs(distance);
        float scale = 1;
        if (this.m_wrap != null)
        {
           scale = this.m_wrap.lossyScale.x;
            if (this.m_isAutoScale)
            {
                float angle = this.m_camera.fieldOfView * 0.5f;
                float radian = Mathf.Deg2Rad * angle;
                scale = Mathf.Tan(radian) * distance;
            }
        }

        if (this.m_camera)
            this.m_camera.farClipPlane =  Mathf.Ceil(_abs + scale * 0.5f + 0.5f);

        if (this.m_isAutoScale)
        {
            scale = Mathf.Floor(scale * this.m_childScaleTo1 * 0.78f);
            this.SetModelLocalScale(scale);
        }
    }
    
    public void ReRawWH(float w,float h)
    {
        w = (w <= 0) ? this.m_rtWidth : w;
        h = (h <= 0) ? this.m_rtHeight : h;
        this.m_rawWidth = w;
        this.m_rawHeight = h;

        if (!this.m_imgRaw)
            return;

        RectTransform _rtrsf = this.m_imgRaw.rectTransform;
        Vector2 _size = _rtrsf.sizeDelta;
        _size.x = w;
        _size.y = h;
        _rtrsf.sizeDelta = _size;
    }

    void SetModelLocalScale(float locScale)
    {
        if (!this.m_wrap || locScale <= 0)
            return;
        this.m_wrapLocalScale = locScale;
        this.m_wrap.localScale = new Vector3(locScale, locScale, locScale);
    }

    public void ReModelLocalScale(float locScale)
    {
        this.m_isAutoScale = false;
        this.m_wrapLocalScale = locScale;
        this.SetModelLocalScale(locScale);
    }
}