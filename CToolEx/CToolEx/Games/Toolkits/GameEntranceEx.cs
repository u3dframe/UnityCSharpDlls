using UnityEngine;
using Core;
using Core.Kernel;

/// <summary>
/// 类名 : 游戏入口扩展
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-26 09:33
/// 功能 : Screen , Application , SystemInfo
/// </summary>
public static class GameEntranceEx{
    static public void Entrance( DF_OnError callError )
    {
        if (!GameAppEx.Entrance(callError))
        {
            return;
        }
        _InitMgrsPreUpload();
    }

    static void _InitMgrsPreUpload()
    {
        GameMgr.instance.Init();
    }
}