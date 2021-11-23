using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using LitJson;

namespace Core.Kernel
{
    using UObject = UnityEngine.Object;

    public static class ABDepsHelper
    {
        static public long CurrMaxId = 0;
        static public bool IsYAML = true;
        static public string fnOAsset = "_foassets.json";
        static public string fnDeps = "_fdeps.json";
    }

    /// <summary>
    /// 类名 : 资源原始数据 OriginAsset
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2021-11-14 09:40
    /// 功能 : 将字符串，去掉重复
    /// </summary>
    public class OrgAsset
    {
        public long m_id = 0;
        private string m_res = "";
        public string m_guid = "";

        // private UObject _uobj = null;
        private string _strYAML = null;

        public OrgAsset() { }
        public OrgAsset(string assetPath) {
            this.Init(assetPath);
        }
        public OrgAsset(UObject uobj)
        {
            this.Init(uobj);
        }

        bool _IsCanYAML(string assetPath)
        {
            if (!ABDepsHelper.IsYAML || string.IsNullOrEmpty(assetPath))
                return false;
            assetPath = assetPath.ToLower();
            return assetPath.EndsWith(".prefab") || assetPath.EndsWith(".mat");
        }

        public bool IsYAML()
        {
            if (string.IsNullOrEmpty(this._strYAML))
                return this._IsCanYAML(this.m_res);
            return true;
        }

