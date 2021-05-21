using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using LitJson;
using Core;
using Core.Kernel;

/// <summary>
/// 类名 : 场景Map的前期加载
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-03-21 10:37
/// 功能 : 
/// </summary>
public class SceneMapEx : GobjLifeListener
{
    enum MSM_UpState
    {
        None = 0,
        InitSLInfo,
        CheckSLInfo,
        CheckSLRefl,
        CheckSLProbes,
        ReLMap,
        Finish,
    }

    class SLInfo
    {
        private string m_abName = null;
        private string[] m_assets = new string[3];
        private int m_loaded = 0;
        protected DF_ToLoadTex2D m_cfLoad = null;
        public SceneLightMapData m_curData = new SceneLightMapData();
        public bool m_isDone { get { return m_loaded >= 3; } }

        /// <summary>
        ///  ntype [0 = lightmapColor ， 1 = lightmapDir ，2 = shadowMask]
        /// </summary>
        public SLInfo(string abName, string lColor, string lDir, string lmask, DF_ToLoadTex2D cfLoad = null)
        {
            this.m_abName = abName;
            this.m_cfLoad = cfLoad;
            this.m_curData = new SceneLightMapData();
            if (!string.IsNullOrEmpty(lColor))
                lColor = UGameFile.ReSEnd(lColor,UGameFile.m_suffix_light);
            this.m_assets[0] = lColor;
            if (!string.IsNullOrEmpty(lDir))
                lDir = UGameFile.ReSEnd(lDir, UGameFile.m_suffix_png);
            this.m_assets[1] = lDir;
            if (!string.IsNullOrEmpty(lmask))
                lmask = UGameFile.ReSEnd(lmask, UGameFile.m_suffix_png);
            this.m_assets[2] = lmask;

            this.DoLoading();
        }

        int _GetIndex(string val)
        {
            if (string.IsNullOrEmpty(val))
                return -1;

            string _it = null;
            for (int i = 0; i < this.m_assets.Length; i++)
            {
                _it = this.m_assets[i];
                if (string.IsNullOrEmpty(_it))
                    continue;
                if (_it.Contains(val))
                    return i;
            }
            return -1;
        }

        public void DoLoading()
        {
            string _assetName = null;
            for (int i = 0; i < this.m_assets.Length; i++)
            {
                _assetName = this.m_assets[i];
                if (string.IsNullOrEmpty(_assetName))
                {
                    this.m_loaded++;
                    continue;
                }

                if (this.m_cfLoad != null)
                    this.m_cfLoad(this.m_abName, _assetName, this._OnLoadTexture);
                else
                    AssetInfo.abMgr.LoadAsset<Texture2D>(this.m_abName, _assetName, this._OnLoadAsset);
            }
        }

        void _OnLoadAsset(AssetBase asset)
        {
            AssetInfo ainfo = asset as AssetInfo;
            if (ainfo == null)
                return;

            Texture2D tex = ainfo.GetObject<Texture2D>();
            this.LoadedTex(tex);
        }

        void _OnLoadTexture(Texture2D tex)
        {
            this.LoadedTex(tex);
        }

        private void LoadedTex(Texture2D tex)
        {
            this.m_loaded++;
            if (tex == null)
                return;

            int _index = _GetIndex(tex.name);
            // Debug.LogErrorFormat("==== lt = [{0}] = [{1}]", _index, tex.name);
            switch (_index)
            {
                case 0:
                    this.m_curData.lightmapColor = tex;
                    break;
                case 1:
                    this.m_curData.lightmapDir = tex;
                    break;
                case 2:
                    this.m_curData.shadowMask = tex;
                    break;
            }
        }

        public void Clear()
        {
            var _tmp = this.m_curData;
            this.m_curData = null;
            if (_tmp != null)
                _tmp.Clear();

            for (int i = 0; i < this.m_assets.Length; i++)
            {
                if (string.IsNullOrEmpty(this.m_assets[i]))
                    continue;
                AssetInfo.abMgr.UnLoadAsset(this.m_abName);
            }
        }
    }

    class SLInfoBase
    {
        public string m_abName { get; protected set; }
        public string m_asset { get; protected set; }
        public bool m_isDone { get; protected set; }

        public SLInfoBase(string abName, string assetName)
        {
            this.m_abName = abName;
            this.m_asset = assetName;
        }

