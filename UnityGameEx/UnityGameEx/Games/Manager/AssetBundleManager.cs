using UnityEngine;
using System;
using System.Collections.Generic;
namespace Core
{
    using Core.Kernel;
    using UObject = UnityEngine.Object;
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
        // static long weightMax = long.MaxValue - 1000000;
        static long weightCurr = long.MaxValue - 1000000;

        public int m_nMaxLoad = 106; // 限定加载
        public float m_secLmtUp = 0.022f;
        float _m_secUp = 0f;
        public bool m_isCanLockLoadAB = true;
        private object _objLock = new object();
        private Dictionary<string, string[]> _dependsList = new Dictionary<string, string[]>(); // 依赖关系
        private ListDict<ABInfo> _infos = new ListDict<ABInfo>(true); // 所有的 AB
        private HashSet<string> _ndLoad = new HashSet<string>(); // 需要加载的 AB
        private HashSet<string> _loading = new HashSet<string>(); // 正在加载的 AB
        private HashSet<string> _loaded = new HashSet<string>(); // 已经加载了的 AB
        private HashSet<string> _ndUnLoad = new HashSet<string>(); // 需要销毁的 AB
        AssetBase.SortABase _sortAB = new AssetBase.SortABase();

        private bool m_isInit = false;
        public bool isDebug = false; //是否打印
        private DF_OnStr m_call4NewABInfo = null;

        private List<AssetInfo> _listLate = new List<AssetInfo>();
        private float _abOutSec = -1f;
        public float m_abOutSec
        {
            get { return this._abOutSec; }
            set { this._abOutSec = value; }
        }

        private List<string> upTemp = new List<string>();
        private List<string> upList = new List<string>();

        public void Init()
        {
            if (this.m_isInit)
                return;

            this.m_isInit = true;
            this.isDebug = UGameFile.m_isEditor; //是否打印
            this.StartUpdate();
            GameMgr.DisposeLateUpEvent(this._OnLateUpEnd, true);
            this.LoadMainfest();
        }

        /// <summary>
        ///  初始化
        /// </summary>
        override protected void OnCall4Awake()
        {
            this.csAlias = "ABMgr";
        }

