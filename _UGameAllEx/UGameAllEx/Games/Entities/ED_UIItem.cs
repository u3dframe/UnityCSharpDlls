using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 类名 : UIItem 数据脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-12-18 14:03
/// 功能 : 
/// </summary>
public class ED_UIItem : Core.Kernel.Beans.ED_Comp
{
    public class ItemStar{
        public GameObject m_gobj;
        public ED_UIImg m_imgBg;
        public ED_UIImg m_imgIcon;
    }

    public PrefabElement m_csEle { get; private set; } // 当前对象


    public GameObject m_gobjShadow { get; private set; } // 影子
    public UGUILocalize m_txtName { get; private set; } // 名字
    public UGUILocalize m_txtValue { get; private set; } // 值
    public ED_UIImg m_imgValBg { get; private set; } // 值的底图  lbvalmg
    public UGUILocalize m_txtValDesc { get; private set; } // 值的描述
    public UGUILocalize m_txtDesc { get; private set; }  // 描述
    public UGUILocalize m_txtOrder { get; private set; } // 阶级
    public GameObject m_gobjOrder { get; private set; } // 阶级 - 图片

    public GameObject m_gobjTag { get; private set; } // 标签 - 背景图片
    public UGUILocalize m_txtTag { get; private set; } // 标签

    public ED_UIImg m_imgBg { get; private set; } // 背景
    public ED_UIImg m_imgSSR { get; private set; } // SSR
    public ED_UIImg m_imgIcon { get; private set; } // icon
    public ED_UIImg m_imgQuality { get; private set; } // 品质 quality
    public ED_UIImg m_imgFeatureBg { get; private set; } // 特性背景
    public ED_UIImg m_imgFeatureIcon { get; private set; } // 特性 Icon

    public GameObject m_gobjEmpty { get; private set; } // 空 - 对象
    public GameObject m_gobjSelect { get; private set; } // 选中 - 对象
    public GameObject m_gobjLock { get; private set; } // 锁 - 对象
    public GameObject m_gobjTopTag { get; private set; } // 头顶标签
    public UGUILocalize m_txtTopTag { get; private set; } // 头顶标签

    public GameObject m_gobjMinHero { get; private set; } // 英雄小头像
    public ED_UIImg m_imgMinHeroIcon { get; private set; } // 英雄小头像 Icon
    public UGUILocalize m_txtMinHero { get; private set; } // 英雄小头像

    public GameObject m_gobjFragment { get; private set; } // 

    List<ED_UIItem.ItemStar> m_listStars = new List<ED_UIItem.ItemStar>();
    protected ED_UIItem(GameObject gobj) : base(gobj)
    {
    }

    override public void InitComp(string strComp, Action cfDestroy, Action cfShow, Action cfHide)
    {
        base.InitComp(strComp, cfDestroy, cfShow, cfHide);
    }

