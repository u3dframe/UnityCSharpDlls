using UnityEngine;
using System;

/// <summary>
/// 类名 : Lua 帮助脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-07-07 10:37
/// 功能 : 
/// </summary>
public sealed class LuaHelper : UtilityHelper {

	static public int LuaMemroy() {
		var mgr = LuaManager.instance;
		if (mgr != null) return mgr.LuaMemroy;
		return 0;
	}

	/// <summary>
	/// 清理内存
	/// </summary>
	static public void ClearMemory() {
		GC.Collect(); Resources.UnloadUnusedAssets();
		var mgr = LuaManager.instance;
		if (mgr != null) mgr.LuaGC();
	}

	// [Obsolete]
	static public bool CFuncLuaMore(string funcName, params object[] args) {
		var mgr = LuaManager.instance;
		if (mgr != null) { return mgr.CFuncLua(funcName,args); } 
		return false;
	}

	static public bool CFuncLua(string funcName) {
		return CFuncLuaMore(funcName);
	}

	static public bool CFuncLua(string funcName,object obj1) {
		return CFuncLuaMore(funcName,obj1);
	}

	static public bool CFuncLua(string funcName,object obj1,object obj2) {
		return CFuncLuaMore(funcName,obj1,obj2);
	}

	static public bool CFuncLua(string funcName,object obj1,object obj2,object obj3) {
		return CFuncLuaMore(funcName,obj1,obj2,obj3);
	}

	static public bool CFuncLua(string funcName,object obj1,object obj2,object obj3,object obj4) {
		return CFuncLuaMore(funcName,obj1,obj2,obj3,obj4);
	}

	static public bool CFuncLua(string funcName,object obj1,object obj2,object obj3,object obj4,object obj5) {
		return CFuncLuaMore(funcName,obj1,obj2,obj3,obj4,obj5);
	}

	static public bool CFuncLua(string funcName,object obj1,object obj2,object obj3,object obj4,object obj5,object obj6) {
		return CFuncLuaMore(funcName,obj1,obj2,obj3,obj4,obj5,obj6);
	}
}