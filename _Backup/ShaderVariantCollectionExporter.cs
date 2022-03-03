﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Debug = UnityEngine.Debug;
using SShaderVariant = UnityEngine.ShaderVariantCollection.ShaderVariant;

/// <summary>
/// 类名 : ShaderVariantCollection 收集工具
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2021-01-20 16:35
/// 功能 : 
/// 来源 : https://github.com/networm/ShaderVariantCollectionExporter
/// </summary>
public static class ShaderVariantCollectionExporter
{
    [MenuItem("Tools/Shader/Export ShaderVariantCollection")]
    static void Export()
    {
        ExportSVC();
    }

    [MenuItem("Tools/Shader/Export GameSVC")]
    static void Export2()
    {
        ExportSVC("Assets/_Develop/Builds/all_svcs.shadervariants", "_Develop/");
    }

    /// <summary>
    /// fpSave 以 Assets 开头的要保存的SVC文件的地址
    /// fpDir 是 Assets 资源查找路径地址
    /// </summary>
    static public void ExportSVC(string fpSave = null, string rootDir = null, string fpDir = null)
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
        System.Threading.Thread.Sleep(20);
        Debug.LogFormat("=== ExportSVC T0 = [{0}]", System.DateTime.Now.ToString("HH:mm:ss"));
        if (!string.IsNullOrEmpty(fpSave))
            _SVCPath = fpSave;

        // if (string.IsNullOrEmpty(fpDir))
        //    fpDir = Application.dataPath;
        // var _arrs2 = Directory.GetFiles(fpDir, "*.*", SearchOption.AllDirectories)
        //         .Where(s => s.ToLower().EndsWith(".mat")).ToArray();

        EditorUtility.UnloadUnusedAssetsImmediate(true);
        System.Threading.Thread.Sleep(20);
        Debug.LogFormat("=== ExportSVC T1 = [{0}]", System.DateTime.Now.ToString("HH:mm:ss"));

        var _arrs2 = AssetDatabase.FindAssets("t:Material"); // t:Material t:Shader        

