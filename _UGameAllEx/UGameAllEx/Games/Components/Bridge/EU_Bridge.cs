using Core.Kernel;

/// <summary>
/// 类名 : 通讯回调函数
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2016-05-22 10:15
/// 功能 : data = json 统一结构，{code = 状态(success,fails),data = [json_str 数据],msg = 提示消息}
/// [json_str 数据] = {cmd,...}
/// </summary>
public delegate void DF_CBBridge(string data);

/// <summary>
/// 类名 : U3D 与 平台 通讯
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2016-05-22 10:15
/// 功能 : 
/// </summary>
public static class EU_Bridge {

	static public void InitBase(string jclassListener, DF_CBBridge onResult) {
        if (UGameFile.m_isEditor)
            return;

        if (UGameFile.m_isIOS)
		    EUP_IOSBridge.shareInstance.Init(onResult);
        else if (UGameFile.m_isAndroid)
            EUP_JavaBridge.shareInstance.Init(jclassListener,onResult);
    }

    static public void Init(DF_CBBridge onResult) {
		InitBase("",onResult);
	}
	
	static public void Send(string param){
        if (UGameFile.m_isEditor)
            return;

        if (UGameFile.m_isIOS)
            EUP_IOSBridge.shareInstance.SendToIOS(param);
        else if (UGameFile.m_isAndroid)
            EUP_JavaBridge.shareInstance.SendToJava(param);
    }
	
	static public void SendAndCall(string param, DF_CBBridge onResult,string jclassListener){
		InitBase (jclassListener,onResult);
		Send (param);
	}
	
	static public void SendAndCall(string param, DF_CBBridge onResult){
		SendAndCall(param,onResult,"");
	}
}