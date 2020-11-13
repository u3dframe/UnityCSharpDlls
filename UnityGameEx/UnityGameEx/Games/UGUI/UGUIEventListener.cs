using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public delegate void DF_UGUIPos(GameObject gameObject,Vector2 pos);
public delegate void DF_UGUI2V2(GameObject gameObject,Vector2 pos,Vector2 delta);
public delegate void DF_UGUIV2Bool(GameObject gameObject,bool isBl,Vector2 pos);

/// <summary>
/// 类名 : UGUIEventListener
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-03-08 16:37
/// 功能 : 处理UGUI界面上事件
/// </summary>
public class UGUIEventListener : EventTrigger {
	static public UGUIEventListener Get(GameObject gobj,bool isAdd){
		return UtilityHelper.Get<UGUIEventListener>(gobj,isAdd);
	}

	static public UGUIEventListener Get(GameObject gobj){
		return Get(gobj,true);
	}

	static public float maxDistance = 70f;
	
	[HideInInspector] public event DF_UGUIV2Bool onMouseEnter;

	[HideInInspector] public event DF_UGUIPos onClick;
	
	[HideInInspector] public event DF_UGUIPos onBegDrag;
	
	[HideInInspector] public event DF_UGUI2V2 onDraging;
	
	[HideInInspector] public event DF_UGUIPos onEndDrag;
	
	[HideInInspector] public event DF_UGUIPos onDrop;

	[HideInInspector] public event DF_UGUIV2Bool onPress;

	
	private Vector2 v2Start;
	bool _isPressed = false,_isCanClick = false;

	float press_time = 0,diff_time = 0,dis_curr = 0,
	limit_time = 0.2f,limit_dis_min = 0.1f * 0.1f,limit_dis_max = 0;
	bool _isAppQuit = false;

	[HideInInspector] public bool m_isPropagation = false; // 是否透传

	[HideInInspector] public bool m_isSyncScroll = true;
	ScrollRect _sclParent = null;

	ScrollRect GetScrollInParent(Transform trsf)
    {
		if(trsf == null) return null;
		ScrollRect ret = trsf.GetComponent<ScrollRect> ();
		if (ret != null) return ret;
		return GetScrollInParent(trsf.parent);
    }

	ScrollRect GetScrollInParent(GameObject gobj)
    {
		if(gobj == null) return null;
		return GetScrollInParent(gobj.transform);
    }
	
	void Awake(){
		this.limit_dis_max = maxDistance * maxDistance;
		_sclParent = GetScrollInParent(transform);
	}

	void OnDisable()
    {
		if(this._isAppQuit) return;
		
		if (_isPressed && onPress != null) {
			onPress (gameObject, false, transform.position);
		}
        _isPressed = false;
    }

    void OnEnable()
    {
		if(this._isAppQuit) return;

		_isPressed = false;
		press_time = 0;
		diff_time = 0;
		v2Start = Vector2.zero;
    }
	
	void OnApplicationQuit(){
		this._isAppQuit = true;
	}

	// 移入
	override public void OnPointerEnter (PointerEventData eventData){
		if (onMouseEnter != null) {
			onMouseEnter (gameObject,true,eventData.position);
		}
	}

	// 移出
	override public void OnPointerExit (PointerEventData eventData){
		if (onMouseEnter != null) {
			onMouseEnter (gameObject,false, eventData.position);
		}
	}

	// 按下
	override public void OnPointerDown (PointerEventData eventData){
		_isPressed = true;
		press_time = Time.realtimeSinceStartup;
		v2Start = eventData.position;
		if(m_isSyncScroll && _sclParent != null){
			_sclParent.OnBeginDrag(eventData);
		}
        PropagationFirst(eventData, ExecuteEvents.pointerDownHandler);
        if (onPress != null) {
			onPress (gameObject, _isPressed, eventData.position);
		}
	}

	// 抬起
	override public void OnPointerUp (PointerEventData eventData){
		_isPressed = false;
		if (press_time > 0) {
			diff_time = Time.realtimeSinceStartup - press_time;
			press_time = 0;
		}

		if(m_isSyncScroll && _sclParent != null){
			_sclParent.OnEndDrag(eventData);
		}
        PropagationFirst(eventData, ExecuteEvents.pointerUpHandler);
        if (onPress != null) {
			onPress (gameObject, _isPressed, eventData.position);
		}
	}

	// 单击
	override public void OnPointerClick (PointerEventData eventData){
		if (press_time > 0) {
			diff_time = Time.realtimeSinceStartup - press_time;
			press_time = 0;
		}
	
		dis_curr = (eventData.position - v2Start).sqrMagnitude;
		_isCanClick = dis_curr <= limit_dis_min;
		if (!_isCanClick) {
			_isCanClick = dis_curr <= limit_dis_max && diff_time <= limit_time;
		}
		
		v2Start = eventData.position;
		diff_time = 0;
		if (!_isCanClick) return;
        PropagationFirst(eventData, ExecuteEvents.submitHandler);
        PropagationFirst(eventData, ExecuteEvents.pointerClickHandler);
        if (onClick != null) {
			onClick (gameObject, eventData.position);
		}
	}
	
    // 开始拖拽
    override public void OnBeginDrag(PointerEventData eventData)
    {
		if(m_isSyncScroll && _sclParent != null){
			_sclParent.OnBeginDrag(eventData);
		}
        if (onBegDrag != null) {
			onBegDrag (gameObject, eventData.position);
		}
    }
	
	// 推拽中
	override public void OnDrag (PointerEventData eventData){
		if(m_isSyncScroll && _sclParent != null){
			_sclParent.OnDrag(eventData);
		}
		if (onDraging != null) {
			onDraging (gameObject, eventData.position,eventData.delta);
		}
	}
	
	// 结束拖拽
    override public void OnEndDrag(PointerEventData eventData)
    {
		if(m_isSyncScroll && _sclParent != null){
			_sclParent.OnEndDrag(eventData);
		}
        if (onEndDrag != null) {
			onEndDrag (gameObject, eventData.position);
		}
    }
	
	// 将元素拖拽到另外一个元素下面执行
    override public void OnDrop(PointerEventData eventData)
    {
        if (onDrop != null) {
			onDrop (gameObject,eventData.position);
		}
    }

	public void PropagationFirst<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function) where T : IEventSystemHandler
    {
		if(!this.m_isPropagation)
			return;
		
		// 参考 http://www.xuanyusong.com/archives/4241 
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        GameObject current = data.pointerCurrentRaycast.gameObject,gobj;
		int lens = results.Count;
		if(lens <= 0)
			return;

		bool isDo = false;
        for (int i = 0; i < lens; i++)
        {
			gobj = results[i].gameObject;
            if (current != gobj)
            {
				do
				{
					isDo = ExecuteEvents.Execute(gobj, data, function);
                    if (isDo)
                        break;

					if (gobj.transform.parent == null)
                        break;
					gobj = gobj.transform.parent.gameObject;
				} while (!isDo);

				if(isDo)
                	break;
            }
        }
    }
}
