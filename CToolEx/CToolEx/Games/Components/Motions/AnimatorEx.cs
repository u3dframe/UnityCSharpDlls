using UnityEngine;
using Core.Kernel;

public delegate void DF_ASM_MotionLife(Animator animator, AnimatorStateInfo stateInfo, int layerIndex,int action_state);
public delegate void DF_ASM_SubLife(Animator animator, int stateMachinePathHash,int action_state);

/// <summary>
/// 类名 : Amimator 扩展脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2019-08-22 22:17
/// 功能 : 
/// </summary>
// [ExecuteInEditMode]
[System.Serializable]
public class AnimatorEx : PrefabBasic
{
	static public new AnimatorEx Get(UnityEngine.Object uobj, bool isAdd){
		return GHelper.Get<AnimatorEx>(uobj, isAdd);
	}

	static public new AnimatorEx Get(UnityEngine.Object uobj)
    {
		return Get(uobj, true);
	}

	public bool m_isUseGID4MsgKey = true;
	private bool _pre_IsUseGID = false;

	public Animator m_animator = null;
	private int m_aniGID = 0;
	public string m_kActionState = "ation_state";
	public int m_actionState = -1;
	private int _pre_aState = -1;
	[Range(0f,50f)] public float m_actionSpeed = 1;
	private float _pre_aSpeed = -1;

	private BasicStateMachine[] _s_behaviours = null;

	public event DF_ASM_MotionLife m_evt_smEnter = null;
    public event DF_ASM_MotionLife m_evt_smUpdate = null;
    public event DF_ASM_MotionLife m_evt_smLoop = null;
    public event DF_ASM_MotionLife m_evt_smExit = null;
    public event DF_ASM_MotionLife m_evt_smMove = null;
    public event DF_ASM_MotionLife m_evt_smIK = null;

    public event DF_ASM_SubLife m_evt_subEnter = null;
    public event DF_ASM_SubLife m_evt_subExit = null;
    private int _lastLoop = 0;

    override protected void OnCall4Awake(){
        base.OnCall4Awake();

        this.csAlias = "ANI_Ex";
		if(this.m_animator == null)
			this.m_animator = this.m_gobj.GetComponentInChildren<Animator>(true);
        this._ReAniEvents();
	}

    override protected void OnCall4Start()
    {
        base.OnCall4Start();

        this.StartUpdate();
    }

    override protected void OnCall4Hide(){
		SetActionState(0);
		SetSpeedState(1);
		base.OnCall4Hide();
	}

	override protected void OnClear(){
		_ReAniEvents(false);

		this.m_animator = null;
		this._s_behaviours = null;
		this.m_evt_smEnter = null;
		this.m_evt_smUpdate = null;
        this.m_evt_smLoop = null;
        this.m_evt_smExit = null;
		this.m_evt_smMove = null;
		this.m_evt_smIK = null;
		this.m_evt_subEnter = null;
		this.m_evt_subExit = null;
	}

    override public void OnUpdate(float dt, float unscaledDt)
    {
        base.OnUpdate(dt, unscaledDt);

		if(this.m_animator){
			if(this.m_isUseGID4MsgKey != this._pre_IsUseGID){
				_ReAniEvents(true);
			}

			if(this.m_actionState != this._pre_aState){
				SetAction(this.m_actionState);
			}

			if(this.m_actionSpeed != this._pre_aSpeed){
				SetSpeed(this.m_actionSpeed);
			}
		}
	}

	private string _ReAniPreEvtKey(string key){
		return this._pre_IsUseGID ? string.Format("[{0}]_[{1}]",key,this.m_aniGID) : key;
	}

	private string _ReAniEvtKey(string key){
		return this.m_isUseGID4MsgKey ? string.Format("[{0}]_[{1}]",key,this.m_aniGID) : key;
	}

