using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Core;
using Core.Kernel;
using UObject = UnityEngine.Object;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 类名 : 资源导出工具脚本 
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-03-07 09:29
/// 功能 : 将protobuf 文件转为 lua 文件
/// 修改 : 2020-03-26 09:29
/// </summary>
public class BuildTools : BuildPatcher
{
    static string[] GenBuildScene()
    {
        string[] buildList = {
            "Assets/_Develop/Scene/Launcher.unity",
            "Assets/_Develop/Scene/Loading01.unity",
            "Assets/_Develop/Scene/Loading02.unity"
        };

        var settings = new List<EditorBuildSettingsScene>();
        var paths = new List<string>();
        foreach (EditorBuildSettingsScene setting in EditorBuildSettings.scenes)
        {
            bool enable = false;
            foreach (string name in buildList)
            {
                if (setting.path == name)
                {
                    paths.Add(name);
                    enable = true;
                }
            }
            setting.enabled = enable;
            settings.Add(setting);
        }
        EditorBuildSettings.scenes = settings.ToArray();
        return paths.ToArray();
    }

    static public void CopyTextFiles(string source, string dist)
    {
        FileUtil.DeleteFileOrDirectory(dist);
        FileUtil.CopyFileOrDirectory(source, dist);
        void RemoveMeta(DirectoryInfo _dir)
        {
            foreach (var file in _dir.GetFiles())
            {

                if (file.Extension == ".meta")
                {
                    file.Delete();
                }
            }
            foreach (var file in _dir.GetDirectories())
            {
                RemoveMeta(file);
            }
        }
        RemoveMeta(new DirectoryInfo(dist));
    }

    static void CopyTextFiles()
    {
        EditorUtility.DisplayProgressBar("CopyTextFiles", "Copy text files ...", 0.4f);
        var txtDir = new DirectoryInfo(Path.Combine(m_dirData, m_edtAssetPath, "CsvTxts"));
        foreach (var dir in txtDir.GetDirectories())
        {
            CopyTextFiles(dir.ToString(), Path.Combine(m_dirRes, dir.Name));
        }

        EditorUtility.DisplayProgressBar("CopyTextFiles", "Copy lua files ...", 0.5f);
        CopyTextFiles(Path.Combine(m_dirData, "Lua"), Path.Combine(m_dirRes, "Lua"));
        FileUtil.DeleteFileOrDirectory(Path.Combine(m_dirRes, "Lua/games/cfg/svr"));
        FileUtil.DeleteFileOrDirectory(Path.Combine(m_dirRes, "Lua/games/cfg/.git"));

        EditorUtility.ClearProgressBar();
    }

    static void InnerBuildAll(string []scenes, string outpath, BuildTargetGroup targetgroup, BuildTarget target, BuildOptions option)
    {
        EditorUtility.DisplayProgressBar("BuidPlayer", "Switch Targe Group", 0.1f);
        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(targetgroup, target);
        option |= BuildOptions.CompressWithLz4;
        AssetDatabase.Refresh();

        // BuildAssetBundles();

        EditorUtility.DisplayProgressBar("BuidPlayer", "Generate Resource", 0.3f);
        CopyTextFiles();
        AssetDatabase.Refresh();

        FileUtil.DeleteFileOrDirectory(m_dirStreaming);
        Directory.CreateDirectory(m_dirStreaming);

        EditorUtility.DisplayProgressBar("BuidPlayer", "Compress Resource", 0.4f);
        SharpZipLib.Zipper.Compress(Path.Combine(Application.dataPath,"../..", m_resFdRoot), Path.Combine(m_dirStreaming, "base.zip"));
        AssetDatabase.Refresh();

        EditorUtility.DisplayProgressBar("BuidPlayer", "Build Runtime", 0.5f);
        UnityEditor.Build.Reporting.BuildReport ret = BuildPipeline.BuildPlayer(scenes, outpath, target, option);

        EditorUtility.DisplayProgressBar("BuidPlayer", "Clean tmp files", 0.9f);
        FileUtil.DeleteFileOrDirectory(m_dirStreaming);
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        if (ret.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("Build Failure:\n");
            foreach (UnityEditor.Build.Reporting.BuildStep step in ret.steps)
            {
                foreach (UnityEditor.Build.Reporting.BuildStepMessage msg in step.messages)
                {
                    sb.Append(step.name + ":" + msg.content + "\n");
                }
            }
            throw new Exception(sb.ToString());
        }
        else
        {
            Debug.Log("Build " + outpath);
        }
    }

