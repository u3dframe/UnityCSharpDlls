using UnityEngine;
using System;
using System.Collections.Generic;
namespace Core
{
    using Core.Kernel;
    using UObject = UnityEngine.Object;
    public delegate void DF_LoadedAsset(AssetBase asset);

    /// <summary>
    /// 类名 : ab 基础类
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2019-06-26 10:29
    /// 功能 : 用于继承
    /// </summary>
    public class AssetBase : Kernel.Beans.ED_Basic
    {
        public class SortABase : Comparer<string>
        {
            public override int Compare(string x, string y)
            {
                AssetBase vA = abMgr.GetABInfo(x, false);
                AssetBase vB = abMgr.GetABInfo(y, false);
                if (vA == null && vB != null)
                    return 1;
                else if (vA != null && vB == null)
                    return -1;
                else if (vA == null && vB == null)
                    return 0;
                if (vA.m_weight > vB.m_weight)
                    return -1;
                if (vA.m_weight < vB.m_weight)
                    return 1;
                return 0;
            }
        }
        static public AssetBundleManager abMgr { get { return AssetBundleManager.instance; } }

        public string m_abName; // assetbundle名
        public bool m_isASync = false; // sync 同步 ,async 异步
        public uint m_abCRC32 = 0;
        public ulong m_abOffset = 0;
        public long m_weight = 0;

        public ABInfo GetAssetBundleInfo()
        {
            return abMgr.GetABInfo(this.m_abName, false);
        }

        virtual public void UnloadAsset()
        {
            abMgr.UnLoadAsset(this.m_abName);
        }
    }

    /// <summary>
    /// 类名 : ab 的资源
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2019-06-26 10:29
    /// 功能 : 
    /// </summary>
    public class ABInfo : AssetBase
    {
        public string m_fpath; // 资源地址
        public ET_AssetBundle m_abState = ET_AssetBundle.None; // 状态机制
        private ET_AssetBundle m_preState = ET_AssetBundle.None; // 上一次状态
        private ET_AssetBundle m_abUnloadState = ET_AssetBundle.None; // 状态机制
        private ET_AssetBundle m_preUnloadState = ET_AssetBundle.None; // 上一次状态
        public bool m_isDoned { get; private set; } // 是否已经执行了 LoadAB
        private int m_depNeedCount = 0;// 需要的依赖关系的数量;
        private int m_depNeedLoaded = 0;// 依赖关系加载了的数量;

        private AssetBundleCreateRequest m_abcr; // 异步加载 AssetBundle
        public AssetBundle m_ab { get; private set; } // 资源包
        protected string[] m_allAssetNames;
        public DF_LoadedAsset m_onLoadedAB = null;
        private ListDict<ABInfo> m_depNeeds = new ListDict<ABInfo>(true); // 需要的依赖关系对象
        private ListDict<AssetInfo> m_assets = new ListDict<AssetInfo>(true); // 内部资源对象

        public int m_useCount { get; private set; } // 引用计数,用于自动卸载;
        public bool m_isImmUnload = false; // 当引用计数为0时候，是否立即释放
        private float m_timeout = 0; // 当引用计数为0时候，不立即释放时控制
        private bool m_isShader = false;
        private bool m_isSVC = false;
        private bool m_isLoadAll = false;
        public bool m_isStayForever = false; // 是否常驻，无需释放(shader资源)

        public bool m_isInUnload { get { return this.m_abUnloadState != ET_AssetBundle.None; } }
        public bool isUnloaded { get { return this.m_abUnloadState == ET_AssetBundle.Destroying || this.m_abUnloadState == ET_AssetBundle.Destroyed; } }
        public bool isLoaded { get { return this.m_abState == ET_AssetBundle.CompleteLoad; } }
        private float _defOutSecMin = 5f; // 3 分钟 60 * 3 = 180
        private float _defOutSecMax = 20f; // 3 分钟 60 * 3 = 180

        private float _nearTimeOut = 0.3f; // 临近点
        public float m_nearTimeOut
        {
            get { return this._nearTimeOut; }
            set
            {
                if (value >= 0)
                    this._nearTimeOut = value;
            }
        }

