using UnityEngine;
using Core;

/// <summary>
/// 类名 : 场景参数
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-03-21 10:37
/// 功能 : 场景的雾效，摄像机参数等
/// 修改 ：2020-10-10 10:02
/// </summary>
[AddComponentMenu("Scene/SceneInfoEx")]
public class SceneInfoEx : SceneBasicEx
{
	static public new SceneInfoEx Get(GameObject gobj,bool isAdd){
		return UtilityHelper.Get<SceneInfoEx>(gobj,isAdd);
	}

	static public new SceneInfoEx Get(GameObject gobj){
		return Get(gobj,true);
	}
}