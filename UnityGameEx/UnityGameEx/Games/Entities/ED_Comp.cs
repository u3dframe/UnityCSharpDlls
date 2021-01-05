using UnityEngine;
using System;
using System.Collections.Generic;
using UObject = UnityEngine.Object;

namespace Core.Kernel.Beans
{
    /// <summary>
    /// 类名 : 组件数据脚本
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2020-12-15 14:05
    /// 功能 : 
    /// </summary>
    public class ED_Comp : ED_Basic
    {
        static Dictionary<Type, Queue<ED_Comp>> m_caches = new Dictionary<Type, Queue<ED_Comp>>();
        static T GetCache<T>() where T : ED_Comp
        {
            Type _tp = typeof(T);
            Queue<ED_Comp> _que = null;
            if(!m_caches.TryGetValue(_tp,out _que))
            {
                _que = new Queue<ED_Comp>();
                m_caches.Add(_tp, _que);
            }
            if (_que.Count <= 0)
                return null;
            return (T)_que.Dequeue();
        }

        static void AddCache(ED_Comp entity)
        {
            if (entity == null || GHelper.Is_App_Quit)
                return;

            Type _tp = entity.GetType();
            Queue<ED_Comp> _que = null;
            if (!m_caches.TryGetValue(_tp, out _que))
            {
                _que = new Queue<ED_Comp>();
                m_caches.Add(_tp, _que);
            }
            _que.Enqueue(entity);
        }

        static public T Builder<T>(UObject uobj) where T : ED_Comp,new()
        {
            GameObject _go = GHelper.ToGObj(uobj);
            if (_go == null || !_go)
                return null;
            T ret = GetCache<T>();
            if (ret == null)
                ret = new T();
            ret.InitGobj(_go);
            return ret;
        }

        static public ED_Comp Builder(UObject uobj)
        {
            return Builder<ED_Comp>(uobj);
        }

        static public ED_Comp BuilderComp(UObject uobj, Component comp, Action cfDestroy, Action cfShow, Action cfHide)
        {
            ED_Comp _comp = Builder(uobj);            
            if (_comp == null)
                return null;
            _comp.InitComp(comp, cfDestroy, cfShow, cfHide);
            return _comp;
        }

        static public ED_Comp BuilderComp(UObject uobj, string comp, Action cfDestroy, Action cfShow, Action cfHide)
        {
            ED_Comp _comp = Builder(uobj);
            if (_comp == null)
                return null;
            _comp.InitComp(comp, cfDestroy, cfShow, cfHide);
            return _comp;
        }

        public string m_g_name { get; private set; }
        public GameObject m_gobj { get; private set; }
        public int m_gobjID { get; private set; }
        public Transform m_trsf { get; private set; }
        public RectTransform m_trsfRect { get; private set; }
        public Transform m_parent { get { return this.m_trsf?.parent; } }
        public GameObject m_parentGobj { get { return this.m_parent?.gameObject; } }
        public bool m_isActiveInView { get { return this.m_gobj && this.m_gobj.activeInHierarchy; } }
        public Component m_comp { get; private set; }
        public Behaviour m_behav { get; private set; }
        public string m_strComp { get; private set; }
        public GobjLifeListener m_compGLife { get; private set; }
        Action m_cfShow = null, m_cfHide = null, m_cfDestroy = null;
        protected Vector3 m_startPos = Vector3.zero;
        protected Vector3 m_startLocPos = Vector3.zero;

        public ED_Cavs m_edCvs { get; private set; }

        public ED_Comp()
        {
        }

        protected void InitGobj(GameObject gobj)
        {
            if (!gobj)
                throw new Exception("=== gobj is null");

            this.m_gobj = gobj;
            this.m_g_name = gobj.name;
            this.m_gobjID = this.m_gobj.GetInstanceID();
            this.m_trsf = this.m_gobj.transform;
            this.m_trsfRect = this.m_trsf as RectTransform;
            this.m_startPos = this.GetCurrPos(false);
            this.m_startLocPos = this.GetCurrPos();
        }

        public void InitCallFunc(Action cfDestroy, Action cfShow, Action cfHide)
        {
            this.m_cfDestroy = cfDestroy;
            this.m_cfShow = cfShow;
            this.m_cfHide = cfHide;
        }

        virtual public void InitComp(Component comp, Action cfDestroy, Action cfShow, Action cfHide)
        {
            this.InitCallFunc(cfDestroy, cfShow, cfHide);
            this.m_comp = comp;
            this.m_behav = comp as Behaviour;
            if (string.IsNullOrEmpty(this.m_strComp) && comp != null)
                this.m_strComp = comp.ToString();
            bool _isGLife = GHelper.IsGLife(comp);
            if (_isGLife)
                this.m_compGLife = comp as GobjLifeListener;
            else
                this.m_compGLife = GobjLifeListener.Get(this.m_gobj);
            this.m_compGLife.OnlyOnceCallDetroy(On_Destroy);
            this.m_compGLife.OnlyOnceCallShow(On_Show);
            this.m_compGLife.OnlyOnceCallHide(On_Hide);

            this.m_edCvs = ED_Cavs.Builder(this.m_gobj);
        }