        private ABInfo() { }

        override public string ToString()
        {
            var _sb = new System.Text.StringBuilder();
            ToStringBy(ref _sb);
            string _v = _sb.ToString();
            _sb.Clear();
            _sb.Length = 0;
            return _v;
        }

        public void ToStringBy(ref System.Text.StringBuilder _sb)
        {
            if (_sb == null)
                return;
            Dictionary<string, object> _dic = new Dictionary<string, object>
            {
                {"abName",this.m_abName },
                {"fpath",this.m_fpath },
                {"isDone",this.m_isDoned },
                {"state",this.m_abState },
                {"preState",this.m_preState },
                {"unloadState",this.m_abUnloadState },
                {"preUnloadState",this.m_preUnloadState },
                {"beUseCount",this.m_useCount },
                {"needDepCount",this.m_depNeedCount },
                {"needDepCount_List",this.m_depNeeds.Count() },
                {"needDepCountLoaded",this.m_depNeedLoaded },
                {"assetCount",this.m_assets.Count() },
                {"isImmUnload",this.m_isImmUnload },
                {"timeout",this.m_timeout },
            };

            foreach (var item in _dic)
                _sb.AppendFormat("[{0}] = [{1}],", item.Key, item.Value);

            if (this.m_allAssetNames != null)
            {
                string[] arrs = this.m_allAssetNames;
                for (int i = 0; i < arrs.Length; i++)
                {
                    _sb.AppendFormat("[{0}],", arrs[i]);
                }
            }
        }

        private ABInfo _Init(string abName, float secOut, DF_LoadedAsset cfunc,long weightParent)
        {
            this.m_abName = abName;
            this.m_isShader = UGameFile.IsShaderAB(abName);
            this.m_isSVC = UGameFile.IsSVCAB(abName);
            this.m_isLoadAll = m_isShader || m_isSVC;
            this.m_fpath = UGameFile.curInstance.GetPath(this.m_abName);
            this.SetUnloadState(ET_AssetBundle.None);
            this.SetUpState(ET_AssetBundle.CheckNeedDown);

            this._ReOutSceMinMax(secOut);
            this.m_onLoadedAB = cfunc;
            this.m_weight += weightParent;
            return this;
        }

        void _ReOutSceMinMax(float secOut)
        {
            if (secOut >= 0)
            {
                this._defOutSecMax = secOut;
                this._defOutSecMin = secOut - 2.6f;
                this._defOutSecMin = Mathf.Max(0, this._defOutSecMin);
            }
        }

        /// <summary>
        ///  更新
        /// </summary>
        override public void OnUpdate(float dt, float unscaledDt)
        {
            UpLoadAB();
        }

