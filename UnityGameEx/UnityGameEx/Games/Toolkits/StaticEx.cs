﻿using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using Core;
using UObject = UnityEngine.Object;

/// <summary>
/// 类名 : 静态类工具
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-08-16 17:03
/// 功能 : Extension method must be defined in a non-generic static class
/// </summary>
public static class StaticEx {
	static public void SetProperty(this Material material, int type, string name, object value)
    {
        if(null == material) return;

        switch(type)
        {
        case 0:
            material.SetColor(name, (Color)value);
            break;
        case 1:
            material.SetVector(name, (Vector4) value);
            break;
        case 2:
            material.SetFloat(name, (float)value);
            break;
        case 3: // Range
            material.SetFloat(name, (float)value);
            break;
        case 4:
            material.SetTexture(name, (Texture) value);
            break;
        }
    }

    static public void SetProperty(this Material material,string name,Color value)
    {
        SetProperty(material,0,name,value);
    }

    static public void SetProperty(this Material material,string name,Vector4 value)
    {
        SetProperty(material,1,name,value);
    }

    static public void SetProperty(this Material material,string name,float value)
    {
        SetProperty(material,2,name,value);
    }

    static public void SetProperty(this Material material,string name,Texture value)
    {
        SetProperty(material,4,name,value);
    }

    static public void ReShader(this Material material)
    {
        if(null == material) return;
        
        Shader _sd = material.shader;
        if(_sd != null && !string.IsNullOrEmpty(_sd.name)){
            _sd = ABShader.FindShader(_sd.name);
            if(_sd != null){
                material.shader = _sd;
            }
        }
    }

    static public Material GetMat(Renderer render)
    {
        if(null == render) return null;
#if UNITY_EDITOR
        return render.material;
#else
        return render.sharedMaterial;
#endif
    }

    static public Material[] GetMats(Renderer render)
    {
        if(null == render) return null;
#if UNITY_EDITOR
        return render.materials;
#else
        return render.sharedMaterials;
#endif
    }

#if UNITY_EDITOR
    static public void ReShader(this Renderer render)
    {
        if(null == render) return;
        ReShader(GetMat( render ));
        Material[] _mats_ = GetMats( render );
        if (_mats_ != null && _mats_.Length > 0)
        {
            int lens = _mats_.Length;
            for (int i = 0; i < lens; i++)
            {
                ReShader(_mats_[i]);
            }
        }
    }
#else
    static public void ReShader(this Renderer render)
    {
        if(null == render) return;
        ReShader(render.material);
        Material[] _mats_ = render.materials;
        if (_mats_ != null && _mats_.Length > 0)
        {
            int lens = _mats_.Length;
            for (int i = 0; i < lens; i++)
            {
                ReShader(_mats_[i]);
            }
        }
    }
#endif

    static public void ReColor(this Material material,Color color)
    {
        if(null == material) return;
        if (material.HasProperty("_Color")) {
            material.SetColor("_Color", color);
        } else if (material.HasProperty("_TintColor")) {
            material.SetColor("_TintColor", color);
        }
    }

#if UNITY_EDITOR
    static public void ReColor(this Renderer render,Color color)
    {
        if(null == render) return;
        ReColor(GetMat( render ),color);
        Material[] _mats_ = GetMats( render );
        if (_mats_ != null && _mats_.Length > 0)
        {
            int lens = _mats_.Length;
            for (int i = 0; i < lens; i++)
            {
                ReColor(_mats_[i],color);
            }
        }
    }
#else
    static public void ReColor(this Renderer render,Color color)
    {
        if(null == render) return;
        ReColor(render.material,color);
        Material[] _mats_ = render.materials;
        if (_mats_ != null && _mats_.Length > 0)
        {
            int lens = _mats_.Length;
            for (int i = 0; i < lens; i++)
            {
                ReColor(_mats_[i],color);
            }
        }
    }
#endif

    static public void ReAlpha(this Material material,float alpha)
    {
        if(null == material) return;
        if (material.HasProperty("_Color")) {
            Color col = material.GetColor ("_Color");
            col.a = alpha;
            ReColor(material,col);
        } else if (material.HasProperty("_TintColor")) {
            Color col = material.GetColor ("_TintColor");
            col.a = alpha;
            ReColor(material,col);
        } else if (material.HasProperty("_Alpha")) {
            SetProperty(material,"_Alpha",alpha);
        }
    }

#if UNITY_EDITOR
    static public void ReAlpha(this Renderer render,float alpha)
    {
        if(null == render) return;
        ReAlpha(GetMat( render ),alpha);
        Material[] _mats_ = GetMats( render );
        if (_mats_ != null && _mats_.Length > 0)
        {
            int lens = _mats_.Length;
            for (int i = 0; i < lens; i++)
            {
                ReAlpha(_mats_[i],alpha);
            }
        }
    }
#else
    static public void ReAlpha(this Renderer render,float alpha)
    {
        if(null == render) return;
        ReAlpha(render.material,alpha);
        Material[] _mats_ = render.materials;
        if (_mats_ != null && _mats_.Length > 0)
        {
            int lens = _mats_.Length;
            for (int i = 0; i < lens; i++)
            {
                ReAlpha(_mats_[i],alpha);
            }
        }
    }
#endif

