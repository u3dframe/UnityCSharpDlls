using UnityEngine;

namespace Core.Kernel.Beans
{

    /// <summary>
    /// 类名 : LightProbes 灯光探头数据收集
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2021-05-20 17:16
    /// 功能 : Unity2019后，以前的 LightProbes 保存Asset模式不能正常运行了
    ///        测试，没还原成功，因为自建的空场景的 LightmapSettings.lightProbes 为空
    /// </summary>
    public class LProbeData: ScriptableObject
    {
        public UnityEngine.Rendering.SphericalHarmonicsL2[] lightProbes;
    }
}
