using System.IO;
using UnityEditor;
using UnityEngine;
namespace Core.Art
{
    /// <summary>
    /// 类名 : 命令 工具
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2020-11-24 11:40
    /// 功能 : 
    /// </summary>
    public static class ToolsByBuild
    {
        static public string m_rootRelative = "Builds";

        static public bool IsFolder(string fn)
        {
            return Directory.Exists(fn);
        }

        static public T[] GetSelectObject<T>()
        {
            return Selection.GetFiltered<T>(SelectionMode.Assets | SelectionMode.DeepAssets);
        }

        static public T[] FindObjectsOfTypeAll<T>() where T : Object
        {
            return Resources.FindObjectsOfTypeAll<T>();
        }

        static public Object[] CollectDependencies(Object obj)
        {
            Object[] roots = new Object[] { obj };
            return EditorUtility.CollectDependencies(roots);
        }

        static public string[] GetDependencies(string objAsset, bool recursive)
        {

            return AssetDatabase.GetDependencies(objAsset, recursive);
        }
    }
}