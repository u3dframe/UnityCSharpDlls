using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XLua;
using System.Runtime.InteropServices;
#if UNITY_5_4_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using Core;

/// <summary>
/// 类名 : Lua 管理脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-07-07 10:37
/// 功能 : 
/// </summary>
public class LuaManager : GobjLifeListener
{
	static LuaManager _instance;
	static public LuaManager instance{
		get{
			if (IsNull(_instance)) {
				GameObject _gobj = GameMgr.mgrGobj;
				_instance = UtilityHelper.Get<LuaManager>(_gobj,true);
			}
			return _instance;
		}
	}

	internal static LuaEnv luaEnv = new LuaEnv(); //all lua behaviour shared one luaenv only!
	internal static float lastGCTime = 0;
	internal const float GCInterval = 5;//1 second 

	private DF_OnUpdate luaUpdate,luaFixedUpdate;
	private DF_OnSceneChange luaSceneChange;
	private Action luaLateUpdate,luaOnApplicationQuit;
	public bool m_isPaused{get; private set;}
	private DF_OnBool luaAppPaused;

	public int LuaMemroy
	{
		get{ return (luaEnv != null) ? luaEnv.Memroy : 0; }
	}

	public void Init(){
		Core.Kernel.Messenger.AddListener<string,int,TNet.ByteBuffer>("OnCF2Lua",this.OnCFNet2Lua);
	}

	/// <summary>
	///  初始化
	/// </summary>
	override protected void OnCall4Awake(){
		this.csAlias = "LuaMgr";
		luaEnv.AddLoader(new LuaFileLoader());
		InitSelfLibs();
		m_isOnUpdate = true;
	}

	override protected void OnCall4Start(){
		var _luaG = luaEnv.Global;
		_Init_Global(_luaG);

		luaEnv.DoString("require('Main');","Main");
		var luaStart = _luaG.Get<Action>("Main");
		luaUpdate = _luaG.Get<DF_OnUpdate>("Update");
		luaFixedUpdate = _luaG.Get<DF_OnUpdate>("FixedUpdate");
		luaLateUpdate = _luaG.Get<Action>("LateUpdate");
		luaOnApplicationQuit = _luaG.Get<Action>("OnApplicationQuit");
		luaSceneChange = _luaG.Get<DF_OnSceneChange>("OnLevelWasLoaded");
		luaAppPaused = _luaG.Get<DF_OnBool>("OnApplicationPause");
		if (luaStart != null)
		{
			luaStart();
		}

#if UNITY_5_4_OR_NEWER
		SceneManager.sceneLoaded += _OnSceneLoaded;
#endif
		GameFile.IsInitLuaMgr = true;
	}

#if UNITY_5_4_OR_NEWER
    void _OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnLevelLoaded(scene.buildIndex);
    }
#else
    protected void OnLevelWasLoaded(int level)
    {
        OnLevelLoaded(level);
    }
