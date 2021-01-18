using UnityEngine;
using Core.Kernel;
namespace Core
{	
	/// <summary>
	/// 类名 : shader 记录
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2020-05-26 13:29
	/// 功能 : 
	/// </summary>
	public static class ABShader{
		static ListDict<AssetInfo> m_dicShader = new ListDict<AssetInfo>(false);
		
		static public void AddLoaded(AssetInfo asInfo){
			if(asInfo == null || !asInfo.isHasObj || asInfo.m_objType != UGameFile.tpShader)
				return;
			Shader _sd = asInfo.GetObject<Shader>();
			if(_sd == null)
				return;
			
			string _key = _sd.name;
			m_dicShader.Remove(_key);
			m_dicShader.Add(_key,asInfo);
		}

		static public Shader FindShader(string shaderName){
            bool _useFind = UGameFile.m_isEditor;
            AssetInfo asInfo = null;
            if (!_useFind)
            {
                asInfo = m_dicShader.Get(shaderName);
                _useFind = (asInfo == null || !asInfo.isHasObj);
            }
			// Debug.LogFormat("========= shader name =[{0}] = [{1}]" , shaderName,_useFind);
			if(_useFind)
				return Shader.Find(shaderName);
			return asInfo.GetObject<Shader>();
		}
	}
}