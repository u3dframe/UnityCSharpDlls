using UnityEngine;
using System.Collections;
using System.IO;
using XLua;
using Core;

/// <summary>
/// 类名 : 重写里面的ReadFile
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-07-07 10:37
/// 功能 : 加载 lua 脚本
/// </summary>
public class LuaFileLoader { 
    public byte[] ReadFile(ref string fileName) {
        string fn = fileName.Replace('.', '/');
        if(fn.IndexOf("Lua/") == -1){
            fn = "Lua/" + fn;
        }
        if(fn.LastIndexOf(".lua") == -1){
            fn += ".lua";
        }
#if UNITY_EDITOR
        fileName = string.Format("{0}{1}",GameFile.m_dirData,fn);
        return GameFile.GetFileBytes(fileName);
#else
        fileName = GameFile.curInstance.GetPath(fn);
        return GameFile.curInstance.GetDecryptTextBytes(fn);
#endif
    }

    public static implicit operator LuaEnv.CustomLoader(LuaFileLoader luaLoader)
    {
        return luaLoader.ReadFile;
    }
}