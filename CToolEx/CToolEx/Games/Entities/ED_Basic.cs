using System;
using UnityEngine;
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
        public override bool keepWaiting
        {
            get
            {
                return false;
            }
        }

        public bool m_isOnUpdate = false;
        public bool IsOnUpdate() { return this.m_isOnUpdate; }
        virtual public void OnUpdate(float dt, float unscaledDt) { }

        protected void RegUpdate(bool isUp)
        {
            this.m_isOnUpdate = false;
            GameMgr.DiscardUpdate(this);
            if (isUp)
                GameMgr.RegisterUpdate(this);
            this.m_isOnUpdate = isUp;
        }

        public ED_Basic StartUpdate()
        {
            if (!this.m_isOnUpdate)
                this.RegUpdate(true);
            return this;
        }

        public void StopUpdate()
        {
            this.RegUpdate(false);
        }

        public bool m_isOnLateUpdate = false;
        public bool IsOnLateUpdate() { return this.m_isOnLateUpdate; }
        virtual public void OnLateUpdate() { }

        protected void RegLateUpdate(bool isUp)
        {
            this.m_isOnLateUpdate = false;
            GameMgr.DiscardLateUpdate(this);
            if (isUp)
                GameMgr.RegisterLateUpdate(this);
            this.m_isOnLateUpdate = isUp;
        }

        public ED_Basic StartLateUpdate()
        {
            if (!this.m_isOnLateUpdate)
                this.RegLateUpdate(true);
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
