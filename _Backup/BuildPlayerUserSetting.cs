using UnityEditor;
namespace Core.Kernel
{
    /// <summary>
    /// 类名 : 资源导出工具基础脚本
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2020-10-14 12:29
    /// 功能 : 抽离为父类
    /// </summary>
    public class BuildPlayerUserSetting
    {
        static protected void LandscapePlatformSetting(BuildTarget buildTarget, string applicationIdentifier, string bundleVersion, string bundleVersionCode, bool isAddBVer = true)
        {
            if (!string.IsNullOrEmpty(applicationIdentifier))
                PlayerSettings.applicationIdentifier = applicationIdentifier;
            // PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, applicationIdentifier);
            string _pre_ver = PlayerSettings.bundleVersion;
            bool _is_ver = !string.IsNullOrEmpty(bundleVersion);
            if (_is_ver)
                PlayerSettings.bundleVersion = bundleVersion;

            int cur = -1, pre = 0;
            if (!string.IsNullOrEmpty(bundleVersionCode))
                int.TryParse(bundleVersionCode, out cur);

            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.MTRendering = true; // 多线程渲染 Multithreaded Rendering
            PlayerSettings.gpuSkinning = true; // 将 Skinning活动 推送到 GPU  Compute SKinning
            PlayerSettings.stripUnusedMeshComponents = true; // optimize mesh data
            ScriptingImplementation scripting = ScriptingImplementation.IL2CPP;
            // EditorUserBuildSettings.activeBuildTarget
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    PlayerSettings.Android.startInFullscreen = true;
                    PlayerSettings.Android.renderOutsideSafeArea = false;
                    scripting = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
                    if (scripting != ScriptingImplementation.IL2CPP)
                        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                    PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
                    PlayerSettings.Android.forceInternetPermission = true;
                    PlayerSettings.Android.forceSDCardPermission = true;
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
                    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
                    PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
                    PlayerSettings.legacyClampBlendShapeWeights = true;
                    PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard_2_0);
                    PlayerSettings.Android.androidTVCompatibility = true;
                    PlayerSettings.Android.androidIsGame = true;
                    pre = PlayerSettings.Android.bundleVersionCode;
                    if (cur <= pre)
                        cur = pre + 1;
                    // if (isAddBVer && _is_ver && !_pre_ver.StartsWith(bundleVersion))
                    //     cur = 1;
                    bundleVersionCode = cur.ToString();
                    PlayerSettings.Android.bundleVersionCode = cur;
                    // PlayerSettings.allowFullscreenSwitch = true;
                    break;
                case BuildTarget.iOS:
                    scripting = PlayerSettings.GetScriptingBackend(BuildTargetGroup.iOS);
                    if (scripting != ScriptingImplementation.IL2CPP)
                        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);

                    int.TryParse(PlayerSettings.iOS.buildNumber, out pre);
                    if (cur <= pre)
                        cur = pre + 1;
                    // if (isAddBVer && _is_ver && !_pre_ver.StartsWith(bundleVersion))
                    //     cur = 1;
                    bundleVersionCode = cur.ToString();
                    PlayerSettings.iOS.buildNumber = bundleVersionCode;
                    break;
            }

            if (isAddBVer)
            {
                if (_is_ver)
                {
                    bundleVersion = bundleVersion + "." + cur;
                    PlayerSettings.bundleVersion = bundleVersion;
                }
                else
                {
                    bundleVersion = LeftLast(_pre_ver, ".", true) + cur;
                    PlayerSettings.bundleVersion = bundleVersion;
                }
            }

            // 设置下开发模式下面的 Profiler 
            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.connectProfiler = true;
            EditorUserBuildSettings.buildWithDeepProfilingSupport = true; // 深度Profiler
			
			/*
			// 或者
			BuildOptions option = BuildOptions.None;
			bool development = true;
			EditorUserBuildSettings.development = development;
			if(development) {
				option |= BuildOptions.Development;
				option |= BuildOptions.ConnectWithProfiler;
				option |= BuildOptions.EnableDeepProfilingSupport;
				option |= BuildOptions.AllowDebugging;
			}
			option |= BuildOptions.CompressWithLz4;
			*/
        }
        
    }
}