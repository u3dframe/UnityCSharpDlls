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
	/// 类名 : 枚举 - 更新的流程状态
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

        PreUnZipRes = 4,
        UnZipRes = 5,
        UnGpOBB = 6, // google的obb资源
        CheckAppCover = 7, // 安装检测(流和可读写version对比)
        CheckNet = 8,

        CheckVersion = 9,
        CheckFileList = 10,
        InitMustFiles = 11, // 初始化 用于判断必须下载的文件列表
        CompareFileList = 12,
        SaveFileList = 13,
        SaveVersion = 14,
        CheckAppFull = 15, // 检测App的完整性
        
        Error = 100,
        Error_NoNet = 101,
        Error_NotEnoughMemory = 102,

        Error_DF_EmptyUrl = 104, // DF = DownFiles
        Error_DF_TimeOut = 105,
        Error_DF_LoadDown = 106,
        Error_DF_NotMatchCode = 107,
        Error_DF_ExcuteCall = 108,

        Error_LoadZipList = 160,
        Error_LoadZipOne = 161,
        Error_UnZipOne = 162,
        Error_UnZipOBB = 163,
        Error_LoadStreamVer = 164,
        Error_DownVer = 165,
        Error_DownFileList = 166,
        Error_NullCompareFiles = 167,
        Error_SaveFList = 168,
        Error_SaveVer = 169,
        Error_AppFull = 170,

        Error_NeedDownApkIpa = 999,
    }

    /// <summary>
	/// 类名 : 枚举 - 对比FileList流程状态
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2018-10-16 16:30
	/// 功能 : 
	/// </summary>
	public enum EM_CompFiles
    {
        None = 0,
        Completed = 1,
        WaitCommand = 2,
        Init = 3,

        CheckDelFiles = 30,
        DelFiles = 31,
        CheckDownFiles = 32,
        DownFiles = 33,

        Error = 100,
        Error_NoNet = 101,
        Error_NotEnoughMemory = 102,

        Error_DF_EmptyUrl = 104, // DF = DownFiles
        Error_DF_TimeOut = 105,
        Error_DF_LoadDown = 106,
        Error_DF_NotMatchCode = 107,
        Error_DF_ExcuteCall = 108,
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

    /// <summary>
	/// 类名 : 枚举 - 文件加密方式
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2020-02-26 14:27
	/// 功能 : 
	/// </summary>
	public enum EM_EnCode
    {
        None = 0,
        XXTEA = 1,
        BASE64 = 2,
    }
}
