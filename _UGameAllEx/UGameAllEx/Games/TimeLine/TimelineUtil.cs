﻿using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using System;

public class TimelineUtil : ED_Animator
{
    public PrefabElement m_csEle { get; private set; } // 当前对象
    public PlayableDirector m_director { get; private set; }                                  // 当前组件（物体上的播放组件）
    public PlayableAsset m_defAsset { get; private set; }                                        // 资源（编辑的playable文件）
    public PlayableAsset m_asset { get; private set; }                                        // 资源（编辑的playable文件）
    private Dictionary<string, PlayableBinding> bindings;                // PlayableAsset下的所有binding
    private Dictionary<string, Dictionary<string, PlayableAsset>> clips; // binding里的所有clip

    public PostProcessLayer m_pLayer{ get;private set; }
    public Transform caster { get; private set; }
    //结束
    float currDurationTime;    //当前播放时间
    float timelineDurationTime;//总时间
    Action overCallback;


    public TimelineUtil() : base()
    {
    }

    static public new TimelineUtil Builder(UnityEngine.Object uobj)
    {
        return Builder<TimelineUtil>(uobj);
    }

    override public void InitComp(Component comp, Action cfDestroy, Action cfShow, Action cfHide)
    {
        base.InitComp(comp, cfDestroy, cfShow, cfHide);
        m_director = UtilityHelper.GetOrAddPlayableDirector(this.m_gobj);
        if(m_director != null)
            m_defAsset = this.m_director.playableAsset;
        PrefabElement csEle = this.m_comp as PrefabElement;
        if (!csEle)
            return;
        m_csEle = csEle;
        caster = csEle.GetTrsfElement("Caster");
        m_pLayer = csEle.GetComponent4Element<PostProcessLayer>("Camera");
        
        this.SetPosition(0,10000,0);
        this.InitTagets(10);
        this.InitPAsset(m_defAsset);
    }

    public void Init(Action callback,PlayableAsset curAsset = null)
    {
        //初始数据保存
        this.overCallback = callback;
        InitPAsset(curAsset);
    }

    void InitPAsset(PlayableAsset curAsset)
    {
        if(curAsset == null || curAsset == this.m_asset)
            return;
        
        //this.m_director.stopped += PlayOver;//不使用结束回调，正在播放时结束游戏会产生回调，如果Lua侧在回调中有操作场景物体，会由于CS和Lua
        //this.m_director.played += PlayBegin;
        PlayableAsset _asset = this.m_defAsset;
        if (curAsset)
        {
            _asset = curAsset;
            this.m_director.playableAsset = _asset; // 设置播放的资源
        }
        this.m_asset = _asset; // 资源（playable文件）

        bindings = new Dictionary<string, PlayableBinding>();   // PlayableAsset下的所有binding信息
        clips = new Dictionary<string, Dictionary<string, PlayableAsset>>(); // binding里的所有clip
        //PlayableAsset.outputs中储存了所有的轨道信息
        string _name = null;
        TrackAsset _srcObj = null;
        Dictionary<string, PlayableAsset> _dic = null;
        foreach (var o in _asset.outputs)//每一个binding，是轨道资源和需要动画的模型之间的链接关系
        {
            _name = o.streamName;
            // 每一个binding的名字和binding绑定
            bindings.Add(_name, o);
            // 每个binding下的对象都是TrackAsset类型
            _srcObj = o.sourceObject as TrackAsset;
            if (_srcObj == null)
                continue;
            
            // 获得每一个轨道下的动画片段
            var clipList = _srcObj.GetClips();
            foreach (var c in clipList) // 存入clips
            {
                if(!clips.TryGetValue(_name,out _dic))
                {
                    _dic = new Dictionary<string, PlayableAsset>();
                    clips.Add(_name,_dic);
                }
                
                if (!_dic.ContainsKey(c.displayName))
                    _dic.Add(c.displayName, c.asset as PlayableAsset);
            }
        }
    }

    // 动画和模型进行绑定
    public void SetBinding(string trackName, UnityEngine.Object o)
    {
        if (o != null && bindings.ContainsKey(trackName))
        {
            UtilityHelper.Get<Animator>(o,true);
            m_director.SetGenericBinding(bindings[trackName].sourceObject, o);
        }
    }

    public bool CanBinding(string trackName)
    {
        return bindings.ContainsKey(trackName);
    }

    // 获得动画轨道
    public T GetTrack<T>(string trackName) where T : TrackAsset
    {
        if (bindings.ContainsKey(trackName))
        {
            return bindings[trackName].sourceObject as T;
        }
        return null;
    }

    // 获得动画片段(不建议通过代码进行赋值修改)
    //--如果要代码对Clip赋值，赋值的时候得创建ExposedReference类型的结构体作为中转，对结构体的defaultValue进行赋值--
    //--如果要代码修改Clip参数，必须解析playableGraph,否则修改的目标不是实际游戏中的物体，且playableGraph只在运行时才会创建，未运行时值未Null--
    public T GetClip<T>(string trackName, string clipName) where T : PlayableAsset
    {
        if (clips.ContainsKey(trackName))
        {
            var track = clips[trackName];
            if (track.ContainsKey(clipName))
                return track[clipName] as T;
        }
        Debug.LogError("GetClip Error, Track does not contain clip, trackName: " + trackName + ", clipName: " + clipName);
        return null;
    }

