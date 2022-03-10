using UnityEngine;
using Core;

/// <summary>
/// 类名 : CharacterController 扩展脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-23 19:17
/// 功能 : 
/// </summary>
// [ExecuteInEditMode]
[RequireComponent(typeof(CharacterController))]
[System.Serializable]
public class CharacterControllerEx : AnimatorEx
{
	static public new CharacterControllerEx Get(UnityEngine.Object uobj,bool isAdd){
		return UtilityHelper.Get<CharacterControllerEx>(uobj,isAdd);
	}

	static public new CharacterControllerEx Get(UnityEngine.Object uobj){
		return Get(uobj,true);
	}

	public CharacterController m_c_ctrler = null;
    public event DF_OnUpdate m_cf_OnUpdate = null;
    bool _isUsePhysics_ = true;
	public bool m_isUsePhysics{
		get{return _isUsePhysics_;}
		set{
			this._isUsePhysics_ = value;
			if(this.m_c_ctrler){
				this.m_c_ctrler.enabled = this._isUsePhysics_;
			}
		}
	}
	private Vector3 m_v3Scale = Vector3.one;
    public RendererMatData[] m_skinDatas { get; private set; }
    public AudioData m_audioData { get; private set; }

    override public void OnUpdate(float dt, float unscaledDt)
    {
        base.OnUpdate(dt, unscaledDt);

		if(this.m_cf_OnUpdate != null){
			this.m_cf_OnUpdate(Time.deltaTime,Time.unscaledDeltaTime);
		}
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
        // Rigidbody body = hit.collider.attachedRigidbody;
        // if (body == null || body.isKinematic)
        //     return;
        
        // if (hit.moveDirection.y < -0.3F)
        //     return;
        
        // Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        // body.velocity = pushDir * 2;
    }

	override protected void OnCall4Awake(){
        this.ReBindNodes();
        base.OnCall4Awake();
		this.csAlias = "CCtrler_Ex";
		if(this.m_c_ctrler == null)
			this.m_c_ctrler = this.m_gobj.GetComponentInChildren<CharacterController>(true);
        this.ReRHeight();
        this.ReSkinRer();
        // 加入音效
        m_audioData = AudioManager.instance.GetAudioData(this.m_gobj);
    }

	override protected void OnCall4Show(){
		base.OnCall4Show();
		_ReCalcSetOffset();
	}

    override protected void OnCall4Destroy() {
        this.ChgSkinMat(null, 99);
        base.OnCall4Destroy();
    }

    override protected void OnClear(){
		base.OnClear();
        this.m_c_ctrler = null;
		this.m_cf_OnUpdate = null;
	}

	private void _ReCalcSetOffset(){
		if(this.m_c_ctrler){
			var _diff = this.m_v3Scale - this.m_trsf.lossyScale;
			if(_diff.sqrMagnitude < 0.000001f) return;
			this.m_v3Scale = this.m_trsf.lossyScale;
			float _sOff = this.m_c_ctrler.height * this.m_v3Scale.y + this.m_c_ctrler.radius * 2 * this.m_v3Scale.x;
			_sOff = _sOff > 0.3f ? 0.3f : _sOff;
			this.m_c_ctrler.stepOffset =  _sOff;
		}
	}
	public void SetRadiusAndHeight(float radius,float height){
		if(this.m_c_ctrler == null) return;
		this.m_c_ctrler.radius = radius;
		this.m_c_ctrler.height = height;
		float yCenter =  (height <=  2 * radius) ? radius : height / 2;
		float skinWidth = this.m_c_ctrler.skinWidth;
		this.m_c_ctrler.center = new Vector3(0,yCenter + skinWidth,0);

		_ReCalcSetOffset();
	}

	[ContextMenu("Re Def Radius Height")]
	public void ReRHeightDef(){
		SetRadiusAndHeight(0.5f,2f);
	}

	[ContextMenu("Re Radius Height")]
	public void ReRHeight(){
		if(this.m_c_ctrler == null) return;
		float radius = this.m_c_ctrler.radius;
		float height = this.m_c_ctrler.height;
		SetRadiusAndHeight(radius,height);
	}

    protected override void ReBindNodes(bool isChild = false)
    {
        _arrs_nodes = new string[]{
            "heads","shadows","foot","skin",
            "f_head","f_l_hand","f_r_hand","f_mid","f_back",
            "f_l_foot","f_r_foot","f_l_weapon","f_r_weapon","victory",
            "f_node_00","f_node_01","f_node_02","f_node_03","f_node_04",
            "f_node_05","f_node_06","f_node_07","f_node_08","f_node_09",
            "f_node_10","f_node_11","f_node_12","f_node_13","f_node_14",
        };
        base.ReBindNodes(isChild);
    }

