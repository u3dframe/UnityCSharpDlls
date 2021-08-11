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
    private class AudioInfo
    {
        private string m_abName = null;
        private string m_assetName = null;
        private int m_nTagType = 0;
        public AssetInfo m_ainfo = null;
        public string m_key { get; private set; }
        public AudioInfo(string abName,string assetName,int tagType)
        {
            this.m_abName = abName;
            this.m_assetName = assetName;
            this.m_nTagType = tagType;
            this.m_key = string.Format("{0}@@{1}", abName, assetName);
            this.m_ainfo = AssetInfo.abMgr.GetAssetInfo<AudioClip>(abName, assetName);
        }

        public void Clear(bool isUnload)
        {
            AssetInfo _tmp = this.m_ainfo;
            this.m_ainfo = null;
            if (isUnload && _tmp != null)
                _tmp.UnloadAsset();
        }

        public bool isAutoPlay()
        {
            return this.m_nTagType == 1 || this.m_nTagType == 3;
        }

        public bool isCanUnload()
        {
            return this.m_nTagType == 2 || this.m_nTagType == 3;
        }

        public bool isHasObj()
        {
            return this.m_ainfo != null && this.m_ainfo.isHasObj;
        }

        public AudioClip GetClip()
        {
            return this.m_ainfo?.GetObject<AudioClip>();
        }

        public void SetTagType(int tagType)
        {
            this.m_nTagType = tagType;
        }
    }

    private bool m_isMusic = false;
    private float m_volume = 1f;
    private AudioSource m_audio = null;
    private int m_playState = 0;
    private int m_notiyState = 1;
    public bool m_isBreak = true;

    private string _key_loading = null;
    private AudioInfo m_curAsset = null;

    public float m_timeDuration { get; private set; }
    private DF_ToLoadAdoClip m_cfLoad = null;
    private ListDict<AudioInfo> m_assets = new ListDict<AudioInfo>(true);

    private AudioData() { }
    private AudioData(GameObject gobj, bool isNew, bool isMusic, float volume, bool playOnAwake)
    {
        Init(gobj, isNew, isMusic, volume, playOnAwake);
    }

    AudioData Init(GameObject gobj, bool isNew, bool isMusic, float volume, bool playOnAwake)
    {
        GobjLifeListener glife = GobjLifeListener.Get(gobj);
        glife.OnlyOnceCallDetroy(this.OnNotifyDestry);

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

    public AudioData SyncSetting(int notiyState, DF_ToLoadAdoClip cfLoad,bool isBreak = true)
    {
        this.m_notiyState = notiyState;
        this.m_cfLoad = cfLoad;
        this.m_isBreak = isBreak;
        return this;
    }

    public AudioData LoadAsset(string abName, string assetName,int tagType)
    {
        if (this.m_audio)
        {
            string _key = string.Format("{0}@@{1}", abName, assetName);
            this._key_loading = _key;

            AudioInfo _info = this.m_assets.Get(_key);
            if(_info != null)
            {
                if(_info.isHasObj())
                    this.OnLoadAsset(_info.m_ainfo);
                return this;
            }
            AssetInfo _ainfo = null;
            if (this.m_cfLoad != null)
            {
                this.m_cfLoad(abName, assetName, OnLoadAdoClip);
                _ainfo = AssetInfo.abMgr.GetAssetInfo<AudioClip>(abName, assetName);
            }
            else
            {
                _ainfo = AssetInfo.abMgr.LoadAsset<AudioClip>(abName, assetName, OnLoadAsset);
            }

            if(_ainfo != null)
            {
                _info = new AudioInfo(abName, assetName, tagType);
                m_assets.Add(_key, _info);
            }
        }
        return this;
    }

    public AudioData LoadAsset(string abName, int tagType)
    {
        string assetName = UGameFile.GetFileNameNoSuffix(abName);
        return LoadAsset(abName, assetName, tagType);
    }

    void OnLoadAsset(AssetBase asset)
    {
        if (asset == null)
            return;

        this.OnLoadAdoClip(null);
    }

    void OnLoadAdoClip(AudioClip clip)
    {
        AudioInfo _last = this.m_curAsset;
        AudioInfo _info = this.m_assets.Get(this._key_loading);
        this.m_curAsset = _info;

        if(_last != null && _last != _info)
        {
            if (_last.isCanUnload())
            {
                _last.Clear(true);
                this.m_assets.Remove(this._key_loading);
            }
        }

        if (_info != null && _info.isAutoPlay())
            this.PlayClip();
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

    private void Play()
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

    public void PlayClip(bool isBreak)
    {
        if (this.m_curAsset == null)
            return;

        AudioClip _clip = this.m_curAsset.GetClip();
        this.PlayClip(_clip, isBreak);
    }

    public void PlayClip()
    {
        this.PlayClip(this.m_isBreak);
    }

    public void RePlay()
    {
        this.Stop();
        this.PlayClip();
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

    void ClearCurrAssets(bool isUnload)
    {
        var _tmp = this.m_curAsset;
        this.m_curAsset = null;
        if (_tmp != null)
            _tmp.Clear(isUnload);
    }

    public void ClearAssets()
    {
        this.ClearCurrAssets(false);
        var infos = new System.Collections.Generic.List<AudioInfo>(this.m_assets.m_list);
        this.m_assets.Clear();
        int lens = infos.Count;
        AudioInfo _tmp = null;
        for (int i = 0; i < lens; i++)
        {
            _tmp = infos[i];
            if (_tmp != null)
                _tmp.Clear(true);
        }
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