        var materials = new List<Material>();
        var errMats = new HashSet<string>();
        string _pIt;
        Material _mat;
        Shader _shader = null;
        bool _isCheckRoot = !string.IsNullOrEmpty(rootDir);
        for (int j = 0; j < _arrs2.Length; j++)
        {
            _pIt = AssetDatabase.GUIDToAssetPath(_arrs2[j]);
            _pIt = _pIt.Substring(_pIt.LastIndexOf("Assets"));
            _pIt = _pIt.Replace('\\', '/');
            if (_isCheckRoot && !_pIt.Contains(rootDir))
                continue;

            _mat = AssetDatabase.LoadAssetAtPath<Material>(_pIt);
            if (_mat != null)
            {
                _shader = _mat.shader;
                if (_shader != null)
                {
                    if ("Hidden/InternalErrorShader".Equals(_shader.name, System.StringComparison.OrdinalIgnoreCase))
                    {
                        errMats.Add(_pIt);
                        continue;
                    }

                    if (!materials.Contains(_mat))
                        materials.Add(_mat);
                }
                else
                {
                    errMats.Add(_pIt);
                }
            }
        }
        LogInfo(errMats);
        Debug.LogFormat("=== ExportSVC T2 = [{0}]", System.DateTime.Now.ToString("HH:mm:ss"));
        ProcessMaterials(materials);
    }

    static private void LogInfo(HashSet<string> errMats)
    {
        if (errMats == null || errMats.Count <= 0)
            return;

        var sb = new System.Text.StringBuilder("====== error == mat assetPath =\n");
        foreach (var err in errMats)
        {
            sb.AppendLine("==" + err);
        }
        errMats.Clear();

        var dt8 = System.DateTime.UtcNow.AddHours(8);
        string _fp = string.Format("{0}/../__err_mat_path_{1}.txt", Application.dataPath, dt8.ToString("MMddHHmmss"));
        string _cont = sb.ToString();
        sb.Clear();
        sb.Length = 0;
        File.WriteAllText(_fp, _cont);
        Debug.LogFormat("=== WriteError = [{0}] = [{1}]", _fp, dt8.ToString("HH:mm:ss"));
    }

    static private void EditorUpdate()
    {
        if (_elapsedTime.ElapsedMilliseconds >= WaitTimeBeforeSave)
        {
            EditorApplication.update -= EditorUpdate;
            EditorApplication.isPlaying = true;
            System.Threading.Thread.Sleep(20000);
            object _obj = InvokeInternalStaticMethod(TP_CSU, "GetCurrentShaderVariantCollectionVariantCount");
            Debug.LogFormat("=== Update CurrSVC_VariantCount = [{0}] = [{1}]", _obj, System.DateTime.Now.ToString("HH:mm:ss"));
            InvokeInternalStaticMethod(TP_CSU, "SaveCurrentShaderVariantCollection", _SVCPath);
            _obj = InvokeInternalStaticMethod(TP_CSU, "GetCurrentShaderVariantCollectionShaderCount");
            Debug.LogFormat("=== Update CurrSVC_ShaderCount = [{0}] = [{1}]", _obj, System.DateTime.Now.ToString("HH:mm:ss"));
            _elapsedTime.Stop();
            _elapsedTime.Reset();
            EditorApplication.isPlaying = false;

            if(isReSplitShaderVariants)
            {
                _elapsedTime.Start();
                EditorApplication.update -= _Update4ReSplitSVC;
                EditorApplication.update += _Update4ReSplitSVC;
            }
        }
    }

    static private void ProcessMaterials(List<Material> materials)
    {
        InvokeInternalStaticMethod(TP_CSU, "ClearCurrentShaderVariantCollection");
        object _obj = InvokeInternalStaticMethod(TP_CSU, "GetCurrentShaderVariantCollectionShaderCount");
        Debug.LogFormat("=== CurrSVC_ShaderCount = [{0}] = [{1}]", _obj, System.DateTime.Now.ToString("HH:mm:ss"));

        int totalMaterials = materials.Count;

        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("=== Main Camera didn't exist");
            return;
        }

        float aspect = camera.aspect;

        float height = Mathf.Sqrt(totalMaterials / aspect) + 1;
        float width = Mathf.Sqrt(totalMaterials / aspect) * aspect + 1;

        float halfHeight = Mathf.CeilToInt(height / 2f);
        float halfWidth = Mathf.CeilToInt(width / 2f);

        camera.orthographic = true;
        camera.orthographicSize = halfHeight;
        camera.transform.position = new Vector3(0f, 0f, -10f);

        Selection.activeGameObject = camera.gameObject;
        EditorApplication.ExecuteMenuItem("GameObject/Align View to Selected");

        int xMax = (int)(width - 1);
        int x = 0;
        int y = 0;
        Material material = null;
        for (int i = 0; i < materials.Count; i++)
        {
            material = materials[i];
            var position = new Vector3(x - halfWidth + 1f, y - halfHeight + 1f, 0f);
            CreateSphere(material, position, x, y, i);

            if (x == xMax)
            {
                x = 0;
                y++;
            }
            else
            {
                x++;
            }
            if (i > 0 && i % 5 == 0)
            {
                System.Threading.Thread.Sleep(20);
            }
        }

        Debug.LogFormat("=== ExportSVC T3 = [{0}]", System.DateTime.Now.ToString("HH:mm:ss"));
        _elapsedTime.Stop();
        _elapsedTime.Reset();
        _elapsedTime.Start();
        EditorApplication.isPlaying = false;
        EditorApplication.update -= EditorUpdate;
        EditorApplication.update += EditorUpdate;
    }

    static private void CreateSphere(Material material, Vector3 position, int x, int y, int index)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.GetComponent<Renderer>().material = material;
        go.transform.position = position;
        go.name = string.Format("Sphere_{0:000}|{1:00}_{2:00}|{3}", index, y, x, material.name);
    }

    static private object InvokeInternalStaticMethod(System.Type type, string method, params object[] parameters)
    {
        try
        {
            return _InvokeInternalStaticMethod(type, method, parameters);
        }
        catch (System.Exception ex)
        {
            Debug.LogErrorFormat("=== {0}.{1} err = \n{2}", type, method, ex);
        }
        return null;
    }

    static private object _InvokeInternalStaticMethod(System.Type type, string method, params object[] parameters)
    {
        var methodInfo = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
        if (methodInfo == null)
        {
            Debug.LogErrorFormat("=== {0} method didn't exist", method);
            return null;
        }

        return methodInfo.Invoke(null, parameters);
    }
	
	
	static private void _Update4ReSplitSVC()
    {
        if (_elapsedTime.ElapsedMilliseconds >= 1000)
        {
            EditorApplication.update -= _Update4ReSplitSVC;
            _elapsedTime.Stop();
            _elapsedTime.Reset();
			
			EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
			System.Threading.Thread.Sleep(200);
			EditorUtility.UnloadUnusedAssetsImmediate(true);
			System.Threading.Thread.Sleep(200);
            try
            {
                _ReSplitSVC();
            }
            finally
            {
                if ( File.Exists( _SVCPath ) ) {
                    if ( !keepTempShaderVariants ) {
                        AssetDatabase.DeleteAsset( _SVCPath );
                    }
                }
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }
    }

    static private void _ReSplitSVC()
    {
        Debug.LogFormat("=== _ReSplitSVC 0 = [{0}]", System.DateTime.Now.ToString("HH:mm:ss"));
        string _assetPath = _SVCPath;
        ShaderVariantCollection svcAlls = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(_assetPath);
        if (svcAlls == null)
            return;

        Dictionary<Shader, List<SShaderVariant>> uiDic = null,ftDic = null;
        var _svs = GetSShaderVariant(svcAlls,out uiDic,out ftDic);
        System.Threading.Thread.Sleep(20);
        Debug.LogFormat("=== _ReSplitSVC 1 = [{0}]", System.DateTime.Now.ToString("HH:mm:ss"));
        string _uiSvc = "Assets/_Develop/Builds/all_svc.shadervariants";
        string _fightSvc = "Assets/_Develop/Builds/all_svc_ft.shadervariants";
        CreateSVC(_uiSvc,uiDic);
        System.Threading.Thread.Sleep(200);
        CreateSVC(_fightSvc,ftDic);
        Debug.LogFormat("=== _ReSplitSVC 2 = [{0}]", System.DateTime.Now.ToString("HH:mm:ss"));
    }

    static bool _IsInScene(string assetPath){
        string _ap = assetPath.Replace("\\","/");
        return _ap.Contains("/Scene/Builds/");
    }

    static Dictionary<Shader, List<SShaderVariant>> GetSShaderVariant(ShaderVariantCollection svc,out Dictionary<Shader, List<SShaderVariant>> uiDic,out Dictionary<Shader, List<SShaderVariant>> ftDic)
    {
        uiDic = new Dictionary<Shader, List<SShaderVariant>>();
        ftDic = new Dictionary<Shader, List<SShaderVariant>>();

        var shaderVariants = new Dictionary<Shader, List<SShaderVariant>>();
        using (var so = new SerializedObject(svc))
        {
            var array = so.FindProperty("m_Shaders.Array");
            if (array != null && array.isArray)
            {
                var arraySize = array.arraySize;
                for (int i = 0; i < arraySize; ++i)
                {
                    var shaderRef = array.FindPropertyRelative(string.Format("data[{0}].first", i));
                    var shaderShaderVariants = array.FindPropertyRelative(string.Format("data[{0}].second.variants", i));
                    if (shaderRef != null && shaderRef.propertyType == SerializedPropertyType.ObjectReference &&
                        shaderShaderVariants != null && shaderShaderVariants.isArray)
                    {
                        var shader = shaderRef.objectReferenceValue as Shader;
                        if (shader == null)
                            continue;
                        var shaderAssetPath = AssetDatabase.GetAssetPath(shader);
                        List<SShaderVariant> variants = null;
                        if (!shaderVariants.TryGetValue(shader, out variants))
                        {
                            variants = new List<SShaderVariant>();
                            shaderVariants.Add(shader, variants);
                        }
                        var variantCount = shaderShaderVariants.arraySize;
                        for (int j = 0; j < variantCount; ++j)
                        {
                            var prop_keywords = shaderShaderVariants.FindPropertyRelative(string.Format("Array.data[{0}].keywords", j));
                            var prop_passType = shaderShaderVariants.FindPropertyRelative(string.Format("Array.data[{0}].passType", j));
                            if (prop_keywords != null && prop_passType != null && prop_keywords.propertyType == SerializedPropertyType.String)
                            {
                                var srcKeywords = prop_keywords.stringValue;
                                if(string.IsNullOrWhiteSpace(srcKeywords))
                                    continue;
                                var keywords = srcKeywords.Split(' ');
                                if(keywords == null || keywords.Length <= 0)
                                    continue;
                                
                                var pathType = (UnityEngine.Rendering.PassType)prop_passType.intValue;
                                variants.Add(new SShaderVariant(shader, pathType, keywords));
                            }
                        }

                        if(_IsInScene(shaderAssetPath)){
                            if(!ftDic.ContainsKey(shader))
                            {
                                ftDic.Add(shader,variants);
                            }
                        }else{
                            if(!uiDic.ContainsKey(shader))
                            {
                                uiDic.Add(shader,variants);
                            }
                        }
                    }
                }
            }
        }
        return shaderVariants;
    }

    static ShaderVariantCollection CreateSVC(string svcPath)
    {
        var va = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>( svcPath );
        if (va == null) {
            va = new ShaderVariantCollection();
            AssetDatabase.CreateAsset( va, svcPath );
            AssetDatabase.Refresh();
            va = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>( svcPath );
        }
        return va;
    }

    static void CreateSVC(string svcPath,Dictionary<Shader, List<SShaderVariant>> dic)
    {
        var va = CreateSVC(svcPath);
        va.Clear();
        foreach(var it in dic){
            var _list = it.Value;
            foreach (var _sv in _list)
            {
                va.Add( _sv );
            }
        }
        EditorUtility.SetDirty( va );
    }

    static private System.Type TP_CSU = typeof(ShaderUtil);
    static private readonly Stopwatch _elapsedTime = new Stopwatch();
    private const int WaitTimeBeforeSave = 20000;
    static private string _SVCPath = "Assets/ShaderVariantCollection.shadervariants";
    static private readonly bool isReSplitShaderVariants = false;
    static private readonly bool keepTempShaderVariants = true;
}
