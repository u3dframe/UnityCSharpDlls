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
using UPPrefs = UnityEngine.PlayerPrefs;

/// <summary>
/// 类名 : 资源导出工具脚本 
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-03-07 09:29
/// 功能 : 将protobuf 文件转为 lua 文件
/// 修改 : 2020-03-26 09:29
/// </summary>
public class BuildTools : BuildPatcher
{
	static bool _CheckTimeline(){
        bool _isChg = false;
        string[] searchInFolders = {
            "Assets/_Develop/Characters/Builds/timeline"
        };
        string[] _tes = AssetDatabase.FindAssets("t:TimelineAsset",searchInFolders);
        string _assetPath,_filePath,_fcont,_fcont2;
        for (int i = 0; i < _tes.Length; i++)
        {
            _assetPath = AssetDatabase.GUIDToAssetPath(_tes[i]);
            _filePath =  m_dirDataNoAssets + _assetPath;
            _fcont = GetText4File(_filePath);
            _fcont2 = _fcont.Replace("m_ObjectHideFlags: 52","m_ObjectHideFlags: 0");
            if(!_fcont2.Equals(_fcont))
            {
                WriteFile(_filePath,_fcont2);
                Debug.LogErrorFormat("=== Timeline isChange == [{0}]",_assetPath);
                _isChg = true;
            }
        }
        return _isChg;
    }

