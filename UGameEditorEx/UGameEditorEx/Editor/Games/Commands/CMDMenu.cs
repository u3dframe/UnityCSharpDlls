using UnityEditor;
using UnityEngine;

using System.Reflection;
using System.IO;
using System.Text;

/// <summary>
/// 类名 : Menu 菜单Tools里面的命名
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-11-24 11:40
/// 功能 : 
/// </summary>
public static class CMDMenu
{
    [MenuItem("Tools/ExportCSV4ShaderVariantCount", false, 20)]
    public static void GetAllShaderVariantCount()
    {
        string _fpDll = string.Concat(EditorApplication.applicationContentsPath, @"\Managed\UnityEditor.dll");
        Assembly asm = Assembly.LoadFile(_fpDll);
        System.Type t2 = asm.GetType("UnityEditor.ShaderUtil");
        MethodInfo method = t2.GetMethod("GetVariantCount", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var shaderList = AssetDatabase.FindAssets("t:Shader");

        var output = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
        string pathF = string.Format("{0}/ShaderVariantCount_{1}.csv", output,System.DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        using(FileStream fs = new FileStream(pathF, FileMode.OpenOrCreate, FileAccess.Write))
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

    [MenuItem("Tools/Print/PrintMaterial", false, 50)]
    static void PrintMaterial()
    {
        bool _isChg = false;
        string[] searchInFolders = {
            "Assets/_Develop/"
        };
        string[] _tes = AssetDatabase.FindAssets("t:Material", searchInFolders);
        string _assetPath;
        Material _mat = null;
        for (int i = 0; i < _tes.Length; i++)
        {
            _assetPath = AssetDatabase.GUIDToAssetPath(_tes[i]);
            _mat = AssetDatabase.LoadAssetAtPath(_assetPath, typeof(Material)) as Material;
            Debug.LogFormat("=== {0} = [{1}]", _mat.name, _mat);
        }
        // return _isChg;
    }

    [MenuItem("Tools/Print/PrintAllAB", false, 50)]
    static public void PrintAllAB()
    {
        var _fp = string.Format("{0}../_abs_{1}.txt", m_dirRes, System.DateTime.Now.ToString("yyyyMMddHHmmss"));
        var info = AssetBundleManager.instance.AllInfo();
        GameFile.WriteText(_fp, info, true);
    }

    [MenuItem("Tools/Print/PrintResType", false, 30)]
    static public void PrintResType()
    {
        string[] searchInFolders = {
            "Assets/_Develop/Builds/fnts"
        };
        string[] _tes = AssetDatabase.FindAssets("t:Object", searchInFolders);
        string _assetPath;
        UnityEngine.Object _obj;
        for (int i = 0; i < _tes.Length; i++)
        {
            _assetPath = AssetDatabase.GUIDToAssetPath(_tes[i]);
            _obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_assetPath);
            if (_obj != null)
            {
                Debug.LogFormat("==== [{0}] , type = [{1}]", _assetPath, _obj.GetType());
            }
        }
    }

    [MenuItem("Tools/CopyLua", false, 50)]
    static public void CopyTest()
    {
        ClearAll();
        m_isDevelopment = false;
        _CopyFiles(BuildPatcher.luaDir, m_dirResCache, true, null);
        // _CopyFiles(BuildPatcher.txtDir, m_dirResCache, true, null);
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/GetLuaCompareCode", false, 50)]
    static public void GetLuaCompareCode()
    {
        string lua = string.Concat(luaDir, "tolua.lua");
        string _v = BuildPatcher.GetText4File(lua);
        byte[] _bts = BuildPatcher.GetFileBytes(lua);
        string v64 = System.Text.Encoding.UTF8.GetString(_bts);
        string code1 = CRCClass.GetCRCContent(v64);
        string code2 = CRCClass.GetCRCContent(_v);
        Debug.LogErrorFormat("=== [{0}] = [{1}] = [{2}]", v64.Equals(_v), code1, code2);
    }
}