        void UpLoadAB()
        {
            if (m_isDoned)
            {
                _ExcuteLoadedCall();
                return;
            }

            switch (m_abState)
            {
                case ET_AssetBundle.WaitLoadDeps:
                    {
                        this._CheckCount4DepLoaded();
                        if (this.m_depNeedLoaded >= this.m_depNeedCount)
                            SetUpState(ET_AssetBundle.PreLoad);
                    }
                    break;
                case ET_AssetBundle.CheckNeedDown:
                    {
                        if (UGameFile.IsFile(this.m_fpath))
                        {
                            SetUpState(ET_AssetBundle.WaitLoadDeps);
                        }
                        else if (CfgFileList.instanceDowning.IsHas(this.m_abName))
                        {
                            SetUpState(ET_AssetBundle.WaitCommand);
                            MgrDownload.shareInstance.AddPreLoad(this.m_abName, true, _CFDownFile);
                        }
                        else if (CfgFileList.instance.IsHas(this.m_abName))
                        {
                            if (CfgFileList.instanceDowned.IsHas(this.m_abName))
                            {
                                SetUpState(ET_AssetBundle.WaitCommand);
                                MgrDownload.shareInstance.AddPreLoad(this.m_abName, true, _CFDownFile);
                            }
                            else
                            {
                                SetUpState(ET_AssetBundle.WaitLoadDeps);
                            }
                        }
                        else
                        {
                            this.Finished();
                            Debug.LogErrorFormat("=== not find ab,\n[{0}]", this);
                        }
                    }
                    break;
                case ET_AssetBundle.PreLoad:
                    {
                        if (this.m_ab != null)
                        {
                            this.Finished();
                            return;
                        }
                        if (!this.m_isASync)
                        {
                            if (this.m_abcr != null)
                            {
                                if (this.m_abcr.assetBundle != null && this.m_abcr.isDone)
                                {
                                    this.m_ab = this.m_abcr.assetBundle;
                                    this._DoComplete();
                                }
                            }
                            else
                            {
                                this.m_ab = AssetBundle.LoadFromFile(this.m_fpath, this.m_abCRC32, this.m_abOffset);
                                this._DoComplete();
                            }

                            return;
                        }

                        if (this.m_abcr == null)
                            this.m_abcr = AssetBundle.LoadFromFileAsync(this.m_fpath, this.m_abCRC32, this.m_abOffset);
                        SetUpState(ET_AssetBundle.Loading);
                    }
                    break;
                case ET_AssetBundle.Loading:
                    {
                        if (this.m_ab != null)
                        {
                            this.Finished();
                            return;
                        }

                        if (this.m_abcr == null)
                        {
                            this.m_isDoned = true;
                            SetUpState(ET_AssetBundle.Err_Null_Abcr); // m_abcr 为空了
                            return;
                        }

                        if (!this.m_abcr.isDone)
                        {
                            return;
                        }

                        if (this.m_abcr.assetBundle == null)
                        {
                            this.m_isDoned = true;
                            SetUpState(ET_AssetBundle.Err_Null_AssetBundle); // 资源 assetBundle 为空了
                            return;
                        }

                        this.m_ab = this.m_abcr.assetBundle;
                        this._DoComplete();
                    }
                    break;
            }
        }

        void _DoComplete()
        {
            this._ReAllAssetNames();
            if (this.m_isLoadAll)
                this.LoadAllASync();

            this.Finished();
        }

        void _CFDownFile(int state, ResInfo dlFile)
        {
            this.m_fpath = UGameFile.curInstance.GetPath(this.m_abName);
            SetUpState(ET_AssetBundle.CheckNeedDown);
        }

        void _ReAllAssetNames()
        {
            string[] arrs = this.m_ab.GetAllAssetNames();
            int _lens_ = arrs.Length;
            string[] _a2 = new string[_lens_];
            for (int i = 0; i < _lens_; i++)
            {
                _a2[i] = UGameFile.GetFileName(arrs[i]);
            }
            this.m_allAssetNames = _a2;
        }

        public bool IsHasAsset(string assetName)
        {
            if (null != this.m_allAssetNames)
            {
                string _tolow1 = assetName.ToLower();
                string _tolow2 = null;
                string[] arrs = this.m_allAssetNames;
                int _lens_ = arrs.Length;
                string fname = null;
                for (int i = 0; i < _lens_; i++)
                {
                    fname = arrs[i];
                    if (string.IsNullOrEmpty(fname))
                        continue;
                    _tolow2 = fname.ToLower();
                    if (_tolow2.StartsWith(_tolow1))
                        return true;
                }
            }
            return false;
        }

        string _ReAssetName(string assetName)
        {
            bool _hasSuf = UGameFile.IsHasSuffix(assetName);
            if (!_hasSuf && null != this.m_allAssetNames)
            {
                string[] arrs = this.m_allAssetNames;
                int _lens_ = arrs.Length;
                string fname = null;
                string _tolow1 = assetName.ToLower();
                string _tolow2 = null;
                for (int i = 0; i < _lens_; i++)
                {
                    fname = arrs[i];
                    if (string.IsNullOrEmpty(fname))
                        continue;
                    _tolow2 = fname.ToLower();
                    if (_tolow2.StartsWith(_tolow1))
                        return fname;
                }
            }

            return assetName;
        }

        void _CheckCount4DepLoaded()
        {
            var _list = this.m_depNeeds.m_list;
            int lens = _list.Count;
            this.m_depNeedLoaded = 0;
            ABInfo _dep_;
            for (int i = 0; i < lens; i++)
            {
                _dep_ = _list[i];
                if (_dep_.m_isDoned)
                    this.m_depNeedLoaded++;
            }
        }

