using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Editor
{
    public static class BuildUpdateAndroidSDKLocationPostProcessor
    {
        [PostProcessBuild(887)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target == BuildTarget.Android)
            {
                UpdateAndroidSDKLocation(path, @"F\:\\AndroidStudioSDK");
            }
        }

        private static void UpdateAndroidSDKLocation(string path, string pathValue)
        {
            string buildGradleFile = path + "/local.properties"; //2019版及其以上使用的路径
            File.WriteAllText(buildGradleFile, $"sdk.dir={pathValue}");
        }
    }
}