        virtual public void InitComp(string strComp, Action cfDestroy, Action cfShow, Action cfHide)
        {
            this.m_strComp = strComp;
            Component comp = this.m_gobj.GetComponent(strComp);
            this.InitComp(comp, cfDestroy, cfShow, cfHide);
        }

        virtual public void ClearComp()
        {
            this.StopAllUpdate();
            this.m_gobj = null;
            this.m_trsf = null;
            this.m_trsfRect = null;
            this.m_comp = null;
            this.m_behav = null;
            this.m_strComp = null;
            this.m_compGLife = null;

            ED_Cavs _e_ = this.m_edCvs;
            this.m_edCvs = null;
            if (_e_ != null)
                _e_.ClearComp();

            this.m_cfDestroy = null;
            this.m_cfShow = null;
            this.m_cfHide = null;
            this.m_cfUpdate = null;
            this.m_cfEndUpdate = null;
        }

        public Vector3 GetCurrPos(bool isLocal = true)
        {
            return isLocal ? this.m_trsf.localPosition : this.m_trsf.position;
        }

        public void SetCurrPos(Vector3 pos, bool isLocal = true)
        {
            if (isLocal)
                this.m_trsf.localPosition = pos;
            else
                this.m_trsf.position = pos;
        }

        public Component GetComponent(string cType)
        {
            return this.m_gobj.GetComponent(cType);
        }

        public Component GetComponent(Type cType)
        {
            return this.m_gobj.GetComponent(cType);
        }

        public void SetActive(bool isActive)
        {
            this.m_gobj.SetActive(isActive);
        }

        public void SetGName(string gname)
        {
            if (string.IsNullOrEmpty(gname) || this.m_g_name.Equals(gname))
                return;
            this.m_g_name = gname;
            this.m_gobj.name = gname;
        }

        public void SetLayer(string layer, bool isAll)
        {
            GHelper.SetLayerBy(this.m_gobj, layer, isAll);
        }

        public void SetLayer(int layer, bool isAll)
        {
            GHelper.SetLayerBy(this.m_gobj, layer, isAll);
        }

        public GameObject Clone(object parent)
        {
            if (parent != null)
            {
                Transform _p = null;
                if (parent is Transform)
                {
                    _p = (Transform)parent;
                }
                else if (parent is GameObject)
                {
                    _p = ((GameObject)parent).transform;
                }
                return GHelper.Clone(this.m_gobj, _p);
            }
            return GHelper.Clone(this.m_gobj);
        }

        public bool DestroyObj(bool isImmediate)
        {
            GameObject _gobj = this.m_gobj;
            bool _isBl = _gobj != null && !!_gobj;
            if (_isBl)
            {
                if (isImmediate)
                    GameObject.DestroyImmediate(_gobj);
                else
                    GameObject.Destroy(_gobj);
            }
            return _isBl;
        }

        public void DonotDestory()
        {
            GameObject.DontDestroyOnLoad(this.m_gobj);
        }

        public int m_childCount { get { if (this.m_trsf) return this.m_trsf.childCount; return 0; } }

        public Transform GetChild(int index)
        {
            int _lens = this.m_childCount;
            if (index >= 0 && index < _lens)
                return this.m_trsf.GetChild(index);
            return null;
        }

        public Vector3 GetPosition()
        {
            return this.GetCurrPos(false);
        }

        public void SetPosition(float x, float y, float z)
        {
            this.m_trsf.position = new Vector3(x, y, z);
        }

        public void SetLocalPosition(float x, float y, float z)
        {
            this.m_trsf.localPosition = new Vector3(x, y, z);
        }

        public void SetLocalScale(float x, float y, float z)
        {
            this.m_trsf.localScale = new Vector3(x, y, z);
        }

        public Vector3 GetEulerAngles()
        {
            return this.m_trsf.eulerAngles;
        }

        public void SetEulerAngles(float x, float y, float z)
        {
            this.m_trsf.eulerAngles = new Vector3(x, y, z);
        }

        public void SetLocalEulerAngles(float x, float y, float z)
        {
            this.m_trsf.localEulerAngles = new Vector3(x, y, z);
        }

        public Vector3 GetForward()
        {
            return this.m_trsf.forward;
        }

        public void SetForward(float x, float y, float z)
        {
            this.m_trsf.forward = new Vector3(x, y, z);
        }

