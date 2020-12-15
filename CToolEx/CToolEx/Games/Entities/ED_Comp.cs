using UnityEngine;
using System;

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
        public string m_g_name { get; private set; }
        public GameObject m_gobj { get; private set; }
        public Transform m_trsf { get; private set; }
        public RectTransform m_trsfRect { get; private set; }
        public Transform m_parent { get { return this.m_trsf?.parent; } }
        public GameObject m_parentGobj { get { return this.m_parent?.gameObject; } }
        public bool m_isActiveInView { get { return this.m_gobj && this.m_gobj.activeInHierarchy; } }
        public Component m_comp { get; private set; }
        public string m_strComp { get; private set; }
        public GobjLifeListener m_compGLife { get; private set; }
        Action m_cfShow = null, m_cfHide = null, m_cfDestroy = null;

        public ED_Comp(GameObject gobj, Component comp, Action cfDestroy, Action cfShow, Action cfHide)
        {
            this.InitComp(gobj, comp, cfDestroy, cfShow, cfHide);
        }

        public ED_Comp(GameObject gobj, string comp, Action cfDestroy, Action cfShow, Action cfHide)
        {
            this.InitComp(gobj, comp, cfDestroy, cfShow, cfHide);
        }

        void InitGobj(GameObject gobj)
        {
            if (!gobj)
                throw new Exception("=== gobj is null");

            this.m_gobj = gobj;
            this.m_g_name = gobj.name;
            this.m_trsf = gobj.transform;
            this.m_trsfRect = this.m_trsf as RectTransform;
        }

        void InitCallFunc(Action cfDestroy, Action cfShow, Action cfHide)
        {
            this.m_cfDestroy = cfDestroy;
            this.m_cfShow = cfShow;
            this.m_cfHide = cfHide;
        }

        void InitComp(GameObject gobj, Component comp, Action cfDestroy, Action cfShow, Action cfHide)
        {
            this.InitGobj(gobj);
            this.InitCallFunc(cfDestroy, cfShow, cfHide);
            this.m_comp = comp;
            if (string.IsNullOrEmpty(this.m_strComp) && comp != null)
                this.m_strComp = comp.ToString();
            bool _isGLife = GHelper.IsGLife(comp);
            if (_isGLife)
                this.m_compGLife = comp as GobjLifeListener;
            else
                this.m_compGLife = GobjLifeListener.Get(this.m_gobj);

            this.m_compGLife.AddOnlyOnceDCall(On_Destroy);
            this.m_compGLife.AddOnlyOnceCallShow(On_Show);
            this.m_compGLife.AddOnlyOnceCallHide(On_Hide);
        }

        void InitComp(GameObject gobj, string strComp, Action cfDestroy, Action cfShow, Action cfHide)
        {
            if (!gobj)
                throw new Exception("=== gobj is null");
            Component comp = gobj.GetComponent(strComp);
            this.m_strComp = strComp;
            this.InitComp(gobj, comp, cfDestroy, cfShow, cfHide);
        }

        public void ClearComp()
        {
            this.m_gobj = null;
            this.m_trsf = null;
            this.m_trsfRect = null;
            this.m_comp = null;
            this.m_strComp = null;
            this.m_compGLife = null;

            this.m_cfDestroy = null;
            this.m_cfShow = null;
            this.m_cfHide = null;
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
            return this.m_trsf.position;
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

        void On_Destroy(GobjLifeListener obj)
        {
            var _call = this.m_cfDestroy;
            this.ClearComp();
            if (_call != null)
                _call();
        }

        void On_Show()
        {
            if (this.m_cfShow != null)
                this.m_cfShow();
        }

        void On_Hide()
        {
            if (this.m_cfHide != null)
                this.m_cfHide();
        }
    }
}
