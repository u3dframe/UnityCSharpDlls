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
using LitJson;

/// <summary>
/// 类名 : 资源导出工具脚本 
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-03-07 09:29
/// 功能 : 将protobuf 文件转为 lua 文件
/// 修改 : 2020-03-26 09:29
/// </summary>
public class BuildTools : BuildPatcher
{
	[MenuItem("Tools/ShaderName",false,50)]
    static public void CMD_ShaderName(){
        EditorUtility.DisplayProgressBar("ShaderName", "Checking", 0.1f);
        string[] searchInFolders = {
            "Assets"
        };

        string[] _tes = AssetDatabase.FindAssets("t:Material",searchInFolders);
        string _assetPath,_sname;
        Material _mat = null;
        int _len = _tes.Length;
        for (int i = 0; i < _len; i++)
        {
            _assetPath = AssetDatabase.GUIDToAssetPath(_tes[i]);
            EditorUtility.DisplayProgressBar("ShaderName", _assetPath, i / (float)_len);
            _mat = AssetDatabase.LoadAssetAtPath<Material>(_assetPath);
            _sname = _mat.shader.name;
            if(_sname.Contains("Standard")){
                Debug.LogErrorFormat("=== Standard == [{0}] = [{1}]",_assetPath,_sname);
            }
        }
        EditorUtility.ClearProgressBar();
    }
	
	static bool CleanUpPlayableBind(UnityEngine.Playables.PlayableDirector playable) {
        Dictionary<UnityEngine.Object, UnityEngine.Object> bindings = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
        foreach (var pb in playable.playableAsset.outputs){
            var key = pb.sourceObject;
            var value = playable.GetGenericBinding(key);
            if (key != null && value != null && !bindings.ContainsKey(key)){
                bindings.Add(key, value);
            }else{
                Debug.LogErrorFormat("===err== [{0}] = [{1}] , [{0}] = [{1}]",key,value,key == null,value == null);
            }
        }
        int lens = bindings.Count;
        
        var dirSO = new UnityEditor.SerializedObject(playable);
        var sceneBindings = dirSO.FindProperty("m_SceneBindings");
        for (var i = sceneBindings.arraySize - 1; i >= 0; i--)
        {
            var binding = sceneBindings.GetArrayElementAtIndex(i);
            var key = binding.FindPropertyRelative("key");
            if (key.objectReferenceValue == null || !bindings.ContainsKey(key.objectReferenceValue))
                sceneBindings.DeleteArrayElementAtIndex(i);
        }
        dirSO.ApplyModifiedProperties();
        return lens > 0;
    }

