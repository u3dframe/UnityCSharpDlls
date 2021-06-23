using UnityEngine;
using LitJson;

public class SceneInfoTest : MonoBehaviour
{
	[ContextMenu("SceneInfos")]
	void SceneInfos()
    {
        _SaveReflectionProbe();
    }
	
	JsonData NewJObj(){
		return LJsonHelper.NewJObj();
	}

	JsonData NewJArr(){
		return LJsonHelper.NewJArr();
	}
	
	void _SaveReflectionProbe()
    {
		JsonData _jdRoot = NewJObj();
        var _list = NewJArr();
        ReflectionProbe[] _arrs = null;
        Texture _brake = null;
        JsonData _jdTemp = null;
        Vector3 _v3Temp;
        int _maxL = 0;
		string _key = null;
        _arrs = this.gameObject.GetComponentsInChildren<ReflectionProbe>();	
		foreach (var item in _arrs)
		{
			_brake = item.bakedTexture;
			if (_brake != null)
			{
				_jdTemp = NewJObj();
				_key = _brake.name;
				_jdTemp["importance"] = item.importance;
				_jdTemp["intensity"] = item.intensity.ToString();
				_v3Temp = item.center;
				_jdTemp["center_x"] = _v3Temp.x.ToString();
				_jdTemp["center_y"] = _v3Temp.y.ToString();
				_jdTemp["center_z"] = _v3Temp.z.ToString();
				_v3Temp = item.size;
				_jdTemp["size_x"] = _v3Temp.x.ToString();
				_jdTemp["size_y"] = _v3Temp.y.ToString();
				_jdTemp["size_z"] = _v3Temp.z.ToString();
				_jdTemp["rp_exr"] = _key;
				_jdTemp["g_name"] = item.name;
				
				_jdRoot[_key] = _jdTemp;
				_list.Add(_key);
				_maxL++;
			}
		}
		
		bool isHasRP_Environment = (RenderSettings.defaultReflectionMode == UnityEngine.Rendering.DefaultReflectionMode.Skybox) || (RenderSettings.customReflection != null);
        if(isHasRP_Environment)
        {
            _jdTemp = NewJObj();
            _jdTemp["isEnvironment"] = true;
            _jdTemp["defaultReflectionResolution"] = RenderSettings.defaultReflectionResolution;
            _jdTemp["reflectionBounces"] = RenderSettings.reflectionBounces;
            _jdTemp["reflectionIntensity"] = RenderSettings.reflectionIntensity.ToString();
            if (RenderSettings.customReflection != null)
                _key = RenderSettings.customReflection.name;
            else
                _key = "ReflectionProbe-" + _maxL;
            _jdTemp["rp_exr"] = _key;

            _jdRoot[_key] = _jdTemp;
            _jdRoot["environment"] = _key;
            _list.Add(_key);
        }
		
		int _lens = _list.Count;
        if (_lens > 0)
        {
            _jdRoot["lens"] = _lens;
            _jdRoot["list"] = _list;
        }
		
		string _json = _jdRoot.ToJson();
		Debug.Log(_json);
	}
	
	[ContextMenu("LoadProbes In Resources")]
    void LoadProbes()
    {
        // var probes  = Resources.Load<LightProbes>("maincity_probes");
        // LightmapSettings.lightProbes = probes;
        LightProbes.Tetrahedralize();
        var bakedProbes  = Resources.Load<LProbeData>("maincity_probes1");
        Debug.Log(LightmapSettings.lightProbes);
        if(bakedProbes != null && LightmapSettings.lightProbes != null)
            LightmapSettings.lightProbes.bakedProbes = bakedProbes.lightProbes;
    }
}