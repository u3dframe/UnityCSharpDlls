using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 类名 : 粒子系统
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2012-12-05 14:15
/// 功能 : UNITY_5_5_OR_NEWER 以后的逻辑
/// 修改 : 2018-09-14 16:38 整合
/// 修改 : 2020-09-12 09:38 优化
/// </summary>
public class ParticleSystemEx : GobjLifeListener {
	static public new ParticleSystemEx Get(UnityEngine.Object uobj, bool isAdd)
    {
		return GHelper.Get<ParticleSystemEx>(uobj, isAdd);
	}

	static public new ParticleSystemEx Get(UnityEngine.Object uobj)
    {
		return Get(uobj, true);
	}
	
	List<ParticleSystem> listAll = new List<ParticleSystem>();
	List<Renderer> listAllRenders = new List<Renderer>();
	Dictionary<int,List<float>> dicDefaultScale = new Dictionary<int,List<float>>();

	int lens = 0;
	// 该粒子的最长时间
	float _maxTime = 1f;
	public float maxTime{ get{ return _maxTime + 0.01f; } }

	bool tPause = false;
	float tScale = 1f,tSpeed = 1f,tStartSize = -1f;

	public bool isUpdate = true;
	public bool m_isUseControlPause = true;
	public bool isPause = false;
	public float scale = 1f,speedRate = 1f,startSize = -1f;
    public bool isIgnoreTimeScale = false;

    ParticleSystem.MainModule mainModule;
	ParticleSystem.MinMaxCurve minMaxCurve;
	ParticleSystem.ShapeModule shapeModule;
    ParticleSystem.VelocityOverLifetimeModule velocityLifetime;
	
	List<Animator> m_listAni = new List<Animator>();
	Dictionary<int,float> m_dicAniSpeed = new Dictionary<int,float>();

	List<Animation> m_listAnm = new List<Animation>();
	Dictionary<string,float> m_dicAnmSpeed = new Dictionary<string,float>();

	override protected void OnCall4Awake() {
		InitParticleSystem ();
		InitAnimator ();
		InitAnimation ();
	}

	override protected void OnCall4Start() {
		if(listAllRenders.Count > 0){
			GameObject _gobj = listAllRenders[0].gameObject;
			ParticleEvent.Get(_gobj);	
		}
	}
	
	void Update(){
		if (!isUpdate) {
			return;
		}

        this._UpUnscaledTime();

        if (tStartSize != startSize) {
			SetStartSize(startSize);
		}

		if (tScale != scale) {
			SetScale(scale);
		}

		if (tPause != isPause) {
			ChangePauseState(isPause);
		}

		if (tSpeed != speedRate) {
			SetSpeedRate(speedRate);
		}
	}
	
	void InitParticleSystem(){
		listAll.Clear ();
		ParticleSystem[] arr = transform.GetComponentsInChildren<ParticleSystem> (true);
		Renderer[] arrRenders = transform.GetComponentsInChildren<Renderer> (true);
		if (arrRenders != null && arrRenders.Length > 0) {
			listAllRenders.AddRange (arrRenders);
		}

		if (arr != null && arr.Length > 0) {
			listAll.AddRange (arr);
		}

		lens = listAll.Count;
		if (lens <= 0) {
			return;
		}
		
		ParticleSystem ps;
		float curTime = 0f;
		int key;
		List<float> vList;
		Vector3 _v3 = Vector3.zero;
		for (int i = 0; i < lens; i++) {
			ps = listAll[i];
			key = ps.GetInstanceID();
			vList = new List<float>();

			mainModule = ps.main;
			curTime = mainModule.startDelayMultiplier + mainModule.duration;

			// 大小
			vList.Add(mainModule.startSize.constantMin); // 0
			vList.Add(mainModule.startSize.constantMax);
			vList.Add(mainModule.gravityModifier.constantMin);
			vList.Add(mainModule.gravityModifier.constantMax);

			// 速度
			vList.Add(mainModule.startSpeed.constantMin);
			vList.Add(mainModule.startSpeed.constantMax);
			vList.Add(mainModule.simulationSpeed);

			// 处理shape的
			shapeModule = ps.shape;
			vList.Add(shapeModule.randomDirectionAmount);// 7
			vList.Add(shapeModule.sphericalDirectionAmount);
			vList.Add(shapeModule.angle);
			vList.Add(shapeModule.arc);
			vList.Add(shapeModule.arcSpread);

			_v3 = shapeModule.scale;
			vList.Add(_v3.x); // 12
			vList.Add(_v3.y);
			vList.Add(_v3.z);
			vList.Add(shapeModule.radius);
			vList.Add(shapeModule.arcSpeed.constantMin); // 16
			vList.Add(shapeModule.arcSpeed.constantMax);
			vList.Add(shapeModule.arcSpeed.curveMultiplier);
			vList.Add(shapeModule.arcSpeedMultiplier); // 19

            velocityLifetime = ps.velocityOverLifetime;
            vList.Add(velocityLifetime.xMultiplier);//20
            vList.Add(velocityLifetime.yMultiplier);
            vList.Add(velocityLifetime.zMultiplier);

			dicDefaultScale.Add(key,vList);

			if(curTime > _maxTime){
				_maxTime = curTime;
			}
		}
	}
	
