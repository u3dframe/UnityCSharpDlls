using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 类名 : Render 渲染 排序控制
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2019-09-10 19:33
/// 功能 : 
/// </summary>
public class RendererSortOrder : GobjLifeListener
{
    public enum SortType
    {
        None = 0,
        SLayer = 1,
        RenderQueue = 2,
        LayerAndQueue = 3
    }

    static public new RendererSortOrder Get(GameObject gobj, bool isAdd)
    {
        return UtilityHelper.Get<RendererSortOrder>(gobj, isAdd);
    }

    static public new RendererSortOrder Get(GameObject gobj)
    {
        return Get(gobj, true);
    }

    public SortType m_sType = SortType.LayerAndQueue;
    public string m_nmLayer = "Default";
    int m_sLayerID = 0;
    bool m_isNameInSLayer = true;

    public bool m_isAdd = true;
    public int m_val_layer = 0;
    public int m_val_queue = 0;

    Renderer[] renderers;
    string _k_sl_id = "r_sid_{0}";
    string _k_sl_val = "r_val_{0}";
    string _k_mat_val = "m_val_{0}";
    Dictionary<string, int> m_r_sinfo = new Dictionary<string, int>();

    public float m_delay = 0.1f;
    bool m_isInited = false;
    [SerializeField] bool m_isNewMat = true;
    Dictionary<int, RendererMatData> m_dicAllMats = new Dictionary<int, RendererMatData>();

    override protected void OnCall4Start()
    {
        this.Init(this.m_isNewMat);
    }

    override protected void OnCall4Destroy()
    {
        if (m_dicAllMats.Count > 0)
        {
            foreach (RendererMatData item in m_dicAllMats.Values)
            {
                item.ClearAll();
            }
        }
    }

    override protected void OnClear()
    {
        this.m_dicAllMats.Clear();
    }

    void Update()
    {
        _UpDelayReRS();
    }

    public RendererSortOrder Init(bool isNewMat = true)
    {
        if (m_isInited)
            return this;
        m_isInited = true;
        this.m_isNewMat = isNewMat;

        renderers = this.GetComponentsInChildren<Renderer>(true);
        _InitSortInfo();

        if (this.m_delay <= 0)
            ReRenderSorting();
        return this;
    }

    void _UpDelayReRS()
    {
        if (this.m_delay <= 0)
            return;

        this.m_delay -= Time.deltaTime;
        if (this.m_delay > 0)
            return;

        ReRenderSorting();
    }

    void _InitSortInfo()
    {
        if (renderers == null || renderers.Length <= 0)
            return;

        int _r_o_id = 0;
        int _m_o_id = 0;

        int lens = renderers.Length;
        Renderer _rer_;
        string _k;

        RendererMatData rmData = null;
        Material _mat_;
        int len_mat = 0;
        for (int i = 0; i < lens; i++)
        {
            _rer_ = renderers[i];
            if (_rer_ == null)
                continue;
            _r_o_id = _rer_.GetInstanceID();

            _k = string.Format(_k_sl_id, _r_o_id);
            _AddDefVal(_k, _rer_.sortingLayerID);

            _k = string.Format(_k_sl_val, _r_o_id);
            _AddDefVal(_k, _rer_.sortingOrder);

            rmData = RendererMatData.Builder(_rer_, this.m_isNewMat);
            if (rmData == null)
            {
                // Debug.LogErrorFormat("=========[{0}] =[{1}] =[{2}] =[{3}]",_r_o_id,this.m_isNewMat,RendererMatData.IsMatInRenderer(_rer_),_rer_.name);
                continue;
            }
            this.m_dicAllMats.Add(_r_o_id, rmData);

            len_mat = rmData.m_allMats.Count;
            for (int j = 0; j < len_mat; j++)
            {
                _mat_ = rmData.m_allMats[j];
                if (_mat_ != null)
                {
                    _m_o_id = _mat_.GetInstanceID();
                    _k = string.Format(_k_mat_val, _m_o_id);
                    _AddDefVal(_k, _mat_.renderQueue);
                }
            }
        }
    }

