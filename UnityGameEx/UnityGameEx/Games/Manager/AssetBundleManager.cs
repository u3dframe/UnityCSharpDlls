using UnityEngine;
using System;
using System.Collections.Generic;
namespace Core
{
    using Core.Kernel;
    /// <summary>
    /// 类名 : ab 的资源 管理器
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2020-06-26 10:29
    /// 功能 : internal
    /// </summary>
    public class AssetBundleManager : GobjLifeListener
	{
		static AssetBundleManager _instance;
		static public AssetBundleManager instance{
			get{
				if (IsNull(_instance)) {
					GameObject _gobj = GameMgr.mgrGobj;
					_instance = UtilityHelper.Get<AssetBundleManager>(_gobj,true);
				}
				return _instance;
			}
		}
		
		public int m_nMaxLoad = 5; // 限定加载
		private Dictionary<string, string[]> _dependsList = new Dictionary<string, string[]>(); // 依赖关系
		private ListDict<ABInfo> _ndLoad = new ListDict<ABInfo>(true); // 需要加载的 AB
		private ListDict<ABInfo> _loading = new ListDict<ABInfo>(true); // 正在加载的 AB
		private ListDict<ABInfo> _loaded = new ListDict<ABInfo>(true); // 已经加载了的 AB
		private ListDict<ABInfo> _unLoad = new ListDict<ABInfo>(true); // 需要销毁的 AB

		private int nUpLens = 0;
		private ABInfo upInfo = null;
		private List<ABInfo> upTemp = new List<ABInfo>();
		private List<ABInfo> upList = new List<ABInfo>();
        private bool m_isInit = false;
        public bool isDebug = UGameFile.m_isEditor; //是否打印

        public void Init()
        {
            if (this.m_isInit)
                return;

            this.m_isInit = true;
            this.isDebug = UGameFile.m_isEditor; //是否打印

            this.LoadMainfest();
        }


        /// <summary>
        ///  初始化
        /// </summary>
        override protected void OnCall4Awake(){
			this.csAlias = "ABMgr";
            this.m_isOnUpdate = true;
			GameMgr.RegisterUpdate(this);
		}

		/// <summary>
		/// 销毁
		/// </summary>
		override protected void OnCall4Destroy() {
			GameMgr.DiscardUpdate(this);
		}

		

		protected void LogErr(string fmt, params object[] msgs){
			if(!isDebug || fmt == null)
				return;
			int lens = 0;
			if(msgs != null) lens = msgs.Length;
			string msg = fmt;
			if(lens > 0){
				msg = string.Format(fmt,msgs);
			}
			Debug.LogErrorFormat("==== ABMgr = [{0}]",msg);
			// Debug.LogErrorFormat("== [{0}] == [{1}] == [{2}]",this.GetType(),this.GetInstanceID(),msg);
		}

		protected void LogErr(object msg){
			LogErr(msg.ToString());
		}
		
		public void LoadMainfest()
		{
			string path = UGameFile.m_fpABManifest;
			_dependsList.Clear();
			if(UGameFile.curInstance.IsLoadOrg4Editor()){
				return;
			}
			AssetBundle ab = AssetBundle.LoadFromFile(path); // LoadFromMemory

			if(ab == null)
			{
				LogErr("LoadMainfest ab is NULL , fp = [{0}]!",path);
				return;
			}

			AssetBundleManifest mainfest =  ab.LoadAsset<AssetBundleManifest> ("AssetBundleManifest");;
			if (mainfest == null)
			{
				LogErr("LoadMainfest ab.mainfest is NULL , fp = [{0}]!",path);
				return;
			}

			foreach(string assetName in mainfest.GetAllAssetBundles())
			{
				// GetAllDependencies = 所有依赖的AssetBundle名字
				// GetDirectDependencies = 直接依赖的AssetBundle名字
				string[] dps = mainfest.GetDirectDependencies(assetName); 
				_dependsList.Add(assetName, dps);
			}
			ab.Unload(true);
			ab = null;
		}

		public string[] GetDependences(string abName){
			if(!string.IsNullOrEmpty(abName) && this._dependsList.ContainsKey(abName)){
				return this._dependsList[abName];
			}
			return null;
		}

		private void ExcAction(Action cfLoaded){
			if(cfLoaded != null) cfLoaded();
		}