    override public void InitComp(Component comp, Action cfDestroy, Action cfShow, Action cfHide)
    {
        if(comp == null)
            comp = PrefabElement.Get(this.m_gobj,false);
        base.InitComp(comp, cfDestroy, cfShow, cfHide);
        PrefabElement csEle = this.m_comp as PrefabElement;
        if(!csEle)
            return;
        
        this.m_csEle = csEle;
        this.m_gobjShadow = csEle.GetGobjElement("shadow");
        this.m_txtName = csEle.GetComponent4Element<UGUILocalize>("name");
        this.m_txtValDesc = csEle.GetComponent4Element<UGUILocalize>("val_desc");
        this.m_txtValue = csEle.GetComponent4Element<UGUILocalize>("value");
        this.m_txtDesc = csEle.GetComponent4Element<UGUILocalize>("desc");
        this.m_txtOrder = csEle.GetComponent4Element<UGUILocalize>("order");
        this.m_gobjOrder = csEle.GetGobjElement("orderimg");
        this.m_gobjTag = csEle.GetGobjElement("bg_tag");
        this.m_txtTag = csEle.GetComponent4Element<UGUILocalize>("tag");

        Image _img = csEle.GetComponent4Element<Image>("bg");
        this.m_imgBg = ED_UIImg.Builder(_img);

        _img = csEle.GetComponent4Element<Image>("rare");
        this.m_imgSSR = ED_UIImg.Builder(_img);

        _img = csEle.GetComponent4Element<Image>("icon");
        this.m_imgIcon = ED_UIImg.Builder(_img);

        _img = csEle.GetComponent4Element<Image>("quality");
        this.m_imgQuality = ED_UIImg.Builder(_img);

        _img = csEle.GetComponent4Element<Image>("val_bg");
        this.m_imgValBg = ED_UIImg.Builder(_img);

        _img = csEle.GetComponent4Element<Image>("feature");
        this.m_imgFeatureBg = ED_UIImg.Builder(_img);

        _img = csEle.GetComponent4Element<Image>("featureIcon");
        this.m_imgFeatureIcon = ED_UIImg.Builder(_img);


        this.m_gobjEmpty = csEle.GetGobjElement("empty");
        this.m_gobjSelect = csEle.GetGobjElement("selected");
        this.m_gobjLock = csEle.GetGobjElement("lock");

        this.m_gobjTopTag = csEle.GetGobjElement("topTag");
        this.m_txtTopTag = csEle.GetComponent4Element<UGUILocalize>("topTagTxt");

        PrefabElement _csEleTemp = csEle.GetComponent4Element<PrefabElement>("minHero");
        if(_csEleTemp != null){
            this.m_gobjMinHero = _csEleTemp.gameObject;
            _img = _csEleTemp.GetComponent4Element<Image>("icon");
            this.m_imgMinHeroIcon = ED_UIImg.Builder(_img);
            this.m_txtMinHero = _csEleTemp.GetComponent4Element<UGUILocalize>("lvTxt");
        }

        Transform _trsfStars = csEle.GetTrsfElement("wrapStars");
        if(_trsfStars != null){
            Transform _star = null;
            ED_UIItem.ItemStar _it = null;
            for (int i = 0; i < _trsfStars.childCount; i++)
            {
                _star = _trsfStars.GetChild(i);
                _csEleTemp = _star.GetComponent<PrefabElement>();
                if(_csEleTemp == null)
                    continue;
                
                _it = new ItemStar();
                _it.m_gobj = _csEleTemp.gameObject;
                _img = _csEleTemp.GetComponent4Element<Image>("bg");
                _it.m_imgBg = ED_UIImg.Builder(_img);

                _img = _csEleTemp.GetComponent4Element<Image>("icon");
                _it.m_imgIcon = ED_UIImg.Builder(_img);
                this.m_listStars.Add(_it);
            }
        }

        this.m_gobjFragment = csEle.GetGobjElement("fragment");
    }

    public void VwBgImg(string icon)
    {
        if(this.m_imgBg == null)
            return;
        bool _isBl = !string.IsNullOrEmpty(icon);
        if(_isBl)
        {
            this.m_imgBg.SetIcon(icon,false);
        }
        this.m_imgBg.SetActive(_isBl);
    }

    ED_UIItem.ItemStar NewStar()
    {
        if(this.m_listStars.Count <= 0)
            return null;

        ED_UIItem.ItemStar _it = this.m_listStars[0];
        GameObject gobj = UtilityHelper.Clone(_it.m_gobj);
        PrefabElement _csEleTemp = gobj.GetComponent<PrefabElement>();
        if(_csEleTemp == null)
            return null;
        
        _it = new ED_UIItem.ItemStar();
        _it.m_gobj = gobj;
        Image _img = _csEleTemp.GetComponent4Element<Image>("bg");
        _it.m_imgBg = ED_UIImg.Builder(_img);

        _img = _csEleTemp.GetComponent4Element<Image>("icon");
        _it.m_imgIcon = ED_UIImg.Builder(_img);
        this.m_listStars.Add(_it);
        return _it;
    }

    public ED_UIItem.ItemStar GetStar(int nIndex,ref bool isNew)
    {
        isNew = false;
        int _cout = this.m_listStars.Count;
        if(_cout <= 0)
            return null;
        if(_cout > nIndex)
            return this.m_listStars[nIndex];
        
        isNew = true;
        return this.NewStar();
    }

