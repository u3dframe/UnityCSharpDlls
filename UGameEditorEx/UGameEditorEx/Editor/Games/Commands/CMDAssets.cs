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
    static public void CleanupMissingScripts(GameObject gObj, bool isSave = true)
    {
        // We must use the GetComponents array to actually detect missing components
        var components = gObj.GetComponents<Component>();

        // Create a serialized UObject so that we can edit the component list
        var serializedObject = new SerializedObject(gObj);
        // Find the component list property
        var prop = serializedObject.FindProperty("m_Component");
        bool isChg = false;
        // Iterate over all components
        Component _comp;
        for (int j = components.Length - 1; j >= 0; j--)
        {
            // Check if the ref is null
            _comp = components[j];
            if (_comp == null)
            {
                prop.DeleteArrayElementAtIndex(j);
                isChg = true;
            }
        }

        if(isChg)
        {
            // Apply our changes to the game UObject
            serializedObject.ApplyModifiedProperties();
            if (gObj != null)
                EditorUtility.SetDirty(gObj); //这一行一定要加！！！
            if (isSave)
                AssetDatabase.SaveAssets(); //以及最后记得要保存资源的修改
        }
    }
    
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
            CleanupMissingScripts(gobjs[i]);
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