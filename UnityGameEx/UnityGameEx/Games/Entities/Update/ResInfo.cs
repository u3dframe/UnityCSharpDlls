using UnityEngine;
using UnityEngine.Networking;

namespace Core.Kernel
{
    /// <summary>
    /// 类名 : 委托 - 资源下载状态回调
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2018-01-30 14:35
    /// 功能 : 
    /// 描述 : state=EM_SucOrFails枚举int值
    /// </summary>
    public delegate void DF_LDownFile(int state,ResInfo dlFile);

    /// <summary>
    /// 类名 : 资源信息对象
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2017-12-07 14:35
    /// 功能 : 
    /// </summary>
    public class ResInfo : Beans.ED_Basic {
        // 资源名 = m_compareCode + 后缀名
        public string m_resName = "";

        // 真实资源名 - 用于打包时候的判断 (一般是相对路径 xxx/xx/xx.ab,xx.lua,)
        public string m_realName = "";

		// 对比码
		public string m_compareCode = "";

		// 资源的包体(上面的相对路径还在该resPackage下面)
		string _m_resPackage = "";
		public string m_resPackage{
			get{ return _m_resPackage; }
			set{
				_m_resPackage = value;
				if (!string.IsNullOrEmpty (_m_resPackage)) {
					int _ind = _m_resPackage.LastIndexOf ("/");
					if (_ind == _m_resPackage.Length - 1) {
						_m_resPackage = _m_resPackage.Substring (0, _ind);
					}
				}
			}
		}

		// 文件大小
		public int m_size = 0;

		// 文件位置(下载的时候用)
		public string m_filePath {
			get { 
				if (string.IsNullOrEmpty (this.m_resPackage)) {
					return this.m_resName;
				} else {
					return string.Format ("{0}/{1}", this.m_resPackage, this.m_resName);
				}
			}
		}

        public string m_curName { get; private set; }
        public bool isManifest { get; private set; }
        public bool m_isMustFile { get; private set; } // 是否是包体必要文件

        public ResInfo(){
		}

		public ResInfo(string row){
			Init (row);
		}

		public ResInfo(string resName, string compareCode, string resPackage, int size, string realName) {
			Init (resName, compareCode,resPackage,size,realName);
		}

		public void Init(string row){
            string[] _arrs = UGameFile.SplitComma(row);
            int _tmp = _arrs.Length;
            if (_tmp < 3)
				return;
			string resPackage = "";
			if (_tmp > 3)
				resPackage = _arrs [3];

			string resReal = "";
			if (_tmp > 4)
				resReal = _arrs [4];

            _tmp = UtilityHelper.Str2Int(_arrs[2]);
            Init (_arrs [0], _arrs [1],resPackage, _tmp, resReal);
		}

        bool IsMustFileByCurName() {
            string fn = this.m_curName;
            return fn.EndsWith(".lua") || fn.EndsWith(".txt") || fn.EndsWith(".csv") || fn.EndsWith(".minfo") || fn.IndexOf("protos/") != -1;
        }

		public void Init(string resName,string compareCode,string resPackage,int size,string realName){
			this.m_resName = resName;
			this.m_compareCode = compareCode;
			this.m_resPackage = resPackage == null ? "" : resPackage;
			this.m_size = size;
			this.m_realName = realName;

            if (string.IsNullOrEmpty(realName))
                this.m_curName = resName;
            else
                this.m_curName = realName;

            this.isManifest = this.m_curName.Equals(UGameFile.m_curPlatform);
            this.m_isMustFile = this.isManifest || this.IsMustFileByCurName();
        }
        
		public bool IsSame(ResInfo other){
			if (other == null)
				return false;
			int v = string.Compare(other.m_compareCode,m_compareCode,true);
			return v == 0;
		}

		public void CloneFromOther(ResInfo other){
			this.m_resName = other.m_resName;
			this.m_compareCode = other.m_compareCode;
			this.m_resPackage = other.m_resPackage;
			this.m_size = other.m_size;
			this.m_realName = other.m_realName;

            this.Clone4Down(other);
        }

