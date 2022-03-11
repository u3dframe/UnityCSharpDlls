using System;
using UnityEngine;
using System.Collections.Generic;
namespace Core.Kernel.Beans
{
    /// <summary>
    /// 类名 : 数据对象基础类
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2017-01-05 09:53
    /// 功能 : 
    /// 修订 : 2020-12-14 17:51 
    /// </summary>
    [Serializable]
    public class ED_Basic : CustomYieldInstruction, IUpdate, ILateUpdate
    {
        static public int maxCache = 500;
        static Dictionary<Type, Queue<ED_Basic>> m_caches = new Dictionary<Type, Queue<ED_Basic>>();
        static protected T GetCache<T>() where T : ED_Basic
        {
            Type _tp = typeof(T);
            Queue<ED_Basic> _que = null;
            if (!m_caches.TryGetValue(_tp, out _que))
            {
                _que = new Queue<ED_Basic>();
                m_caches.Add(_tp, _que);
            }
            if (_que.Count <= 0)
                return null;
            return (T)_que.Dequeue();
        }

        static protected void AddCache(ED_Basic entity)
        {
            if (entity == null || GHelper.Is_App_Quit)
                return;

            entity.StopAllUpdate();

            Type _tp = entity.GetType();
            Queue<ED_Basic> _que = null;
            if (!m_caches.TryGetValue(_tp, out _que))
            {
                _que = new Queue<ED_Basic>();
                m_caches.Add(_tp, _que);
            }
            int _size = _que.Count;
            if (_size >= maxCache)
                return;
            if (!_que.Contains(entity))
                _que.Enqueue(entity);
        }
        /*
        static public T GetOrNew<T>() where T : ED_Basic, new()
        {
            T ret = GetCache<T>();
            if (ret == null)
                ret = new T();
            return ret;
        }
        */

        protected bool m_isInUp = false, m_isInLateUp = false;

        public override bool keepWaiting
        {
            get
            {
                return false;
            }
        }

        public bool m_isOnUpdate { get; set; }
        public bool IsOnUpdate() { return this.m_isOnUpdate; }
        virtual public void OnUpdate(float dt, float unscaledDt) { }

        protected void RegUpdate(bool isUp)
        {
            this.m_isOnUpdate = false;
            if (m_isInUp)
            {
                m_isInUp = false;
                GameMgr.DiscardUpdate(this);
            }
            if (isUp && !m_isInUp)
            {
                GameMgr.RegisterUpdate(this);
                m_isInUp = true;
            }
            this.m_isOnUpdate = isUp;
        }

        public ED_Basic StartUpdate()
        {
            if (!this.m_isInUp)
                this.RegUpdate(true);
            this.m_isOnUpdate = true;
            return this;
        }

        public void StopUpdate()
        {
            this.RegUpdate(false);
        }

        public bool m_isOnLateUpdate { get; set; }
        public bool IsOnLateUpdate() { return this.m_isOnLateUpdate; }
        virtual public void OnLateUpdate() { }

        protected void RegLateUpdate(bool isUp)
        {
            this.m_isOnLateUpdate = false;
            if (m_isInLateUp)
            {
                m_isInLateUp = false;
                GameMgr.DiscardLateUpdate(this);
            }
            if (isUp && !m_isInLateUp)
            {
                GameMgr.RegisterLateUpdate(this);
                m_isInLateUp = true;
            }
            this.m_isOnLateUpdate = isUp;
        }

        public ED_Basic StartLateUpdate()
        {
            if (!this.m_isInLateUp)
                this.RegLateUpdate(true);
            this.m_isOnLateUpdate = true;
            return this;
        }

        public void StopLateUpdate()
        {
            this.RegLateUpdate(false);
        }

        public void StopAllUpdate()
        {
            this.StopUpdate();
            this.StopLateUpdate();
        }
    }
}
