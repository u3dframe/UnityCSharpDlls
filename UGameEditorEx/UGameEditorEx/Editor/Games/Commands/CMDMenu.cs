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
}