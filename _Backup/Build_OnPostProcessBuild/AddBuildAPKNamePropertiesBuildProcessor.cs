using System.IO;
using System.Text;
using UnityEditor.Android;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// 设置 APK 的打包出来的apk 名称
    /// </summary>
    public class AddBuildAPKNamePropertiesBuildProcessor : IPostGenerateGradleAndroidProject
    {
        // 同种插件的优先级
        public int callbackOrder => 1000;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            Debug.Log("Build path : " + path);
            AddBuildAPKNameSetting(path);
        }

        private void AddBuildAPKNameSetting(string path)
        {
            string buildGradleFile = path + "/../launcher/build.gradle"; //2019版及其以上使用的路径
            string[] lines = File.ReadAllLines(buildGradleFile);

            StringBuilder stringBuilder = new StringBuilder();
            foreach (string line in lines)
            {
                if (line.Contains("apply plugin: 'com.android.application'"))
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("static def releaseTime() {");
                    stringBuilder.AppendLine(
                        "    return new Date().format(\"yyyy-MM-dd-HH-mm-ss\", TimeZone.getTimeZone(\"GMT+8:00\"))");
                    stringBuilder.AppendLine("}");
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                }
                else if (line.Equals("}"))
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("    android.applicationVariants.all { variant ->");
                    stringBuilder.AppendLine(
                        "        variant.outputs.all {");
                    stringBuilder.AppendLine("            outputFileName =");
                    stringBuilder.AppendLine(
                        "                    \"${defaultConfig.applicationId}_${releaseTime()}_v_${defaultConfig.versionName}_code_${defaultConfig.versionCode}_${variant.buildType.name}.apk\"");
                    stringBuilder.AppendLine("        }");
                    stringBuilder.AppendLine("    }");
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                }

                stringBuilder.AppendLine(line);
            }

            File.WriteAllText(buildGradleFile, stringBuilder.ToString());
        }
    }
}