    [MenuItem("Tools/CheckTimeline",false,50)]
    static public void CMD_CheckTimeline(){
        bool isChg = _CheckTimeline();
        if(isChg){
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    [MenuItem("Tools/CheckPrefab",false,50)]
    static public void CheckPrefab(){
        EditorUtility.DisplayProgressBar("CheckPrefab", "CheckPrefab Start", 0.01f);
        string _check = "sinfo_skill_test";
        string[] searchInFolders = {
            "Assets"
        };
        string[] _tes = AssetDatabase.FindAssets("t:Prefab",searchInFolders);
        int _lens = _tes.Length;
        string _assetPath,_filePath,_fcont;
        for (int i = 0; i < _lens; i++)
        {
            _assetPath = AssetDatabase.GUIDToAssetPath(_tes[i]);
            _filePath =  m_dirDataNoAssets + _assetPath;
            _fcont = GetText4File(_filePath);
            EditorUtility.DisplayProgressBar(string.Format("CheckPrefab ({0}/{1})",(i + 1),_lens),_assetPath, i / (float)_lens);
            if(_fcont.Contains(_check))
            {
                Debug.LogErrorFormat("=== Has [{0}] == [{1}]",_check,_assetPath);
            }
        }
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("CheckPrefab","isOver","Okey","Yes");
    }

    [MenuItem("Tools/FindAssetByGUIDs",false,50)]
    static void FindAssetByGUIDs()
    {
        try
        {
            string _edGUIDS = "Assets/Editor/Games/Build_Patcher/guids.txt";
            string _fp = m_dirDataNoAssets + _edGUIDS;
            string[] _lines = File.ReadAllLines(_fp);
            string _guid,_assetPath;
             for (int i = 0; i < _lines.Length; i++)
            {
                _guid = _lines[i];
                _assetPath = AssetDatabase.GUIDToAssetPath(_guid);
                Debug.LogFormat("===== GUID = [{0}]  ,  [{1}]",_guid,_assetPath);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }
	
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

    static void InnerBuildAll(string []scenes, string outpath, BuildTargetGroup targetgroup, BuildTarget target, BuildOptions option)
    {
        EditorUtility.DisplayProgressBar("BuidPlayer", "Switch Targe Group", 0.1f);
        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(targetgroup, target);
        option |= BuildOptions.CompressWithLz4;
        AssetDatabase.Refresh();

        FileUtil.DeleteFileOrDirectory(m_dirStreaming);
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayProgressBar("BuidPlayer", "Compress Resource", 0.4f);
        Zip_Main();
        AssetDatabase.Refresh();

        EditorUtility.DisplayProgressBar("BuidPlayer", "Build Runtime", 0.5f);
        UnityEditor.Build.Reporting.BuildReport ret = BuildPipeline.BuildPlayer(scenes, outpath, target, option);

        EditorUtility.DisplayProgressBar("BuidPlayer", "Clean tmp files", 0.9f);
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

    [MenuItem("Tools/CMD BuildAndroid")]
    static public void BuildAndroid()
    {
        Core.GameFile.CurrDirRes();
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
        string choiceSvlist = getOption(args, "choiceSvlist","");
        CopySVList(choiceSvlist);
        
        AssetDatabase.Refresh();
        string directory = getOption(args, "targetDir", Path.Combine(Application.dataPath.Replace("/Assets", ""),"../build/"));
        directory = Path.Combine(directory, "android/");
        Directory.CreateDirectory(directory);
        //PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        //PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
        bool strip = getOption(args, "stripEngineCode", "false") == "true";
        PlayerSettings.stripEngineCode = strip;
        //PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
        PlayerSettings.companyName = getOption(args, "companyName", "com.dianyuegame");
        PlayerSettings.productName = getOption(args, "productName", "kesulu");
        string ident = PlayerSettings.companyName + "." + PlayerSettings.productName;
		string bundleVersion = getOption(args, "bundleVersion", "1.0");
        string bundleVersionCode = getOption(args, "bundleVersionCode",null);
		LandscapePlatformSetting(BuildTarget.Android,ident,ref bundleVersion,ref bundleVersionCode);
        //PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, ident);
        //PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        //PlayerSettings.allowedAutorotateToLandscapeRight = true;
        //PlayerSettings.allowedAutorotateToPortrait = false;
        //PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        string pName = $"{getOption(args, "targetName", "kesulu")}_{System.DateTime.Now.ToString("MMdd_HHmm")}_ver{bundleVersion}_code{bundleVersionCode}";
        string targetDir = Path.Combine(directory, pName + ".apk");
        FileUtil.DeleteFileOrDirectory(targetDir);

        
        BuildOptions option = BuildOptions.None;
        bool development = getOption(args, "development", "false") == "true";
        EditorUserBuildSettings.development = development;
        if(development) {
            option |= BuildOptions.Development;
            option |= BuildOptions.ConnectWithProfiler;
            option |= BuildOptions.EnableDeepProfilingSupport;
            option |= BuildOptions.AllowDebugging;
        }
        string[] scenes = GenBuildScene();

        InnerBuildAll(scenes, targetDir, BuildTargetGroup.Android, BuildTarget.Android, option);
    }

    static public void CMD_ClearWrap(){
        CMD_ClearCSWrap();
        CMD_GenCSWrap();
    }

    static public void CMD_ClearCSWrap(){
        CSObjectWrapEditor.Generator.ClearAll();
        AssetDatabase.Refresh();
    }

    static public void CMD_GenCSWrap(){
        CSObjectWrapEditor.Generator.GenAll();
        AssetDatabase.Refresh();
    }

    static public void CMD_BuildResource(){ // async
        CMD_CheckTimeline();
        BuildAllResource();
    }

    static public void SaveDefaultCfgVersion(){
        CfgVersion.instance.LoadDefault4EDT();
        CfgVersion.instance.SaveDefault();
    }

    [MenuItem("Tools/ZipMain")]
    static public void Zip_Main(){
        m_luacExe = "D:/lua-5.3.5_w64/luac.exe";
        SaveDefaultCfgVersion();
        BuildPatcher.ZipMain();
        // BuildPatcher2.ZipMain();
        // BuildPatcher.CopyTest();
    }
	
	 static public void CopySVList(string suff = ""){
        string _fname = "severlist";
        if((!string.IsNullOrEmpty(suff)) && (!"default".Equals(suff) && !"def".Equals(suff)))
            _fname = string.Concat(_fname,suff);
        string _fp = string.Format("{0}/../_svlists/{1}.lua", Application.dataPath,_fname);
        string _fpDest = string.Format("{0}/Lua/games/net/severlist.lua", Application.dataPath);
        FileInfo fInfo = new FileInfo(_fp);
        fInfo.CopyTo(_fpDest, true);
    }

    // change net 2 out(切为外网)
    // change net 2 in(切为内网)
    [MenuItem("Tools/ChangeNet/切为内网",false,50)]
    static void Net2In(){
        CopySVList("");
    }

    [MenuItem("Tools/ChangeNet/切为外网(测试服)",false,50)]
    static void Net2Out(){
        CopySVList("_sdk173");
    }

    [MenuItem("Tools/ChangeNet/切为内网QA",false,50)]
    static void Net2InQA(){
        CopySVList("_qa");
    }

    [MenuItem("Tools/ChangeNet/切为外网(QA)",false,50)]
    static void Net2Out_QA(){
        CopySVList("_sdk173_qa2");
    }

    [MenuItem("Tools/Clean(清除-本地缓存)",false,50)]
    static void CleanPlayerPrefs()
    {
        UPPrefs.DeleteAll();
    }
	
	const string ExploreIsEditorTitleKey = "IsExploreIsEditorTitle";
    const string ExploreIsEditorTitleName = "Tools/探索场景修改名称坐标(不能游戏中设置)";
    const string BattleChoicePrintDataKey = "BattleChoicePrintData";
    const string BattleChoicePrintDataName = "Tools/打印开启布阵界面系统传入数据(不能游戏中设置)";
    [MenuItem(ExploreIsEditorTitleName, true)]
    public static bool CheckPlatform()
    {
        int platform = UPPrefs.GetInt(ExploreIsEditorTitleKey, 0);
        Menu.SetChecked(ExploreIsEditorTitleName, platform == 1);
        platform = UPPrefs.GetInt(BattleChoicePrintDataKey, 0);
        Menu.SetChecked(BattleChoicePrintDataName, platform == 1);
        return true;
    }

    [MenuItem(ExploreIsEditorTitleName)]
    static void SetExploreIsEditorTitle()
    {
        int platform = UPPrefs.GetInt(ExploreIsEditorTitleKey, 0);
        UPPrefs.SetInt(ExploreIsEditorTitleKey, platform == 0 ? 1 : 0);
    }

    [MenuItem(BattleChoicePrintDataName)]
    static void BattleChoicePrintDataTool()
    {
        int platform = UPPrefs.GetInt(BattleChoicePrintDataKey, 0);
        UPPrefs.SetInt(BattleChoicePrintDataKey, platform == 0 ? 1 : 0);
    }

    [MenuItem("Tools/ZipMainChild")]
    static public void Zip_MainChild(){
        SaveDefaultCfgVersion();
        BuildPatcher.ZipMainChild();
    }

    [MenuItem("Tools/ZipMainObb")]
    static public void Zip_MainObb(){
        SaveDefaultCfgVersion();
        BuildPatcher.ZipMainObb();
    }

    [MenuItem("Tools/ZipPatche")]
    static public void Zip_Patche(){
        SaveDefaultCfgVersion();
        BuildPatcher.ZipPatche();
    }
}