        public Vector2 GetAnchoredPosition()
        {
            if (this.m_trsfRect)
                return this.m_trsfRect.anchoredPosition;
            return Vector2.zero;
        }

        public void SetAnchoredPosition(float x, float y)
        {
            if (this.m_trsfRect)
                this.m_trsfRect.anchoredPosition = new Vector2(x, y);
        }

        public void SetAnchoredPosition3D(float x, float y, float z)
        {
            if (this.m_trsfRect)
                this.m_trsfRect.anchoredPosition3D = new Vector3(x, y, z);
        }

        public void SetAnchorMin(float x, float y)
        {
            if (this.m_trsfRect)
                this.m_trsfRect.anchorMin = new Vector2(x, y);
        }

        public void SetAnchorMax(float x, float y)
        {
            if (this.m_trsfRect)
                this.m_trsfRect.anchorMax = new Vector2(x, y);
        }

        public Vector2 GetPivot()
        {
            if (this.m_trsfRect)
                return this.m_trsfRect.pivot;
            return Vector2.zero;
        }

        public void SetPivot(float x, float y)
        {
            if (this.m_trsfRect)
                this.m_trsfRect.pivot = new Vector2(x, y);
        }

        public void GetRectSize(ref float w, ref float h)
        {
            GHelper.GetRectSize(this.m_trsf, ref w, ref h);
        }

        public void SetSizeDelta(float x, float y)
        {
            if (this.m_trsfRect)
                this.m_trsfRect.sizeDelta = new Vector2(x, y);
        }

        public void SetParent(Transform parent, bool isLocal, bool isSyncLayer)
        {
            if (isSyncLayer)
                GHelper.SetParentSyncLayer(this.m_trsf, parent, isLocal);
            else
                GHelper.SetParent(this.m_trsf, parent, isLocal);
        }

        public void SetParent(GameObject parent, bool isLocal, bool isSyncLayer)
        {
            if (isSyncLayer)
                GHelper.SetParentSyncLayer(this.m_gobj, parent, isLocal);
            else
                GHelper.SetParent(this.m_gobj, parent, isLocal);
        }

        public void LookAt(float x, float y, float z)
        {
            Vector3 _v3 = new Vector3(x, y, z);
            this.m_trsf.LookAt(_v3);
        }

        public void Translate(float x, float y, float z, Space space)
        {
            this.m_trsf.Translate(x, y, z, space);
        }

        public void TranslateWorld(float x, float y, float z)
        {
            this.Translate(x, y, z, Space.World);
        }

        public void TranslateSelf(float x, float y, float z)
        {
            this.Translate(x, y, z, Space.Self);
        }

        public void AddLocalPos(float x, float y, float z)
        {
            Vector3 _v3 = this.m_trsf.localPosition;
            _v3 += new Vector3(x, y, z);
            this.m_trsf.localPosition = _v3;
        }

        public int GetSiblingIndex()
        {
            return this.m_trsf.GetSiblingIndex();
        }

        public void SetSiblingIndex(int bIndex)
        {
            this.m_trsf.SetSiblingIndex(bIndex);
        }

        public void SetAsFirstSibling()
        {
            this.m_trsf.SetAsFirstSibling();
        }

        public void SetAsLastSibling()
        {
            this.m_trsf.SetAsLastSibling();
        }

        public Transform Find(string childName)
        {
            if (string.IsNullOrEmpty(childName))
                return null;
            return this.m_trsf.Find(childName);
        }

        public GameObject FindGobj(string childName)
        {
            Transform _ret = this.Find(childName);
            return _ret?.gameObject;
        }

        virtual protected void On_Destroy(GobjLifeListener obj)
        {
            var _call = this.m_cfDestroy;
            this.ClearComp();
            if (_call != null)
                _call();

            AddCache(this);
        }

        virtual protected void On_Show()
        {
            if (this.m_cfShow != null)
                this.m_cfShow();
        }

        virtual protected void On_Hide()
        {
            if (this.m_cfHide != null)
                this.m_cfHide();
        }

        public void SetEnabled(bool isBl)
        {
            if (this.m_behav)
                this.m_behav.enabled = isBl;
        }

        public bool m_isUpByLate { get; set; }
        protected bool m_isSmoothPos { get; private set; }
        private int m_upPosState = 0;
        protected float m_jugdePosDis = 0.0025f;
        private Vector3 m_curPos = Vector3.zero;
        private Vector3 m_toPos = Vector3.zero;
        private Vector3 m_diffPos = Vector3.zero;
        private float m_currentVelocity = 0.0F;
        private float m_smoothTime = 0.1F;
        protected Action m_cfUpdate = null;
        private Action m_cfEndUpdate = null;

        public void StartCurrUpdate()
        {
            this.StopAllUpdate();
            if (this.m_isUpByLate)
                this.StartLateUpdate();
            else
                this.StartUpdate();
        }

