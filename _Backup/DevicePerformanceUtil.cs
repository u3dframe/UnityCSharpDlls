using UnityEngine;
namespace Core.Kernel
{
    /// <summary>
    /// 类名 : 硬件设备性能适配工具
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2021-03-29 16:02
    /// 功能 : 
    /// 来源 ：https://www.jianshu.com/p/620e1efa8ae5
    /// </summary>
    public static class DevicePerformanceUtil
    {
        /// <summary>
        /// 获取 设备性能评级
        /// </summary>
        public static DevicePerformanceLevel GetDevicePerformanceLevel()
        {
            if (SystemInfo.graphicsDeviceVendorID == 32902)
                return DevicePerformanceLevel.Low; //集显

            //NVIDIA系列显卡（N卡）和AMD系列显卡
            //根据目前硬件配置三个平台设置了不一样的评判标准（仅个人意见）
            //CPU核心数
            int _nProp = SystemInfo.processorCount;
            int _nPropLimit = 2;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		_nPropLimit = 2;
#elif UNITY_STANDALONE_OSX || UNITY_IPHONE
		_nPropLimit = 1;
#elif UNITY_ANDROID
            _nPropLimit = 4;
#endif
            if (_nProp <= _nPropLimit)
                return DevicePerformanceLevel.Low; //CPU核心数 <= 限定数量，判定为低端

            //显存
            int graphicsMemorySize = SystemInfo.graphicsMemorySize;
            //内存
            int systemMemorySize = SystemInfo.systemMemorySize;

            int lmtGMS_H = 4000; // 高级 显存
            int lmtSMS_H = 8000; // 高级 内存
            int lmtGMS_M = 2000; // 中级 显存
            int lmtSMS_M = 4000; // 中级 内存
#if !UNITY_EDITOR && UNITY_ANDROID
            lmtGMS_H = 6000;
#endif
            if (graphicsMemorySize >= lmtGMS_H && systemMemorySize >= lmtSMS_H)
                return DevicePerformanceLevel.High;
            else if (graphicsMemorySize >= lmtGMS_M && systemMemorySize >= lmtSMS_M)
                return DevicePerformanceLevel.Mid;
            else
                return DevicePerformanceLevel.Low;
        }

        /// <summary>
        /// 根据手机性能修改项目设置
        /// </summary>
        /// QualitySettings.SetQualityLevel(int index, [DefaultValue("true")] bool applyExpensiveChanges);
        /// 但是如果你想在运行时动态调整级别，通过applyExpensiveChanges 设置为false，这样昂贵的变化并不总是被应用。
        /// 对应 QualitySettings.names; UnityEngine.QualityLevel
        /**
        public static void ModifySettingsBasedOnPerformance(int lowQuality, int midQuality, int highQuality)
        {
            DevicePerformanceLevel level = GetDevicePerformanceLevel();
            switch (level)
            {
                case DevicePerformanceLevel.Low:
                    QualitySettings.SetQualityLevel(lowQuality, true);
                    break;
                case DevicePerformanceLevel.Mid:
                    QualitySettings.SetQualityLevel(midQuality, true);
                    break;
                case DevicePerformanceLevel.High:
                    QualitySettings.SetQualityLevel(highQuality, true);
                    break;
            }
        }
        */

        /// <summary>
        /// 根据机型配置自动设置质量
        /// </summary>
        public static void ModifySettingsBasedOnPerformance()
        {
            DevicePerformanceLevel level = GetDevicePerformanceLevel();
            switch (level)
            {
                case DevicePerformanceLevel.Low:
                    SetQualitySettings(DeviceQualityLevel.Low);
                    break;
                case DevicePerformanceLevel.Mid:
                    SetQualitySettings(DeviceQualityLevel.Mid);
                    break;
                case DevicePerformanceLevel.High:
                    SetQualitySettings(DeviceQualityLevel.High);
                    break;
            }
        }

        /// <summary>
        /// 根据自身需要调整各级别需要修改的设置，可根据需求修改低中高三种方案某一项具体设置
        /// </summary>
        /// <param name="qualityLevel">质量等级</param>
        public static void SetQualitySettings(DeviceQualityLevel qualityLevel)
        {
            switch (qualityLevel)
            {
                case DeviceQualityLevel.Low:
                    //前向渲染使用的像素灯的最大数量，建议最少为1
                    QualitySettings.pixelLightCount = 2;
                    //你可以设置使用最大分辨率的纹理或者部分纹理（低分辨率纹理的处理开销低）。选项有 0_完整分辨率，1_1/2分辨率，2_1/4分辨率，3_1/8分辨率
                    QualitySettings.masterTextureLimit = 1;
                    //设置抗锯齿级别。选项有​​ 0_不开启抗锯齿，2_2倍，4_4倍和8_8倍采样。
                    QualitySettings.antiAliasing = 0;
                    //是否使用粒子软融合
                    QualitySettings.softParticles = false;
                    //启用实时反射探针，此设置需要用的时候再打开
                    QualitySettings.realtimeReflectionProbes = false;
                    //如果启用，公告牌将面向摄像机位置而不是摄像机方向。似乎与地形系统有关，此处没啥必要打开
                    QualitySettings.billboardsFaceCameraPosition = false;
                    //设置软硬阴影是否打开
                    QualitySettings.shadows = ShadowQuality.Disable;
                    //设置垂直同步方案，VSyncs数值需要在每帧之间传递，使用0为不等待垂直同步。值必须是0，1或2。
                    QualitySettings.vSyncCount = 0;
                    break;
                case DeviceQualityLevel.Mid:
                    QualitySettings.pixelLightCount = 4;
                    QualitySettings.antiAliasing = 2;
                    QualitySettings.softParticles = false;
                    QualitySettings.realtimeReflectionProbes = true;
                    QualitySettings.billboardsFaceCameraPosition = true;
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.vSyncCount = 2;
                    break;
                case DeviceQualityLevel.High:
                    QualitySettings.pixelLightCount = 4;
                    QualitySettings.antiAliasing = 8;
                    QualitySettings.softParticles = true;
                    QualitySettings.realtimeReflectionProbes = true;
                    QualitySettings.billboardsFaceCameraPosition = true;
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.vSyncCount = 2;
                    break;
            }
        }
    }

    public enum DevicePerformanceLevel
    {
        Low,
        Mid,
        High
    }

    public enum DeviceQualityLevel
    {
        Low,
        Mid,
        High
    }
}