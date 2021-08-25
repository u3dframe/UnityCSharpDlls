using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
namespace Core.Art
{
    using UObject = UnityEngine.Object;
    /// <summary>
    /// 类名 : 命令 工具
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2020-11-24 11:40
    /// 功能 : 
    /// </summary>
    public static class ToolsByBuild
    {
        static public string m_rootRelative = "Builds";
        static public string m_rootRelativeDir = "Builds/";
        // 是 Component 或 GameObject 的子类 支持全部
        // 不是（如 Mesh 或 ScriptableObject），则仅支持 SelectionMode.ExcludePrefab 和 SelectionMode.Editable。
        static public SelectionMode m_smAssets = SelectionMode.Assets | SelectionMode.DeepAssets;

        static public bool IsFolder(string fn)
        {
            return Directory.Exists(fn);
        }

        static public string[] GetDependencies(string objAsset, bool recursive)
        {
            return AssetDatabase.GetDependencies(objAsset, recursive);
        }

        static public string GetAssetPath(UObject obj)
        {
            string _pIt = AssetDatabase.GetAssetPath(obj);
            _pIt = _pIt.Replace('\\', '/');
            return _pIt;
        }

        static public string GetFolder(UObject obj)
        {
            string _pIt = GetAssetPath(obj);
            if (!IsFolder(_pIt))
                _pIt = Path.GetDirectoryName(_pIt);
            return _pIt.Replace('\\', '/');
        }

        static public T[] GetSelectObject<T>()
        {
            return GetSelectObject<T>(m_smAssets);
        }

        static public T[] GetSelectObject<T>(SelectionMode mode)
        {
            return Selection.GetFiltered<T>(mode);
        }

        static public UObject[] GetSelObjects(SelectionMode mode)
        {
            return Selection.GetFiltered<UObject>(mode);
        }

        static public UObject[] GetSelObjects()
        {
            return GetSelObjects(SelectionMode.Assets);
        }

        static public void InsetList4Min(List<string> list, string src)
        {
            string min = src, max = src;
            bool _isHas = false;
            foreach (var item in list)
            {
                min = src;
                max = src;
                if (item.Length > src.Length)
                    max = item;
                else
                    min = item;

                if (max.Contains(min))
                {
                    _isHas = true;
                    break;
                }
            }
            if (_isHas)
            {
                if (min.Equals(src))
                {
                    list.Remove(max);
                    InsetList4Min(list, src);
                }
            }
            else
            {
                list.Add(src);
            }
        }

        static public List<string> GetSelectFolders(bool isCheckBuild = true)
        {
            List<string> _listFolders = new List<string>();
            var _objs = GetSelObjects();
            string _foder;
            foreach (var item in _objs)
            {
                _foder = GetFolder(item);
                if (isCheckBuild && !_foder.Contains(m_rootRelativeDir) && !_foder.EndsWith(m_rootRelative))
                    continue;
                InsetList4Min(_listFolders, _foder);
            }
            return _listFolders;
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

        static public string[] GetFiles(string fpdir, string ends)
        {
            ends = ends.ToLower();
            var _arrs3 = GetFiles(fpdir)
                .Where(s => ends.Contains(GetSuffixToLower(s))).ToArray();
            return _arrs3;
        }

        static public string[] GetRelativeAssetPaths(string filter, string[] searchInFolders)
        {
            // filter = t:Prefab , t:Material , t:Model , t:Texture
            string[] arrs = AssetDatabase.FindAssets(filter, searchInFolders);
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

        static public string[] GetSelectAssetPaths(string filter, bool isCheckBuild = true)
        {
            List<string> _listFolders = GetSelectFolders(isCheckBuild);
            if (_listFolders.Count <= 0)
                return null;
            string[] searchInFolders = _listFolders.ToArray();
            return GetRelativeAssetPaths(filter, searchInFolders);
        }

        static public GameObject[] GetSelectGobjs2()
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

                _rets = GetGobjsToArrs(_fdirs.ToArray());
                if(_rets != null)
                {
                    GameObject _gobj = null;
                    for (int i = 0; i < _rets.Length; i++)
                    {
                        _gobj = _rets[i];
                        if (!_list.Contains(_gobj))
                            _list.Add(_gobj);
                    }
                }
            }
            
            return _list.ToArray();
        }

        static public GameObject[] GetSelectGobjs(bool isCheckBuild = true)
        {
            List<GameObject> _listObjs = new List<GameObject>();
            List<string> _listFolders = GetSelectFolders(isCheckBuild);
            string[] searchInFolders = _listFolders.ToArray();
            var _arrGobjs = GetGobjsToArrs(searchInFolders);
            if(_arrGobjs != null)
            {
                GameObject _gobj = null;
                for (int i = 0; i < _arrGobjs.Length; i++)
                {
                    _gobj = _arrGobjs[i];
                    if (!_listObjs.Contains(_gobj))
                        _listObjs.Add(_gobj);
                }
            }
            return _listObjs.ToArray();
        }

        static public List<GameObject> GetGobjs(params string[] fpdir)
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
                if (_prefab != null)
                    _list.Add(_prefab);
            }
            return _list;
        }

        static public GameObject[] GetGobjsToArrs(params string[] fpdir)
        {
            List<GameObject> _list = GetGobjs(fpdir);
            if (_list == null || _list.Count <= 0)
                return null;
            GameObject[] _arrs = _list.ToArray();
            return _arrs;
        }
    }
}