    [MenuItem("Tools/CleanUpPlayableBind",false,50)]
    static public void CMD_CleanUpPlayableBind(){
        string[] searchInFolders = {
            "Assets/_Develop/Characters/Builds/timeline"
        };
        string[] _tes = AssetDatabase.FindAssets("t:Prefab",searchInFolders);
        string _assetPath;
        GameObject _gobj;
        UnityEngine.Playables.PlayableDirector playable;
        bool _isChg = false;
        bool _isCur = false;
        for (int i = 0; i < _tes.Length; i++)
        {
            _assetPath = AssetDatabase.GUIDToAssetPath(_tes[i]);
            _gobj = AssetDatabase.LoadAssetAtPath<GameObject>(_assetPath);
            playable = _gobj.GetComponentInChildren<UnityEngine.Playables.PlayableDirector>(true);
            _isCur = CleanUpPlayableBind(playable);
            _isChg = _isChg || _isCur;
        }
        if(_isChg){
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
	
    static bool _CheckTimeline(){
        bool _isChg = false;
        string[] searchInFolders = {
            "Assets/_Develop/Characters/Builds/timeline"
        };

        string _tlskill = "Assets/_Develop/Scene/Builds/prefabs/skill_test/skill_camera.prefab";
        string _guidmcam = AssetDatabase.AssetPathToGUID(_tlskill);
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
                Debug.LogFormat("=== Timeline isChange == [{0}]",_assetPath);
                _isChg = true;
            }
            string _name = GetFileNameNoSuffix(_assetPath).Replace("tl_","");
            int _ind = _name.IndexOf("_");
            if(_ind > 0)
                _name = _name.Substring(0,_ind);
            string[] _arrs = AssetDatabase.GetDependencies(_assetPath);
            foreach (var item in _arrs)
            {
                if(!item.Contains(_name) && item.EndsWith(".anim"))
                {
                    Debug.LogErrorFormat("=== Timeline == [{0}] Has Other Asset = [{1}]",_assetPath,item);
                }
            }

            if(!string.IsNullOrEmpty(_guidmcam) && _fcont2.Contains(_guidmcam)){
                Debug.LogErrorFormat("=== Timeline == [{0}] Has skill_camera",_assetPath);
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

    [MenuItem("Tools/CheckBigTexture",false,50)]
    static public void CMD_CheckBigTexture(){
        EditorUtility.DisplayProgressBar("CheckBigTexture", "Checking", 0.1f);
        string[] searchInFolders = {
            "Assets/_Develop"
        };
        string[] _tes = AssetDatabase.FindAssets("t:Texture",searchInFolders);
        string _assetPath;
        System.Text.StringBuilder _sbd = new System.Text.StringBuilder();
        _sbd.Append("Texture's Size  >  256x256").AppendLine();
        int _len = _tes.Length;
        for (int i = 0; i < _len; i++)
        {
            _assetPath = AssetDatabase.GUIDToAssetPath(_tes[i]);
            var _size = GetTextureSize(_assetPath);
            EditorUtility.DisplayProgressBar("CheckBigTexture", _assetPath, i / (float)_len);
            if(_size.Item1 > 256 && _size.Item2 > 256){
                _sbd.AppendFormat("{0}    {1}x{2}",_assetPath,_size.Item1,_size.Item2).AppendLine();
            }
        }
        string _cont = _sbd.ToString();
        _sbd.Clear();
        _sbd.Length = 0;
        EditorUtility.ClearProgressBar();

        string _fp = String.Format("{0}../{1}.txt",m_dirRes,DateTime.Now.ToString("MMddHHmmss"));
        WriteText(_fp,_cont,true);
        Debug.LogErrorFormat("===== write to {0}",_fp);
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

    [MenuItem("Tools/CMD BuildPatcher")]
    static public void BuildPatcher() {
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
        string choiceChannel = getOption(args, "choiceChannel","kp_and");
        CopySVList(choiceSvlist, choiceChannel);
        AssetDatabase.Refresh();
        string directory = getOption(args, "targetDir","");
        SetZipBackup(directory,choiceChannel);
        Zip_Patch();
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
        string choiceChannel = getOption(args, "choiceChannel","kp_and");
        CopySVList(choiceSvlist, choiceChannel);
        
        AssetDatabase.Refresh();
        string directory = getOption(args, "targetDir", Path.Combine(Application.dataPath.Replace("/Assets", ""),"../build/"));
        SetZipBackup(directory,choiceChannel);
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

    static public void SaveDefaultCfgVersion(string chn="kp_and"){
        if(string.IsNullOrEmpty(chn))
            chn = "kp_and";
        CfgVersion _cVer = CfgVersion.instance;
        _cVer.LoadDefault4EDT();
        _cVer.m_pkgVersion = chn;
        _cVer.m_pkgFilelist = chn;
        _cVer.m_pkgFiles = chn+"/files";
        _cVer.SaveDefault();
    }

    static void SetZipBackup(string dir,string fparent)
    {
        m_fdZipBackup = dir;
        m_fparentZipBackup = fparent;
    }

    static public void Zip_Main(){
        m_luacExe = "D:/lua-5.3.5_w64/luac.exe";
        SaveDefaultCfgVersion(m_fparentZipBackup);
        ZipMain();
    }

    static public void Zip_Patch(){
        SaveDefaultCfgVersion(m_fparentZipBackup);
        ZipPatche();
    }

    static public void CopySVList(string fname = "", string ver="kp_and"){
        if (string.IsNullOrEmpty(fname))
            fname = "default";
        string _fp = string.Format("{0}/../_svlists/{1}.json", Application.dataPath,fname);
        var data = LJsonHelper.ToJData(File.ReadAllText(_fp));
        string _fpTemp = string.Format("{0}/../_svlists/serverlist.lua", Application.dataPath);
        string _fpDest = string.Format("{0}/Lua/games/net/severlist.lua", Application.dataPath);
        if (File.Exists(_fpTemp))
        {
            string text = File.ReadAllText(_fpTemp);
            foreach (var key in data.Keys)
                text = text.Replace(key, LJsonHelper.ToStr(data, key));
            CreateFolder(_fpDest);
            File.WriteAllText(_fpDest, text);
        }
        else
            Debug.LogError("Cannot find file: " + _fpTemp);
        string _fpTemp2 = string.Format("{0}/../_svlists/cfg_game_package.json", Application.dataPath);
        string _fpDest2 = string.Format("{0}/Plugins/{1}/assets/cfg_game_package.json", Application.dataPath,m_curPlatform);
        if (File.Exists(_fpTemp2))
        {
            string text = File.ReadAllText(_fpTemp2);
            foreach (var key in data.Keys)
                text = text.Replace(key, LJsonHelper.ToStr(data, key));
            text = text.Replace("__UPDATEVER__",ver);
            CreateFolder(_fpDest2);
            File.WriteAllText(_fpDest2, text);
        }
        else
            Debug.LogError("Cannot find file: " + _fpTemp2);
    }

    // change net 2 out(切为外网)
    // change net 2 in(切为内网)
    [MenuItem("Tools/ChangeNet/切为内网",false,50)]
    static void Net2In(){
        CopySVList("");
    }

    [MenuItem("Tools/ChangeNet/切为外网(测试服)",false,50)]
    static void Net2Out(){
        CopySVList("sdk173");
    }

    [MenuItem("Tools/ChangeNet/切为内网QA",false,50)]
    static void Net2InQA(){
        CopySVList("qa");
    }

    [MenuItem("Tools/ChangeNet/切为外网(QA)",false,50)]
    static void Net2Out_QA(){
        CopySVList("sdk173_qa2");
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
	
	[MenuItem("Tools/PatcheCompareFiles(更新测试)",false,50)]
    static void PatcheCompareFiles()
    {
        
        string fpOld = string.Format("{0}/../../_fls/filelist_old.txt", Application.dataPath);
        string fpNew = string.Format("{0}/../../_fls/filelist.txt", Application.dataPath);
        LogPatcheFiles(fpOld,fpNew);

        Core.GameFile.CurrDirRes();
        string _destFp = string.Concat(m_dirRes, "filelist.txt");
        Debug.LogError(_destFp);
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