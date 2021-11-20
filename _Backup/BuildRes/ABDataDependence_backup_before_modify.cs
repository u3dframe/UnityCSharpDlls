using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using LitJson;

namespace Core.Kernel
{
    using UObject = UnityEngine.Object;

    /// <summary>
    /// 类名 : 资源原始数据
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2021-11-14 09:40
    /// 功能 : 将字符串，去掉重复
    /// </summary>
    public class OriginAsset
    {
        static public long CurrMaxId = 0;
        public long m_id = 0;
        public string m_res = "";
        public string m_guid = "";

        private UObject _uobj = null;
        private string _strYAML = null;

        public OriginAsset() { }

        bool _IsCanYAML(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;
            assetPath = assetPath.ToLower();
            if (assetPath.EndsWith(".prefab") || assetPath.EndsWith(".mat"))
                return true;
            return false;
        }

        public bool IsYAML()
        {
            if (string.IsNullOrEmpty(_strYAML))
                return this._IsCanYAML(this.m_res);
            return true;
        }

        public void InitParts()
        {
            this._uobj = AssetDatabase.LoadAssetAtPath<UObject>(this.m_res);
            if (this._IsCanYAML(this.m_res))
            {
                // _filePath
                string _fp = BuildPatcher.m_dirDataNoAssets + this.m_res;
                this._strYAML = BuildPatcher.GetText4File(_fp);
            }
        }

        public void Init(string objAssetPath)
        {
            if (string.IsNullOrEmpty(objAssetPath) || objAssetPath.Equals(this.m_res))
                return;
            if (!BuildPatcher.IsExistsInAssets(objAssetPath)) // 自身是否存在
                return;

            this.m_id = (++CurrMaxId);
            this.m_res = objAssetPath;
            this.m_guid = AssetDatabase.AssetPathToGUID(this.m_res);
            this.InitParts();
        }

        public void Init(UObject obj)
        {
            if (obj == null)
            {
                return;
            }
            string _assetPath = AssetDatabase.GetAssetPath(obj);
            this.Init(_assetPath);
        }
    }

    /// <summary>
    /// 类名 : AB数据关系
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2018-03-12 13:40
    /// 功能 : 
    /// 修订 : 2020-03-26 09:29
    /// </summary>
    public class ABDataDependence
    {
        public string m_res = "";
        public string m_guid = "";
        public bool m_isMustAB = false;
        public int m_nBeUsed = 0;
        public List<string> m_lDependences = new List<string>();
        // 被谁引用了?
        public List<string> m_lBeDeps = new List<string>();
        public string m_abName = "";
        public string m_abSuffix = "";
        public bool m_isShader { get; private set; }
        public bool m_isShaderSVC { get; private set; }

        public ABDataDependence() { }

        public ABDataDependence(string objAssetPath, bool isMust)
        {
            InitDeps(objAssetPath, isMust);
        }

        public ABDataDependence(UObject obj, bool isMust)
        {
            InitDeps(obj, isMust);
        }

        public void InitDeps(string objAssetPath, bool isMust)
        {
            // 相对于objAssetPath;
            UObject obj = AssetDatabase.LoadAssetAtPath<UObject>(objAssetPath);
            InitDeps(obj, isMust);
        }

        public void InitDeps(UObject obj, bool isMust)
        {
            if (obj == null)
            {
                return;
            }
            this.m_isMustAB = isMust;

            this.m_res = AssetDatabase.GetAssetPath(obj);
            this.m_guid = AssetDatabase.AssetPathToGUID(this.m_res);
            if (MgrABDataDependence.IsIgnoreFile(this.m_res))
            {
                this.m_isMustAB = false;
                this.m_nBeUsed = -999999999;
                return;
            }

            if (MgrABDataDependence.IsMustFile(this.m_res))
                this.m_isMustAB = true;

            System.Type _objType = obj.GetType();
            this.m_isShader = BuildPatcher.IsSameClass(_objType, BuildPatcher.tpShader);
            this.m_isShaderSVC = BuildPatcher.IsSameClass(_objType, BuildPatcher.tpSVC);

            string[] deps = AssetDatabase.GetDependencies(m_res, false);
            if (deps == null || deps.Length <= 0)
                return;

            foreach (var item in deps)
            {
                if (!m_lDependences.Contains(item))
                    m_lDependences.Add(item);
            }
        }