        public void InitParts()
        {
            if(string.IsNullOrEmpty(this.m_res))
            {
                this.m_res = AssetDatabase.GUIDToAssetPath(this.m_guid);
            }
            // this._uobj = AssetDatabase.LoadAssetAtPath<UObject>(this.m_res);
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

            this.m_id = (++ABDepsHelper.CurrMaxId);
            this.m_res = objAssetPath;
            this.m_guid = AssetDatabase.AssetPathToGUID(objAssetPath);
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

        public bool IsGUIDInYAML(string guid)
        {
            if (string.IsNullOrEmpty(this._strYAML) || string.IsNullOrEmpty(guid))
                return false;
            return this._strYAML.Contains(guid);
        }

        public string GetRes()
        {
            return this.m_res;
        }

        public string ToJson()
        {
            return JsonMapper.ToJson(this);
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
        public long m_oaid = 0;
        public bool m_isMustAB = false;
        public int m_nBeUsed = 0;
        public List<long> m_lDependences = new List<long>();
        // 被谁引用了?
        public List<long> m_lBeDeps = new List<long>();
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

        public void InitDeps(string assetPath, bool isMust)
        {
            this.m_isMustAB = isMust;

            OrgAsset _oaObj = MgrABDataDependence.GetOAsset(assetPath);
            this.m_oaid = _oaObj.m_id;

            if (MgrABDataDependence.IsIgnoreFile(assetPath))
            {
                this.m_isMustAB = false;
                this.m_nBeUsed = -999999999;
                return;
            }

            if (MgrABDataDependence.IsMustFile(assetPath))
                this.m_isMustAB = true;

            string _toLower = assetPath.ToLower();
            this.m_isShader = BuildPatcher.IsAPEndShader(_toLower);
            this.m_isShaderSVC = BuildPatcher.IsAPEndSVC(_toLower);

            this._RecordDeps(assetPath);
        }

        public void InitDeps(UObject obj, bool isMust)
        {
            if (obj == null)
            {
                return;
            }
            string assetPath = AssetDatabase.GetAssetPath(obj);
            this.InitDeps(assetPath, isMust);
        }

        void _RecordDeps(string curAssetPath)
        {
            this.m_lDependences.Clear();
            string[] deps = AssetDatabase.GetDependencies(curAssetPath, false);
            if (deps == null || deps.Length <= 0)
                return;

            OrgAsset _oaObj = null;
            foreach (var item in deps)
            {
                _oaObj = MgrABDataDependence.GetOrNewAsset(item);
                if (!m_lDependences.Contains(_oaObj.m_id))
                    m_lDependences.Add(_oaObj.m_id);
            }
        }

        public void RecordBeDeps()
        {
            var _res = this.GetCurrRes();
            ABDataDependence _beData = null;
            OrgAsset _oaObj = null;
            foreach (var item in this.m_lDependences)
            {
                _beData = MgrABDataDependence.GetData(item);
                if (_beData != null)
                    _beData.AddBeDeps(this.m_oaid);
                else
                {
                    _oaObj = MgrABDataDependence.GetOAsset(item);
                    MgrABDataDependence.Init(_oaObj.GetRes(), false,_res);
                }
            }
        }

        public OrgAsset GetOAsset()
        {
            return MgrABDataDependence.GetOAsset(this.m_oaid);
        }

        public string GetCurrRes()
        {
            OrgAsset _oaObj = this.GetOAsset();
            return _oaObj?.GetRes();
        }

        public string GetCurrGUID()
        {
            OrgAsset _oaObj = this.GetOAsset();
            return _oaObj?.m_guid;
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

        public void AddBeDeps(long beDeps)
        {
            var _oaObj = MgrABDataDependence.GetOAsset(beDeps);
            if (_oaObj == null)
                return;
            bool _isYM = _oaObj.IsYAML();
            bool _isAdd = !_isYM;
            if (_isYM)
            {
                var _cur = this.GetOAsset();
                _isAdd = _oaObj.IsGUIDInYAML(_cur.m_guid);
            }

            if (_isAdd && !m_lBeDeps.Contains(beDeps))
            {
                m_lBeDeps.Add(beDeps);
                ++this.m_nBeUsed;
            }
        }

        public void AddBeDeps(string beDeps)
        {
            var oaItem = MgrABDataDependence.GetOAsset(beDeps);
            if (oaItem != null)
            {
                this.AddBeDeps(oaItem.m_id);
            }
        }

        public void RmvBeDeps(long beDeps)
        {
            if (m_lBeDeps.Contains(beDeps))
            {
                m_lBeDeps.Remove(beDeps);
                --this.m_nBeUsed;
            }
        }

        public void RmvBeDeps(string beDeps)
        {
            var oaItem = MgrABDataDependence.GetOAsset(beDeps);
            if (oaItem != null)
            {
                this.RmvBeDeps(oaItem.m_id);
            }
        }

        public void CheckCurrDeps()
        {
            long _oaID = -1;
            ABDataDependence _data = null;
            int _lens = m_lDependences.Count;
            for (int i = _lens - 1; i >= 0; --i)
            {
                _oaID = m_lDependences[i];
                _data = MgrABDataDependence.GetData(_oaID);
                if (_data != null)
                    _data.RmvBeDeps(this.m_oaid);
            }

            var _strRes = this.GetCurrRes();
            this._RecordDeps(_strRes);
            this.RecordBeDeps();
        }

        public void CheckCurrBeDeps() {
            long _oaID = -1;
            OrgAsset _oaObj = null;
            ABDataDependence _data = null;
            int _lens = this.m_lBeDeps.Count;
            for (int i = _lens - 1; i >= 0; --i)
            {
                _oaID = this.m_lBeDeps[i];
                _oaObj = MgrABDataDependence.GetOAsset(_oaID);
                _data = MgrABDataDependence.GetData(_oaID);
                if (_oaObj == null || _data == null || !_data.m_lDependences.Contains(this.m_oaid))
                {
                    this.RmvBeDeps(_oaID);
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
            OrgAsset _oaObj = null;
            long _oaID = -1;
            for (int i = 0; i < _lens; i++)
            {
                _oaID = this.m_lDependences[i];
                _oaObj = MgrABDataDependence.GetOAsset(_oaID);
                _it_ = MgrABDataDependence.GetData(_oaID);
                if (_it_ == null || _oaObj == null || _oaObj.GetRes().Contains("/PostProcessing/"))
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
            var _strRes = this.GetCurrRes();
            _builder.AppendFormat("m_oaid = [{0}]", this.m_oaid);
            _builder.AppendLine();
            _builder.AppendFormat("m_res = [{0}]", _strRes);
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

    /// <summary>
    /// 类名 : 管理AB数据关系
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2018-03-12 14:28
    /// 功能 : 
    /// 修订 : 2020-03-26 10:35
    /// </summary>
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
        private ListDict<OrgAsset> m_dicOAsset = new ListDict<OrgAsset>(false);
        private ListDict<string> m_dicId2Key = new ListDict<string>(false);
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

        static public void Init(UObject obj, bool isMust, string beDeps = "")
        {
            if (obj == null)
                return;
            string resPath = AssetDatabase.GetAssetPath(obj);
            Init(resPath, isMust, beDeps);
        }

        static public void Init(string objAssetPath, bool isMust, string beDeps = "")
        {
            if (string.IsNullOrEmpty(objAssetPath) || !BuildPatcher.IsExistsInAssets(objAssetPath)) // 自身是否存在
                return;

            if (IsIgnoreFile(objAssetPath))
                return;

            OrgAsset _oaObj = GetOrNewAsset(objAssetPath);
            ABDataDependence _data = GetData(_oaObj.m_id);
            bool _isHas = _data != null;
            if (_isHas) {
                _data.InitDeps(objAssetPath, isMust);
            } else {
                _data = new ABDataDependence(objAssetPath, isMust);
                instance.m_dicList.Add(_oaObj.m_id, _data);
            }
            
            if (!string.IsNullOrEmpty(beDeps)) {
                _data.AddBeDeps(beDeps);
            }

            if(!_isHas) {
                _data.RecordBeDeps();
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

        static public OrgAsset GetOAsset(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            return instance.m_dicOAsset.Get(key);
        }

        static public OrgAsset GetOrNewAsset(string assetPath)
        {
            OrgAsset _oaObj = GetOAsset(assetPath);
            if (_oaObj == null)
            {
                _oaObj = new OrgAsset(assetPath);
                instance.m_dicOAsset.Add(assetPath, _oaObj);
                instance.m_dicId2Key.Add(_oaObj.m_id, assetPath);
            }
            return _oaObj;
        }

        static public OrgAsset GetOAsset(long id)
        {
            string key = instance.m_dicId2Key.Get(id);
            return GetOAsset(key);
        }

        static public ABDataDependence GetData(long key)
        {
            return instance.m_dicList.Get(key);
        }

        static public ABDataDependence GetData(string key)
        {
            OrgAsset _oaObj = GetOAsset(key);
            if(_oaObj != null)
                return instance.m_dicList.Get(_oaObj.m_id);
            return null;
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
            ABDataDependence _data = GetData(objAssetPath);
            if (_data == null)
                return -1;
            return _data.GetBeUsedCount();
        }

        static public int GetCount(UObject obj)
        {
            if (obj == null)
                return -1;

            string resPath = AssetDatabase.GetAssetPath(obj);
            return GetCount(resPath);
        }

        // [MenuItem("Tools/Deps/ClearDeps")]
        static public void ClearDeps()
        {
            instance.m_dicOAsset.Clear();
            instance.m_dicId2Key.Clear();
            instance.m_dicList.Clear();
            ABDepsHelper.CurrMaxId = 0;
        }

        // [MenuItem("Tools/Deps/SaveDeps", false, 31)]
        static public void SaveDeps()
        {
            string _fp1 = string.Format("{0}{1}", BuildPatcher.m_dirDataNoAssets, ABDepsHelper.fnOAsset);
            var _obj1 = instance.m_dicOAsset.m_dic;
            string _v1 = JsonMapper.ToJson(_obj1);
            BuildPatcher.WriteText(_fp1,_v1, true);

            string _fp = string.Format("{0}{1}", BuildPatcher.m_dirDataNoAssets, ABDepsHelper.fnDeps);
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

            string _fp1 = string.Format("{0}{1}", BuildPatcher.m_dirDataNoAssets, ABDepsHelper.fnOAsset);
            string _v1 = BuildPatcher.GetText4File(_fp1);
            if (!string.IsNullOrEmpty(_v1))
            {
                JsonData _jd = JsonMapper.ToObject<JsonData>(_v1);
                string _val = null;
                OrgAsset _oaObj = null;
                foreach (string key in _jd.Keys)
                {
                    _val = _jd[key].ToJson();
                    if (!BuildPatcher.IsExistsInAssets(key)) // 自身是否存在
                        continue;

                    _oaObj = JsonMapper.ToObject<OrgAsset>(_val);
                    _oaObj.InitParts();
                    if (_oaObj.m_id > ABDepsHelper.CurrMaxId)
                        ABDepsHelper.CurrMaxId = _oaObj.m_id;
                    instance.m_dicOAsset.Add(key, _oaObj);
                    instance.m_dicId2Key.Add(_oaObj.m_id, key);
                }
            }

            // Core/Kernel/Editor/Build_Patcher/
            string _fp = string.Format("{0}{1}", BuildPatcher.m_dirDataNoAssets, ABDepsHelper.fnDeps);
            string _v = BuildPatcher.GetText4File(_fp);
            
            if (!string.IsNullOrEmpty(_v))
            {
                JsonData _jd = JsonMapper.ToObject<JsonData>(_v);
                string _val = null;
                OrgAsset _oaObj = null;
                ABDataDependence _obj = null;
                long _oaID = -1;
                foreach(string key in _jd.Keys)
                {
                    _val = _jd[key].ToJson();
                    // if(!BuildPatcher.IsExistsInAssets(key)) // 自身是否存在
                    //    continue;
                    if (!long.TryParse(key, out _oaID))
                        continue;
                    _oaObj = GetOAsset(_oaID);
                    if (_oaObj == null)
                        continue;
                    _obj = JsonMapper.ToObject<ABDataDependence>(_val);
                    instance.m_dicList.Add(_oaID, _obj); 
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
            string _fp = string.Format("{0}_{1}{2}", BuildPatcher.m_dirDataNoAssets, limitCount, ABDepsHelper.fnDeps);
            BuildPatcher.WriteText(_fp, _v,true);
        }
    }
}