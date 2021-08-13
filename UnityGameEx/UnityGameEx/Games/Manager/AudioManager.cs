using UnityEngine;
using Core;
using Core.Kernel;
using UObject = UnityEngine.Object;
using UPlayerPrefs = UnityEngine.PlayerPrefs;

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
                _instance.InitAudio();
            }
            return _instance;
        }
    }

    AudioData m_musicData;
    AudioData m_soundData;
    AudioData m_uisoundData;

    [Range(0f, 1f)] [SerializeField] float m_volumeMusic = 1f;
    [Range(0f, 1f)] [SerializeField] float m_volumeSound = 1f;
    [SerializeField] bool m_isCloseMusic = false;
    [SerializeField] bool m_isCloseSound = false;

    ListDict<AudioData> m_data = new ListDict<AudioData>(false);

    private DF_ToLoadAdoClip m_cfLoad = null;
    string _keyCache = "ado_info";
    public string m_keyCache { get { return _keyCache; } set { if(!string.IsNullOrEmpty(value)) _keyCache = value; } }

    public void Init(DF_ToLoadAdoClip cfLoad)
    {
        this.m_cfLoad = cfLoad;
        int _st = this.m_isCloseMusic ? 2 : 1;
        this.m_musicData = AudioData.Builder(this.m_gobj, true, true, this.m_volumeMusic).SyncSetting(_st,cfLoad);

        GameObject _music = UtilityHelper.NewGobj("_sound", this.m_gobj);
        _st = this.m_isCloseSound ? 2 : 1;
        this.m_soundData = AudioData.BuilderSound(_music, true, this.m_volumeSound).SyncSetting(_st, cfLoad);

        _music = UtilityHelper.NewGobj("_sound_ui", this.m_gobj);
        this.m_uisoundData = AudioData.BuilderSound(_music, true, this.m_volumeSound).SyncSetting(_st, cfLoad);
    }

    private void InitAudio()
    {
        if (!UPlayerPrefs.HasKey(m_keyCache))
            return;
        string _v = UPlayerPrefs.GetString("m_keyCache");
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

    public void PlayMusic(string abName,int tagType)
    {
        this.m_musicData.LoadAsset(abName, tagType);
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
            default:
                return this.m_musicData;
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
}
