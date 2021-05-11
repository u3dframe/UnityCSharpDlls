﻿using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Video;
/// <summary>
/// 类名 : 定义 通用的 代理事件
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-26 08:29
/// 功能 : 
/// </summary>
namespace Core
{
    public delegate void DF_LoadedFab(GameObject gobj);

    public delegate void DF_LoadedTex2D(Texture2D tex);
    public delegate void DF_ToLoadTex2D(string abName, string assetName, DF_LoadedTex2D clip);

    public delegate void DF_LoadedTex2DExt(Texture2D tex,object ext1,object ext2);
    public delegate void DF_ToLoadTex2DExt(string abName, string assetName, DF_LoadedTex2DExt clip, object ext1, object ext2);

    public delegate void DF_LoadedCube(Cubemap tex);
    public delegate void DF_ToLoadCube(string abName, string assetName, DF_LoadedCube clip);

    public delegate void DF_LoadedSprite(Sprite sprite);
    public delegate void DF_LoadedAnimator(Animator ator);
    public delegate void DF_LoadedAnimationClip(AnimationClip clip);
    public delegate void DF_LoadedMaterial(Material mat);
    public delegate void DF_LoadedShader(Shader mat);

    public delegate void DF_LoadedAdoClip(AudioClip clip);
    public delegate void DF_ToLoadAdoClip(string abName, string assetName,DF_LoadedAdoClip clip);

    public delegate void DF_LoadedVdoClip(VideoClip clip);
    public delegate void DF_ToLoadVdoClip(string abName, string assetName, DF_LoadedVdoClip clip);

    public delegate void DF_LoadedTimelineAsset(TimelineAsset pa);
    public delegate void DF_OnBool(bool isBl);
    public delegate void DF_OnInt(int val);
    public delegate void DF_OnFloat(float val);
    public delegate void DF_OnStr(string val);
    public delegate void DF_OnKVal(object key,object val);
    public delegate void DF_OnUpdate(float dt,float unscaledDt);
    public delegate void DF_CurrMax(double curr,double max);
    public delegate void DF_OnState(int state,int preState);
    public delegate void DF_OnError(bool isException, string errMsg);
    public delegate void DF_OnSceneChange(int level);
    public delegate void DF_OnNotifyDestry(GobjLifeListener obj);
}