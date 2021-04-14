using UnityEngine;
using UnityEngine.Video;
using System;
namespace Core.Kernel
{
    /// <summary>
    /// VideoPlayer 的 外部扩展脚本 - 模块
    /// </summary>
    public static class VideoModule
    {
        /// <summary>
        /// 获取视频总时长
        /// </summary>
        /// <param name="vp"></param>
        /// <return></return>
        public static float GetVideoTime(this VideoPlayer vp)
        {
            return (vp.frameCount / vp.frameRate);
        }

        public static int GetVideoTime4Int(this VideoPlayer vp)
        {
            float _vt = vp.GetVideoTime();
            return (int)_vt;
        }

        /// <summary>
        /// 获取视频进度
        /// </summary>
        public static float GetVideoProgression(this VideoPlayer vp)
        {
            float _vt = vp.GetVideoTime();
            return (float)((vp.time * vp.frameRate) / _vt);
        }

        /// <summary>
        /// 设置视频进度
        /// </summary>
        public static void SetVideoProgression(this VideoPlayer vp, float progression)
        {
            float _vt = vp.GetVideoTime();
            float time = _vt * progression;
            vp.time = time;
            vp.Play();
        }
    }

    /// <summary>
    /// 类名 : 视频 播放 扩展
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2018-09-30 09:39
    /// 修订 : 2021-02-03 10:17
    /// 功能 : 支持url,streamingAssetsPath,persistentDataPath,原始文件
    /// 格式 : .mov, .mpg, .mpeg, .mp4, .avi, .asf等格式
    /// </summary>
    public class VideoEx : PrefabBasic
    {
        static public void PlayMovie(string fp, Color bgColor, FullScreenMovieControlMode ctlMode)
        {
            // fp = StreamingAssets or URL
            Handheld.PlayFullScreenMovie(fp, bgColor, ctlMode);
        }

        static public void PlayMovieCancelOnInput(string fp)
        {
            PlayMovie(fp, Color.black, FullScreenMovieControlMode.CancelOnInput);
        }

        static public void PlayMovieHide(string fp)
        {
            PlayMovie(fp, Color.black, FullScreenMovieControlMode.Hidden);
        }

        static public new VideoEx Get(UnityEngine.Object uobj, bool isAdd)
        {
            return UtilityHelper.Get<VideoEx>(uobj, isAdd);
        }

        static public new VideoEx Get(UnityEngine.Object uobj)
        {
            return Get(uobj, false);
        }

        public string m_fpath { get; private set; }
        public string m_fpReal { get; private set; }
        public VideoPlayer m_vper { get; private set; }
        public AudioSource m_auds { get; private set; }
        [SerializeField] RenderTextureFormat m_rtFmt = RenderTextureFormat.RGB565;
        public RenderTexture m_rtex { get; private set; }
        public int m_playState { get; private set; }
        public string m_err_msg { get; private set; }


        public bool m_isInited { get; private set; }
        public Action m_cfVdoOnReady = null;
        public Action m_cfVdoOnEnd = null;

        private UnityEngine.UI.RawImage m_rawImg = null;

        override protected void OnCall4Destroy()
        {
            CancelInvoke();
            this._Stop();

            if (null != this.m_vper)
                this.m_vper.targetTexture = null;
            if (null != this.m_rawImg)
                this.m_rawImg.texture = null;

            this.m_cfVdoOnEnd = null;
            this.m_cfVdoOnEnd = null;

            RenderTexture _rt = this.m_rtex;
            this.m_rtex = null;
            base.OnCall4Destroy();

            if(null != _rt)
            {
                if (RenderTexture.active == _rt)
                    RenderTexture.active = null;

                _rt.Release();
                // error : Destroying object "TempBuffer 682 1920x1080" is not allowed at this time.
                GameObject.DestroyImmediate(_rt);
            }
        }

        public bool Init(string fp, Action callReady, Action callEnd)
        {
            if (string.IsNullOrEmpty(fp))
                return false;

            this.m_cfVdoOnReady = callReady;
            this.m_cfVdoOnEnd = callEnd;

            this.m_fpath = fp;
            bool _isUrl = fp.Contains("http://") || fp.Contains("https://");
            this.m_fpReal = _isUrl ? fp : UGameFile.curInstance.GetPath(fp);

            this.m_playState = 0;
            this._Init();
            this.m_vper.source = VideoSource.Url;
            return true;
        }

        void _Init()
        {
            if (this.m_isInited)
                return;
            this.m_isInited = true;

            this.m_rtFmt = UtilityHelper.GetRTexFmt(this.m_rtFmt);
            this.m_rtex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, this.m_rtFmt);
            this.m_rtex.hideFlags = HideFlags.HideAndDontSave;

            this.m_vper = this.m_gobj.GetComponentInChildren<VideoPlayer>(true);
            if (null == this.m_vper)
                this.m_vper = UtilityHelper.Get<VideoPlayer>(this.m_gobj, true);


            this.m_auds = UtilityHelper.Get<AudioSource>(this.m_vper, true);
            this.m_auds.playOnAwake = false;

