using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;
using Core.Kernel;
using System.Linq;
using UObject = UnityEngine.Object;
namespace Core
{
    /// <summary>
    /// 类名 : 资源导出工具基础脚本
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2020-10-14 12:29
    /// 功能 : 抽离为父类
    /// </summary>
    public class BuildBasic : EditorGameFile
    {
        static public T[] GetSelectObject<T>()
        {
            return Selection.GetFiltered<T>(SelectionMode.Assets | SelectionMode.DeepAssets);
        }

        // Resources.FindObjectsOfTypeAll 取得的是 指向 Library 目录下面的资源
        static public T[] FindObjectsOfTypeAll<T>() where T : UObject
        {
            return UnityEngine.Resources.FindObjectsOfTypeAll<T>();
        }

        // EditorUtility 取得的是 指向 Library 目录下面
        static public UObject[] CollectDependencies(UObject obj)
        {
            UObject[] roots = new UObject[] { obj };
            return EditorUtility.CollectDependencies(roots);
        }

        static public string[] GetDependencies(string objAsset, bool recursive)
        {
            return AssetDatabase.GetDependencies(objAsset, recursive);
        }

        static public string[] GetFiles(string fpdir)
        {
            return Directory.GetFiles(fpdir, "*.*", SearchOption.AllDirectories);
        }

        static public string[] GetFilesBySuffix(string fpdir,string suffixToLowers)
        {
            string[] _files = GetFiles(fpdir);
            if (string.IsNullOrEmpty(suffixToLowers))
                return _files;
            return _files.Where(s => suffixToLowers.Contains(GetSuffixToLower(s))).ToArray();
        }

        static public void SaveAssets(UObject obj, bool isSave = true)
        {
            if (obj != null)
                EditorUtility.SetDirty(obj); //这一行一定要加！！！
            if (isSave)
                AssetDatabase.SaveAssets(); //以及最后记得要保存资源的修改
        }

