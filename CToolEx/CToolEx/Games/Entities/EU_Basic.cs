using System;
using UnityEngine;
namespace Core.Kernel.Beans
{
    /// <summary>
    /// 类名 : MonoBehaviour 对象 基础类
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2017-01-05 09:55
    /// 功能 : 
    /// 修订 : 2020-12-14 18:03 
    /// </summary>
    /// [HideInInspector]
    [Serializable]
    public class EU_Basic : MonoBehaviour, IUpdate, ILateUpdate
    {
        public bool m_isOnUpdate { get; set; }
        public bool IsOnUpdate() { return this.m_isOnUpdate; }
        virtual public void OnUpdate(float dt, float unscaledDt) { }

        protected bool m_isInUp = false, m_isInLateUp = false;

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

        public EU_Basic StartUpdate()
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

        public EU_Basic StartLateUpdate()
        {
            if(!this.m_isInLateUp)
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
