using UnityEngine;
using Core;
using Core.Kernel;

/// <summary>
/// 类名 : 游戏 扩展
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-26 09:33
/// 功能 : Screen , Application , SystemInfo
/// 修改 : 2021-07-14 18:40
/// </summary>
public static class GameAppEx{
    static public int fpsFrameRate { get; set; }
    static public DF_OnError cfuncError = null;

    static public bool Entrance( DF_OnError callError )
    {
        if (!Application.isPlaying)
        {
            Application.logMessageReceivedThreaded -= _HandleLog;
            return false;
        }

        cfuncError = callError;
        _InitAppPars();
        _InitScreen();
        return true;
    }

    static void _InitAppPars()
    {
        GHelper.Is_App_Quit = false;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        fpsFrameRate = (UGameFile.m_isEditor || UGameFile.m_isIOS) ? 60 : 45;
        Application.targetFrameRate = fpsFrameRate;
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

    static public int sd_width { get; private set; }
    static public int sd_height { get; private set; }
    static void _InitScreen()
    {
        if (sd_width == 0)
        {
            sd_width = Screen.currentResolution.width;
            sd_height = Screen.currentResolution.height;
        }
    }

    static public void ReResolution(float rate = 0.5f)
    {
        _InitScreen();
        rate = Mathf.Max(rate, 0.3f);
        rate = Mathf.Min(rate, 1.0f);
        int width = (int)(sd_width * rate);
        int height = (int)(sd_height * rate);
        Screen.SetResolution(width, height, true);
    }

    static public void SetFrameRate(int frameRate,bool isChgDef = false)
    {
        frameRate = frameRate < 30 ? 30 : frameRate;
        if (isChgDef)
            fpsFrameRate = frameRate;
        Application.targetFrameRate = frameRate;
    }

    static public void SetFrameRateByRate(float rate)
    {
        int _fps = fpsFrameRate;
        if(rate != 1)
            _fps = Mathf.CeilToInt(fpsFrameRate * rate);
        _fps = _fps < 5 ? 5 : _fps;
        Application.targetFrameRate = _fps;
    }
}