    private void _ReAniEvents() {
        if (this.m_animator == null)
        {
            Debug.LogErrorFormat("=== this animator is null, gobj name = [{0}]", this.m_gobj.name);
            return;
        }
        this.m_aniGID = this.m_animator.gameObject.GetInstanceID();
        this._s_behaviours = this.m_animator.GetBehaviours<BasicStateMachine>();
        _ReAniEvents(true, true);
    }

	private void _ReAniEvents(bool isBinde,bool isMust = false){
		if(this.m_animator == null) return;

		if(!isMust){
			if(this._pre_IsUseGID == this.m_isUseGID4MsgKey) return;
		}

		this._pre_IsUseGID = this.m_isUseGID4MsgKey;

		string _key;
		_key = _ReAniPreEvtKey(MsgConst.Msg_OnSMEnter);
		Messenger.RemoveListener<Animator,AnimatorStateInfo,int>(_key,_CF_SM_Enter);

		_key = _ReAniPreEvtKey(MsgConst.Msg_OnSMUpdate);
		Messenger.RemoveListener<Animator,AnimatorStateInfo,int>(_key,_CF_SM_Update);

		_key = _ReAniPreEvtKey(MsgConst.Msg_OnSMExit);
		Messenger.RemoveListener<Animator,AnimatorStateInfo,int>(_key,_CF_SM_Exit);

		_key = _ReAniPreEvtKey(MsgConst.Msg_OnSMMove);
		Messenger.RemoveListener<Animator,AnimatorStateInfo,int>(_key,_CF_SM_Move);

		_key = _ReAniPreEvtKey(MsgConst.Msg_OnSM_IK);
		Messenger.RemoveListener<Animator,AnimatorStateInfo,int>(_key,_CF_SM_IK);


		_key = _ReAniPreEvtKey(MsgConst.Msg_OnSubSMEnter);
		Messenger.RemoveListener<Animator,int>(_key,_CF_Sub_Enter);

		_key = _ReAniPreEvtKey(MsgConst.Msg_OnSubSMExit);
		Messenger.RemoveListener<Animator,int>(_key,_CF_Sub_Exit);

		if(isBinde){
			_key = _ReAniEvtKey(MsgConst.Msg_OnSMEnter);
			Messenger.AddListener<Animator,AnimatorStateInfo,int>(_key,_CF_SM_Enter);

			_key = _ReAniEvtKey(MsgConst.Msg_OnSMUpdate);
			Messenger.AddListener<Animator,AnimatorStateInfo,int>(_key,_CF_SM_Update);

			_key = _ReAniEvtKey(MsgConst.Msg_OnSMExit);
			Messenger.AddListener<Animator,AnimatorStateInfo,int>(_key,_CF_SM_Exit);

			_key = _ReAniEvtKey(MsgConst.Msg_OnSMMove);
			Messenger.AddListener<Animator,AnimatorStateInfo,int>(_key,_CF_SM_Move);

			_key = _ReAniEvtKey(MsgConst.Msg_OnSM_IK);
			Messenger.AddListener<Animator,AnimatorStateInfo,int>(_key,_CF_SM_IK);


			_key = _ReAniEvtKey(MsgConst.Msg_OnSubSMEnter);
			Messenger.AddListener<Animator,int>(_key,_CF_Sub_Enter);

			_key = _ReAniEvtKey(MsgConst.Msg_OnSubSMExit);
			Messenger.AddListener<Animator,int>(_key,_CF_Sub_Exit);

			int _lens = 0;
			if(_s_behaviours != null){
				_lens = _s_behaviours.Length;
			}
			for (int i = 0; i < _lens; i++)
			{
				_s_behaviours[i].m_isUseGID4MsgKey = this.m_isUseGID4MsgKey;
			}
		}
	}

    void _Exc_SM_Call(DF_ASM_MotionLife cfunc, Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        this._Exc_SM_Call(cfunc, animator, stateInfo, layerIndex, this.m_actionState);
    }

	void _Exc_SM_Call(DF_ASM_MotionLife cfunc,Animator animator, AnimatorStateInfo stateInfo, int layerIndex,int pars1) {
		if(cfunc != null)
			cfunc(animator,stateInfo,layerIndex, pars1);
	}