        public int GetBeUsedCount()
        {
            if (m_isMustAB)
                return 999999999;

            return m_nBeUsed;
        }

        public int GetNDeps()
        {
            if (m_lDependences != null)
                return m_lDependences.Count;
            return 0;
        }

        public void AddBeDeps(string beDeps)
        {
            if (!m_lBeDeps.Contains(beDeps))
            {
                m_lBeDeps.Add(beDeps);
                ++this.m_nBeUsed;
            }
        }

        public void RmvBeDeps(string beDeps)
        {
            if (m_lBeDeps.Contains(beDeps))
            {
                m_lBeDeps.Remove(beDeps);
                --this.m_nBeUsed;
            }
        }

        public void CheckCurrDeps()
        {
            string _bfile = null;
            ABDataDependence _data = null;
            int _lens = m_lDependences.Count;
            for (int i = _lens - 1; i >= 0; --i)
            {
                _bfile = m_lDependences[i];
                _data = MgrABDataDependence.GetData(_bfile);
                if (_data != null)
                {
                    _data.RmvBeDeps(this.m_res);
                }
            }

            this.m_lDependences.Clear();
            string[] deps = AssetDatabase.GetDependencies(this.m_res, false);
            _lens = (deps != null) ? deps.Length : 0;
            for (int i = _lens - 1; i >= 0; --i)
            {
                _bfile = deps[i];
                if (!this.m_lDependences.Contains(_bfile))
                    this.m_lDependences.Add(_bfile);

                _data = MgrABDataDependence.GetData(_bfile);
                if (_data == null)
                    MgrABDataDependence.Init(_bfile, false, this.m_res);
                else
                    _data.AddBeDeps(this.m_res);
            }
        }
        public void CheckCurrBeDeps() {
            string _bfile = null;
            ABDataDependence _data = null;
            int _lens = this.m_lBeDeps.Count;
            for (int i = _lens - 1; i >= 0; --i)
            {
                _bfile = this.m_lBeDeps[i];
                _data = MgrABDataDependence.GetData(_bfile);
                if (_data == null || !_data.m_lDependences.Contains(this.m_res))
                {
                    this.RmvBeDeps(_bfile);
                }
            }
        }

        public List<ABDataDependence> ReABBySVC4Shader()
        {
            if (!this.m_isShaderSVC)
                return null;
            int _lens = this.GetNDeps();
            if (_lens <= 0)
                return null;
            List<ABDataDependence> ret = new List<ABDataDependence>();
            ABDataDependence _it_ = null;
            for (int i = 0; i < _lens; i++)
            {
                _it_ = MgrABDataDependence.GetData(this.m_lDependences[i]);
                if (_it_ == null || _it_.m_res.Contains("/PostProcessing/"))
                    continue;
                _it_.ReAB(this.m_abName, this.m_abSuffix);
                ret.Add(_it_);
            }
            return ret;
        }

        public void ReAB(string abName = "", string abSuffix = "")
        {
            this.m_abName = abName == null ? "" : abName;
            this.m_abSuffix = abSuffix == null ? "" : abSuffix;
        }

        override public string ToString()
        {
            StringBuilder _builder = new StringBuilder();
            _builder.AppendFormat("m_res = [{0}]", m_res);
            _builder.AppendLine();
            _builder.AppendFormat("m_isMustAB = [{0}]", m_isMustAB);
            _builder.AppendLine();
            _builder.AppendFormat("m_nUseCount = [{0}]", m_nBeUsed);
            _builder.AppendLine();
            int _lens = this.GetNDeps();
            _builder.AppendFormat("m_lDependences = [{0}]", _lens);
            _builder.AppendLine();
            for (int i = 0; i < _lens; i++)
            {
                _builder.AppendLine("  " + m_lDependences[i]);
            }
            _builder.AppendLine();

            _builder.AppendFormat("m_lBeDeps = [{0}]", m_lBeDeps.Count);
            _builder.AppendLine();
            _lens = m_lBeDeps.Count;
            for (int i = 0; i < _lens; i++)
            {
                _builder.AppendLine("  " + m_lBeDeps[i]);
            }
            _builder.AppendLine();

            return _builder.ToString();
        }

        public string ToJson()
        {
            return JsonMapper.ToJson(this);
        }
    }

