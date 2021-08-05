using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
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
        // 是 Component 或 GameObject 的子类 支持全部
        // 不是（如 Mesh 或 ScriptableObject），则仅支持 SelectionMode.ExcludePrefab 和 SelectionMode.Editable。
        static public SelectionMode m_smAssets = SelectionMode.Assets | SelectionMode.DeepAssets;

        static public bool IsFolder(string fn)
        {
            return Directory.Exists(fn);
        }

        static public T[] GetSelectObject<T>()
        {
            return GetSelectObject<T>(m_smAssets);
        }

        static public T[] GetSelectObject<T>(SelectionMode mode)
        {
            return Selection.GetFiltered<T>(mode);
        }

        static public GameObject[] GetSelectGobjs()
        {
            GameObject[] _rets = GetSelectObject<GameObject>();
            List<GameObject> _list = new List<GameObject>();
            bool _isHas = (_rets != null && _rets.Length > 0);
            if (_isHas)
            {
                for (int i = 0; i < _rets.Length; i++)
                {
                    _list.Add(_rets[i]);
                }
            }

            var _guids = Selection.assetGUIDs;
            if (_guids != null && _guids.Length > 0)
            {
                List<string> _fdirs = new List<string>();
                string _fdir = null;
                for (int i = 0; i < _guids.Length; i++)
                {
                    _fdir = AssetDatabase.GUIDToAssetPath(_guids[i]);
                    if (Directory.Exists(_fdir))
                        _fdirs.Add(_fdir);
                }

                _rets = GetGobjs(_fdirs.ToArray());
                GameObject _gobj = null;
                for (int i = 0; i < _rets.Length; i++)
                {
                    _gobj = _rets[i];
                    if (!_list.Contains(_gobj))
                        _list.Add(_gobj);
                }
            }
            
            return _list.ToArray();
        }

        static public string[] GetFiles(string fpdir)
        {
            return Directory.GetFiles(fpdir, "*.*", SearchOption.AllDirectories);
        }

        static public string GetSuffixToLower(string path)
        {
            string _suffix = Path.GetExtension(path);
            return _suffix.ToLower();
        }

        static public string[] GetFiles(string fpdir,string ends)
        {
            ends = ends.ToLower();
            var _arrs3 = GetFiles(fpdir)
                .Where(s => ends.Contains(GetSuffixToLower(s))).ToArray();
            return _arrs3;
        }

        static public string[] GetRelativeAssetPaths(string filter, string[] searchInFolders)
        {
            // filter = t:Prefab , t:Material , t:Model , t:Texture
            string[] arrs = AssetDatabase.FindAssets(filter,searchInFolders);
            if (arrs == null || arrs.Length <= 0)
                return null;
            string _assetPath;
            List<string> _list = new List<string>();
            for (int i = 0; i < arrs.Length; i++)
            {
                _assetPath = AssetDatabase.GUIDToAssetPath(arrs[i]);
                _list.Add(_assetPath);
            }
            return _list.ToArray();
        }

        static public GameObject[] GetGobjs(params string[] fpdir)
        {
            string[] arrs = GetRelativeAssetPaths("t:Prefab", fpdir);
            if (arrs == null || arrs.Length <= 0)
                return null;
            string _prefabPath = "";
            GameObject _prefab = null;
            List<GameObject> _list = new List<GameObject>();
            for (int i = 0; i < arrs.Length; i++)
            {
                _prefabPath = arrs[i];
                _prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
                if(_prefab != null)
                    _list.Add(_prefab);
            }
            GameObject[] _arrs = _list.ToArray();
            return _arrs;
        }

        static public string[] GetDependencies(string objAsset, bool recursive)
        {
            return AssetDatabase.GetDependencies(objAsset, recursive);
        }
    }
}