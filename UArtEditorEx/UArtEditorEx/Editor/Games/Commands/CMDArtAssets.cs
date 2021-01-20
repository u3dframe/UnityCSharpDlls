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
            return IsInOrgPath(pOrg,pP1);
        }

        static bool IsInOrgPath(string pOrg, string pP1)
        {
            pP1 = pP1.Replace('\\', '/');
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
            EditorUtility.DisplayProgressBar("Tex To Mat", "Start ...", 0.0f);
            var arrs = ToolsByBuild.GetSelectObject<Texture2D>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Tex To Mat = is not select textures");
                EditorUtility.ClearProgressBar();
                return;
            }

            var _arrs2 = ToolsByBuild.GetFiles(Application.dataPath)
                .Where(s => s.ToLower().EndsWith(".mat")).ToArray();
            if (_arrs2 == null || _arrs2.Length <= 0)
            {
                Debug.LogError("=== Tex To Mat = is not has materials");
                EditorUtility.ClearProgressBar();
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
            EditorUtility.DisplayProgressBar("Shader To Mat", "Start ...", 0.0f);
            var arrs = ToolsByBuild.GetSelectObject<Shader>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Shader To Mat = is not select shaders");
                EditorUtility.ClearProgressBar();
                return;
            }

            var _arrs2 = ToolsByBuild.GetFiles(Application.dataPath)
                .Where(s => s.ToLower().EndsWith(".mat")).ToArray();
            if (_arrs2 == null || _arrs2.Length <= 0)
            {
                Debug.LogError("=== Shader To Mat = is not has materials");
                EditorUtility.ClearProgressBar();
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

        [MenuItem("Assets/Tools_Art/Find Use Shader's  All Prefabs")]
        static void FindAllPrefabs4Shader()
        {
            EditorUtility.DisplayProgressBar("Shader To Fab", "Start ...", 0.0f);
            var arrs = ToolsByBuild.GetSelectObject<Shader>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Shader To Fab = is not select shaders");
                EditorUtility.ClearProgressBar();
                return;
            }

            var _arrs2 = ToolsByBuild.GetFiles(Application.dataPath)
                .Where(s => s.ToLower().EndsWith(".mat")).ToArray();
            if (_arrs2 == null || _arrs2.Length <= 0)
            {
                Debug.LogError("=== Shader To Fab = is not has materials");
                EditorUtility.ClearProgressBar();
                return;
            }

            var _arrs3 = ToolsByBuild.GetFiles(Application.dataPath)
                .Where(s => s.ToLower().EndsWith(".prefab")).ToArray();

            StringBuilder _sb = new StringBuilder();
            string _pIt,_pIt2;
            Shader _sIt;
            for (int i = 0; i < arrs.Length; i++)
            {
                _sIt = arrs[i];
                EditorUtility.DisplayProgressBar("Shader To Mat", _sIt.name, (i + 1) / (float)arrs.Length);
                _sb.AppendFormat("=== begin = [{0}]", _sIt.name).AppendLine();
                for (int j = 0; j < _arrs2.Length; j++)
                {
                    _pIt = _arrs2[j];
                    _pIt = _pIt.Substring(_pIt.LastIndexOf("Assets"));
                    if (IsInOrgPath(_pIt, _sIt))
                    {
                        _sb.AppendLine(_pIt);
                        for (int k = 0; k < _arrs3.Length; k++)
                        {
                            _pIt2 = _arrs3[k];
                            _pIt2 = _pIt2.Substring(_pIt2.LastIndexOf("Assets"));
                            if (IsInOrgPath(_pIt2,_pIt))
                            {
                                _sb.AppendLine(_pIt2);
                            }
                        }
                    }
                }
                _sb.AppendFormat("=== end = [{0}]", _sIt.name).AppendLine();
                _sb.AppendLine();
            }
            EditorUtility.ClearProgressBar();
            _pIt = ReName(arrs[0]);
            _pIt = string.Format("{0}/../../S2F{1}_{2}.txt", Application.dataPath, _pIt, System.DateTime.Now.ToString("MMddHHmmss"));

            File.WriteAllText(_pIt, _sb.ToString());
            EditorUtility.DisplayDialog("Shader To Fab Finished", _pIt, "Okey");
        }

        [MenuItem("Assets/Tools_Art/Find Use Material's  All Prefabs")]
        static void FindAllPrefabs4Material()
        {
            EditorUtility.DisplayProgressBar("Mat To Fab", "Start ...", 0.0f);
            var arrs = ToolsByBuild.GetSelectObject<Material>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Mat To Fab = is not select materials");
                EditorUtility.ClearProgressBar();
                return;
            }

            var _arrs2 = ToolsByBuild.GetFiles(Application.dataPath)
                .Where(s => s.ToLower().EndsWith(".prefab")).ToArray();
            if (_arrs2 == null || _arrs2.Length <= 0)
            {
                Debug.LogError("=== Mat To Fab = is not has prefabs");
                EditorUtility.ClearProgressBar();
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
            EditorUtility.DisplayProgressBar("Fbx To Fab", "Start ...", 0.0f);
            var arrs = ToolsByBuild.GetSelectObject<Object>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Fbx To Fab = is not select fbx");
                EditorUtility.ClearProgressBar();
                return;
            }

            var _arrs2 = ToolsByBuild.GetFiles(Application.dataPath)
                .Where(s => s.ToLower().EndsWith(".prefab")).ToArray();
            if (_arrs2 == null || _arrs2.Length <= 0)
            {
                Debug.LogError("=== Fbx To Fab = is not has prefabs");
                EditorUtility.ClearProgressBar();
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
            EditorUtility.DisplayProgressBar("Rmv Mat Properties", "Start ...", 0.0f);
            var arrs = ToolsByBuild.GetSelectObject<Material>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Rmv Mat Properties = is not select materials");
                EditorUtility.ClearProgressBar();
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

        // [MenuItem("Assets/Tools_Art/Re - Optimization Anim Clip")]
        static void ReAnimClipOptimization()
        {
            EditorUtility.DisplayProgressBar("Optimization Anim Clip", "Start ...", 0.0f);
            var arrs = ToolsByBuild.GetSelectObject<AnimationClip>();
            if (arrs == null || arrs.Length <= 0)
            {
                Debug.LogError("=== Optimization Anim Clip = is not select clips");
                EditorUtility.ClearProgressBar();
                return;
            }
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

        static bool CleanupEmptyComp(GameObject gobj)
        {
            string _newPath = AssetDatabase.GetAssetPath(gobj);
            GameObject gobjClone = PrefabUtility.InstantiatePrefab(gobj) as GameObject;
            bool _isChg = false;
            var _arrs1 = gobjClone.GetComponentsInChildren<Animator>(true);
            foreach (var item in _arrs1)
            {
                if (item != null && item.runtimeAnimatorController == null)
                {
                    GameObject.DestroyImmediate(item);
                    _isChg = true;
                }
            }

            var _arrs2 = gobjClone.GetComponentsInChildren<Animation>(true);
            foreach (var item in _arrs2)
            {
                if (item != null && item.GetClipCount() <= 0)
                {
                    GameObject.DestroyImmediate(item);
                    _isChg = true;
                }
            }

            var _arrs3 = gobjClone.GetComponentsInChildren<UnityEngine.Playables.PlayableDirector>(true);
            foreach (var item in _arrs3)
            {
                if (item != null && item.playableAsset == null)
                {
                    GameObject.DestroyImmediate(item);
                    _isChg = true;
                }
            }

            var _arrs4 = gobjClone.GetComponentsInChildren<ParticleSystem>(true);
            Renderer _rer_ = null;
            foreach (var item in _arrs4)
            {
                if (item != null)
                {
                    _rer_ = item.GetComponent<Renderer>();
                    if(_rer_ == null || _rer_.sharedMaterial == null)
                    {
                        GameObject.DestroyImmediate(item);
                        _isChg = true;
                    }
                }
            }

            if (_isChg)
            {
                PrefabUtility.SaveAsPrefabAsset(gobjClone, _newPath);
                gobjClone.hideFlags = HideFlags.HideAndDontSave;
            }

            GameObject.DestroyImmediate(gobjClone); // 删除掉实例化的对象
            return _isChg;
        }

        [MenuItem("Assets/Tools_Art/Rmv Select Prefab's UnUsed Comp")]
        static void RemoveEmpty4Prefab()
        {
            EditorUtility.DisplayProgressBar("Remove Empty Comp", "Start ...", 0.0f);
            var arrs = ToolsByBuild.GetSelectObject<GameObject>();
            if (arrs == null || arrs.Length <= 0)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("=== Remove Empty Comp = is not select Prefabs");
                return;
            }
            EditorUtility.DisplayProgressBar("Remove Empty", "begin ...", 0);
            int _lens = arrs.Length;
            GameObject _it = null;
            for (int i = 0; i < _lens; i++)
            {
                _it = arrs[i];
                EditorUtility.DisplayProgressBar("Remove Empty", _it.name, (i + 1) / (float)_lens);
                CleanupEmptyComp(_it);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Remove Empty Finished", "this is over", "Okey");
        }
    }
}