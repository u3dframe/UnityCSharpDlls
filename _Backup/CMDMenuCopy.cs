using UnityEditor;
using UnityEngine;

using System.IO;
using SDTime = System.DateTime;
// using UObject = UnityEngine.Object;

/// <summary>
/// 类名 : Menu 菜单Tools里面的命名
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-11-24 11:40
/// 功能 : 
/// </summary>
public static class CMDMenuCopy
{
	[MenuItem("Tools_Art/_Opts/CopyTexture",false,5)]
    static public void CMD_CopyTexture(){
        EditorUtility.DisplayProgressBar("CopyTexture", "Checking", 0.1f);
        string[] searchInFolders = {
            "Assets"
        };
        string[] _tes = AssetDatabase.FindAssets("t:Texture",searchInFolders);
        string _assetPath;
        int _len = _tes.Length;
        string _fp,_fpOut;
        string _fdNoAsset = Application.dataPath.Replace("Assets","");
        string _fdDesk = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + "/";
        string _fdOutRoot = string.Format("{0}CpTextrue_{1}/",_fdDesk,SDTime.UtcNow.ToString("MMddHHmmss"));
        try
        {
            for (int i = 0; i < _len; i++)
            {
                _assetPath = AssetDatabase.GUIDToAssetPath(_tes[i]);
                _fp = _fdNoAsset + _assetPath;
                _fpOut = _fdOutRoot + _assetPath;
                EditorUtility.DisplayProgressBar("CopyTexture ("+i+"/"+_len+")", _fp, i / (float)_len);
                var o = Directory.GetParent(_fpOut);
                o.Create();
                FileInfo fi = new FileInfo(_fp);
                fi.CopyTo(_fpOut);
            }
        }
        catch
        {
        }
        EditorUtility.ClearProgressBar();
        string _tt = "Cp To Folder = " + _fdOutRoot;
        EditorUtility.DisplayDialog("CopyTexture Finished", _tt, "Okey");
        Debug.LogErrorFormat("=====  {0}",_tt);
    }
}