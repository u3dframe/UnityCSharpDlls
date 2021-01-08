using UnityEngine;
using System.Collections;
using System.IO;

namespace Core.Kernel
{
    using UObject = UnityEngine.Object;
    using UResources = UnityEngine.Resources;

    /// <summary>
    /// 类名 : 读取 Resources 文件夹下面的资源
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2017-03-07 09:29
    /// 功能 : 
    /// </summary>
    public partial class Resources : UGameFile
    {
#if UNITY_EDITOR
        static public string Path2AssetsStart(string fp)
        {
            fp = ReplaceSeparator(fp);
            if (fp.Contains(m_appAssetPath))
            {
                fp = _AssetsStart(fp);
            }
            return fp;
        }
		
		// AssetDatabase 取得的 Path 都是 AssetPath [以 Assets/ 开头路径]
        static public string GetPath(UObject obj)
        {
            return UnityEditor.AssetDatabase.GetAssetPath(obj);
        }

        static public T GetObject<T>(string assetPath,string suffix = "") where T : UObject
        {
             // 去掉第一个Assets文件夹路径
			assetPath = _AssetsStart(assetPath);
			string suffix2 = Path.GetExtension (assetPath);
			if (string.IsNullOrEmpty (suffix2) && !string.IsNullOrEmpty (suffix)) {
				assetPath += suffix;
			}
			return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

		static public UObject GetObject(string assetPath,string suffix = "")
        {
            // 去掉第一个Assets文件夹路径
			return GetObject<UObject>(assetPath,suffix);
        }

        static public T LoadInEditor<T>(string abName,string assetName) where T : UObject{
            if (string.IsNullOrEmpty (abName))
                return null;
            string _fp = m_appAssetPath;
            bool _isFab = false , _isLmap =  false;
            if(abName.Contains("/effects/")  || abName.Contains("/uis/")  || abName.Contains("/ef_") || abName.Contains("/special_effects/")){
                _fp  += "Effects/Builds/";
            }else if(abName.Contains("/c_")){
                _fp  += "Characters/Builds/";
            }else if(abName.Contains("timeline/")){
                _fp  += "Characters/Builds/";
            }else if(abName.Contains("/maps/") || abName.Contains("/explores/") || abName.Contains("post_process/") || abName.Contains("skyboxs/")){
                _fp  += "Scene/Builds/";
            }else if(abName.Contains("lightmaps/")){
                _fp  += "Scene/Builds/";
                _isLmap = true;
            }else{
                _fp += "Builds/";
            }

            _isFab = abName.EndsWith(m_strUI) || abName.EndsWith(m_strFab);
            _fp += GetPathNoSuffix(abName);
            if(_isFab){
                return Load4Develop<T>(_fp,m_suffix_fab);
            }else if(abName.EndsWith(m_strAtlas)){
                _fp += GetPathNoSuffix(assetName);
                return Load4Develop<T>(_fp,m_suffix_png);
            }else if(abName.EndsWith(m_strTex2D)){
                return Load4Develop<T>(_fp,m_suffix_png);
            }else if(_isLmap){
                string _suffix = GetSuffix(assetName);
                _fp += "/" + GetPathNoSuffix(assetName);
                if(string.IsNullOrEmpty(_suffix))
                    _suffix = m_suffix_png;
                return Load4Develop<T>(_fp,_suffix);
             }else if(abName.EndsWith(m_strMat)){
                return Load4Develop<T>(_fp,m_suffix_mat);
             }else if(abName.EndsWith(m_strScriptable)){
                return Load4Develop<T>(_fp,m_suffix_scriptable);
             }
            return null;
        }
#endif

        static public T Load4Develop<T>(string path, string suffix) where T : UObject
        {
            T _ret_ = null;
            path = ReplaceSeparator(path);
#if UNITY_EDITOR
			int index = path.LastIndexOf (m_fnResources);
			if(index < 0){
				_ret_ = GetObject<T>(path,suffix);
			}
#endif

            if (_ret_ == null)
            {
                _ret_ = LoadInResources<T>(path);
            }
            return _ret_;
        }

        /// <summary>
        /// Load the specified path.
        /// </summary>
        /// <param name="path">相对路径(有无后缀都可以处理)</param>
        static public UObject Load4Develop(string path, string suffix)
        {
            UObject ret = null;
            path = ReplaceSeparator(path);
#if UNITY_EDITOR
			int index = path.LastIndexOf (m_fnResources);
			if(index < 0){
				ret = GetObject(path,suffix);
			}
#endif

            if (ret == null)
            {
                ret = LoadInResources(path);
            }
            return ret;
        }

        static public UObject Load4Develop(string path)
        {
            return Load4Develop(path, null);
        }
    }
}