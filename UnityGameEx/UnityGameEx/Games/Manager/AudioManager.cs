using UnityEngine;
using Core;
using Core.Kernel;
using UObject = UnityEngine.Object;
using CPPrefs = Core.PlayerPrefs;

/// <summary>
/// 类名 : 声音播放管理器
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-12-10 11:15
/// 功能 : 
/// </summary>
public class AudioManager : GobjLifeListener
{
    static AudioManager _instance;
    static public AudioManager instance
    {
        get
        {
            if (IsNull(_instance))
            {
                GameObject _gobj = GameMgr.mgrGobj2;
                _instance = UtilityHelper.Get<AudioManager>(_gobj, true);
                _instance.csAlias = "AudioMgr";
            }
            return _instance;
        }
    }

    float _musicTransition = 1f;
    public float m_musicTransition
    {
        get { return _musicTransition; }
        set
        {
            if (value <= 0 && _musicTransition > 0)
            {
                bool _prePause = this.m_isPause;
                this.m_isPause = true;

                AudioData _cur = this.GetCurrMus();
                AudioData _other = (_cur == this.m_musicData) ? this.m_musicData2 : this.m_musicData;
                _other.Stop();
                _cur.SetVolume(this.m_volumeMusic);

                this._mmCurrTrans = 0.0f;
                this._chgMusicVal = 0.0f;
                this._musicTransition = value;
                this.m_isPause = _prePause;
            }
            else
                this._musicTransition = value;
        }
    }
    float _mmCurrTrans = 0.0f,_chgMusicVal = 0.0f;
    bool m_isMus1 = false;
    AudioData m_musicData;
    AudioData m_soundData;
    AudioData m_uisoundData;
    AudioData m_musicPre, m_musicData2;

    [Range(0f, 1f)] [SerializeField] float m_volumeMusic = 1f;
    [Range(0f, 1f)] [SerializeField] float m_volumeSound = 1f;
    [SerializeField] bool m_isCloseMusic = false;
    [SerializeField] bool m_isCloseSound = false;

    ListDict<AudioData> m_data = new ListDict<AudioData>(false);

    private DF_ToLoadAdoClip m_cfLoad = null;
    string _keyCache = "ado_info";
    public string m_keyCache { get { return _keyCache; } set { if(!string.IsNullOrEmpty(value)) _keyCache = value; } }
    public bool m_isPause = false;

    public void Init(string crcDataPath,DF_ToLoadAdoClip cfLoad,float mmChg = 1f)
    {
        this.m_musicTransition = mmChg;
        this.InitAudio(crcDataPath);
        this.m_cfLoad = cfLoad;
        int _st = this.m_isCloseMusic ? 2 : 1;
        this.m_musicPre = AudioData.Builder(this.m_gobj, true, true, this.m_volumeMusic).SyncSetting(_st, cfLoad);
        this.m_musicData = AudioData.Builder(this.m_gobj, true, true, this.m_volumeMusic).SyncSetting(_st,cfLoad);
        this.m_musicData2 = AudioData.Builder(this.m_gobj, true, true, this.m_volumeMusic).SyncSetting(_st, cfLoad);

        GameObject _music = this.m_gobj;
        // _music = UtilityHelper.NewGobj("_sound", this.m_gobj);
        _st = this.m_isCloseSound ? 2 : 1;
        this.m_soundData = AudioData.BuilderSound(_music, true, this.m_volumeSound).SyncSetting(_st, cfLoad);

        // _music = UtilityHelper.NewGobj("_sound_ui", this.m_gobj);
        this.m_uisoundData = AudioData.BuilderSound(_music, true, this.m_volumeSound).SyncSetting(_st, cfLoad);
    }

    private void InitAudio(string crcDataPath)
    {
        if(string.IsNullOrEmpty(crcDataPath))
            crcDataPath = CRCClass.GetCRCContent( UGameFile.m_dirDataNoAssets);
        string _key = string.Format("{0}_{1}", m_keyCache, crcDataPath);
        _key = m_keyCache;
        if (!CPPrefs.HasKey(_key))
            return;
        string _v = CPPrefs.GetString(_key);
        var _arrs = _v.Split('_');
        bool isMusic = true, isSound = true;
        float music = 1, sound = 1;
        int lens = _arrs.Length;
        if(lens >= 4)
        {
            isMusic = "1".Equals(_arrs[0]);
            float.TryParse(_arrs[1], out music);
            isSound = "1".Equals(_arrs[2]);
            float.TryParse(_arrs[3], out sound);
        }
        // Debug.LogErrorFormat("=== InitAudio = [{0}] = [{1}] = [{2}] = [{3}] ", isMusic,music,isSound,sound);
        this.InitAudio(isMusic, music, isSound, sound);
    }

    public void InitAudio(bool isMusic,float music,bool isSound,float sound)
    {
        this.SetMusicState(!isMusic);
        this.SetMusicVolume(music);
        this.SetSoundState(!isSound);
        this.SetSoundVolume(sound);
    }