        /// <summary>
        /// 判断Object是否是预制体资源。
        /// </summary>
        /// <param name="includePrefabInstance">是否将预制体资源的Scene实例视为预制体资源？</param>
        /// <returns>如果是则返回 `true` ，如果不是则返回 `false` 。</returns>
        static public bool IsPrefabAsset(UObject obj, bool includePrefabInstance)
        {
            if (!obj)
            {
                return false;
            }

            var type = PrefabUtility.GetPrefabAssetType(obj);
            if (type == PrefabAssetType.NotAPrefab)
            {
                return false;
            }

            var status = PrefabUtility.GetPrefabInstanceStatus(obj);
            if (status != PrefabInstanceStatus.NotAPrefab && !includePrefabInstance)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判断GameObject是否是预制体资源的实例。
        /// </summary>
        /// <param name="includeMissingAsset">是否将丢失预制体关联的GameObject视为预制体实例？</param>
        /// <returns>如果是则返回 `true` ，如果不是则返回 `false` 。</returns>
        static public bool IsPrefabInstance(UnityEngine.GameObject gobj, bool includeMissingAsset)
        {
            if (!gobj)
            {
                return false;
            }

            var type = PrefabUtility.GetPrefabAssetType(gobj);
            if (type == PrefabAssetType.NotAPrefab || (!includeMissingAsset && type == PrefabAssetType.MissingAsset))
            {
                return false;
            }

            var status = PrefabUtility.GetPrefabInstanceStatus(gobj);
            if (status == PrefabInstanceStatus.NotAPrefab)
            {
                return false;
            }
            return true;
        }

        const string _fnSharder = "shaders.ab_shader";

        public static void ClearBuild()
        {
            MgrABDataDependence.ClearDeps();
        }

        // 分析文件夹 - 得到所有文件的依赖关系
        public static void AnalyseDir4Deps(UObject obj)
        {
            if (obj == null)
                return;

            string strObjPath = GetPath(obj);
#if Shader2OneAB
		if (IsShader(strObjPath)) {
			SetABInfo(strObjPath,_fnSharder);
			return;
		}
#endif
            EL_Path.Init(strObjPath);
            float count = EL_Path.files.Count;
            int curr = 0;
            string _tmp = "";
            EditorUtility.DisplayProgressBar("Analysis Dependence Init", strObjPath, 0.00f);

            foreach (var item in EL_Path.files)
            {
                _tmp = Path2AssetsStart(item);
                AnalyseFile4Deps(_tmp);
                curr++;
                EditorUtility.DisplayProgressBar(string.Format("{0} - ({1}/{2})", strObjPath, curr, count), _tmp, (curr / count));
            }
            EditorUtility.ClearProgressBar();
        }

        // 分析文件的依赖关系
        public static void AnalyseFile4Deps(UObject obj)
        {
            string strObjPath = GetPath(obj);
            AnalyseFile4Deps(strObjPath);
        }

        public static void AnalyseFile4Deps(string assetPath)
        {
            bool isMust = false;
            if (!IsInBuild(assetPath, ref isMust))
                return;

            MgrABDataDependence.Init(assetPath, isMust);
        }

        static public void ClearObjABName(string abname)
        {
            string[] _arrs = AssetDatabase.GetAssetPathsFromAssetBundle(abname);
            if (_arrs == null || _arrs.Length <= 0)
                return;

            foreach (string assetPath in _arrs)
            {
                SetABInfo(assetPath);
            }
        }

        static int _CheckABName(bool isCheck = true)
        {
            EditorUtility.DisplayProgressBar("DoBuild", "CheckABName ...", 0.0f);
            AssetDatabase.RemoveUnusedAssetBundleNames();
            string[] strABNames = AssetDatabase.GetAllAssetBundleNames();
            if(!isCheck)
                return strABNames.Length;

            float count = strABNames.Length;
            string abName;
            for (int curr = 0; curr < count; curr++)
            {
                abName = strABNames[curr];
                EditorUtility.DisplayProgressBar(string.Format("CheckABName - ({0}/{1})", curr, count), abName, (curr / count));
                if (abName.EndsWith("error"))
                {
                    ClearObjABName(abName);
                    AssetDatabase.RemoveAssetBundleName(abName, true);
                    Debug.LogErrorFormat("===  _CheckABName Error ABName = [{0}]", abName);
                }
            }
            AssetDatabase.RemoveUnusedAssetBundleNames();
            strABNames = AssetDatabase.GetAllAssetBundleNames();
            EditorUtility.DisplayProgressBar("DoBuild", "RemoveUnusedAssetBundleNames ...", 0.1f);
            return strABNames.Length;
        }

        static public void ClearAllABNames(bool isClearBuild = true)
        {
            EditorUtility.DisplayProgressBar("Clear", "ClearABName ...", 0.0f);
            AssetDatabase.RemoveUnusedAssetBundleNames();
            string[] arrs = AssetDatabase.GetAllAssetBundleNames();
            float count = arrs.Length;
            int curr = 0;
            foreach (string abName in arrs)
            {
                ClearObjABName(abName);
                AssetDatabase.RemoveAssetBundleName(abName, true);
                curr++;
                EditorUtility.DisplayProgressBar(string.Format("ClearABName - ({0}/{1})", curr, count), abName, (curr / count));
            }
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();

            if (isClearBuild)
            {
                DelABFolders();
                ClearBuild();
            }
        }

        static void _ReBindABName(string objAssetPath)
        {
            UObject obj = Load4Develop(objAssetPath);
            if (obj == null)
                return;
            _ReBindABName(obj);
            _UnloadOne(obj);
        }

        static string m_strNotRmvEmptyComp = "Editor/Cfgs/not_rmv_empty_comp_fab.txt";
        static CfgMustFiles _cfgNREC = null;
        static CfgMustFiles m_cfgNREC
        {
            get
            {
                if (_cfgNREC == null)
                {
                    string fp = string.Concat(m_dirData, m_strNotRmvEmptyComp);
                    _cfgNREC = CfgMustFiles.BuilderFp(fp);
                }
                return _cfgNREC;
            }
        }
        static public void ReCfgNREC(string fn, bool isForce = false)
        {
            bool isEmpty = string.IsNullOrEmpty(fn);
            bool isSame = m_strNotRmvEmptyComp.Equals(fn);
            bool isOkey = !(isEmpty || isSame);
            isForce = isForce || isOkey;
            if (!isForce)
                return;

            _cfgNREC = null;
            if (isOkey)
                m_strNotRmvEmptyComp = fn;
            _cfgNREC = m_cfgNREC;
        }

        static void _HandlerEmpty(UObject obj)
        {
            if (obj is GameObject)
            {
                GameObject gobj = obj as GameObject;

                bool isNoEmpty = m_cfgNREC.IsHas(gobj.name);
                if (isNoEmpty)
                    return;

                Animator[] arrsAnit = gobj.GetComponentsInChildren<Animator>(true);
                foreach (var item in arrsAnit)
                {
                    if (item != null && item.runtimeAnimatorController == null)
                    {
                        GameObject.DestroyImmediate(item, true);
                    }
                }

                Animation[] arrsAnim = gobj.GetComponentsInChildren<Animation>(true);
                foreach (var item in arrsAnim)
                {
                    if (item != null && item.GetClipCount() <= 0)
                    {
                        GameObject.DestroyImmediate(item, true);
                    }
                }

                // CMDAssets.CleanupMissingScripts(gobj); // 通过命令可以清除(有可能会清除掉有的)，打开unity怎么都不会清除

                // 加上这句，才会保存修改后的prefab
                if (IsPrefabInstance(gobj, false))
                {
                    PrefabUtility.SavePrefabAsset(gobj);
                }
            }
        }

        static void _ReBindABName(UObject obj)
        {
            _HandlerEmpty(obj);
            string _abSuffix = null;
            string _abName = GetAbName(obj, ref _abSuffix);
            bool _isError = _abName.EndsWith("error");
            if (_isError)
            {
                if (obj != null)
                {
                    var _ap = GetPath(obj);
                    Debug.LogErrorFormat("=== _ReBindABName = [{0}] , [{1}] , [{2}]", obj.GetType(), _abName, _ap);
                }

                _abName = null;
                _abSuffix = null;
                SetABInfo(obj);
            }
            else
            {
                SetABInfo(obj, _abName, _abSuffix);
            }

            var _abEn = MgrABDataDependence.GetData(obj);
            _abEn.ReAB(_abName, _abSuffix);
        }

        static protected void ReBindABNameByMgr()
        {
            UnloadUnusedAssets();
            List<ABDataDependence> _list = MgrABDataDependence.GetCurrList();
            string _strRes = null;
            float count = _list.Count;
            ABDataDependence item;
            for (int i = 0; i < count; i++)
            {
                item = _list[i];
                if (item.GetBeUsedCount() > 1)
                {
                    _strRes = item.GetCurrRes();
                    EditorUtility.DisplayProgressBar(string.Format("ReBindABName m_dic - ({0}/{1})", i, count), _strRes, (i / count));
                    _ReBindABName(_strRes);
                }
            }

            _list = MgrABDataDependence.ReAB4Shader();
            if (_list != null)
            {
                count = _list.Count;
                for (int i = 0; i < count; i++)
                {
                    item = _list[i];
                    _strRes = item.GetCurrRes();
                    SetABInfo(_strRes, item.m_abName, item.m_abSuffix);
                }
            }
            AssetDatabase.RemoveUnusedAssetBundleNames();
            UnloadUnusedAssets();
        }

        public static void BuildNow(bool isBuildAB = true, bool isTip = true)
        {
            EditorUtility.DisplayProgressBar("BuildNow", "Start BuildNow ...", 0.05f);

            ReBindABNameByMgr();

            if (isBuildAB)
                DoBuild(true, isTip);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            if (!isBuildAB)
                EditorUtility.DisplayDialog("提示", "资源重新绑定abname完成!!!", "确定");
        }

        // [MenuItem("Tools/Re - AB")]
        static public void DoBuild()
        {
            DoBuild(true);
        }

        static void BuildAssetBundles()
        {
            string _dirRes_ = CurrDirRes();
            EditorUtility.DisplayProgressBar("DoBuild", "BuildAssetBundles ...", 0.2f);
            CreateFolder(_dirRes_);
            BuildPipeline.BuildAssetBundles(_dirRes_, BuildAssetBundleOptions.ChunkBasedCompression, GetBuildTarget());
            EditorUtility.DisplayProgressBar("DoBuild", "ClearBuild ...", 0.3f);
            EditorUtility.ClearProgressBar();
            MgrABDataDependence.SaveDeps();
        }

        static public void DoBuild(bool isCheckABSpace, bool isTip = true)
        {
            string _vSpace = null;
            if (isCheckABSpace && _IsHasSpace(ref _vSpace))
            {
                EditorUtility.ClearProgressBar();
                // EditorUtility.DisplayDialog("提示", "[原始资源]名有空格，请查看输出打印!!!", "确定");
                // return;
                throw new System.Exception(_vSpace); // 空格 打包异常抛出
            }

            EditorUtility.DisplayProgressBar("DoBuild", "Start DoBuild ...", 0.0f);
            int _lensAb = _CheckABName();
            bool _isMakeAB = (_lensAb > 0);
            // BuildAssetBundleOptions.None : 使用LZMA算法压缩，压缩的包更小，但是加载时间更长，需要解压全部。
            // BuildAssetBundleOptions.ChunkBasedCompression : 使用LZ4压缩，压缩率没有LZMA高，但是我们可以加载指定资源而不用解压全部。
            if (_isMakeAB)
            {
                BuildAssetBundles();
                if (isTip)
                    EditorUtility.DisplayDialog("提示", "[ab资源] - 打包完成!!!", "确定");
            }
            else
            {
                EditorUtility.ClearProgressBar();
                if (isTip)
                    EditorUtility.DisplayDialog("提示", "没有[原始资源]设置了AssetBundleName , 即资源的abname都为None!!!", "确定");
            }
        }

        static public BuildTarget m_buildTarget = BuildTarget.NoTarget;
        static BuildTarget GetBuildTarget()
        {
            if (m_buildTarget == BuildTarget.NoTarget)
            {
                switch (EditorUserBuildSettings.activeBuildTarget)
                {
                    case BuildTarget.iOS:
                        return BuildTarget.iOS;
                    default:
                        return BuildTarget.Android;
                }
            }
            return m_buildTarget;
        }

        [MenuItem("Assets/Tools/导出 - 选中的Object")]
        static void BuildSelectPrefab()
        {
            UObject[] _arrs = Selection.GetFiltered(typeof(UObject), SelectionMode.DeepAssets);
            for (int i = 0; i < _arrs.Length; ++i)
            {
                AnalyseFile4Deps(_arrs[i]);
            }
            BuildNow(true);
        }

        [MenuItem("Assets/Tools/清除 - 选中的AssetBundleName")]
        static void ClearABName4Select()
        {
            UObject[] _arrs = Selection.GetFiltered(typeof(UObject), SelectionMode.DeepAssets);
            for (int i = 0; i < _arrs.Length; ++i)
            {
                SetABInfo(_arrs[i]);
            }
            UnloadUnusedAssets();
            EditorUtility.DisplayDialog("提示", "已清除选中的所有文件(files)及文件夹(folders)的abname!!!", "确定");
        }

        // [MenuItem("Tools/Delete ABFolders")]
        static void _DelABFolders()
        {
            DelABFolders(true);
        }

        static bool _IsContains(string[] src, string cur)
        {
            if (src == null || src.Length <= 0)
                return false;

            foreach (var item in src)
            {
                if (cur.Contains(item))
                    return true;
            }
            return false;
        }

        static public void DelABFolders(bool isTip = false)
        {
            string _dirRes_ = CurrDirRes();
            EditorUtility.DisplayProgressBar("DeleteABFolders", " rm folder where is ab_resources inside ...", 0.0f);
            EL_Path _ep = EL_Path.builder.DoInit(_dirRes_);

            // "audios/","fnts/","materials/","prefabs/","shaders/","textures/","ui/"
            string[] arrs = null;
            // arrs = new string[]{
            //     "configs/","protos/","lanuage/","maps/",
            // };


            int curr = 0;
            float count = _ep.m_folders.Count;
            string _fd = null;
            foreach (string _fn in _ep.m_folders)
            {
                curr++;
                _fd = ReFnPath(_fn);
                EditorUtility.DisplayProgressBar(string.Format("DeleteAB Folders rm - ({0}/{1})", curr, count), _fd, (curr / count));
                if (_fd.EndsWith(m_assetRelativePath) || _IsContains(arrs, _fd))
                    continue;
                DelFolder(_fd);
            }

            _ep = EL_Path.builder.DoInit(_dirRes_, true);
            curr = 0;
            count = _ep.m_files.Count;
            foreach (string _fn in _ep.m_files)
            {
                curr++;
                EditorUtility.DisplayProgressBar(string.Format("DeleteAB Files rm - ({0}/{1})", curr, count), _fn, (curr / count));
                if (_fn.EndsWith(m_assetRelativePath) || _IsContains(arrs, _fn))
                    continue;
                DelFile(_fn);
            }

            EditorUtility.ClearProgressBar();
            if (isTip)
                EditorUtility.DisplayDialog("提示", "已删除指定文件夹ABFolders!", "确定");
        }

        // [MenuItem("Tools/Delete Same Material")]
        static public void DeleteSameMaterial()
        {
            // 这个是遍历当前场景的对象(不是全部资源对象)有思路，未实现
            Dictionary<string, string> dicMaterial = new Dictionary<string, string>();
            MeshRenderer[] _arrs = UnityEngine.Resources.FindObjectsOfTypeAll<MeshRenderer>();
            string rootPath = Directory.GetCurrentDirectory();
            int _lens = _arrs.Length, _lens2 = 0;
            for (int i = 0; i < _lens; i++)
            {
                MeshRenderer meshRender = _arrs[i];
                _lens2 = meshRender.sharedMaterials.Length;
                Material[] newMaterials = new Material[_lens2];
                for (int j = 0; j < _lens2; j++)
                {
                    Material m = meshRender.sharedMaterials[j];
                    string mPath = GetPath(m);
                    if (!string.IsNullOrEmpty(mPath) && mPath.Contains("Assets/"))
                    {
                        string fullPath = Path.Combine(rootPath, mPath);
                        Debug.Log("fullPath = " + fullPath);
                        string text = File.ReadAllText(fullPath).Replace(" m_Name: " + m.name, "");
                        string change;
                        Debug.Log("text = " + text);
                        if (!dicMaterial.TryGetValue(text, out change))
                        {
                            dicMaterial[text] = mPath;
                            change = mPath;
                        }
                        newMaterials[j] = Load4Develop(change) as Material;
                    }
                }
                meshRender.sharedMaterials = newMaterials;
            }
            EditorSceneManager.MarkAllScenesDirty();
        }

        // [MenuItem("Tools/Check Has Space ABName")]
        static public bool IsHasSpace()
        {
            string _vSpace = null;
            return _IsHasSpace(ref _vSpace);
        }
        static protected bool _IsHasSpace(ref string vSpace)
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            string[] arrs = AssetDatabase.GetAllAssetBundleNames();
            int count = arrs.Length;
            string strName = null;
            bool _isRet = false;
            System.Text.StringBuilder _builder = new System.Text.StringBuilder();
            bool _isFirst = true;
            for (int i = 0; i < count; i++)
            {
                strName = arrs[i];
                if (strName.Contains(" "))
                {
                    _isRet = true;
                    if (_isFirst)
                    {
                        _builder.AppendLine("====== this has space,ab name = ");
                        _isFirst = false;
                    }
                    _builder.Append(strName).AppendLine(",");
                }
            }
            vSpace = _builder.ToString();
            _builder.Clear();
            _builder.Length = 0;
            if (!string.IsNullOrEmpty(vSpace))
                vSpace += "===============";
            return _isRet;
        }

