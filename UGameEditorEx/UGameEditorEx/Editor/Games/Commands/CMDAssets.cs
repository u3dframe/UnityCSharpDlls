using UnityEditor;
using UnityEngine;

/// <summary>
/// 类名 : Assets 文件夹下面的命令
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-11-24 11:40
/// 功能 : 
/// </summary>
public static class CMDAssets
{
	 // [MenuItem("Tools/Cleanup Missing Scripts")]
    [MenuItem("Assets/Tools/Cleanup Missing Scripts",false,20)]
    static void CleanupMissingScripts()
    {
        var gobjs = BuildPatcher.GetSelectObject<GameObject>();
        if(gobjs == null || gobjs.Length <= 0)
        {
            Debug.LogError("=== CleanupMissingScripts = is not select gobjs");
            return;
        }
        for (int i = gobjs.Length - 1; i >= 0; i--)
        {
            BuildPatcher.CleanupMissingScripts(gobjs[i]);
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/Tools/Rmv Missing Scripts", false, 20)]
    static void RemoveMonoBehavioursWithMissingScript()
    {
        var gobjs = BuildPatcher.GetSelectObject<GameObject>();
        if (gobjs == null || gobjs.Length <= 0)
        {
            Debug.LogError("=== RemoveMonoBehavioursWithMissingScript = is not select gobjs");
            return;
        }

        GameObject gobj;
        for (int i = gobjs.Length - 1; i >= 0; i--)
        {
            gobj = gobjs[i];
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gobjs[i]);
            BuildPatcher.SaveAssets(gobj);
        }
        AssetDatabase.Refresh();
    }
}