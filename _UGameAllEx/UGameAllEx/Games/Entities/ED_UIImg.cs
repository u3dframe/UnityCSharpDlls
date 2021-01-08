using UnityEngine;
using System;
using Core;
using UnityEngine.UI;

/// <summary>
/// 类名 : UIImage 数据脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-12-18 14:03
/// 功能 : 数据加载逻辑
/// </summary>
public class ED_UIImg : Core.Kernel.Beans.ED_Comp
{
    static public new ED_UIImg Builder(UnityEngine.Object uobj)
    {
        return Builder<ED_UIImg>(uobj);
    }

    static public ED_UIImg Builder(Image image)
    {
        if (image == null || !image)
            return null;
        ED_UIImg _it = Builder(image.gameObject);
        _it.InitComp(image,null,null,null);
        return _it;
    }
    
    public string m_sAtals { get; private set; }
    public string m_sImg { get; private set; }
    public Image m_img { get; private set; }
    public AssetInfo m_asset { get; private set; }
    public bool m_isNativeSize { get; private set; }

    public ED_UIImg() : base()
    {
    }

    override public void InitComp(string strComp, Action cfDestroy, Action cfShow, Action cfHide)
    {
        base.InitComp(strComp, cfDestroy, cfShow, cfHide);
    }
    
    override public void InitComp(Component comp, Action cfDestroy, Action cfShow, Action cfHide)
    {
        base.InitComp(comp, cfDestroy, cfShow, cfHide);
        this.m_img = this.m_comp as Image;
    }

    override protected void On_Destroy(GobjLifeListener obj)
    {
        this.m_img = null;
        this.OnUnLoadAsset();
        this.m_sAtals = null;
        this.m_sImg = null;
        this.m_isNativeSize = false;
        base.On_Destroy(obj);
    }

    void OnUnLoadAsset()
    {
        AssetInfo _asset = this.m_asset;
        this.m_asset = null;
        if(_asset != null){
            _asset.UnLoadAsset();
        }
    }

    public string ReAtals(int nType,string sAtals)
    {
        switch (nType)
        {
            case 1:
            sAtals = GameFile.ReSBegEnd( sAtals,"textures/ui_sngs/icons/",".tex" );
            break;
            case 2:
            sAtals = GameFile.ReSBegEnd( sAtals,"textures/ui_sngs/bgs/",".tex" );
            break;
            case 3:
            sAtals = GameFile.ReSBegEnd( sAtals,"textures/ui_sngs/minihead/",".tex" );
            break;
            case 4:
            sAtals = GameFile.ReSBegEnd( sAtals,"textures/ui_sngs/halfbody/",".tex" );
            break;
            case 5:
            sAtals = GameFile.ReSBegEnd( sAtals,"textures/ui_sngs/fullbody/",".tex" );
            break;
            case 6:
            sAtals = GameFile.ReSBegEnd( sAtals,"textures/ui_sngs/",".tex" );
            break;
            default:
            sAtals = GameFile.ReSBegEnd( sAtals,"textures/ui_atlas/",".tex_atlas" );
            break;
        }
        return sAtals;
    }

    public string RePng(string sImg)
    {
        var _arrs = GameFile.Split(sImg,"/".ToCharArray(),true);
        sImg = _arrs[_arrs.Length - 1];
        return GameFile.ReSEnd(sImg,".png");
    }

    public void SetImage(int nType,string sAtals,string sImg,bool isNativeSize,bool isNdReRes = true)
    {
        if(isNdReRes == true)
        {
            sAtals = this.ReAtals(nType,sAtals);
            sImg =  this.RePng(sImg);
        }
        this.m_isNativeSize = isNativeSize;

        if(!string.IsNullOrEmpty(this.m_sAtals) && !string.IsNullOrEmpty(this.m_sImg))
        {
            bool isSameAB = this.m_sAtals.Equals(sAtals);
            bool isSameAsset = this.m_sImg.Equals(sImg);
            if(isSameAB && isSameAsset)
            {
                return;
            }

            this.OnUnLoadAsset();
        }
        this.m_sAtals = sAtals;
        this.m_sImg = sImg;
        this.m_asset = ResourceManager.LoadSprite(sAtals,sImg,_OnLoadSprite);
    }

    void _OnLoadSprite(Sprite sprite)
    {
        if(!this.m_img)
            return;
        if(!sprite)
        {
            Debug.LogErrorFormat( "=== ED_UIImg Load Err ,sAtals = [{0}] , sImg = [{1}]" , this.m_sAtals,this.m_sImg );
            return;
        }
        
        this.m_img.sprite = sprite;
        if(this.m_isNativeSize)
            this.SetNativeSize();
    }

    public void SetIcon(string icon,bool isNativeSize)
    {
        this.SetImage( 1,icon,icon,isNativeSize );
    }

    public void SetBg(string icon,bool isNativeSize)
    {
        this.SetImage( 2,icon,icon,isNativeSize );
    }

    public void SetImgHead(string icon,bool isNativeSize)
    {
        this.SetImage( 3,icon,icon,isNativeSize );
    }

    public void SetImgHalfBody(string icon,bool isNativeSize)
    {
        this.SetImage( 4,icon,icon,isNativeSize );
    }

    public void SetImgBody(string icon,bool isNativeSize)
    {
        this.SetImage( 5,icon,icon,isNativeSize );
    }

    public void SetImgSng(string icon,bool isNativeSize)
    {
        this.SetImage( 6,icon,icon,isNativeSize );
    }

    public void SetFillAmount(float val,float max = 0)
    {
        if(!this.m_img)
            return;
        float amount = (max > 0) ? (val / max) : val;
        this.m_img.fillAmount = amount;
    }

    public void SetNativeSize()
    {
        if(!this.m_img)
            return;
        this.m_img.SetNativeSize();
    }

    public void SetNativeSizeASync()
    {
        this.m_isNativeSize = true;
        this.SetNativeSize();
    }
}