using UnityEngine;
using System;
using System.Collections.Generic;
namespace Core
{
    using Core.Kernel;
    /// <summary>
    /// 类名 : ab 的资源 管理器
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2018-06-26 10:29
    /// 功能 : internal
    /// </summary>
    public class AssetBundleManager : GobjLifeListener
    {
        static AssetBundleManager _instance;
        static public AssetBundleManager instance
        {
            get
            {
                if (IsNull(_instance))
                {
                    GameObject _gobj = GameMgr.mgrGobj;
                    _instance = UtilityHelper.Get<AssetBundleManager>(_gobj, true);
                }
                return _instance;
            }
        }

        public int m_nMaxLoad = 5; // 限定加载
        private Dictionary<string, string[]> _dependsList = new Dictionary<string, string[]>(); // 依赖关系
        private ListDict<ABInfo> _ndLoad = new ListDict<ABInfo>(true); // 需要加载的 AB
        private ListDict<ABInfo> _loading = new ListDict<ABInfo>(true); // 正在加载的 AB
        private ListDict<ABInfo> _loaded = new ListDict<ABInfo>(true); // 已经加载了的 AB
        private ListDict<ABInfo> _ndUnLoad = new ListDict<ABInfo>(true); // 需要销毁的 AB
        private ListDict<ABInfo> _unLoad = new ListDict<ABInfo>(true); // 立即销毁的 AB

        private int nUpLens = 0;
        private ABInfo upInfo = null;
        private List<ABInfo> upTemp = new List<ABInfo>();
        private List<ABInfo> upList = new List<ABInfo>();
        private bool m_isInit = false;
        public bool isDebug = false; //是否打印
        private DF_OnStr m_call4NewABInfo = null;

        private List<AssetInfo> _listLate = new List<AssetInfo>();

        public void Init()
        {
            if (this.m_isInit)
                return;

            this.m_isInit = true;
            this.isDebug = UGameFile.m_isEditor; //是否打印
            GameMgr.DisposeLateUpEvent(this._OnLateUpEnd,true);
            this.LoadMainfest();
        }

        /// <summary>
        ///  初始化
        /// </summary>
        override protected void OnCall4Awake()
        {
            this.csAlias = "ABMgr";
            this.m_isOnUpdate = true;
            GameMgr.RegisterUpdate(this);
        }

        /// <summary>
        /// 销毁
        /// </summary>
        override protected void OnCall4Destroy()
        {
            GameMgr.DiscardUpdate(this);
            GameMgr.DisposeLateUpEvent(this._OnLateUpEnd,false);
        }

        void _OnLateUpEnd()
        {
            this._OnEnd4AInfo();
        }

        void _OnEnd4AInfo()
        {
            if (_listLate.Count <= 0)
                return;
            var _list = new List<AssetInfo>(_listLate);
            int lens = _list.Count;
            AssetInfo _info = null;
            for (int i = 0; i < lens; i++)
            {
                _info = _list[i];
                _listLate.Remove(_info);
                _info.JudgeStart();
            }
        }

        protected void LogErr(string fmt, params object[] msgs)
        {
            if (!isDebug || fmt == null)
                return;
            int lens = 0;
            if (msgs != null) lens = msgs.Length;
            string msg = fmt;
            if (lens > 0)
            {
                msg = string.Format(fmt, msgs);
            }
            Debug.LogErrorFormat("==== ABMgr = [{0}]", msg);
            // Debug.LogErrorFormat("== [{0}] == [{1}] == [{2}]",this.GetType(),this.GetInstanceID(),msg);
        }

        protected void LogErr(object msg)
        {
            LogErr(msg.ToString());
        }

        public void LoadMainfest()
        {
            string path = UGameFile.m_fpABManifest;
            _dependsList.Clear();
            if (UGameFile.curInstance.IsLoadOrg4Editor())
            {
                return;
            }
            AssetBundle ab = AssetBundle.LoadFromFile(path); // LoadFromMemory

            if (ab == null)
            {
                LogErr("LoadMainfest ab is NULL , fp = [{0}]!", path);
                return;
            }

            AssetBundleManifest mainfest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest"); ;
            if (mainfest == null)
            {
                LogErr("LoadMainfest ab.mainfest is NULL , fp = [{0}]!", path);
                return;
            }

            foreach (string assetName in mainfest.GetAllAssetBundles())
            {
                // GetAllDependencies = 所有依赖的AssetBundle名字
                // GetDirectDependencies = 直接依赖的AssetBundle名字
                string[] dps = mainfest.GetDirectDependencies(assetName);
                _dependsList.Add(assetName, dps);
            }
            ab.Unload(true);
            ab = null;
        }

