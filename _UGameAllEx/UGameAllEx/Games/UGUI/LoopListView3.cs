using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class LoopListView3 : UIBehaviour, IEventSystemHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public delegate string DF_GetItemName(int index);

    public delegate void DF_OnIndexChanged(int index);

    public delegate void DF_OnItemCreated(GameObject obj);

    public delegate void DF_SetItemData(GameObject obj, int index, float normalizedDistance);

    public delegate void DF_OnValueChanged(float value);

    protected class ListItem
    {
        public GameObject gameObject;
        public int index;
        public string name;
        public float normalizedDistance;
    }

    protected class VirtualContent
    {
        private readonly LoopListView3 view;
        private float[] data = null;
        private readonly List<float> worldPos = new List<float>() { 0 };
        private float initWorldPos = 0;
        private float pivot = 0;
        private float lossyScale = 1;

        public VirtualContent(LoopListView3 view)
        {
            this.view = view;
        }

        public float Length
        {
            get
            {
                if (data == null || data.Length == 0) return 0;
                if (view.loop) return LengthWithoutPadding;
                float padding;
                if (view.vertical)
                    padding = view.contentPadding.top + view.contentPadding.bottom;
                else
                    padding = view.contentPadding.left + view.contentPadding.right;
                return data[data.Length - 1] + (view.contentSpacing * (data.Length - 1) + padding) * lossyScale;
            }
        }

        private float LengthWithoutPadding
        {
            get
            {
                if (data == null || data.Length == 0) return 0;
                return data[data.Length - 1] + (view.contentSpacing * (data.Length - 1)) * lossyScale;
            }
        }

        public float LowBound
        {
            get { return worldPos[GetNearestContentIndexToCenter()]; }
            //set { WorldMove(value - LowBound); }
        }

        public float HighBound
        {
            get { return LowBound + Length; }
            //set { WorldMove(value - HighBound); }
        }

        public float normalizedPosition
        {
            get
            {
                var viewDelta = view.viewWorldHigh - view.viewWorldLow;
                var total = Length - viewDelta;
                var i = GetNearestContentIndexToCenter();
                return (initWorldPos - worldPos[i]) / total + pivot;
            }
            set
            {
                //var viewDelta = view.viewWorldHigh - view.viewWorldLow;
                //var total = Length - viewDelta;
                //var i = GetNearestContentIndexToCenter();
                //var delta = initWorldPos - (value - pivot) * total - worldPos[i];
                //WorldMove(delta);
            }
        }

        public void Reset()
        {
            var count = view.itemCount;
            data = new float[count];
            float v = 0;
            for (int i = 0; i < count; i++)
            {
                if (view.vertical)
                {
                    var obj = view.GetPrefabByIndex(count - i - 1);
                    var rt = obj.GetComponent<RectTransform>();
                    v += rt.rect.height * rt.lossyScale.y;
                }
                else
                {
                    var obj = view.GetPrefabByIndex(i);
                    var rt = obj.GetComponent<RectTransform>();
                    v += rt.rect.width * rt.lossyScale.x;
                }
                data[i] = v;
            }
            //重置坐标
            worldPos.RemoveRange(1, worldPos.Count - 1);
            var position = view.contentInitPosition;
            if (view.vertical)
            {
                pivot = view.content.pivot.y;
                worldPos[0] = position.y - Length * pivot;
                lossyScale = view.content.lossyScale.y;
            }
            else
            {
                pivot = view.content.pivot.x;
                worldPos[0] = position.x - Length * pivot;
                lossyScale = view.content.lossyScale.x;
            }
            initWorldPos = worldPos[0];
            CheckLoop();
        }

        public void WorldMove(float delta)
        {
            for (int i = 0; i < worldPos.Count; i++)
                worldPos[i] += delta;
            CheckLoop();
        }

        private void CheckLoop()
        {
            if (!view.loop || data.Length <= 1) return;   //元素数量不超过1个不循环
            var lenWithoutPadding = LengthWithoutPadding;
            var viewWorldHigh = view.viewWorldHigh;
            var viewWorldLow = view.viewWorldLow;
            if (lenWithoutPadding <= viewWorldHigh - viewWorldLow) return;    //content长度不超过视口不循环
            var nearestContIndex = GetNearestContentIndexToCenter();
            var contentLow = worldPos[nearestContIndex];
            var contentHigh = contentLow + lenWithoutPadding;
            var spacing = view.contentSpacing * lossyScale;
            if (nearestContIndex == worldPos.Count - 1 && contentHigh + spacing < viewWorldHigh)  //在high的一边添加content
            {
                var n = Mathf.FloorToInt((viewWorldLow - contentHigh) / (lenWithoutPadding + spacing));
                var pos = contentHigh + spacing;
                if (n > 0)  //离中心最近的content已完全离开视口超过一个content长度
                    pos += n * (lenWithoutPadding + spacing);
                if (CheckNewContent(pos))
                {
                    worldPos.Insert(nearestContIndex + 1, pos);
                    pos += lenWithoutPadding + spacing;
                    if (pos < viewWorldHigh && CheckNewContent(pos))  //最多2个content位于视口内
                        worldPos.Insert(nearestContIndex + 2, pos);
                }
            }
            if (nearestContIndex == 0 && contentLow - spacing > viewWorldLow)    //在low的一边添加content
            {
                var n = Mathf.FloorToInt((contentLow - viewWorldHigh) / (lenWithoutPadding + spacing));
                var pos = contentLow - spacing - lenWithoutPadding;
                if (n > 0)
                    pos -= n * (lenWithoutPadding + spacing);
                if (CheckNewContent(pos))
                {
                    worldPos.Insert(nearestContIndex, pos);
                    pos -= spacing;
                    if (pos > viewWorldLow && CheckNewContent(pos - lenWithoutPadding))
                        worldPos.Insert(nearestContIndex, pos - lenWithoutPadding);
                }
            }
            //移除完全在视口之外的content
            worldPos.RemoveAll(pos => pos > viewWorldHigh || (pos + lenWithoutPadding) < viewWorldLow);
        }

        private int GetNearestContentIndexToCenter()
        {
            float minDistance = float.MaxValue;
            int ret = -1;
            var halfLen = LengthWithoutPadding / 2;
            for (int i = 0; i < worldPos.Count; i++)
            {
                var dis = Mathf.Abs(worldPos[i] + halfLen - view.worldCenter);
                if (dis < minDistance)
                {
                    minDistance = dis;
                    ret = i;
                }
            }
            return ret;
        }

        private readonly HashSet<int> checkSet = new HashSet<int>();

        private bool CheckNewContent(float pos)
        {
            //判断如果添加了content是否会导致视口内出现重复列表元素
            //如果重复，返回false
            GetVisibleItemIndexes(checkSet, out _, out _);
            if (GetVisibleItemRange(pos, out var s, out var e))
            {
                for (int i = s; i < e; i++)
                {
                    var x = i;
                    if (view.vertical)
                        x = view.itemCount - x - 1;
                    if (checkSet.Contains(x))
                        return false;
                }
            }
            else
                return false;
            return true;
        }

        private void GetIntervalByInternalIndex(float contentPos, int index, out float low, out float high)
        {
            low = high = -1;
            if (view == null || view.itemCount == 0) return;
            var padding = 0;
            if (!view.loop)
                padding = view.vertical ? view.contentPadding.bottom : view.contentPadding.left;
            var delta = contentPos + (padding + view.contentSpacing * index) * lossyScale;
            high = data[index] + delta;
            if (index == 0)
                low = 0;
            else
                low = data[index - 1];
            low += delta;
        }

        private void GetIntervalIncludeSpacingByInternalIndex(float contentPos, int index, out float low, out float high)
        {
            GetIntervalByInternalIndex(contentPos, index, out low, out high);
            if (view.itemCount == 0) return;
            if (view.loop || index > 0)
                low -= view.contentSpacing * lossyScale / 2;
            if (view.loop || index < view.itemCount - 1)
                high += view.contentSpacing * lossyScale / 2;
        }

        public float GetDistanceToCenterFrom(int index)
        {
            if (view.vertical)
                index = view.itemCount - index - 1;
            var minDistance = float.MaxValue;
            for (int i = 0; i < worldPos.Count; i++)
            {
                GetIntervalByInternalIndex(worldPos[i], index, out var low, out var high);
                var dis = (low + high) / 2 - view.worldCenter;
                if (Mathf.Abs(dis) < Mathf.Abs(minDistance))
                {
                    minDistance = dis;
                }
            }
            return minDistance;
        }

        //二分搜索，计算当前定位到中心的物体索引
        public int CalcLocationIndex()
        {
            if (view.itemCount == 0) return -1;
            int s = 0;
            int e = view.itemCount;
            int mid;
            float center = view.worldCenter;
            var contIndex = GetNearestContentIndexToCenter();
            while (true)
            {
                mid = (s + e) / 2;
                if (mid >= e) break;
                GetIntervalIncludeSpacingByInternalIndex(worldPos[contIndex], mid, out float low, out float high);
                if (low <= center && center < high) break;
                if (low > center)
                    e = mid;
                else
                    s = mid + 1;
            }
            if (mid >= view.itemCount) mid = view.itemCount - 1;
            if (view.vertical)
                mid = view.itemCount - mid - 1;
            return mid;
        }

        //二分搜索，获得一个content中可见物体的索引范围 [outStart, outEnd)
        private bool GetVisibleItemRange(float contentPos, out int outStart, out int outEnd)
        {
            outStart = -1;
            outEnd = -1;
            int s = 0;
            int e = view.itemCount;
            int mid;
            float low, high;
            while (true)
            {
                mid = (s + e) / 2;
                if (mid >= e) return false;
                GetIntervalByInternalIndex(contentPos, mid, out low, out high);
                if (IsIntervalVisible(low, high))
                    break;
                else
                {
                    if (high < view.viewWorldLow)
                        s = mid + 1;
                    else
                        e = mid;
                }
            }
            outStart = mid;
            while (outStart - 1 >= 0)
            {
                GetIntervalByInternalIndex(contentPos, outStart - 1, out low, out high);
                if (IsIntervalVisible(low, high))
                    outStart--;
                else
                    break;
            }
            outEnd = mid + 1;
            while (outEnd + 1 <= view.itemCount)
            {
                GetIntervalByInternalIndex(contentPos, outEnd, out low, out high);
                if (IsIntervalVisible(low, high))
                    outEnd++;
                else
                    break;
            }
            return true;
        }

        private void GetVisibleItemIndexes(ICollection<int> results, out float low, out float high)
        {
            low = float.MaxValue;
            high = float.MinValue;
            results.Clear();
            var len = Length;
            for (int i = 0; i < worldPos.Count; i++)
            {
                if (worldPos[i] >= view.viewWorldHigh || (worldPos[i] + len) <= view.viewWorldLow)
                    continue;
                if (GetVisibleItemRange(worldPos[i], out var s, out var e))
                {
                    GetIntervalByInternalIndex(worldPos[i], s, out var sLow, out _);
                    GetIntervalByInternalIndex(worldPos[i], e - 1, out _, out var eHigh);
                    low = Mathf.Min(low, sLow);
                    high = Mathf.Max(high, eHigh);
                    for (int x = s; x < e; x++)
                    {
                        if (view.vertical)
                            results.Add(view.itemCount - x - 1);
                        else
                            results.Add(x);
                    }
                }
            }
        }

        public void GetVisibleItemIndexList(List<int> results, out float low, out float high)
        {
            GetVisibleItemIndexes(results, out low, out high);
            if (view.vertical)
                results.Reverse();
        }

        public bool IsIntervalVisible(float low, float high)
        {
            return low <= view.viewWorldHigh && high >= view.viewWorldLow;
        }

    }

    protected readonly Dictionary<string, List<ListItem>> pool = new Dictionary<string, List<ListItem>>();

    protected readonly List<ListItem> itemList = new List<ListItem>();

    protected VirtualContent virtualContent;

    protected Rect viewWorldRect;

    protected float viewWorldLow
    {
        get
        {
            if (vertical)
                return viewWorldRect.min.y;
            return viewWorldRect.min.x;
        }
    }

    protected float viewWorldHigh
    {
        get
        {
            if (vertical)
                return viewWorldRect.max.y;
            return viewWorldRect.max.x;
        }
    }

    protected bool isInitted = false;

    protected bool isForceMoving = false;

    protected bool isDragging = false;

    protected bool isPointerDown = false;

    protected int currentPointerId = 0;

    public DF_GetItemName GetItemName;

    //当视口中心进入item的范围时调用
    public DF_OnIndexChanged OnIndexChanged;

    //当视口中心到达或越过item中心时调用
    public DF_OnIndexChanged OnMovingCompleted;

    public DF_OnItemCreated OnItemCreated;

    public DF_SetItemData SetItemData;

    public DF_OnValueChanged OnValueChanged;

    public bool interactable = true;

    [SerializeField]
    protected RectTransform content;

    [SerializeField]
    protected RectTransform viewport;

    public enum MovementType
    {
        AutoAlignment,
        Unrestricted,
        //Elastic,
        //Clamped,
    }

    [SerializeField]
    protected MovementType movementType = MovementType.AutoAlignment;

    public enum LayoutType
    {
        Horizontal,
        Vertical,
    }

    [SerializeField]
    protected LayoutType layoutType = LayoutType.Horizontal;

    public bool vertical { get { return layoutType == LayoutType.Vertical; } }

    public int itemCount { get; protected set; }

    private int _currentIndex = 0;
    public int currentIndex
    {
        get => _currentIndex;
        set
        {
            _currentIndex = Mathf.Clamp(value, 0, itemCount - 1);
            CheckedOnIndexChanged(_currentIndex);
        }
    }

    public float normalizedPosition
    {
        get => virtualContent.normalizedPosition;
        set
        {
            if (isForceMoving || isDragging) return;
            worldDragSpeed = 0;
            virtualContent.normalizedPosition = value;
        }
    }

    [SerializeField]
    [Range(0.01f, 0.99f)]
    protected float center = 0.5f;

    public float worldCenter
    {
        get
        {
            if (vertical)
            {
                var height = viewRect.rect.height * viewRect.lossyScale.y;
                return viewRect.position.y - height * viewRect.pivot.y + height * center;
            }
            var width = viewRect.rect.width * viewRect.lossyScale.x;
            return viewRect.position.x - width * viewRect.pivot.x + width * center;
        }
    }

    protected Vector3 contentInitPosition
    {
        get
        {
            var pivot = content.pivot;
            var parent = content.parent;
            var parentRect = parent.GetComponent<RectTransform>();
            var parentWidth = parentRect.rect.width * parentRect.lossyScale.x;
            var parentHeight = parentRect.rect.height * parentRect.lossyScale.y;
            var delta = new Vector3(parentWidth * (pivot.x - parentRect.pivot.x), parentHeight * (pivot.y - parentRect.pivot.y), 0);
            return parent.position + delta;     //content pivot坐标
        }
    }

    [SerializeField]
    protected bool loop = false;

    [Range(0.01f, 1f)]
    [SerializeField]
    protected float distanceMinDelta = 0.01f;

    //拖动后惯性速度的衰减率
    [Range(0.01f, 0.9f)]
    [SerializeField]
    protected float inertiaDecelerationRate = 0.3f;

    //自动定位速度随定位距离的正比系数
    [Range(0.1f, 20f)]
    [SerializeField]
    protected float autoSpeedChangeRate = 8f;

    //自动定位速度最小值
    [Range(0.1f, 50f)]
    [SerializeField]
    protected float autoSpeedMinValue = 4f;

    [SerializeField]
    protected RectOffset contentPadding;

    [SerializeField]
    protected int contentSpacing;

    [SerializeField]
    protected TextAnchor contentChildAlignment;

    [SerializeField]
    protected Scrollbar scrollbar;

    public enum ScrollbarVisibility
    {
        Permanent,
        AutoHide,
    }

    [SerializeField]
    protected ScrollbarVisibility scrollbarVisibility = ScrollbarVisibility.AutoHide;

    [SerializeField]
    protected List<GameObject> itemPrefabs = new List<GameObject>();

    private int prevChangedIndex = -1;

    private int prevMovingCompletedIndex = -1;

    private int currMovingCompletedIndex = -1;
    protected void CheckedOnIndexChanged(int index)
    {
        if (index == prevChangedIndex) return;
        prevChangedIndex = index;
        OnIndexChanged?.Invoke(index);
    }

    protected void CheckedOnMovingCompleted()
    {
        if (currMovingCompletedIndex < 0 || currMovingCompletedIndex == prevMovingCompletedIndex) return;
        prevMovingCompletedIndex = currMovingCompletedIndex;
        OnMovingCompleted?.Invoke(currMovingCompletedIndex);
        currMovingCompletedIndex = -1;
    }

    protected float CalcAutoMovingSpeed(int index)
    {
        var distance = GetDistanceToCenterFrom(index);
        if (distance == 0) return 0;
        var s = -distance * autoSpeedChangeRate;
        if (Mathf.Abs(s) < autoSpeedMinValue)
        {
            s = s > 0 ? autoSpeedMinValue : -autoSpeedMinValue;
        }
        return s;
    }

    protected RectTransform viewRect
    {
        get
        {
            if (viewport != null) return viewport;
            Transform p = content.parent;
            while (p != null)
            {
                if (p.GetComponent<Mask>() != null)
                    return p.GetComponent<RectTransform>();
                else
                    p = p.parent;
            }
            return GetComponent<RectTransform>();
        }
    }

    public void SetItemCount(int count, bool resetPos = true)
    {
        if (!isInitted || itemCount <= 0) resetPos = true;
        isInitted = true;
        itemCount = count;
        if (count <= 0)
        {
            foreach (Transform t in content)
            {
                if (t.gameObject.activeSelf)
                    t.gameObject.SetActive(false);
            }
            return;
        }
        prevChangedIndex = -1;
        prevMovingCompletedIndex = -1;
        currMovingCompletedIndex = -1;
        ClearItemList();
        float np = 0;
        if (!resetPos)
            np = normalizedPosition;
        virtualContent.Reset();
        if (movementType == MovementType.AutoAlignment)
            MoveToImmediately(resetPos ? 0 : Mathf.Min(currentIndex, itemCount - 1));
        else
            normalizedPosition = resetPos ? (vertical ? 1 : 0) : np;
        if (scrollbar != null)
            scrollbar.size = (viewWorldHigh - viewWorldLow) / virtualContent.Length;
    }

    public int GetItemIndex(GameObject obj)
    {
        foreach (var item in itemList)
        {
            if (item.gameObject == obj)
                return item.index;
        }
        return -1;
    }

    public float GetDistanceToCenterFrom(int index)
    {
        return virtualContent.GetDistanceToCenterFrom(index);
    }

    public float GetNormalizedDistanceToCenterFrom(int index)
    {
        var dis = GetDistanceToCenterFrom(index);
        if (dis > 0)
            return dis / (viewWorldHigh - worldCenter);
        return dis / (worldCenter - viewWorldLow);
    }

    //立即移动到目标索引，没有动画
    public void MoveToImmediately(int index)
    {
        StopMovement();
        currentIndex = index;
        MoveVirtualContent(-GetDistanceToCenterFrom(currentIndex));
        currMovingCompletedIndex = currentIndex;
    }

    //强制滑动到目标索引，期间不能控制滑块
    public void MoveToForcibly(int index)
    {
        StopMovement();
        currentIndex = index;
        isForceMoving = true;
    }

    public void StopMovement()
    {
        //不能停止自动定位运动
        isForceMoving = false;
        worldDragSpeed = 0;
    }

    protected LoopListView3 syncView = null;

    protected float syncDelta = 0;

    public void Sync()
    {
        if (syncView == null) return;
        syncView.StopMovement();
        float x = (worldCenter - virtualContent.LowBound) / virtualContent.Length;
        var newLowBound = syncView.worldCenter - x * syncView.virtualContent.Length;
        syncView.syncDelta = newLowBound - syncView.virtualContent.LowBound;
    }

    public void SetSyncView(LoopListView3 view)
    {
        syncView = view;
        this.Sync();
    }

    public void IterateItemList(DF_SetItemData callback)
    {
        foreach (var item in itemList)
            callback(item.gameObject, item.index, item.normalizedDistance);
    }

    protected void MoveVirtualContent(float delta)
    {
        virtualContent.WorldMove(delta);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPointerDown || !interactable) return;
        isPointerDown = true;
        worldDragSpeed = 0;
        currentPointerId = eventData.pointerId; //只处理第一个触屏的指针/手指
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != currentPointerId || !interactable) return;
        isPointerDown = false;
    }

    protected float worldDragSpeed = 0;
    protected Vector3 prevPointerPosition;
    protected Vector3 currPointerPosition;
    protected float inertiaLerp = 1;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != currentPointerId || !interactable || isForceMoving) return;
        isDragging = true;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out prevPointerPosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != currentPointerId || !interactable || isForceMoving) return;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out currPointerPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != currentPointerId || !interactable) return;
        isDragging = false;
        inertiaLerp = 0;
    }

    protected GameObject GetPrefabByName(string name)
    {
        foreach (var p in itemPrefabs)
        {
            if (p.name == name)
                return p;
        }
        return null;
    }

    protected GameObject GetPrefabByIndex(int index)
    {
        return GetPrefabByName(GetItemName(index));
    }

    protected ListItem GetItemFromPool(string name)
    {
        pool.TryGetValue(name, out var list);
        ListItem item;
        if (list == null || list.Count == 0)
        {
            var prefab = GetPrefabByName(name);
            if (prefab == null)
                return null;
            var obj = Instantiate(prefab, content, false);
            OnItemCreated?.Invoke(obj);
            item = new ListItem()
            {
                name = name,
                gameObject = obj,
                index = -1,
                normalizedDistance = -9999,
            };
        }
        else
        {
            item = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
        }
        return item;
    }

    protected void PutItemToPool(ListItem item)
    {
        pool.TryGetValue(item.name, out var list);
        if (list == null)
        {
            list = new List<ListItem>();
            pool[item.name] = list;
        }
        list.Add(item);
    }

    protected void ClearItemList()
    {
        foreach (var item in itemList)
            PutItemToPool(item);
        itemList.Clear();
    }

    protected readonly List<int> visibleItemIndexList = new List<int>();

    protected void UpdateVisibleItemIndexList(out float low, out float high)
    {
        virtualContent.GetVisibleItemIndexList(visibleItemIndexList, out low, out high);
    }

    protected static Rect WorldRect(RectTransform rt)
    {
        float rtWidth = rt.rect.width * rt.lossyScale.x;
        float rtHeight = rt.rect.height * rt.lossyScale.y;
        Vector3 position = rt.position;
        return new Rect(position.x - rtWidth * rt.pivot.x, position.y - rtHeight * rt.pivot.y, rtWidth, rtHeight);
    }

    protected void UpdateContent(float lowBound)
    {
        var si = itemList[0].index;
        var ei = itemList[itemList.Count - 1].index;
        var initPos = contentInitPosition;
        var contScale = content.lossyScale;
        if (vertical)
        {
            //开启循环或item不在virtual content边缘时，填充spacing而不是padding
            int margin = (!loop && ei == itemCount - 1) ? contentPadding.bottom : contentSpacing;
            contentLayout.padding.left = contentPadding.left;
            contentLayout.padding.right = contentPadding.right;
            contentLayout.padding.top = (!loop && si == 0) ? contentPadding.top : contentSpacing;
            contentLayout.padding.bottom = margin;
            //ForceRebuildLayoutImmediate()不起作用，调用了依然没刷新content尺寸，先手动计算
            //LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            lowBound -= margin * contScale.y;
            //var contHeight = content.rect.height * contScale.y;
            float height = (margin + contentSpacing * (itemList.Count - 1));
            for (int i = 0; i < itemList.Count; i++)
                height += itemList[i].gameObject.GetComponent<RectTransform>().rect.height;
            height *= contScale.y;
            content.position = new Vector3(initPos.x, lowBound + height * content.pivot.y, initPos.z);
        }
        else
        {
            int margin = (!loop && si == 0) ? contentPadding.left : contentSpacing;
            contentLayout.padding.left = margin;
            contentLayout.padding.right = (!loop && ei == itemCount - 1) ? contentPadding.right : contentSpacing;
            contentLayout.padding.top = contentPadding.top;
            contentLayout.padding.bottom = contentPadding.bottom;
            //LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            lowBound -= margin * contScale.x;
            //var contWidth = content.rect.width * contScale.x;
            float width = (margin + contentSpacing * (itemList.Count - 1));
            for (int i = 0; i < itemList.Count; i++)
                width += itemList[i].gameObject.GetComponent<RectTransform>().rect.width;
            width *= contScale.x;
            content.position = new Vector3(lowBound + width * content.pivot.x, initPos.y, initPos.z);
        }
    }

    private HorizontalOrVerticalLayoutGroup contentLayout;

    protected override void Awake()
    {
        if (!Application.IsPlaying(gameObject)) return;
        virtualContent = new VirtualContent(this);
    }

    protected override void Start()
    {
        if (!Application.IsPlaying(gameObject)) return;
        viewWorldRect = WorldRect(viewRect);
        foreach (var obj in itemPrefabs)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        //借用LayoutGroup和ContentSizeFitter组件布局
        if (vertical)
        {
            contentLayout = content.gameObject.GetComponent<VerticalLayoutGroup>();
            if (contentLayout == null)
                contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        else
        {
            contentLayout = content.gameObject.GetComponent<HorizontalLayoutGroup>();
            if (contentLayout == null)
                contentLayout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
        }
        contentLayout.enabled = true;
        contentLayout.spacing = contentSpacing;
        contentLayout.childAlignment = contentChildAlignment;
        contentLayout.childForceExpandWidth = false;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childControlWidth = false;
        contentLayout.childControlHeight = false;
        contentLayout.childScaleWidth = true;
        contentLayout.childScaleHeight = true;

        var sizeFilter = content.gameObject.GetComponent<ContentSizeFitter>();
        if (sizeFilter == null)
            sizeFilter = content.gameObject.AddComponent<ContentSizeFitter>();
        sizeFilter.enabled = true;
        sizeFilter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFilter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private float prevNormalizedPos = 0;

    private float endMovingDelta = 0.01f;   //触发到达中心回调的最小距离

    private readonly Dictionary<int, ListItem> itemCache = new Dictionary<int, ListItem>();

    protected void LateUpdate()
    {
        if (!Application.IsPlaying(gameObject)) return;
        if (!isInitted) return;

        if (itemCount <= 0) return;

        //content运动
        var deltaTime = Time.deltaTime; //暂时不用Time.unscaledDeltaTime
        var prevDis = GetDistanceToCenterFrom(currentIndex);
        if (isForceMoving)
        {
            var speed = CalcAutoMovingSpeed(currentIndex);
            MoveVirtualContent(speed * deltaTime);
            var dis = GetDistanceToCenterFrom(currentIndex);
            if (dis * prevDis <= 0)    //前后距离异号或其中一个为0，说明物体越过或到达了视口中心
            {
                MoveVirtualContent(-dis);
                isForceMoving = false;
                currMovingCompletedIndex = currentIndex;
            }
        }
        else if (isDragging || !isPointerDown)
        {
            float speed;
            if (isDragging)
            {
                var worldDragVelocity = (currPointerPosition - prevPointerPosition) / deltaTime;
                var newSpeed = vertical ? worldDragVelocity.y : worldDragVelocity.x;
                worldDragSpeed = Mathf.Lerp(worldDragSpeed, newSpeed, deltaTime * 10);
                speed = worldDragSpeed;
                prevPointerPosition = currPointerPosition;
            }
            else if (syncDelta != 0)
            {
                speed = syncDelta / deltaTime;
                syncDelta = 0;
            }
            else
            {
                //惯性速度衰减
                worldDragSpeed *= Mathf.Pow(inertiaDecelerationRate, deltaTime);
                if (Mathf.Abs(worldDragSpeed) < 1)
                    worldDragSpeed = 0;
                speed = worldDragSpeed;
                if (movementType == MovementType.AutoAlignment)
                {
                    //惯性速度和自动定位速度插值
                    var autoSpeed = CalcAutoMovingSpeed(currentIndex);
                    inertiaLerp += deltaTime * 1.5f;
                    speed = Mathf.Lerp(speed, autoSpeed, Mathf.Clamp(inertiaLerp, 0, 1));
                }
            }
            if (speed != 0)
            {
                var delta = speed * deltaTime;
                MoveVirtualContent(delta);
                var newIndex = virtualContent.CalcLocationIndex();
                var newDis = GetDistanceToCenterFrom(newIndex);
                void adjust()
                {
                    if (isDragging || movementType != MovementType.AutoAlignment) return;
                    //预测下一帧速度
                    var nextWorldDragSpeed = worldDragSpeed;
                    if (Mathf.Abs(nextWorldDragSpeed) > 0)
                    {
                        nextWorldDragSpeed *= Mathf.Pow(inertiaDecelerationRate, deltaTime);
                        if (Mathf.Abs(nextWorldDragSpeed) < 1)
                            nextWorldDragSpeed = 0;
                    }
                    var nextSpeed = Mathf.Lerp(nextWorldDragSpeed, CalcAutoMovingSpeed(currentIndex), Mathf.Clamp(inertiaLerp + deltaTime * 1.5f, 0, 1));
                    if (nextSpeed * speed <= 0)
                        MoveVirtualContent(-newDis);
                }
                if (newIndex != currentIndex)
                {
                    if (newDis * delta >= 0)    //定位索引改变时，位移与新索引距离同向，说明已越过或到达新物体中心
                    {
                        currMovingCompletedIndex = newIndex;
                        adjust();
                    }
                    else if (Mathf.Abs(newDis) <= endMovingDelta)
                    {
                        currMovingCompletedIndex = newIndex;
                    }
                    else if (Mathf.Abs(prevDis) <= endMovingDelta)
                    {
                        currMovingCompletedIndex = currentIndex;
                    }
                    else
                    {
                        //位移与新索引距离反向，说明没有达到新索引物体中心，检查是否越过了前一个物体中心
                        var prevIndex = (newIndex - currentIndex > 0) ? (newIndex - 1) : (newIndex + 1);
                        if (prevIndex >= 0 && prevIndex < itemCount)
                        {
                            var dis = GetDistanceToCenterFrom(prevIndex);
                            if (Mathf.Abs(dis) <= Mathf.Abs(delta))
                                currMovingCompletedIndex = prevIndex;
                        }
                    }
                    currentIndex = newIndex;
                }
                else
                {
                    //定位索引未改变，则仅当前后距离异号或其中一个为0时，物体越过或到达视口中心
                    if (newDis * prevDis <= 0 || Mathf.Abs(newDis) <= endMovingDelta || Mathf.Abs(prevDis) <= endMovingDelta)
                    {
                        currMovingCompletedIndex = newIndex;
                        adjust();
                    }
                }
            }
        }

        //更新itemList
        UpdateVisibleItemIndexList(out var vl, out _);
        if (visibleItemIndexList.Count > 0)
        {
            itemCache.Clear();
            foreach (var item in itemList)
            {
                if (visibleItemIndexList.Contains(item.index))
                    itemCache[item.index] = item;
                else
                    PutItemToPool(item);
            }
            itemList.Clear();
            ListItem it;
            bool isChanged;
            foreach (int i in visibleItemIndexList)
            {
                isChanged = false;
                if (itemCache.ContainsKey(i))
                {
                    it = itemCache[i];
                }
                else
                {
                    it = GetItemFromPool(GetItemName(i));
                    it.index = i;
                    isChanged = true;
                }
                var dis = GetNormalizedDistanceToCenterFrom(i);
                if (Mathf.Abs(it.normalizedDistance - dis) >= distanceMinDelta)
                {
                    isChanged = true;
                    it.normalizedDistance = dis;
                }
                if (isChanged)
                    _ExcSetDate(it.gameObject, it.index, it.normalizedDistance);
                itemList.Add(it);
                if (!it.gameObject.activeSelf)
                    it.gameObject.SetActive(true);
                it.gameObject.transform.SetAsLastSibling();
            }
            UpdateContent(vl);
        }

        foreach (var kv in pool)
        {
            var cnt = kv.Value.Count;
            for (int i = 0; i < cnt; i++)
            {
                var obj = kv.Value[i].gameObject;
                if (obj.activeSelf)
                    obj.SetActive(false);
            }
        }

        CheckedOnMovingCompleted();
        
        //自动定位时，normalizedPosition会不断有微小变化
        //为避免不必要的回调，只有当其变化量换算为世界坐标不小于1时才触发回调
        var currNormalizedPos = normalizedPosition;
        var normalizedTotal = virtualContent.Length - viewWorldHigh + viewWorldLow;
        if (Mathf.Abs((currNormalizedPos - prevNormalizedPos) * normalizedTotal) >= (vertical ? content.lossyScale.y : content.lossyScale.x))
        {
            OnValueChanged?.Invoke(currNormalizedPos);
            prevNormalizedPos = currNormalizedPos;
            Sync();
        }

        //更新ScrollBar
        if (scrollbar != null)
        {
            if (loop || scrollbarVisibility == ScrollbarVisibility.AutoHide && normalizedTotal <= 0)
            {
                scrollbar.gameObject.SetActive(false);
            }
            else
            {
                scrollbar.gameObject.SetActive(true);
                scrollbar.SetValueWithoutNotify(currNormalizedPos);
            }
        }
    }

    void _ExcSetDate(GameObject gobj,int index,float normalizedDistance){
        SetItemData?.Invoke(gobj,index,normalizedDistance);
    }

    override protected void OnDestroy()
    {
        this.GetItemName = null;
        this.OnItemCreated = null;
        this.SetItemData = null;
        this.OnMovingCompleted = null;
        this.OnIndexChanged = null;
        this.OnValueChanged = null;
        
        base.OnDestroy();
    }

    public void Init(LoopListView3.DF_GetItemName cfGetName,LoopListView3.DF_OnItemCreated cfCreate,LoopListView3.DF_SetItemData cfUpdate,LoopListView3.DF_OnIndexChanged cfMvEnd = null,LoopListView3.DF_OnValueChanged cfChgNormal = null)
    {
        this.GetItemName = cfGetName;
        this.OnItemCreated = cfCreate;
        this.SetItemData = cfUpdate;
        this.OnMovingCompleted = cfMvEnd;
        this.OnValueChanged = cfChgNormal;
    }

    public void SetScale(GameObject gobj,float val,string childNode = null)
    {
        if(!gobj || val <= 0)
            return;
        
        Transform _trsf = gobj.transform;
        if(!string.IsNullOrEmpty(childNode)){
            if(!"/".Equals(childNode))
                _trsf = _trsf.Find(childNode);
        }
        if(!_trsf)
            return;
        _trsf.localScale = new Vector3(val,val,1);
    }

    public void SetAlpha4Arrs(GameObject gobj,float val,params string[] cnes)
    {
        if(!gobj || val < 0 || val > 1)
            return;
        
        Transform _trsf = gobj.transform;
        CanvasGroup _grp = null;
        if(cnes == null || cnes.Length <= 0)
        {
            _grp = _trsf.GetComponent<CanvasGroup>();
            if(!_grp)
                _grp = _trsf.gameObject.AddComponent<CanvasGroup>();
            _grp.alpha = val;
            return;
        }

        string _str = null;
        Transform _trsfIt = null;
        for (int i = 0; i < cnes.Length; i++)
        {
            _str = cnes[i];
            if(string.IsNullOrEmpty(_str))
                continue;
            if("/".Equals(_str))
                _trsfIt = _trsf;
            else
                _trsfIt = _trsf.Find(_str);
            
            if(!_trsfIt)
                continue;
            
            _grp = _trsfIt.GetComponent<CanvasGroup>();
            if(!_grp)
                _grp = _trsfIt.gameObject.AddComponent<CanvasGroup>();
            _grp.alpha = val;
        }
    }

    public void SetAlpha(GameObject gobj,float val,string cn1 = null,string cn2 = null,string cn3 = null,string cn4 = null,string cn5 = null,string cn6 = null)
    {
        this.SetAlpha4Arrs(gobj,val,cn1,cn2,cn3,cn4,cn5,cn5);
    }
}
