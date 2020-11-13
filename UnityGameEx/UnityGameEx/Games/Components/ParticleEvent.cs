using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 类名 : 粒子系统
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2012-12-05 14:15
/// 功能 : 向父类发送消息
/// </summary>
public class ParticleEvent : GobjLifeListener {
	static public new ParticleEvent Get(GameObject gobj,bool isAdd){
		return UtilityHelper.Get<ParticleEvent>(gobj,isAdd);
	}

	static public new ParticleEvent Get(GameObject gobj){
		return Get(gobj,true);
	}

	void OnBecameInvisible(){
		SendMessageUpwards("ChangePauseStateByEvent",true,SendMessageOptions.DontRequireReceiver);
	}

	void OnBecameVisible(){
		SendMessageUpwards("ChangePauseStateByEvent",false,SendMessageOptions.DontRequireReceiver);
	}
}
