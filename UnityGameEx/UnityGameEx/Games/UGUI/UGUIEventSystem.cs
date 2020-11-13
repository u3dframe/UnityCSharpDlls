using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
/// <summary>
/// 类名 : UGUI EventSystem 单例对象
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-12-14 16:10
/// 功能 : EventSystem 只能存在一个
/// </summary>
[RequireComponent(typeof(EventSystem),typeof(StandaloneInputModule))]
public class UGUIEventSystem : MonoBehaviour {

	bool _isCheck = false;
	bool _isAppQuit = false;
	EventSystem _curr;

	void OnEnable(){
		_curr = gameObject.GetComponent<EventSystem> ();
	}

	// Update is called once per frame
	void Update () {
		if (!_isCheck)
			return;
		
		if (EventSystem.current != _curr) {
			EventSystem.current = _curr;
		}
	}

	void OnApplicationQuit(){
		this._isCheck = false;
		this._isAppQuit = true;
	}

	void OnDestroy(){
		if (this._isAppQuit)
			return;
		
		_instance = null;
		instance.Init (true);
	}

	public void Init(bool isCheck){
		this._isCheck = isCheck;
		this._isAppQuit = false;
	}

	static UGUIEventSystem _instance;
	static public UGUIEventSystem instance{
		get{
			if (_instance == null) {
				string NM_Gobj = "EventSystem";
				GameObject _gobj = GameObject.Find(NM_Gobj);
				if (!_gobj)
				{
					_gobj = new GameObject(NM_Gobj, typeof(UGUIEventSystem));
				}
				_instance = _gobj.GetComponent<UGUIEventSystem>();
				if (_instance == null)
				{
					_instance = _gobj.AddComponent<UGUIEventSystem> ();
				}
				GameObject.DontDestroyOnLoad (_gobj);
			}

			return _instance;
		}
	}
}
