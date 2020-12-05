using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 类名 : Prefab 单元对象
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-08-04 00:10
/// 功能 : 缓存需要操作的对象
/// </summary>
public class PrefabElement : PrefabBasic {
	static public new PrefabElement Get(GameObject gobj,bool isAdd){
		return UtilityHelper.Get<PrefabElement>(gobj,true);
	}

	static public new PrefabElement Get(GameObject gobj){
		return Get(gobj,true);
	}
}