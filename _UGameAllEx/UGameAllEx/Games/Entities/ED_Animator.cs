using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 类名 : Animator 数据脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2021-01-08 12:33
/// 功能 : 
/// </summary>
public class ED_Animator : Core.Kernel.Beans.ED_Comp
{
    static public new ED_Animator Builder(UnityEngine.Object uobj)
    {
        return Builder<ED_Animator>(uobj);
    }

    public Animator m_curAni { get; private set; }
    protected AnimationClip[] m_clips = null;
    private Dictionary<string,bool> m_dic = null;

    
    public ED_Animator() : base()
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
        this.m_curAni = this.m_gobj.GetComponent<Animator>();

        if(this.m_curAni)
        {
            RuntimeAnimatorController run = this.m_curAni.runtimeAnimatorController;
            if(run)
                this.m_clips = run.animationClips;
        }
    }

    override protected void On_Destroy(GobjLifeListener obj)
    {
        this.m_curAni = null;
        this.m_dic = null;
        this.CleanAllEvent(true);

        base.On_Destroy(obj);
    }

    private void CleanAllEvent(bool isDiscard)
    {
        if(UtilityHelper.IsNullOrEmpty(this.m_clips))
            return;

        for (int i = 0; i < this.m_clips.Length; i++)
            this.m_clips[i].events = default;

        if(isDiscard)
            this.m_clips = null;
    }

    private bool IsInEvents(AnimationClip clip,string key,float normarl)
    {
        if(clip == null)
            return true;
        AnimationEvent[] events = clip.events;
        if(string.IsNullOrEmpty(key))
            return true;
        
        if(UtilityHelper.IsNullOrEmpty(events))
            return false;
        
        AnimationEvent _evt = null;
        string _cur_key = null;
        float _cur_normarl = 1;
        for (int i = 0; i < events.Length; i++)
        {
            _evt = events[i];
            if(_evt == null)
                continue;
            _cur_key = string.Format("[{0}]_[{1}]_[{2}]",clip.name,_evt.functionName,_evt.intParameter);
            if(key.StartsWith(_cur_key))
            {
                _cur_normarl = _evt.time - normarl;
                if(Mathf.Abs(_cur_normarl) <= 0.01f)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 添加动画事件
    /// </summary>
    public void AddAnimationEvent(string clipName,float normarl,string func,int pval)
    {
        if(UtilityHelper.IsNullOrEmpty(this.m_clips) || string.IsNullOrEmpty(clipName))
            return;

        normarl = Mathf.Clamp01(normarl);

        string _key = string.Format("[{0}]_[{1}]_[{2}]_[{3}]",clipName,func,pval,normarl);
        if(this.m_dic == null)
            this.m_dic = new Dictionary<string, bool>();
        if(this.m_dic.ContainsKey(_key))
            return;
        this.m_dic.Add(_key,true);

        AnimationClip[] _clips = this.m_clips;
        AnimationClip _clip = null;
        for (int i = 0; i < _clips.Length; i++)
        {
            _clip = _clips[i];
            if(_clip == null)
                continue;

            if (_clip.name.Equals(clipName,StringComparison.OrdinalIgnoreCase))
            {
                normarl = normarl * _clip.length;
                if(IsInEvents(_clip,_key,normarl))
                    continue;

                AnimationEvent _event = new AnimationEvent();
                _event.functionName = func;
                _event.intParameter = pval;
                _event.time = normarl;
                _clip.AddEvent(_event);
                break;
            }
        }
        this.m_curAni.Rebind();
    }

    public void PlayAnimator(string stateName,bool isOrder,float speed,int unique,Action callFinished)
    {
        if(UtilityHelper.IsNullOrEmpty(this.m_clips) || string.IsNullOrEmpty(stateName))
            return;
        
        this.AddAnimationEvent(stateName,1f,"OnCallAnimEnd",unique);
        this.m_compGLife.InitAnimEnd(unique,callFinished,true);
        if(!isOrder)
            this.m_curAni.StartPlayback();
        this.m_curAni.speed = (isOrder ? 1 : -1) * speed;
        this.m_curAni.Play(stateName,0,isOrder ? 0 : 1);
        // this.m_curAni.StopPlayback();
    }

    public void ReAnimator()
    {
        if(!this.m_curAni || !this.m_curAni.isActiveAndEnabled)
            return;
        this.m_curAni.Play("None");
    }
}
