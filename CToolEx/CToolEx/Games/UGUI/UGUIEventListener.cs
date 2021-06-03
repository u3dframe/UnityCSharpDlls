using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public delegate void DF_UGUIPos(GameObject gameObject, Vector2 pos);
public delegate void DF_UGUI2V2(GameObject gameObject, Vector2 pos, Vector2 delta);
public delegate void DF_UGUIV2Bool(GameObject gameObject, bool isBl, Vector2 pos);

/// <summary>
/// 类名 : UGUIEventListener
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-03-08 16:37
/// 功能 : 处理UGUI界面上事件
/// </summary>
public class UGUIEventListener : EventTrigger
{
    static public UGUIEventListener Get(UnityEngine.Object uobj, bool isAdd)
    {
        return GHelper.Get<UGUIEventListener>(uobj, isAdd);
    }

    static public UGUIEventListener Get(UnityEngine.Object uobj)
    {
        return Get(uobj, true);
    }

    static public float maxDistance = 70f;
    static public float limitTime = 0.2f;

    enum SyncEventType
    {
        PointerEnter = 0,
        PointerExit,
        PointerDown,
        PointerUp,
        BeginDrag,
        Drag,
        EndDrag,
        Drop,
    }

    public event DF_UGUIV2Bool onMouseEnter;
    public event DF_UGUIPos onClick;
    public event DF_UGUIPos onBegDrag;
    public event DF_UGUI2V2 onDraging;
    public event DF_UGUIPos onEndDrag;
    public event DF_UGUIPos onDrop;
    public event DF_UGUIV2Bool onPress;

    private Vector2 v2Start;
    bool _isPressed = false, _isCanClick = false;

    float press_time = 0, diff_time = 0, dis_curr = 0,
    limit_dis_min = 0.1f * 0.1f, limit_dis_max = 0;
    bool _isAppQuit = false;

    [HideInInspector] public bool m_isPropagation = false; // 是否透传
    [HideInInspector] public bool m_isSyncScroll = true;
    public ScrollRect m_sclParent { get; set; }
    
    void Awake()
    {
        this.limit_dis_max = maxDistance * maxDistance;
        m_sclParent = GHelper.GetInParent<ScrollRect>(transform,true);
    }

    void OnDisable()
    {
        if (this._isAppQuit) return;

        if (_isPressed && onPress != null)
            onPress(gameObject, false, transform.position);
        _isPressed = false;
    }

    void OnEnable()
    {
        if (this._isAppQuit) return;

        _isPressed = false;
        press_time = 0;
        diff_time = 0;
        v2Start = Vector2.zero;
    }

    void OnApplicationQuit()
    {
        this._isAppQuit = true;
    }

