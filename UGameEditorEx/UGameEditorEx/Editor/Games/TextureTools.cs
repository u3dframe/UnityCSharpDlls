using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Linq;


public class TextureTools : BuildTools
{
    static bool CheckSngStaticBg = true; // 检查静态背景图
    static void TextureMissingWarning(StreamWriter streamWriter, string prefabPath, string content, string texGuid)
    {
        string pattern = @"m_GameObject:\s*{fileID:\s*(\w+)}[\s\S]*?" + texGuid;
        Match match = Regex.Match(content, pattern, RegexOptions.RightToLeft);
        if (!match.Success) return;
        var objID = match.Groups[1].Value;
        pattern = @"---\s*!u!1\s*&\s*" + objID + @"[\s\S]*?m_Name:\s*(\w+)";
        match = Regex.Match(content, pattern);
        if (!match.Success) return;
        var s = string.Format("文件{0}中的组件{1}有图片丢失！", prefabPath, match.Groups[1].Value);
        streamWriter.WriteLine(s);
        //Debug.LogWarning(s);
    }

    private class GuidRef
    {
        public readonly string guid;
        public readonly HashSet<string> refPrefabs;

        public GuidRef(string guid)
        {
            this.guid = guid;
            refPrefabs = new HashSet<string>();
        }
    }

    static public void MoveAsset(string oldPath, string newPath)
    {
        var newDir = Path.GetDirectoryName(newPath);
        var fullDir = m_dirDataNoAssets + newDir;
        if (!Directory.Exists(fullDir))
        {
            Directory.CreateDirectory(fullDir);
            AssetDatabase.Refresh();
        }
        if (File.Exists(m_dirDataNoAssets + newPath))
        {
            int i = 1;
            var fn = Path.Combine(newDir, Path.GetFileNameWithoutExtension(newPath));
            var ext = Path.GetExtension(newPath);
            var path = fn + "(" + i + ")" + ext;
            while (File.Exists(m_dirDataNoAssets + path))
            {
                i++;
                path = fn + "(" + i + ")" + ext;
            }
            newPath = path;
        }
        var s = AssetDatabase.MoveAsset(oldPath, newPath);
        if (!string.IsNullOrEmpty(s))
            Debug.LogError(oldPath + "移动失败：" + s);
    }

    [MenuItem("Tools/UI Textures/Clean UI Textures and CheckSameIcon", false, 30)]
    static public void CleanUITexturesSameIcon()
    {
        _CleanUITextures(true);
    }

    [MenuItem("Tools/UI Textures/Clean UI Textures", false, 30)]
    static public void CleanUITextures()
    {
        _CleanUITextures();
    }

