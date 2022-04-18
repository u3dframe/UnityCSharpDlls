using UnityEngine;
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
/// 修改 : 抽为父类，开发模式中继承并实现，取消宏 UNITY_ANDROID , UNITY_IOS
/// </summary>
public class EUP_BasicBridge<T> : MonoSingleton<T> where T : EUP_BasicBridge<T>
{
    protected DF_CBBridge _callBack = null;
    protected string _className = null;
    protected const string NM_ON_RESULT_FUNC = "OnCB4RetBridge";

    protected override void OnCall4Destroy()
    {
        DF_CBBridge _call = this._callBack;
        this._callBack = null;
        if (_isMustNewWhenDestroy)
        {
            curInstance.Init(_call, this._className);
        }
    }

    public void Init(DF_CBBridge onResult, string className)
    {
        this._callBack = onResult;
        this._className = className;
        this.OnInit();
    }

    protected virtual void OnInit()
    {
    }

    protected virtual void OnCB4RetBridge(string data)
    {
        if (_callBack != null)
            _callBack(data);
        else
            Debug.LogWarning("OnCB4RetBridge: _callBack is null");
    }

    public virtual void Send4Bridge(string param)
    {
    }

    public virtual E CallBridge<E>(bool Static,string className, string methodName,params object[] args)
    {
        return default(E);
    }

    static public void InitBridge(DF_CBBridge onResult, string jclassListener)
    {
        if (UGameFile.m_isEditor)
            return;
        curInstance.Init(onResult, jclassListener);
    }

    static public void SendBridge(string param)
    {
        if (UGameFile.m_isEditor)
            return;

        curInstance.Send4Bridge(param);
    }

    static public void SendBridgeAndCall(string param, DF_CBBridge onResult, string jclassListener = "")
    {
        InitBridge(onResult,jclassListener);
        SendBridge(param);
    }
}