        protected void _OnLoadAsset(AssetBase asset)
        {
            this.m_isDone = true;
            AssetInfo ainfo = asset as AssetInfo;
            if (ainfo == null)
                return;
            this._OnLoadAssetInfo(ainfo);
        }

        protected virtual void _OnLoadAssetInfo(AssetInfo asset)
        {
        }

        public virtual void Clear()
        {
            AssetInfo.abMgr.UnLoadAsset(this.m_abName);
        }
    }

    class SLInfoReflection : SLInfoBase
    {
        protected DF_ToLoadCube m_cfLoad = null;
        
        public Cubemap m_obj { get; private set; }

        public SLInfoReflection(string abName, string refl, DF_ToLoadCube cfLoad = null):base(abName,refl)
        {
            this.m_cfLoad = cfLoad;
            this.m_asset = UGameFile.ReSEnd(refl, UGameFile.m_suffix_light);
            this.DoLoading();
        }

        public void DoLoading()
        {
            string _assetName = this.m_asset;
            if (this.m_cfLoad != null)
                this.m_cfLoad(this.m_abName, _assetName, this._OnLoadCubemap);
            else
                AssetInfo.abMgr.LoadAsset<Cubemap>(this.m_abName, _assetName, this._OnLoadAsset);
        }

        override protected void _OnLoadAssetInfo(AssetInfo ainfo)
        {
            if (ainfo == null)
                return;

            Cubemap cube = ainfo.GetObject<Cubemap>();
            this.LoadedObj(cube);
        }

        void _OnLoadCubemap(Cubemap cube)
        {
            this.LoadedObj(cube);
        }

        private void LoadedObj(Cubemap cube)
        {
            this.m_isDone = true;
            this.m_obj = cube;
        }

        override public void Clear()
        {
            this.m_obj = null;
            base.Clear();
        }
    }

    class SLInfoProbes : SLInfoBase
    {
        protected DF_ToLoadLProbes m_cfLoad = null;

        public LightProbes m_obj { get; private set; }

        public SLInfoProbes(string abName, string assetName, DF_ToLoadLProbes cfLoad = null) : base(abName, assetName)
        {
            this.m_cfLoad = cfLoad;
            this.m_asset = UGameFile.ReSEnd(this.m_asset, UGameFile.m_suffix_scriptable);
            this.DoLoading();
        }

        public void DoLoading()
        {
            string _assetName = this.m_asset;
            if (this.m_cfLoad != null)
                this.m_cfLoad(this.m_abName, _assetName, this._OnLoadLightProbes);
            else
                AssetInfo.abMgr.LoadAsset<Cubemap>(this.m_abName, _assetName, this._OnLoadAsset);
        }

        override protected void _OnLoadAssetInfo(AssetInfo ainfo)
        {
            if (ainfo == null)
                return;

            LightProbes cube = ainfo.GetObject<LightProbes>();
            this.LoadedObj(cube);
        }

        void _OnLoadLightProbes(LightProbes cube)
        {
            this.LoadedObj(cube);
        }

        private void LoadedObj(LightProbes cube)
        {
            this.m_isDone = true;
            this.m_obj = cube;
        }

        override public void Clear()
        {
            this.m_obj = null;
            base.Clear();
        }
    }

    static public string ReFname(string fname) {
        if (string.IsNullOrEmpty(fname)) return "";
        if (!fname.StartsWith("maps/")) fname = "maps/" + fname;
        if (!fname.EndsWith(".minfo")) fname += ".minfo";
        return fname;
    }

    static public void ClearLightMap()
    {
        LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
        LightmapSettings.lightmaps = new LightmapData[0];
    }
	
    static public int m_cursor_map { get; private set; }
    static DF_ToLoadTex2D _cfLoad = null;
    static DF_ToLoadCube _cfLoadCube = null;
    static DF_ToLoadLProbes _cfLoadProbes = null;
    static Dictionary<string, SceneMapEx> m_caches = new Dictionary<string, SceneMapEx>();

    static public void ReStatic(DF_ToLoadTex2D toLoadTex, DF_ToLoadCube toLoadCube, DF_ToLoadLProbes toLoadProbes)
    {
        _cfLoad = toLoadTex;
        _cfLoadCube = toLoadCube;
        _cfLoadProbes = toLoadProbes;
    }

    static public SceneMapEx GetMSM(string map_key)
    {
        SceneMapEx _cs = null;
        m_caches.TryGetValue(map_key, out _cs);
        return _cs;
    }

