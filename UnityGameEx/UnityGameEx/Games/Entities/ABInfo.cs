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
        static public AssetBundleManager abMgr { get { return AssetBundleManager.instance; } }

        public string m_abName; // assetbundle名

        public ABInfo GetAssetBundleInfo()
        {
            return abMgr.GetABInfo(this.m_abName);
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
        public ET_AssetBundle m_abUnloadState = ET_AssetBundle.None; // 状态机制
        public bool m_isDoned { get; private set; } // 是否已经执行了 LoadAB
        public int m_depNeedCount { get; private set; }// 需要的依赖关系的数量;
        public int m_depNeedLoaded { get; private set; }// 依赖关系加载了的数量;

        private AssetBundleCreateRequest m_abcr; // 异步加载 AssetBundle
        public AssetBundle m_ab { get; private set; } // 资源包
        protected string[] m_allAssetNames;
        public DF_LoadedAsset m_onLoadedAB = null;
        private List<ABInfo> m_depNeeds = new List<ABInfo>(); // 需要的依赖关系对象
        private ListDict<AssetInfo> m_assets = new ListDict<AssetInfo>(true); // 内部资源对象

        private int m_useCount = 0;// 引用计数,用于自动卸载;
        public bool m_isImmUnload = false; // 当引用计数为0时候，是否立即释放
        private float m_timeout = 0; // 当引用计数为0时候，不立即释放时控制
        private bool m_isShader = false;
        private bool m_isSVC = false;
        private bool m_isLoadAll = false;
        public bool m_isStayForever = false; // 是否常驻，无需释放(shader资源)

        public bool isUnloaded { get { return this.m_abUnloadState == ET_AssetBundle.Destroyed; } }
        public bool isLoaded { get { return this.m_abState == ET_AssetBundle.CompleteLoad; } }
        protected internal float m_defOutSec = 15f; // 3 分钟 60 * 3 = 180

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
        public bool m_isCanUnload
        {
            get
            {
                if (this.m_abUnloadState != ET_AssetBundle.None)
                    return (this.m_timeout - this.m_nearTimeOut) <= 0;
                return false;
            }
        }

        public ABInfo(string abName)
        {
            this.m_abName = abName;
            m_isShader = UGameFile.IsShaderAB(abName);
            m_isSVC = UGameFile.IsSVCAB(abName);
            this.m_isLoadAll = m_isShader || m_isSVC;
            this.m_fpath = UGameFile.curInstance.GetPath(abName);
            SetUpState(ET_AssetBundle.WaitLoadDeps);
        }

        override public string ToString()
        {
            Dictionary<string, object> _dic = new Dictionary<string, object>
            {
                {"abName",this.m_abName },
                {"fpath",this.m_fpath },
                {"isDone",this.m_isDoned },
                {"abState",this.m_abState },
                {"abUnloadState",this.m_abUnloadState },
                {"useCount",this.m_useCount },
                {"needDepCount",this.m_depNeedCount },
                {"needDepCount2",this.m_depNeeds.Count },
                {"needDepLoadCount",this.m_depNeedLoaded },
                {"assetCount",this.m_assets.m_list.Count },
                {"isImmUnload",this.m_isImmUnload },
                {"timeout",this.m_timeout },
            };

            System.Text.StringBuilder _sb = new System.Text.StringBuilder();
            foreach (var item in _dic)
                _sb.AppendFormat("[{0}] = [{1}],", item.Key, item.Value);

            string _v = _sb.ToString();
            _sb.Clear();
            _sb.Length = 0;
            return _v;
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
                            SetUpState(ET_AssetBundle.CheckNeedDown);
                    }
                    break;
                case ET_AssetBundle.CheckNeedDown:
                    {
                        if (UGameFile.IsFile(this.m_fpath))
                        {
                            SetUpState(ET_AssetBundle.PreLoad);
                        }
                        else if (CfgFileList.instanceDown.IsHas(this.m_abName))
                        {
                            SetUpState(ET_AssetBundle.WaitCommand);
                            MgrDownload.shareInstance.AddPreLoad(this.m_abName, _CFDownFile);
                        }
                        else if (CfgFileList.instance.IsHas(this.m_abName))
                        {
                            if (CfgFileList.instanceDowned.IsHas(this.m_abName))
                            {
                                SetUpState(ET_AssetBundle.WaitCommand);
                                MgrDownload.shareInstance.AddPreLoad(this.m_abName, _CFDownFile);
                            }
                            else
                            {
                                SetUpState(ET_AssetBundle.PreLoad);
                            }
                        }
                        else
                        {
                            SetUpState(ET_AssetBundle.CompleteLoad);
                            Debug.LogErrorFormat("=== not find ab,nm = [{0}] , fp = [{1}]", this.m_abName,this.m_fpath);
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

                        if (this.m_abcr != null)
                        {
                            SetUpState(ET_AssetBundle.Loading);
                            return;
                        }

                        this.m_abcr = AssetBundle.LoadFromFileAsync(this.m_fpath);
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
                        this._ReAllAssetNames();

                        this.Finished();

                        if (this.m_isLoadAll)
                            this.LoadAllASync();
                    }
                    break;
            }
        }

        void _CFDownFile(int state, ResInfo dlFile)
        {
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

        string _ReAssetName(string assetName)
        {
            bool _hasSuf = UGameFile.IsHasSuffix(assetName);
            if (!_hasSuf && null != this.m_allAssetNames)
            {
                string[] arrs = this.m_allAssetNames;
                int _lens_ = arrs.Length;
                string fname = null;
                for (int i = 0; i < _lens_; i++)
                {
                    fname = arrs[i];
                    if (string.IsNullOrEmpty(fname))
                        continue;
                    if (fname.StartsWith(assetName))
                        return fname;
                }
            }

            return assetName;
        }

        void _CheckCount4DepLoaded()
        {
            int lens = this.m_depNeeds.Count;
            this.m_depNeedLoaded = 0;
            ABInfo _dep_;
            for (int i = 0; i < lens; i++)
            {
                _dep_ = this.m_depNeeds[i];
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
                Debug.LogErrorFormat("=== GetAsset error = [{0}] \n[{1}]", ex,this);
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
            if(info != null)
                RefCount();
            return info;
        }

        public void AddNeedDeps(ABInfo info)
        {
            if (this.m_depNeeds.Contains(info))
                return;
            this.m_depNeeds.Add(info);
            this.m_depNeedCount++;
        }

        public void RefCount()
        {
            this.m_abUnloadState = ET_AssetBundle.None;
            this.m_timeout = 0;
            this.m_useCount++;
        }

        public void SetDefOut(float sec)
        {
            float _diff = sec - this.m_defOutSec;
            this.m_defOutSec = sec;
            if(_diff != 0 && this.m_timeout > 0)
            {
                this.m_timeout += _diff;
            }
        }

        bool IsStayForever()
        {
            return m_isStayForever || m_isShader || m_isSVC;
        }

        public void Unload()
        {
            this.m_useCount--;
            if (this.m_useCount <= 0)
            {

                if (IsStayForever())
                {
                    this.m_useCount = 0;
                    return;
                }

                this.JugdeDestroy(-1);
            }
        }

        public void UpDestroy(float dt)
        {
            switch (this.m_abUnloadState)
            {
                case ET_AssetBundle.Destroy:
                    {
                        this.m_timeout -= dt;
                        if (this.m_timeout <= 0)
                            this._Destroying();
                    }
                    break;
            }
        }

        public bool JugdeDestroy(float sec)
        {
            if(sec >= 0)
                this.SetDefOut(sec);

            if (this.m_abUnloadState == ET_AssetBundle.None)
            {
                this.m_abUnloadState = ET_AssetBundle.PreDestroy;
                this.m_timeout = this.m_isImmUnload ? 0 : m_defOutSec;
            }

            if (this.m_isCanUnload)
            {
                this._Destroying();
                return true;
            }
            else if (this.m_abUnloadState == ET_AssetBundle.PreDestroy)
            {
                this.m_abUnloadState = ET_AssetBundle.Destroy;
                abMgr.Add2NdUnload(this);
            }
            return false;
        }

        private void _Destroying()
        {
            this.m_abUnloadState = ET_AssetBundle.Destroying;
            abMgr.DoUnLoadAB(this.m_abName);
        }

        private void ToUnload()
        {
            this.m_abUnloadState = ET_AssetBundle.Destroyed;
            this.m_timeout = 0;
            this.m_onLoadedAB = null;
            this.m_abcr = null;
            this.OnUnloadAB(true);

            int lens = 0;
            lens = this.m_depNeeds.Count;
            ABInfo _a_ = null;
            for (int i = 0; i < lens; i++)
            {
                _a_ = this.m_depNeeds[i];
                _a_.Unload();
            }
            this.m_depNeeds.Clear();
            this.m_depNeedCount = 0;

            lens = this.m_assets.m_list.Count;
            AssetInfo _asset = null;
            for (int i = 0; i < lens; i++)
            {
                _asset = this.m_assets.m_list[i];
                _asset.OnUnloadAsset();
            }
            this.m_assets.Clear();
        }

        public void DoUnLoad()
        {
            if (this.isUnloaded)
                return;
            this.ToUnload();
        }

        private void OnUnloadAB(bool isAll)
        {
            var _ab = this.m_ab;
            this.m_ab = null;
            if (_ab != null)
                _ab.Unload(isAll);
        }

        public void CheckDep2ReUse()
        {
            List<ABInfo> _rmList = new List<ABInfo>();
            int lens = this.m_depNeeds.Count;
            ABInfo _dep_, _it;
            for (int i = 0; i < lens; i++)
            {
                _dep_ = this.m_depNeeds[i];
                _it = abMgr.GetInUnloadDic(_dep_.m_abName);
                if (_it == null)
                    _rmList.Add(_dep_);
            }

            lens = _rmList.Count;
            this.m_depNeedCount -= lens;
            for (int i = 0; i < lens; i++)
            {
                _dep_ = _rmList[i];
                this.m_depNeeds.Remove(_dep_);
                _dep_ = abMgr.LoadAB(_dep_.m_abName, null);
                this.AddNeedDeps(_dep_);
            }

            this.m_isDoned = false;
            SetUpState(ET_AssetBundle.WaitLoadDeps);
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

        private AssetInfo(string abName, string assetName, Type assetType)
        {
            this.m_abName = abName;
            this.m_assetName = assetName;
            this.m_assetType = assetType;
            this.m_isOnUpdate = false;
            this.m_upState = ET_Asset.PreLoad;
        }

        static public AssetInfo Builder(string abName, string assetName, Type assetType)
        {
            return new AssetInfo(abName, assetName, assetType);
        }

        override public string ToString()
        {
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

            System.Text.StringBuilder _sb = new System.Text.StringBuilder();
            foreach (var item in _dic)
                _sb.AppendFormat("[{0}] = [{1}],", item.Key, item.Value);

            string _v = _sb.ToString();
            _sb.Clear();
            _sb.Length = 0;
            return _v;
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

                        if (m_assetType != null)
                        {
                            this.m_abr = ab.m_ab.LoadAssetAsync(this.m_assetName, this.m_assetType);
                        }
                        else
                        {
                            this.m_abr = ab.m_ab.LoadAssetAsync(this.m_assetName);
                        }
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
                        this.m_upState = ET_Asset.CompleteLoad;
                        this.m_isDoned = true;
                        this.m_abr = null;
                        if (this.m_obj != null)
                        {
                            this.m_objName = this.m_obj.name;
                            this.m_objType = this.m_obj.GetType();
                            this.m_isGobj = this.m_objType == UGameFile.tpGobj;

                            ABShader.AddLoaded(this);
                        }
                    }
                    break;
            }
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
            this.m_upState = ET_Asset.None;
            this.StopAllUpdate();

            this.m_onLoadedAsset = null;
            this.m_abr = null;
            var _obj = this.m_obj;
            this.m_obj = null;
            this.m_assetType = null;

            UGameFile.UnLoadOne(_obj);
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
            if(call == null)
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
    }
}