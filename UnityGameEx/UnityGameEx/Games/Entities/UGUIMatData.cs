using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 类名 : UGUI Graphic 渲染 材质
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-10-14 09:53
/// 功能 : 
/// </summary>
[System.Serializable]
public class UGUIMatData
{
    public MaskableGraphic m_graphic = null;
    public Material m_mat = null;
    public Material m_newMat = null;

    public UGUIMatData(){}

    public UGUIMatData(MaskableGraphic graphic,bool isNewMat){
        Init(graphic,isNewMat);
    }

    public UGUIMatData Init(MaskableGraphic graphic,bool isNewMat){
        this.m_graphic = graphic;
        this.m_mat = this.m_graphic.material;

        GobjLifeListener glife = GobjLifeListener.Get(graphic.gameObject);
        glife.AddOnlyOnceDCall(this.OnNotifyDestry);
        glife.m_obj1 = this;

        if (isNewMat){
            if(this.m_mat != null){
                this.m_newMat = new Material(this.m_mat);
                this.m_graphic.material = this.m_newMat;
            }
        }
       
        return this;
    }

    void OnNotifyDestry(GobjLifeListener gLife){
        this.ClearAll();
    }

    public void ClearAll(){
        Material _mat_ = this.m_newMat;
        this.m_graphic = null;
        this.m_mat = null;
        this.m_newMat = null;

        if (_mat_ != null)
        {
            Core.Kernel.UGameRes.UnLoadOne(_mat_,true);
        }
    }

    static public UGUIMatData Builder(MaskableGraphic graphic,bool isNewMat){
        if(!graphic)
            return null;
        return new UGUIMatData(graphic,isNewMat);
    }
}
