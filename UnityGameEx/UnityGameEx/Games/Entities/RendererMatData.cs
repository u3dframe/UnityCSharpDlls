using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 类名 : Render 渲染 材质 Mats
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-10-14 09:53
/// 功能 : 
/// </summary>
[System.Serializable]
public class RendererMatData
{
    public Renderer m_currRer = null;
    public bool m_isNewMat = false;
    public List<Material> m_rerMats = new List<Material>();

    public RendererMatData(){}

    public RendererMatData(Renderer rer,bool isNewMat){
        Init(rer,isNewMat);
    }

    public RendererMatData Init(Renderer rer,bool isNewMat){
        this.m_currRer = rer;
        this.m_isNewMat = isNewMat;

        Material _mat_;
        Material[] _mats_;
        if(isNewMat){
            _mat_ = rer.sharedMaterial;
            if(_mat_){
                _mat_ = new Material(_mat_);
                rer.material = _mat_;
            }
            _mats_ = rer.sharedMaterials;
        }else{
            _mat_ = StaticEx.GetMat(rer);
            _mats_ = StaticEx.GetMats(rer);
        }

        if(_mat_ != null){
            this.m_rerMats.Add(_mat_);
        }

        if (_mats_ != null && _mats_.Length > 0) {
            int _len = _mats_.Length;
            List<Material> _mats_1 = new List<Material>();
            for (int j = 0; j < _len; j++) {
                _mat_ = _mats_[j];
                if(_mat_ != null){
                    if(isNewMat){
                        _mat_ = new Material(_mat_);
                        _mats_1.Add(_mat_);
                    }
                    this.m_rerMats.Add(_mat_);
                }
            }

            if(isNewMat && _mats_1.Count > 0){
                rer.materials = _mats_1.ToArray();
            }
        }
        return this;
    }

    public void ClearAll(){
        this.m_currRer = null;

        bool isNew = this.m_isNewMat;
#if UNITY_EDITOR
        isNew = true;
#endif
        List<Material> _list = null;
        if(isNew){
            _list = new List<Material>();
            _list.AddRange(this.m_rerMats);
        }
        this.m_rerMats.Clear();

        if(_list == null || _list.Count <= 0)
            return;
        
        int _len_ = _list.Count;
        for (int i = 0; i < _len_; i++)
        {
            Core.Kernel.UGameRes.UnLoadOne(_list[i],true);
        }
    }

    static public bool IsMatInRenderer(Renderer rer){
        if(rer == null)
            return false;
        bool isHas = (rer.sharedMaterial != null);
        if(!isHas){
            Material[] _mats_ = rer.sharedMaterials;
            if (_mats_ != null && _mats_.Length > 0) {
                int _len = _mats_.Length;
                for (int i = 0; i < _len; i++)
                {
                    if(_mats_[i] != null){
                        isHas = true;
                        break;
                    }
                }
            }
        }
        return isHas;
    }

    static public RendererMatData Builder(Renderer rer,bool isNewMat){
        bool isHasMat = IsMatInRenderer(rer);
        if(!isHasMat)
            return null;
        
        return new RendererMatData(rer,isNewMat);
    }

    static public RendererMatData BuilderNew(Renderer rer){
        return Builder(rer,true);
    }
}
