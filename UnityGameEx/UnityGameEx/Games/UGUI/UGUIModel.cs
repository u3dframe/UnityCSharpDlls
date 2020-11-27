using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 类名 : UGUI界面展示模型
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-08-04 00:10
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
    public Camera m_camera { get; private set; }
    [SerializeField] RenderTexture m_rtTarget;
    [SerializeField] bool m_isUseRT = true;
    [SerializeField] bool m_isUseRTFmt = false;
    [SerializeField] int m_rtWidth = 1024;
    [SerializeField] int m_rtHeight = 1024;
    [SerializeField][Range(0, 256)] int m_rtdepth = 0;
    [SerializeField][Range(1, 16)] int m_rtAntiAliasing = 2;
    RawImage _imgRaw = null;
    public RawImage m_imgRaw { get; private set; }
    [SerializeField] Material m_rawMat = null;

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
        this._imgRaw = this.m_trsf.GetComponentInChildren<RawImage>(true);
        this.m_camera = this.m_trsf.GetComponentInChildren<Camera>(true);

        if (this.m_camera)
        {
            this.m_sfwer = SmoothFollower.Get(this.m_camera.gameObject);
            this.m_sfwer.height = 1.5f;
            this.m_sfwer.distance = 4;
            this.m_sfwer.lookAtHeight = 1.1f;
            this.m_sfwer.target = this.m_wrap;
            this.m_sfwer.isUpByLate = true;
            this.m_sfwer.isRunning = true;
        }
        this._ReRtex(this.m_rtWidth, this.m_rtHeight);
        this.ReRawImg(_imgRaw);
    }

    protected override void OnCall4Destroy()
    {
        base.OnCall4Destroy();
        this._DestroyRt();
        Material _mat = this.m_rawMat;
        this.m_rawMat = null;
        if (_mat)
            GameObject.DestroyImmediate(_mat);
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
                this.m_rtTarget = new RenderTexture(this.m_rtWidth, this.m_rtHeight,this.m_rtdepth, rtFmt);
            }
            else
            {
                this.m_rtTarget = new RenderTexture(this.m_rtWidth, this.m_rtHeight, this.m_rtdepth);
            }
            
            this.m_rtTarget.antiAliasing = m_rtAntiAliasing;
        }

        if (this.m_camera)
        {
            this.m_camera.targetTexture = this.m_rtTarget;

            if(this.m_camera.clearFlags == CameraClearFlags.Depth)
                this.m_camera.clearFlags = CameraClearFlags.Depth;
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
            this.m_rtTarget = null;
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

            RectTransform _rtrsf = this.m_imgRaw.rectTransform;
            Vector2 _size = _rtrsf.sizeDelta;
            _size.x = this.m_rtWidth;
            _size.y = this.m_rtHeight;
            _rtrsf.sizeDelta = _size;
            this.m_imgRaw.material = m_rawMat;
        }
    }

    public void ReSet(int rtWidth,int rtHeight,bool isUseRt)
    {
        this.m_isUseRT = isUseRt;
        if (!isUseRt)
            this._DestroyRt();

        this._ReRtex(rtWidth, rtHeight);
        this.ReRawImg(this.m_imgRaw);
    }

    public void EnableKeyword(string key)
    {
        if (this.m_rawMat)
        {
            this.m_rawMat.EnableKeyword(key);
        }
    }
}