        /// <summary>
        /// 销毁
        /// </summary>
        override protected void OnCall4Destroy()
        {
            GameMgr.DisposeLateUpEvent(this._OnLateUpEnd, false);
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

            AssetBundleManifest mainfest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
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

            AssetInfo _assetInfo = GetAssetInfo<ShaderVariantCollection>(abName, assetName);
            if (_assetInfo != null && _assetInfo.m_isDoned)
            {
                ExcAction(cfLoaded);
                return;
            }

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

        public void LoadShadersAndWarmUp(string abName, Action cfLoaded)
        {
            this.LoadShadersAndWarmUp(abName, null, cfLoaded);
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
            if (this.m_secLmtUp > 0.01f)
            {
                this._m_secUp += unscaledDt;
                if (this._m_secUp < this.m_secLmtUp)
                    return;

                this._m_secUp -= this.m_secLmtUp;
                dt = this.m_secLmtUp;
                unscaledDt = this.m_secLmtUp;
            }

            UpdateLoad(dt, unscaledDt);
            UpdateReady(dt, unscaledDt);
            UpdateNdUnLoad(dt, unscaledDt);
        }

        public ABInfo GetABInfo4Infos(string abName)
        {
            if (this.isAppQuit) return null;
            return this._infos.Get(abName);
        }

        public bool TryGetABInfo(string abName, out ABInfo info)
        {
            info = this.GetABInfo4Infos(abName);
            if (info == null)
                return false;
            else if (info.m_isInUnload)
            {
                info = null;
                return false;
            }
            return true;
        }

        void UpdateLoad(float dt, float unscaledDt)
        {
            upTemp.AddRange(this._loading);
            int _lens = upTemp.Count;
            if (_lens <= 0)
                return;

            string _abName;
            ABInfo upInfo;
            bool _isOkey;
            for (int i = 0; i < _lens; i++)
            {
                _abName = upTemp[i];
                _isOkey = this.TryGetABInfo(_abName, out upInfo);
                if (!_isOkey)
                {
                    upList.Add(_abName);
                    continue;
                }
                upInfo.OnUpdate(dt, unscaledDt);
                if (upInfo.m_isDoned)
                    upList.Add(_abName);
            }
            upTemp.Clear();

            _lens = upList.Count;
            for (int i = 0; i < _lens; i++)
            {
                _abName = upList[i];
                this._loading.Remove(_abName);

                _isOkey = this.TryGetABInfo(_abName, out upInfo);
                if (_isOkey)
                {
                    switch (upInfo.m_abState)
                    {
                        case ET_AssetBundle.CompleteLoad:
                            Add_Loaded(upInfo);
                            break;
                        case ET_AssetBundle.Err_Null_AssetBundle:
                            LogErr("ab is null , [{0}]", upInfo);
                            this.DoUnLoadAB(upInfo.m_abName);
                            break;
                        case ET_AssetBundle.Err_Null_Abcr:
                            LogErr("ab CreateRequest is null , [{0}]", upInfo);
                            this.DoUnLoadAB(upInfo.m_abName);
                            break;
                    }
                }
            }
            upList.Clear();
        }

        void UpdateReady(float dt, float unscaledDt)
        {
            upTemp.AddRange(this._ndLoad);
            int _l1 = upTemp.Count;
            if (_l1 <= 0)
                return;

            upTemp.Sort(_sortAB);
            string _abName;
            ABInfo upInfo;
            bool _isOkey;
            int _l2 = this._loading.Count;
            int _lens = this.m_nMaxLoad - _l2;
            _lens = (_lens > _l1) ? _l1 : _lens;
            for (int i = 0; i < _lens; i++)
            {
                _abName = upTemp[i];
                this._ndLoad.Remove(_abName);

                _isOkey = this.TryGetABInfo(_abName, out upInfo);
                if (_isOkey)
                    this._loading.Add(_abName);
            }
            upTemp.Clear();
        }

        void UpdateNdUnLoad(float dt, float unscaledDt)
        {
            upTemp.AddRange(this._ndUnLoad);
            int _lens = upTemp.Count;
            if (_lens <= 0)
                return;

            string _abName;
            ABInfo upInfo;

            for (int i = 0; i < _lens; i++)
            {
                _abName = upTemp[i];
                upInfo = this.GetABInfo4Infos(_abName);
                if (upInfo != null)
                {
                    upInfo.UpDestroy(unscaledDt, this.m_abOutSec);
                }
            }
            upTemp.Clear();
        }

        public string AllInfo(int printType = 0)
        {
            var _list1 = new List<string>(this._ndLoad);
            var _list2 = new List<string>(this._loading);
            var _list3 = new List<string>(this._loaded);
            var _list4 = new List<string>(this._ndUnLoad);
            List<ABInfo> _list5 = new List<ABInfo>(this._infos.m_list);
            int _lens = _list1.Count;
            var _build = new System.Text.StringBuilder();
            _build.AppendFormat("====== ndLoad [{0}] =", _lens).AppendLine();
            for (int i = 0; i < _lens; i++)
            {
                _build.Append(_list1[i]).AppendLine();
            }
            _build.AppendLine();

            _lens = _list2.Count;
            _build.AppendFormat("====== loading [{0}] =", _lens).AppendLine();
            for (int i = 0; i < _lens; i++)
            {
                _build.Append(_list2[i]).AppendLine();
            }
            _build.AppendLine();

            _lens = _list3.Count;
            _build.AppendFormat("====== loaded [{0}] =", _lens).AppendLine();
            for (int i = 0; i < _lens; i++)
            {
                _build.Append(_list3[i]).AppendLine();
            }
            _build.AppendLine();

            _lens = _list4.Count;
            _build.AppendFormat("====== ndUnLoad [{0}] =", _lens).AppendLine();
            for (int i = 0; i < _lens; i++)
            {
                _build.Append(_list4[i]).AppendLine();
            }
            _build.AppendLine();

            _lens = _list5.Count;
            _build.AppendFormat("====== alls [{0}] =", _lens).AppendLine();
            for (int i = 0; i < _lens; i++)
            {
                _list5[i].ToStringBy(ref _build);
                _build.AppendLine();
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

        private bool Add_NdLoad(ABInfo ab)
        {
            if (null == ab || ab.isUnloaded) return false;
            _ndLoad.Add(ab.m_abName);
            return true;
        }

        private bool Add_Loaded(ABInfo ab)
        {
            if (null == ab || ab.isUnloaded) return false;
            _loaded.Add(ab.m_abName);
            return true;
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

        public ABInfo GetABInfo(string abName, bool isJugde = true)
        {
            ABInfo _ab_ = this.GetABInfo4Infos(abName);
            bool _isCanUnload = false;
            if (_ab_ != null)
            {
                if (isJugde && _ab_.m_isInUnload)
                {
                    this._ndUnLoad.Remove(abName);
                    _isCanUnload = _ab_.JugdeDestroy();
                    if (!_isCanUnload)
                    {
                        bool _isBl = _ab_.CheckDep2ReUse();
                        if (_isBl)
                        {
                            if (_ab_.m_isDoned)
                                Add_Loaded(_ab_);
                            else
                                Add_NdLoad(_ab_);
                        }

                        if (_isBl)
                        {
                            this._ndUnLoad.Remove(abName);
                            return _ab_;
                        }
                        _isCanUnload = true;
                    }
                }

                if (_ab_.m_isDoned && !_ab_.isLoaded)
                {
                    _isCanUnload = true;
                    LogErr("GetABInfo is not Loaded successed , [{0}]", _ab_);
                }
            }

            if (_isCanUnload)
            {
                this.DoUnLoadAB(abName);
                _ab_ = null;
            }
            return _ab_;
        }

        public ABInfo LoadAB(string abName, DF_LoadedAsset cfunc, long weightParent)
        {
            if (this.m_isCanLockLoadAB)
            {
                lock (this._objLock)
                {
                    return this._LoadAB(abName, cfunc, weightParent);
                }
            }
            return this._LoadAB(abName, cfunc, weightParent);
        }

        private ABInfo _LoadAB(string abName, DF_LoadedAsset cfunc, long weightParent)
        {
            ABInfo _abInfo = GetABInfo(abName, true);
            if (_abInfo != null)
            {
                if (_abInfo.m_isDoned && _abInfo.isLoaded)
                    _ExcCall(cfunc, _abInfo);
                else
                    _AddABLoadCall(cfunc, _abInfo);
                return _abInfo;
            }

            string[] _des = GetDependences(abName);
            if (_des == null)
            {
                LogErr("=== ab has no dependencies, abname = [{0}]", abName);
                return null;
            }
            if (0 == weightParent)
            {
                --weightCurr;
                if (weightCurr == 0)
                    --weightCurr;
                weightParent = weightCurr;
            }

            _abInfo = ABInfo.Builder(abName, this.m_abOutSec, cfunc, weightParent);
            this._infos.Add(abName, _abInfo);
            // 通知一下
            if (this.m_call4NewABInfo != null)
                this.m_call4NewABInfo(abName);
            // 依赖关系
            _LoadABDeps(_abInfo);

            Add_NdLoad(_abInfo);
            return _abInfo;
        }

        private void _LoadABDeps(ABInfo ab)
        {
            string abName = ab.m_abName;
            string[] _des = GetDependences(abName);
            if (_des == null || _des.Length <= 0)
                return;

            // 依赖关系
            int lens = _des.Length;
            for (int i = 0; i < lens; i++)
                LoadABDep(_des[i], ab);
        }

        private void LoadABDep(string abName4Dep, ABInfo abInfo, Action cfLoad = null)
        {
            if (abInfo == null || abInfo.m_isInUnload || abInfo.IsHasNeedDeps(abName4Dep))
                return;
            DF_LoadedAsset _call = null;
            if (cfLoad != null)
            {
                _call = obj => { ExcAction(cfLoad); };
            }
            ABInfo abDep = LoadAB(abName4Dep, _call, abInfo.m_weight);
            if (abDep == null)
                return;
            abDep.RefCount();
            abInfo.AddNeedDeps(abDep);
        }

        public AssetInfo LoadAsset(string abName, string assetName, Type assetType, DF_LoadedAsset cfunc)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                _ExcCall(cfunc, null);
                return null;
            }

            ABInfo _abInfo = LoadAB(abName, null, 0);
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

        public AssetInfo LoadAsset<T>(string abName, string assetName, DF_LoadedAsset cfunc) where T : UObject
        {
            Type assetType = typeof(T);
            return LoadAsset(abName, assetName, assetType, cfunc);
        }

        public T LoadObjectSync<T>(string abName, string assetName) where T : UObject
        {
            Type assetType = typeof(T);
            AssetInfo info = LoadAsset(abName, assetName, assetType, null);
            return info?.GetObject<T>();
        }

        public GameObject LoadFabSync(string abName, string assetName, UObject uobjParent)
        {
            Type assetType = typeof(GameObject);
            AssetInfo info = LoadAsset(abName, assetName, assetType, null);
            Transform _parent = UtilityHelper.ToTransform(uobjParent);
            if (info != null)
                return info.NewGObjInstance(_parent);
            return null;
        }

        public AssetInfo GetAssetInfo(string abName, string assetName, Type assetType)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }

            ABInfo _abInfo = GetABInfo(abName, true);
            if (_abInfo == null)
            {
                return null;
            }

            return _abInfo.GetAsset(assetName, assetType);
        }

        public AssetInfo GetAssetInfo<T>(string abName, string assetName) where T : UObject
        {
            Type assetType = typeof(T);
            return GetAssetInfo(abName, assetName, assetType);
        }

        private ABInfo _DiscardAndGet(string abName)
        {
            this._ndLoad.Remove(abName);
            this._loading.Remove(abName);
            this._loaded.Remove(abName);
            return this.GetABInfo4Infos(abName);
        }

        private void _Add2NdUnload(string abName)
        {
            if (!string.IsNullOrEmpty(abName))
                this._ndUnLoad.Add(abName);
        }

        public void Add2NdUnload(ABInfo ab)
        {
            if (this.isAppQuit || ab == null || ab.isUnloaded) return;
            string abName = ab.m_abName;
            this._DiscardAndGet(abName);
            this._Add2NdUnload(abName);
        }

        public void DoUnLoadAB(string abName)
        {
            if (this.isAppQuit) return;
            ABInfo _ab = _DiscardAndGet(abName);
            this._infos.Remove(abName);
            this._ndUnLoad.Remove(abName);
            if (_ab != null)
                _ab.DoUnLoad();
        }

        public void UnLoadAB(ABInfo abInfo)
        {
            if (this.isAppQuit || null == abInfo) return;
            abInfo.Unload();
        }

        public void UnLoadAsset(string abName)
        {
            ABInfo _ab = this.GetABInfo4Infos(abName);
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