        void Finished()
        {
            this.m_isDoned = true;
            SetUpState(ET_AssetBundle.CompleteLoad);
        }

        void SetUpState(ET_AssetBundle state)
        {
            this.m_preState = (state == ET_AssetBundle.None) ? state : this.m_abState;
            this.m_abState = state;
        }

        void _ExcuteLoadedCall()
        {
            var _func = this.m_onLoadedAB;
            this.m_onLoadedAB = null;
            if (_func != null)
                _func(this);
        }

        public AssetInfo GetAsset(string assetName, Type assetType)
        {
            try
            {
                string _key = _ReAssetName(assetName);
                if (assetType != null)
                    _key = string.Format("{0}_{1}", _key, assetType);

                return this.m_assets.Get(_key);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("=== GetAsset error = [{0}] \n[{1}]", ex, this);
            }
            return null;
        }

        private AssetInfo GetOrNewAsset(string assetName, Type assetType)
        {
            try
            {
                string _aName = _ReAssetName(assetName);
                string _key = _aName;
                if (assetType != null)
                    _key = string.Format("{0}_{1}", _aName, assetType);

                AssetInfo info = this.m_assets.Get(_key);
                if (info == null && !_key.Equals(_aName))
                {
                    info = this.m_assets.Get(_aName);
                    if (info != null)
                    {
                        if (info.m_objType == assetType)
                            this.m_assets.Add(_key, info);
                        else
                            info = null;
                    }
                }

                if (info == null)
                {
                    info = AssetInfo.Builder(this.m_abName, _aName, assetType);
                    this.m_assets.Add(_key, info);
                }
                return info;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("=== GetOrNewAsset error = [{0}] \n[{1}]", ex, this);
            }
            return null;
        }

        public AssetInfo GetAssetAndCount(string assetName, Type assetType)
        {
            AssetInfo info = GetOrNewAsset(assetName, assetType);
            if (info != null)
                RefCount();
            return info;
        }

        public bool IsHasNeedDeps(string abName)
        {
            bool _isHas = this.m_depNeeds.ContainsKey(abName);
            return _isHas;
        }

        public void AddNeedDeps(ABInfo info)
        {
            string _name = info.m_abName;
            if (this.IsHasNeedDeps(_name))
                return;
            this.m_depNeeds.Add(_name, info);
            this.m_depNeedCount++;
        }

        public void RefCount()
        {
            this.SetUnloadState(ET_AssetBundle.None);
            this.m_timeout = 0;
            this.m_useCount++;
        }

        public void SetDefOut(float sec)
        {
            if (sec < 0 || sec == this._defOutSecMax)
                return;

            float _diff = sec - this._defOutSecMax;
            this._ReOutSceMinMax(sec);

            if (_diff != 0 && this.m_timeout > 0)
            {
                this.m_timeout += _diff;
            }
        }

        bool IsStayForever()
        {
            return m_isStayForever || m_isShader || m_isSVC;
        }

        void SetUnloadState(ET_AssetBundle state)
        {
            this.m_preUnloadState = (state == ET_AssetBundle.None) ? state : this.m_abUnloadState;
            this.m_abUnloadState = state;
        }

        public void Unload()
        {
            --this.m_useCount;
            if (this.m_useCount > 0)
                return;

            if (IsStayForever())
            {
                this.m_useCount = 0;
                return;
            }

            this.JugdeDestroy();
        }

        public void UpDestroy(float dt, float defOutSec)
        {
            switch (this.m_abUnloadState)
            {
                case ET_AssetBundle.Destroy:
                    {
                        this.SetDefOut(defOutSec);
                        this.m_timeout -= dt;
                        if (this.m_timeout <= 0)
                            this._Destroying();
                    }
                    break;
            }
        }

