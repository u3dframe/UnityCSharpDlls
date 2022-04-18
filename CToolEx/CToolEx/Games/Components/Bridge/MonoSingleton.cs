using UnityEngine;

/// <summary>
/// 类名 : mono 单例对象
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2018-09-21 10:15
/// 功能 : 
/// 描述 : MonoBehaviour 生命周期可以抽出一个父对象
/// </summary>
public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static private object _lock = new object();
	static protected string NM_Gobj = "GameMgrs";
	static private T _shareT;
	static public T shareInstance{
		get{
			lock (_lock)
            {
				if (_shareT == null) {
					_shareT = (T)FindObjectOfType(typeof(T));
					
					GameObject gobj = null;
					if (_shareT != null)
                    {
						gobj = _shareT.gameObject;
						gobj.name = NM_Gobj;
                    } else {
						gobj = GameObject.Find (NM_Gobj);
						if (!gobj) {
							gobj = new GameObject (NM_Gobj, typeof(T));
						}
					}
					_shareT = gobj.GetComponent<T> ();
					if (_shareT == null) {
						_shareT = gobj.AddComponent<T> ();
					}
					GameObject.DontDestroyOnLoad (gobj);
				}
				return _shareT;
			}
		}
	}

    static private T _curInstance = null;
    static public T curInstance
    {
        get
        {
            if (_curInstance == null)
                _curInstance = shareInstance;
            return _curInstance;
        }
        set
        {
            if (value != null)
                _curInstance = value;
        }
    }

    static protected bool _initInstace = false;
	static public T InitInstance(string gobjName)
    {
		bool _isEmt = string.IsNullOrEmpty(gobjName);
		if(!_isEmt)
			NM_Gobj = gobjName;
		
		T _ret = curInstance;
		if(!_isEmt){
			GameObject gobj = _ret.gameObject;
			if(!gobj.name.Equals(gobjName))
				gobj.name = gobjName;
		}
		
		_initInstace = true;
        return _ret;
    }
	
	protected bool _isMustNewWhenDestroy = false; // 销毁了后是否会重新再创建
	protected bool _isAppQuit = false;

	void Awake() {
		if(_shareT == null)
			_shareT = GetComponent<T> ();
		OnCall4Awake ();
	}

	void Start() {
		OnCall4Start ();
	}

	void OnApplicationQuit(){
		this._isAppQuit = true;
	}
	
	void OnDestroy(){
		if (this._isAppQuit) {
			return;
		}
		_shareT = null;
		OnCall4Destroy ();
	}

	protected virtual void OnCall4Awake(){ }
	protected virtual void OnCall4Start(){ }
	protected virtual void OnCall4Destroy(){ }

	public bool isDebug = false; //是否打印
	protected void Log(object msg){
			if(!isDebug || msg == null)
				return;
			Debug.LogFormat("==== single = [{0}]",msg);
			// Debug.LogFormat("== [{0}] == [{1}] == [{2}]",this.GetType(),this.GetInstanceID(),msg);
		}
		
	protected void LogFmt(string fmt,params object[] pars){
		if(!isDebug || string.IsNullOrEmpty(fmt))
			return;
		string msg = string.Format(fmt,pars);
		Log(msg);
	}
}