#endif

	void OnLevelLoaded(int level){
		if(luaSceneChange != null) luaSceneChange(level);
	}

	void Update() {

		if(!m_isOnUpdate) return;

		if(luaUpdate != null) luaUpdate(Time.deltaTime,Time.unscaledDeltaTime);

		if (Time.unscaledTime - lastGCTime > GCInterval)
		{
			luaEnv.Tick();
			lastGCTime = Time.unscaledTime;
		}
	}

	void LateUpdate() {
		if(luaLateUpdate != null) luaLateUpdate();
	}

	void FixedUpdate() {
		if(luaFixedUpdate != null) luaFixedUpdate(Time.fixedDeltaTime,Time.fixedUnscaledDeltaTime);
	}
	
	protected new void OnApplicationQuit(){
		if(luaOnApplicationQuit != null) luaOnApplicationQuit();
		base.OnApplicationQuit();
	}

	void OnApplicationFocus(bool hasFocus){
        this.m_isPaused = !hasFocus;
		_ExcCF_Pause();
    }

    void OnApplicationPause(bool pauseStatus){
        this.m_isPaused = pauseStatus;
		_ExcCF_Pause();
    }

	private void _ExcCF_Pause(){
		if(luaAppPaused != null) luaAppPaused(this.m_isPaused);
	}

	public LuaTable NewLuaTable(){
		LuaTable rTb = luaEnv.NewTable();
		LuaTable meta = luaEnv.NewTable();
		meta.Set("__index", luaEnv.Global);
		rTb.SetMetaTable(meta);
		meta.Dispose();
		return rTb;
	}

	void _Init_Global(LuaTable _G){
		_Init_G_Layer(_G);
	}

	void _Init_G_Layer(LuaTable _G){
		LuaTable _nt = NewLuaTable();
		string str = null;
		for (int i = 0; i < 32; i++)
		{
			str = LayerMask.LayerToName(i);
			if (!string.IsNullOrEmpty(str))
			{
				_nt.Set(str,i);
			}
		}
		_G.Set("Layer",_nt);
	}

	

	[DllImport("xlua", CallingConvention = CallingConvention.Cdecl)]
	public static extern int luaopen_lpeg(IntPtr L);

	[DllImport("xlua", CallingConvention = CallingConvention.Cdecl)]
	public static extern int luaopen_sproto_core(IntPtr L);

	[DllImport("xlua", CallingConvention = CallingConvention.Cdecl)]
	public static extern int luaopen_cjson_safe(IntPtr L);

	[DllImport("xlua", CallingConvention = CallingConvention.Cdecl)]
	public static extern int luaopen_cjson(IntPtr L);

	[DllImport("xlua", CallingConvention = CallingConvention.Cdecl)]
	public static extern int luaopen_crypt(IntPtr L);

	[DllImport("xlua", CallingConvention = CallingConvention.Cdecl)]
	public static extern int luaopen_crypt_rc4(IntPtr L);
	[MonoPInvokeCallback(typeof(XLua.LuaDLL.lua_CSFunction))]
	public static int OpenLpeg(IntPtr L)
	{
		return luaopen_lpeg(L);
	}

	[MonoPInvokeCallback(typeof(XLua.LuaDLL.lua_CSFunction))]
	public static int OpenSprotoCore(IntPtr L)
	{
		return luaopen_sproto_core(L);
	}

	[MonoPInvokeCallback(typeof(XLua.LuaDLL.lua_CSFunction))]
	public static int OpenCjson(IntPtr L)
	{
		return luaopen_cjson(L);
	}

	[MonoPInvokeCallback(typeof(XLua.LuaDLL.lua_CSFunction))]
	public static int OpenCjsonSafe(IntPtr L)
	{
		return luaopen_cjson_safe(L);
	}

	[MonoPInvokeCallback(typeof(XLua.LuaDLL.lua_CSFunction))]
	public static int OpenCrypt(IntPtr L)
	{
		return luaopen_crypt(L);
	}

	[MonoPInvokeCallback(typeof(XLua.LuaDLL.lua_CSFunction))]
	public static int OpenCryptRc4(IntPtr L)
	{
		return luaopen_crypt_rc4(L);
	}

	

	void InitSelfLibs()
	{
		luaEnv.AddBuildin("sproto.core", OpenSprotoCore);
		luaEnv.AddBuildin("lpeg", OpenLpeg);
		luaEnv.AddBuildin("cjson", OpenCjson);
		luaEnv.AddBuildin("cjson.safe", OpenCjsonSafe);
		luaEnv.AddBuildin("crypt", OpenCrypt);
		luaEnv.AddBuildin("crypt.rc4", OpenCryptRc4);
	}

	public void LuaGC(){
		luaEnv.GC();
	}

	public bool CFuncLua(string funcName, params object[] args) {
		LuaFunction func = luaEnv.Global.GetInPath<LuaFunction>(funcName);
		if (func != null) {
			int lens = 0;
			if(args != null){
				lens = args.Length;
			}
			switch(lens){
				case 0:
					func.Call();
					break;
				case 1:
					func.Action(args[0]);
					break;
				case 2:
					func.Action(args[0],args[1]);
					break;
				default:
					func.Call(args);
					break;
			}
			return true;
		}
		return false;
	}

	public T GetGlobal<T>(string name)
    {
		return luaEnv.Global.GetInPath<T>(name);
	}

	void OnCFNet2Lua(string luaFunc,int code,TNet.ByteBuffer data) {
		bool isState = this.CFuncLua(luaFunc,code,data);
		if (!isState)
			Debug.LogErrorFormat("=== OnCFNet2Lua Fails,lua func = [{0}], code = [{1}]", luaFunc, code);
	}

	override protected void OnClear(){
		luaUpdate = null;
		luaFixedUpdate = null;
		luaLateUpdate = null;
		luaOnApplicationQuit = null;
		luaSceneChange = null;
		luaAppPaused = null;
		StopAllCoroutines();
		Core.Kernel.Messenger.RemoveListener<string,int,TNet.ByteBuffer>("OnCF2Lua",this.OnCFNet2Lua);
	}

	override protected void OnCall4Destroy(){
		luaEnv.Dispose();
	}
}
