using UnityEngine;
using System.Collections;
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
        private int m_nTagType = 0; // [0=预加载 , 1=加载完毕后自动播放 , 2=被下一个资源替换是，自身可销毁 , 3=自动播放，并且自身可销毁]
        private AudioClip m_clip = null;
        public AssetInfo m_ainfo { get; private set; }
        public string m_key { get; private set; }
        public AudioInfo(string abName, string assetName, int tagType)
        {
            this.m_abName = abName;
            this.m_assetName = assetName;
            this.m_nTagType = tagType;
            this.m_key = string.Format("{0}@@{1}", abName, assetName);
            this.Init();
        }

        public void Init()
        {
            this.m_ainfo = AssetInfo.abMgr.GetAssetInfo<AudioClip>(this.m_abName, this.m_assetName);
        }

        public void Clear(bool isUnload)
        {
            AssetInfo _tmp = this.m_ainfo;
            this.m_ainfo = null;
            bool _isAinfo = (_tmp != null);
            AudioClip _clip = this.m_clip;
            this.m_clip = null;
            if (!_isAinfo && _clip != null)
                UGameFile.UnLoadOne(_clip);

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
            if (this.m_clip != null)
                return true;
            return this.m_ainfo != null && this.m_ainfo.isHasObj;
        }

        public AudioClip GetClip()
        {
            if (this.m_clip != null)
                return this.m_clip;
            return this.m_ainfo?.GetObject<AudioClip>();
        }

        public void SetClip(AudioClip clip)
        {
            this.m_clip = clip;
        }

        public void SetTagType(int tagType)
        {
            this.m_nTagType = tagType;
        }
    }

    static public AudioData Builder(GameObject gobj, bool isNew, int nType, float volume, bool playOnAwake)
    {
        if (!gobj)
            return null;
        return new AudioData(gobj, isNew, nType, volume, playOnAwake);
    }

    static public AudioData Builder(GameObject gobj, bool isNew, int nType, float volume)
    {
        return Builder(gobj, isNew, nType, volume, false);
    }

    static public AudioData BuilderSound(GameObject gobj, bool isNew, float volume)
    {
        return Builder(gobj, isNew,0, volume);
    }

    static long _nCursor = 0;

    public int m_nType = 0; // 0 = sound , 1 = music ,2 = fight sound
    private float m_volume = 1f;
    private AudioSource m_audio = null;
    private int m_playState = 0;
    private int m_notiyState = 1;
    public bool m_isBreak = true;
    public bool m_isAutoStop = false;
    private string m_lastClipName = null;

    private string _key_loading = null;
    private AudioInfo m_curAsset = null;
    private int m_curTagType = 0;

    public long m_currCursor { get; private set; }
    public float m_timeDuration { get; private set; }
    private float m_timeRemainder = 0, m_speed = 1;
    private GobjLifeListener m_glife = null;
    private DF_ToLoadAdoClip m_cfLoad = null;
    private ListDict<AudioInfo> m_assets = new ListDict<AudioInfo>(true);

    private AudioData(GameObject gobj, bool isNew, int nType, float volume, bool playOnAwake)
    {
        this.m_currCursor = ++_nCursor;
        Init(gobj, isNew, nType, volume, playOnAwake);
    }

    AudioData Init(GameObject gobj, bool isNew, int nType, float volume, bool playOnAwake)
    {
        m_glife = GobjLifeListener.Get(gobj);
        m_glife.OnlyOnceCallDetroy(this.OnNotifyDestry);

        if (!isNew)
            this.m_audio = UtilityHelper.Get<AudioSource>(gobj, true);

        if (!this.m_audio)
            this.m_audio = UtilityHelper.Add<AudioSource>(gobj,false);

        this.m_nType = nType;
        this.m_volume = volume;
        this.m_audio.loop = (nType == 1);
        this.m_audio.volume = volume;
        this.m_audio.playOnAwake = playOnAwake;

        Messenger.AddListener<int, int>(MsgConst.MSound_State, this.OnNotifyState);
        Messenger.AddListener<int, float>(MsgConst.MSound_Volume, this.OnNotifyVolume);
        return this;
    }

    public AudioData SyncSetting(int notiyState, DF_ToLoadAdoClip cfLoad, bool isBreak = true)
    {
        this.m_notiyState = notiyState;
        this.m_cfLoad = cfLoad;
        this.m_isBreak = isBreak;
        return this;
    }

    bool isAutoPlay()
    {
        return this.m_curTagType == 1 || this.m_curTagType == 3;
    }

    public AudioData LoadAsset(string abName, string assetName, int tagType)
    {
        if (this.m_audio)
        {
            string _key = string.Format("{0}@@{1}", abName, assetName);
            this._key_loading = _key;
            this.m_curTagType = tagType;

            AudioInfo _info = this.m_assets.Get(_key);
            if (_info != null)
            {
                if (_info.isHasObj())
                {
                    if (_info.m_ainfo != null)
                        this.OnLoadAsset(_info.m_ainfo);
                    else
                        this.OnLoadAdoClip(null);
                }
                return this;
            }
            _info = new AudioInfo(abName, assetName, tagType);
            m_assets.Add(_key, _info);
            if (this.m_cfLoad != null)
            {
                this.m_cfLoad(abName, assetName, OnLoadAdoClip);
            }
            else
            {
                AssetInfo.abMgr.LoadAsset<AudioClip>(abName, assetName, OnLoadAsset);
            }
            _info.Init();
        }
        return this;
    }

    public AudioData LoadAsset(string abName, int tagType)
    {
        string assetName = UGameFile.GetFileNameNoSuffix(abName);
        return LoadAsset(abName, assetName, tagType);
    }

    public bool UnLoadAsset(string abName, string assetName)
    {
        string _key = string.Format("{0}@@{1}", abName, assetName);
        AudioInfo _info = this.m_assets.Remove4Get(_key);
        bool _isHas = _info != null;
        if (_isHas)
            _info.Clear(true);
        return _isHas;
    }

    public bool UnLoadAsset(string abName)
    {
        string assetName = UGameFile.GetFileNameNoSuffix(abName);
        return this.UnLoadAsset(abName, assetName);
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
        string _keyLast = null;
        bool _lastUnload = false;
        string _clipName = null;
        if (_last != null && _last != _info)
        {
            _keyLast = _last.m_key;
            _lastUnload = _last.isCanUnload();
            if (_lastUnload)
            {
                this.m_assets.Remove(_keyLast);
                _last.Clear(true);
            }
        }

        if (clip != null)
        {
            _clipName = clip.name;
            string _key = string.Format("audios/{0}.ado@@{0}", _clipName);
            AudioInfo _cInfo = this.m_assets.Get(_key);
            if (_cInfo == null)
            {
                _key = string.Format("audios/atk/{0}.ado@@{0}", _clipName);
                _cInfo = this.m_assets.Get(_key);
            }
            if (_cInfo != null)
                _cInfo.SetClip(clip);
        }

        // Debug.LogFormat("=== OnLoadAdoClip = [{0}] = [{1}] = [{2}] = [{3}] = [{4}] ", _keyLast, _lastUnload,this._key_loading,_info,_clipName);
        if (_info != null)
        {
            // Debug.LogErrorFormat("=== OnLoadAdoClip = [{0}] = [{1}]", this.m_audio,_info.isAutoPlay(), isAutoPlay());
            if (_info.isAutoPlay() || isAutoPlay())
                this.PlayClip();
        }
    }

    void OnNotifyDestry(GobjLifeListener gLife)
    {
        this.ClearAll();
    }

    void OnNotifyState(int nType, int state)
    {
        if (this.m_nType != nType)
            return;

        this.m_notiyState = state;
        switch (state)
        {
            case 1:
                if (this.m_timeRemainder > 0)
                {
                    this.Play(false);
                }
                else
                {
                    this.Stop();
                }
                break;
            case 2:
                this.Pause();
                break;
            default:
                this.Stop();
                break;
        }
    }

    void OnNotifyVolume(int nType, float volume)
    {
        // Debug.LogErrorFormat("=== OnNotifyVolume = [{0}] = [{1}] = [{2}] = [{3}] ", isMusic,this.m_isMusic,volume,this.m_volume);
        if (this.m_nType != nType)
            return;
        this.SetVolume(volume);
    }

    public int SetNotifyState(int state)
    {
        int pre = this.m_notiyState;
        this.OnNotifyState(this.m_nType, state);
        return pre;
    }

    public float GetVolume()
    {
        return this.m_volume;
    }

    public void SetVolume(float volume)
    {
        if (this.m_audio == null || this.m_volume == volume)
        {
            return;
        }
        this.m_volume = volume;
        this.m_audio.volume = volume;
    }

    private void Play(bool isToStart = true)
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
        if (isToStart)
        {
            this.m_audio.time = 0;
            this.m_audio.timeSamples = 0;
        }
        this.m_audio.Play();
        this.CorDelayEnd(true);
    }

    public void Stop()
    {
        this.CorDelayEnd();
        this.m_playState = 0;
        this.m_timeRemainder = 0;
        if (this.m_audio == null || !this.m_audio.isPlaying)
        {
            return;
        }
        this.m_audio.Stop();
        this.m_audio.timeSamples = 0;
    }

    public void Pause()
    {
        if (this.m_audio == null || !this.m_audio.isPlaying)
        {
            return;
        }
        this.m_playState = 2;
        this.CorDelayEnd();
        this.m_audio.Pause();
    }

    public void PlayClip(AudioClip clip, bool isBreak)
    {
        if (this.m_audio == null)
        {
            return;
        }

        if (!isBreak && this.m_playState != 0 && this.m_audio.isPlaying)
        {
            return;
        }

        this.Stop();

        this.m_audio.clip = clip;
        this.m_timeDuration = 0.1f;
        this.m_timeRemainder = this.m_timeDuration;
        bool _isHasClip = clip != null;
        if (_isHasClip)
        {
            this.m_timeDuration += clip.length;
            this.m_timeRemainder = this.m_timeDuration;
            this.m_lastClipName = clip.name;
            this.Play();
        }
        // Debug.LogFormat("=== PlayClip === [{0}] = [{1}] = [{2}]", this.m_timeRemainder, this.m_timeDuration, _isHasClip);
    }

    private void CorDelayEnd(bool isAgain = false)
    {
        if (this.m_nType == 1)
            return;
        m_glife.StopCoroutine(_IEnAdoEnd());
        if(this.m_isAutoStop && isAgain)
            m_glife.StartCoroutine(_IEnAdoEnd());
    }

    IEnumerator _IEnAdoEnd()
    {
        if (this.m_playState != 1)
            yield break;
        float _ct = this.m_audio.timeSamples;
        float _dt = this.m_timeDuration - _ct;
        float _absSpeed = this.m_speed < 0 ? -this.m_speed : this.m_speed;
        this.m_timeRemainder = (_absSpeed == 0) ? _dt : _dt / _absSpeed;
        if (_dt > 0.02f)
            yield return new WaitForSecondsRealtime(_dt);
        else if (_dt > 0f)
            yield return null;

        this.Pause();
        string _v = this.m_audio.clip?.name;
        if (!string.Equals(this.m_lastClipName, _v))
            this.Play(false);
        else
            this.Stop();
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

    public bool IsPlayEnd()
    {
        return this.m_audio != null && this.m_timeDuration > 0.1f && this.m_playState == 0 && this.m_timeRemainder <= 0;
    }

    public bool IsStop()
    {
        return this.m_playState == 0 && this.m_timeRemainder <= 0;
    }

    void ClearAll()
    {
        Messenger.RemoveListener<int, int>(MsgConst.MSound_State, this.OnNotifyState);
        Messenger.RemoveListener<int, float>(MsgConst.MSound_Volume, this.OnNotifyVolume);
        this.m_glife = null;
        this.m_audio = null;
        this.m_playState = 0;
        this.m_timeDuration = 0;
        this.m_timeRemainder = 0;
        this.m_speed = 1;
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

    public void SetSpeed(float speed)
    {
        if (this.m_audio)
        {
            float _pre = this.m_audio.pitch;
            this.m_audio.pitch = speed;
            if (_pre != speed)
            {
                this.m_speed = speed;
                this.CorDelayEnd(true);
            }
        }
    }
}