    static public void _CleanUITextures(bool isSameIcon = false)
    {
        EditorUtility.DisplayProgressBar("Clean UI Textures", "Doing ... ...", 0.1f);
        string dynamicTexDir = "Assets/_Develop/Builds/textures/ui_sngs";
        string sngBgDir = "Assets/_Develop/Builds/textures/ui_sngs/bgs/_statics/"; // 静态不可代码设置的

        //遍历引用图片，生成guid到hash的映射
        string[] uiTexDirs =
        {
            "Assets/_Develop/Builds/textures/ui_atlas",
            "Assets/_Develop/Builds/textures/ui_imgs",
            dynamicTexDir,      //有的prefab引用了该目录下的图片
        };
        string[] guids = AssetDatabase.FindAssets("t:Texture", uiTexDirs);
        var guid2hash = new Dictionary<string, string>();
        for (int i = 0; i < guids.Length; i++)
        {
            var filePath = m_dirDataNoAssets + AssetDatabase.GUIDToAssetPath(guids[i]);
            guid2hash[guids[i]] = CRCClass.GetCRC(filePath);
        }

        //遍历UI prefab，整理重复引用的图片
        var refHash2guid = new Dictionary<string, GuidRef>();
        var unusedTexs = new HashSet<string>();
        string[] uiPrefabDirs =
        {
            "Assets/_Develop/Builds/prefabs/ui"
        };
        guids = AssetDatabase.FindAssets("t:Prefab", uiPrefabDirs);
        Regex regexTex = new Regex(@"m_Sprite:[\r\n\s\t]*{[\r\n\s\t]*fileID:[\r\n\s\t]*\w*[\r\n\s\t]*,[\r\n\s\t]*guid:[\r\n\s\t]*(\w+)[\r\n\s\t]*,[\r\n\s\t]*type:[\r\n\s\t]*3[\r\n\s\t]*}");
        //Regex regexFab = new Regex(@"-\s*target:[\r\n\s\t]*{[\r\n\s\t]*fileID:[\r\n\s\t]*\w*[\r\n\s\t]*,[\r\n\s\t]*guid:[\r\n\s\t]*(\w+)[\r\n\s\t]*,[\r\n\s\t]*type:[\r\n\s\t]*3[\r\n\s\t]*}");
        StreamWriter streamWriter = null;
        string logName = null;
        string logNamePrefix = m_dirDataNoAssets + "Assets/_Develop/Builds/textures/";
        string nowStr = DateTime.Now.ToString("yyyyMMddHHmmss");
        var refTexGuids = new HashSet<string>();
        var didFabGuids = new HashSet<string>();
        string fabGuid = null;
        for (int i = 0; i < guids.Length; i++)
        {
            fabGuid = guids[i];
            if(didFabGuids.Contains(fabGuid))
                continue;
            didFabGuids.Add(fabGuid);

            var prefabAssetPath = AssetDatabase.GUIDToAssetPath(fabGuid);
            var filePath = m_dirDataNoAssets + prefabAssetPath;
            var content = File.ReadAllText(filePath);
            bool isChanged = false;
            var matches = regexTex.Matches(content);
            for (int j = 0; j < matches.Count; j++)
            {
                var texGuid = matches[j].Groups[1].Value;
                var texAssetPath = AssetDatabase.GUIDToAssetPath(texGuid);
                if (string.IsNullOrEmpty(texAssetPath))
                {
                    if (streamWriter == null)
                    {
                        logName = string.Format("{0}missing_images_{1}.txt", logNamePrefix, nowStr);
                        streamWriter = new StreamWriter(new FileStream(logName, FileMode.Create));
                    }
                    TextureMissingWarning(streamWriter, filePath, content, texGuid);
                    continue;
                }
                refTexGuids.Add(texGuid);
                if (texAssetPath.StartsWith(dynamicTexDir) && (!(CheckSngStaticBg && texAssetPath.StartsWith(sngBgDir))))    //不统计icon图片目录 // 检查静态背景图
                    continue;
                if(!guid2hash.ContainsKey(texGuid))
                {
                    Debug.LogErrorFormat("====== [{0}] is not has texture = [{1}]",prefabAssetPath,texAssetPath);
                    continue;
                }
                var hash = guid2hash[texGuid];
                if (refHash2guid.ContainsKey(hash))
                {
                    if (refHash2guid[hash].guid != texGuid)
                    {
                        content = content.Replace(texGuid, refHash2guid[hash].guid);
                        isChanged = true;
                        unusedTexs.Add(texAssetPath);
                    }
                }
                else
                {
                    refHash2guid[hash] = new GuidRef(texGuid);
                }
                refHash2guid[hash].refPrefabs.Add(fabGuid);
            }
            if (isChanged)
                File.WriteAllText(filePath, content);
        }

        if (streamWriter != null)
        {
            streamWriter.Close();
            Debug.LogWarning("prefab的图片丢失信息已写入" + logName);
        }

        EditorUtility.DisplayProgressBar("Clean UI Textures", "Not Use in Prefab ... ...", 0.3f);

        //整理未被prefab引用的图片
        foreach (var pair in guid2hash)
        {
            if (!refTexGuids.Contains(pair.Key))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(pair.Key);
                if (!(assetPath.StartsWith(dynamicTexDir) && (!(CheckSngStaticBg && assetPath.StartsWith(sngBgDir)))))    //不统计icon图片目录 // 检查静态背景图
                    unusedTexs.Add(assetPath);
            }
        }
        refTexGuids.Clear();

