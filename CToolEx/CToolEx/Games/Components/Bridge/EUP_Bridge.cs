using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;

/// <summary>
/// 类名 : U3D 与 平台的通信
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2016-05-22 10:15
/// 功能 : Android,IOS等
/// 修改 : 重新整合 2022-04-17 15:35
/// </summary>
public class EUP_Bridge : EUP_BasicBridge<EUP_Bridge>
{
	static public void InitBase(DF_CBBridge onResult){
		InitBridge(onResult,"");
    }

	static public void Send(string param){
		SendBridge(param);
    }
	
	static public void SendAndCall(string param, DF_CBBridge onResult,string jclassListener = "")
	{
		SendBridgeAndCall(param,onResult,jclassListener);
	}

#if UNITY_EDITOR
#elif UNITY_ANDROID
    const string NM_JAVA_BRIDGE_CLASS = "com.sdkplugin.bridge.U3DBridge";
    const string NM_JAVA_METHOD_INITALL = "initAll";
    const string NM_JAVA_METHOD_INITPARS = "initPars";
    const string NM_JAVA_METHOD_NOTIFY = "request";
    protected Dictionary<string,object> dicJo = new Dictionary<string, object>();
	AndroidJavaClass _jcBridge = null;
	string _clsListener = "";
    AndroidJavaClass jcBridge{ get { if( _jcBridge == null ) _jcBridge = new AndroidJavaClass( NM_JAVA_BRIDGE_CLASS ); return _jcBridge; } }

    AndroidJavaObject GetListener(string className){
		this._clsListener = className;
		if(dicJo.ContainsKey(className)){
			return (AndroidJavaObject)dicJo[className];
		}
		AndroidJavaObject jo = null;
		try{
			AndroidJavaClass _jc = new AndroidJavaClass( className );
			jo = _jc.CallStatic<AndroidJavaObject>("getInstance");
		}catch{
			jo = null;
		}
		try{
			if(jo == null){
				jo = new AndroidJavaObject(className);
			}
			dicJo.Add(className,jo);
		}catch{
		}
		return jo;
	}
    public override E CallBridge<E>(bool isStatic, string className, string methodName, params object[] args)
    {
		try{
			if(isStatic){
				AndroidJavaClass _jc = new AndroidJavaClass( className );
				return _jc.CallStatic<E>(methodName,args);
			}

			AndroidJavaObject jo = GetListener(className);
			if(jo != null)
				return jo.Call<E>(methodName,args);
		}catch{
		}
        return base.CallBridge<E>(isStatic, className, methodName, args);
    }

	void Init4Android() 
	{
		string className  = this._className;
		if(string.IsNullOrEmpty(className)){
			jcBridge.CallStatic(NM_JAVA_METHOD_INITPARS,NM_Gobj,NM_ON_RESULT_FUNC);
			return;
		}
		if(className.Equals(this._clsListener))
			return;
		
		AndroidJavaObject joListener = GetListener(className);

		if(joListener == null){
			jcBridge.CallStatic(NM_JAVA_METHOD_INITPARS,NM_Gobj,NM_ON_RESULT_FUNC);
			return;
		}
		jcBridge.CallStatic(NM_JAVA_METHOD_INITALL,joListener,NM_Gobj,NM_ON_RESULT_FUNC);
	}

	void Send4Android(string data)
	{
		jcBridge.CallStatic(NM_JAVA_METHOD_NOTIFY, data);
	}
#elif UNITY_IOS
	void Init4IOS()
	{
	}

	void Send4IOS(string data)
	{
	}
#endif

    protected override void OnInit()
    {
        base.OnInit();
#if UNITY_EDITOR
#elif UNITY_ANDROID
		Init4Android();
#elif UNITY_IOS
		Init4IOS();
#endif
    }

    protected override void Send4Bridge(string param)
    {
#if UNITY_EDITOR
#elif UNITY_ANDROID
		Send4Android(param);
#elif UNITY_IOS
		Send4IOS(param);
#endif
    }

}
