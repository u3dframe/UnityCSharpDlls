using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Linq;
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
        [MenuItem("Assets/Tools_Art/Check Files Is Has Space", false, 99)]
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

        static string ReName(Object obj)
        {
            string _pIt = AssetDatabase.GetAssetPath(obj);
            _pIt = _pIt.Substring(_pIt.LastIndexOf(ToolsByBuild.m_rootRelative) + ToolsByBuild.m_rootRelative.Length);
            _pIt = _pIt.Replace('\\', '/');
            _pIt = _pIt.Replace('/', '_');
            return Path.GetFileNameWithoutExtension(_pIt);
        }

        static bool IsInOrgPath(string pOrg, Object p1)
        {
            string pP1 = AssetDatabase.GetAssetPath(p1);
            string[] pArrs = ToolsByBuild.GetDependencies(pOrg, true);
            foreach (string item in pArrs)
            {
                if (item.Equals(pP1))
                    return true;
            }
            return false;
        }

        [MenuItem("Assets/Tools_Art/Find Use Texture's  All Materials")]
        static void FindAllMaterials4Texture()
        {
            var arrs = ToolsByBuild.GetSelectObject<Texture2D>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Tex To Mat = is not select textures");
                return;
            }

            var _arrs2 = ToolsByBuild.GetFiles(Application.dataPath)
                .Where(s => s.ToLower().EndsWith(".mat")).ToArray();
            if (_arrs2 == null || _arrs2.Length <= 0)
            {
                Debug.LogError("=== Tex To Mat = is not has materials");
                return;
            }

            StringBuilder _sb = new StringBuilder();
            string _pIt;
            for (int i = 0; i < arrs.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Tex To Mat",arrs[i].name, (i + 1) / (float)arrs.Length);
                _sb.AppendFormat("=== begin = [{0}]", arrs[i].name).AppendLine();
                for (int j = 0; j < _arrs2.Length; j++)
                {
                    _pIt = _arrs2[j];
                    _pIt = _pIt.Substring(_pIt.LastIndexOf("Assets"));
                    if (IsInOrgPath(_pIt, arrs[i]))
                    {
                        _sb.AppendLine(_pIt);
                    }
                }
                _sb.AppendFormat("=== end = [{0}]", arrs[i].name).AppendLine();
                _sb.AppendLine();
            }
            EditorUtility.ClearProgressBar();
            _pIt = ReName(arrs[0]);
            _pIt = string.Format("{0}/../../T2M{1}_{2}.txt", Application.dataPath, _pIt, System.DateTime.Now.ToString("MMddHHmmss"));

            File.WriteAllText(_pIt, _sb.ToString());
            EditorUtility.DisplayDialog("Tex To Mat Finished", _pIt, "Okey");
        }
        
        [MenuItem("Assets/Tools_Art/Find Use Shader's  All Materials")]
        static void FindAllMaterials4Shader()
        {
            var arrs = ToolsByBuild.GetSelectObject<Shader>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Shader To Mat = is not select shaders");
                return;
            }

            var _arrs2 = ToolsByBuild.GetFiles(Application.dataPath)
                .Where(s => s.ToLower().EndsWith(".mat")).ToArray();
            if (_arrs2 == null || _arrs2.Length <= 0)
            {
                Debug.LogError("=== Shader To Mat = is not has materials");
                return;
            }

            StringBuilder _sb = new StringBuilder();
            string _pIt;
            for (int i = 0; i < arrs.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Shader To Mat", arrs[i].name, (i + 1) / (float)arrs.Length);
                _sb.AppendFormat("=== begin = [{0}]", arrs[i].name).AppendLine();
                for (int j = 0; j < _arrs2.Length; j++)
                {
                    _pIt = _arrs2[j];
                    _pIt = _pIt.Substring(_pIt.LastIndexOf("Assets"));
                    if (IsInOrgPath(_pIt, arrs[i]))
                    {
                        _sb.AppendLine(_pIt);
                    }
                }
                _sb.AppendFormat("=== end = [{0}]", arrs[i].name).AppendLine();
                _sb.AppendLine();
            }
            EditorUtility.ClearProgressBar();
            _pIt = ReName(arrs[0]);
            _pIt = string.Format("{0}/../../S2M{1}_{2}.txt", Application.dataPath, _pIt, System.DateTime.Now.ToString("MMddHHmmss"));

            File.WriteAllText(_pIt, _sb.ToString());
            EditorUtility.DisplayDialog("Shader To Mat Finished", _pIt, "Okey");
        }

        [MenuItem("Assets/Tools_Art/Find Use Material's  All Prefabs")]
        static void FindAllPrefabs4Material()
        {
            var arrs = ToolsByBuild.GetSelectObject<Material>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Mat To Fab = is not select materials");
                return;
            }

            var _arrs2 = ToolsByBuild.GetFiles(Application.dataPath)
                .Where(s => s.ToLower().EndsWith(".prefab")).ToArray();
            if (_arrs2 == null || _arrs2.Length <= 0)
            {
                Debug.LogError("=== Mat To Fab = is not has prefabs");
                return;
            }

            StringBuilder _sb = new StringBuilder();
            string _pIt;
            for (int i = 0; i < arrs.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Mat To Fab", arrs[i].name, (i + 1) / (float)arrs.Length);
                _sb.AppendFormat("=== begin = [{0}]", arrs[i].name).AppendLine();
                for (int j = 0; j < _arrs2.Length; j++)
                {
                    _pIt = _arrs2[j];
                    _pIt = _pIt.Substring(_pIt.LastIndexOf("Assets"));
                    if (IsInOrgPath(_pIt, arrs[i]))
                    {
                        _sb.AppendLine(_pIt);
                    }
                }
                _sb.AppendFormat("=== end = [{0}]", arrs[i].name).AppendLine();
                _sb.AppendLine();
            }
            EditorUtility.ClearProgressBar();
            _pIt = ReName(arrs[0]);
            _pIt = string.Format("{0}/../../M2P{1}_{2}.txt", Application.dataPath, _pIt, System.DateTime.Now.ToString("MMddHHmmss"));
            File.WriteAllText(_pIt, _sb.ToString());
            EditorUtility.DisplayDialog("Mat To Fab Finished", _pIt, "Okey");
        }
        
        [MenuItem("Assets/Tools_Art/Find Use Fbx's  All Prefabs")]
        static void FindAllPrefabs4Fbx()
        {
            var arrs = ToolsByBuild.GetSelectObject<Object>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Fbx To Fab = is not select fbx");
                return;
            }

            var _arrs2 = ToolsByBuild.GetFiles(Application.dataPath)
                .Where(s => s.ToLower().EndsWith(".prefab")).ToArray();
            if (_arrs2 == null || _arrs2.Length <= 0)
            {
                Debug.LogError("=== Fbx To Fab = is not has prefabs");
                return;
            }

            StringBuilder _sb = new StringBuilder();
            string _pIt;
            for (int i = 0; i < arrs.Length; i++)
            {
                _pIt = AssetDatabase.GetAssetPath(arrs[i]);
                if (!_pIt.EndsWith(".fbx",System.StringComparison.OrdinalIgnoreCase))
                    continue;
                EditorUtility.DisplayProgressBar("Fbx To Fab", arrs[i].name, (i + 1) / (float)arrs.Length);
                _sb.AppendFormat("=== begin = [{0}]", arrs[i].name).AppendLine();
                for (int j = 0; j < _arrs2.Length; j++)
                {
                    _pIt = _arrs2[j];
                    _pIt = _pIt.Substring(_pIt.LastIndexOf("Assets"));
                    // if (!_pIt.EndsWith(".prefab",System.StringComparison.OrdinalIgnoreCase))
                    //     continue;

                    if (IsInOrgPath(_pIt, arrs[i]))
                    {
                        _sb.AppendLine(_pIt);
                    }
                }
                _sb.AppendFormat("=== end = [{0}]", arrs[i].name).AppendLine();
                _sb.AppendLine();
            }
            EditorUtility.ClearProgressBar();
            _pIt = ReName(arrs[0]);
            _pIt = string.Format("{0}/../../F2P{1}_{2}.txt", Application.dataPath,_pIt,System.DateTime.Now.ToString("MMddHHmmss"));
            File.WriteAllText(_pIt, _sb.ToString());
            EditorUtility.DisplayDialog("Fbx To Fab Finished", _pIt, "Okey");
        }

        // [MenuItem("Assets/Tools_Art/Rmv All Mat's OverdueProperties")]
        static void HandlerOverdueMatProperties()
        {
            EditorUtility.DisplayProgressBar("Rmv Mat Properties","begin ...",0);
            string[] searchInFolders = new string[]
            {
                "Assets/Builds",
                "Assets/Characters/Builds/materials",
                "Assets/Effects/Builds/materials",
                "Assets/Scene/Builds/materials",
            };
            string[] arrs = AssetDatabase.FindAssets("t:Material", searchInFolders);
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Rmv Mat Properties = is not has materials");
                EditorUtility.ClearProgressBar();
                return;
            }
            string _it;
            int _lens = arrs.Length;
            for (int i = 0; i < _lens; i++)
            {
                _it = arrs[i];
                var path = AssetDatabase.GUIDToAssetPath(_it);
                EditorUtility.DisplayProgressBar("Rmv Mat Properties = " + _it, path, (i + 1) / (float)_lens);
                Material mat = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
                SerializedObject so = new SerializedObject(mat);
                SerializedProperty m_SavedProperties = so.FindProperty("m_SavedProperties");
                RemoveElement(mat, "m_TexEnvs", m_SavedProperties);
                RemoveElement(mat, "m_Floats", m_SavedProperties);
                RemoveElement(mat, "m_Colors", m_SavedProperties);
                so.ApplyModifiedProperties();
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Rmv Mat Properties Finished","Rmv Mat's OverdueProperties", "Okey");
        }

        static private void RemoveElement(Material mat, string spName, SerializedProperty saveProperty)
        {
            SerializedProperty property = saveProperty.FindPropertyRelative(spName);
            for (int i = property.arraySize - 1; i >= 0; i--)
            {
                var prop = property.GetArrayElementAtIndex(i);
                string propertyName = prop.displayName;
                if (!mat.HasProperty(propertyName))
                {
                    property.DeleteArrayElementAtIndex(i);
                }
            }
        }

        [MenuItem("Assets/Tools_Art/Rmv Select Mat's OverdueProperties")]
        static void HandlerOverdueMatProperties4Select()
        {
            var arrs = ToolsByBuild.GetSelectObject<Material>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Rmv Mat Properties = is not select materials");
                return;
            }
            EditorUtility.DisplayProgressBar("Rmv Mat Properties", "begin ...", 0);
            int _lens = arrs.Length;
            for (int i = 0; i < _lens; i++)
            {
                Material mat = arrs[i];
                EditorUtility.DisplayProgressBar("Rmv Mat Properties",mat.name, (i + 1) / (float)_lens);
                SerializedObject so = new SerializedObject(mat);
                SerializedProperty m_SavedProperties = so.FindProperty("m_SavedProperties");
                RemoveElement(mat, "m_TexEnvs", m_SavedProperties);
                RemoveElement(mat, "m_Floats", m_SavedProperties);
                RemoveElement(mat, "m_Colors", m_SavedProperties);
                so.ApplyModifiedProperties();
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Rmv Mat Properties Finished", "Rmv Mat's OverdueProperties", "Okey");
        }

        static public void HandlerAnimationClip(AnimationClip theAnimation, bool isRvScale = true, string assetPath = "")
        {
            try
            {
                //去除scale曲线
                foreach (EditorCurveBinding theCurveBinding in AnimationUtility.GetCurveBindings(theAnimation))
                {
                    string name = theCurveBinding.propertyName.ToLower();
                    if (isRvScale && name.Contains("scale"))
                    {
                        AnimationUtility.SetEditorCurve(theAnimation, theCurveBinding, null);
                        continue;
                    }

                    // 浮点数精度压缩到 - f3
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(theAnimation, theCurveBinding);
                    Keyframe key;
                    for (int ii = 0; ii < curve.length; ++ii)
                    {
                        key = curve[ii];
                        key.value = float.Parse(key.value.ToString("f3"));
                        key.inTangent = float.Parse(key.inTangent.ToString("f3"));
                        key.outTangent = float.Parse(key.outTangent.ToString("f3"));
                    }
                    AnimationUtility.SetEditorCurve(theAnimation, theCurveBinding, curve);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat("HandleAnimationClip Failed !!! animationPath : {0} error: {1}", assetPath, e);
            }
        }

        [MenuItem("Assets/Tools_Art/Re - Optimization Anim Clip")]
        static void ReAnimClipOptimization()
        {
            var arrs = ToolsByBuild.GetSelectObject<AnimationClip>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Optimization Anim Clip = is not select clips");
                return;
            }
            EditorUtility.DisplayProgressBar("Optimization Anim Clip", "begin ...", 0);
            int _lens = arrs.Length;
            AnimationClip _it = null;
            string _assetPath = null;
            for (int i = 0; i < _lens; i++)
            {
                _it = arrs[i];
                EditorUtility.DisplayProgressBar("Optimization Anim Clip", _it.name, (i + 1) / (float)_lens);
                _assetPath = AssetDatabase.GetAssetPath(_it);
                HandlerAnimationClip(_it, false, _assetPath);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Optimization Anim Clip Finished", "this is over", "Okey");
        }
    }
}