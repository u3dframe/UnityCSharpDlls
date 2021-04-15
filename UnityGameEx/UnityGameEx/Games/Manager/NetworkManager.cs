using UnityEngine;
using System;
using System.Collections.Generic;

namespace TNet
{
    public class NetworkManager : GobjLifeListener
    {
        static NetworkManager _instance;
        static public NetworkManager instance
        {
            get
            {
                if (IsNull(_instance))
                {
                    GameObject _gobj = GameMgr.mgrGobj;
                    _instance = UtilityHelper.Get<NetworkManager>(_gobj, true);
                }
                return _instance;
            }
        }

        static readonly object m_lockObject = new object();
        static public bool mIsUseQueue { get; set; }
        static Queue<KeyValuePair<int, ByteBuffer>> mEvents = new Queue<KeyValuePair<int, ByteBuffer>>();
        static List<KeyValuePair<int, ByteBuffer>> mListEvents = new List<KeyValuePair<int, ByteBuffer>>();

        public SocketClient socket { get; private set; }
        string _lua_func = "Network.OnSocket";
        public string lua_func { get { return _lua_func; } private set { this._lua_func = value; } }
        String m_host = null;
        int m_port = 0;

        /// <summary>
        ///  初始化
        /// </summary>
        override protected void OnCall4Awake()
        {
            InitSocket();
            this.csAlias = "NetMgr";
            m_isOnUpdate = true;
            GameMgr.RegisterUpdate(this);
        }

        /// <summary>
        ///  更新 - 接受到数据
        /// </summary>
        override public void OnUpdate(float dt, float unscaledDt)
        {
            this.OnCF4UpQueue();
            this.OnCF4UpList();
        }

        /// <summary>
        /// 销毁
        /// </summary>
        override protected void OnCall4Destroy()
        {
            GameMgr.DiscardUpdate(this);
            socket.OnRemove();
        }

        override protected void OnClear()
        {
            mEvents.Clear();
            mListEvents.Clear();
            socket = null;
        }

        /// <summary>
        /// 通知到lua那边
        /// </summary>
        void OnCF2Lua(int code, ByteBuffer data)
        {
            Core.Kernel.Messenger.Brocast<string, int, ByteBuffer>("OnCF2Lua", lua_func, code, data);
        }

        void _OnCF4KV(KeyValuePair<int, ByteBuffer> _event)
        {
            // 通知到lua那边
            OnCF2Lua(_event.Key, _event.Value);
            // 放入对象池				
            ByteBuffer.ReBack(_event.Value);
        }

        void OnCF4UpQueue()
        {
            if (mEvents.Count > 0)
            {
                while (mEvents.Count > 0)
                {
                    KeyValuePair<int, ByteBuffer> _event = mEvents.Dequeue();
                    this._OnCF4KV(_event);
                }
            }
        }

        void OnCF4UpList()
        {
            if (mListEvents.Count > 0)
            {
                List<KeyValuePair<int, ByteBuffer>> _list = new List<KeyValuePair<int, ByteBuffer>>(mListEvents);
                int _lens = _list.Count;
                for (int i = 0; i < _lens; i++)
                {
                    KeyValuePair<int, ByteBuffer> _event = _list[i];
                    mListEvents.Remove(_event);

                    this._OnCF4KV(_event);
                }
            }
        }

        public NetworkManager InitNet(string host, int port, string luaFunc)
        {
            if (!string.IsNullOrEmpty(host))
            {
                this.m_host = host;
            }
            if (port > 0)
            {
                this.m_port = port;
            }
            if (!string.IsNullOrEmpty(luaFunc))
                this.lua_func = luaFunc;
            return this;
        }

        private void InitSocket()
        {
            if (socket != null) return;
            socket = new SocketClient();
            socket.OnRegister();
        }

        ///------------------------------------------------------------------------------------
        public static void AddEvent(int code, ByteBuffer data)
        {
            lock (m_lockObject)
            {
                if (mIsUseQueue)
                    mEvents.Enqueue(new KeyValuePair<int, ByteBuffer>(code, data));
                else
                    mListEvents.Add(new KeyValuePair<int, ByteBuffer>(code, data));
            }
        }

        public bool ShutDown()
        {
            if (this.socket == null)
                return false;

            return this.socket.Close();
        }

        public void Connect(string host, int port, bool isReConnect)
        {
            InitSocket();
            if (!isReConnect)
                isReConnect = !string.Equals(this.m_host, host) || this.m_port != port || this.socket.IsEmptyClient();
            if (isReConnect) ShutDown();
            this.InitNet(host, port, this.lua_func);
            this.socket.SendConnect(this.m_host, this.m_port);
        }

        /// <summary>
        /// 发送SOCKET消息
        /// </summary>
        public void SendMessage(ByteBuffer buffer)
        {
            socket.SendMessage(buffer);
        }

        public void SendBytes(byte[] msg)
        {
            socket.SendMessage(msg);
        }
    }
}
