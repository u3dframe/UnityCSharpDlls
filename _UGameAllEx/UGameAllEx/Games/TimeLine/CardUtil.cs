using System;
using UnityEngine;
using System.Collections.Generic;

public class CardUtil : ED_Animator
{
    public PrefabElement m_csEle { get; private set; } // 当前对象
    public GameObject m_csd { get; private set; }
    public ED_Animator m_edm { get; private set; }
    public CardUtil() : base(){}

    static public new CardUtil Builder(UnityEngine.Object uobj)
    {
        return Builder<CardUtil>(uobj);
    }

    override public void InitComp(Component comp, Action cfDestroy, Action cfShow, Action cfHide)
    {
        base.InitComp(comp, cfDestroy, cfShow, cfHide);
        m_csEle = this.m_comp as PrefabElement;
        SetPosition(0,10000,0);
        if (!m_csEle)
            return;
        m_csd = m_csEle.GetGobjElement("card_scene_01");
        var anim = m_csEle.GetComponent4Element<Animator>("card_animation_skin");
        m_edm = ED_Animator.Builder(anim);
        m_edm.InitComp(anim,null,null,null);
        this.InitPoints(10);
    }


    Dictionary<int, Transform> m_points = new Dictionary<int, Transform>();
    Dictionary<int, Transform> m_target = new Dictionary<int, Transform>();
    public void InitPoints(int tagetLens)
    {
        string _name = null;
        Transform _trsf = null;
        for (int i = 1; i < tagetLens + 1; i++)
        {
            _name = "Point"+ i.ToString().PadLeft(3,'0');
            _trsf = m_csEle.GetTrsfElement(_name);
            if (_trsf)
            {
                m_points.Add(i, _trsf);
                _trsf.gameObject.SetActive(false);
                GameObject obj = GameObject.Instantiate(m_csd);
                obj.transform.SetParent(_trsf);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
                obj.SetActive(true);
            }
        }
    }

    public void BindTaget(int idx , Transform tras)
    {
        if (m_points[idx] != null)
        {
            tras.gameObject.SetActive(false);
            tras.SetParent(m_points[idx]);
            m_target[idx] = tras;
        }
    }

    public void PlayAnim(string animName, float v1, float v2, float v3, float v4, float v5, Core.DF_OnInt overCallback)
    {
        m_edm.AddAnimationEvent(animName, v3, 3);
        m_edm.AddAnimationEvent(animName, v4, 4);
        m_edm.AddAnimationEvent(animName, v5, 5);
        m_edm.AddAnimationEvent(animName, v1, 0);
        m_edm.AddAnimationEvent(animName, v2, 1);
        m_edm.PlayAnimator(animName, true, 1, 2, overCallback);
        m_edm.m_curAni.Update(0);
    }
    public void PointActive(int index, bool isShow)
    {
        if (m_points.ContainsKey(index))
        {
            m_points[index].gameObject.SetActive(isShow);
        }
    }

    public void TargetActive(int index, bool isShow)
    {
        if (m_target.ContainsKey(index))
        {
            m_target[index].gameObject.SetActive(isShow);
        }

    }

    public void BindEffect(int index, Transform tras)
    {
        if (m_points[index] != null)
        {
            tras.SetParent(m_points[index]); 
        }

    }

    // 清空数据,销毁时调用
    public void Dispose()
    {
        m_csEle = null;
        m_points.Clear();
    }

    override protected void On_Destroy(GobjLifeListener obj)
    {
        Dispose();
        base.On_Destroy(obj);
    }
}