		public override bool keepWaiting {
			get {
                if(this.m_downState == EM_DownLoad.None)
				    return false;

                OnUpdate();
                return !m_isEnd;
            }
		}

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4}", m_resName, m_compareCode, m_size, m_resPackage, m_realName);
        }

        // ============== 下载相关的
        int m_numLimitTry = 3; // 限定失败后下载次数
        int m_numCountTry = 1; // 当前下载次数

        // 下载地址
        private string _m_url = "";
        private string[] _arrsUrls = null;
        private int _indexUrl = -1;
        public string m_url
        {
            get { return _m_url; }
            set
            {
                this.m_numLimitTry = 1;
                if (string.IsNullOrEmpty(value))
                {
                    _arrsUrls = null;
                }
                else if (!value.Equals(_m_url))
                {
                    _arrsUrls = UGameFile.SplitDivision(value, true);
                    _indexUrl = -1;
                    this.m_numLimitTry = UGameFile.LensArrs(_arrsUrls);
                }
                _m_url = value;
            }
        }

        public EM_DownLoad m_downState = EM_DownLoad.None;
        public int m_iDownState { get { return (int)this.m_downState; } }
        public bool isError { get { return this.m_iDownState >= (int)EM_DownLoad.Error; } }
        public bool isCompleted { get { return this.m_downState == EM_DownLoad.Completed; } }
        UnityWebRequest m_uwr = null;
        string m_realUrl = "";
        public string m_strError { get; private set; }
        float m_wwwProgress = 0; // 下载进度
        float m_timeout = 5;
        float m_curtime = 0;
        public int m_nLogError { get; set; } // 0-不打印,1-每次错误打印,2-只打印最后一次
        public object m_objTarget { get; private set; } // 下载得到的目标对象
        EM_Asset m_assetType = EM_Asset.Text; // 默认资源类型为:文本内容
        public int m_nWrite { get; set; } // 下载完毕后写文件,0-不写,1-要写
        public event DF_LDownFile m_callFunc = null; // 加载,下载的成功失败状态回调
        public bool m_isEnd { get { return isError || isCompleted; } }
        public bool m_isCheckCompareCode { get; set; }

        public ResInfo(string url, string proj, string fn, DF_LDownFile callFunc, EM_Asset aType)
        {
            ReInit(url, proj, fn, callFunc, aType);
        }

        public ResInfo ReInit(string url, string proj, string fn, DF_LDownFile callFunc, EM_Asset aType)
        {
            this.m_url = url;
            this.m_resPackage = proj;
            this.m_resName = fn;
            this.m_assetType = aType;

            this.AddOnlyOnceCall(callFunc);
            return this;
        }

        public void AddOnlyOnceCall(DF_LDownFile call)
        {
            if (call == null)
                return;
            this.m_callFunc -= call;
            this.m_callFunc += call;
        }

        public void Clone4Down(ResInfo df) {
            this.m_assetType = df.m_assetType;
            this.m_url = df.m_url;
            this.m_callFunc = df.m_callFunc;
        }

        public void OnUpdate()
        {
            this.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }

        override public void OnUpdate(float dt, float unscaledDt) {
            switch (this.m_downState)
            {
                case EM_DownLoad.Init:
                    _ST_Init();
                    break;
                case EM_DownLoad.DownLoading:
                    _ST_DownLoad();
                    break;
            }
        }

        void _ST_Init()
        {
            if (m_uwr == null)
            {
                this.m_strError = "";
                if (string.IsNullOrEmpty(m_url))
                {
                    this.m_strError = "Url Error : url is null";
                    this.m_downState = EM_DownLoad.Error_EmptyUrl;
                    return;
                }

                string _url = UGameFile.GetUrl(_arrsUrls, m_url, ref _indexUrl);
                _url = UGameFile.ReUrlEnd(_url);
                this.m_realUrl = string.Format("{0}{1}", _url, this.m_filePath);
                m_uwr = new UnityWebRequest(this.m_realUrl);
                m_uwr.timeout = 30;

                DownloadHandler _down = null;
                switch (this.m_assetType)
                {
                    case EM_Asset.Texture:
                        _down = new DownloadHandlerTexture();
                        break;
                    case EM_Asset.AssetBundle:
                        _down = new DownloadHandlerAssetBundle(this.m_realUrl, 0);
                        break;
                    case EM_Asset.Bytes:
                    case EM_Asset.Text:
                        _down = new DownloadHandlerBuffer();
                        break;
                }

                if(_down != null)
                {
                    m_uwr.downloadHandler = _down;
                }

                m_uwr.SendWebRequest();
                m_curtime = 0;
                m_wwwProgress = 0;                
            }
            this.m_downState = EM_DownLoad.DownLoading;
        }

        void _ST_DownLoad()
        {
            if (this.m_uwr == null)
            {
                if (this.m_downState == EM_DownLoad.DownLoading)
                    this.m_downState = EM_DownLoad.Init;

                return;
            }

            if (this.m_uwr.isDone)
            {
                if (this.m_uwr.isHttpError || this.m_uwr.isNetworkError)
                // if (this.m_uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    this.m_downState = EM_DownLoad.Error_LoadDown;
                    this.m_strError = this.m_uwr.error;
                }
                else
                {
                    bool _isValid = true;
                    if (this.m_isCheckCompareCode && !string.IsNullOrEmpty(this.m_compareCode))
                    {
                        string _code = CRCClass.GetCRC(this.m_uwr.downloadHandler.data);
                        _isValid = this.m_compareCode.Equals(_code);
                        if (!_isValid)
                        {
                            this.m_strError = string.Format("CRC not match,old = [{0}],new = [{1}]",this.m_compareCode, _code);
                            this.m_downState = EM_DownLoad.Error_NotMatchCode;
                        }
                    }

                    if (_isValid)
                    {
                        this._DoWWWDone();
                    }
                }
                this.m_uwr.Dispose();
            }
            else
            {
                if (this.m_uwr.downloadProgress == this.m_wwwProgress)
                {
                    this.m_curtime += Time.unscaledDeltaTime;
                }
                else
                {
                    this.m_curtime = 0;                    
                    this.m_wwwProgress = this.m_uwr.downloadProgress;
                }

                if (this.m_timeout <= this.m_curtime)
                {
                    this.m_downState = EM_DownLoad.Error_TimeOut;
                    this.m_strError = "time out";
                    try
                    {
                        this.m_uwr.Abort();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogErrorFormat("=== abort uwr error = [{0}]", ex);
                    }
                }
            }

            if (this.m_isEnd)
            {
                this.m_uwr = null;

                if (this.isError)
                {
                    if (this.m_nLogError == 1 || this.m_numLimitTry <= this.m_numCountTry)
                    {
                        this.m_strError = string.Format("== Down Load Error : url = [{0}] , Error = [{1}]", this.m_realUrl, this.m_strError);
                        if (m_nLogError != 0)
                            Debug.LogError(this.m_strError);
                    }

                    if (this.m_numLimitTry > this.m_numCountTry)
                    {
                        this.m_numCountTry++;
                        this.m_downState = EM_DownLoad.Init;
                    }
                    else
                    {
                        _ExcuteCallFunc(EM_SucOrFails.Fails);
                    }
                }
            }
        }

        void _DoWWWDone()
        {
            this.m_downState = EM_DownLoad.WaitCommand;
            this.m_objTarget = null;
            var _down_ = this.m_uwr.downloadHandler;
            try
            {
                switch (this.m_assetType)
                {
                    case EM_Asset.Text:
                        this.m_objTarget = _down_.text;
                        break;
                    case EM_Asset.Texture:
                        this.m_objTarget = DownloadHandlerTexture.GetContent(this.m_uwr);
                        break;
                    case EM_Asset.AssetBundle:
                        this.m_objTarget = DownloadHandlerAssetBundle.GetContent(this.m_uwr);
                        break;
                    default:
                        this.m_objTarget = _down_.data;
                        break;
                }

                _ExcuteCallFunc(EM_SucOrFails.Success);
            }
            catch (System.Exception ex)
            {
                this.m_downState = EM_DownLoad.Error_ExcuteCall;
                this.m_strError = ex.Message;
            }
        }

        void _ExcuteCallFunc(EM_SucOrFails emState)
        {
            if (emState == EM_SucOrFails.Success)
            {
                if (m_nWrite == 1)
                {
                    _WriteFile();
                }
                else
                {
                    this.m_downState = EM_DownLoad.Completed;
                }
            }
            var _call = this.m_callFunc;
            this.m_callFunc = null;
            if (_call != null)
            {
                _call((int)emState, this);
            }
        }

        void _WriteFile()
        {
            this.m_downState = EM_DownLoad.WaitCommand;
            try
            {
                string _fp = UGameFile.curInstance.GetFilePath(this.m_resName);
                UGameFile.WriteFile(_fp,m_uwr.downloadHandler.data);
                this.m_downState = EM_DownLoad.Completed;
            }
            catch (System.Exception ex)
            {
                this.m_downState = EM_DownLoad.Error_NotEnoughMemory;
                m_strError = ex.Message;
            }
        }

        public ResInfo DownReady(string url, string proj, DF_LDownFile callFunc = null, EM_Asset aType = EM_Asset.Bytes,int nWrite = 0,int nLog = 0)
        {
            this.ReInit(url, proj, this.m_resName, callFunc, aType);
            this.m_nWrite = nWrite;
            this.m_nLogError = nLog;
            this.m_numCountTry = 1;
            this.m_isOnUpdate = false;
            this.m_objTarget = null;
            this.m_downState = EM_DownLoad.Init;
            return this;
        }

        public ResInfo DownStart()
        {
            this.m_downState = EM_DownLoad.Init;
            this.StartUpdate();
            return this;
        }

        public ResInfo ReDownReady(DF_LDownFile callFunc = null)
        {
            return this.DownReady(this.m_url, this.m_resPackage, callFunc, this.m_assetType, this.m_nWrite, this.m_nLogError);
        }
    }
}