	void InitAnimator(){
		Animator[] arrs = gameObject.GetComponentsInChildren<Animator> (true);
		if (arrs == null || arrs.Length <= 0) {
			return;
		}

		Animator ani;
		AnimationClip[] arrClips;
		int key;
		RuntimeAnimatorController runAniCtrl;
		for (int i = 0; i < arrs.Length; i++) {
			ani = arrs [i];
			runAniCtrl = ani.runtimeAnimatorController;
			if(runAniCtrl == null)
				continue;
			arrClips = runAniCtrl.animationClips;
			if (arrClips == null || arrClips.Length <= 0) {
				continue;
			}
			m_listAni.Add (ani);

			key = ani.GetInstanceID ();
			m_dicAniSpeed.Add (key, ani.speed);
		}
	}

	void InitAnimation(){
		Animation[] arrs = gameObject.GetComponentsInChildren<Animation> (true);
		if (arrs == null || arrs.Length <= 0) {
			return;
		}

		Animation anm;
		int key;
		for (int i = 0; i < arrs.Length; i++) {
			anm = arrs [i];
			if (anm.GetClipCount () <= 0)
				continue;
			
			m_listAnm.Add (anm);
			key = anm.GetInstanceID ();
			foreach (AnimationState state in anm) {
				m_dicAnmSpeed.Add (string.Format("{0}_{1}",key,state.name), state.speed);
			}
		}
	}

	public void SetStartSize(float size){
		if (lens <= 0 || size < 0) {
			return;
		}
		this.tStartSize = size;
		this.startSize = size;
		ParticleSystem ps;
		for (int i = 0; i < lens; i++) {
			ps = listAll[i];
			mainModule = ps.main;
			minMaxCurve = mainModule.startSize;
			minMaxCurve.constantMax = size;
			mainModule.startSize = minMaxCurve;
		}
	}

	public void SetScale(float _scale){
		if (lens <= 0 || _scale < 0) {
			return;
		}
		
		this.tScale = _scale;
		this.scale = _scale;

		ParticleSystem ps;
		List<float> vList;
		Vector3 _v3 = Vector3.zero;
		for (int i = 0; i < lens; i++) {
			ps = listAll[i];
            ps.Clear();
			vList = dicDefaultScale[ps.GetInstanceID()];

			mainModule = ps.main;

			minMaxCurve = mainModule.startSize;
			minMaxCurve.constantMin = _scale * (vList[0]);
			minMaxCurve.constantMax = _scale * (vList[1]);
			mainModule.startSize = minMaxCurve;

			minMaxCurve = mainModule.gravityModifier;
			minMaxCurve.constantMin = _scale * (vList[2]);
			minMaxCurve.constantMax = _scale * (vList[3]);
			mainModule.gravityModifier = minMaxCurve;

			minMaxCurve = mainModule.startSpeed;
			minMaxCurve.constantMin = _scale * (vList[4]);
			minMaxCurve.constantMax = _scale * (vList[5]);
			mainModule.startSpeed = minMaxCurve;
			
			shapeModule = ps.shape;
			shapeModule.angle = _scale * (vList[9]);
			shapeModule.arc = _scale * (vList[10]);
			_v3.x = _scale * (vList[12]);
			_v3.y = _scale * (vList[13]);
			_v3.z = _scale * (vList[14]);
			shapeModule.scale = _v3;
			shapeModule.radius = _scale * (vList[15]);

			velocityLifetime = ps.velocityOverLifetime;
            velocityLifetime.xMultiplier = _scale * (vList[20]);
            velocityLifetime.yMultiplier = _scale * (vList[21]);
            velocityLifetime.zMultiplier = _scale * (vList[22]);
		}
	}

	public void ChangePauseStateByEvent(bool _isPause){
		if(this.m_isUseControlPause)
			return;
		
		ChangePauseState(_isPause);
	}

	public void ChangePauseState(bool _isPause){
		this.tPause = _isPause;
		this.isPause = _isPause;

		int _lens = lens;
		ParticleSystem ps;
		for (int i = 0; i < _lens; i++) {
			ps = listAll[i];
			if(_isPause){
				ps.Pause();
			}else{
				ps.Play(false);
			}
		}

		_lens = m_listAni.Count;
		Animator ani;
		for (int i = 0; i < _lens; i++) {
			ani = m_listAni [i];
			// ani.enabled = !_isPause;
			if(_isPause){
				ani.speed = 0;
			}else{
				ani.speed = m_dicAniSpeed[ani.GetInstanceID()] * this.speedRate;
			}
		}

		_lens = m_listAnm.Count;
		Animation anm;
		for (int i = 0; i < _lens; i++) {
			anm = m_listAnm [i];
			foreach (AnimationState state in anm) {
				if(_isPause){
					state.speed = 0;
				}else{
					state.speed = m_dicAnmSpeed[string.Format("{0}_{1}",anm.GetInstanceID(),state.name)] * this.speedRate;
				}
			}
		}
	}

