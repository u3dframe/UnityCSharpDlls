using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
namespace Core.Art
{
    /// <summary>
    /// 类名 : Assets 文件夹下面的命令
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2020-11-24 11:40
    /// 功能 : 
    /// </summary>
    public static class CMDArtAssets
    {
        [MenuItem("Assets/Tools_Art/Check Files Is Has Space")]
        static void CheckHasSpace()
        {
            Object[] _arrs = ToolsByBuild.GetSelectObject<Object>();
            string _fp;
            float _lens_ = _arrs.Length;
            int _curr_ = 0;

            EditorUtility.DisplayProgressBar("CheckHasSpace", "Start ...", 0.0f);
            foreach (Object _obj_ in _arrs)
            {
                _fp = AssetDatabase.GetAssetPath(_obj_);
                _curr_++;
                EditorUtility.DisplayProgressBar(string.Format("CheckHasSpace - ({0}/{1})", _curr_, _lens_), _fp, (_curr_ / _lens_));
                if (_fp.Contains(" "))
                {
                    Debug.LogErrorFormat("====== has space,fp = [{0}]", _fp);
                }
            }
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("Assets/Tools_Art/Find Use Texture's  All Materials", false, 20)]
        static void FindAllMaterials4Texture()
        {
            var arrs = ToolsByBuild.GetSelectObject<Texture2D>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Tex To Mat = is not select textures");
                return;
            }

            Material[] _arrs2 = ToolsByBuild.FindObjectsOfTypeAll<Material>();
            if (_arrs2 == null || _arrs2.Length <= 0)
            {
                Debug.LogError("=== Tex To Mat = is not has materials");
                return;
            }

            StringBuilder _sb = new StringBuilder();
            Dictionary<string, bool> _dic = new Dictionary<string, bool>();
            string _pIt;
            Material _it;
            for (int i = 0; i < arrs.Length; i++)
            {
                for (int j = 0; j < _arrs2.Length; j++)
                {
                    _it = _arrs2[j];
                    if (IsTexInMat(_it, arrs[i]))
                    {
                        _pIt = AssetDatabase.GetAssetPath(_it);
                        if (_dic.ContainsKey(_pIt))
                            continue;
                        _dic.Add(_pIt, true);
                        _sb.AppendLine(_pIt);
                    }
                }
            }

            Texture2D _first = arrs[0];
            _pIt = string.Format("{0}/../../T2M_{1}.text", Application.dataPath, _first.name);
            File.WriteAllText(_pIt, _sb.ToString());
            EditorUtility.DisplayDialog("Tex To Mat Finished", _pIt, "Okey");
        }

        static bool IsTexInMat(Material org, Texture2D p1)
        {
            string pP1 = AssetDatabase.GetAssetPath(p1);

            string pOrg = AssetDatabase.GetAssetPath(org);
            string[] pArrs = ToolsByBuild.GetDependencies(pOrg,true);
            foreach (string item in pArrs)
            {
                if (item.Equals(pP1))
                        return true;
            }
            return false;
        }

        [MenuItem("Assets/Tools_Art/Find Use Material's  All Prefabs", false, 20)]
        static void FindAllPrefabs4Material()
        {
            var arrs = ToolsByBuild.GetSelectObject<Material>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Mat To Fab = is not select materials");
                return;
            }

            GameObject[] _arrs2 = ToolsByBuild.FindObjectsOfTypeAll<GameObject>();
            if (_arrs2 == null || _arrs2.Length <= 0)
            {
                Debug.LogError("=== Mat To Fab = is not has prefabs");
                return;
            }

            StringBuilder _sb = new StringBuilder();
            Dictionary<string, bool> _dic = new Dictionary<string, bool>();
            string _pIt;
            GameObject _it;
            for (int i = 0; i < arrs.Length; i++)
            {
                for (int j = 0; j < _arrs2.Length; j++)
                {
                    _it = _arrs2[j];
                    if (IsInPrefab(_it, arrs[i]))
                    {
                        _pIt = AssetDatabase.GetAssetPath(_it);
                        if (_dic.ContainsKey(_pIt))
                            continue;
                        _dic.Add(_pIt, true);
                        _sb.AppendLine(_pIt);
                    }
                }
            }

            Material _first = arrs[0];
            _pIt = string.Format("{0}/../../M2P_{1}.text", Application.dataPath, _first.name);
            File.WriteAllText(_pIt, _sb.ToString());
            EditorUtility.DisplayDialog("Mat To Fab Finished", _pIt, "Okey");
        }

        static bool IsInPrefab(GameObject org, Object p1)
        {
            string pP1 = AssetDatabase.GetAssetPath(p1);

            string pOrg = AssetDatabase.GetAssetPath(org);
            string[] pArrs = ToolsByBuild.GetDependencies(pOrg,true);
            foreach (string item in pArrs)
            {
                if (item.Equals(pP1))
                        return true;
            }
            return false;
        }

        [MenuItem("Assets/Tools_Art/Find Use Fbx's  All Prefabs", false, 20)]
        static void FindAllPrefabs4Fbx()
        {
            var arrs = ToolsByBuild.GetSelectObject<GameObject>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Fbx To Fab = is not select materials");
                return;
            }

            GameObject[] _arrs2 = ToolsByBuild.FindObjectsOfTypeAll<GameObject>();
            if (_arrs2 == null || _arrs2.Length <= 0)
            {
                Debug.LogError("=== Fbx To Fab = is not has prefabs");
                return;
            }

            StringBuilder _sb = new StringBuilder();
            Dictionary<string, bool> _dic = new Dictionary<string, bool>();
            string _pIt;
            GameObject _it;
            for (int i = 0; i < arrs.Length; i++)
            {
                for (int j = 0; j < _arrs2.Length; j++)
                {
                    _it = _arrs2[j];
                    _pIt = AssetDatabase.GetAssetPath(_it);
                    if (!_pIt.EndsWith(".prefab",System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (IsInPrefab(_it, arrs[i]))
                    {
                        if (_dic.ContainsKey(_pIt))
                            continue;
                        _dic.Add(_pIt, true);
                        _sb.AppendLine(_pIt);
                    }
                }
            }

            GameObject _first = arrs[0];
            _pIt = AssetDatabase.GetAssetPath(_first);
            _pIt = _pIt.Substring(_pIt.LastIndexOf(ToolsByBuild.m_rootRelative)+ ToolsByBuild.m_rootRelative.Length);
            _pIt = _pIt.Replace('\\', '/');
            _pIt = _pIt.Replace('/', '_');
            _pIt = Path.GetFileNameWithoutExtension(_pIt);
            _pIt = string.Format("{0}/../../F2P{1}.text", Application.dataPath,_pIt);
            File.WriteAllText(_pIt, _sb.ToString());
            EditorUtility.DisplayDialog("Fbx To Fab Finished", _pIt, "Okey");
        }
    }
}