    // 移入
    override public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_isSyncScroll)
            this.ExcuteSyncDrag(eventData, SyncEventType.PointerEnter);
        if (onMouseEnter != null)
            onMouseEnter(gameObject, true, eventData.position);
    }

    // 移出
    override public void OnPointerExit(PointerEventData eventData)
    {
        if (m_isSyncScroll)
            this.ExcuteSyncDrag(eventData, SyncEventType.PointerExit);
        if (onMouseEnter != null)
            onMouseEnter(gameObject, false, eventData.position);
    }

    // 按下
    override public void OnPointerDown(PointerEventData eventData)
    {
        if (m_isSyncScroll)
            this.ExcuteSyncDrag(eventData, SyncEventType.PointerDown);

        _isPressed = true;
        press_time = Time.realtimeSinceStartup;
        v2Start = eventData.position;
        if (onPress != null)
            onPress(gameObject, _isPressed, eventData.position);
        PropagationFirst(eventData, ExecuteEvents.pointerDownHandler);
    }

    // 抬起
    override public void OnPointerUp(PointerEventData eventData)
    {
        if (m_isSyncScroll)
            this.ExcuteSyncDrag(eventData, SyncEventType.PointerUp);

        _isPressed = false;
        if (press_time > 0)
        {
            diff_time = Time.realtimeSinceStartup - press_time;
            press_time = 0;
        }
        if (onPress != null)
            onPress(gameObject, _isPressed, eventData.position);
        PropagationFirst(eventData, ExecuteEvents.pointerUpHandler);
    }

    // 单击
    override public void OnPointerClick(PointerEventData eventData)
    {
        if (press_time > 0)
        {
            diff_time = Time.realtimeSinceStartup - press_time;
            press_time = 0;
        }

        dis_curr = (eventData.position - v2Start).sqrMagnitude;
        _isCanClick = dis_curr <= limit_dis_min;
        if (!_isCanClick)
        {
            _isCanClick = dis_curr <= limit_dis_max && diff_time <= limitTime;
        }

        v2Start = eventData.position;
        diff_time = 0;
        if (!_isCanClick) return;
        if (onClick != null)
            onClick(gameObject, eventData.position);
        PropagationFirst(eventData, ExecuteEvents.submitHandler);
        PropagationFirst(eventData, ExecuteEvents.pointerClickHandler);
    }

    // 开始拖拽
    override public void OnBeginDrag(PointerEventData eventData)
    {
        if (m_isSyncScroll)
        {
            if (m_sclParent != null)
                m_sclParent.OnBeginDrag(eventData);

            this.ExcuteSyncDrag(eventData, SyncEventType.BeginDrag);
        }

        if (onBegDrag != null)
            onBegDrag(gameObject, eventData.position);
    }

    // 推拽中
    override public void OnDrag(PointerEventData eventData)
    {
        if (m_isSyncScroll)
        {
            if (m_sclParent != null)
                m_sclParent.OnDrag(eventData);

            this.ExcuteSyncDrag(eventData, SyncEventType.Drag);
        }
        if (onDraging != null)
            onDraging(gameObject, eventData.position, eventData.delta);
    }

    // 结束拖拽
    override public void OnEndDrag(PointerEventData eventData)
    {
        if (m_isSyncScroll)
        {
            if (m_sclParent != null)
                m_sclParent.OnEndDrag(eventData);

            this.ExcuteSyncDrag(eventData, SyncEventType.EndDrag);
        }
        if (onEndDrag != null)
            onEndDrag(gameObject, eventData.position);
    }

    // 将元素拖拽到另外一个元素下面执行
    override public void OnDrop(PointerEventData eventData)
    {
        if (m_isSyncScroll)
            this.ExcuteSyncDrag(eventData, SyncEventType.Drop);
        if (onDrop != null)
            onDrop(gameObject, eventData.position);
    }

    public void PropagationFirst<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function) where T : IEventSystemHandler
    {
        if (!this.m_isPropagation)
            return;

        // 参考 http://www.xuanyusong.com/archives/4241 
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        GameObject current = data.pointerCurrentRaycast.gameObject, gobj;
        int lens = results.Count;
        if (lens <= 0)
            return;

        bool isDo = false;
        Transform trsfCurrent = current?.transform;
        for (int i = 0; i < lens; i++)
        {
            gobj = results[i].gameObject;
            if (!gobj || !gobj.activeInHierarchy || current == gobj)
                continue;
            if(trsfCurrent != null && GHelper.IsInParentRecursion(gobj, trsfCurrent))
                continue;

            do
            {
                isDo = ExecuteEvents.Execute(gobj, data, function); // 执行脚本事件,如果没脚本，只有 Raycast 永远是 False
                if (isDo)
                    break;

                if (gobj.transform.parent == null)
                    break;
                gobj = gobj.transform.parent.gameObject;
            } while (!isDo);
            break; //只转发给第一个响应
        }
    }

    void OnDestroy()
    {
        this.onMouseEnter = null;
        this.onClick = null;
        this.onBegDrag = null;
        this.onDraging = null;
        this.onEndDrag = null;
        this.onDrop = null;
        this.onPress = null;
        this.m_sclParent = null;
        this.m_listSyncDrag.Clear();
    }

    public void OnlyOnceCallMEnter(DF_UGUIV2Bool call, bool isAdd = true)
    {
        if (call == null)
            return;
        this.onMouseEnter -= call;
        if (isAdd)
            this.onMouseEnter += call;
    }

    public void OnlyOnceCallClick(DF_UGUIPos call, bool isAdd = true)
    {
        if (call == null)
            return;
        this.onClick -= call;
        if (isAdd)
            this.onClick += call;
    }

    public void OnlyOnceCallBegDrag(DF_UGUIPos call, bool isAdd = true)
    {
        if (call == null)
            return;
        this.onBegDrag -= call;
        if (isAdd)
            this.onBegDrag += call;
    }

    public void OnlyOnceCallDrag(DF_UGUI2V2 call, bool isAdd = true)
    {
        if (call == null)
            return;
        this.onDraging -= call;
        if (isAdd)
            this.onDraging += call;
    }

    public void OnlyOnceCallEndDrag(DF_UGUIPos call, bool isAdd = true)
    {
        if (call == null)
            return;
        this.onEndDrag -= call;
        if (isAdd)
            this.onEndDrag += call;
    }

    public void OnlyOnceCallDrop(DF_UGUIPos call, bool isAdd = true)
    {
        if (call == null)
            return;
        this.onDrop -= call;
        if (isAdd)
            this.onDrop += call;
    }

    public void OnlyOnceCallPress(DF_UGUIV2Bool call, bool isAdd = true)
    {
        if (call == null)
            return;
        this.onPress -= call;
        if (isAdd)
            this.onPress += call;
    }

    List<EventTrigger> m_listSyncDrag = new List<EventTrigger>();
    public void AddSyncDrag4EventTrigger(UnityEngine.Object uobj)
    {
        if (this._isAppQuit)
            return;

        GameObject gobj = GHelper.ToGObj(uobj);
        if (gobj == null || !gobj || gobj == this.gameObject)
            return;

        EventTrigger[] _evts = gobj.GetComponents<EventTrigger>();
        if (GHelper.IsNullOrEmpty(_evts))
            return;
        EventTrigger _evt = null;
        for (int i = 0; i < _evts.Length; i++)
        {
            _evt = _evts[i];
            if (this.m_listSyncDrag.Contains(_evt))
                continue;
            this.m_listSyncDrag.Add(_evt);
        }
    }

    void ExcuteSyncDrag(PointerEventData eventData, SyncEventType syncType)
    {
        if (this._isAppQuit)
            return;
        if (this.m_listSyncDrag == null || this.m_listSyncDrag.Count <= 0)
            return;
        int _lens = this.m_listSyncDrag.Count;
        EventTrigger _evt = null;
        for (int i = 0; i < _lens; i++)
        {
            _evt = this.m_listSyncDrag[i];
            switch (syncType)
            {
                case SyncEventType.BeginDrag:
                    _evt.OnBeginDrag(eventData);
                    break;
                case SyncEventType.Drag:
                    _evt.OnDrag(eventData);
                    break;
                case SyncEventType.EndDrag:
                    _evt.OnEndDrag(eventData);
                    break;
                case SyncEventType.PointerEnter:
                    _evt.OnPointerEnter(eventData);
                    break;
                case SyncEventType.PointerExit:
                    _evt.OnPointerExit(eventData);
                    break;
                case SyncEventType.PointerDown:
                    _evt.OnPointerDown(eventData);
                    break;
                case SyncEventType.PointerUp:
                    _evt.OnPointerUp(eventData);
                    break;
                case SyncEventType.Drop:
                    _evt.OnDrop(eventData);
                    break;
            }
        }
    }
}