    class SortABDep : Comparer<ABDataDependence>
    {
        public override int Compare(ABDataDependence a, ABDataDependence b)
        {
            if (a.m_isShaderSVC && !b.m_isShaderSVC)
                return -1;
            if (!a.m_isShaderSVC && b.m_isShaderSVC)
                return 1;
            if (a.GetBeUsedCount() > b.GetBeUsedCount())
                return -1;
            if (a.GetBeUsedCount() < b.GetBeUsedCount())
                return 1;
            if (a.GetNDeps() > b.GetNDeps())
                return -1;
            if (a.GetNDeps() < b.GetNDeps())
                return 1;
            return 0;
        }
    }

    public class MgrABDataDependence
    {
        static private MgrABDataDependence _instance = null;
        static public MgrABDataDependence instance
        {
            get
            {
                if (_instance == null){
                    _instance = new MgrABDataDependence();
                    _instance.InitIgnoreAndMust();
                    ClearDeps();
                    ReLoadDeps();
                }
                return _instance;
            }
        }

        private SortABDep m_sort = new SortABDep();
        private ListDict<ABDataDependence> m_dicList = new ListDict<ABDataDependence>(true);
        private CfgMustFiles m_ignoreFiles;
        private CfgMustFiles m_mustFiles;

        public int Count
        {
            get { return this.m_dicList.Count(); }
        }

        public List<ABDataDependence> GetList(bool isSort = false)
        {
            if (isSort)
                this.m_dicList.m_list.Sort(m_sort);
            return this.m_dicList.m_list;
        }

        public List<ABDataDependence> ReABBySVC4Shader()
        {
            this.GetList(true);
            int lens = this.m_dicList.m_list.Count;
            ABDataDependence _f = lens > 0 ? this.m_dicList.m_list[0] : null;
            return (_f != null) ? _f.ReABBySVC4Shader() : null;
        }

        public void InitIgnoreAndMust(bool isForce = false)
        {
            isForce = isForce || this.m_ignoreFiles == null;
            if (!isForce)
                return;

            string fp = string.Format("{0}/Editor/Cfgs/ab_ignore_files.txt", Application.dataPath);
            this.m_ignoreFiles = CfgMustFiles.BuilderFp(fp);
            this.m_ignoreFiles.Appends(ignoreFiles);
            
            fp = string.Format("{0}/Editor/Cfgs/ab_must_files.txt", Application.dataPath);
            this.m_mustFiles = CfgMustFiles.BuilderFp(fp);
            this.m_mustFiles.Appends(mustFiles);
        }

        static string[] ignoreFiles = {
            ".manifest",
            ".meta",
            ".cs",
            ".fnt",
            ".txt",
            ".dll",
            "README",
            "LightingData.asset", // == UnityEditor.LightingDataAsset
            //"ReflectionProbe-0.exr",
        };

        static string[] mustFiles = {
            ".prefab",
            ".ttf",
            ".fontsettings",
            ".shadervariants",
            //".playable",
            // ".fbx",
        };

        static public bool IsIgnoreFile(string fp)
        {
            return instance.m_ignoreFiles.IsHas(fp);
        }

        static public bool IsMustFile(string fp)
        {
            return instance.m_mustFiles.IsHas(fp);
        }

        static public void Init(string objAssetPath, bool isMust, string beDeps = "")
        {
            // 相对于objAssetPath;
            UObject obj = null;
            try
            {
                obj = AssetDatabase.LoadAssetAtPath<UObject>(objAssetPath);
            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("== path = [{0}],error = {1}", objAssetPath, ex);
            }
            Init(obj, isMust, beDeps);
        }

        static public void Init(UObject obj, bool isMust, string beDeps = "")
        {
            if (obj == null)
            {
                return;
            }
            string resPath = AssetDatabase.GetAssetPath(obj);
            if (IsIgnoreFile(resPath))
                return;

            ABDataDependence _data = GetData(resPath);
            bool _isHas = _data != null;
            if (_isHas) {
                _data.InitDeps(obj,isMust);
            } else {
                _data = new ABDataDependence(obj, isMust);
                instance.m_dicList.Add(resPath, _data);
            }
            
            if (!string.IsNullOrEmpty(beDeps)) {
                _data.AddBeDeps(beDeps);
            }

            if(!_isHas) {
                ABDataDependence _idate = null;
                foreach (var item in _data.m_lDependences) {
                    _idate = GetData(item);
                    if(_idate != null)
                    {
                        _idate.AddBeDeps(resPath);
                        continue;
                    }
                    Init(item, false, resPath);
                }
            }
        }

