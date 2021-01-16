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
    }

    /// <summary>
    /// 类名 : ab 的资源
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2019-06-26 10:29
    /// 功能 : 
    /// </summary>
    public class ABInfo : AssetBase
    {
        public string m_abName; // assetbundle名
        public string m_fpath; // 资源地址
        public ET_AssetBundle m_abState = ET_AssetBundle.None; // 状态机制
        public ET_AssetBundle m_abUnloadState = ET_AssetBundle.None; // 状态机制
        public bool m_isDoned = false; // 是否已经执行了 LoadAB
        public int m_depNeedCount { get; private set; }// 需要的依赖关系的数量;
        public int m_depNeedLoaded { get; private set; }// 依赖关系加载了的数量;

        private AssetBundleCreateRequest m_abcr; // 异步加载 AssetBundle
        public AssetBundle m_ab; // 资源包
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

        public bool isInUnload { get { return this.m_abUnloadState != ET_AssetBundle.None; } }
        public bool isUnloaded { get { return this.m_abUnloadState == ET_AssetBundle.Destroyed; } }
        public bool isLoaded { get { return this.m_abState == ET_AssetBundle.CompleteLoad; } }
        protected internal float m_defOutSec = 10f; // 3 分钟 60 * 3 = 180

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
            string fmt = "abName = [{0}] , isDone = [{1}] , state = [{2}] , un = [{3}] , useCount = [{4}] , assetCount = [{5}] , ndDepCount = [{6}],depLoadCount = [{7}] , immUnlod = [{8}] , timeout = [{9}]";
            return string.Format(
                fmt,
                this.m_abName,
                this.m_isDoned,
                this.m_abState,
                this.m_abUnloadState,
                this.m_useCount,
                this.m_assets.m_list.Count,
                this.m_depNeedCount,
                this.m_depNeedLoaded,
                this.m_isImmUnload,
                this.m_timeout
            );
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
                            this.m_abcr = null;
                            SetUpState(ET_AssetBundle.Err_Null_AssetBundle); // 资源 assetBundle 为空了
                            return;
                        }

                        this.m_ab = this.m_abcr.assetBundle;
                        this._ReAllAssetNames();

                        this.Finished();

                        if (this.m_isLoadAll)
                        {
                            this.LoadAllASync();
                        }
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
            if (!_hasSuf)
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

        private void _CheckCount4DepLoaded()
        {
            int lens = this.m_depNeeds.Count;
            this.m_depNeedLoaded = 0;
            ABInfo _dep_;
            for (int i = 0; i < lens; i++)
            {
                _dep_ = this.m_depNeeds[i];
                if (_dep_.m_isDoned)
                {
                    this.m_depNeedLoaded++;
                }
            }
        }

        private void Finished()
        {
            this.m_isDoned = true;
            this.m_abcr = null;
            SetUpState(ET_AssetBundle.CompleteLoad);
        }

        private void SetUpState(ET_AssetBundle state)
        {
            this.m_abState = state;
        }

        void _ExcuteLoadedCall()
        {
            var _func = this.m_onLoadedAB;
            this.m_onLoadedAB = null;
            if (_func != null)
            {
                _func(this);
            }
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
                Debug.LogErrorFormat("=== abName = [{0}] = assetName = [{1}],assetType = [{2}] \n error = [{3}]", this.m_abName,assetName, assetType, ex);
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
                        {
                            this.m_assets.Add(_key, info);
                        }
                        else
                        {
                            info = null;
                        }
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
                Debug.LogErrorFormat("=== abName = [{0}] = assetName = [{1}],assetType = [{2}] \n error = [{3}]", this.m_abName, assetName, assetType, ex);
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
            this.m_useCount++;
            this.m_timeout = 0;
        }

        public void SetDefOut(float sec)
        {
            this.m_defOutSec = sec;
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

                this.m_timeout = m_defOutSec;
                if (this.m_isImmUnload || this.m_timeout < 0)
                    OnUnload();
                else
                    this.m_abUnloadState = ET_AssetBundle.PreDestroy;
            }
        }

        public void UpDestroy(float dt)
        {
            switch (this.m_abUnloadState)
            {
                case ET_AssetBundle.PreDestroy:
                    {
                        if (this.m_timeout > 0)                        
                            this.m_timeout -= dt;
                        if (this.m_timeout <= 0)
                            this.m_abUnloadState = ET_AssetBundle.Destroying;
                    }
                    break;
                case ET_AssetBundle.Destroying:
                    {
                        OnUnload();
                    }
                    break;
            }
        }

        private void OnUnloadAB(bool isAll)
        {
            var _ab = this.m_ab;
            this.m_ab = null;
            if (_ab != null)
            {
                _ab.Unload(isAll);
            }
        }

        private void OnUnload()
        {
            this.m_onLoadedAB = null;
            this.m_abcr = null;
            int lens = 0;
            lens = this.m_depNeeds.Count;
            for (int i = 0; i < lens; i++)
            {
                this.m_depNeeds[i].Unload();
            }
            this.m_depNeeds.Clear();
            this.m_depNeedCount = 0;

            lens = this.m_assets.m_list.Count;
            for (int i = 0; i < lens; i++)
            {
                this.m_assets.m_list[i].OnUnloadAsset();
            }
            this.m_assets.Clear();

            OnUnloadAB(true);

            this.m_abUnloadState = ET_AssetBundle.Destroyed;
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
                {
                    _rmList.Add(_dep_);
                    continue;
                }
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
        public string m_abName;
        public string m_assetName;
        public Type m_assetType;
        public Type m_objType;

        private AssetBundleRequest m_abr; // 异步加载 Asset 资源
        public UObject m_obj;
        public string m_objName = ""; // 加载出来的资源的名字
        public long m_countNewGobj = 0;
        public bool m_isGobj = false;
        public ET_Asset m_upState = ET_Asset.None;
        public bool m_isDoned = false;

        public DF_LoadedAsset m_onLoadedAsset = null;
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
            string fmt = "m_abName = [{0}],m_assetName = [{1}],m_assetType = [{2}],m_objName = [{3}],m_isDoned = [{4}],m_upState = [{5}],m_objType = [{6}],m_countNewGobj = [{7}]";
            return string.Format(
                fmt,
                this.m_abName,
                this.m_assetName,
                this.m_assetType,
                this.m_objName,
                this.m_isDoned,
                this.m_upState,
                this.m_objType,
                this.m_countNewGobj
            );
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
            this.StopUpdate();

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
            this.StopUpdate();

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

        public ABInfo GetAssetBundleInfo()
        {
            return abMgr.GetABInfo(this.m_abName);
        }

        public void UnLoadAsset()
        {
            abMgr.UnLoadAsset(this.m_abName);
        }
    }
}