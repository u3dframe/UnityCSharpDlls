using System;
namespace Core
{
	/// <summary>
	/// 类名 : 枚举 - AB加载状态
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2020-06-26 08:29
	/// 功能 : 
	/// </summary>
	public enum ET_AssetBundle{
		None = 0,
        WaitCommand = 1,
        WaitLoadDeps = 2,
        CheckNeedDown = 3,

        PreLoad = 10,
		Loading = 11,
		CompleteLoad = 12,
		
		PreDestroy = 20,
		Destroying = 21,
		Destroyed = 22,
		
		Error = 100,
		Err_Null_Abcr = 101,
		Err_Null_AssetBundle = 102,
	}
	
	/// <summary>
	/// 类名 : 枚举 - Asset加载状态
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2020-06-26 09:09
	/// 功能 : 
	/// </summary>
	public enum ET_Asset{
		None = 0,
		PreLoad = 1,
		Loading = 2,
		CompleteLoad = 3,
		
		Error = 100,
		Err_Null_AbInfo = 101,
		Err_Null_AssetBundle = 102,
		Err_Null_Abr = 103,
	}

    /// <summary>
	/// 类名 : 枚举 - 资源对象类型
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2018-01-30 14:35
	/// 功能 : 
	/// </summary>
	public enum EM_Asset
    {
        None = 0,
        Text = 1,
        Bytes = 2,
        Texture = 3,
        AssetBundle = 4
    }

    /// <summary>
	/// 类名 : 枚举 - 下载状态
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2018-01-30 14:35
	/// 功能 : 
	/// </summary>
	public enum EM_DownLoad
    {
        None = 0,
        Completed = 1,
        WaitCommand = 2,
        Init = 3,

        DownLoading = 21,

        Error = 100,
        Error_NoNet = 101,
        Error_NotEnoughMemory = 102,

        Error_EmptyUrl = 104,
        Error_TimeOut = 105,
        Error_LoadDown = 106,
        Error_NotMatchCode = 107,
        Error_ExcuteCall = 108,
    }

    /// <summary>
	/// 类名 : 枚举 - 流程状态(更新的，对比的)
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2018-10-16 16:30
	/// 功能 : 
	/// </summary>
	public enum EM_Process
    {
        None = 0,
        Completed = 1,
        WaitCommand = 2,
        Init = 3,

        CheckAppCover = 4, // 安装检测(流和可读写version对比)
        PreUnZipRes = 5,
        UnZipRes = 6,
        CheckNet = 7,
        CheckAppFull = 8, // 检测App的完整性
        UnGpOBB = 9, // google的obb资源

        InitCompare = 30,
        DownVersion = 31,
        CheckVersion = 32,
        InitCfgMustFiles = 33, // 初始化 用于判断必须下载的文件列表
        DownFileList = 34,
        CompareFileList = 35,
        DeleteFiles = 36,
        PreDownFiles = 37,
        DownFiles = 38,
        SaveFileList = 39,
        SaveVersion = 40,

        Error = 100,
        Error_NoNet = 101,
        Error_NotEnoughMemory = 102,
        Error_DownFiles = 130,

        Error_UnZip_Init = 150,
        Error_UnZip_OBB = 151,

        Error_DownVer = 201,
        Error_DownFileList = 202,

        Error_NeedDownApkIpa = 999,
    }

    /// <summary>
	/// 类名 : 枚举 - 成功失败状态
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2018-10-18 14:35
	/// 功能 : 
	/// </summary>
	public enum EM_SucOrFails
    {        
        Success = 1,
        Fails = 2,
    }
}
