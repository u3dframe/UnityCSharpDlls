using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Core.Kernel.Cipher;

namespace Core.Kernel
{
	/// <summary>
	/// 类名 : 更新流程脚本
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2019-08-20 19:35
	/// 功能 : 下载更新
	/// </summary>
	public class UpdateProcess : Beans.ED_Basic {
        bool m_isValidVersion = true, m_isUnZip = true;
        Action m_cfFinish = null;
        DF_OnState m_cfChgState = null;
        DF_OnUpdate m_cfProgress = null;
        Queue<string> m_zipIndexs = new Queue<string>();
        EM_Process m_state = EM_Process.None;
        EM_Process m_preState = EM_Process.None;
        UnZipClass unzip = null;
        bool m_isBegZip = false;
        int nZipCurr = 0;
        public long nCurr { get; private set; }
        public long m_nSize { get; private set; }
        string m_obbPath = null;
        public CompareFiles m_compare { get; private set; }
        ResInfo _downVerOrFList = null;
        int n_appfull_down = 0;
        List<ResInfo> m_lNdDown4AppFull = new List<ResInfo>();
        List<ResInfo> m_lDownError4AppFull = new List<ResInfo>();
        List<ResInfo> m_lTemp = new List<ResInfo>();

        override public void OnUpdate(float dt, float unscaledDt) {
            switch (this.m_state)
            {
                case EM_Process.PreUnZipRes:
                    _ST_PreUnZipRes();
                    break;
                case EM_Process.UnZipRes:
                    _ST_UnZipRes();
                    break;
                case EM_Process.UnGpOBB:
                    _ST_UnGpObb();
                    break;
                case EM_Process.CheckAppCover:
                    _ST_CheckAppCover();
                    break;
                case EM_Process.CheckNet:
                    _ST_CheckNet();
                    break;
                case EM_Process.CheckAppFull:
                    _ST_CheckAppFull();
                    break;
                case EM_Process.Completed:
                    _ST_Completed();
                    break;
                case EM_Process.CheckVersion:
                    _ST_CheckVersion();
                    break;
                case EM_Process.CheckFileList:
                    _ST_CheckFileList();
                    break;
                case EM_Process.InitMustFiles:
                    _ST_InitMustFiles();
                    break;
                case EM_Process.CompareFileList:
                    _ST_CompareFileList();
                    break;
                case EM_Process.SaveFileList:
                    _ST_SaveFileList();
                    break;
                case EM_Process.SaveVersion:
                    _ST_SaveVersion();
                    break;
            }

            /*
            var _curInfo = this._downVerOrFList;
            if (_curInfo != null)
            {
                this._ExcuteProgress(_curInfo.m_wwwProgress);
            }
            */
        }

        void _ExcCompleted()
        {
            this.m_lTemp.Clear();
            this.m_lNdDown4AppFull.Clear();
            this.m_lDownError4AppFull.Clear();

            var _fun = this.m_cfFinish;
            this.m_cfFinish = null;
            this.m_cfChgState = null;
            this.m_cfProgress = null;
            if (_fun != null)
                _fun();
        }

        void _SetState(EM_Process state,bool isCall = true) {
            this.m_state = state;
            if (isCall && this.m_cfChgState != null)
                this.m_cfChgState((int)this.m_state,(int)this.m_preState);
        }

        void _SetStatePre(EM_Process state, bool isCall = false) {
            this.m_preState = this.m_state;
            this._SetState(state, isCall);
        }

        void _ExcuteProgress(float cur,float size = 1.0f)
        {
            var _cfCall = this.m_cfProgress;
            if (_cfCall == null)
                return;
            float min = Mathf.Min(cur, size);
            float max = min == cur ? size : cur;
            if (max <= 0) max = 1.0f;
            _cfCall(min,max);
        }

        void _OnGC(bool isIMM = false)
        {
            if (!isIMM)
                isIMM = Time.frameCount % 100 == 0;
            if (isIMM)
                GC.Collect();
        }

