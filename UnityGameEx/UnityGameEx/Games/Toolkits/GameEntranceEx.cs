using UnityEngine;
using Core;

/// <summary>
/// 类名 : 游戏入口扩展
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-26 09:33
/// 功能 : 
/// </summary>
public static class GameEntranceEx{

    static public DF_OnError cfuncError = null;

    static public void Entrance( DF_OnError callError )
    {
        cfuncError = callError;
        _InitAppPars();
        _InitMgrsPreUpload();
    }

    static void _InitAppPars()
    {
        GHelper.Is_App_Quit = false;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
        Application.runInBackground = true;

        Application.logMessageReceivedThreaded -= _HandleLog;
        Application.logMessageReceivedThreaded += _HandleLog;
    }

    static void _HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            string _error = string.Format("=== [{0}] = [{1}]", logString, stackTrace);
            try
            {
                if (cfuncError != null)
                    cfuncError(_error);
            } catch {
            }
        }
    }

    static void _InitMgrsPreUpload()
    {
        GameMgr.instance.Init();
        AssetBundleManager.instance.Init();
    }
}