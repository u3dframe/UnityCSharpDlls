using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 类名 : Input 管理脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-07-13 21:29
/// 功能 : 
/// </summary>
public class InputMgr  : InputBaseMgr{
	static InputMgr _instance;
	static public InputMgr instance{
		get{
			if (IsNull(_instance)) {
				GameObject _gobj = GameMgr.mgrGobj;
				_instance = UtilityHelper.Get<InputMgr>(_gobj,true);
			}
			return _instance;
		}
	}

#if UNITY_EDITOR
	static private System.Type _tpKeyCode = typeof(KeyCode);
	static private Dictionary<KeyCode, DF_InpKeyState> m_diCalls;
	static private void OnUpdate()
	{
		if (m_diCalls == null || m_diCalls.Count <= 0)
			return;
		var e = m_diCalls.GetEnumerator();
		while (e.MoveNext())
		{
			var current = e.Current;
			if (Input.GetKeyDown(current.Key))
			{
				// 按键按下的第一帧返回true
				current.Value(current.Key.ToString(), 1);
			}
			else if (Input.GetKeyUp(current.Key))
			{
				// 按键松开的第一帧返回true
				current.Value(current.Key.ToString(), 2);
			}
			else if (Input.GetKey(current.Key))
			{
				// 按键按下期间返回true
				current.Value(current.Key.ToString(), 3);
			}
		}
	}

	static private void RegKeyCode(string key, DF_InpKeyState callBack,bool isAppend)
	{
		if (m_diCalls == null) m_diCalls = new Dictionary<KeyCode, DF_InpKeyState>();

		KeyCode _code = EnumEx.Str2Enum<KeyCode>(_tpKeyCode,key);
		DF_InpKeyState _val;
		if (m_diCalls.ContainsKey(_code)) {
			if(isAppend){
				_val = m_diCalls[_code] + callBack;
				m_diCalls[_code] = _val;
			}
		}else{
			m_diCalls.Add(_code, callBack);
		}
	}
#endif

	static public void RegKeyCode(string key, DF_InpKeyState callBack){
#if UNITY_EDITOR
		RegKeyCode(key,callBack,false);
#endif
	}

	override protected void OnClear() {
		base.OnClear();
#if UNITY_EDITOR
		if(m_diCalls != null) m_diCalls.Clear();
#endif
	}

    override protected void Update () {
		base.Update();
#if UNITY_EDITOR
		OnUpdate();
#endif
    }
}