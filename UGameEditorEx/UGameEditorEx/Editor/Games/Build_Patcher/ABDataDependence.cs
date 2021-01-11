using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;
using LitJson;

namespace Core.Kernel
{

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

        public ABDataDependence(Object obj, bool isMust)
        {
            InitDeps(obj, isMust);
        }

        public void InitDeps(string objAssetPath, bool isMust)
        {
            // 相对于objAssetPath;
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(objAssetPath);
            InitDeps(obj, isMust);
        }

        public void InitDeps(Object obj, bool isMust)
        {
            if (obj == null)
            {
                return;
            }
            this.m_isMustAB = isMust;

            this.m_res = AssetDatabase.GetAssetPath(obj);
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
                return 99999999;

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
                m_lBeDeps.Add(beDeps);
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
                    ReLoadDeps();
                }
                return _instance;
            }
        }

        private SortABDep m_sort = new SortABDep();
        private ListDict<ABDataDependence> m_dicList = new ListDict<ABDataDependence>(true);

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

        void InitIgnoreAndMust()
        {
            string fp = string.Format("{0}/Editor/Cfgs/ab_ignore_files.txt", Application.dataPath);
            string val = null;
            if (File.Exists(fp))
                val = File.ReadAllText(fp);
            List<string> _list_ = new List<string>(ignoreFiles);
            if (!string.IsNullOrEmpty(val))
            {
                string[] _arrs = val.Split("\r\n\t".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                if(_arrs != null && _arrs.Length > 0)
                {
                    _list_.AddRange(_arrs);
                    ignoreFiles = _list_.ToArray();
                }
            }

            _list_.Clear();
            fp = string.Format("{0}/Editor/Cfgs/ab_must_files.txt", Application.dataPath);
            val = null;
            if (File.Exists(fp))
                val = File.ReadAllText(fp);
            if (!string.IsNullOrEmpty(val))
            {
                _list_.AddRange(mustFiles);
                string[] _arrs = val.Split("\r\n\t".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                if (_arrs != null && _arrs.Length > 0)
                {
                    _list_.AddRange(_arrs);
                    mustFiles = _list_.ToArray();
                }
            }
        }

        static string[] ignoreFiles = {
            ".manifest",
            ".meta",
            ".cs",
            ".fnt",
            ".txt",
            ".dll",
            "README",
            "LightingData.asset",
            "ReflectionProbe-0.exr",
        };

        static string[] mustFiles = {
            ".prefab",
            ".ttf",
            ".fontsettings",
            ".shadervariants",
            //".playable",
            // ".fbx",
        };

        static private bool _IsIn(string fp, string[] arrs)
        {
            if (arrs == null || arrs.Length <= 0)
                return false;
            string fpTolower = fp.ToLower();

            for (int i = 0; i < arrs.Length; i++)
            {
                if (fpTolower.Contains(arrs[i]))
                {
                    return true;
                }
            }
            return false;
        }

        static public bool IsIgnoreFile(string fp)
        {
            return _IsIn(fp, ignoreFiles);
        }

        static public bool IsMustFile(string fp)
        {
            return _IsIn(fp, mustFiles);
        }

        static public void Init(string objAssetPath, bool isMust, string beDeps = "")
        {
            // 相对于objAssetPath;
            Object obj = null;
            try
            {
                obj = AssetDatabase.LoadAssetAtPath<Object>(objAssetPath);
            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("== path = [{0}],error = {1}", objAssetPath, ex);
            }
            Init(obj, isMust, beDeps);
        }

        static public void Init(Object obj, bool isMust, string beDeps = "")
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
                _data.m_nBeUsed++;
            }

            if(!_isHas) {
                foreach (var item in _data.m_lDependences) {
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

        static public ABDataDependence GetData(Object obj)
        {
            if (obj == null)
                return null;

            string resPath = AssetDatabase.GetAssetPath(obj);
            return GetData(resPath);
        }

        static public int GetCount(string objAssetPath)
        {
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(objAssetPath);
            return GetCount(obj);
        }

        static public int GetCount(Object obj)
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
        static public void ReLoadDeps()
        {
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
                    if(!BuildPatcher.IsExistsInAssets(key))
                        continue;
                    
                    _obj = JsonMapper.ToObject<ABDataDependence>(_val);
                    instance.m_dicList.Add(key,_obj);
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