        public bool JugdeDestroy()
        {
            if (this.m_useCount > 0)
                return false;

            if (this.isUnloaded)
                return true;

            if (this.m_abUnloadState == ET_AssetBundle.None)
            {
                if (this.m_isImmUnload || this._defOutSecMax < this.m_nearTimeOut)
                    this.m_timeout = 0;
                else
                {
                    float _diff = UnityEngine.Random.Range(this._defOutSecMin,this._defOutSecMax);
                    this.m_timeout = _diff;
                }
                this.SetUnloadState(ET_AssetBundle.PreDestroy);
            }

            bool _isCanUnload = (this.m_timeout - this.m_nearTimeOut) <= 0;
            if (_isCanUnload)
            {
                this._Destroying();
                return true;
            }

            if (this.m_abUnloadState == ET_AssetBundle.PreDestroy)
            {
                this.SetUnloadState(ET_AssetBundle.Destroy);
                abMgr.Add2NdUnload(this);
            }
            return false;
        }

        private void _Destroying()
        {
            this.SetUnloadState(ET_AssetBundle.Destroying);
            abMgr.DoUnLoadAB(this.m_abName);
        }

        public void DoUnLoad()
        {
            if (this.m_abUnloadState == ET_AssetBundle.Destroyed)
                return;
            this.ToUnload();
        }

        private void OnUnloadAB(bool isAll)
        {
            var _ab = this.m_ab;
            this.m_ab = null;
            AssetBundleCreateRequest _abcr = this.m_abcr;
            this.m_abcr = null;
            if (_ab != null)
                _ab.Unload(isAll);
            else if (_abcr != null && _abcr.assetBundle != null)
                _abcr.assetBundle.Unload(isAll);
        }

        private void ToUnload()
        {
            this.SetUnloadState(ET_AssetBundle.Destroyed);
            this.SetUpState(ET_AssetBundle.None);
            this.m_onLoadedAB = null;
            this.m_fpath = null;
            this.m_isDoned = false;
            this.m_depNeedCount = 0;
            this.m_depNeedLoaded = 0;
            this.m_weight = 0;

            var _list_ = this.m_assets.m_list;
            int lens = _list_.Count;
            AssetInfo _asset = null;
            for (int i = 0; i < lens; i++)
            {
                _asset = _list_[i];
                _asset.OnUnloadAsset();
            }
            this.m_assets.Clear();

            var _list = this.m_depNeeds.m_list;
            lens = _list.Count;
            ABInfo _a_ = null;
            for (int i = 0; i < lens; i++)
            {
                _a_ = _list[i];
                _a_.Unload();
            }
            this.m_depNeeds.Clear();

            this.m_allAssetNames = null;
            this.m_useCount = 0;
            this.m_isImmUnload = false;
            this.m_timeout = 0;
            this.m_isShader = false;
            this.m_isSVC = false;
            this.m_isLoadAll = false;
            this.m_isStayForever = false;
            this._nearTimeOut = 0.3f;

            this.OnUnloadAB(true);

            AddCache(this);
        }

        public bool CheckDep2ReUse()
        {
            if (this.isUnloaded)
                return false;
            bool _isDone = this.m_isDoned;
            this.SetUnloadState(ET_AssetBundle.None);
            ET_AssetBundle _abState = this.m_abState;

            var _rmList = new List<ABInfo>();
            var _list = this.m_depNeeds.m_list;
            int lens = _list.Count;
            ABInfo _dep_, _it;
            string _abName_;
            for (int i = 0; i < lens; i++)
            {
                _dep_ = _list[i];
                _abName_ = _dep_.m_abName;
                _it = abMgr.GetABInfo(_abName_, true);
                if (_it == null)
                    _rmList.Add(_dep_);
            }

            lens = _rmList.Count;
            bool _isNotRmv = lens <= 0;
            this.m_depNeedCount -= lens;
            for (int i = 0; i < lens; i++)
            {
                _dep_ = _rmList[i];
                _abName_ = _dep_.m_abName;

                this.m_depNeeds.Remove(_abName_);

                _dep_ = abMgr.LoadAB(_abName_, null, this.m_weight);
                this.AddNeedDeps(_dep_);
            }

            this.m_isDoned = _isDone && _isNotRmv;
            ET_AssetBundle _state = _isNotRmv ? _abState : ET_AssetBundle.CheckNeedDown;
            SetUpState(_state);
            return true;
        }