        static public int GetCount()
        {
            return instance.Count;
        }

        static public List<ABDataDependence> GetCurrList(bool isSort = false)
        {
            return instance.GetList(isSort);
        }

        static public List<ABDataDependence> ReAB4Shader()
        {
            return instance.ReABBySVC4Shader();
        }

        static public ABDataDependence GetData(string key)
        {
            return instance.m_dicList.Get(key);
        }

        static public ABDataDependence GetData(UObject obj)
        {
            if (obj == null)
                return null;

            string resPath = AssetDatabase.GetAssetPath(obj);
            return GetData(resPath);
        }

        static public int GetCount(string objAssetPath)
        {
            UObject obj = AssetDatabase.LoadAssetAtPath<UObject>(objAssetPath);
            return GetCount(obj);
        }

        static public int GetCount(UObject obj)
        {
            if (obj == null)
                return -1;

            ABDataDependence _data = GetData(obj);
            if (_data == null)
                return 0;

            return _data.GetBeUsedCount();
        }

        // [MenuItem("Tools/Deps/ClearDeps")]
        static public void ClearDeps()
        {
            instance.m_dicList.Clear();
        }

        // [MenuItem("Tools/Deps/SaveDeps", false, 31)]
        static public void SaveDeps()
        {
            string _fp = string.Format("{0}_deps.json", BuildPatcher.m_dirDataNoAssets);
            var _obj = instance.m_dicList.m_dic;
            string _v = JsonMapper.ToJson(_obj);
            BuildPatcher.WriteText(_fp,_v,true);
            AssetDatabase.Refresh();
        }

        // [MenuItem("Tools/Deps/ReLoadDeps", false, 33)]
        //static void _ReLoadDeps()
        //{
        //    ReLoadDeps(true);
        //}

        static bool _isLoaded = false;
        static public void ReLoadDeps(bool isIsRelead = false)
        {
            if (_isLoaded && !isIsRelead)
                return;
            _isLoaded = true;
            ClearDeps();
            // Core/Kernel/Editor/Build_Patcher/
            string _fp = string.Format("{0}_deps.json", BuildPatcher.m_dirDataNoAssets);
            string _v = BuildPatcher.GetText4File(_fp);
            
            if (!string.IsNullOrEmpty(_v))
            {
                JsonData _jd = JsonMapper.ToObject<JsonData>(_v);
                string _val = null;
                ABDataDependence _obj = null;
                foreach(string key in _jd.Keys)
                {
                    _val = _jd[key].ToJson();
                    if(!BuildPatcher.IsExistsInAssets(key)) // 自身是否存在
                        continue;
                    
                    _obj = JsonMapper.ToObject<ABDataDependence>(_val);
                    instance.m_dicList.Add(key,_obj); 
                }

                var list = instance.m_dicList.m_list;
                int lens = list.Count;
                for (int i = lens - 1; i >= 0; --i)
                {
                    _obj = list[i];
                    _obj.CheckCurrDeps(); // 检测资源 依赖
                }

                for (int i = lens - 1; i >= 0; --i)
                {
                    _obj = list[i];
                    _obj.CheckCurrBeDeps(); // 检测资源 被依赖
                }
            }
        }

        // [MenuItem("Tools/Deps/CurPath")]
        static public string GetCurPath()
        {
            return System.Environment.CurrentDirectory;
        }

        // [MenuItem("Tools/Deps/PrintDic")]
        static public void PrintDic()
        {
            var _obj = instance.GetList(true);
            foreach (var item in _obj)
            {
                Debug.Log(item);
            }
        }

        // [MenuItem("Tools/Deps/WriteDepsTxt")]
        static void WriteDepsTxt()
        {
            WriteDepsToTxt();
        }

        static public void WriteDepsToTxt(int limitCount = 2)
        {
            StringBuilder builder = new StringBuilder();
            var _obj = instance.GetList(true);
            foreach (var item in _obj)
            {
                Debug.Log(item.GetBeUsedCount());
                if (item.GetBeUsedCount() >= limitCount)
                {
                    builder.AppendLine(item.ToJson());
                }
            }
            string _v = builder.ToString();
            string _fp = string.Format("{0}_deps.json", BuildPatcher.m_dirDataNoAssets);
            BuildPatcher.WriteText(_fp, _v,true);
        }
    }
}