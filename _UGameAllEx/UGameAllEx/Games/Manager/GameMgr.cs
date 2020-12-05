using UnityEngine;
using System;
using System.Collections.Generic;
using Core;

/// <summary>
/// 类名 : Update 管理
/// 作者 : Canyon
/// 日期 : 2019-06-27 20:37
/// 功能 : 所有需要Update函数，统一从这里调用
/// </summary>
public class GameMgr : GobjLifeListener {
	static GameObject _mgrGobj;
	static public GameObject mgrGobj{
		get{
			if (IsNull(_mgrGobj)) {
				_mgrGobj = GetGobj("GameManager");
			}
			return _mgrGobj;
		}
	}

    static GameObject _mgrGobj2;
    static public GameObject mgrGobj2
    {
        get
        {
            if (IsNull(_mgrGobj2))
            {
                _mgrGobj2 = GetGobj("GManager");
            }
            return _mgrGobj2;
        }
    }

    static GameMgr _instance;
	static public GameMgr instance{
		get{
			if (IsNull(_instance)) {
				GameObject _gobj = GameMgr.mgrGobj;
				_instance = GHelper.Get<GameMgr>(_gobj,true);
			}
			return _instance;
		}
	}
	
	static private DF_OnUpdate onUpdate = null;
	static private List<IUpdate> mListUps = new List<IUpdate>(); // 无用质疑，直接调用函数，比代理事件快

	static private Action onLateUpdate = null;
	static private List<ILateUpdate> mListLateUps = new List<ILateUpdate>();

	List<IUpdate> upList = new List<IUpdate>();
	IUpdate upItem = null;
	List<ILateUpdate> upLateList = new List<ILateUpdate>();
	ILateUpdate upLateItem = null;
	[SerializeField] int upLens = 0;
	[SerializeField] int lateLens = 0;
    bool m_isInitAfterUpload = false;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
	{
        this.csAlias = "GMgr";
    }

    /// <summary>
	/// 初始化 - 在更新之后的
	/// </summary>
    public void InitAfterUpload()
    {
        if (this.m_isInitAfterUpload)
            return;
        this.m_isInitAfterUpload = true;

        UGUIEventSystem.instance.Init(false);
        GameLanguage.Init();
        Localization.language = GameLanguage.strCurLanguage;
        AssetBundleManager.instance.Init();
    }

	void Update() {
		_Exc_Up(Time.deltaTime,Time.unscaledDeltaTime);
	}
	
	void LateUpdate() {
		_Exc_LateUp();
	}

	/// <summary>
	/// 销毁
	/// </summary>
	override protected void OnCall4Destroy() {
		_mgrGobj = null;
		onUpdate = null;
		onLateUpdate = null;
		upItem = null;
		upLateItem = null;
		mListUps.Clear();
		mListLateUps.Clear();
		upList.Clear();
		upLateList.Clear();
	}

	void _Exc_Up(float dt,float unscaledDt){
		upList.AddRange(mListUps);
		upLens = upList.Count;
		for (int i = 0; i < upLens; i++)
		{
			upItem = upList[i];
			if(upItem != null && upItem.IsOnUpdate()){
				upItem.OnUpdate(dt,unscaledDt);
			}
		}
		upList.Clear();

		if(onUpdate != null)
		{
			onUpdate(dt,unscaledDt);
		}
	}

	void _Exc_LateUp(){
		upLateList.AddRange(mListLateUps);
		lateLens = upLateList.Count;
		for (int i = 0; i < lateLens; i++)
		{
			upLateItem = upLateList[i];
			if(upLateItem != null && upLateItem.IsOnLateUpdate()){
				upLateItem.OnLateUpdate();
			}
		}
		upLateList.Clear();

		if(onLateUpdate != null)
		{
			onLateUpdate();
		}
	}
	
	static public void RegisterUpdate(IUpdate up) {
		if(mListUps.Contains(up))
			return;
		mListUps.Add(up);
	}

	static public void DiscardUpdate(IUpdate up) {
		mListUps.Remove(up);
	}

	static public void DisposeUpEvent(DF_OnUpdate call,bool isReBind) {
		onUpdate -= call;
		if(isReBind)
		{
			if(onUpdate == null)
				onUpdate = call;
			else
				onUpdate += call;
		}
	}

	static public void RegisterLateUpdate(ILateUpdate up) {
		if(mListLateUps.Contains(up))
			return;
		mListLateUps.Add(up);
	}

	static public void DiscardLateUpdate(ILateUpdate up) {
		mListLateUps.Remove(up);
	}

	static public void DisposeLateUpEvent(Action call,bool isReBind) {
		onLateUpdate -= call;
		if(isReBind)
		{
			if(onLateUpdate == null)
				onLateUpdate = call;
			else
				onLateUpdate += call;
		}
	}
}