        private void LoadAllASync()
        {
            string[] arrs = this.m_allAssetNames;
            int _lens_ = arrs.Length;
            System.Type tp = null;
            // int _n_load_ = 0;
            for (int i = 0; i < _lens_; i++)
            {
                AssetInfo asInfo = this.GetOrNewAsset(arrs[i], tp);
                // asInfo.m_onLoadedAsset += (_obj)=>{
                // _n_load_++;
                // if(_n_load_ >= _lens_){
                // this.OnUnloadAB(false); // 会导致依赖关系丢失 shader 都会变成 Hidden/InternalErrorShader
                // }
                // };
                asInfo.StartUpdate();
            }
        }

        static public ABInfo Builder(string name, float secOut, DF_LoadedAsset cfunc, long weightParent)
        {
            ABInfo info = GetCache<ABInfo>();
            if (info == null)
                info = new ABInfo();
            info._Init(name, secOut, cfunc, weightParent);
            return info;
        }
    }

    /// <summary>
    /// 类名 : Asset资源
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2019-06-26 10:29
    /// 功能 : 
    /// </summary>
    public class AssetInfo : AssetBase
    {
        public string m_assetName;
        public Type m_assetType;
        public Type m_objType;

        private AssetBundleRequest m_abr; // 异步加载 Asset 资源
        public UObject m_obj;
        public string m_objName = ""; // 加载出来的资源的名字
        public long m_countNewGobj = 0;
        public bool m_isGobj = false;
        public ET_Asset m_upState = ET_Asset.None;
        public bool m_isDoned { get; private set; }

        private DF_LoadedAsset m_onLoadedAsset = null;
        private DF_LoadedAsset m_cfAppendLA = null;
        public bool isHasObj { get { return this.m_obj != null; } }
        public bool m_isCompleteLoaded { get { return this.m_upState == ET_Asset.CompleteLoad; } }
        private AssetInfo() { }

        override public string ToString()
        {
            var _sb = new System.Text.StringBuilder();
            ToStringBy(ref _sb);
            string _v = _sb.ToString();
            _sb.Clear();
            _sb.Length = 0;
            return _v;
        }

        public void ToStringBy(ref System.Text.StringBuilder _sb)
        {
            if (_sb == null)
                return;
            Dictionary<string, object> _dic = new Dictionary<string, object>
            {
                {"abName",this.m_abName },
                {"assetName",this.m_assetName },
                {"isDone",this.m_isDoned },
                {"assetType",this.m_assetType },
                {"objName",this.m_objName },
                {"objType",this.m_objType },
                {"upState",this.m_upState },
                {"isHasObj",this.isHasObj },
                {"countNewGobj",this.m_countNewGobj },
            };

            foreach (var item in _dic)
                _sb.AppendFormat("[{0}] = [{1}],", item.Key, item.Value);
        }

        private AssetInfo _Init(string abName, string assetName, Type assetType)
        {
            this.m_abName = abName;
            this.m_assetName = assetName;
            this.m_assetType = assetType;
            this.m_upState = ET_Asset.PreLoad;
            return this;
        }

        /// <summary>
        ///  更新
        /// </summary>
        override public void OnUpdate(float dt, float unscaledDt)
        {
            UpLoadAsset();
        }

