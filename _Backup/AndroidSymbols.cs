using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class AndroidSymbols
{
    private const string ARM64 = "arm64-v8a";

    private const string ARMv7 = "armeabi-v7a";

    //打出的 apk 包生成的 符号表文件
    private static string APKSymbols = Application.dataPath + "/../Temp/StagingArea/symbols/";

    private static string UnitySymbols =
        "/Applications/Unity/Hub/Editor/2020.3.10f1c1/PlaybackEngines/AndroidPlayer/Variations/il2cpp/Release/Symbols/"; //Unity 本身自带的符号表文件

    private static string SymbolUnityDir = Application.dataPath + "/../Symbols/Unity/"; //所有Unity符号表的文件夹
    private static string SymbolAPKDir = Application.dataPath + "/../Symbols/APK/"; //所有APK符号表的文件夹


    [PostProcessBuildAttribute()] //打包后需要执行的方法
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
#if UNITY_EDITOR_OSX
                if (target == BuildTarget.Android)
        {
            //将符号表文件全部拷贝到统一目录文件下
            CopyAllSymbolFilesToOneDir();
            // PostProcessAndroidBuild(pathToBuiltProject);//需要将所有的 symbols 符号表全部上传到服务器上面
        }
#endif

    }

    //将所有文件都拷贝到统一文件夹下
    private static void CopyAllSymbolFilesToOneDir()
    {
        Debug.Log(PlayerSettings.Android.targetArchitectures);
        if (PlayerSettings.Android.targetArchitectures.ToString().Contains(AndroidArchitecture.ARMv7.ToString()))
        {
            FileTool.CopyFolder(APKSymbols + ARMv7 + "/", SymbolAPKDir + ARMv7 + "/");
            FileTool.CopyFolder(UnitySymbols + ARMv7 + "/", SymbolUnityDir + ARMv7 + "/");
        }

        if (PlayerSettings.Android.targetArchitectures.ToString().Contains(AndroidArchitecture.ARM64.ToString()))
        {
            FileTool.CopyFolder(APKSymbols + ARM64 + "/", SymbolAPKDir + ARM64 + "/");
            FileTool.CopyFolder(UnitySymbols + ARM64 + "/", SymbolUnityDir + ARM64 + "/");
        }
    }


    public class FileTool
    {
        public static bool CheckFolder(string path)
        {
            if (Directory.Exists(path))
            {
                return true;
            }

            UnityEditor.EditorUtility.DisplayDialog("Error", "Path does not exist \n\t" + path, "确认");
            return false;
        }

        public static void OpenFolder(string path)
        {
            if (CheckFolder(path))
            {
                System.Diagnostics.Process.Start(path);
            }

        }

        public static void CopyFolder(Dictionary<string, string> copyDic)
        {
            foreach (KeyValuePair<string, string> path in copyDic)
            {

                if (CheckFolder(path.Key))
                {

                    CopyDir(path.Key, path.Value);
                    Debug.Log("Copy Success : \n\tFrom:" + path.Key + " \n\tTo:" + path.Value);
                }
            }

            EditorUtility.ClearProgressBar();
        }

        public static void CopyFolder(string fromPath, string toPath)
        {
            CopyDir(fromPath, toPath);
            Debug.Log("Copy Success : \n\tFrom:" + fromPath + " \n\tTo:" + toPath);
            EditorUtility.ClearProgressBar();
        }

        public static void CreateFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
        }

        public static void DeleteFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private static void CopyDir(string origin, string target)
        {
// #if UNITY_IOS
//       
//         if (!origin.EndsWith("/"))
//         {
//             origin += "/";
//         }
//
//         if (!target.EndsWith("/"))
//         {
//             target += "/";
//         }
// #else
//           if (!origin.EndsWith("\\")) {
//             origin += "\\";
//         }
//
//         if (!target.EndsWith("\\")) {
//             target += "\\";
//         }
//
// #endif
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }

            DirectoryInfo info = new DirectoryInfo(origin);
            FileInfo[] fileList = info.GetFiles();
            DirectoryInfo[] dirList = info.GetDirectories();
            float index = 0;
            foreach (FileInfo fi in fileList)
            {


                if (fi.Extension == ".zip" || fi.Extension == ".meta" || fi.Extension == ".rar")
                {
                    Debug.Log("dont copy :" + fi.FullName);
                    continue;
                }

                float progress = (index / (float) fileList.Length);
                EditorUtility.DisplayProgressBar("Copy ", "Copying: " + Path.GetFileName(fi.FullName), progress);
                File.Copy(fi.FullName, target + fi.Name, true);
                index++;
            }

            foreach (DirectoryInfo di in dirList)
            {
                if (di.FullName.Contains(".svn"))
                {
                    Debug.Log("Continue SVN " + di.FullName);
                    continue;
                }

                CopyDir(di.FullName, target + "\\" + di.Name);
            }
        }



    }
}