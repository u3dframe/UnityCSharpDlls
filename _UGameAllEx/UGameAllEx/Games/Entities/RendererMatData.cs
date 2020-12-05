using UnityEngine;
using System.Collections.Generic;
using Core.Kernel;

/// <summary>
/// 类名 : Render 渲染 材质 Mats
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2019-10-14 09:53
/// 功能 : 
/// </summary>
[System.Serializable]
public class RendererMatData
{
    private int m_rerID = -1;
    Renderer m_currRer = null;
    bool m_isNewMat = false;
    bool m_isEditor = false;
    public List<Material> m_allMats { get; private set; }
    public List<Material> m_mats { get; private set; }

    private RendererMatData()
    {
        m_allMats = new List<Material>();
        m_mats = new List<Material>();
    }

    private RendererMatData(Renderer rer, bool isNewMat) : this()
    {
        Init(rer, isNewMat);
    }

    public RendererMatData Init(Renderer rer, bool isNewMat)
    {
        bool isHasMat = IsMatInRenderer(rer);
        if (!isHasMat)
            return this;

        CacheAll.Remove(this.m_rerID);

        this.m_isEditor = UGameFile.m_isEditor;
        this.ClearMat();
        this.m_currRer = rer;
        this.m_isNewMat = isNewMat;
        this.m_rerID = this.m_currRer.GetInstanceID();

        Material _mat_ = StaticEx.GetMat(rer);
        if (isNewMat && !m_isEditor)
        {
            _mat_ = UtilityHelper.NewMat(_mat_);
            if (_mat_ != null)
                rer.material = _mat_;
        }

        if (_mat_ != null)
            this.m_allMats.Add(_mat_);

        Material[] _mats_ = StaticEx.GetMats(rer);
        if (_mats_ != null && _mats_.Length > 0)
        {
            int _len = _mats_.Length;
            for (int j = 0; j < _len; j++)
            {
                _mat_ = _mats_[j];
                if (isNewMat && !m_isEditor)
                {
                    _mat_ = UtilityHelper.NewMat(_mat_);
                    if (_mat_ != null)
                        this.m_mats.Add(_mat_);
                }
                if (_mat_ != null)
                    this.m_allMats.Add(_mat_);
            }

            if (isNewMat && this.m_mats.Count > 0)
            {
                rer.materials = this.m_mats.ToArray();
            }
        }

        CacheAll.Add(this.m_rerID, this);

        return this;
    }

    public void ClearAll()
    {
        CacheAll.Remove(this.m_rerID);
        this.m_rerID = -1;
        this.m_currRer = null;
        this.ClearMat();
    }

    void ClearMat()
    {
        bool isNew = this.m_isNewMat || this.m_isEditor;
        List<Material> _list = null;
        if (isNew)
        {
            _list = new List<Material>();
            _list.AddRange(this.m_allMats);
        }
        this.m_allMats.Clear();
        this.m_mats.Clear();

        if (_list == null || _list.Count <= 0)
            return;

        int _len_ = _list.Count;
        for (int i = 0; i < _len_; i++)
        {
            UGameFile.UnLoadOne(_list[i], true);
        }
    }

    public void ChangeMat(Material newMat, int nType)
    {
        if (!this.m_isNewMat && nType != 1)
            return;

        this.m_allMats.Remove(newMat);
        this.m_mats.Remove(newMat);
        switch (nType)
        {
            case 0:
                // delete
                UGameFile.UnLoadOne(newMat, true);
                break;
            default:
                // 1 = replace, other = add
                if (nType == 1)
                {
                    this.ClearMat();
                    this.m_isNewMat = true;
                }

                this.m_allMats.Add(newMat);
                this.m_mats.Add(newMat);
                break;

        }
        this.m_currRer.materials = this.m_mats.ToArray();
    }

    static Dictionary<int, RendererMatData> CacheAll = new Dictionary<int, RendererMatData>();

    static public bool IsMatInRenderer(Renderer rer)
    {
        if (rer == null)
            return false;
        bool isHas = (rer.sharedMaterial != null);
        if (!isHas)
        {
            Material[] _mats_ = rer.sharedMaterials;
            if (_mats_ != null && _mats_.Length > 0)
            {
                int _len = _mats_.Length;
                for (int i = 0; i < _len; i++)
                {
                    if (_mats_[i] != null)
                    {
                        isHas = true;
                        break;
                    }
                }
            }
        }
        return isHas;
    }

    static public RendererMatData Builder(Renderer rer, bool isNewMat)
    {
        bool isHasMat = IsMatInRenderer(rer);
        if (!isHasMat)
            return null;
        RendererMatData _it = null;
        if (!CacheAll.TryGetValue(rer.GetInstanceID(), out _it))
            _it = new RendererMatData(rer, isNewMat);
        return _it;
    }

    static public RendererMatData BuilderNew(Renderer rer)
    {
        return Builder(rer, true);
    }
}