    static public void ReSortingOrder(this Renderer render,int sortingOrder){
        if(null == render) return;
        render.sortingOrder = sortingOrder;
    }

    static public void AddSortingOrder(this Renderer render,int addValue){
        if(null == render) return;
        render.sortingOrder += addValue;
    }

    static public void ReRenderQueue(this Material material,int renderQueue)
    {
        if(null == material) return;
        material.renderQueue = renderQueue;
    }

#if UNITY_EDITOR
    static public void ReRenderQueue(this Renderer render,int renderQueue)
    {
        if(null == render) return;
        ReRenderQueue(GetMat( render ),renderQueue);
        Material[] _mats_ = GetMats( render );
        if (_mats_ != null && _mats_.Length > 0)
        {
            int lens = _mats_.Length;
            for (int i = 0; i < lens; i++)
            {
                ReRenderQueue(_mats_[i],renderQueue);
            }
        }
    }
#else
    static public void ReRenderQueue(this Renderer render,int renderQueue)
    {
        if(null == render) return;
        ReRenderQueue(render.material,renderQueue);
        Material[] _mats_ = render.materials;
        if (_mats_ != null && _mats_.Length > 0)
        {
            int lens = _mats_.Length;
            for (int i = 0; i < lens; i++)
            {
                ReRenderQueue(_mats_[i],renderQueue);
            }
        }
    }
#endif

    static public void AddRenderQueue(this Material material,int addValue)
    {
        if(null == material) return;
        material.renderQueue += addValue;
    }
    
#if UNITY_EDITOR
    static public void AddRenderQueue(this Renderer render,int addValue)
    {
        if(null == render) return;
        AddRenderQueue(GetMat( render ),addValue);
        Material[] _mats_ = GetMats( render );
        if (_mats_ != null && _mats_.Length > 0)
        {
            int lens = _mats_.Length;
            for (int i = 0; i < lens; i++)
            {
                AddRenderQueue(_mats_[i],addValue);
            }
        }
    }
#else
    static public void AddRenderQueue(this Renderer render,int addValue)
    {
        if(null == render) return;
        AddRenderQueue(render.material,addValue);
        Material[] _mats_ = render.materials;
        if (_mats_ != null && _mats_.Length > 0)
        {
            int lens = _mats_.Length;
            for (int i = 0; i < lens; i++)
            {
                AddRenderQueue(_mats_[i],addValue);
            }
        }
    }
#endif

    static public void ReShader(this GameObject gobj)
    {
        if(null == gobj) return;
        Renderer[] _arrs = gobj.GetComponentsInChildren<Renderer>(true);
        if(null == _arrs) return;
        int _lens = _arrs.Length;
        for (int i = 0; i < _lens; i++)
        {
            ReShader(_arrs[i]);
        }

        ReShaderBox(gobj);
    }

    static public void ReShaderBox(this GameObject gobj)
    {
        if(null == gobj) return;
        Skybox[] _arrs = gobj.GetComponentsInChildren<Skybox>(true);
        if(null == _arrs) return;
        int _lens = _arrs.Length;
        for (int i = 0; i < _lens; i++)
        {
            ReShader(_arrs[i].material);
        }
    }

    static public void ReUIShader(this UnityEngine.UI.Image img)
    {
        if(null == img) return;
        Material material = img.material;
        if(null == material) return;
        Shader shader = material.shader;
        if(null == shader) return;
        if (!"UI/Default".Equals(shader.name,StringComparison.OrdinalIgnoreCase)) {
            Shader _sd = ABShader.FindShader(shader.name);
            if(_sd != null){
                material.shader = _sd;
            }
        }
    }

    static public void ReUIShader(this GameObject gobj)
    {
        if(null == gobj) return;
        UnityEngine.UI.Image[] _arrs = gobj.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        if(null == _arrs) return;
        int _lens = _arrs.Length;
        for (int i = 0; i < _lens; i++)
        {
            ReUIShader(_arrs[i]);
        }
    }

    static public void SetProperty(this UnityEngine.UI.MaskableGraphic graphic, int type, string name, object value)
    {
        if(null == graphic) return;
        Material material = graphic.material;
        if(null == material) return;
        GobjLifeListener glife = GobjLifeListener.Get(graphic.gameObject);
        UGUIMatData udata = glife.m_obj1 as UGUIMatData;
        if(udata == null){
            udata = UGUIMatData.Builder(graphic,true);
        }
        if(udata != null){
            material = udata.m_newMat;
        }
        SetProperty(material,type,name,value);
    }

    static public void SetProperty(this UnityEngine.UI.MaskableGraphic graphic,string name, float value){
        SetProperty(graphic,2,name,value);
    }
}