        public UpdateProcess Init(Action callComplete, DF_OnState callStateChange = null, DF_OnUpdate callProgress = null, string obbPath = null,bool isUnZip = true,bool isValidVer = true)
        {
            this.m_cfChgState = callStateChange;
            this.m_cfFinish = callComplete;
            this.m_cfProgress = callProgress;
            this.m_obbPath = obbPath;
            this.m_isValidVersion = isValidVer;
            this.m_isUnZip = isUnZip;
            this._SetState(EM_Process.PreUnZipRes);
            return this;
        }
        
        public UpdateProcess Start()
        {
            this.StartUpdate();
            return this;
        }

        void _ST_PreUnZipRes()
        {
            if (!this.m_isUnZip)
            {
                this._SetState(EM_Process.CheckAppCover);
                return;
            }

            string _vPath = UGameFile.curInstance.GetFilePath(CfgVersion.m_defFileName);
            bool _isHas = UGameFile.IsFile(_vPath);
            if (_isHas)
            {
                this._SetState(EM_Process.CheckAppCover);
                return;
            }

            this._SetStatePre(EM_Process.WaitCommand);
            _vPath = UGameFile.ReWwwUrl(UGameFile.m_fpZipList);
            WWWMgr.instance.StartUWR(_vPath, _CFLoadZipList,null, _vPath);
        }

        void _CFLoadZipList(bool isSuccess, UnityWebRequest uwr, object pars)
        {
            if (isSuccess)
            {
                var _data = uwr.downloadHandler.text;
                string[] arrs = UGameFile.SplitComma(_data);
                int _lens = arrs.Length - 1;
                if(_lens > 0)
                {
                    this.m_nSize = UtilityHelper.Str2Int(arrs[_lens]);
                    for (int i = 0; i < _lens; ++i)
                        m_zipIndexs.Enqueue(arrs[i]);
                    this._SetState(EM_Process.UnZipRes);
                }
                else
                {
                    this._SetState(EM_Process.UnGpOBB);
                }
            }
            else
            {
                this._SetState(EM_Process.Error_LoadZipList);
                Debug.LogErrorFormat("=== load ziplist error ,path =[{0}], err = [{1}]", pars,uwr.error);
            }
        }

        void _ST_UnZipRes()
        {
            if (m_zipIndexs.Count <= 0)
            {
                this._SetState(EM_Process.UnGpOBB);
                return;
            }
            string _ind = m_zipIndexs.Peek();
            if (string.IsNullOrEmpty(_ind))
            {
                this.unzip = null;
                this.m_zipIndexs.Dequeue();
                this._ST_UnZipRes();
                return;
            }
            bool _isZip = _UnZipOne(_ind);
            if (_isZip)
            {
                m_zipIndexs.Dequeue();
                this._ST_UnZipRes();
            }
        }

        bool _UnZipOne(string nmZipIndex)
        {
            try
            {
                if (this.unzip == null)
                {
                    this._SetStatePre(EM_Process.WaitCommand);
                    string _vPath = string.Format(UGameFile.m_fmtZip, nmZipIndex);
                    _vPath = UGameFile.ReWwwUrl(_vPath);
                    WWWMgr.instance.StartUWR(_vPath, _CFLoadZipOne,null, _vPath);
                }
                else
                {
                    this._UnZip_();
                    if (this.unzip == null)
                        return true;
                }
            }
            catch (Exception ex)
            {
                UnZipClass zipOne = this.unzip;
                this.unzip = null;
                if (zipOne != null)
                    zipOne.Close();

                this._SetState(EM_Process.Error_UnZipOne);
                Debug.LogErrorFormat("=== unzip error = [{0}]", ex);
            }
            return false;
        }

