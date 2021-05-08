using UnityEngine;
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
    static public DF_ToLoadTex2D m_cfLoad { get; set; }
    static Dictionary<string, SceneMapEx> m_caches = new Dictionary<string, SceneMapEx>();

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
            _loadData_ = new SLInfo(_fp_ab, _c, _d, _s,m_cfLoad);
            this.listSLInfos.Add(_loadData_);
        }
        if(_nLens > 0)
            this.m_u_state = MSM_UpState.CheckSLInfo;
        else
            this.m_u_state = MSM_UpState.ReLMap;
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
            this.m_u_state = MSM_UpState.ReLMap;
    }

    void _ST_ReLightmap()
    {
        this.m_u_state = MSM_UpState.Finish;
        this._ReLightmap();
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

    public void ReLightmap()
    {
        if(this.m_isDoned)
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
    }

    [ContextMenu("Clear Lightmap")]
    void _ClearLMap()
    {
        this._ClearSLInfo();
        this._ReLightmap();
    }
	
	void _ClearJson()
    {
        if (this.m_mapJdRoot != null)
            this.m_mapJdRoot.Clear();
        this.m_mapJdRoot = null;
    }
}