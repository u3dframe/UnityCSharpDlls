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
	public class UpdateProcess : IUpdate  {
        bool m_isValidVersion = true, m_isUnZip = true;
        Action m_cfFinish = null;
        DF_OnState m_cfChgState = null;
        Queue<string> m_zipIndexs = new Queue<string>();
        EM_Process m_state = EM_Process.None;
        EM_Process m_preState = EM_Process.None;
        UnZipClass unzip = null;
        bool m_isBegZip = false;
        long nCurr = 0, m_nSize = 0, nZipCurr = 0;
        string m_obbPath = null;

        public bool m_isOnUpdate = false;
        public bool IsOnUpdate() { return this.m_isOnUpdate; }
        public void OnUpdate(float dt, float unscaledDt) {
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
                case EM_Process.Completed:
                    _ST_Completed();
                    break;
            }
        }

        void _ExcCompleted()
        {
            var _fun = this.m_cfFinish;
            this.m_cfFinish = null;
            this.m_cfChgState = null;
            if (_fun != null)
                _fun();
        }

        void _SetState(EM_Process state,bool isCall = true) {
            this.m_state = state;
            if (isCall && this.m_cfChgState != null)
                this.m_cfChgState((int)this.m_state);
        }

        void _SetStatePre(EM_Process state, bool isCall = false) {
            this.m_preState = this.m_state;
            this._SetState(state, isCall);
        }

        void _OnGC(bool isIMM = false)
        {
            if (!isIMM)
                isIMM = Time.frameCount % 100 == 0;
            if (isIMM)
                GC.Collect();
        }

        public UpdateProcess Init(Action callComplete, DF_OnState callStateChange = null,string obbPath = null,bool isUnZip = true,bool isValidVer = true)
        {
            this.m_cfChgState = callStateChange;
            this.m_cfFinish = callComplete;
            this.m_obbPath = obbPath;
            this.m_isValidVersion = isValidVer;
            this.m_isUnZip = isUnZip;
            this._SetState(EM_Process.PreUnZipRes);
            return this;
        }

        public void RegUpdate(bool isUp)
        {
            this.m_isOnUpdate = isUp;
            GameMgr.DiscardUpdate(this);
            if (isUp)
            {
                GameMgr.RegisterUpdate(this);
            }
        }

        public UpdateProcess Start()
        {
            if (!this.m_isOnUpdate)
                this.RegUpdate(true);

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
            WWWMgr.instance.StartUWR(_vPath, _CFLoadZipList);
        }

        void _CFLoadZipList(bool isSuccess, UnityWebRequest uwr, object pars)
        {
            if (isSuccess)
            {
                string[] arrs = UGameFile.SplitComma(uwr.downloadHandler.text);
                for (int i = 0; i < arrs.Length; i++)
                {
                    if (i == arrs.Length - 1)
                    {
                        this.m_nSize = UtilityHelper.Str2Long(arrs[i]);
                    }
                    else
                    {
                        m_zipIndexs.Enqueue(arrs[i]);
                    }
                }

                this._SetState(EM_Process.UnZipRes);
            }
            else
            {
                this._SetState(EM_Process.Error_LoadZipList);
                Debug.LogErrorFormat("=== load ziplist error = [{0}]", uwr.error);
            }
        }

        void _ST_UnZipRes()
        {
            if (m_zipIndexs.Count <= 0)
            {
                this._SetState(EM_Process.UnGpOBB);
            }
            else
            {
                string _ind = m_zipIndexs.Peek();
                if (string.IsNullOrEmpty(_ind))
                {
                    this.unzip = null;
                    this.m_zipIndexs.Dequeue();
                    return;
                }

                if (_UnZipOne(_ind))
                {
                    this.unzip = null;
                    m_zipIndexs.Dequeue();
                }
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
                    WWWMgr.instance.StartUWR(_vPath, _CFLoadZipOne);
                }
                else
                {
                    this._UnZip_();
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
            // if (this.m_nSize <= 0)
            //     this.m_nSize = state.m_nAllFileCount;

            if (this.unzip.m_bFinished)
            {
                this.unzip.Close();

                _OnGC(true);

                if (this.unzip.error != null)
                {
                    this.unzip = null;
                    throw this.unzip.error;
                }
                this.unzip = null;
            }
        }

        void _CFLoadZipOne(bool isSuccess, UnityWebRequest uwr, object pars)
        {
            if (isSuccess)
            {
                this.unzip = new UnZipClass(uwr.downloadHandler.data, UGameFile.m_dirRes);
                this.m_isBegZip = false;
                this.nZipCurr = 0;
                this._SetState(EM_Process.UnZipRes);
            }
            else
            {
                this._SetState(EM_Process.Error_LoadZipOne);
                Debug.LogErrorFormat("=== load one zip error = [{0}]", uwr.error);
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
            string _vPath = string.Concat(UGameFile.m_dirStreaming,CfgVersion.m_defFileName);
            _vPath = UGameFile.ReWwwUrl(_vPath);
            WWWMgr.instance.StartUWR(_vPath, _CFLoadStreamVersion,_vPath);
        }

        void _CFLoadStreamVersion(bool isSuccess, UnityWebRequest uwr, object pars)
        {
            if (isSuccess)
            {
                CfgVersion _verStream = CfgVersion.BuilderBy(uwr.downloadHandler.text);
                CfgVersion _verCurr = CfgVersion.Builder(null);
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
                Debug.LogErrorFormat("=== load StreamVersion,pars=[{0}], error = [{1}]", pars, uwr.error);
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
                this._SetState(EM_Process.Completed);
            }
        }

        void _ST_Completed()
        {
            this.RegUpdate(false);
            _OnGC(true);

            CfgFileList.instance.LoadDefault();
            var cfg = CfgVersion.instance.LoadDefault();
            XXTEA.SetCustKey(cfg.m_keyLua);

            // 完成回调
            this._ExcCompleted();
        }
    }
}