        void _UnZip_()
        {
            if (this.unzip == null)
                return;

            if (!this.m_isBegZip && !this.unzip.m_bFinished)
            {
                this.m_isBegZip = true;
                this.unzip.Begin();
            }

            ZipState state = this.unzip.m_zipState;
            this.nCurr += state.m_nZipedFileCount - this.nZipCurr;
            this.nZipCurr = state.m_nZipedFileCount;
            this._ExcuteProgress(this.nCurr, this.m_nSize);
            // if (this.m_nSize <= 0)
            //     this.m_nSize = state.m_nAllFileCount;

            if (this.unzip.m_bFinished)
            {
                this.unzip.Close();
                if (this.unzip.error != null)
                {
                    this.unzip = null;
                    throw this.unzip.error;
                }
                this.unzip = null;
                _OnGC(true);
            }
        }

        void _CFLoadZipOne(bool isSuccess, UnityWebRequest uwr, object pars)
        {
            if (isSuccess)
            {
                var _data = uwr.downloadHandler.data;
                this.unzip = new UnZipClass(_data, UGameFile.m_dirRes);
                this.m_isBegZip = false;
                this.nZipCurr = 0;
                this._SetState(EM_Process.UnZipRes);
            }
            else
            {
                this._SetState(EM_Process.Error_LoadZipOne);
                Debug.LogErrorFormat("=== load one zip error,path = [{0}],err = [{1}]", pars, uwr.error);
            }
        }

        void _ST_UnGpObb()
        {
            if (string.IsNullOrEmpty(this.m_obbPath) || !UGameFile.IsFile(this.m_obbPath))
            {
                this._SetState(EM_Process.CheckAppCover);
                return;
            }

            try
            {
                if (this.unzip == null)
                {
                    byte[] bts = UGameFile.GetFileBytes(this.m_obbPath);
                    if (bts != null && bts.Length > 0)
                    {
                        this.unzip = new UnZipClass(bts,UGameFile.m_dirRes);
                        this.m_isBegZip = false;
                        this.nZipCurr = 0;
                        this.nCurr = 0;
                        this.m_nSize = this.unzip.m_zipState.m_nAllFileCount;
                        this._ExcuteProgress(this.nCurr, this.m_nSize);
                    }
                    else
                    {
                        this._SetState(EM_Process.Error_UnZipOBB);
                        Debug.LogErrorFormat("=== _ST_UnGpObb = [bytes is null] = [{0}]", this.m_obbPath);
                    }
                }
                else
                {
                    this._UnZip_();
                    if (this.unzip == null)
                        this._SetState(EM_Process.CheckAppCover);
                }
            }
            catch (Exception ex)
            {
                UnZipClass zipOne = this.unzip;
                this.unzip = null;
                if (zipOne != null)
                    zipOne.Close();

                this._SetState(EM_Process.Error_UnZipOBB);
                Debug.LogErrorFormat("=== unzip obb error,path = [{0}] err = [{1}]",this.m_obbPath, ex);
            }
        }

        void _ST_CheckAppCover()
        {
            if (UGameFile.m_isEditor)
            {
                this._SetState(EM_Process.CheckNet);
                return;
            }

            this._SetStatePre(EM_Process.WaitCommand);
            string _vPath = string.Concat(UGameFile.m_appContentPath,CfgVersion.m_defFileName);
            _vPath = UGameFile.ReWwwUrl(_vPath);
            WWWMgr.instance.StartUWR(_vPath, _CFLoadStreamVersion,null,_vPath);
        }

        void _CFLoadStreamVersion(bool isSuccess, UnityWebRequest uwr, object pars)
        {
            if (isSuccess)
            {
                var _data = uwr.downloadHandler.text;
                CfgVersion _verStream = CfgVersion.BuilderBy(_data);
                CfgVersion _verCurr = CfgVersion.instance.LoadDefault();
                if (_verCurr.IsUpdate4Other(_verStream))
                {
                    UGameFile.DelFolder(UGameFile.m_dirRes);
                    this._SetState(EM_Process.PreUnZipRes);
                }
                else
                {
                    this._SetState(EM_Process.CheckNet);
                }
            }
            else
            {
                this._SetState(EM_Process.Error_LoadStreamVer);
                Debug.LogErrorFormat("=== load StreamVersion error ,pars=[{0}], err = [{1}]", pars, uwr.error);
            }
        }