    static public SceneMapEx LoadMapData(string map_key)
    {
        if (string.IsNullOrEmpty(map_key))
            return null;

        SceneMapEx _cs = GetMSM(map_key);
        if (_cs != null)
            return _cs;

        string _fname = ReFname(map_key);
        string _cfg_val = UGameFile.curInstance.GetDecryptText(_fname);
        if (string.IsNullOrEmpty(_cfg_val))
            return null;

        JsonData jdRoot = LJsonHelper.ToJData(_cfg_val);
        if (jdRoot == null)
            return null;

        m_cursor_map++;
        string _fmv = string.Format("_map_{0}_{1}", map_key, m_cursor_map);
        GameObject _gobj = new GameObject(_fmv);
        _cs = Get(_gobj, true);
        m_caches.Add(map_key, _cs);

        _cs.Init(map_key, jdRoot);
        _cs.DoLoad();
        return _cs;
    }

    static public new SceneMapEx Get(UnityEngine.Object uobj, bool isAdd)
    {
        return UtilityHelper.Get<SceneMapEx>(uobj, isAdd);
    }

    static public new SceneMapEx Get(UnityEngine.Object uobj)
    {
        return Get(uobj, false);
    }

    MSM_UpState m_u_state = MSM_UpState.None;
    int m_lightmapsMode = 0;
    List<SLInfo> listSLInfos = new List<SLInfo>();
    protected JsonData m_mapJdRefl { get; private set; }
    protected bool m_isReflProbe { get; private set; }
    ListDict<SLInfoReflection> listRefls = new ListDict<SLInfoReflection>(true);
    SLInfoProbes m_lProbes = null;
    protected string m_map_key { get; private set; }
    public JsonData m_mapJdRoot { get; private set; }
    public bool m_isDoned { get { return this.m_u_state == MSM_UpState.Finish; } }

    override protected void OnCall4Destroy()
    {
        this.m_u_state = MSM_UpState.None;
        m_caches.Remove(this.m_map_key);
        this.m_map_key = null;
		
        this.StopUpdate();
        this._ClearLMap();
        this._ClearJson();
		
        base.OnCall4Destroy();
    }

    override public void OnUpdate(float dt, float unscaledDt)
    {
        base.OnUpdate(dt, unscaledDt);
        switch (m_u_state)
        {
            case MSM_UpState.InitSLInfo:
                this._ST_InitSLInfo();
                break;
            case MSM_UpState.CheckSLInfo:
                this._ST_CheckSLInfo();
                break;
            case MSM_UpState.CheckSLRefl:
                this._ST_CheckSLRefl();
                break;
            case MSM_UpState.CheckSLProbes:
                this._ST_CheckSLProbes();
                break;
            case MSM_UpState.ReLMap:
                this._ST_ReLightmap();
                break;
            default:
                break;
        }
    }

    protected void Init(string map_key,JsonData root)
    {
        this.m_map_key = map_key;
        this.m_mapJdRoot = root;
    }

    public void DoLoad()
    {
        if(this.m_u_state == MSM_UpState.None)
            this.m_u_state = MSM_UpState.InitSLInfo;
        this.StartUpdate();
    }

    void _ST_InitSLInfo()
    {
        JsonData jdLm = LJsonHelper.ToJData(this.m_mapJdRoot,"info_lms");
        if (jdLm == null)
        {
            this.m_u_state = MSM_UpState.ReLMap;
            return;
        }

        this.m_lightmapsMode = (int)jdLm["lightmapsMode"];
        JsonData jdLmds = LJsonHelper.ToJData(jdLm, "lmDatas");
        if (jdLmds == null || jdLmds.Count <= 0)
        {
            this.m_u_state = MSM_UpState.ReLMap;
            return;
        }

        string _fp = jdLm["fp_lm"].ToString();
        string _fp_ab = _fp + UGameFile.m_strLightmap;
        int _nLens = jdLmds.Count;
        JsonData _jd;
        string _c,_d,_s;
        SLInfo _loadData_;
        for (int i = 0; i < _nLens; i++)
        {
            _jd = jdLmds[i];
            _c = LJsonHelper.ToStr(_jd, "lightmapColor");
            _d = LJsonHelper.ToStr(_jd, "lightmapDir");
            _s = LJsonHelper.ToStr(_jd, "shadowMask");
            _loadData_ = new SLInfo(_fp_ab, _c, _d, _s,_cfLoad);
            this.listSLInfos.Add(_loadData_);
        }

        _c = LJsonHelper.ToStr(jdLm, "lprobes");
        if(!string.IsNullOrEmpty(_c))
        {
            this.m_lProbes = new SLInfoProbes(_fp_ab, _c, _cfLoadProbes);
        }

        JsonData _jdRefls = LJsonHelper.ToJData(jdLm, "reflections");
        if(_jdRefls != null)
        {
            this.m_mapJdRefl = _jdRefls;
            JsonData jdList = LJsonHelper.ToJData(_jdRefls, "list");
            int _nLens2 = jdList.Count;
            string _refl;
            SLInfoReflection _load_;
            for (int i = 0; i < _nLens2; i++)
            {
                _refl = LJsonHelper.ToStr(jdList, i);
                if (string.IsNullOrEmpty(_refl))
                    continue;
                _load_ = this.GetSLRefl(_refl);
                if(_load_ == null)
                {
                    _load_ = new SLInfoReflection(_fp_ab, _refl, _cfLoadCube);
                    this.listRefls.Add(_refl, _load_);
                }
            }
        }

        if (_nLens > 0)
            this.m_u_state = MSM_UpState.CheckSLInfo;
        else
            this.m_u_state = MSM_UpState.CheckSLRefl;
    }

