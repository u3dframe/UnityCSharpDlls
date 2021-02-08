﻿using UnityEngine;
using Core;
using Core.Kernel;

/// <summary>
/// 类名 : 游戏入口扩展
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-26 09:33
/// 功能 : Screen , Application , SystemInfo
/// </summary>
public static class GameEntranceEx{

    static public DF_OnError cfuncError = null;

    static public void Entrance( DF_OnError callError )
    {
        if (!Application.isPlaying)
        {
            Application.logMessageReceivedThreaded -= _HandleLog;
            return;
        }

        cfuncError = callError;
        _InitAppPars();
        _InitMgrsPreUpload();
    }

    static void _InitAppPars()
    {
        GHelper.Is_App_Quit = false;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = (UGameFile.m_isEditor || UGameFile.m_isIOS) ? 60 : 30;
        Application.runInBackground = true;
        
        Application.logMessageReceivedThreaded -= _HandleLog;
        Application.logMessageReceivedThreaded += _HandleLog;
    }

    static void _HandleLog(string logString, string stackTrace, LogType type)
    {
        bool isException = type == LogType.Exception;
        if (isException || type == LogType.Error)
        {
            string _error = string.Format("=== [{0}] = [{1}]", logString, stackTrace);
            try
            {
                if (cfuncError != null)
                    cfuncError(isException,_error);
            } catch {
            }
        }
    }

    static void _InitMgrsPreUpload()
    {
        GameMgr.instance.Init();
    }
}