        override public void OnLateUpdate()
        {
            base.OnLateUpdate();
            if (!m_isUpByLate) return;
            _On_Update();
        }

        override public void OnUpdate(float dt, float unscaledDt)
        {
            base.OnUpdate(dt, unscaledDt);
            if (m_isUpByLate) return;
            _On_Update();
        }

        void _On_Update()
        {
            if(this.m_cfUpdate != null)
                this.m_cfUpdate();
            this._OnCurrUpdate();
        }

        virtual protected void _OnCurrUpdate()
        {
            if(this.m_cfUpdate != null)
            {
                if(this.m_upPosState != 0)
                {
                    this.SetCurrPos();

                    this.m_diffPos = this.m_toPos - this.m_curPos;
                    if (this.m_diffPos.sqrMagnitude < this.m_jugdePosDis)
                    {
                        this.m_cfUpdate -= _SmoothMoveX;
                        this.m_cfUpdate -= _SmoothMoveY;
                        this.m_cfUpdate -= _SmoothMoveZ;
                        this.m_upPosState = 0;
                    }
                }
            }

            if (this.m_cfUpdate == null)
            {
                this.StopAllUpdate();
                this.ExcuteCFUpdateEnd();
            }
        }

        protected void SetCurrPos()
        {
            bool isLocal = this.m_upPosState == 1;
            this.SetCurrPos(this.m_curPos, isLocal);
        }

        protected void ExcuteCFUpdateEnd()
        {
            var _call = this.m_cfEndUpdate;
            this.m_cfEndUpdate = null;
            if (_call != null)
                _call();
        }

        virtual protected void _SmoothMoveX()
        {
            this.m_curPos.x = Mathf.SmoothDamp(this.m_curPos.x, this.m_toPos.x, ref m_currentVelocity, m_smoothTime);
        }

        virtual protected void _SmoothMoveY()
        {
            this.m_curPos.y = Mathf.SmoothDamp(this.m_curPos.y, this.m_toPos.y, ref m_currentVelocity, m_smoothTime);
        }

        virtual protected void _SmoothMoveZ()
        {
            this.m_curPos.z = Mathf.SmoothDamp(this.m_curPos.z, this.m_toPos.z, ref m_currentVelocity, m_smoothTime);
        }

        protected bool IsChgSmoothPos(float toX, float toY, float toZ)
        {
            Vector3 _toV3 = new Vector3(toX, toY, toZ);
            _toV3 = this.m_toPos - _toV3;
            if (_toV3.sqrMagnitude <= m_jugdePosDis)
                return false;
            return true;
        }
        
        public bool IsSmoothPos(float toX, float toY, float toZ, bool isLocal, float smoothTime = 0f, Action callFinish = null)
        {
            if (!this.IsChgSmoothPos(toX, toY, toZ))
                return this.m_isSmoothPos;

            this.StopAllUpdate();
            this.m_cfUpdate = null;
            this.m_upPosState = isLocal ? 1 : 2;
            this.m_cfEndUpdate = callFinish;
            this.m_smoothTime = smoothTime;
            this.m_toPos.x = toX;
            this.m_toPos.y = toY;
            this.m_toPos.z = toZ;
            this.m_curPos = GetCurrPos(isLocal);

            this.m_diffPos = this.m_toPos - this.m_curPos;
            this.m_isSmoothPos = (smoothTime > 0) && (m_diffPos.sqrMagnitude > this.m_jugdePosDis);
            if (this.m_isSmoothPos)
            {
                if (this.m_curPos.x != toX)
                    this.m_cfUpdate += _SmoothMoveX;

                if (this.m_curPos.y != toY)
                    this.m_cfUpdate += _SmoothMoveY;

                if (this.m_curPos.z != toZ)
                    this.m_cfUpdate += _SmoothMoveZ;
            }
            else
            {
                this.m_curPos.x = toX;
                this.m_curPos.y = toY;
                this.m_curPos.z = toZ;
                this.SetCurrPos();
            }
            return this.m_isSmoothPos;
        }

        public void ToSmoothPos(float toX, float toY, float toZ, bool isLocal, float smoothTime = 0f, Action callFinish = null)
        {
            bool _isSmoonth = this.IsSmoothPos(toX, toY, toZ, isLocal, smoothTime, callFinish);
            if (_isSmoonth)
                this.StartCurrUpdate();
            else
                this.ExcuteCFUpdateEnd();
        }

        public void ReCavsSort(bool isBack)
        {
            if (this.m_edCvs != null)
                this.m_edCvs.AutoSortOrder(isBack);
        }

        public void SetCavsSort(int sortOrder)
        {
            if (this.m_edCvs != null)
                this.m_edCvs.SetSortOrder(sortOrder);
        }
    }
}