        EditorUtility.DisplayProgressBar("Clean UI Textures", "More Use or Not Use ... ...", 0.5f);

        //处理重复引用和未引用图片
        foreach (var path in unusedTexs)
        {
            var newPath = path.Replace("Assets/_Develop/Builds/textures", "Assets/_Develop/Builds/textures/unused");
            newPath = newPath.Replace("ui_atlas/","ui_atlass/");
            newPath = newPath.Replace("ui_sngs/","ui_sngss/");
            MoveAsset(path, newPath);
        }
        unusedTexs.Clear();
        foreach (var pair in refHash2guid)
        {
            var guidRef = pair.Value;
            var assetPath = AssetDatabase.GUIDToAssetPath(guidRef.guid);
            if (assetPath.StartsWith(dynamicTexDir) && (!(CheckSngStaticBg && assetPath.StartsWith(sngBgDir))))
                continue;   //不移动动态引用图片
            var fileName = Path.GetFileName(assetPath);
            var newPath = assetPath;
            var size = GetTextureSize(assetPath);
            var _count = guidRef.refPrefabs.Count;
            if (_count == 1)
            {
                newPath = assetPath.Replace("Assets/_Develop/Builds/textures/ui_atlas", "Assets/_Develop/Builds/textures/ui_imgs");
            }
            else if(size.Item1 <= 768 && size.Item2 <= 432)
            {
                _count = Mathf.Min(_count,6);
                newPath = string.Format("Assets/_Develop/Builds/textures/ui_atlas/coms{0}/{1}",(6 - _count),fileName);
            }
            else
            {
                newPath = sngBgDir + fileName;
            }
            if (newPath != assetPath)
            {
                MoveAsset(assetPath, newPath);
            }
        }
        refHash2guid.Clear();

        _RemoveEmptyFolders(0.8f);

