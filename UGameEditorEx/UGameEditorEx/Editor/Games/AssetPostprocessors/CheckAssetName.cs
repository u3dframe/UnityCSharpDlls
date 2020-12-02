using UnityEditor;
using System.IO;
 
 /// <summary>
/// 类名 : 检查资源名字的大小写和空格
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-11-26 12:23
/// 功能 : 
/// </summary>
public class CheckAssetName : AssetPostprocessor
{
    static bool IsExcludes(string strTower)
    {
        return strTower.EndsWith(".cs") || strTower.EndsWith(".meta") || strTower.EndsWith(".shader") || strTower.EndsWith(".tga") || strTower.EndsWith(".fbx") || strTower.EndsWith(".rendertexture") || strTower.Contains("/lightmaps/");
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        CheckAllAssets(importedAssets);
    }

    static public void CheckAllAssets(string[] importedAssets)
    {
        int nLens = 0;
        if (importedAssets != null) nLens = importedAssets.Length;
        if (nLens > 0)
        {
            string _fn = null, _fnTower = null, _fpStr = null;
            foreach (string str in importedAssets)
            {
                _fpStr = BuildPatcher.ReplaceSeparator(str);
                if (!_fpStr.Contains(BuildPatcher.m_rootRelative))
                    continue;

                if (BuildPatcher.IsFolder(_fpStr))
                    continue;

                _fn = Path.GetFileName(_fpStr);
                _fnTower = _fn.ToLower();
                if (_fpStr.Contains(" "))
                {
                    UnityEngine.Debug.LogErrorFormat("=== filename has space(空格), fp = [{0}],fn = [{1}]", _fpStr, _fn);
                }
                else if (!IsExcludes(_fpStr) && !_fn.Equals(_fnTower))
                {
                    UnityEngine.Debug.LogErrorFormat("=== filename has Upper(大写), fp = [{0}],fn = [{1}]", _fpStr, _fn);
                }
            }
        }
    }

    [MenuItem("Tools/Check AllRes Format(检查所有资源的命名)")]
    static public void ReCheckAll()
    {
        string _fd = BuildPatcher.m_appAssetPath;
        string[] files = Directory.GetFiles(_fd, "*.*", SearchOption.AllDirectories);
        CheckAllAssets(files);
    }
}