using UnityEngine;
using Core;
using Core.Kernel;

/// <summary>
/// 类名 : AudioSource 数据
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2019-11-17 09:53
/// 功能 : 
/// </summary>
[System.Serializable]
public class AudioData
{
    private bool m_isMusic = false;
    private float m_volume = 1f;
    private AudioSource m_audio = null;
    private int m_playState = 0;
    private int m_notiyState = 1;
    public bool m_isBreak = false;

    private AssetInfo m_ainfo = null;
    private string m_abName = null;
    private string m_assetName = null;

    public float m_timeDuration { get; private set; }
    public DF_ToLoadAdo m_cfLoad = null;

    private AudioData() { }
    private AudioData(GameObject gobj, bool isNew, bool isMusic, float volume, bool playOnAwake)
    {
        Init(gobj, isNew, isMusic, volume, playOnAwake);
    }

    AudioData Init(GameObject gobj, bool isNew, bool isMusic, float volume, bool playOnAwake)
    {
        GobjLifeListener glife = GobjLifeListener.Get(gobj);
        glife.AddOnlyOnceDCall(this.OnNotifyDestry);

        if (!isNew)
            this.m_audio = UtilityHelper.Get<AudioSource>(gobj, true);

        if (!this.m_audio)
            this.m_audio = UtilityHelper.Add<AudioSource>(gobj);

        this.m_timeDuration = 1f;
        this.m_isMusic = isMusic;
        this.m_volume = volume;
        this.m_audio.loop = isMusic;
        this.m_audio.volume = volume;
        this.m_audio.playOnAwake = playOnAwake;

        Messenger.AddListener<bool, int>(MsgConst.MSound_State, this.OnNotifyState);
        Messenger.AddListener<bool, float>(MsgConst.MSound_Volume, this.OnNotifyVolume);
        return this;
    }

    public AudioData LoadAsset(string abName, string assetName)
    {
        if (this.m_audio)
        {
            this.m_abName = abName;
            this.m_assetName = assetName;
            this.ClearAssets();
            if (this.m_cfLoad != null)
            {
                this.m_cfLoad(abName, assetName, OnLoadAdoClip);
            }
            else
            {
                this.m_ainfo = AssetInfo.abMgr.LoadAsset<AudioClip>(abName, assetName, OnLoadAsset);
            }
        }

        return this;
    }

    public AudioData LoadAsset(string abName)
    {
        string assetName = UGameFile.GetFileNameNoSuffix(abName);
        return LoadAsset(abName, assetName);
    }

    void OnLoadAsset(AssetBase asset)
    {
        if (asset == null)
        {
            return;
        }

        AudioClip clip = this.m_ainfo.GetObject<AudioClip>();
        this.PlayClip(clip, this.m_isBreak);
    }

    void OnLoadAdoClip(AudioClip clip)
    {
        this.m_ainfo = AssetInfo.abMgr.GetAssetInfo<AudioClip>(this.m_abName, this.m_assetName);
        this.PlayClip(clip, this.m_isBreak);
    }

    void OnNotifyDestry(GobjLifeListener gLife)
    {
        this.ClearAll();
    }

    void OnNotifyState(bool isMusic, int state)
    {
        if (this.m_isMusic != isMusic)
            return;

        this.m_notiyState = state;
        switch (state)
        {
            case 1:
                this.Play();
                break;
            case 2:
                this.Pause();
                break;
            default:
                this.Stop();
                break;
        }
    }

    void OnNotifyVolume(bool isMusic, float volume)
    {
        if (this.m_isMusic != isMusic)
            return;

        this.SetVolume(volume);
    }

    public void SetVolume(float volume)
    {
        if (this.m_audio == null || this.m_volume != volume)
        {
            return;
        }
        this.m_volume = volume;
        this.m_audio.volume = volume;
    }

    public void Play()
    {
        if (this.m_notiyState != 1)
        {
            return;
        }

        if (this.m_audio == null || this.m_audio.isPlaying)
        {
            return;
        }
        this.m_playState = 1;
        this.m_audio.Play();
    }

    public void Stop()
    {
        this.m_playState = 0;
        if (this.m_audio == null || !this.m_audio.isPlaying)
        {
            return;
        }
        this.m_audio.Stop();
    }

    public void Pause()
    {
        if (this.m_audio == null || !this.m_audio.isPlaying)
        {
            return;
        }
        this.m_playState = 2;
        this.m_audio.Pause();
    }

    public void PlayClip(AudioClip clip, bool isBreak)
    {
        if (this.m_audio == null)
        {
            return;
        }

        if (this.m_playState != 0 && !isBreak && this.m_audio.isPlaying)
        {
            return;
        }

        this.Stop();

        this.m_audio.clip = clip;
        this.m_timeDuration = 1f;
        if (clip == null)
            return;
        this.m_timeDuration = clip.length * 2;
        this.Play();
    }

    public void RePlay()
    {
        this.Stop();
        this.Play();
    }

    void ClearAll()
    {
        Messenger.RemoveListener<bool, int>(MsgConst.MSound_State, this.OnNotifyState);
        Messenger.RemoveListener<bool, float>(MsgConst.MSound_Volume, this.OnNotifyVolume);
        this.m_audio = null;
        this.m_playState = 0;
        this.m_isBreak = false;
        this.ClearAssets();
    }

    void ClearAssets()
    {
        var _tmp = this.m_ainfo;
        this.m_ainfo = null;
        if (_tmp != null)
            _tmp.UnLoadAsset();
    }

    static public AudioData Builder(GameObject gobj, bool isNew, bool isMusic, float volume, bool playOnAwake)
    {
        if (!gobj)
            return null;
        return new AudioData(gobj, isNew, isMusic, volume, playOnAwake);
    }

    static public AudioData Builder(GameObject gobj, bool isNew, bool isMusic, float volume)
    {
        return Builder(gobj, isNew, isMusic, volume, false);
    }

    static public AudioData BuilderSound(GameObject gobj, bool isNew, float volume)
    {
        return Builder(gobj, isNew, false, volume);
    }
}