    public void SetClip(CharacterControllerEx ch, string name)
    {
        if (ch == null || ch.m_animator == null || ch.m_animator.runtimeAnimatorController == null || string.IsNullOrEmpty(name))
            return;

        TrackAsset track = GetTrack<TrackAsset>(name);
        if (track == null)
            return;
        
        RuntimeAnimatorController run = ch.m_animator.runtimeAnimatorController;
        AnimationClip[] clips = run.animationClips;
        double tims;
        //循环轨道下所有动画片段
        foreach (TimelineClip item in track.GetClips())
        {
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] && item.animationClip != null && string.Equals(clips[i].name, item.animationClip.name))
                {
                    var p = item.asset as AnimationPlayableAsset;
                    p.clip = clips[i];
                    // m_director.playableGraph.Stop();
                    tims = m_director.time;
                    m_director.RebuildGraph();
                    m_director.time = tims;
                    //m_director.RebindPlayableGraphOutputs();
                    m_director.playableGraph.Play();
                    break;
                }
            }
        }
    }

    // 播放动画
    public void Play()
    {
        m_director.Play();
        PlayBegin(m_director);
    }

    public void Pause()
    {
        m_director.Pause();
    }

    public void Stop()
    {
        this.PlayOver(m_director);
    }

    public void Resume()
    {
        m_director.Resume();
    }

    public void SetDirectorPosition(float x, float y, float z)
    {
        m_director.transform.position = new Vector3(x, y, z);
    }

    public void SetTimeUpdateModeType(bool isGameTime)
    {
        m_director.timeUpdateMode = isGameTime ? DirectorUpdateMode.GameTime : DirectorUpdateMode.UnscaledGameTime;
    }

    // 清空数据,销毁时调用
    public void Dispose()
    {
        m_csEle = null;
        m_director = null;
        m_defAsset = null;
        m_asset = null;
        m_pLayer = null;
        caster = null;

        m_tages.Clear();
        clips = null;
        bindings = null;
        overCallback = null;
    }

    #region 播放结束计时
    //播放开始回调
    private void PlayBegin(PlayableDirector playable)
    {
        currDurationTime = 0;
        timelineDurationTime = (float)m_director.duration;
        this.StartUpdate();
    }

    //播放结束回调
    private void PlayOver(PlayableDirector playable)
    {
        if (m_director)
        {
            //关闭Update
            this.StopUpdate();
            //停止
            m_director.Stop();
            //通知Lua动画播放已结束，关闭
            // this.overCallback?.Invoke();
            if(this.overCallback != null)
                this.overCallback();
        }
    }

    override public void OnUpdate(float dt, float unscaledDt)
    {
        currDurationTime += dt;
        if (currDurationTime >= timelineDurationTime)
        {
            this.PlayOver(m_director);
        }
    }

    public GameObject SetCaster(GameObject obj)
    {
        GameObject a = GameObject.Instantiate(obj, caster);
        GHelper.SetLayerAll(a, "CG");
        a.transform.localPosition = Vector3.zero;
        a.transform.localRotation = Quaternion.identity;
        CharacterControllerEx ex = CharacterControllerEx.Get(a, false);
        SetBinding("Caster", ex.m_animator);
        return a;
    }
    #endregion

    Dictionary<string, Transform> m_tages = new Dictionary<string, Transform>();
    public void InitTagets(int tagetLens)
    {
        string _name = null;
        Transform _trsf = null;
        for (int i = 1; i < tagetLens + 1; i++)
        {
            _name = string.Format("Target{0}", i);
            _trsf = m_csEle.GetTrsfElement(_name);
            if (_trsf)
                m_tages.Add(_name, _trsf);
        }
    }

    public GameObject SetTarget(GameObject gobj, int index)
    {
        string _name = string.Format("Target{0}", index);
        Transform _trsf = null;
        if (!m_tages.TryGetValue(_name, out _trsf))
            return null;

        GameObject obj = GameObject.Instantiate(gobj, _trsf);
        GHelper.SetLayerAll(obj, "CG");
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        CharacterControllerEx ex = CharacterControllerEx.Get(obj);
        GameObject hd = ex.GetGobjElement("heads");
        SetBinding(_name, ex.m_animator);
        SetClip(ex, _name);
        return hd;
    }
    
    public void SetTargetActive(int index, bool isActive)
    {
        string _name = string.Format("Target{0}", index);
        Transform _trsf = null;
        if (!m_tages.TryGetValue(_name, out _trsf))
            return;
        _trsf.gameObject.SetActive(isActive);
    }

    override protected void On_Destroy(GobjLifeListener obj)
    {
        Dispose();
        base.On_Destroy(obj);
    }
}