        void UpLoadAsset()
        {
            if (m_isDoned)
            {
                _ExcuteLoadedCall();
                return;
            }

            switch (m_upState)
            {
                case ET_Asset.PreLoad:
                    {
                        var ab = GetAssetBundleInfo();
                        if (ab == null)
                        {
                            this.m_upState = ET_Asset.Err_Null_AbInfo; // abinfo对象为空了
                            this.m_isDoned = true;
                            return;
                        }

                        if (!ab.m_isDoned)
                        {
                            return;
                        }

                        if (ab.m_ab == null)
                        {
                            this.m_upState = ET_Asset.Err_Null_AssetBundle; // abinfo的资源AssetBundel为空了
                            this.m_isDoned = true;
                            return;
                        }

                        if (!ab.IsHasAsset(this.m_assetName))
                        {
                            this.Finished();
                            return;
                        }

                        if (!this.m_isASync)
                        {
                            if (m_assetType != null)
                                this.m_obj = ab.m_ab.LoadAsset(this.m_assetName, this.m_assetType);
                            else
                                this.m_obj = ab.m_ab.LoadAsset(this.m_assetName);
                            this._DoComplete();
                            return;
                        }
                        if (m_assetType != null)
                            this.m_abr = ab.m_ab.LoadAssetAsync(this.m_assetName, this.m_assetType);
                        else
                            this.m_abr = ab.m_ab.LoadAssetAsync(this.m_assetName);
                        this.m_upState = ET_Asset.Loading;
                    }
                    break;
                case ET_Asset.Loading:
                    {
                        if (this.m_abr == null)
                        {
                            this.m_upState = ET_Asset.Err_Null_Abr; // asset的加载对象为空了
                            this.m_isDoned = true;
                            return;
                        }

                        if (!this.m_abr.isDone)
                        {
                            return;
                        }
                        this.m_obj = this.m_abr.asset;
                        this._DoComplete();
                    }
                    break;
            }
        }

        void _DoComplete()
        {
            this.m_abr = null;
            if (this.m_obj != null)
            {
                this.m_objName = this.m_obj.name;
                this.m_objType = this.m_obj.GetType();
                this.m_isGobj = this.m_objType == UGameFile.tpGobj;

                ABShader.AddLoaded(this);
            }
            this.Finished();
        }

        void Finished()
        {
            this.m_isDoned = true;
            this.m_upState = ET_Asset.CompleteLoad;
        }

        void _ExcuteLoadedCall()
        {
            this.StopAllUpdate();

            var _func = this.m_onLoadedAsset;
            this.m_onLoadedAsset = null;
            if (_func != null)
            {
                _func(this);
            }
        }

        public void OnUnloadAsset()
        {
            this.StopAllUpdate();

            bool _isGobj = this.m_isGobj;
            this.m_upState = ET_Asset.None;
            this.m_assetType = null;
            this.m_objType = null;
            this.m_objName = "";
            this.m_countNewGobj = 0;
            this.m_isGobj = false;
            this.m_isDoned = false;
            this.m_onLoadedAsset = null;
            this.m_cfAppendLA = null;

            this.m_abr = null;
            var _obj = this.m_obj;
            this.m_obj = null;

            if (!_isGobj)
                UGameFile.UnLoadOne(_obj);
            AddCache(this);
        }

        public GameObject NewGObjInstance(Transform root)
        {
            if (!this.m_isGobj) return null;
            GameObject gobj = GameObject.Instantiate(this.m_obj, root, false) as GameObject;
            if (this.m_countNewGobj > 0)
            {
                gobj.name = string.Format("{0}_{1}", this.m_objName, this.m_countNewGobj);
            }
            else
            {
                gobj.name = this.m_objName;
            }
            this.m_countNewGobj++;
            return gobj;
        }

        public GameObject NewGObjInstance()
        {
            return NewGObjInstance(null);
        }

        public T GetObject<T>() where T : UnityEngine.Object
        {
            if (this.m_objType != null && this.m_objType == typeof(T))
            {
                return (T)this.m_obj;
            }

            return null;
        }

        override public void UnloadAsset()
        {
            this.StopAllUpdate();
            base.UnloadAsset();
        }

        public void ReEvent4LoadAsset(DF_LoadedAsset call, bool isReBind)
        {
            if (call == null)
                return;

            this.m_onLoadedAsset -= call;
            if (isReBind)
            {
                if (this.m_onLoadedAsset == null)
                    this.m_onLoadedAsset = call;
                else
                    this.m_cfAppendLA += call;
            }
        }

        public void JudgeStart()
        {
            DF_LoadedAsset _evt = this.m_cfAppendLA;
            this.m_cfAppendLA = null;
            if (_evt != null)
                this.m_onLoadedAsset += _evt;

            this.StartUpdate();
        }

        static public AssetInfo Builder(string abName, string assetName, Type assetType)
        {
            AssetInfo info = GetCache<AssetInfo>();
            if (info == null)
                info = new AssetInfo();
            info._Init(abName, assetName, assetType);
            return info;
        }
    }
}