    public void VwStars(int star,string sbg,string sicon)
    {
        int _cout = this.m_listStars.Count;
        if(_cout <= 0)
            return;
        ED_UIItem.ItemStar _cs = null;
        int _max = UtilityHelper.NMaxMore(star,_cout);

        bool _isBlBg = !string.IsNullOrEmpty(sbg);
        bool _isBl = !string.IsNullOrEmpty(sicon);
        bool _isBlIcon = false;
        for (int i = 0; i < _max; i++)
        {
            if( i >= _cout )
                _cs = this.NewStar();
            else
                _cs = this.m_listStars[i];
                
            if(_isBlBg)
                _cs.m_imgBg.SetIcon(sbg,false);
            
            _isBlIcon = _isBl && (i < star);
            if(_isBlIcon)
                _cs.m_imgIcon.SetIcon(sicon,false);
            _cs.m_imgIcon.SetActive(_isBlIcon);
        }
    }

    public void VwName(object obj)
    {
        if(this.m_txtName == null)
            return;
        
        bool _isBl = obj != null;
        if(_isBl)
        {
            if(obj is int)
                this.m_txtName.SetText((int)obj);
            else
                this.m_txtName.SetText(obj.ToString());
        }
    }

    public void VwNameColor(float r,float g,float b,float a = 1)
    {
        if(this.m_txtName == null)
            return;
        r = r > 1 ? r / 255f : r;
        g = g > 1 ? g / 255f : g;
        b = b > 1 ? b / 255f : b;
        a = a > 1 ? a / 255f : a;
        Color _c = new Color(r,g,b,a);
        this.VwNameColor(_c);
    }

    public void VwNameColor(Color color)
    {
        if(this.m_txtName == null)
            return;
        this.m_txtName.SetColor(color);
    }

    public void VwValueBg(string icon)
    {
        if(this.m_imgValBg == null)
            return;
        bool _isBl = !string.IsNullOrEmpty(icon);
        if(_isBl)
        {
            this.m_imgValBg.SetIcon(icon,false);
        }
        this.m_imgValBg.SetActive(_isBl);
    }

    public void VwValue(object obj)
    {
        if(this.m_txtValue == null)
            return;
        
        bool _isBl = obj != null;
        if(_isBl)
        {
            if(obj is int)
                this.m_txtValue.SetText((int)obj);
            else
                this.m_txtValue.SetText(obj.ToString());
        }
    }

    public void VwValueColor(float r,float g,float b,float a = 1)
    {
        if(this.m_txtValue == null)
            return;
        
        r = r > 1 ? r / 255f : r;
        g = g > 1 ? g / 255f : g;
        b = b > 1 ? b / 255f : b;
        a = a > 1 ? a / 255f : a;
        Color _c = new Color(r,g,b,a);
        this.VwValueColor(_c);
    }

    public void VwValueColor(Color color)
    {
        if(this.m_txtValue == null)
            return;
        this.m_txtValue.SetColor(color);
    }

    public void VwValueDesc(object obj)
    {
        if(this.m_txtValDesc == null || obj == null)
            return;
        
        bool _isBl = obj != null;
        if(_isBl)
        {
            if(obj is int)
                this.m_txtValDesc.SetText((int)obj);
            else
                this.m_txtValDesc.SetText(obj.ToString());
        }
    }

    public void VwOrder(object obj)
    {
        if(this.m_txtOrder == null)
            return;

        bool _isBl = obj != null;
        if(_isBl)
            this.m_txtOrder.SetUText(obj.ToString());
        
        this.m_gobjOrder.SetActive(_isBl);
    }

    public void VwTag(object obj)
    {
        if(this.m_txtTag == null)
            return;
        
        bool _isBl = obj != null;
        if(_isBl)
        {
            if(obj is int)
                this.m_txtTag.SetText((int)obj);
            else
                this.m_txtTag.SetText(obj.ToString());
        }

        this.m_gobjTag.SetActive(_isBl);
    }

    public void VwDesc(object obj)
    {
        if(this.m_txtDesc == null)
            return;
        
        bool _isBl = obj != null;
        if(_isBl)
        {
            if(obj is int)
                this.m_txtDesc.SetText((int)obj);
            else
                this.m_txtDesc.SetText(obj.ToString());
        }
    }

