using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 类名 : PlayerLoop 加入自定义事件 
/// 作者 : Canyon/龚阳辉
/// 日期 : 2021-02-23 09:17
/// 功能 : 在FixedUpdate和Update之间插入了自己的事件
/// 来源 : https://blog.csdn.net/u010019717/article/details/94318215
/// Unity调用的事件获取的事件列表
/// 0 = Initialization   “时间”更新或状态？同步
/// 1 = EarlyUpdate       更新“输入”等
/// 2 = FixedUpdate       物理操作系统，调用FixedUpdate
/// 3 = PreUpdate         物理计算的反映？或鼠标输入
/// 4 = Update            更新过程
/// 5 = PreLateUpdate     动画更新等。最后是LateUpdate
/// 6 = PostLateUpdate    布料更新，渲染等
/// </summary>
public class PreUpdateSystem
{
    [RuntimeInitializeOnLoadMethod()]
    static void Init()
    {
        var mySystem = new UnityEngine.LowLevel.PlayerLoopSystem()
        {
            type = typeof(MyLoopSystemUpdate),
            updateDelegate = () =>
            {
                Debug.Log("DO IT!!!");
            }
        };
        var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetDefaultPlayerLoop();
        var updateSystem = playerLoop.subSystemList[4]; // Update
        var subSystem = new List<UnityEngine.LowLevel.PlayerLoopSystem>(updateSystem.subSystemList);
        subSystem.Insert(0, mySystem);
        updateSystem.subSystemList = subSystem.ToArray();
        playerLoop.subSystemList[4] = updateSystem;
        UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);
    }
    public struct MyLoopSystemUpdate { }
}