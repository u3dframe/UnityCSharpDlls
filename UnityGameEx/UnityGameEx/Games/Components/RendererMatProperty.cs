using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 类名 : Render 渲染 材质 Mat的属性
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-09-20 09:53
/// 功能 : 
/// </summary>
public class RendererMatProperty : GobjLifeListener
{
    static public new RendererMatProperty Get(GameObject gobj,bool isAdd){
		return UtilityHelper.Get<RendererMatProperty>(gobj,isAdd);
	}

	static public new RendererMatProperty Get(GameObject gobj){
		return Get(gobj,true);
	}

    MaterialPropertyBlock m_mpb;
    [HideInInspector] public Renderer[] m_renderers;
    int m_lensRender = 0;
    Color m_color = Color.white;

    override protected void OnCall4Start()
    {
        m_mpb = new MaterialPropertyBlock();
        ReInitRenderers();
    }

    public void ReInitRenderers(){
        m_renderers = this.GetComponentsInChildren<Renderer>(true);
        if(null != m_renderers){
            this.m_lensRender = m_renderers.Length;
        }
    }

    public void SetInt(string key,int val){
        if(this.m_lensRender <= 0)
            return;
        
        Renderer _rer_;
        for (int i = 0; i < m_lensRender; i++)
        {
            _rer_ = m_renderers[i];
            if(null == _rer_)
                continue;
            m_mpb.Clear();
            _rer_.GetPropertyBlock(m_mpb);
            m_mpb.SetInt(key,val);            
            _rer_.SetPropertyBlock(m_mpb);
        }
    }

    public void SetFloat(string key,float val){
        if(this.m_lensRender <= 0)
            return;
        
        Renderer _rer_;
        for (int i = 0; i < m_lensRender; i++)
        {
            _rer_ = m_renderers[i];
            if(null == _rer_)
                continue;
            m_mpb.Clear();
            _rer_.GetPropertyBlock(m_mpb);
            m_mpb.SetFloat(key,val);            
            _rer_.SetPropertyBlock(m_mpb);
        }
    }

    public float CalcColor(float val){
        if(val <= 0)
            return 0;
        val %= 255;
        val = val > 1 ? val / 255 : val;
        return val;
    }

    public void SetColor(string key,float r,float g,float b,float a){
        if(this.m_lensRender <= 0)
            return;
        
        m_color.a = CalcColor(a);
        m_color.b = CalcColor(b);
        m_color.g = CalcColor(g);
        m_color.r = CalcColor(r);
        if(m_color == Color.clear){
            return;
        }

        Renderer _rer_;
        for (int i = 0; i < m_lensRender; i++)
        {
            _rer_ = m_renderers[i];
            if(null == _rer_)
                continue;
             m_mpb.Clear();
            _rer_.GetPropertyBlock(m_mpb);
            m_mpb.SetColor(key,m_color);            
            _rer_.SetPropertyBlock(m_mpb);
        }
    }
    
    public void SetMatrix(string key,Matrix4x4 val){
        if(this.m_lensRender <= 0)
            return;
        
        Renderer _rer_;
        for (int i = 0; i < m_lensRender; i++)
        {
            _rer_ = m_renderers[i];
            if(null == _rer_)
                continue;
            m_mpb.Clear();
            _rer_.GetPropertyBlock(m_mpb);
            m_mpb.SetMatrix(key,val);            
            _rer_.SetPropertyBlock(m_mpb);
        }
    }

    public void SetTexture(string key,Texture val){
        if(this.m_lensRender <= 0)
            return;
        
        Renderer _rer_;
        for (int i = 0; i < m_lensRender; i++)
        {
            _rer_ = m_renderers[i];
            if(null == _rer_)
                continue;
            m_mpb.Clear();
            _rer_.GetPropertyBlock(m_mpb);
            m_mpb.SetTexture(key,val);            
            _rer_.SetPropertyBlock(m_mpb);
        }
    }

    public void SetVector(string key,Vector4 val){
        if(this.m_lensRender <= 0)
            return;
        
        Renderer _rer_;
        for (int i = 0; i < m_lensRender; i++)
        {
            _rer_ = m_renderers[i];
            if(null == _rer_)
                continue;
            m_mpb.Clear();
            _rer_.GetPropertyBlock(m_mpb);
            m_mpb.SetVector(key,val);            
            _rer_.SetPropertyBlock(m_mpb);
        }
    }
}