    void _ST_CheckSLInfo()
    {
        SLInfo _it;
        int _nd = 0;
        int _count = this.listSLInfos.Count;
        for (int i = 0; i < _count; i++)
        {
            _it = this.listSLInfos[i];
            if (_it == null || _it.m_isDone)
                _nd++;
        }
        if(_nd >= _count)
            this.m_u_state = MSM_UpState.CheckSLRefl;
    }

    void _ST_CheckSLRefl()
    {
        SLInfoReflection _it;
        int _nd = 0;
        int _count = this.listRefls.m_dic.Count;
        for (int i = 0; i < _count; i++)
        {
            _it = this.listRefls.m_list[i];
            if (_it == null || _it.m_isDone)
                _nd++;
        }
        if (_nd >= _count)
            this.m_u_state = MSM_UpState.CheckSLProbes;
    }

    void _ST_CheckSLProbes()
    {
        if (this.m_lProbes == null || this.m_lProbes.m_isDone )
            this.m_u_state = MSM_UpState.ReLMap;
    }

    void _ST_ReLightmap()
    {
        this.m_u_state = MSM_UpState.Finish;
        // this._ReLightmap();
    }

    void _ReLightmap()
    {
        List<LightmapData> _list = new List<LightmapData>();
        if (this.listSLInfos.Count > 0)
        {
            SceneLightMapData _item;
            for (int i = 0; i < this.listSLInfos.Count; i++)
            {
                _item = this.listSLInfos[i].m_curData;
                _list.Add(_item.ToLightmapData());
            }
        }
        LightmapSettings.lightmapsMode = (LightmapsMode)this.m_lightmapsMode;
        LightmapSettings.lightmaps = _list.ToArray();
    }

    SLInfoReflection GetSLRefl(string key)
    {
        return this.listRefls.Get(key);
    }

    public void ReLightmap()
    {
        if (!this.m_isDoned)
            return;
        this._ReLightmap();
    }

    void _ClearSLInfo()
    {
        SLInfo _it;
        int _count = this.listSLInfos.Count;
        for (int i = 0; i < _count; i++)
        {
            _it = this.listSLInfos[i];
            if (_it != null)
                _it.Clear();
        }
		this.listSLInfos.Clear();

        this._ClearSLRefls();

        if (this.m_lProbes != null)
            this.m_lProbes.Clear();
        this.m_lProbes = null;
    }

    void _ClearSLRefls()
    {
        SLInfoReflection _it;
        int _count = this.listRefls.m_dic.Count;
        for (int i = 0; i < _count; i++)
        {
            _it = this.listRefls.m_list[i];
            if (_it != null)
                _it.Clear();
        }
        this.listRefls.Clear();
    }

    [ContextMenu("Clear Lightmap")]
    void _ClearLMap()
    {
        this._ClearEnvironment();
        this._ClearSLInfo();
        this._ClearProbes();
        this._ReLightmap();
    }
	
	void _ClearJson()
    {
        if (this.m_mapJdRefl != null)
            this.m_mapJdRefl.Clear();
        this.m_mapJdRefl = null;

        if (this.m_mapJdRoot != null)
            this.m_mapJdRoot.Clear();
        this.m_mapJdRoot = null;
    }