        static public void ReLoadFolders(ref List<UObject> list, bool isTip = true)
        {
            string[] _dirs = GetFns4Folders(m_appAssetPath);
            if (_dirs == null)
            {
                if (isTip)
                    EditorUtility.DisplayDialog("提示", "没有可进行Load的文件夹!", "确定");
                return;
            }

            if (list == null)
                list = new List<UObject>();

            if (isTip)
                EditorUtility.DisplayProgressBar("ReLoad", "Reload folders for develops ...", 0.0f);
            UObject _one = null;
            int curr = 0;
            float count = _dirs.Length;
            foreach (var item in _dirs)
            {
                _one = Load4Develop(item);
                if (_one != null)
                {
                    list.Add(_one);
                }
                curr++;

                if (isTip)
                    EditorUtility.DisplayProgressBar(string.Format("ReLoad - ({0}/{1})", curr, count), item, (curr / count));
            }

            if (isTip)
                EditorUtility.ClearProgressBar();
        }

        [MenuItem("Assets/Tools/Check Files Is Has Space")]
        static void CheckHasSpace()
        {
            UObject[] _arrs = Selection.GetFiltered(typeof(UObject), SelectionMode.Assets | SelectionMode.DeepAssets);
            string _fp;
            float _lens_ = _arrs.Length;
            int _curr_ = 0;

            EditorUtility.DisplayProgressBar("CheckHasSpace", "Start ...", 0.0f);
            foreach (UObject _obj_ in _arrs)
            {
                _fp = GetPath(_obj_);
                _curr_++;
                EditorUtility.DisplayProgressBar(string.Format("CheckHasSpace - ({0}/{1})", _curr_, _lens_), _fp, (_curr_ / _lens_));
                if (_fp.Contains(" "))
                {
                    Debug.LogErrorFormat("====== has space,fp = [{0}]", _fp);
                }
            }
            EditorUtility.ClearProgressBar();
        }