        public string[] GetDependences(string abName)
        {
            if (!string.IsNullOrEmpty(abName) && this._dependsList.ContainsKey(abName))
            {
                return this._dependsList[abName];
            }
            return null;
        }

        private void ExcAction(Action cfLoaded)
        {
            if (cfLoaded != null) cfLoaded();
        }

        public void LoadShadersAndWarmUp(string abName, string assetName, Action cfLoaded)
        {
            if (UGameFile.curInstance.IsLoadOrg4Editor() || string.IsNullOrEmpty(abName) || !abName.EndsWith(UGameFile.m_strSVC))
            {
                ExcAction(cfLoaded);
                return;
            }

            if (string.IsNullOrEmpty(assetName))
                assetName = UGameFile.GetFileNameNoSuffix(abName);

            if (!assetName.EndsWith(".shadervariants"))
                assetName = assetName + ".shadervariants";

            LoadAsset<ShaderVariantCollection>(abName, assetName, abObj =>
            {
                AssetInfo aInfo = abObj as AssetInfo;
                if (aInfo != null && aInfo.isHasObj)
                {
                    ABInfo _abInfo = aInfo.GetAssetBundleInfo();
                    _abInfo.m_isStayForever = true;

                    ShaderVariantCollection _svc = aInfo.GetObject<ShaderVariantCollection>();
                    if (_svc != null && !_svc.isWarmedUp)
                        _svc.WarmUp();
                }
                ExcAction(cfLoaded);
            });
        }

        public void LoadShadersAndWarmUp(Action cfLoaded)
        {
            this.LoadShadersAndWarmUp("all_svc.ab_svc", null, cfLoaded);
        }

        /// <summary>
        ///  更新
        /// </summary>
        override public void OnUpdate(float dt, float unscaledDt)
        {
            UpdateLoad(dt, unscaledDt);
            UpdateReady(dt, unscaledDt);
            UpdateUnLoad(dt, unscaledDt);
            UpdateNdUnLoad(dt, unscaledDt);
        }

        void UpdateLoad(float dt, float unscaledDt)
        {
            upTemp.AddRange(this._loading.m_list);
            nUpLens = upTemp.Count;
            for (int i = 0; i < nUpLens; i++)
            {
                upInfo = upTemp[i];
                upInfo.OnUpdate(dt, unscaledDt);
                if (upInfo.m_isDoned)
                {
                    upList.Add(upInfo);
                }
            }
            upTemp.Clear();

            nUpLens = upList.Count;
            for (int i = 0; i < nUpLens; i++)
            {
                upInfo = upList[i];
                upInfo.OnUpdate(dt, unscaledDt);
                this._loading.Remove(upInfo.m_abName);

                switch (upInfo.m_abState)
                {
                    case ET_AssetBundle.CompleteLoad:
                        Add_Loaded(upInfo);
                        break;
                    case ET_AssetBundle.Err_Null_AssetBundle:
                        LogErr("ab is null,abName = [{0}] , path = [{1}]", upInfo.m_abName, upInfo.m_fpath);
                        break;
                    case ET_AssetBundle.Err_Null_Abcr:
                        LogErr("ab CreateRequest is null,abName = [{0}] , path = [{1}]", upInfo.m_abName, upInfo.m_fpath);
                        break;
                }
            }
            upList.Clear();
        }

        void UpdateReady(float dt, float unscaledDt)
        {
            upTemp.AddRange(this._ndLoad.m_list);
            int _l1 = upTemp.Count;
            if (_l1 <= 0)
            {
                return;
            }

            int _l2 = this._loading.m_list.Count;
            nUpLens = this.m_nMaxLoad - _l2;
            nUpLens = (nUpLens > _l1) ? _l1 : nUpLens;
            string _abName = null;
            for (int i = 0; i < nUpLens; i++)
            {
                upInfo = upTemp[i];
                _abName = upInfo.m_abName;
                this._ndLoad.Remove(_abName);
                this._loading.Add(_abName, upInfo);
            }
            upTemp.Clear();
        }

        void UpdateUnLoad(float dt, float unscaledDt)
        {
            upTemp.AddRange(this._unLoad.m_list);
            nUpLens = upTemp.Count;
            if (nUpLens <= 0)
                return;

            for (int i = 0; i < nUpLens; i++)
            {
                upInfo = upTemp[i];
                this._unLoad.Remove(upInfo.m_abName);
                upInfo.DoUnLoad();
            }
            upTemp.Clear();
        }