    void _ClearEnvironment()
    {
        RenderSettings.customReflection = null;
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
    }

    public void ReReflEnv()
    {
        if (!this.m_isDoned)
            return;

        this._ClearEnvironment();
        
        if (this.m_mapJdRefl == null)
            return;

        string _rev = LJsonHelper.ToStr(this.m_mapJdRefl, "environment");
        if(!string.IsNullOrEmpty(_rev))
        {
            SLInfoReflection _srefl = this.GetSLRefl(_rev);
            JsonData _jd = LJsonHelper.ToJData(this.m_mapJdRefl, _rev);
            if(_srefl != null && _jd != null)
            {
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
                RenderSettings.defaultReflectionResolution = LJsonHelper.ToInt(_jd, "defaultReflectionResolution");
                RenderSettings.reflectionBounces = LJsonHelper.ToInt(_jd, "reflectionBounces");
                RenderSettings.reflectionIntensity = LJsonHelper.ToFloat(_jd, "reflectionIntensity");
                RenderSettings.customReflection = _srefl.m_obj;
            }
        }
    }

    public void ReReflProbe(Object uobj)
    {
        if (!this.m_isDoned || this.m_isReflProbe)
            return;

        if (this.m_mapJdRefl == null || UtilityHelper.IsNull(uobj))
            return;

        this.m_isReflProbe = true;
        string _rev = LJsonHelper.ToStr(this.m_mapJdRefl, "environment");
        JsonData jdList = LJsonHelper.ToJData(this.m_mapJdRefl, "list");
        JsonData jdTemp = null;
        int _nLens = jdList.Count;
        string _refl = null;

        GameObject _gobj_ = null;
        SLInfoReflection _srefl = null;
        System.Type _tp = typeof(ReflectionProbe);
        ReflectionProbe _rprobe = null;
        float _v1 = 0, _v2 = 0, _v3 = 0;
        for (int i = 0; i < _nLens; i++)
        {
            _refl = LJsonHelper.ToStr(jdList, i);
            if (string.IsNullOrEmpty(_refl) || _rev.Equals(_refl))
                continue;

            _srefl = this.GetSLRefl(_refl);
            if (_srefl == null || _srefl.m_obj == null)
                continue;

            jdTemp = LJsonHelper.ToJData(this.m_mapJdRefl, _refl);
            if (jdTemp == null)
                continue;
            _gobj_ = new GameObject(_refl, _tp);
            _rprobe = _gobj_.GetComponent<ReflectionProbe>();
            UtilityHelper.SetParentSyncLayer(_gobj_, uobj, true);
            _v1 = LJsonHelper.ToFloat(jdTemp, "pos_x");
            _v2 = LJsonHelper.ToFloat(jdTemp, "pos_y");
            _v3 = LJsonHelper.ToFloat(jdTemp, "pos_z");
            _gobj_.transform.position = new Vector3(_v1,_v2,_v3);

            _rprobe.importance = LJsonHelper.ToInt(jdTemp, "importance");
            _rprobe.intensity = LJsonHelper.ToFloat(jdTemp, "intensity");
            _v1 = LJsonHelper.ToFloat(jdTemp, "center_x");
            _v2 = LJsonHelper.ToFloat(jdTemp, "center_y");
            _v3 = LJsonHelper.ToFloat(jdTemp, "center_z");
            _rprobe.center = new Vector3(_v1, _v2, _v3);
            _v1 = LJsonHelper.ToFloat(jdTemp, "size_x");
            _v2 = LJsonHelper.ToFloat(jdTemp, "size_y");
            _v3 = LJsonHelper.ToFloat(jdTemp, "size_z");
            _rprobe.size = new Vector3(_v1, _v2, _v3);
            _rprobe.mode = ReflectionProbeMode.Custom;
            _rprobe.customBakedTexture = _srefl.m_obj;
        }
    }

    void _ClearProbes()
    {
        if (LightmapSettings.lightProbes != null)
        {
            LightmapSettings.lightProbes.bakedProbes = null;
            LightProbes.Tetrahedralize();
        }
    }

    public void ReProbes()
    {
        if (!this.m_isDoned)
            return;

        this._ClearProbes();

        if (this.m_lProbes == null)
            return;
        LightProbes _lp = this.m_lProbes.m_obj;
        LightmapSettings.lightProbes = _lp;
        LightProbes.TetrahedralizeAsync();
    }
}
