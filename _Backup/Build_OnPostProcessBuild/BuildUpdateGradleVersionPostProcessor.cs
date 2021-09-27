using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Editor
{
    public static class BuildUpdateGradleVersionPostProcessor
    {
        [PostProcessBuild(888)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target == BuildTarget.Android)
            {
                Debug.Log(path);
                UpdateGradleVersion(path, "4.1.3");
                UpdateGradleWrapperPropertiesVersion(path, "6.5");
            }
        }

        static void UpdateGradleVersion(string path, string val)
        {
            string buildGradleFile = path + "/build.gradle"; //2019版及其以上使用的路径

            string[] lines = File.ReadAllLines(buildGradleFile);

            StringBuilder stringBuilder = new StringBuilder();
            foreach (string line in lines)
            {
                if (line.Contains("com.android.tools.build"))
                {
                    stringBuilder.AppendLine($"            classpath 'com.android.tools.build:gradle:{val}'");
                    continue;
                }

                stringBuilder.AppendLine(line);
            }

            File.WriteAllText(buildGradleFile, stringBuilder.ToString());
        }

        static void UpdateGradleWrapperPropertiesVersion(string path, string val)
        {
            string savePath = Path.GetFullPath(path + "/gradle/wrapper/");
            string gradleWrapperProperties = savePath + "gradle-wrapper.properties";

            StringBuilder stringBuilder = new StringBuilder();
            if (File.Exists(gradleWrapperProperties))
            {
                string[] lines = File.ReadAllLines(gradleWrapperProperties);


                foreach (string line in lines)
                {
                    if (line.Contains("distributionUrl="))
                    {
                        stringBuilder.AppendLine("distributionUrl=https\\://services.gradle.org/distributions/gradle-" +
                                                 val +
                                                 "-bin.zip");
                        continue;
                    }

                    stringBuilder.AppendLine(line);
                }
            }
            else
            {
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                stringBuilder.AppendLine("# " + DateTime.Now.ToString("R"));
                stringBuilder.AppendLine("distributionBase=GRADLE_USER_HOME");
                stringBuilder.AppendLine("distributionPath=wrapper/dists");
                stringBuilder.AppendLine("zipStoreBase=GRADLE_USER_HOME");
                stringBuilder.AppendLine("zipStorePath=wrapper/dists");
                stringBuilder.AppendLine("distributionUrl=https\\://services.gradle.org/distributions/gradle-" +
                                         val +
                                         "-bin.zip");
            }

            File.WriteAllText(gradleWrapperProperties, stringBuilder.ToString());
        }
    }
}