    public void VwIcon(string icon,int type)
    {
        if(this.m_imgIcon == null)
            return;
        bool _isBl = !string.IsNullOrEmpty(icon);
        if(_isBl)
        {
            if(type == 6)
                this.m_imgIcon.SetImgHead(icon,false);
            else
                this.m_imgIcon.SetIcon(icon,false);
        }

        this.m_imgIcon.SetActive(_isBl);
    }

    public void VwQuality(string icon)
    {
        if(this.m_imgQuality == null)
            return;
        bool _isBl = !string.IsNullOrEmpty(icon);
        if(_isBl)
        {
            this.m_imgQuality.SetIcon(icon,false);
        }
        this.m_imgQuality.SetActive(_isBl);
    }

    public void VwSSR(string icon)
    {
        if(this.m_imgSSR == null)
            return;
        bool _isBl = !string.IsNullOrEmpty(icon);
        if(_isBl)
        {
            this.m_imgSSR.SetIcon(icon,false);
        }
        this.m_imgSSR.SetActive(_isBl);
    }

    public void VwEmptyObj(bool isActive)
    {
        if(this.m_gobjEmpty == null)
            return;
        this.m_gobjEmpty.SetActive(isActive);
    }

    public void VwShadow(bool isActive)
    {
        if(this.m_gobjShadow == null)
            return;
        this.m_gobjShadow.SetActive(isActive);
    }

    public void VwSelect(bool isActive)
    {
        if(this.m_gobjSelect == null)
            return;
        this.m_gobjSelect.SetActive(isActive);
    }

    public void VwLock(bool isActive)
    {
        if(this.m_gobjLock == null)
            return;
        this.m_gobjLock.SetActive(isActive);
    }

    public void VwFragment(bool isActive)
    {
        if(this.m_gobjFragment == null)
            return;
        this.m_gobjFragment.SetActive(isActive);
    }

    public void VwTopTag(object obj)
    {
        if(this.m_txtTopTag == null)
            return;
        
        bool _isBl = obj != null;
        if(_isBl)
        {
            if(obj is int)
                this.m_txtTopTag.SetText((int)obj);
            else
                this.m_txtTopTag.SetText(obj.ToString());
        }

        this.m_gobjTopTag.SetActive(_isBl);
    }

    public void VwFeatureBg(string icon)
    {
        if(this.m_imgFeatureBg == null)
            return;
        bool _isBl = !string.IsNullOrEmpty(icon);
        if(_isBl)
        {
            this.m_imgFeatureBg.SetIcon(icon,false);
        }
        this.m_imgFeatureBg.SetActive(_isBl);
    }

    public void VwFeatureIcon(string icon)
    {
        if(this.m_imgFeatureIcon == null)
            return;
        bool _isBl = !string.IsNullOrEmpty(icon);
        if(_isBl)
        {
            this.m_imgFeatureIcon.SetIcon(icon,false);
        }
        this.m_imgFeatureIcon.SetActive(_isBl);
    }

    public void VwMinHero(string icon,string strHero,float offX,float offY)
    {
        if(this.m_gobjMinHero == null)
            return;
        bool _isBl = !string.IsNullOrEmpty(icon);
        if(_isBl)
        {
            this.m_txtMinHero.SetUText(strHero);
            this.m_imgMinHeroIcon.SetImgHead(icon,false);
            this.m_imgMinHeroIcon.m_img.SetProperty("_OffsetX",offX);
            this.m_imgMinHeroIcon.m_img.SetProperty("_OffsetY",offY);
        }
        this.m_gobjMinHero.SetActive(_isBl);
    }

    override protected void On_Destroy(GobjLifeListener obj)
    {
        this.m_listStars.Clear();
        base.On_Destroy(obj);
    }

    static public new ED_UIItem Builder(UnityEngine.Object uobj)
    {
        GameObject _go = UtilityHelper.ToGObj(uobj);
        if (_go == null || !_go)
            return null;
        return new ED_UIItem(_go); 
    }
}