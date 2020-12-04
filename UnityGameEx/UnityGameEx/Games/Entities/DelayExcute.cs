using System;
/// <summary>
/// 类名 : 延迟执行
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2019-11-25 12:53
/// 功能 : 
/// </summary>
[Serializable]
public class DelayExcute : IUpdate
{
	public bool m_isUnscaled = false;
	private float m_delayTime = 0.1f;
	private float _curDelayTime = 0f;
	private Action m_call = null;

	public DelayExcute(float delayTime,Action callBack){
	   this.Init(delayTime,callBack);
	}
	
	public DelayExcute Init(float delayTime,Action callBack){
		this.RegUpdate(false);
		this._curDelayTime = 0;
		this.m_delayTime = delayTime;
		this.m_call = callBack;
		return this;
	}
	
	private bool m_isOnUpdate = false;
    public bool IsOnUpdate() { return this.m_isOnUpdate; }
	public void OnUpdate(float dt, float unscaledDt) {
		this._curDelayTime += this.m_isUnscaled ? unscaledDt : dt;
		if(this._curDelayTime >= this.m_delayTime){
			this.RegUpdate(false);
			Action _call = this.m_call;
			this.m_call = null;
			if(_call != null){
				_call();
			}
		}
	}
	
	public DelayExcute RegUpdate(bool isUp)
	{
		this.m_isOnUpdate = false;
		GameMgr.DiscardUpdate(this);
		if (isUp)
			GameMgr.RegisterUpdate(this);
        this.m_isOnUpdate = isUp;
        return this;
	}
	
	public DelayExcute Start()
	{
		if (!this.m_isOnUpdate)
			this.RegUpdate(true);

		return this;
	}
	
	public DelayExcute Stop(bool isClear)
	{
		this.RegUpdate(false);
		if(isClear){
			this.m_call = null;
		}
		return this;
	}
}