    static string getOption( Dictionary<string,string> args, string key, string def){
        string o;
        return args.TryGetValue(key, out o) ? o : def; 
    }

    static public void BuildAndroid()
    {
        string CommandLine = Environment.CommandLine;
        string[] CommandLineArgs = Environment.GetCommandLineArgs();
        var args = new Dictionary<string,string>();
        foreach(var c in CommandLineArgs) {
            string[] vals=c.Split(new char[]{'='}, StringSplitOptions.RemoveEmptyEntries);
            if(vals.Length==1) {
                args.Add(vals[0].Trim(), "true");
            }else{
                args.Add(vals[0].Trim(), vals[1].Trim());
            }
        }
        AssetDatabase.Refresh();
        string directory = getOption(args, "targetDir", Path.Combine(Application.dataPath.Replace("/Assets", ""),"../build/"));
        directory = Path.Combine(directory, "android/");
        Directory.CreateDirectory(directory);

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        bool strip = getOption(args, "stripEngineCode", "false") == "true";
        PlayerSettings.stripEngineCode = strip;
        //PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
        PlayerSettings.companyName = getOption(args, "companyName", "com.dianyuegame");
        PlayerSettings.productName = getOption(args, "productName", "kesulu");
        string ident = PlayerSettings.companyName + "." + PlayerSettings.productName;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, ident);

        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        string pName = $"{getOption(args, "targetName", "kesulu")}_{System.DateTime.Now.ToString("MMdd_HHmm")}";
        string targetDir = Path.Combine(directory, pName + ".apk");
        FileUtil.DeleteFileOrDirectory(targetDir);

        
        BuildOptions option = BuildOptions.None;
        bool development = getOption(args, "development", "true") == "true";
        EditorUserBuildSettings.development = development;
        if(development) {
            option |= BuildOptions.Development;
            option |= BuildOptions.AllowDebugging;
        }
        string[] scenes = GenBuildScene();

        InnerBuildAll(scenes, targetDir, BuildTargetGroup.Android, BuildTarget.Android, option);
    }

    static public void CMD_ClearWrap(){
        CSObjectWrapEditor.Generator.ClearAll();
        CSObjectWrapEditor.Generator.GenAll();
        AssetDatabase.Refresh();
    }

    static public void CMD_BuildResource(){ // async
        BuildAllResource();
    }

    static void CMD_BuildApk(bool isThread){
        string _fp = CSObjectWrapEditor.GeneratorConfig.common_path;
        System.DateTime _ldtime = System.DateTime.UtcNow;
        if(!IsFolder(_fp)){
            CMD_ClearWrap();
            
            if(!isThread){
                System.Threading.Thread.Sleep(8000);
                Debug.LogError("====== please re build apk");
                return;
            }
            
            System.TimeSpan diffSpan;
            do{
                System.DateTime _ntime = System.DateTime.UtcNow;
                diffSpan = _ntime - _ldtime;
                System.Threading.Thread.Sleep(1000);
            }while(diffSpan.TotalSeconds < 50);
            // Debug.LogErrorFormat("======= [{0}] = [{1}] = [{2}]",diffSpan.TotalMinutes,diffSpan.TotalSeconds,diffSpan.TotalMilliseconds);
        }
        
        CMD_BuildResource();

        BuildAndroid();
    }

    [MenuItem("Tools/CMD BuildApk")]
    static void CMD_BuildApk(){
        CMD_BuildApk(false);
    }
}