	void _Exc_Sub_Call(DF_ASM_SubLife cfunc,Animator animator , int stateMachinePathHash) {
		if(cfunc != null)
			cfunc(animator,stateMachinePathHash,this.m_actionState);
	}

	void _CF_SM_Enter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if(animator != this.m_animator) return;
        this._lastLoop = 0;
		_Exc_SM_Call(m_evt_smEnter,animator,stateInfo,layerIndex);
	}

	void _CF_SM_Update(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if(animator != this.m_animator) return;
		_Exc_SM_Call(m_evt_smUpdate,animator,stateInfo,layerIndex);
        int loop = (int)stateInfo.normalizedTime;
        if(loop != this._lastLoop)
        {
            this._lastLoop = loop;
            _Exc_SM_Call(m_evt_smLoop, animator, stateInfo,this.m_actionState, loop);
        }
	}

	void _CF_SM_Exit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if(animator != this.m_animator) return;
		_Exc_SM_Call(m_evt_smExit,animator,stateInfo,layerIndex);
	}

	void _CF_SM_Move(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if(animator != this.m_animator) return;
		_Exc_SM_Call(m_evt_smMove,animator,stateInfo,layerIndex);
	}

	void _CF_SM_IK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if(animator != this.m_animator) return;
		_Exc_SM_Call(m_evt_smIK,animator,stateInfo,layerIndex);
	}

	void _CF_Sub_Enter(Animator animator, int stateMachinePathHash){
		if(animator != this.m_animator) return;
		_Exc_Sub_Call(m_evt_subEnter,animator,stateMachinePathHash);
	}

	void _CF_Sub_Exit(Animator animator, int stateMachinePathHash){
		if(animator != this.m_animator) return;
		_Exc_Sub_Call(m_evt_subExit,animator,stateMachinePathHash);
	}

	public void SetSpeedState(float value){
		this._pre_aSpeed = this.m_actionSpeed;
		this.m_actionSpeed = value;
	}

	public void SetSpeed(float value){
		if(this.m_animator == null){
			this.m_actionSpeed = value;
			return;	
		}

		SetSpeedState(value);

		this.m_animator.speed = this.m_actionSpeed;
	}

    public bool IsHasParameter(string pkey)
    {
        if (this.m_animator == null || string.IsNullOrEmpty(pkey)) return false;
        var _arrPars = this.m_animator.parameters;
        if(_arrPars == null || _arrPars.Length <= 0) return false;
        AnimatorControllerParameter _ap;
        int _lens = _arrPars.Length;
        for (int i = 0; i < _lens; i++)
        {
            _ap = _arrPars[i];
            if (pkey.Equals(_ap.name))
                return true;
        }
        return false;
    }

    public void SetParameter4Int(string key,int value){
		if(this.m_animator == null) return;
		this.m_animator.SetInteger(key,value);
	}

	public void SetActionState(int value){
		this._pre_aState = this.m_actionState;
		this.m_actionState = value;
	}

	public void SetAction(int value){
		if(this.m_animator == null){
			this.m_actionState = value;
			return;	
		}

		if(this._pre_aSpeed != value && this.m_animator.gameObject.activeInHierarchy){
			this.m_animator.Update(0);
		}

		SetActionState(value);
		
		SetParameter4Int(this.m_kActionState,this.m_actionState);
	}

	public void SetActionAndASpeed(int aState,float aSpeed){
		this.SetSpeed(aSpeed);
		this.SetAction(aState);
	}

	public void PlayAction(string stateName,float normalizedTime = 0, int layer = -1)
    {
		if(this.m_animator == null) return;
		this.m_animator.Update(0);
		this.m_animator.Play(stateName,layer,normalizedTime);
	}

    public void ResetCurrAnimator()
    {
        if (!this.m_animator) return;
        this.m_animator.Rebind();
        this.m_animator.Update(0);
    }
}