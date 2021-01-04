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
#if UNITY_2019
    //递归物体的子物体
    static public bool SearchChild(GameObject gameObject)
    {
        int number = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
        bool _isBl = number > 0;
        Transform _trsf = gameObject.transform;
        if (_trsf.childCount > 0)
        {
            Transform _trsfChild;
            for (int i = 0; i < _trsf.childCount; i++)
            {
                _trsfChild = _trsf.GetChild(i);
                _isBl = _isBl || SearchChild(_trsfChild.gameObject);
            }
        }
        return _isBl;
    }

    [MenuItem("Assets/Tools/Cleanup GObj Missing Scripts", false, 20)]
    static void RemoveMonoBehavioursWithMissingScript()
    {
        var gobjs = BuildPatcher.GetSelectObject<GameObject>();
        if (gobjs == null || gobjs.Length <= 0)
        {
            Debug.LogError("=== RemoveMonoBehavioursWithMissingScript = is not select gobjs");
            return;
        }

        GameObject gobj,gobjClone;
        string _newPath;
        bool _isChg = false, _isChgAll = false;
        for (int i = gobjs.Length - 1; i >= 0; i--)
        {
            gobj = gobjs[i];
            _newPath = AssetDatabase.GetAssetPath(gobj);
            gobjClone = PrefabUtility.InstantiatePrefab(gobj) as GameObject;
            _isChg = SearchChild(gobjClone); // 递归删除
            if(_isChg)
            {
                PrefabUtility.SaveAsPrefabAsset(gobjClone, _newPath);
                gobjClone.hideFlags = HideFlags.HideAndDontSave;
            }
            _isChgAll = _isChgAll || _isChg;
            GameObject.DestroyImmediate(gobjClone);// 删除掉实例化的对象
        }
        if(_isChgAll)
            AssetDatabase.Refresh();
    }
#else
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
            else
            {
                Debug.LogErrorFormat("==== [{0}] = [{1}] = [{2}]", _comp, _comp.name, _comp.tag);
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
#endif
}