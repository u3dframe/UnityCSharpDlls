using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
        static public System.Func<string, int, int> funcCompressionRate = null; // 外部计算压缩比例 1 - 100

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
            return IsInOrgPath(pOrg, pP1);
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
                EditorUtility.DisplayProgressBar("Tex To Mat", arrs[i].name, (i + 1) / (float)arrs.Length);
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
            string _pIt, _pIt2;
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
                            if (IsInOrgPath(_pIt2, _pIt))
                            {
                                _sb.AppendLine(_pIt2);
                            }
                        }
                        _sb.AppendLine();
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
                if (!_pIt.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
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
            _pIt = string.Format("{0}/../../F2P{1}_{2}.txt", Application.dataPath, _pIt, System.DateTime.Now.ToString("MMddHHmmss"));
            File.WriteAllText(_pIt, _sb.ToString());
            EditorUtility.DisplayDialog("Fbx To Fab Finished", _pIt, "Okey");
        }

        // [MenuItem("Assets/Tools_Art/Rmv All Mat's OverdueProperties")]
        static void HandlerOverdueMatProperties()
        {
            EditorUtility.DisplayProgressBar("Rmv Mat Properties", "begin ...", 0);
            string[] searchInFolders = new string[]
            {
                "Assets/Builds",
                "Assets/Characters/Builds/materials",
                "Assets/Effects/Builds/materials",
                "Assets/Scene/Builds/materials",
            };
            string[] arrs = ToolsByBuild.GetRelativeAssetPaths("t:Material", searchInFolders);
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
                EditorUtility.DisplayProgressBar("Rmv Mat Properties", _it, (i + 1) / (float)_lens);
                Material mat = AssetDatabase.LoadAssetAtPath(_it, typeof(Material)) as Material;
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

        static private void RemoveElement(Material mat, string spName, SerializedProperty saveProperty)
        {
            SerializedProperty property = saveProperty.FindPropertyRelative(spName);
            bool _isTexture = "m_TexEnvs".Equals(spName);
            for (int i = property.arraySize - 1; i >= 0; i--)
            {
                var prop = property.GetArrayElementAtIndex(i);
                string propertyName = prop.displayName;
                if (!mat.HasProperty(propertyName))
                {
                    property.DeleteArrayElementAtIndex(i);
                    continue;
                }

                if (_isTexture)
                {
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
                EditorUtility.DisplayProgressBar("Rmv Mat Properties", mat.name, (i + 1) / (float)_lens);
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

        static private bool RemoveMatMonoOrNoDependency(string fp)
        {
            if (!fp.EndsWith(".mat"))
                return false;
            bool _isChg = false, _isHas = false, _isDelNext = false;
            string[] _lines = File.ReadAllLines(fp);
            List<string> _list = new List<string>();
            string _cur, _next, _curTrim;
            int _lens = _lines.Length;
            string RegexStr = @"guid: \w+", _guid, _assetPath4GUID;
            Match _match = null; // 单行匹配
            for (int i = 0; i < _lens; i++)
            {
                _cur = _lines[i];
                _curTrim = _cur.Trim();
                if (_curTrim.StartsWith("---"))
                    _isHas = false;

                if (_isHas)
                    continue;

                if (_curTrim.StartsWith("MonoBehaviour:"))
                    _isHas = true;
                _next = null;
                if ((i + 1) < _lens)
                {
                    _next = _lines[i + 1].Trim();
                    if (_next.StartsWith("MonoBehaviour:"))
                        _isHas = true;
                }
                _isChg = _isChg || _isHas;
                if (_isHas)
                    continue;
                if (_isDelNext)
                {
                    _isDelNext = false;
                    continue;
                }
                if (Regex.IsMatch(_cur, RegexStr))
                {
                    _match = Regex.Match(_cur, RegexStr);
                    _guid = _match.Value;
                    _guid = _guid.Replace("guid: ", "").Trim();
                    _assetPath4GUID = AssetDatabase.GUIDToAssetPath(_guid);
                    if (string.IsNullOrEmpty(_assetPath4GUID))
                    {
                        int _index = _cur.IndexOf("{");
                        int _index2 = _cur.LastIndexOf("}");
                        _cur = _cur.Substring(0, _index);
                        _cur = string.Concat(_cur, "{fileID: 0}");
                        _isChg = true;
                        _isDelNext = _index2 <= 0;
                    }
                }
                _list.Add(_cur);
            }
            if (_isChg)
            {
                string[] _line2 = _list.ToArray();
                File.WriteAllLines(fp, _line2);
            }
            return _isChg;
        }

        static private bool RemoveFabNoDependency(string fp)
        {
            if (!fp.EndsWith(".prefab"))
                return false;
            bool _isChg = false, _isDelNext = false;
            string[] _lines = File.ReadAllLines(fp);
            List<string> _list = new List<string>();
            string _cur;
            int _lens = _lines.Length;
            string RegexStr = @"guid: \w+", _guid, _assetPath4GUID;
            Match _match = null; // 单行匹配
            for (int i = 0; i < _lens; i++)
            {
                _cur = _lines[i];
                if (_isDelNext)
                {
                    _isDelNext = false;
                    continue;
                }
                if (Regex.IsMatch(_cur, RegexStr))
                {
                    _match = Regex.Match(_cur, RegexStr);
                    _guid = _match.Value;
                    _guid = _guid.Replace("guid: ", "").Trim();
                    _assetPath4GUID = AssetDatabase.GUIDToAssetPath(_guid);
                    if (string.IsNullOrEmpty(_assetPath4GUID))
                    {
                        int _index = _cur.IndexOf("{");
                        int _index2 = _cur.LastIndexOf("}");
                        _cur = _cur.Substring(0, _index);
                        _cur = string.Concat(_cur, "{fileID: 0}");
                        _isChg = true;
                        _isDelNext = _index2 <= 0;
                    }
                }
                _list.Add(_cur);
            }
            if (_isChg)
            {
                string[] _line2 = _list.ToArray();
                File.WriteAllLines(fp, _line2);
            }
            return _isChg;
        }

        [MenuItem("Assets/Tools_Art/Rmv Select Mat's Mono Or NoDependency")]
        static void RmvSelectMatsMonoOrNoDependency()
        {
            EditorUtility.DisplayProgressBar("Rmv Mat's MonoBehaviour", "Start ...", 0.0f);
            var arrs = ToolsByBuild.GetSelectAssetPaths("t:Material");
            if (arrs == null || arrs.Length <= 0)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("=== Rmv Mat's MonoBehaviour = is not select folders");
                return;
            }
            EditorUtility.DisplayProgressBar("Rmv Mat's MonoBehaviour", "begin ...", 0);
            int _lens = arrs.Length;
            string dirDataNoAssets = Application.dataPath.Replace("Assets", "");
            dirDataNoAssets = dirDataNoAssets.Replace('\\', '/');
            string _assetPath, _filePath;
            bool _isChg = false, _isCurChg = false;
            for (int i = 0; i < _lens; i++)
            {
                _assetPath = arrs[i];
                EditorUtility.DisplayProgressBar("Rmv Mat's MonoBehaviour", _assetPath, (i + 1) / (float)_lens);
                _filePath = dirDataNoAssets + _assetPath;
                _isCurChg = RemoveMatMonoOrNoDependency(_filePath);
                _isChg = _isChg || _isCurChg;
                System.Threading.Thread.Sleep(10);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Rmv Mat's MonoBehaviour Finished", "this is over", "Okey");
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

        [MenuItem("Assets/Tools_Art/Optimization Anim Clip", false, 50)]
        static void AnimClipOptimization()
        {
            try
            {
                _AnimClipOptimization();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        static void _AnimClipOptimization()
        {
            EditorUtility.DisplayProgressBar("Optimization Anim Clip", "Start ...", 0.0f);
            List<string> _listFolders = ToolsByBuild.GetSelectFolders();
            string[] searchInFolders = _listFolders.ToArray();
            if (searchInFolders.Length <= 0)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Optimization Anim Clip", "this is empty,not select Folder", "Okey");
                return;
            }

            string[] arrs = ToolsByBuild.GetRelativeAssetPaths("t:AnimationClip", searchInFolders);
            if (arrs == null || arrs.Length <= 0)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Optimization Anim Clip", "not has reses", "Okey");
                return;
            }

            int _lens = arrs.Length;
            string m_dirDataNoAssets = Application.dataPath.Replace("Assets", "");
            string _assetPath, _filePath;
            string[] _lines;
            string _line, _line1;
            float _temp;
            string _p1 = @"[\r\n\s\t]*\w+:[\r\n\s\t]*{[\r\n\s\t]*(x:[\r\n\s\t]*-?[\d\.]+),[\r\n\s\t]*(y:[\r\n\s\t]*-?[\d\.]+),[\r\n\s\t]*(z:[\r\n\s\t]*-?[\d\.]+),?[\r\n\s\t]*?(w:[\r\n\s\t]*-?[\d\.]+)?[\r\n\s\t]*?}";

            // _p1 = @"[\r\n\s\t]*\w+:[\r\n\s\t]*{[\r\n\s\t]*x:[\r\n\s\t]*(-?\d+\.?\d+?),[\r\n\s\t]*y:[\r\n\s\t]*(-?\d+\.?\d+?),[\r\n\s\t]*z:[\r\n\s\t]*(-?\d+\.?\d+?),?[\r\n\s\t]*?w?:?[\r\n\s\t]*(-?\d+\.?\d+?)?[\r\n\s\t]*?}";

            // _p1 = @"[\r\n\s\t]*\w+:[\r\n\s\t]*{[\r\n\s\t]*x:[\r\n\s\t]*(-?[\d\.]+),[\r\n\s\t]*y:[\r\n\s\t]*(-?[\d\.]+),[\r\n\s\t]*z:[\r\n\s\t]*(-?[\d\.]+),[\r\n\s\t]*w:[\r\n\s\t]*(-?[\d\.]+)[\r\n\s\t]*}";
            // _p1 = @"[\r\n\s\t]*\w+:[\r\n\s\t]*{[\r\n\s\t]*x:[\r\n\s\t]*(-?\d+\.?\d+?),[\r\n\s\t]*y:[\r\n\s\t]*(-?\d+\.?\d+?),[\r\n\s\t]*z:[\r\n\s\t]*(-?\d+\.?\d+?),[\r\n\s\t]*w:[\r\n\s\t]*(-?\d+\.?\d+?)[\r\n\s\t]*}";

            Regex regexTex = new Regex(_p1);
            for (int i = 0; i < _lens; i++)
            {
                _assetPath = arrs[i];
                EditorUtility.DisplayProgressBar("Optimization Anim Clip", _assetPath, (i + 1) / (float)_lens);
                _filePath = m_dirDataNoAssets + _assetPath;

                if (!File.Exists(_filePath) || !_filePath.EndsWith(".anim"))
                    continue;

                _lines = File.ReadAllLines(@_filePath);

                for (int j = 0; j < _lines.Length; j++)
                {
                    _line = _lines[j];
                    _line1 = _line;
                    if (_line1.Contains("value:") || _line1.Contains("inSlope:") || _line1.Contains("outSlope:") || _line1.Contains("inWeight:") || _line1.Contains("outWeight:"))
                    {
                        var matches = regexTex.Matches(_line1);
                        for (int k = 0; k < matches.Count; k++)
                        {
                            var _gp = matches[k].Groups;
                            for (int m = 1; m < _gp.Count; m++)
                            {
                                var tv = _gp[m].Value;
                                if (string.IsNullOrWhiteSpace(tv))
                                    continue;
                                var _index = tv.LastIndexOf(".");
                                _index = _index < 0 ? (tv.Length - 1) : _index;
                                var fv = tv.Substring(_index);
                                var fnlen = fv.Length;
                                var tv2 = tv.Replace("x:", "").Replace("y:", "").Replace("z:", "").Replace("w:", "").Trim();
                                if (float.TryParse(tv2, out _temp))
                                {
                                    if (fnlen > 4)
                                        _temp += 0.00005f;
                                    int vt = (int)(_temp * 10000);
                                    _temp = vt * 0.0001f;
                                }
                                var _nTv = tv.Replace(tv2, _temp.ToString());
                                _line1 = _line1.Replace(tv, _nTv);
                            }
                        }
                    }
                    if (!_line.Equals(_line1))
                    {
                        _lines[j] = _line1;
                        // Debug.LogErrorFormat("= [{0}] = [{1}] = [{2}]",_assetPath, _line, _line1);
                    }
                }

                File.WriteAllLines(@_filePath, _lines);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Optimization Anim Clip Finished", "this is over", "Okey");
        }

        static void CleanupShapeModuleMesh(ref ParticleSystem.ShapeModule shape, ref bool isChg, int ntype = 3)
        {
            bool isMesh = ntype != 0 || ntype == 3;
            if (isMesh)
            {
                isChg = isChg || shape.mesh != null;
                shape.mesh = null;
            }
            bool isMeshRenderer = ntype != 1 || ntype == 3;
            if (isMeshRenderer)
            {
                isChg = isChg || shape.meshRenderer != null;
                shape.meshRenderer = null;
            }
            bool isSkinMeshRenderer = ntype != 2 || ntype == 3;
            if (isSkinMeshRenderer)
            {
                isChg = isChg || shape.skinnedMeshRenderer != null;
                shape.skinnedMeshRenderer = null;
            }
        }

        static void CleanupShapeModuleSprite(ref ParticleSystem.ShapeModule shape, ref bool isChg, int ntype = 3)
        {
            bool isSprite = ntype != 0 || ntype == 3;
            if (isSprite)
            {
                isChg = isChg || shape.sprite != null;
                shape.sprite = null;
            }
            bool isSpriteRenderer = ntype != 1 || ntype == 3;
            if (isSpriteRenderer)
            {
                isChg = isChg || shape.spriteRenderer != null;
                shape.spriteRenderer = null;
            }
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
            ParticleSystemRenderer _rer_ = null;
            bool _isTrail;
            foreach (var item in _arrs4)
            {
                _isTrail = false;
                if (item != null)
                {
                    var _shapeModule = item.shape;
                    switch (_shapeModule.shapeType)
                    {
                        case ParticleSystemShapeType.Mesh:
                            CleanupShapeModuleMesh(ref _shapeModule, ref _isChg, 0);
                            break;
                        case ParticleSystemShapeType.MeshRenderer:
                            CleanupShapeModuleMesh(ref _shapeModule, ref _isChg, 1);
                            break;
                        case ParticleSystemShapeType.SkinnedMeshRenderer:
                            CleanupShapeModuleMesh(ref _shapeModule, ref _isChg, 2);
                            break;
                        case ParticleSystemShapeType.Sprite:
                            CleanupShapeModuleSprite(ref _shapeModule, ref _isChg, 0);
                            break;
                        case ParticleSystemShapeType.SpriteRenderer:
                            CleanupShapeModuleSprite(ref _shapeModule, ref _isChg, 1);
                            break;
                        default:
                            CleanupShapeModuleMesh(ref _shapeModule, ref _isChg);
                            CleanupShapeModuleSprite(ref _shapeModule, ref _isChg);
                            break;
                    }

                    _rer_ = item.GetComponent<ParticleSystemRenderer>();
                    if (_rer_ != null)
                    {
                        _isTrail = (item.trails.enabled && _rer_.trailMaterial != null);

                        switch (_rer_.renderMode)
                        {
                            case ParticleSystemRenderMode.Mesh:
                                break;
                            default:
                                if (!_isTrail && _rer_.renderMode == ParticleSystemRenderMode.None && _rer_.sharedMaterial != null)
                                {
                                    // 清除过 sharedMaterials 最终还是会记录两个 空数据
                                    _rer_.sharedMaterial = null;
                                    _isChg = _isChg || true;
                                }
                                _isChg = _isChg || _rer_.mesh != null;
                                _rer_.mesh = null;
                                break;
                        }
                    }

                    if ((_rer_ == null) || (!_rer_.enabled) || ((_rer_.sharedMaterial == null) && !_isTrail))
                    {
                        if (_rer_ != null)
                        {
                            _rer_.sharedMaterial = null;
                            _rer_.sharedMaterials = new Material[0];
                        }
                        GameObject.DestroyImmediate(item);
                        _isChg = _isChg || true;
                    }
                }
            }

            bool _isBl = gobj.isStatic;
            if (_isBl)
            {
                gobjClone.isStatic = false;
                _isChg = true;
            }

            _isBl = gobj.activeSelf;
            if (!_isBl)
            {
                gobjClone.SetActive(true);
                _isChg = true;
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
            EditorUtility.DisplayProgressBar("Remove UnUsed", "Start ...", 0.0f);
            var arrs = ToolsByBuild.GetSelectGobjs();
            if (arrs == null || arrs.Length <= 0)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("=== Remove UnUsed = is not select Prefabs");
                return;
            }
            EditorUtility.DisplayProgressBar("Remove UnUsed", "begin ...", 0);
            int _lens = arrs.Length;
            GameObject _it = null;
            for (int i = 0; i < _lens; i++)
            {
                _it = arrs[i];
                EditorUtility.DisplayProgressBar("Remove UnUsed", _it.name, (i + 1) / (float)_lens);
                CleanupEmptyComp(_it);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Remove UnUsed Finished", "this is over", "Okey");
        }

        [MenuItem("Assets/Tools_Art/Rmv Select Prefab's NoDependency")]
        static void RmvSelectFabNoDependency()
        {
            EditorUtility.DisplayProgressBar("Rmv Select Fab's NoDependency", "Start ...", 0.0f);
            var arrs = ToolsByBuild.GetSelectAssetPaths("t:Prefab");
            if (arrs == null || arrs.Length <= 0)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("=== Rmv Select Fab's NoDependency = is not select folders");
                return;
            }
            EditorUtility.DisplayProgressBar("Rmv Select Fab's NoDependency", "begin ...", 0);
            int _lens = arrs.Length;
            string dirDataNoAssets = Application.dataPath.Replace("Assets", "");
            dirDataNoAssets = dirDataNoAssets.Replace('\\', '/');
            string _assetPath, _filePath;
            bool _isChg = false, _isCurChg = false;
            for (int i = 0; i < _lens; i++)
            {
                _assetPath = arrs[i];
                EditorUtility.DisplayProgressBar("Rmv Select Fab's NoDependency", _assetPath, (i + 1) / (float)_lens);
                _filePath = dirDataNoAssets + _assetPath;
                _isCurChg = RemoveFabNoDependency(_filePath);
                _isChg = _isChg || _isCurChg;
                System.Threading.Thread.Sleep(10);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Rmv Select Fab's NoDependency", "this is over", "Okey");
        }

        static public void CheckMissingBy(string filter = null)
        {
            if (string.IsNullOrEmpty(filter))
                filter = "t:Material t:Prefab"; // "t:Material t:Prefab t:Scene"
            EditorUtility.DisplayProgressBar("Check Missing", "Start ...", 0.0f);
            var arrs = ToolsByBuild.GetSelectAssetPaths(filter);
            if (arrs == null || arrs.Length <= 0)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("=== Check Missing = is not select folders");
                return;
            }
            EditorUtility.DisplayProgressBar("Check Missing", "begin ...", 0);
            int _lens = arrs.Length;
            string dirDataNoAssets = Application.dataPath.Replace("Assets", "");
            dirDataNoAssets = dirDataNoAssets.Replace('\\', '/');
            string _assetPath, _filePath, _fcont;
            string RegexStr = @"guid: \w+";
            // Match mat = null; // 单行匹配
            MatchCollection _mCols = null; // 全文本匹配
            string _guid = null, _assetPath4GUID = null;
            for (int i = 0; i < _lens; i++)
            {
                _assetPath = arrs[i];
                EditorUtility.DisplayProgressBar("Check Missing", _assetPath, (i + 1) / (float)_lens);
                _filePath = dirDataNoAssets + _assetPath;
                _fcont = File.ReadAllText(_filePath);
                if (!Regex.IsMatch(_fcont, RegexStr))
                    continue;
                _mCols = Regex.Matches(_fcont, RegexStr);
                for (int k = 0; k < _mCols.Count; ++k)
                {
                    _guid = _mCols[k].Value;
                    if (string.IsNullOrEmpty(_guid))
                        continue;
                    _guid = _guid.Replace("guid: ", "").Trim();
                    EditorUtility.DisplayProgressBar("Check Missing : " + _assetPath, _guid, (i + 1) / (float)_lens);
                    _assetPath4GUID = AssetDatabase.GUIDToAssetPath(_guid);
                    if (string.IsNullOrEmpty(_assetPath4GUID))
                    {
                        // 还有种丢失的情况，就是 fileID 与 guid 的 object对象的 LocalFileIdentifier 不匹配
                        // AssetDatabase.TryGetGUIDAndLocalFileIdentifier
                        Debug.LogErrorFormat("==== has missing file; _assetPath = [{0}] , miss_guid = [{1}]", _assetPath, _guid);
                    }
                }
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Check Missing Finished", "this is over", "Okey");
        }

        [MenuItem("Assets/Tools_Art/Check Missing 4 Material And Prefab")]
        static void CheckMiss4MaterialAndPrefab()
        {
            CheckMissingBy();
        }

        [MenuItem("Assets/Tools_Art/Check Missing 4 Material")]
        static void CheckMiss4Material()
        {
            CheckMissingBy("t:Material");
        }

        [MenuItem("Assets/Tools_Art/Check Missing 4 Prefab")]
        static void CheckMiss4Prefab()
        {
            CheckMissingBy("t:Prefab");
        }

        [MenuItem("Assets/Tools_Art/Check Missing 4 Scene")]
        static void CheckMiss4Scene()
        {
            CheckMissingBy("t:Scene");
        }

        // [MenuItem("Assets/Tools_Art/Print Select Folders")]
        static void PrintSelectFolder()
        {
            List<string> _listFolders = ToolsByBuild.GetSelectFolders();
            foreach (var item in _listFolders)
            {
                Debug.Log(item);
            }
        }

        static string[] GetAllPrefabs()
        {
            string[] temp = AssetDatabase.GetAllAssetPaths();
            List<string> result = new List<string>();
            foreach (string s in temp)
            {
                if (string.IsNullOrEmpty(s))
                    continue;
                if (s.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                    result.Add(s);
            }
            return result.ToArray();
        }

        static void FindMissingPrefabInGO(GameObject g, string prefabName, bool isRoot)
        {
            if (g.name.Contains("Missing Prefab"))
            {
                Debug.LogError($"=== [{prefabName}] has missing prefab = [{g.name}]");
                return;
            }

            if (PrefabUtility.IsPrefabAssetMissing(g))
            {
                Debug.LogError($"=== [{prefabName}] has missing prefab = [{g.name}]");
                return;
            }

            if (PrefabUtility.IsDisconnectedFromPrefabAsset(g))
            {
                Debug.LogError($"=== [{prefabName}] has missing prefab = [{g.name}]");
                return;
            }

            if (!isRoot)
            {
                if (PrefabUtility.IsAnyPrefabInstanceRoot(g))
                    return;
                GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(g);
                if (root == g)
                    return;
            }

            foreach (Transform childT in g.transform)
            {
                FindMissingPrefabInGO(childT.gameObject, prefabName, false);
            }
        }

        // [MenuItem("Assets/Tools_Art/Find Missing Prefab")]
        static void FindMissPrefab()
        {
            EditorUtility.DisplayProgressBar("Find Missing Prefab", "Start ...", 0.0f);
            var arrs = GetAllPrefabs();
            if (arrs == null || arrs.Length <= 0)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("=== Find Missing Prefab = is not select Prefabs");
                return;
            }

            EditorUtility.DisplayProgressBar("Find Missing Prefab", "begin ...", 0);
            int _lens = arrs.Length;
            string _pIt = null;
            GameObject _it = null;
            for (int i = 0; i < _lens; i++)
            {
                _pIt = arrs[i];
                if (string.IsNullOrEmpty(_pIt))
                    continue;
                EditorUtility.DisplayProgressBar("Find Missing Prefab", _pIt, (i + 1) / (float)_lens);
                _it = AssetDatabase.LoadAssetAtPath<GameObject>(_pIt);
                if (null == _it)
                    continue;
                FindMissingPrefabInGO(_it, _pIt, true);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Find Missing Prefab Finished", "this is over", "Okey");
        }

        static public bool isUseOutInputTextureFmt = true;
        static public int defCompressionRate = 60;

        static public void ReTextureFormat(TextureImporterFormat fmtAlpha, TextureImporterFormat fmtNotAlpha, params string[] dir)
        {
            EditorUtility.DisplayProgressBar("Re - Compression Format", "Start ...", 0.0f);
            string[] arrs = ToolsByBuild.GetRelativeAssetPaths("t:Texture", dir);
            if (arrs == null || arrs.Length <= 0)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("=== Re - Compression Format = is not has reses");
                return;
            }
            int _lens = arrs.Length;
            string _pIt = null;
            TextureImporter importer;
            TextureImporterFormat curFmt;
            bool _isUseOutFmt = isUseOutInputTextureFmt;
            for (int i = 0; i < _lens; i++)
            {
                _pIt = arrs[i];
                if (string.IsNullOrEmpty(_pIt))
                    continue;
                EditorUtility.DisplayProgressBar("Re - Compression Format", _pIt, (i + 1) / (float)_lens);
                importer = TextureImporter.GetAtPath(_pIt) as TextureImporter;
                if (null == importer)
                    continue;

                bool _isHasAlpha = importer.DoesSourceTextureHaveAlpha();
                int t_w = 0, t_h = 0;
                (t_w, t_h) = GetTextureImporterSize(importer);
                bool _isP2 = IsPower2(t_w, t_h);
                bool _isD4 = IsDivisible4(t_w, t_h);

                int _crate = GetCompressionRate(_pIt);
                // iPhone
                if (!_isUseOutFmt)
                {
                    fmtAlpha = _isP2 ? TextureImporterFormat.PVRTC_RGBA4 : TextureImporterFormat.ASTC_10x10;
                    fmtNotAlpha = _isP2 ? TextureImporterFormat.PVRTC_RGB4 : TextureImporterFormat.ASTC_RGB_10x10;
                }
                curFmt = _isHasAlpha ? fmtAlpha : fmtNotAlpha;
                bool isChg = _SaveSetting(importer, "iPhone", 2048, curFmt, _crate);

                // Android
                if (!_isUseOutFmt)
                {
                    fmtAlpha = _isD4 ? TextureImporterFormat.ETC2_RGBA8Crunched : TextureImporterFormat.ASTC_10x10;
                    fmtNotAlpha = _isD4 ? TextureImporterFormat.ETC_RGB4Crunched : TextureImporterFormat.ASTC_RGB_10x10;
                }
                curFmt = _isHasAlpha ? fmtAlpha : fmtNotAlpha;
                bool isChg2 = _SaveSetting(importer, "Android", 2048, curFmt, _crate);
                isChg = isChg || isChg2;
                if (isChg)
                {
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Re - Compression Format", "this is over", "Okey");
        }

        static bool _SaveSetting(TextureImporter importer, string platform, int maxTextureSize, TextureImporterFormat fmt, int compressionQuality)
        {
            bool isChg = false;
            TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
            if (settings == null)
                return isChg;

            bool _isAnd = "Android".Equals(platform);
            TextureImporterCompression _textureCompression = TextureImporterCompression.Compressed;
            switch (fmt)
            {

                case TextureImporterFormat.ETC2_RGBA8:
                case TextureImporterFormat.ETC2_RGBA8Crunched:
                case TextureImporterFormat.ETC_RGB4Crunched:
                    _textureCompression = TextureImporterCompression.CompressedHQ;
                    break;
            }

            if (_isAnd)
            {
                isChg = isChg || settings.androidETC2FallbackOverride != AndroidETC2FallbackOverride.UseBuildSettings;
                settings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
                switch (fmt)
                {
                    case TextureImporterFormat.ETC2_RGB4:
                    case TextureImporterFormat.ETC2_RGB4_PUNCHTHROUGH_ALPHA:
                    case TextureImporterFormat.ETC2_RGBA8:
                    case TextureImporterFormat.ETC2_RGBA8Crunched:
                    case TextureImporterFormat.ETC_RGB4:
                    case TextureImporterFormat.ETC_RGB4Crunched:
                        isChg = isChg || settings.allowsAlphaSplitting;
                        settings.allowsAlphaSplitting = false;
                        break;
                }
            }
            else
            {
                _textureCompression = TextureImporterCompression.CompressedHQ;
            }

            isChg = isChg || settings.textureCompression != _textureCompression;
            isChg = isChg || settings.compressionQuality != compressionQuality;
            isChg = isChg || !settings.overridden;
            settings.textureCompression = _textureCompression;
            settings.compressionQuality = compressionQuality;
            settings.overridden = true;
            int _max = settings.maxTextureSize;
            if (_max <= maxTextureSize)
                settings.maxTextureSize = maxTextureSize;
            isChg = isChg || settings.maxTextureSize != _max;
            isChg = isChg || settings.format != fmt;

            settings.format = fmt;
            importer.SetPlatformTextureSettings(settings);
            return isChg;
        }

        static int GetCompressionRate(string assetPath)
        {
            if (funcCompressionRate != null)
                return funcCompressionRate(assetPath, defCompressionRate);

            int _r = defCompressionRate;
            if (assetPath.Contains("Assets/_Develop/Characters/Builds") && assetPath.Contains("_d."))
                _r = 100;
            return _r;
        }

        //获取导入图片的宽高
        static (int, int) GetTextureImporterSize(TextureImporter importer)
        {
            if (importer != null)
            {
                object[] args = new object[2];
                System.Reflection.BindingFlags _bflag = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                System.Reflection.MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", _bflag);
                mi.Invoke(importer, args);
                return ((int)args[0], (int)args[1]);
            }
            return (0, 0);
        }

        // 2的整数次幂
        static bool IsPower2(int width, int height)
        {
            return (width == height) && (width > 0) && ((width & (width - 1)) == 0);
        }

        // 被4整除
        static bool IsDivisible4(int width, int height)
        {
            return (width > 0) && (height > 0) && (width % 4 == 0 && height % 4 == 0);
        }

        static public void ReCompFormatSelFolder(int ntype = 0, int compressionRate = 60, bool isUseOutInput = false)
        {
            defCompressionRate = compressionRate;
            isUseOutInputTextureFmt = isUseOutInput;
            TextureImporterFormat fmtAlpha = TextureImporterFormat.ASTC_10x10, fmtNotAlpha = TextureImporterFormat.ASTC_RGB_10x10;
            switch (ntype)
            {
                case 4:
                    fmtAlpha = TextureImporterFormat.ASTC_4x4; fmtNotAlpha = TextureImporterFormat.ASTC_RGB_4x4;
                    break;
                case 5:
                    fmtAlpha = TextureImporterFormat.ASTC_5x5; fmtNotAlpha = TextureImporterFormat.ASTC_RGB_5x5;
                    break;
                case 6:
                    fmtAlpha = TextureImporterFormat.ASTC_6x6; fmtNotAlpha = TextureImporterFormat.ASTC_RGB_6x6;
                    break;
                case 8:
                    fmtAlpha = TextureImporterFormat.ASTC_8x8; fmtNotAlpha = TextureImporterFormat.ASTC_RGB_8x8;
                    break;
                case 12:
                    fmtAlpha = TextureImporterFormat.ASTC_12x12; fmtNotAlpha = TextureImporterFormat.ASTC_RGB_12x12;
                    break;
                default:
                    break;
            }

            List<string> _listFolders = ToolsByBuild.GetSelectFolders();
            string[] searchInFolders = _listFolders.ToArray();
            if (searchInFolders.Length <= 0)
            {
                EditorUtility.DisplayDialog("Re - Compression Format SelectFolder", "this is empty,not select Folder", "Okey");
                return;
            }
            ReTextureFormat(fmtAlpha, fmtNotAlpha, searchInFolders);
        }

        [MenuItem("Assets/Tools_Art/ReCompFormat SelectFolder")]
        static void _ReCompFormatSelFolder()
        {
            ReCompFormatSelFolder(10, 60, false);
        }

        [MenuItem("Tools_Art/ExportCSV4ShaderVariantCount", false, 20)]
        static void GetAllShaderVariantCount()
        {
            string _fpDll = string.Concat(EditorApplication.applicationContentsPath, @"\Managed\UnityEditor.dll");
            Assembly asm = Assembly.LoadFile(_fpDll);
            System.Type t2 = asm.GetType("UnityEditor.ShaderUtil");
            MethodInfo method = t2.GetMethod("GetVariantCount", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var shaderList = AssetDatabase.FindAssets("t:Shader");

            var output = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
            string pathF = string.Format("{0}/ShaderVariantCount_{1}.csv", output, System.DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            using (FileStream fs = new FileStream(pathF, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    EditorUtility.DisplayProgressBar("Shader统计文件", "正在写入统计文件中...", 0f);
                    int ix = 0;
                    sw.WriteLine("ShaderFile, VariantCount");
                    foreach (var i in shaderList)
                    {
                        EditorUtility.DisplayProgressBar("Shader统计文件", "正在写入统计文件中...", ix / shaderList.Length);
                        var path = AssetDatabase.GUIDToAssetPath(i);
                        Shader s = AssetDatabase.LoadAssetAtPath(path, typeof(Shader)) as Shader;
                        var variantCount = method.Invoke(null, new System.Object[] { s, true });
                        sw.WriteLine(path + "," + variantCount.ToString());
                        ++ix;
                    }
                    EditorUtility.ClearProgressBar();
                    sw.Close();
                    fs.Close();
                }
            }
        }

        [MenuItem("Assets/Tools_Art/Find Sub-emitters SelectFolder")]
        static void _FindSubEmittersSelFolder()
        {
            List<string> _listFolders = ToolsByBuild.GetSelectFolders();
            string[] searchInFolders = _listFolders.ToArray();
            if (searchInFolders.Length <= 0)
            {
                EditorUtility.DisplayDialog("Find Sub-emitters SelectFolder", "this is empty,not select Folder", "Okey");
                return;
            }
            string[] arrs = ToolsByBuild.GetRelativeAssetPaths("t:Prefab", searchInFolders);

            EditorUtility.DisplayProgressBar("Re - Compression Format", "Start ...", 0.0f);
            if (arrs == null || arrs.Length <= 0)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Find Sub-emitters SelectFolder", "this is empty,has reses", "Okey");
                return;
            }

            string _it;
            int _lens = arrs.Length;
            GameObject _gobj = null;
            ParticleSystem.SubEmittersModule _pssm;
            Dictionary<string, List<string>> _fathers = new Dictionary<string, List<string>>();
            List<string> _childes = null;
            string _rname = null;
            for (int i = 0; i < _lens; i++)
            {
                _it = arrs[i];
                EditorUtility.DisplayProgressBar("Sub-emitters", _it, (i + 1) / (float)_lens);
                _gobj = AssetDatabase.LoadAssetAtPath<GameObject>(_it);
                var pses = _gobj.GetComponentsInChildren<ParticleSystem>(true);
                foreach (var ps in pses)
                {
                    _pssm = ps.subEmitters;
                    if (_pssm.enabled)
                    {
                        if (_fathers.ContainsKey(_it))
                        {
                            _childes = _fathers[_it];
                        }
                        else
                        {
                            _childes = new List<string>();
                            _fathers.Add(_it, _childes);
                        }
                        _rname = ToolsByBuild.ReNodeName(ps.transform);
                        if (!_childes.Contains(_rname))
                            _childes.Add(_rname);
                    }
                }
            }

            StringBuilder _sb = new StringBuilder();
            foreach (var item in _fathers.Keys)
            {
                _sb.Append("===== has Sub-emitters = ").AppendLine(item);
                _childes = _fathers[item];
                foreach (var c in _childes)
                {
                    _sb.AppendLine(c);
                }
                _sb.AppendLine();
            }
            EditorUtility.ClearProgressBar();

            string _pIt = string.Format("{0}/../../SubEmitters{1}.txt", Application.dataPath, System.DateTime.Now.ToString("MMddHHmmss"));
            File.WriteAllText(_pIt, _sb.ToString());
            EditorUtility.DisplayDialog("Find Sub-emitters", _pIt, "Okey");
        }
    }
}