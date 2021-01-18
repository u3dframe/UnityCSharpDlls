using UnityEngine;
using System;

/// <summary>
/// 类名 : 场景的光照贴图数据
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2019-08-19 10:17
/// 功能 : 
/// </summary>
[Serializable]
public class SceneLightMapData : IDisposable
{
	public Texture2D lightmapColor,lightmapDir,shadowMask;

    public SceneLightMapData(){
    }

	public SceneLightMapData(LightmapData data)
    {
		this.SetLightmapData(data);
    }

	public void SetLightmapData(LightmapData data)
    {
        this.lightmapColor = data.lightmapColor;
        this.lightmapDir = data.lightmapDir;
        this.shadowMask = data.shadowMask;
    }

	public LightmapData ToLightmapData()
    {
        LightmapData data = new LightmapData();
        data.lightmapColor = this.lightmapColor;
        data.lightmapDir = this.lightmapDir;
        data.shadowMask = this.shadowMask;
        return data;
    }

	public bool IsSame(LightmapData data)
    {
        return this.lightmapColor == data.lightmapColor &&
        this.lightmapDir == data.lightmapDir &&
        this.shadowMask == data.shadowMask;
    }

    public void Dispose()
    {
        this.lightmapColor = null;
        this.lightmapDir = null;
        this.shadowMask = null;
    }

    public void Clear()
    {
        this.Dispose();
    }
}
