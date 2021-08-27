﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Debug = UnityEngine.Debug;

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
        ExportSVC("Assets/_Develop/Builds/all_svc.shadervariants","_Develop/");
    }

    /// <summary>
    /// fpSave 以 Assets 开头的要保存的SVC文件的地址
    /// fpDir 是 Assets 资源查找路径地址
    /// </summary>
    static public void ExportSVC(string fpSave = null, string rootDir = null, bool isLog = false, string fpDir = null)
    {
		Debug.LogFormat("=== ExportSVC T0 = [{0}]", System.DateTime.Now.ToString("HH:mm:ss"));
		if (!string.IsNullOrEmpty(fpSave))
            _SVCPath = fpSave;
		
        // if (string.IsNullOrEmpty(fpDir))
        //    fpDir = Application.dataPath;
        // var _arrs2 = Directory.GetFiles(fpDir, "*.*", SearchOption.AllDirectories)
        //         .Where(s => s.ToLower().EndsWith(".mat")).ToArray();

        var _arrs2 = AssetDatabase.FindAssets("t:Material"); // t:Material t:Shader

        var materials = new List<Material>();
        var shaderDict = new Dictionary<Shader, List<Material>>();
        Shader _shader = null;
        List<Material> _list = null;
        string _pIt;
        Material _mat;
		bool _isCheckRoot = !string.IsNullOrEmpty(rootDir);
        for (int j = 0; j < _arrs2.Length; j++)
        {
            _pIt = AssetDatabase.GUIDToAssetPath(_arrs2[j]);
            _pIt = _pIt.Substring(_pIt.LastIndexOf("Assets"));
			_pIt = _pIt.Replace('\\', '/');
			if(_isCheckRoot && !_pIt.Contains(rootDir))
				continue;
			
            _mat = AssetDatabase.LoadAssetAtPath<Material>(_pIt);
            if (_mat != null)
            {
                _shader = _mat.shader;
                if (_shader != null)
                {

                    if (!shaderDict.TryGetValue(_shader, out _list))
                    {
                        _list = new List<Material>();
                        shaderDict.Add(_shader, _list);
                    }

                    if (!_list.Contains(_mat))
                        _list.Add(_mat);
                }

                if (!materials.Contains(_mat))
                    materials.Add(_mat);
            }
        }
		Debug.LogFormat("=== ExportSVC T1 = [{0}]", System.DateTime.Now.ToString("HH:mm:ss"));
        ProcessMaterials(materials);

        if (isLog)
            LogInfo(shaderDict);
    }

    static private void LogInfo(Dictionary<Shader, List<Material>> shaderDict)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var kvp in shaderDict)
        {
            sb.AppendLine(kvp.Key + " " + kvp.Value.Count + " times");
        }
        Debug.Log(sb.ToString());
    }

    static private void EditorUpdate()
    {
        if (_elapsedTime.ElapsedMilliseconds >= WaitTimeBeforeSave)
        {
            EditorApplication.update -= EditorUpdate;
			EditorApplication.isPlaying = true;
            System.Threading.Thread.Sleep(15000);
            object _obj = InvokeInternalStaticMethod(TP_CSU, "GetCurrentShaderVariantCollectionVariantCount");
            Debug.LogFormat("=== Update CurrSVC_VariantCount = [{0}] = [{1}]", _obj, System.DateTime.Now.ToString("HH:mm:ss"));
            InvokeInternalStaticMethod(TP_CSU, "SaveCurrentShaderVariantCollection", _SVCPath);
            _obj = InvokeInternalStaticMethod(TP_CSU, "GetCurrentShaderVariantCollectionShaderCount");
            Debug.LogFormat("=== Update CurrSVC_ShaderCount = [{0}] = [{1}]", _obj, System.DateTime.Now.ToString("HH:mm:ss"));
            _elapsedTime.Stop();
            _elapsedTime.Reset();
            EditorApplication.isPlaying = false;
        }
    }

    static private void ProcessMaterials(List<Material> materials)
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
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
			if(i > 0 && i % 5 == 0){
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
        go.name = string.Format("Sphere_{0}|{1}_{2}|{3}", index, x, y, material.name);
    }

    static private object InvokeInternalStaticMethod(System.Type type, string method, params object[] parameters)
    {
        try
        {
            return _InvokeInternalStaticMethod(type,method,parameters);
        }
        catch (System.Exception ex)
        {
			Debug.LogErrorFormat("=== {0}.{1} err = \n{2}", type,method,ex);
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
	
	static private System.Type TP_CSU = typeof(ShaderUtil);
    static private readonly Stopwatch _elapsedTime = new Stopwatch();
    private const int WaitTimeBeforeSave = 20000;
    static private string _SVCPath = "Assets/ShaderVariantCollection.shadervariants";
}
