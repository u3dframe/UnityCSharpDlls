using UnityEngine;
using Core;
using Core.Kernel;

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

    AudioData m_musicData;
    AudioData m_soundData;
    AudioData m_soundDataBreak;

    [Range(0f, 1f)] [SerializeField] float m_volumeMusic = 1f;
    [Range(0f, 1f)] [SerializeField] float m_volumeSound = 1f;
    [SerializeField] bool m_isCloseMusic = false;
    [SerializeField] bool m_isCloseSound = false;

    ListDict<AudioData> m_data = new ListDict<AudioData>(false);

    private DF_ToLoadAdoClip m_cfLoad = null;

    public void Init(DF_ToLoadAdoClip cfLoad)
    {
        this.m_cfLoad = cfLoad;
        this.m_musicData = AudioData.Builder(this.m_gobj, false, true, this.m_volumeMusic);
        this.m_musicData.m_cfLoad = this.m_cfLoad;
        this.m_musicData.m_isBreak = true;

        this.m_soundData = AudioData.BuilderSound(this.m_gobj, true, this.m_volumeSound);
        this.m_soundData.m_cfLoad = this.m_cfLoad;
        this.m_musicData.m_isBreak = true;

        this.m_soundDataBreak = AudioData.BuilderSound(this.m_gobj, true, this.m_volumeSound);
        this.m_soundDataBreak.m_cfLoad = this.m_cfLoad;
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

    public void PlayMusic(string abName)
    {
        this.m_musicData.LoadAsset(abName);
    }

    public void PlaySound(string abName)
    {
        this.m_soundData.LoadAsset(abName);
    }

    public void PlaySoundBreak(string abName)
    {
        this.m_soundDataBreak.LoadAsset(abName);
    }

    public AudioData PlaySound(GameObject gobj, string abName)
    {
        if (!gobj)
            return null;

        int _id = gobj.GetInstanceID();
        AudioData _dt_ = this.m_data.Get(_id.ToString());
        if (_dt_ == null)
        {
            GobjLifeListener glife = GobjLifeListener.Get(gobj);
            glife.OnlyOnceCallDetroy(this._OnNotifyDestry);

            _dt_ = AudioData.BuilderSound(gobj, false, this.m_volumeSound);
        }

        if(_dt_ != null)
        {
            _dt_.LoadAsset(abName);
        }
        return _dt_;
    }

    void _OnNotifyDestry(GobjLifeListener gLife)
    {
        int _id = gLife.gameObject.GetInstanceID();
        this.m_data.Remove(_id.ToString());
    }
}