        static public void BuildAllResource(bool isBuildAB = true, bool isTip = false)
        {// async
            //ClearAllABNames(true);
            //AssetDatabase.Refresh();
            //List<UObject> list = null;
            //ReLoadFolders(ref list, false);
            //if (list == null || list.Count <= 0)
            //{
            //    throw new System.Exception("没有资源");
            //}
            //System.Type typeFolder = typeof(UnityEditor.DefaultAsset);
            //System.Type typeOrg = null;
            //UObject one = null;
            //for (int i = 0; i < list.Count; i++)
            //{
            //    one = list[i];
            //    typeOrg = one.GetType();
            //    if (typeOrg == typeFolder)
            //    {
            //        AnalyseDir4Deps(one);
            //    }
            //}
            BuildNow(isBuildAB, isTip);
        }

        static public bool IsInParams(string cur, params string[] strs)
        {
            if (strs == null || strs.Length <= 0)
                return false;
            if (string.IsNullOrEmpty(cur))
                return false;
            for (int i = 0; i < strs.Length; i++)
            {
                if (cur.Equals(strs[i]))
                    return true;
            }
            return false;
        }

        static public void BindStateMachineBehaviour<T>(params string[] stateNames) where T : StateMachineBehaviour
        {
            string _fd = BuildPatcher.m_appAssetPath;
            string[] files = Directory.GetFiles(_fd, "*.controller", SearchOption.AllDirectories);
            AnimatorController animatorController = null;
            bool _isChg = false;
            foreach (string file in files)
            {
                animatorController = BuildPatcher.GetObject<AnimatorController>(file);
                if (animatorController == null)
                    continue;
                _isChg = false;
                AnimatorControllerLayer[] layers = animatorController.layers;
                foreach (var layer in layers)
                {
                    ChildAnimatorState[] states = layer.stateMachine.states;
                    foreach (var state in states)
                    {
                        if (IsInParams(state.state.name, stateNames))
                        {
                            foreach (var item in state.state.behaviours)
                            {
                                if (item is T)
                                    goto _FC;
                            }
                            _isChg = true;
                            state.state.AddStateMachineBehaviour<T>();
                            _FC: continue;
                        }
                    }
                }
                if (_isChg)
                    SaveAssets(animatorController, true);
            }
        }

