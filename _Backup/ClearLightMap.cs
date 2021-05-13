using UnityEngine;

public class ClearLightMap : MonoBehaviour
{
	[ContextMenu("Clear Lightmap")]
	void ClearLMap()
    {
        LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
        LightmapSettings.lightmaps = new LightmapData[0];
    }
}