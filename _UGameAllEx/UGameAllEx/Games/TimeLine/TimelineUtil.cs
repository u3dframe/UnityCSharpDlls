using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;


public class TimelineUtil : IUpdate
{
    public string name;                                                 // 标签
    public PlayableAsset asset;                                         // 资源（编辑的playable文件）
    public PlayableDirector director;                                   // 当前组件（物体上的播放组件）
    public Dictionary<string, PlayableBinding> bindings;                // PlayableAsset下的所有binding
    public Dictionary<string, Dictionary<string, PlayableAsset>> clips; // binding里的所有clip

    //结束
    bool isUpdate = false;
    float currDurationTime;    //当前播放时间
    float timelineDurationTime;//总时间
    System.Action<PlayableDirector> overCallback;

    public void Init(string path, PlayableDirector director, PlayableAsset asset, System.Action<PlayableDirector> callback)
    {
        //初始数据保存
        this.name = path;               // 标签
        this.asset = asset;             // 资源（playable文件）
        this.director = director;       // 当前组件
        this.overCallback = callback;
        GHelper.SetLayerAll(director.gameObject, "CG");
        //this.director.stopped += PlayOver;//不使用结束回调，正在播放时结束游戏会产生回调，如果Lua侧在回调中有操作场景物体，会由于CS和Lua
        //this.director.played += PlayBegin;
        PlayableAsset _asset = this.director.playableAsset;
        if (asset)
        {
            _asset = asset;
            this.director.playableAsset = _asset; // 设置播放的资源
        }
        bindings = new Dictionary<string, PlayableBinding>();   // PlayableAsset下的所有binding信息
        clips = new Dictionary<string, Dictionary<string, PlayableAsset>>(); // binding里的所有clip
        //PlayableAsset.outputs中储存了所有的轨道信息
        foreach (var o in _asset.outputs)//每一个binding，是轨道资源和需要动画的模型之间的链接关系
        {
            var trackname = o.streamName;
            //Debug.Log(trackname);
            // 每一个binding的名字和binding绑定
            bindings.Add(trackname, o);
            // 每个binding下的对象都是TrackAsset类型
            var track = o.sourceObject as TrackAsset;
            if (track == null) continue;
            // 获得每一个轨道下的动画片段
            var clipList = track.GetClips(); 
            foreach (var c in clipList) // 存入clips
            {
                if (!clips.ContainsKey(trackname))
                {
                    clips[trackname] = new Dictionary<string, PlayableAsset>();
                }
                var name2clips = clips[trackname];
                if (!name2clips.ContainsKey(c.displayName))
                {
                    name2clips.Add(c.displayName, c.asset as PlayableAsset);
                }
            }
        }
    }
    // 动画和模型进行绑定
    public void SetBinding(string trackName, Object o)
    {
        if (bindings.ContainsKey(trackName) && o != null)
        {
            GameObject game = o as GameObject;
            //确保绑定的组件存在Animator组件，否则无法播放
            if (game)
            {
               
                Animator an = game.GetComponent<Animator>();
                if (an == null)
                {
                    game.AddComponent<Animator>();
                }
            }
            director.SetGenericBinding(bindings[trackName].sourceObject, o);
        }
    }

    public bool CanBinding(string trackName)
    {
        return bindings.ContainsKey(trackName);
    }

    // 获得动画轨道
    public T GetTrack<T>(string trackName) where T : TrackAsset
    {
        return bindings[trackName].sourceObject as T;
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
            {
                return track[clipName] as T;
            }
            else
            {
                Debug.LogError("GetClip Error, Track does not contain clip, trackName: " + trackName + ", clipName: " + clipName);
            }
        }
        else
        {
            Debug.LogError("GetClip Error, Track does not contain clip, trackName: " + trackName + ", clipName: " + clipName);
        }
        return null;
    }

    // 播放动画
    public void Play()
    {       
        director.Play();
        PlayBegin(director);
    }
    public void Pause()
    {
        director.Pause();
    }
    public void Stop()
    {
        this.PlayOver(director);
    }
    public void Resume()
    {
        director.Resume();
    }

    public void SetDirectorPosition(float x, float y, float z)
    {
        director.transform.position = new Vector3(x, y, z);
    }

    public void SetTimeUpdateModeType(bool isGameTime)
    {
        director.timeUpdateMode = isGameTime ? DirectorUpdateMode.GameTime : DirectorUpdateMode.UnscaledGameTime;
    }

    // 清空数据,销毁时调用
    public void Dispose()
    {
        name = null;
        asset = null;
        director = null;
        clips.Clear();
        bindings.Clear();
        overCallback = null;
    }

    #region 播放结束计时
    //播放开始回调
    private void PlayBegin(PlayableDirector playable)
    {
        isUpdate = true;
        currDurationTime = 0;
        timelineDurationTime = (float)director.duration;
        GameMgr.RegisterUpdate(this);
    }

    //播放结束回调
    private void PlayOver(PlayableDirector playable)
    {
        isUpdate = false;
        if (director)
        {
            //停止
            director.Stop();
            //关闭Update
            //通知Lua动画播放已结束，关闭
            this.overCallback?.Invoke(director);
        }
        GameMgr.DiscardUpdate(this);
    }
    public bool IsOnUpdate()
    {
        return isUpdate;
    }

    public void OnUpdate(float dt, float unscaledDt)
    {
        currDurationTime += dt;
        if (currDurationTime >= timelineDurationTime)
        {
            this.PlayOver(director);
        }
    }
    #endregion
}