using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 类名 : U3D 与 ios 通讯桥
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2018-09-21 10:15
/// 功能 : 
/// 描述 : ios那边c函数里面必须有 c_init_bridge , c_send2bridge 两个函数
/// </summary>
public class EUP_IOSBridge : MonoSingleton<EUP_IOSBridge> {
#if UNITY_IOS
    [System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern void c_init_bridge(string gobjName,string callFunc);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern void c_send2bridge(string param);
#endif

    // 回调方法名
    public const string NM_ON_RESULT_FUNC = "OnResult4IOS";
	bool _isInited = false;
    DF_CBBridge _callBack;

	protected override void OnCall4Start ()
	{
		base.OnCall4Start ();
		_isMustNewWhenDestroy = true;
	}

	protected override void OnCall4Destroy ()
	{
		_isInited = false;
		if (_isMustNewWhenDestroy) {
            curInstance.Init (this._callBack);
		}
	}

	public virtual void Init(DF_CBBridge onResult) {
		this._callBack = onResult;
		if(_isInited)
			return;
		_isInited = true;
#if UNITY_IOS
		c_init_bridge(NM_Gobj,NM_ON_RESULT_FUNC);
#endif
	}

	public virtual void SendToIOS( string param ){
#if UNITY_IOS
		c_send2bridge(param);
#endif
	}

    protected void OnResult4IOS(string data){
		if(_callBack != null){
			_callBack(data);
		} else {
			Debug.LogWarning("OnResult4IOS: _callBack is null");
		}
	}
}