    void _AddDefVal(string key, int val)
    {
        if (this.m_r_sinfo.ContainsKey(key))
        {
            // Debug.LogFormat("======= same key = [{0}] , val = [{1}] = [{2}]",key,val,this.m_r_sinfo[key]);
            return;
        }
        this.m_r_sinfo.Add(key, val);
    }

    int _GetDefVal(string key)
    {
        if (this.m_r_sinfo.ContainsKey(key))
            return this.m_r_sinfo[key];
        return -9999999;
    }

    [ContextMenu("Print Sort Layer")]
    void _PrintSortLayer()
    {
        SortingLayer[] _layers = SortingLayer.layers;
        int lens = _layers.Length;
        SortingLayer _sLayer;
        int _s_id = -1;
        for (int i = 0; i < lens; i++)
        {
            _sLayer = _layers[i];
            _s_id = SortingLayer.NameToID(_sLayer.name);
            Debug.LogErrorFormat("========== id = [{0}] = [{1}] , name = [{2}] , value = [{3}]", _s_id, _sLayer.id, _sLayer.name, _sLayer.value);
        }
    }

    void _CheckSortName()
    {
        this.m_isNameInSLayer = false;
        this.m_sLayerID = 0;
        if (string.IsNullOrEmpty(this.m_nmLayer))
        {
            return;
        }
        this.m_sLayerID = SortingLayer.NameToID(this.m_nmLayer);
        this.m_isNameInSLayer = SortingLayer.IsValid(this.m_sLayerID);
    }

    [ContextMenu("Re Render Sorting")]
    public void ReRenderSorting()
    {
        if (renderers == null || renderers.Length <= 0)
            return;

        _CheckSortName();

        bool isLayer = this.m_sType == SortType.SLayer || this.m_sType == SortType.LayerAndQueue;
        bool isRender = this.m_sType == SortType.RenderQueue || this.m_sType == SortType.LayerAndQueue;
        int lens = renderers.Length;
        Renderer _rer_;
        for (int i = 0; i < lens; i++)
        {
            _rer_ = renderers[i];
            _ReSLayer(_rer_, isLayer);
            _ReRenderQueue(_rer_, isRender);
        }
    }

    void _ReSLayer(Renderer rer, bool isCan)
    {
        if (!isCan || rer == null || this.m_val_layer == 0) return;
        if (m_isNameInSLayer)
        {
            rer.sortingLayerName = this.m_nmLayer;
            rer.sortingLayerID = this.m_sLayerID;
        }

        if (m_isAdd)
        {
            int _r_o_id = rer.GetInstanceID();
            string _k = string.Format(_k_sl_val, _r_o_id);
            int _v = _GetDefVal(_k);
            if (_v != -9999999)
            {
                rer.ReSortingOrder(this.m_val_layer + _v);
            }
        }
        else
        {
            rer.ReSortingOrder(this.m_val_layer);
        }
    }

    int _GetRenderQueue(Material _mat_)
    {
        if (_mat_ == null)
            return -9999999;
        int _m_o_id = _mat_.GetInstanceID();
        string _k = string.Format(_k_mat_val, _m_o_id);
        return _GetDefVal(_k);
    }

    void _ReRenderQueue(Renderer rer, bool isCan)
    {
        if (!isCan || rer == null || this.m_val_queue == 0) return;
        int _r_o_id = rer.GetInstanceID();
        RendererMatData rmData = null;
        if (this.m_dicAllMats.ContainsKey(_r_o_id))
        {
            rmData = this.m_dicAllMats[_r_o_id];
        }
        if (rmData == null)
        {
            return;
        }

        List<Material> _list = rmData.m_allMats;
        int _len_ = _list.Count;
        if (_len_ <= 0) return;
        Material _mat_;
        int _rq = -1;
        for (int i = 0; i < _len_; i++)
        {
            _mat_ = _list[i];
            if (_mat_ == null)
                continue;

            if (m_isAdd)
            {
                _rq = _GetRenderQueue(_mat_);
                if (_rq != -9999999)
                {
                    _mat_.ReRenderQueue(this.m_val_queue + _rq);
                }
            }
            else
            {
                _mat_.ReRenderQueue(this.m_val_queue);
            }
        }
    }
}