        void UpdateNdUnLoad(float dt, float unscaledDt)
        {
            upTemp.AddRange(this._ndUnLoad.m_list);
            nUpLens = upTemp.Count;
            if (nUpLens <= 0)
                return;

            for (int i = 0; i < nUpLens; i++)
            {
                upInfo = upTemp[i];
                upInfo.UpDestroy(unscaledDt);
            }
            upTemp.Clear();
        }

        public string AllInfo(int printType = 0)
        {
            List<ABInfo> _list1 = new List<ABInfo>(this._ndLoad.m_list);
            List<ABInfo> _list2 = new List<ABInfo>(this._loading.m_list);
            List<ABInfo> _list3 = new List<ABInfo>(this._loaded.m_list);
            List<ABInfo> _list4 = new List<ABInfo>(this._ndUnLoad.m_list);
            List<ABInfo> _list5 = new List<ABInfo>(this._unLoad.m_list);
            int _lens = _list1.Count;
            var _build = new System.Text.StringBuilder();
            _build.AppendFormat("====== ndLoad =").AppendLine();
            for (int i = 0; i < _lens; i++)
            {
                _build.Append(_list1[i]).AppendLine();
            }
            _build.AppendLine();

            _build.AppendFormat("====== loading =").AppendLine();
            _lens = _list2.Count;
            for (int i = 0; i < _lens; i++)
            {
                _build.Append(_list2[i]).AppendLine();
            }
            _build.AppendLine();

            _build.AppendFormat("====== loaded =").AppendLine();
            _lens = _list3.Count;
            for (int i = 0; i < _lens; i++)
            {
                _build.Append(_list3[i]).AppendLine();
            }
            _build.AppendLine();

            _build.AppendFormat("====== ndUnLoad =").AppendLine();
            _lens = _list4.Count;
            for (int i = 0; i < _lens; i++)
            {
                _build.Append(_list4[i]).AppendLine();
            }
            _build.AppendLine();

            _build.AppendFormat("====== unLoad =").AppendLine();
            _lens = _list5.Count;
            for (int i = 0; i < _lens; i++)
            {
                _build.Append(_list5[i]).AppendLine();
            }
            _build.AppendLine();

            string _v = _build.ToString();
            _build.Clear();
            _build.Length = 0;

            switch (printType)
            {
                case 1:
                    Debug.LogError(_v);
                    break;
                case 2:
                    Debug.Log(_v);
                    break;
            }
            return _v;
        }

        public bool Add_NdLoad(ABInfo abInfo)
        {
            if (null == abInfo || abInfo.isUnloaded) return false;
            abInfo.m_abUnloadState = ET_AssetBundle.None;
            _ndLoad.Add(abInfo.m_abName, abInfo);
            return true;
        }

        public bool Add_Loaded(ABInfo abInfo)
        {
            if (null == abInfo || abInfo.isUnloaded) return false;
            if (!abInfo.m_isDoned || !abInfo.isLoaded) return false;
            abInfo.m_abUnloadState = ET_AssetBundle.None;
            _loaded.Add(abInfo.m_abName, abInfo);
            return true;
        }

        public ABInfo GetABInfo(string abName)
        {
            if (this.isAppQuit) return null;

            ABInfo _abInfo = _loaded.Get(abName);
            if (_abInfo == null)
            {
                _abInfo = _ndLoad.Get(abName);
            }
            if (_abInfo == null)
            {
                _abInfo = _loading.Get(abName);
            }
            return _abInfo;
        }

        void _ExcCall(DF_LoadedAsset cfunc, AssetBase info)
        {
            if (cfunc != null)
                cfunc(info);
        }

        private void _AddABLoadCall(DF_LoadedAsset cfunc, ABInfo info)
        {
            if (cfunc != null)
                info.m_onLoadedAB += cfunc;
        }

        public ABInfo GetInUnloadDic(string abName)
        {
            ABInfo _ab_ = this._ndUnLoad.Remove4Get(abName);
            ABInfo _ab_2 = this._unLoad.Remove4Get(abName);

            if (_ab_ != null)
            {
                bool _isBl = false;
                if (_ab_.m_isDoned)
                    _isBl = Add_Loaded(_ab_);
                else
                    _isBl = Add_NdLoad(_ab_);

                if (_isBl)
                {
                    _ab_.CheckDep2ReUse();
                    return _ab_;
                }
                _ab_.DoUnLoad();
            }
            
            if (_ab_2 != null)
                _ab_2.DoUnLoad();
            return null;
        }