        void _ST_CheckNet()
        {
            if (!UGameFile.m_isEditor)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    this._SetState(EM_Process.Error_NoNet);
                    Debug.LogError("=== check net error : is no network");
                    return;
                }
            }

            if (m_isValidVersion)
            {
                this._SetState(EM_Process.CheckVersion);
            }
            else
            {
                this._SetState(EM_Process.CheckAppFull);
            }
        }

        void _ST_CheckAppFull()
        {
            CfgVersion _cfgVer = CfgVersion.instance.LoadDefault();
            CfgFileList _cfgFlDef = CfgFileList.instance.LoadDefault();

            if (UGameFile.m_isEditor)
            {
                this._SetState(EM_Process.Completed);
                return;
            }

            string url = _cfgVer.m_urlFilelist;
            string proj = _cfgVer.m_pkgFiles;
            m_lTemp.AddRange(_cfgFlDef.m_data.m_list);
            int lens = m_lTemp.Count;
            ResInfo _info;
            bool _isNdDown = false;
            for (int i = 0; i < lens; i++) {
                _info = m_lTemp[i];
                if (_info == null || !_info.m_isMustFile)
                    continue;

                _isNdDown = CfgFileList.instanceDowned.IsHas(_info) && !UGameFile.IsExistsFile(_info.m_curName, false);
                if(!_isNdDown)
                    continue;

                _info.DownReady(url, proj, _CFNetAppFull, EM_Asset.Bytes, 1);
                this.m_lNdDown4AppFull.Add(_info);
                ++this.n_appfull_down;
            }
            m_lTemp.Clear();

            int _lens = this.n_appfull_down;
            if (_lens <= 0)
                this._SetState(EM_Process.Completed);
            else
                this._SetStatePre(EM_Process.WaitCommand);

            if (_lens > 0)
            {
                int _min = Mathf.Min(_lens, 5);
                var _subList = this.m_lNdDown4AppFull.GetRange(0, _min);
                for (int i = 0; i < _min; i++)
                {
                    _info = _subList[i];
                    this.m_lNdDown4AppFull.Remove(_info);
                }
                for (int i = 0; i < _min; i++)
                {
                    _info = _subList[i];
                    _info.DownStartCheckCode();
                }
            }
        }

        void _CFNetAppFull(int state, ResInfo dlFile)
        {
            bool isSuccess = state == (int)EM_SucOrFails.Success;
            if (isSuccess)
            {
                --this.n_appfull_down;
                if (this.n_appfull_down <= 0)
                {
                    this._SetState(EM_Process.Completed);
                }
                else if(this.m_lNdDown4AppFull.Count > 0)
                {
                    ResInfo dlFile2 = this.m_lNdDown4AppFull[0];
                    this.m_lNdDown4AppFull.Remove(dlFile2);
                    dlFile2.DownStartCheckCode();
                }
            }
            else
            {
                lock (this.m_lDownError4AppFull)
                {
                    // if(!this.m_lDownError4AppFull.Contains(dlFile))
                    dlFile.ReDownReady(_CFNetAppFull);
                    this.m_lDownError4AppFull.Add(dlFile);
                    // dlFile.ReDownReady(_CFNetAppFull).DownStart();
                    this._SetState(EM_Process.Error_AppFull);
                    Debug.LogErrorFormat("=== AppFull Down error = [{0}]", dlFile.m_strError);
                }
            }
        }

        void _ST_Completed()
        {
            this.StopUpdate();
            _OnGC(true);

            var cfg = CfgVersion.instance;
            XXTEA.SetCustKey(cfg.m_keyLua);

            GameMgr.instance.InitAfterUpload();
            MgrDownload.shareInstance.Init();
            AssetBundleManager.instance.Init();
            // 完成回调
            this._ExcCompleted();
        }

        void _ST_CheckVersion()
        {
            this._SetStatePre(EM_Process.WaitCommand);
            // 下载
            string url = CfgPackage.instance.m_urlVersion;
            string proj = CfgPackage.instance.m_uprojVer;
            string fn = CfgVersion.m_defFileName;
            _downVerOrFList = new ResInfo(url, proj,fn, _CFNetVersion, EM_Asset.Text);
            _downVerOrFList.DownStart();
        }

        void _CFNetVersion(int state, ResInfo dlFile)
        {
            _downVerOrFList = null;
            bool isSuccess = state == (int)EM_SucOrFails.Success;
            if (isSuccess)
            {
                string _vtxt = (string)dlFile.m_objTarget;
                CfgVersion _verNet = CfgVersion.BuilderBy(_vtxt);
                CfgVersion _verCurr = CfgVersion.instance.LoadDefault();
                if (_verCurr.IsNewDown(_verNet))
                {
                    this._SetState(EM_Process.Error_NeedDownApkIpa);
                    return;
                }

                if (_verCurr.IsUpdate4Other(_verNet))
                {
                    _verCurr.CloneFromOther(_verNet);
                    this._SetState(EM_Process.CheckFileList);
                }
                else
                {
                    this._SetState(EM_Process.CheckAppFull);
                }
            }
            else
            {
                this._SetState(EM_Process.Error_DownVer);
                Debug.LogErrorFormat("=== Down Version error = [{0}]", dlFile.m_strError);
            }
        }

        void _ST_CheckFileList()
        {
            this._SetStatePre(EM_Process.WaitCommand);
            // 下载
            CfgVersion cfg = CfgVersion.instance;
            string url = cfg.m_urlFilelist;
            string proj = cfg.m_pkgFilelist;
            string fn = CfgFileList.m_defFileName;

            _downVerOrFList = new ResInfo(url, proj, fn, _CFNetFileList, EM_Asset.Text);
            _downVerOrFList.DownStartCheckCode();
        }

        void _CFNetFileList(int state, ResInfo dlFile)
        {
            _downVerOrFList = null;
            bool isSuccess = state == (int)EM_SucOrFails.Success;
            if (isSuccess)
            {
                CfgVersion cfg = CfgVersion.instance;
                string _vtxt = (string)dlFile.m_objTarget;
                this.m_compare = new CompareFiles();
                this.m_compare.Init(_vtxt, dlFile.m_url, cfg.m_pkgFiles);
                this._SetState(EM_Process.InitMustFiles);
            }
            else
            {
                this._SetState(EM_Process.Error_DownFileList);
                Debug.LogErrorFormat("=== Down Filelist error = [{0}]", dlFile.m_strError);
            }
        }

        void _ST_InitMustFiles()
        {
            this._SetStatePre(EM_Process.WaitCommand);

            CfgVersion cfg = CfgVersion.instance;
            string url = cfg.m_urlFilelist;
            string proj = cfg.m_pkgFilelist;
            string fn = CfgMustFiles.m_defFn;
            ResInfo _info = new ResInfo(url, proj, fn, _CFNetMustFiles, EM_Asset.Text);
            _info.DownStart();
        }

        void _CFNetMustFiles(int state, ResInfo dlFile)
        {
            bool isSuccess = state == (int)EM_SucOrFails.Success;
            if (isSuccess)
            {
                string _vtxt = (string)dlFile.m_objTarget;
                CfgMustFiles.instance.Init(_vtxt);
            }
            else
            {
                CfgMustFiles.instance.LoadDefault();
            }
            this._SetState(EM_Process.CompareFileList);
        }

        void _ST_CompareFileList()
        {
            if(this.m_compare == null)
            {
                this._SetState(EM_Process.Error_NullCompareFiles);
                Debug.LogError("=== CompareFiles is Null ");
                return;
            }

            this.nCurr = this.m_compare.nCurr;
            this.m_nSize = this.m_compare.nSize;
            this._ExcuteProgress(this.nCurr, this.m_nSize);
            this.m_compare.OnUpdate();

            if (this.m_compare.isFinished)
            {
                this._SetState(EM_Process.SaveFileList);
            }
            else if (this.m_compare.isError)
            {
                EM_Process _c = (EM_Process)this.m_compare.m_iState;
                this._SetStatePre(_c, true);
                Debug.LogErrorFormat("=== CompareFiles Down Filelist error = [{0}]", this.m_compare.m_strError);
            }
        }

        void _ST_SaveFileList()
        {
            if (this.m_compare.SaveFileList())
            {
                this._SetState(EM_Process.SaveVersion);
                this.m_compare.ClearAll();
                this.m_compare = null;
            }
            else
            {
                this._SetStatePre(EM_Process.Error_SaveFList,true);
                Debug.LogErrorFormat("=== Save Filelist error = [{0}]", this.m_compare.m_strError);
            }
        }

        void _ST_SaveVersion()
        {
            CfgVersion cfg = CfgVersion.instance;
            if (cfg.SaveDefault())
            {
                this._SetState(EM_Process.CheckAppFull);
            }
            else
            {
                this._SetStatePre(EM_Process.Error_SaveVer,true);
                Debug.LogErrorFormat("=== Save Version error = [{0}]",cfg.m_strError);
            }
        }

        public void ReGoOn()
        {
            switch (this.m_state)
            {
                case EM_Process.Error_NoNet:
                    this._SetState(EM_Process.CheckNet);
                    break;
                case EM_Process.Error_DF_EmptyUrl:
                case EM_Process.Error_DF_ExcuteCall:
                case EM_Process.Error_NotEnoughMemory:
                case EM_Process.Error_DF_TimeOut:
                case EM_Process.Error_DF_LoadDown:
                case EM_Process.Error_DF_NotMatchCode:
                    if (this.m_compare != null)
                    {
                        this.m_compare.ReDownFile();
                    }
                    this._SetState(EM_Process.CompareFileList);
                    break;
                case EM_Process.Error_LoadZipList:
                    this._SetState(EM_Process.PreUnZipRes);
                    break;
                case EM_Process.Error_LoadZipOne:
                case EM_Process.Error_UnZipOne:
                    this._SetState(EM_Process.UnZipRes);
                    break;
                case EM_Process.Error_UnZipOBB:
                    this._SetState(EM_Process.UnGpOBB);
                    break;
                case EM_Process.Error_LoadStreamVer:
                    this._SetState(EM_Process.CheckAppCover);
                    break;
                case EM_Process.Error_DownVer:
                    this._SetState(EM_Process.CheckVersion);
                    break;
                case EM_Process.Error_DownFileList:
                case EM_Process.Error_NullCompareFiles:
                    this._SetState(EM_Process.CheckFileList);
                    break;
                case EM_Process.Error_SaveFList:
                    this._SetState(EM_Process.SaveFileList);
                    break;
                case EM_Process.Error_SaveVer:
                    this._SetState(EM_Process.SaveVersion);
                    break;
                case EM_Process.Error_AppFull:
                    lock (this.m_lDownError4AppFull)
                    {
                        this.m_state = EM_Process.WaitCommand;
                        this.m_preState = EM_Process.CheckAppFull;

                        m_lTemp.AddRange(this.m_lDownError4AppFull);
                        this.m_lDownError4AppFull.Clear();
                        int _lens = m_lTemp.Count;
                        for (int i = 0; i < _lens; i++)
                        {
                            m_lTemp[i].DownStartCheckCode();
                        }
                        m_lTemp.Clear();
                    }
                    break;
                case EM_Process.Error_NeedDownApkIpa:
                    break;
                default:
                    break;
            }
        }
    }
}
