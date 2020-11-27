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
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        int nLens = 0;
        if(importedAssets != null) nLens = importedAssets.Length;
        if(nLens > 0) {
            string _fn = null, _fnTower = null;
            foreach (string str in importedAssets) {
                if (!str.Contains(BuildPatcher.m_rootRelative))
                    continue;
                _fn = Path.GetFileName(str);
                _fnTower = _fn.ToLower();
                if (str.Contains(" ")) {
                    UnityEngine.Debug.LogErrorFormat("=== filename has space(空格), fp = [{0}],fn = [{1}]", str, _fn);
                }
                else if (!_fnTower.EndsWith(".fbx") && !_fn.Equals(_fnTower))
                {
                    UnityEngine.Debug.LogErrorFormat("=== filename has Upper(大写), fp = [{0}],fn = [{1}]", str, _fn);
                }
            }
        } 
    }
}