    public void SetMusicVolume(float val)
    {
        if (this.m_volumeMusic == val)
            return;

        this.m_volumeMusic = val;
        Messenger.Brocast<bool, float>(MsgConst.MSound_Volume, true, this.m_volumeMusic);
    }

    public void SetSoundVolume(float val)
    {
        if (this.m_volumeSound == val)
            return;

        this.m_volumeSound = val;
        Messenger.Brocast<bool, float>(MsgConst.MSound_Volume, false, this.m_volumeSound);
    }

    public void SetVolume(float val)
    {
        this.SetMusicVolume(val);
        this.SetSoundVolume(val);
    }

    public void SetMusicState(bool isClose)
    {
        if (this.m_isCloseMusic == isClose)
            return;

        this.m_isCloseMusic = isClose;
        Messenger.Brocast<bool, int>(MsgConst.MSound_State, true, isClose ? 2 : 1);
    }

    public void SetSoundState(bool isClose)
    {
        if (this.m_isCloseSound == isClose)
            return;

        this.m_isCloseSound = isClose;
        Messenger.Brocast<bool, int>(MsgConst.MSound_State, false, isClose ? 2 : 1);
    }

    public void SetAudioState(bool isClose)
    {
        this.SetMusicState(isClose);
        this.SetSoundState(isClose);
    }

    AudioData GetCurrMus()
    {
        AudioData _cur = this.m_musicData;
        if (this.m_musicTransition > 0.0f)
            _cur = this.m_isMus1 ? this.m_musicData : this.m_musicData2;
        return _cur;
    }

    public void PlayMusic(string abName,int tagType)
    {
        if(tagType == 0)
        {
            this.m_musicPre.LoadAsset(abName, tagType);
            return;
        }

        float _vol = this.m_volumeMusic;
        if (this.m_musicTransition > 0.0f)
        {
            this._mmCurrTrans = 0;
            this.m_isMus1 = !this.m_isMus1;
            _vol = 0.0f;
            int frame = GameAppEx.fpsFrameRate;
            _chgMusicVal = this.m_volumeMusic / frame;
        }

        AudioData _cur = this.GetCurrMus();
        _cur.SetVolume(_vol);
        _cur.LoadAsset(abName, tagType);
    }

    public void PlaySound(string abName, int tagType)
    {
        this.m_soundData.LoadAsset(abName, tagType);
    }

    public void PlayUISound(string abName, int tagType)
    {
        this.m_uisoundData.LoadAsset(abName, tagType);
    }

    public AudioData GetAudioData(int ntype)
    {
        switch (ntype)
        {
            case 1:
                return this.m_soundData;
            case 2:
                return this.m_uisoundData;
            default:
                return this.GetCurrMus();
        }
    }

    public AudioData GetAudioData(UObject uobj)
    {
        if (UtilityHelper.IsNull(uobj))
            return null;

        GameObject gobj = UtilityHelper.ToGObj(uobj);
        int _id = gobj.GetInstanceID();
        AudioData _dt_ = this.m_data.Get(_id);
        if (_dt_ == null)
        {
            GobjLifeListener glife = GobjLifeListener.Get(gobj);
            glife.OnlyOnceCallDetroy(this._OnNotifyDestry);

            int _st = this.m_isCloseSound ? 2 : 1;
            _dt_ = AudioData.BuilderSound(gobj, false, this.m_volumeSound).SyncSetting(_st, this.m_cfLoad);
            this.m_data.Add(_id, _dt_);
        }
        return _dt_;
    }

    public AudioData PlayAudio(UObject uobj, string abName,int nTagType)
    {
        if (UtilityHelper.IsNull(uobj))
            return null;
        AudioData _dt_ = this.GetAudioData(uobj);
        if (_dt_ != null)
            _dt_.LoadAsset(abName, nTagType);
        return _dt_;
    }

    void _OnNotifyDestry(GobjLifeListener gLife)
    {
        int _id = gLife.m_gobjID;
        this.m_data.Remove(_id);
    }

    void Update()
    {
        if (this.m_isPause)
            return;

        this._OnUpMus();
    }

    void _OnUpMus()
    {
        if (this.m_musicTransition <= 0.0f)
            return;
        AudioData _cur = this.GetCurrMus();
        AudioData _other = (_cur == this.m_musicData) ? this.m_musicData2 : this.m_musicData;
        if (_other.IsStop())
        {
            float _v = _cur.GetVolume();
            if(_v != this.m_volumeMusic)
            {
                _cur.SetVolume(this.m_volumeMusic);
            }
            return;
        }

        if(this._mmCurrTrans >= this.m_musicTransition)
        {
            _other.Stop();
            _cur.SetVolume(this.m_volumeMusic);
            return;
        }

        this._mmCurrTrans += Time.deltaTime;
        float _v1 = _other.GetVolume() - _chgMusicVal;
        float _v2 = _cur.GetVolume() + _chgMusicVal;
        if(_v1 <= 0 || _v2 >= this.m_volumeMusic)
        {
            _other.Stop();
            _cur.SetVolume(this.m_volumeMusic);
        }
        else
        {
            _other.SetVolume(_v1);
            _cur.SetVolume(_v2);
        }

    }
}
