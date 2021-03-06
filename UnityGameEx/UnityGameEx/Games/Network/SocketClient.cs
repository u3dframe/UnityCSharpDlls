using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using TNet;

public enum DisType {
    Exception,
    Disconnect,
    ConnectFail,
}

public class SocketClient {
    private TcpClient client = null;
    private NetworkStream outStream = null;
    private MemoryStream memStream;
    private BinaryReader reader;

    private const int MAX_READ = 8192;
    private byte[] _bts = new byte[MAX_READ];

    // Use this for initialization
    public SocketClient() {
    }

    /// <summary>
    /// 注册代理
    /// </summary>
    public void OnRegister() {
        memStream = new MemoryStream();
        reader = new BinaryReader(memStream);
    }

    /// <summary>
    /// 移除代理
    /// </summary>
    public void OnRemove() {
        this.Close();
        reader.Close();
        memStream.Close();
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    void ConnectServer(string host, int port) {
        client = null;
        try {
            IPAddress[] address = Dns.GetHostAddresses(host);
            if (address.Length == 0) {
                OnDisconnected(DisType.ConnectFail,"host invalid");
                return;
            }
            if (address[0].AddressFamily == AddressFamily.InterNetworkV6) {
                client = new TcpClient(AddressFamily.InterNetworkV6);
            }
            else {
                client = new TcpClient(AddressFamily.InterNetwork);
            }
            client.SendTimeout = 1000;
            client.ReceiveTimeout = 1000;
            client.NoDelay = true;
            client.BeginConnect(host, port, new AsyncCallback(OnConnect), null);
        } catch (Exception ex) {
            OnDisconnected(DisType.ConnectFail, ex.Message);
        }
    }

    /// <summary>
    /// 连接上服务器
    /// </summary>
    void OnConnect(IAsyncResult asr) {
        try{
            client.EndConnect(asr);
            outStream = client.GetStream();
            client.GetStream().BeginRead(_bts, 0, MAX_READ, new AsyncCallback(OnRead), null);
            NetworkManager.AddEvent(Protocal.Connect, null);
        } catch (Exception ex) {
            OnDisconnected(DisType.ConnectFail, ex.Message);
        }
    }

    /// <summary>
    /// 写数据
    /// </summary>
    void WriteMessage(byte[] message) {
        MemoryStream ms = null;
        using (ms = new MemoryStream()) {
            ms.Position = 0;
            BinaryWriter writer = new BinaryWriter(ms);
            // ushort msglen = (ushort)message.Length;
            // writer.Write(Converter.GetBigEndian(msglen));
            writer.Write(message);
            writer.Flush();
            if (IsConnected()) {
                byte[] payload = ms.ToArray();
                outStream.BeginWrite(payload, 0, payload.Length, new AsyncCallback(OnWrite), null);
            } else {
                Debug.LogError("client.connected----->>false");
            }
        }
    }

    /// <summary>
    /// 读取消息
    /// </summary>
    void OnRead(IAsyncResult asr) {
        int bytesRead = 0;
        try {
            lock (client.GetStream()) {         //读取字节流到缓冲区
                bytesRead = client.GetStream().EndRead(asr);
            }
            if (bytesRead < 1) {                //包尺寸有问题，断线处理
                OnDisconnected(DisType.Disconnect, "bytesRead < 1");
                return;
            }
            OnReceive(_bts, bytesRead);   //分析数据包内容，抛给逻辑层
            lock (client.GetStream()) {         //分析完，再次监听服务器发过来的新消息
                Array.Clear(_bts, 0, _bts.Length);   //清空数组
                client.GetStream().BeginRead(_bts, 0, MAX_READ, new AsyncCallback(OnRead), null);
            }
        } catch (Exception ex) {
            OnDisconnected(DisType.Exception, ex.Message);
        }
    }

    /// <summary>
    /// 丢失链接
    /// </summary>
    void OnDisconnected(DisType dis, string msg) {
        Close();   //关掉客户端链接
        int protocal = Protocal.Disconnect;
        switch(dis){
            case DisType.Exception:
                protocal = Protocal.Exception;
            break;
            case DisType.ConnectFail:
                protocal = Protocal.ConnectFail;
            break;
        }
        ByteBuffer buffer = ByteBuffer.BuildWriter();
        buffer.WriteString(msg);
        NetworkManager.AddEvent(protocal, buffer.ToReader());
        //Debug.LogError("Connection was closed by the server:>" + msg + " Distype:>" + dis);
    }

    /// <summary>
    /// 打印字节
    /// </summary>
    /// <param name="bytes"></param>
    void PrintBytes() {
        string returnStr = string.Empty;
        for (int i = 0; i < _bts.Length; i++) {
            returnStr += _bts[i].ToString("X2");
        }
        Debug.LogError(returnStr);
    }

    /// <summary>
    /// 向链接写入数据流
    /// </summary>
    void OnWrite(IAsyncResult r) {
        try {
            outStream.EndWrite(r);
            NetworkManager.AddEvent(Protocal.Write, null);
        } catch (Exception) {
            // 其他非UI主线程不能使用Debug
            // Debug.LogError("OnWrite--->>>" + ex.Message);
        }
    }

    /// <summary>
    /// 接收到消息
    /// </summary>
    void OnReceive(byte[] bytes, int length) {
        ByteBuffer _bbf = ByteBuffer.BuildWriter();
        _bbf.WriteBytes(bytes, length);
        OnReceivedMessage(_bbf);
    }

    /// <summary>
    /// 剩余的字节
    /// </summary>
    private long RemainingBytes() {
        return memStream.Length - memStream.Position;
    }

    /// <summary>
    /// 接收到消息
    /// </summary>
    /// <param name="ms"></param>
    void OnReceivedMessage(ByteBuffer data) {
        ByteBuffer buffer = data.ToReader();
        NetworkManager.AddEvent(Protocal.Message, buffer);
    }

    /// <summary>
    /// 是否链接
    /// </summary>
    public bool IsConnected() {
        return (client != null && client.Connected);
    }

    public bool IsEmptyClient() {
        return (client == null);
    }

    /// <summary>
    /// 关闭链接
    /// </summary>
    public bool Close() {
        bool _isBl = IsConnected();
        if (_isBl)
          client.Close();

        client = null;
        return _isBl;
    }

    /// <summary>
    /// 发送连接请求
    /// </summary>
    public void SendConnect(String host,int port) {
        ConnectServer(host,port);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    public void SendMessage(ByteBuffer buffer) {
        WriteMessage(buffer.ToBytes());
        ByteBuffer.ReBack(buffer);
    }

    public void SendMessage(byte [] msg) {
        WriteMessage(msg);
    }
}
