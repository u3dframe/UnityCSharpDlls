using UnityEngine;

/// <summary>
/// 类名 : 标识 - 粒子对象根节点root
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2021-10-19 10:45
/// 功能 : 用于绑定到特效的根节点上面，方便查找
/// </summary>
public class EUR_Particle : GobjLifeListener
{
    static public new EUR_Particle Get(Object uobj, bool isAdd)
    {
        return GHelper.Get<EUR_Particle>(uobj, isAdd);
    }

    static public new EUR_Particle Get(Object uobj)
    {
        return Get(uobj, true);
    }
}