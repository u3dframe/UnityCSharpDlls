namespace Core.Kernel {
    /// <summary>
    /// 类名 : 消息 常用常量
    /// 作者 : canyon / 龚阳辉
    /// 日期 : 2018-08-22 08:57
    /// 功能 : 
    /// </summary>
    public class MsgConst{        
        public const string Msg_OnSubSMEnter = "Msg_OnSubSMEnter"; // Sub State Machine Entrer
        public const string Msg_OnSubSMExit = "Msg_OnSubSMExit"; // Sub State Machine Exit
        public const string Msg_OnSMEnter = "Msg_OnSMEnter"; // State Machine Entrer
        public const string Msg_OnSMUpdate = "Msg_OnSMUpdate"; // State Machine Update
        public const string Msg_OnSMExit = "Msg_OnSMExit"; // State Machine Exit
        public const string Msg_OnSMMove = "Msg_OnSMMove"; // State Machine Move
        public const string Msg_OnSM_IK = "Msg_OnSM_IK"; // State Machine IK

        public const string MSound_Volume = "Msg_OnUp_Volume"; // Musice,Sound up Volume
        public const string MSound_State = "Msg_OnUp_MSState"; // Musice,Sound up Play,Stop

        public const string MSL_Cvs_Destroy = "Msg_MSL_CvsDestroy_{0}"; // 摄像机canvas销毁了
        public const string MSL_Cvs_ValChange = "Msg_MSL_CvsValChg_{0}"; // SortOrder Layer(curVal,cur - last)
        public const string MSL_Cvs_ChangeVal = "Msg_MSL_CvsChgVal_{0}"; // SortOrder Layer(isAdd,orderChangeValue)
    }
}