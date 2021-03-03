using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 类名 : PlayerLoop 禁用特定事件 - 
/// 作者 : Canyon/龚阳辉
/// 日期 : 2021-02-23 09:17
/// 功能 : 删除特定事件 PlayerLoop 的 物理更新 FixedUpdate.PhysicsFixedUpdate
/// 来源 : https://blog.csdn.net/u010019717/article/details/94318215
/// </summary>
public class StopPhysicsFixedUpdate
{
    static UnityEngine.LowLevel.PlayerLoopSystem[] originalPlayerLoop, updatedPlayerLoop;
    [RuntimeInitializeOnLoadMethod()]
    static void Init()
    {
        var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetDefaultPlayerLoop();
        originalPlayerLoop = playerLoop.subSystemList[2].subSystemList; // Update
        var subSystem = new List<UnityEngine.LowLevel.PlayerLoopSystem>(originalPlayerLoop);
        subSystem.RemoveAll(c => c.type == typeof(UnityEngine.PlayerLoop.FixedUpdate.PhysicsFixedUpdate));
        updatedPlayerLoop = subSystem.ToArray();
        PhysicsOFF();
    }

    public static void PhysicsOFF()
    {
        var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetDefaultPlayerLoop();
        var sub = playerLoop.subSystemList[2];
        sub.subSystemList = updatedPlayerLoop;
        playerLoop.subSystemList[2] = sub;
        UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);
    }

    public static void PhysicsON()
    {
        var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetDefaultPlayerLoop();
        var sub = playerLoop.subSystemList[2];
        sub.subSystemList = originalPlayerLoop;
        playerLoop.subSystemList[2] = sub;
        UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);
    }
}