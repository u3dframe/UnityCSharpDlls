using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 类名 : U3D 与 Android 通讯桥
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2016-05-22 10:15
/// 功能 : 本类配合 SDKPlugin 工作，实现java 与  u3d 之间的通讯
/// 描述 : 消息监听者继承com.sdkplugin.extend.PluginBasic,可以通过 Init 函数初始化消息监听者
/// </summary>
public class EUP_JavaBridge : MonoSingleton<EUP_JavaBridge> {
    const string NM_ON_RESULT_FUNC = "OnResult4Java"; // 回调方法名
	const string NM_JAVA_BRIDGE_CLASS = "com.sdkplugin.bridge.U3DBridge"; // java 类名
    const string NM_JAVA_METHOD_INITALL = "initAll"; // java设置方法名 -- 全参数
    const string NM_JAVA_METHOD_INITPARS = "initPars"; // java设置方法名 -- 部分参数
    const string NM_JAVA_METHOD_NOTIFY = "request"; // java消息接受方法

    protected Dictionary<string,object> dicJo = new Dictionary<string, object>();
	string _clsListener = "-1";
    DF_CBBridge _callBack;

#if UNITY_ANDROID
    AndroidJavaClass jcBridge;
    
	void InitBridge(){
		if( jcBridge != null ){
			return;
		}
		jcBridge = new AndroidJavaClass( NM_JAVA_BRIDGE_CLASS );
	}

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
	
	T Call4Listener<T>(string className,string methodName, params object[] args){
		try{
			AndroidJavaObject jo = GetListener(className);
			if(jo != null){
				return jo.Call<T>(methodName,args);
			}
		}catch{
		}
		return default(T);
	}
	
	void Call4Listener(string className,string methodName, params object[] args){
		try{
			AndroidJavaObject jo = GetListener(className);
			if(jo != null){
				jo.Call(methodName,args);
			}
		}catch{
		}
	}
	
	T CallStatic4Class<T>(string className,string methodName, params object[] args){
		try{
			AndroidJavaClass _jc = new AndroidJavaClass( className );
			if(_jc != null){
				return _jc.CallStatic<T>(methodName,args);
			}
		}catch{
		}
		return default(T);
	}
	
	void CallStatic4Class(string className,string methodName, params object[] args){
		try{
			AndroidJavaClass _jc = new AndroidJavaClass( className );
			if(_jc != null){
				_jc.CallStatic(methodName,args);
			}
		}catch{
		}
	}
#endif

    public virtual void Init(string className,DF_CBBridge onResult) {
		this._callBack = onResult;
#if UNITY_ANDROID
		InitBridge();
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
#endif
	}
	
	public virtual void Init(DF_CBBridge onResult) {
		Init("",onResult);
	}
	
	public virtual void SendToJava( string param ){
#if UNITY_ANDROID
		if(jcBridge != null){
			jcBridge.CallStatic(NM_JAVA_METHOD_NOTIFY, param);
		} else {
			Debug.LogWarning("SendToJava: jcBridge is null.");
		}
#endif
	}
	
	public T Call<T>(string className,string methodName, params object[] args){
#if UNITY_ANDROID
		return Call4Listener<T>(className,methodName,args);
#else
        return default(T);
#endif
	}
	
	public void Call(string className,string methodName, params object[] args){
#if UNITY_ANDROID
		Call4Listener(className,methodName,args);
#endif
	}
	
	public T CallStatic<T>(string className,string methodName, params object[] args){
#if UNITY_ANDROID
		return CallStatic4Class<T>(className,methodName,args);
#else
		return default(T);
#endif
	}
	
	public void CallStatic(string className,string methodName, params object[] args){
#if UNITY_ANDROID
		CallStatic4Class(className,methodName,args);
#endif
	}

    protected void OnResult4Java(string data){
		if(_callBack != null){
			_callBack(data);
		} else {
			Debug.LogWarning("OnResult4Java: _callBack is null");
		}
	}

	protected override void OnCall4Destroy ()
	{
		Clear ();
		if (_isMustNewWhenDestroy) {
            curInstance.Init (this._clsListener,this._callBack);
		}
	}

    protected virtual void Clear(){
#if UNITY_ANDROID
		if(jcBridge != null){
			jcBridge.Dispose();
			jcBridge = null;
		}
#endif
		dicJo.Clear();
	}
}