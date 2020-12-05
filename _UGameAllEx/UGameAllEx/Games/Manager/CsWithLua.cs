using System;
using System.Collections.Generic;
using XLua;

public class CsWithLua {
    
    public delegate string Message_Captured(string context);

    static Dictionary<string, Message_Captured> callbacks = new Dictionary<string, Message_Captured>();
    public static void Listen(string key,Message_Captured captured){
        callbacks.Remove(key);
        callbacks.Add(key, captured);
    }
    public static bool CallLua(string method, string message, out string err){
        try
        {
            err = "ok";
            var mgr = LuaManager.instance;
            if (mgr != null) {
                LuaFunction func = mgr.GetGlobal<LuaFunction>(method);
                if (func != null)
                {
                    err = func.Func<string, string>(message);
                    return true;
                }
                else
                    err = "Not Found Function";

            }
            else
                err = "Not Start Game";
            return false;
        }
        catch(Exception e)
        {
            err = e.Message;
        }
        return false;
    }

    [LuaCallCSharp]
    public static string CallCs(string method, string message)
    {
        return callbacks[method](message);
    }
}