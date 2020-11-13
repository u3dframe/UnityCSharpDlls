namespace TNet {
    public class Protocal {
        ///BUILD TABLE
        public const int Connect = 1001; //连接服务器
        public const int ConnectFail = 1006; // 链接失败
        public const int Exception = 1002; //异常掉线
        public const int Disconnect = 1003; //正常断线
        public const int Message = 1004; //返回的消息
        public const int Write = 1005; // 消息投递完成
    }
}