        EditorUtility.DisplayProgressBar("Clean UI Textures", "Same Icons ... ...", 0.9f);
        if(isSameIcon)
        {
            //统计相同的动态引用图片
            guids = AssetDatabase.FindAssets("t:Texture", new string[1] { dynamicTexDir });
            var dynamicTexRef = new Dictionary<string, List<string>>();
            bool hasDup = false;
            for (int i = 0; i < guids.Length; i++)
            {
                var hash = guid2hash[guids[i]];
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (dynamicTexRef.ContainsKey(hash))
                {
                    dynamicTexRef[hash].Add(path);
                    hasDup = true;
                }
                else
                    dynamicTexRef[hash] = new List<string> { path };
            }
            if (hasDup)
            {
                logName = string.Format("{0}same_icons_{1}.txt", logNamePrefix, nowStr);
                streamWriter = new StreamWriter(new FileStream(logName, FileMode.Create));
                foreach (var pair in dynamicTexRef)
                {
                    var list = pair.Value;
                    if (list.Count > 1)
                    {
                        streamWriter.WriteLine(pair.Key);
                        for (int i = 0; i < list.Count; i++)
                            streamWriter.WriteLine("\t" + list[i]);
                        streamWriter.WriteLine();
                    }
                }
                streamWriter.Close();
                Debug.LogWarning("相同的icon图片信息已写入" + logName);
            }
        }

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/UI Textures/Statistics SameIcons", false, 30)]
    static void StatisticsSameIcons(){
        EditorUtility.DisplayProgressBar("Statistics UITexuture Same Icons", "Doing ... ...", 0.1f);
        //统计相同的动态引用图片
        string dynamicTexDir = "Assets/_Develop/Builds/textures/ui_sngs",logName;
        string[] guids = AssetDatabase.FindAssets("t:Texture", new string[1] { dynamicTexDir });
        var guid2hash = new Dictionary<string, string>();
        for (int i = 0; i < guids.Length; i++)
        {
            var filePath = m_dirDataNoAssets + AssetDatabase.GUIDToAssetPath(guids[i]);
            guid2hash[guids[i]] = CRCClass.GetCRC(filePath);
        }

        var dynamicTexRef = new Dictionary<string, List<string>>();
        bool hasDup = false;
        for (int i = 0; i < guids.Length; i++)
        {
            var hash = guid2hash[guids[i]];
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (dynamicTexRef.ContainsKey(hash))
            {
                dynamicTexRef[hash].Add(path);
                hasDup = true;
            }
            else
                dynamicTexRef[hash] = new List<string> { path };
        }
        if (hasDup)
        {
            string logNamePrefix = m_dirDataNoAssets + "Assets/_Develop/Builds/textures/";
            string nowStr = DateTime.Now.ToString("yyyyMMddHHmmss");
            logName = string.Format("{0}same_icons_{1}.txt", logNamePrefix, nowStr);
            StreamWriter streamWriter = new StreamWriter(new FileStream(logName, FileMode.Create));
            foreach (var pair in dynamicTexRef)
            {
                var list = pair.Value;
                if (list.Count > 1)
                {
                    streamWriter.WriteLine(pair.Key);
                    for (int i = 0; i < list.Count; i++)
                        streamWriter.WriteLine("\t" + list[i]);
                    streamWriter.WriteLine();
                }
            }
            streamWriter.Close();
            Debug.LogWarning("相同的icon图片信息已写入" + logName);
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/UI Textures/Remove Empty Folder", false, 30)]
    static void RemoveEmptyFolders(){
        _RemoveEmptyFolders(0.2f);
    }

    static bool _DeletEmptyFolder(string assetPath)
    {
        // 删除空文件夹
        DirectoryInfo infos = new DirectoryInfo(m_dirDataNoAssets + assetPath);
        bool _isChg = false;
        foreach (var item in infos.GetDirectories())
        {
            var files = item.GetFiles().Where(pp => Path.GetExtension(pp.FullName) != ".meta");
            if(files == null || files.Count() <= 0)
            {
                item.Delete(true);
                _isChg = true;
            }
        }
        return _isChg;
    }

    static void _RemoveEmptyFolders(float curVal){
        //统计相同的动态引用图片
        EditorUtility.DisplayProgressBar("Clean UI Textures", "Dele ui_atlas Empty ... ...", curVal);
        // 删除空文件夹
        bool _isChg = _DeletEmptyFolder("Assets/_Develop/Builds/textures/ui_atlas");
        curVal += 0.1f;
        EditorUtility.DisplayProgressBar("Clean UI Textures", "Dele ui_imgs Empty ... ...",curVal);
        _isChg = _DeletEmptyFolder("Assets/_Develop/Builds/textures/ui_imgs") || _isChg;
        if(_isChg)
            AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }


    [MenuItem("Tools/UI Textures/ChangeNameAndPath", false)]
    static void ChangeNameAndPath()
    {
        EditorUtility.DisplayProgressBar("ChangeNameAndPath", "Doing ... ...", 0.1f);
        //遍历引用图片，生成guid到hash的映射
        string[] uiTexDirs =
        {
            "Assets/_Develop/Builds/textures/ui_atlas",
            
        };
        string[] guids = AssetDatabase.FindAssets("t:Texture", uiTexDirs);
        for (int i = 0; i < guids.Length; i++)
        {
            var texturePath = AssetDatabase.GUIDToAssetPath(guids[i]);
            _ChangeNameAndPath(texturePath);
        }
        _RemoveEmptyFolders(0.2f);
        EditorUtility.ClearProgressBar();
    }

    static void _ChangeNameAndPath(string texturePath)
    {

        string[] strs = texturePath.Split('/');
        int _index = strs.Length - 2;
        string qianzui = strs[_index];
        qianzui = qianzui +  '_';
        string old_name = strs[strs.Length - 1];
        bool ishas =  old_name.StartsWith(qianzui);
        string Name = ishas ? old_name : qianzui + old_name;
        var newPath = texturePath.Remove(texturePath.Length - old_name.Length) + Name;
        newPath = texturePath.Replace(texturePath, newPath);
        newPath = "Assets/_Develop/Builds/textures/ui_imgs" + "/" + Name;
        MoveAsset(texturePath, newPath);

    }

}

