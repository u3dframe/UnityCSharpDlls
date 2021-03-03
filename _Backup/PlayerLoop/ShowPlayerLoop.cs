using UnityEngine;

/// <summary>
/// 类名 : 显示 Unity PlayerLoop (Unity的主循环) 初始化函数 
/// 作者 : Canyon/龚阳辉
/// 日期 : 2021-02-23 09:17
/// 功能 : Unity引擎重新更新过程的顺序，或停止特定的事件，或插入自己的事件
/// 来源 : https://blog.csdn.net/u010019717/article/details/94318215
/// </summary>
public class ShowPlayerLoop
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetDefaultPlayerLoop();

        foreach (var header in playerLoop.subSystemList)
        {
            Debug.LogFormat("====== {0} ======", header.type.Name);
            foreach (var subSystem in header.subSystemList)
            {
                Debug.LogFormat("====== {0}.{1}", header.type.Name, subSystem.type.Name);
            }
        }
    }
}