        public ABInfo LoadAB(string abName, DF_LoadedAsset cfunc)
        {
            ABInfo _abInfo = GetInUnloadDic(abName);
            if (_abInfo == null)
                _abInfo = GetABInfo(abName);

            if (_abInfo != null)
            {
                if (_abInfo.m_isDoned)
                {
                    if (_abInfo.isLoaded)
                    {
                        _ExcCall(cfunc, _abInfo);
                    }
                    else
                    {
                        LogErr(_abInfo);
                        _abInfo = null; // 出错的
                    }
                }
                else
                {
                    _AddABLoadCall(cfunc, _abInfo);
                }
            }

            if (_abInfo != null) return _abInfo;

            string[] _des = GetDependences(abName);
            if (_des == null)
            {
                LogErr("=== ab has no dependencies, abname = [{0}]", abName);
                return null;
            }

            _abInfo = new ABInfo(abName);
            _AddABLoadCall(cfunc, _abInfo);
            Add_NdLoad(_abInfo);
            // 通知一下
            if (this.m_call4NewABInfo != null)
                this.m_call4NewABInfo(abName);
            // 依赖关系
            _LoadABDeps(_abInfo);
            return _abInfo;
        }

        private void _LoadABDeps(ABInfo ab)
        {
            string abName = ab.m_abName;
            string[] _des = GetDependences(abName);
            if (_des == null)
            {
                return;
            }

            // 依赖关系
            int lens = _des.Length;
            ABInfo abDep = null;
            for (int i = 0; i < lens; i++)
            {
                abDep = LoadABDep(_des[i], ab);
                ab.AddNeedDeps(abDep);
            }
        }

        private ABInfo LoadABDep(string abName4Dep, ABInfo abInfo, Action cfLoad = null)
        {
            ABInfo abDep = LoadAB(abName4Dep, (obj) =>
            {
                ExcAction(cfLoad);
            });
            if (abDep != null)
                abDep.RefCount();
            return abDep;
        }

        public AssetInfo LoadAsset(string abName, string assetName, Type assetType, DF_LoadedAsset cfunc)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                _ExcCall(cfunc, null);
                return null;
            }

            ABInfo _abInfo = LoadAB(abName, null);
            if (_abInfo == null)
            {
                _ExcCall(cfunc, null);
                return null;
            }

            AssetInfo _info = _abInfo.GetAssetAndCount(assetName, assetType);
            if (_info != null)
            {
                _info.StopAllUpdate();
                _info.ReEvent4LoadAsset(cfunc, true);
                // _info.JudgeStart();
                this._listLate.Add(_info);
            }
            return _info;
        }

        public AssetInfo LoadAsset<T>(string abName, string assetName, DF_LoadedAsset cfunc) where T : UnityEngine.Object
        {
            Type assetType = typeof(T);
            return LoadAsset(abName, assetName, assetType, cfunc);
        }

        public AssetInfo GetAssetInfo(string abName, string assetName, Type assetType)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }

            ABInfo _abInfo = GetABInfo(abName);
            if (_abInfo == null)
            {
                return null;
            }

            return _abInfo.GetAsset(assetName, assetType);
        }

        public AssetInfo GetAssetInfo<T>(string abName, string assetName) where T : UnityEngine.Object
        {
            Type assetType = typeof(T);
            return GetAssetInfo(abName, assetName, assetType);
        }

        public void DoUnLoadAB(string abName)
        {
            if (this.isAppQuit) return;
            ABInfo _ab = GetABInfo(abName);
            this._ndLoad.Remove(abName);
            this._loading.Remove(abName);
            this._loaded.Remove(abName);
            this._ndUnLoad.Remove(abName);

            if (_ab != null)
                this._unLoad.Add(abName, _ab);
        }

        public void UnLoadAB(string abName)
        {
            if (this.isAppQuit) return;
            ABInfo _ab = GetABInfo(abName);
            this._ndLoad.Remove(abName);
            this._loading.Remove(abName);
            this._loaded.Remove(abName);

            if (_ab != null && !this._unLoad.ContainsKey(abName))
                this._ndUnLoad.Add(abName, _ab);
        }

        public void UnLoadAB(ABInfo abInfo)
        {
            if (this.isAppQuit || null == abInfo) return;
            abInfo.Unload();
        }

        public void UnLoadAsset(string abName)
        {
            ABInfo _ab = GetABInfo(abName);
            UnLoadAB(_ab);
        }

        public void UnLoadAsset(AssetInfo info)
        {
            if (info == null)
                return;
            info.UnloadAsset();
        }

        public void AddOnlyNewABInfo(DF_OnStr call)
        {
            if (call == null)
            {
                this.m_call4NewABInfo = null;
                return;
            }
            this.m_call4NewABInfo -= call;
            this.m_call4NewABInfo += call;
        }
    }
}