    public void RePlay(bool isIgnoreTimeScale)
    {
        int _lens = lens;
        ParticleSystem ps;
        for (int i = 0; i < _lens; i++)
        {
            ps = listAll[i];
            mainModule = ps.main;
            mainModule.useUnscaledTime = isIgnoreTimeScale;
            ps.Simulate(0, false, true);
            ps.Play(false);
        }

        _lens = m_listAni.Count;
        Animator ani;
        for (int i = 0; i < _lens; i++)
        {
            ani = m_listAni[i];
            ani.updateMode = isIgnoreTimeScale ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
            ani.Update(0);
        }

        _lens = m_listAnm.Count;
        Animation anm;
        for (int i = 0; i < _lens; i++)
        {
            anm = m_listAnm[i];
            foreach (AnimationState state in anm)
            {
                state.normalizedTime = 0;
            }
            if (isIgnoreTimeScale)
                anm.Sample();
            else
                anm.Play();
        }
        this.isIgnoreTimeScale = isIgnoreTimeScale;
    }

    /// <summary>
    /// 设置速度倍率 - 在原始速度的基础上的倍率
    /// </summary>
    public void SetSpeedRate(float speedRate){
		if (speedRate < 0) {
			return;
		}
		this.speedRate = speedRate;
		this.tSpeed = speedRate;

		int _lens = lens;
		ParticleSystem ps;
		List<float> vList;
		for (int i = 0; i < _lens; i++) {
			ps = listAll[i];
            ps.Clear();
            vList = dicDefaultScale[ps.GetInstanceID()];

			mainModule = ps.main;
			minMaxCurve = mainModule.startSpeed;
			minMaxCurve.constantMin = speedRate * (vList[4]);
			minMaxCurve.constantMax = speedRate * (vList[5]);
			mainModule.startSpeed = minMaxCurve;
			
			mainModule.simulationSpeed = speedRate * (vList[6]);

			shapeModule = ps.shape;
			minMaxCurve = shapeModule.arcSpeed;
			minMaxCurve.constantMin = speedRate * (vList[16]);
			minMaxCurve.constantMax = speedRate * (vList[17]);
			minMaxCurve.curveMultiplier = speedRate * (vList[18]);
			shapeModule.arcSpeed = minMaxCurve;
			shapeModule.arcSpeedMultiplier = speedRate * (vList[19]);
		}

		_lens = m_listAni.Count;
		Animator ani;
		for (int i = 0; i < _lens; i++) {
			ani = m_listAni [i];
			ani.speed = m_dicAniSpeed[ani.GetInstanceID()] * speedRate;
		}

		_lens = m_listAnm.Count;
		Animation anm;
		for (int i = 0; i < _lens; i++) {
			anm = m_listAnm [i];
			foreach (AnimationState state in anm) {
				state.speed = m_dicAnmSpeed[string.Format("{0}_{1}",anm.GetInstanceID(),state.name)] * speedRate;
			}
		}
	}

    public void SetAlpha(float alpha){
		int lensRender = listAllRenders.Count;
		if (lensRender <= 0 || alpha < 0) {
			return;
		}
		Renderer curRender;
		alpha = alpha > 1 ? alpha / 255f : alpha;
		for (int i = 0; i < lensRender; i++) {
			curRender = listAllRenders[i];
			// curRender.ReAlpha(alpha);
		}
	}

    public void SetIgnoreTimeScale(bool isIgnoreTimeScale)
    {
        bool _isChg = this.isIgnoreTimeScale != isIgnoreTimeScale;
        if(_isChg)
        {
            int _lens = lens;
            ParticleSystem ps;
            for (int i = 0; i < _lens; i++)
            {
                ps = listAll[i];
                mainModule = ps.main;
                mainModule.useUnscaledTime = isIgnoreTimeScale;
            }

            _lens = m_listAni.Count;
            Animator ani;
            for (int i = 0; i < _lens; i++)
            {
                ani = m_listAni[i];
                if (ani != null)
                    ani.updateMode = isIgnoreTimeScale ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
            }
        }
        this.isIgnoreTimeScale = isIgnoreTimeScale;
    }

    void _UpUnscaledTime()
    {
        if (!this.isIgnoreTimeScale)
            return;
        float _dt = Time.unscaledDeltaTime;
        int _lens = lens;
        _lens = m_listAnm.Count;
        Animation anm;
        float _ctime = -1;
        for (int i = 0; i < _lens; i++)
        {
            anm = m_listAnm[i];
            foreach (AnimationState state in anm)
            {
                if(anm.IsPlaying(state.name))
                {
                    _ctime = state.normalizedTime * state.length + _dt;
                    state.normalizedTime = _ctime  / state.length;
                }
            }
            anm.Sample();
        }
    }

    [ContextMenu("Pause ParticleSystem")]
	void Pause(){
		ChangePauseState(true);
	}

	[ContextMenu("Regain ParticleSystem")]
	void Regain(){
		ChangePauseState(false);
	}

    [ContextMenu("Change Ignore TimeScale")]
    void ChangeIgnoreTimeScale()
    {
        this.SetIgnoreTimeScale(!this.isIgnoreTimeScale);
    }
}
