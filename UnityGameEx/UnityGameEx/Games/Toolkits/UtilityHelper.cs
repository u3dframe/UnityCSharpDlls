using UnityEngine;
using UnityEngine.Playables;


/// <summary>
/// 类名 : 基础公用帮助脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-26 09:33
/// 功能 : 
/// </summary>
public class UtilityHelper : GHelper {
	/// <summary>
	/// 网络可用
	/// </summary>
	static public bool NetAvailable {
		get {
			return Application.internetReachability != NetworkReachability.NotReachable;
		}
	}

	/// <summary>
	/// 是否是无线
	/// </summary>
	static public bool IsWifi {
		get {
			return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
		}
	}
	
	static public void Log(string str) {
		Debug.Log(str);
	}

	static public void LogWarning(string str) {
		Debug.LogWarning(str);
	}

	static public void LogError(string str) {
		Debug.LogError(str);
	}

	static public int NMaxMore(params int[] vals){
		if(vals == null || vals.Length <= 0) return 0;
		int max = vals[0];
		for (int i = 1; i < vals.Length; i++)
		{
			if(max < vals[i]){
				max = vals[i];
			}
		}
		return max;
	}
	static public int NMax(int v1,int v2,int v3){
		return NMaxMore(v1,v2,v3);
	}

	static public int NMax(int v1,int v2,int v3,int v4){
		return NMaxMore(v1,v2,v3,v4);
	}

	static public double ToDecimal(double org,int acc,bool isRound)
    {
        double pow = 1;
        for (int i = 0; i < acc; i++) {
            pow *= 10;
        }

		double temp = org * pow;
		if(isRound){
			temp += 0.5;
		}

        return ((int)temp) / pow;
    }

	static public float Round(double org, int acc) {
        return (float) ToDecimal(org,acc,true);
    }

	static public float Round(float org, int acc) {
        return (float) ToDecimal(org,acc,true);
    }

    static public int Str2Int(string str)
    {
        int ret = 0;
        int.TryParse(str, out ret);
        return ret;
    }

    static public long Str2Long(string str)
    {
        long ret = 0;
        long.TryParse(str, out ret);
        return ret;
    }

    static public float Str2Float(string str)
    {
        float ret = 0;
        float.TryParse(str, out ret);
        return ret;
    }

    static public void SetMaxFrame(int maxFrame){
		Application.targetFrameRate = maxFrame;
	}

	static public bool IsGLife(object obj) {
		if(IsNull(obj))	return false;
		return obj is GobjLifeListener;
	}

	static public bool IsElement(object obj) {
		if(IsNull(obj))	return false;
		return obj is PrefabBasic;
	}

	static public Camera GetOrAddCamera(GameObject gobj){
		return Get<Camera>(gobj,true);
	}

	static public Camera GetOrAddCamera(Transform trsf){
		return Get<Camera>(trsf,true);
	}

	static public Animator GetOrAddAnimator(GameObject gobj){
		return Get<Animator>(gobj,true);
	}

	static public Animator GetOrAddAnimator(Transform trsf){
		return Get<Animator>(trsf,true);
	}

	static public PlayableDirector GetOrAddPlayableDirector(GameObject gobj)
	{
		return Get<PlayableDirector>(gobj, true);
	}

	static public PlayableDirector GetOrAddPlayableDirector(Transform trsf)
	{
		return Get<PlayableDirector>(trsf, true);
	}
}