            this.m_vper.renderMode = VideoRenderMode.RenderTexture;
            this.m_vper.audioOutputMode = VideoAudioOutputMode.AudioSource;
            this.m_vper.skipOnDrop = true;
            this.m_vper.playOnAwake = false;
            this.m_vper.waitForFirstFrame = false;
            this.m_vper.targetTexture = this.m_rtex;

            this.m_vper.SetTargetAudioSource(0, this.m_auds);
            this.m_vper.EnableAudioTrack(0, true); // 一开始没有声音就可以使用此方法
            // Debug.Log(this.m_vper.IsAudioTrackEnabled(0));

            this.m_vper.prepareCompleted += _OnPrepared;
            this.m_vper.loopPointReached += _OnReachedEnd;
            this.m_vper.errorReceived += _OnErrorReceived;
        }

        void _Play()
        {
            if (this.m_vper == null || this.m_vper.isPlaying)
                return;
            this.m_playState = 1;
            this.m_vper.Play();
            this.m_auds.Play();
        }

        void _Pause()
        {
            if (this.m_vper == null || this.m_playState == 2)
                return;
            this.m_playState = 2;
            this.m_vper.Pause();
            this.m_auds.Pause();
        }

        void _Stop()
        {
            if (this.m_vper == null || !this.m_vper.isPlaying)
                return;
            this.m_playState = 3;
            this.m_vper.Stop();
            this.m_auds.Stop();
        }

        void _OnPrepared(VideoPlayer videoPlayer)
        {
            CancelInvoke("_ExcCF_Ready");
            Invoke("_ExcCF_Ready", 0.01f);
            // this.m_vper.frame = 1;
            this._Play();
        }

        void _ExcCF_Ready()
        {
            Action _cf = this.m_cfVdoOnReady;
            this.m_cfVdoOnReady = null;
            if (null != _cf)
                _cf();
        }

        void _OnReachedEnd(VideoPlayer videoPlayer)
        {
            CancelInvoke("_ExcCF_End");
            Invoke("_ExcCF_End", 0.01f);
        }

        public void ReCFFrameReady(VideoPlayer.FrameReadyEventHandler frameReady,bool isBind)
        {
            if (this.m_vper == null)
                return;

            this.m_vper.frameReady -= frameReady;
            if (isBind)
                this.m_vper.frameReady += frameReady;
            this.m_vper.sendFrameReadyEvents = isBind;
        }

        void _ReCFFrameReady(bool isBind)
        {
            this.ReCFFrameReady(_OnFrameReady, isBind);
        }

        void _OnFrameReady(VideoPlayer source, long frameIdx)
        {
            this._ReCFFrameReady(false);
            this._Pause();
            CancelInvoke("_Play");
            Invoke("_Play", 0.01f);
        }

        void _ExcCF_End()
        {
            Action _cf = this.m_cfVdoOnEnd;
            this.m_cfVdoOnEnd = null;
            if (null != _cf)
                _cf();
        }

        void _OnErrorReceived(VideoPlayer source, string message)
        {
            this.m_err_msg = message;
            CancelInvoke();
            Invoke("_ExcCF_Error", 0.01f);
        }

        void _ExcCF_Error()
        {
            Debug.LogErrorFormat("=== VideoPlayer err =\n{0}", this.m_err_msg);
            Jump();
        }

        public void Jump()
        {
            this._Stop();
            CancelInvoke();
            _ExcCF_End();
        }

        public void SetSpeed(float speed)
        {
            if (this.m_vper == null)
                return;
            this.m_vper.playbackSpeed = speed;
        }

        void _PlayVideo(string fp, UnityEngine.UI.RawImage rmg, Action callReady, Action callEnd)
        {
            if (!rmg || !this.Init(fp, callReady, callEnd))
                return;
            if (null != this.m_rawImg && rmg != this.m_rawImg)
                this.m_rawImg.texture = null;
            this.m_rawImg = rmg;
            rmg.texture = this.m_rtex;
            this.m_vper.url = this.m_fpReal;
            this._ReCFFrameReady(true);
            this.m_vper.Prepare();
        }

        public void PlayVideo(string fp,UnityEngine.Object rawImg,Action callReady, Action callEnd)
        {
            UnityEngine.UI.RawImage rmg = UtilityHelper.Get<UnityEngine.UI.RawImage>(rawImg);
            this._PlayVideo(fp, rmg, callReady, callEnd);
        }

        public void PlayVideo(string fp, Action callReady, Action callEnd)
        {
            UnityEngine.UI.RawImage rmg = this.m_gobj.GetComponentInChildren<UnityEngine.UI.RawImage>(true);
            this._PlayVideo(fp, rmg, callReady, callEnd);
        }

        public void PlayClip(VideoClip clip, Action callReady, Action callEnd)
        {
            if (null == clip)
                return;

            this.m_cfVdoOnReady = callReady;
            this.m_cfVdoOnEnd = callEnd;

            this.m_playState = 0;
            this._Init();
            this.m_vper.source = VideoSource.VideoClip;
            this.m_vper.clip = clip;

            this._ReCFFrameReady(true);
            this.m_vper.Prepare();
        }
    }
}