		public void LoadShadersAndWarmUp(Action cfLoaded)
		{
			if(UGameFile.curInstance.IsLoadOrg4Editor()){
				ExcAction(cfLoaded);
				return;
			}

			string abName = "all_svc.ab_svc";
			string path = UGameFile.curInstance.GetPath(abName);
			AssetBundle ab = AssetBundle.LoadFromFile(path);
			if(ab == null){
				ExcAction(cfLoaded);
				return;	
			} 
			Action _cfCallEnd = delegate(){
				ShaderVariantCollection _svc = ab.LoadAsset<ShaderVariantCollection>("all_svc.shadervariants");
				if(_svc != null && !_svc.isWarmedUp){
					_svc.WarmUp();
				}
			};
			_cfCallEnd += cfLoaded;

			string[] _des = GetDependences(abName);
			bool m_isLoadAll = (_des == null);
			if(m_isLoadAll){
				_cfCallEnd();
				return;
			}

			int lens = _des.Length;
			int nLoaded = 0;
			m_isLoadAll = nLoaded >= lens;
			if(m_isLoadAll){
				_cfCallEnd();
				return;
			}

			Action lfCall = delegate(){
				nLoaded++;
				m_isLoadAll = nLoaded >= lens;
				if(m_isLoadAll){
					_cfCallEnd();
					return;
				}
			};

			ABInfo abDep = null;
			for(int i = 0; i < lens;i++){
				abDep = LoadABDep(_des[i],null,lfCall);
				if(abDep != null){
					abDep.m_isStayForever = true;
				}
			}
		}
		
		/// <summary>
		///  更新
		/// </summary>
		override public void OnUpdate(float dt,float unscaledDt) {
			UpdateLoad(dt,unscaledDt);
			UpdateReady(dt,unscaledDt);
			UpdateUnLoad(dt,unscaledDt);
		}

		void UpdateLoad(float dt,float unscaledDt){
			upTemp.AddRange(this._loading.m_list);
			nUpLens = upTemp.Count;
			for(int i = 0; i < nUpLens;i++){
				upInfo = upTemp[i];
				upInfo.OnUpdate(dt,unscaledDt);
				if(upInfo.m_isDoned){
					upList.Add(upInfo);
				}
			}
			upTemp.Clear();

			nUpLens = upList.Count;
			for(int i = 0; i < nUpLens;i++){
				upInfo = upList[i];
				upInfo.OnUpdate(dt,unscaledDt);
				this._loading.Remove(upInfo.m_abName);

				switch(upInfo.m_abState)
				{
					case ET_AssetBundle.CompleteLoad:
						Add_Loaded( upInfo );
					break;
					case ET_AssetBundle.Err_Null_AssetBundle:
						LogErr("ab is null");
					break;
					case ET_AssetBundle.Err_Null_Abcr:
						LogErr("ab CreateRequest is null");
					break;
				}
			}
			upList.Clear();
		}

		void UpdateReady(float dt,float unscaledDt){
			upTemp.AddRange(this._ndLoad.m_list);
			int _l1 = upTemp.Count;
			if(_l1 <= 0){
				return;
			}
			
			int _l2 = this._loading.m_list.Count;
			nUpLens = this.m_nMaxLoad - _l2;
			nUpLens = (nUpLens > _l1) ? _l1 : nUpLens;
			for(int i = 0; i < nUpLens;i++){
				upInfo = upTemp[i];
				upList.Add(upInfo);
				this._loading.Add(upInfo.m_abName,upInfo);
			}
			upTemp.Clear();

			nUpLens = upList.Count;
			for(int i = 0; i < nUpLens;i++){
				upInfo = upList[i];
				this._ndLoad.Remove(upInfo.m_abName);
			}
			upList.Clear();
		}

		void UpdateUnLoad(float dt,float unscaledDt){
			upTemp.AddRange(this._unLoad.m_list);
			nUpLens = upTemp.Count;
			if(nUpLens <= 0)
				return;
			
			for(int i = 0; i < nUpLens;i++){
				upInfo = upTemp[i];
				upInfo.UpDestroy(unscaledDt);

				if(upInfo.isUnloaded){
					upList.Add(upInfo);
				}
			}
			upTemp.Clear();

			nUpLens = upList.Count;
			for(int i = 0; i < nUpLens;i++){
				upInfo = upList[i];
				this._unLoad.Remove(upInfo.m_abName);
			}
			upList.Clear();
		}

		public bool Add_NdLoad(ABInfo abInfo){
			if(null == abInfo || abInfo.isUnloaded) return false;
			abInfo.m_abUnloadState = ET_AssetBundle.None;
			_ndLoad.Add(abInfo.m_abName,abInfo);
			return true;
		}

		public bool Add_Loaded(ABInfo abInfo){
			if(null == abInfo || abInfo.isUnloaded) return false;
			if(!abInfo.m_isDoned || !abInfo.isLoaded) return false;
			abInfo.m_abUnloadState = ET_AssetBundle.None;
			_loaded.Add(abInfo.m_abName,abInfo);
			return true;
		}

		public ABInfo GetABInfo(string abName)
		{
			if(this.isAppQuit) return null;

			ABInfo _abInfo = _loaded.Get(abName);
			if(_abInfo == null) {
				_abInfo = _ndLoad.Get(abName);
			}
			if(_abInfo == null) {
				_abInfo = _loading.Get(abName);
			}
			return _abInfo;
		}

		void _ExcCall(DF_LoadedAsset cfunc,AssetBase info)
		{
			if(cfunc != null)
				cfunc(info);
		}