    public CharacterControllerEx InitCCEx(DF_OnUpdate on_up,DF_ASM_MotionLife on_a_enter,DF_ASM_MotionLife on_a_up,DF_ASM_MotionLife on_a_exit){
		this.m_cf_OnUpdate += on_up;
		this.m_evt_smEnter += on_a_enter;
		this.m_evt_smUpdate += on_a_up;
		this.m_evt_smExit += on_a_exit;
        // return this.ReSkinRer(); // 可能会导致 Object reference not set to an instance of an object.
        return this;
    }

    public CharacterControllerEx ReSkinRer()
    {
        if (this.m_gobj && this.m_skinDatas == null)
        {
            var arrs = this.m_gobj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if(arrs != null)
            {
                this.m_skinDatas = new RendererMatData[arrs.Length];
                for (int i = 0; i < arrs.Length; i++)
                {
                    this.m_skinDatas[i] = RendererMatData.Builder(arrs[i],false);
                }
            }
        }
        return this;
    }

    public void ChgSkinMat(Material newMat,int nType,string oneName = null)
    {
        var _arrs = this.m_skinDatas;
        this.m_skinDatas = null;
        if (_arrs == null || _arrs.Length <= 0)
            return;

        RendererMatData _itData;
        bool _isEmptyName = string.IsNullOrEmpty(oneName);
        for (int i = 0; i < _arrs.Length; i++)
        {
            _itData = _arrs[i];
            if (_itData == null)
                continue;

            if (nType == 99)
                _itData.ClearAll();
            else if (nType == 98)
            {
                if (_isEmptyName || oneName.Contains(_itData.m_rerName))
                {
                    _itData.SetEnabled(true);
                }
            }
            else if (nType == 97)
            {
                if (_isEmptyName || oneName.Contains(_itData.m_rerName))
                {
                    _itData.SetEnabled(false);
                }
            }
            else if (nType == 96)
            {
                if (_isEmptyName || oneName.Contains(_itData.m_rerName))
                {
                    _itData.SetActive(true);
                }
            }
            else if (nType == 95)
            {
                if (_isEmptyName || oneName.Contains(_itData.m_rerName))
                {
                    _itData.SetActive(false);
                }
            }
            else if(_isEmptyName || oneName.Contains(_itData.m_rerName))
                _itData.ChangeMat(newMat, nType);
        }

        if (nType != 99)
            this.m_skinDatas = _arrs;
    }

    public void EnabledSkinMatKey(string key,bool isEnabled = true)
    {
        if (string.IsNullOrEmpty(key))
            return;

        var _arrs = this.m_skinDatas;
        if (_arrs == null || _arrs.Length <= 0)
            return;

        RendererMatData _itData;
        for (int i = 0; i < _arrs.Length; i++)
        {
            _itData = _arrs[i];
            if (_itData == null)
                continue;
            if (isEnabled)
                _itData.EnableKeyword(key);
            else
                _itData.DisableKeyword(key);
        }
    }

    protected void CMove(Vector3 v3Add){
		if(this.m_c_ctrler == null) return;
		this.m_c_ctrler.Move(v3Add);
	}

	public void CMove(float x,float y,float z){
		if(this.m_c_ctrler == null) return;
		Vector3 _v3 = ToVec3(x,y,z);
		// Debug.LogErrorFormat("======= CMove [{0}] = x =[{1}] , y =[{2}] , z =[{3}] , xx =[{4}] , yy =[{5}] , z =[{6}]",this.name,x,y,z,_v3.x,_v3.y,_v3.z);
		this.CMove(_v3);
	}

	protected void CSimpleMove(Vector3 v3Add){
		if(this.m_c_ctrler == null) return;
		this.m_c_ctrler.SimpleMove(v3Add);
	}

	public void CSimpleMove(float x,float y,float z){
		if(this.m_c_ctrler == null) return;
		Vector3 _v3 = ToVec3(x,y,z);
		this.CSimpleMove(_v3);
	}

	public void Move(float x,float y,float z){
		this.LookAtDirction(x,y,z);
		if(this.m_isUsePhysics){
			this.CMove(x,y,z);
		}else{
			this.SetPosByAdd(x,y,z);
		}
	}

	public void SimpleMove(float x,float y,float z){
		this.LookAtDirction(x,y,z);
		if(this.m_isUsePhysics){
			this.CSimpleMove(x,y,z);
		}else{
			this.SetPosByAdd(x,y,z);
		}
	}
	
	public void SetCurrPos(float x,float y,float z){
		Vector3 _v3 = ToVec3(x,y,z);
		if(this.m_isUsePhysics){
			Vector3 _pos = this.m_trsf.position;
			this.CMove(_v3 - _pos);
		}else{
			this.m_trsf.position = _v3;
		}
	}

	public void GetCurrXYZ(ref float x,ref float y,ref float z){
		Vector3 v3 = this.m_trsf.position;
		x = v3.x;
		y = v3.y;
		z = v3.z;
		// Debug.LogErrorFormat("======= GetCurrXYZ [{0}] = x =[{1}] , z =[{2}]",this.name,x,z);
	}
}
