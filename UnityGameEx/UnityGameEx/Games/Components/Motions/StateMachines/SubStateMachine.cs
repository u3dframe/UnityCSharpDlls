using UnityEngine;
using Core.Kernel;

/// <summary>
/// 类名 : Animator Sub State Machine
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2019-08-22 08:33
/// 功能 : 
/// </summary>
public class SubStateMachine : BasicStateMachine
{
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
        string _key = this.m_isUseGID4MsgKey ? string.Format("[{0}]_[{1}]",MsgConst.Msg_OnSubSMEnter,animator.gameObject.GetInstanceID()) : MsgConst.Msg_OnSubSMEnter;
        Messenger.Brocast<Animator,int>(_key,animator,stateMachinePathHash);
    }

    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash){
        string _key = this.m_isUseGID4MsgKey ? string.Format("[{0}]_[{1}]",MsgConst.Msg_OnSubSMExit,animator.gameObject.GetInstanceID()) : MsgConst.Msg_OnSubSMExit;
        Messenger.Brocast<Animator,int>(_key,animator,stateMachinePathHash);
    }
}
