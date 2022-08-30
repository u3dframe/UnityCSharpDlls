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
    [MenuItem("Tools_Art/_Opts/CopyTexture", false, 5)]
    static public void CMD_CopyTexture()
    {
        EditorUtility.DisplayProgressBar("CopyTexture", "Checking", 0.1f);
        string[] searchInFolders = {
            "Assets"
        };
        string[] _tes = AssetDatabase.FindAssets("t:Texture", searchInFolders);
        string _assetPath;
        int _len = _tes.Length;
        string _fp, _fpOut;
        string _fdNoAsset = Application.dataPath.Replace("Assets", "");
        string _fdDesk = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + "/";
        string _fdOutRoot = string.Format("{0}CpTextrue_{1}/", _fdDesk, SDTime.UtcNow.ToString("MMddHHmmss"));
        try
        {
            for (int i = 0; i < _len; i++)
            {
                _assetPath = AssetDatabase.GUIDToAssetPath(_tes[i]);
                _fp = _fdNoAsset + _assetPath;
                _fpOut = _fdOutRoot + _assetPath;
                EditorUtility.DisplayProgressBar("CopyTexture (" + i + "/" + _len + ")", _fp, i / (float)_len);
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
        Debug.LogErrorFormat("=====  {0}", _tt);
    }

    [MenuItem("Tools_Art/_Opts/RecordLua", false, 5)]
    static public void CMD_RecordLua()
    {
        EditorUtility.DisplayProgressBar("RecordLua", "Checking", 0.1f);
        string fpdir = "Assets";
        string[] _arrs = Directory.GetFiles(fpdir, "*.lua", SearchOption.AllDirectories);
        int _len = _arrs.Length;
        System.Text.StringBuilder _sbd = new System.Text.StringBuilder();
        _sbd.Append("return {").AppendLine();
        // _sbd.Append("Check Spine Json file").AppendLine();
        string _it, _it2;
        for (int i = 0; i < _len; i++)
        {
            _it = _arrs[i].Replace("\\", "/");
            _it2 = _it.Substring(_it.IndexOf("Lua/") + 4).Replace(".lua","");
            _sbd.Append("\"").Append(_it2).Append("\",").AppendLine();
        }
        _sbd.Append("}").AppendLine();
        EditorUtility.ClearProgressBar();

        string _cont = _sbd.ToString();
        _sbd.Clear();
        _sbd.Length = 0;

        string _fdir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + "/";
        string _fp = string.Format("{0}lua_files_{1}.lua", _fdir, SDTime.UtcNow.ToString("MMddHHmmss"));
        File.WriteAllText(_fp, _cont);

        EditorUtility.DisplayDialog("RecordLua Finished", _fp, "Okey");
        Debug.LogErrorFormat("=====  {0}", _fp);
    }
}