		private void _AddABLoadCall(DF_LoadedAsset cfunc,ABInfo info)
		{
			if(cfunc != null)
				info.m_onLoadedAB += cfunc;
		}

		public ABInfo GetInUnloadDic(string abName){
			ABInfo _ab_ = this._unLoad.Get(abName);
			if(_ab_ != null){
				if(_ab_.isUnloaded){
					_ab_ = null;
				}else{
					_ab_ = this._unLoad.Remove4Get(abName);
					_ab_.m_abUnloadState = ET_AssetBundle.None;
					if(_ab_.m_isDoned){
						if(!_ab_.isLoaded){
							_ab_ = null;
						}
					}

					if(_ab_ != null){
						_ab_.CheckDep2ReUse();
						if(_ab_.m_isDoned){
							Add_Loaded(_ab_);
						}else{
							Add_NdLoad(_ab_);
						}
					}
				}
			}
			return _ab_;
		}

		public ABInfo LoadAB(string abName,DF_LoadedAsset cfunc)
		{
			ABInfo _abInfo = GetInUnloadDic(abName);
			if(_abInfo == null) {
				_abInfo = GetABInfo(abName);
			}

			if(_abInfo != null){
				if(_abInfo.m_isDoned){
					if(_abInfo.isLoaded){
						_ExcCall(cfunc,_abInfo);
					}else{
						LogErr(_abInfo);
						_abInfo = null; // 出错的
					}
				} else {
					_AddABLoadCall(cfunc,_abInfo);
				}
			}

			if(_abInfo != null) return _abInfo;

			string[] _des = GetDependences(abName);
			if(_des == null){
				LogErr("=== ab has no dependencies, abname = [{0}]",abName);
				return null;
			}

			_abInfo = new ABInfo(abName);
			_AddABLoadCall(cfunc,_abInfo);
			Add_NdLoad(_abInfo);

			// 依赖关系
			_LoadABDeps(_abInfo);
			return _abInfo;
		}

		private void _LoadABDeps(ABInfo ab){
			string abName = ab.m_abName;
			string[] _des = GetDependences(abName);
			if(_des == null){
				return;
			}

			// 依赖关系
			int lens = _des.Length;
			ABInfo abDep = null;
			for(int i = 0; i < lens;i++){
				abDep = LoadABDep(_des[i],ab);
				ab.AddNeedDeps(abDep);
			}
		}

		private ABInfo LoadABDep(string abName4Dep,ABInfo abInfo,Action cfLoad = null){
			ABInfo abDep = LoadAB(abName4Dep,(obj) => {
				ExcAction(cfLoad);
			});
			if(abDep != null)
				abDep.RefCount();
			return abDep;
		}
		
		public AssetInfo LoadAsset(string abName,string assetName,Type assetType,DF_LoadedAsset cfunc)
		{
			if(string.IsNullOrEmpty(assetName)){
				_ExcCall(cfunc,null);
				return null;
			}

			ABInfo _abInfo = LoadAB(abName,null);
			if(_abInfo == null){
				_ExcCall(cfunc,null);
				return null;
			}
			
			AssetInfo _info = _abInfo.GetAssetAndCount(assetName,assetType);
			if(_info != null){
				if(cfunc != null){
					_info.m_onLoadedAsset += cfunc;
				}
				_info.StartUpdate();
			}
			return _info;
		}

		public AssetInfo LoadAsset<T>(string abName,string assetName,DF_LoadedAsset cfunc) where T : UnityEngine.Object
		{
			Type assetType = typeof(T);
			return LoadAsset(abName,assetName,assetType,cfunc);
		}

		public AssetInfo GetAssetInfo(string abName,string assetName,Type assetType)
		{
			if(string.IsNullOrEmpty(assetName)){
				return null;
			}

			ABInfo _abInfo = GetABInfo(abName);
			if(_abInfo == null){
				return null;
			}

			return _abInfo.GetAsset(assetName,assetType);
		}

		public AssetInfo GetAssetInfo<T>(string abName,string assetName) where T : UnityEngine.Object
		{
			Type assetType = typeof(T);
			return GetAssetInfo(abName,assetName,assetType);
		}
		
		void UnLoadAB(string abName){
			if(this.isAppQuit) return;

			var _ab = GetABInfo(abName);

			this._ndLoad.Remove(abName);
			this._loading.Remove(abName);
			this._loaded.Remove(abName);

			if(_ab != null){
				this._unLoad.Add(abName,_ab);
			}
		}

		public void UnLoadAB(ABInfo abInfo){
			if(this.isAppQuit || null == abInfo) return;
			abInfo.Unload();
			if(!abInfo.isInUnload){
				UnLoadAB(abInfo.m_abName);
			}
		}

		public void UnLoadAsset(string abName)
		{
			UnLoadAB(GetABInfo(abName));
		}

		public void UnLoadAsset(AssetInfo info)
		{
			UnLoadAB(GetABInfo(info.m_abName));
		}
	}
}