        static public string Get4GUID(params string[] guids)
        {
            if (IsNullOrEmpty(guids))
                return "";
            var _sb = new System.Text.StringBuilder();
            string _guid = null;
            string _assetPath = null;
            UObject _uobj;
            for (int i = 0; i < guids.Length; i++)
            {
                _guid = guids[i];
                if (string.IsNullOrEmpty(_guid))
                    continue;
                _assetPath = AssetDatabase.GUIDToAssetPath(_guid);
                _uobj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_assetPath);
                _sb.AppendFormat("=== guid = [{0}], assetPath = [{1}] , type = [{2}]", _guid,_assetPath, _uobj.GetType()).AppendLine();
            }
            string _v = _sb.ToString();
            _sb.Length = 0;
            _sb.Clear();
            return _v;
        }

        static protected void LandscapePlatformSetting(BuildTarget buildTarget, string applicationIdentifier,ref string bundleVersion,ref string bundleVersionCode, bool isAddBVer = true)
        {
            if (!string.IsNullOrEmpty(applicationIdentifier))
                PlayerSettings.applicationIdentifier = applicationIdentifier;
            // PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, applicationIdentifier);
            string _pre_ver = PlayerSettings.bundleVersion;
            bool _is_ver = !string.IsNullOrEmpty(bundleVersion);
            if (_is_ver)
                PlayerSettings.bundleVersion = bundleVersion;

            int cur = -1, pre = 0;
            bool _is_verCode = !string.IsNullOrEmpty(bundleVersionCode);
            if (_is_verCode)
                int.TryParse(bundleVersionCode, out cur);

            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.stripUnusedMeshComponents = true; // optimize mesh data
            // PlayerSettings.MTRendering = true; // 多线程渲染 Multithreaded Rendering
            // PlayerSettings.gpuSkinning = true; // 将 Skinning活动 推送到 GPU Compute SKinning(Compute SKinning*选中)
            // PlayerSettings.graphicsJobs = true; // 把渲染线程的任务分配到工作线程(引起 Vulkan 渲染Crash)
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
                    //PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
                    //PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
                    PlayerSettings.legacyClampBlendShapeWeights = true;
                    PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard_2_0);
                    PlayerSettings.Android.androidTVCompatibility = true;
                    PlayerSettings.Android.androidIsGame = true;
                    pre = PlayerSettings.Android.bundleVersionCode;
                    if (!_is_verCode && cur <= pre)
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
                    if (!_is_verCode && cur <= pre)
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
                    // bundleVersion = bundleVersion + "." + cur;
                    PlayerSettings.bundleVersion = bundleVersion;
                }
                else
                {
                    bundleVersion = LeftLast(_pre_ver, ".", true) + cur;
                    PlayerSettings.bundleVersion = bundleVersion;
                }
            }
        }


        static public void ReBindAllResourceABNames()
        { 
            List<UObject> list = null;
            ReLoadFolders(ref list, false);
            if (list == null || list.Count <= 0)
            {
                throw new System.Exception("没有资源");
            }
            System.Type typeFolder = typeof(UnityEditor.DefaultAsset);
            System.Type typeOrg = null;
            UObject one = null;
            for (int i = 0; i < list.Count; i++)
            {
                one = list[i];
                typeOrg = one.GetType();
                if (typeOrg == typeFolder)
                {
                    AnalyseDir4Deps(one);
                }
            }

            ReBindABNameByMgr();
            _CheckABName(false);
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

    }
}