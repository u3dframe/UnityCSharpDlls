using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UObject = UnityEngine.Object;

namespace Core.Kernel
{

    /// <summary>
    /// 类名 : 原始资源处理
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2017-12-13 11:40
    /// 功能 : 
    /// </summary>
    public class EDW_HandlerRes : EditorWindow
    {
        static bool isOpenWindowView = false;

        static protected EDW_HandlerRes vwWindow = null;

        // 窗体宽高
        static public float width = 400;
        static public float height = 160;

        [MenuItem("Tools/HandlerRes", false, 10)]
        static void AddWindow()
        {
            if (isOpenWindowView || vwWindow != null)
                return;

            try
            {
                isOpenWindowView = true;

                vwWindow = GetWindow<EDW_HandlerRes>("HandlerRes");
                float _w = Screen.width;
                float _h = Screen.height;
                float _x = (_w - width) * 0.45f;
                float _y = (_h - height) * 0.45f;
                Vector2 pos = new Vector2(_x, _y);
                Vector2 size = new Vector2(width, height);
                Rect rect = new Rect(pos, size);
                vwWindow.position = rect;
                vwWindow.minSize = size;
            }
            catch
            {
                ClearWindow();
                throw;
            }
        }

        static void ClearWindow()
        {
            isOpenWindowView = false;
            vwWindow = null;
        }

        UObject mObj = null;
        string _path = "";
        string _orgSuffix = ".prefab";
        UObject mObj2 = null;
        string _path2 = "";
        bool _isStatisticsAll = false;

        #region  == EditorWindow Func ===

        void OnGUI()
        {
            EditorGUILayout.Space(10);
            mObj = EditorGUILayout.ObjectField("目标源:", mObj, typeof(UObject), false);
            if (mObj != null)
                _path = AssetDatabase.GetAssetPath(mObj);
            else
                _path = "";
            EditorGUILayout.Space(10);
            _orgSuffix = EditorGUILayout.TextField("筛查后缀名:", _orgSuffix);
            mObj2 = EditorGUILayout.ObjectField("筛查源:", mObj2, typeof(UObject), false);
            if (mObj2 != null)
                _path2 = AssetDatabase.GetAssetPath(mObj2);
            else
                _path2 = "";
            EditorGUILayout.Space(10);
            _isStatisticsAll = EditorGUILayout.ToggleLeft("是否 - 筛查所有目标源", _isStatisticsAll);
            EditorGUILayout.Space(10);
            if (GUILayout.Button("统计资源"))
            {
                Statistics(_path, _orgSuffix,_path2, _isStatisticsAll);
            }
        }

        // 在给定检视面板每秒10帧更新
        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnDestroy()
        {
            ClearWindow();
        }

        #endregion

        static string GetSuffixToLower(string path)
        {
            string _suffix = System.IO.Path.GetExtension(path);
            return _suffix.ToLower();
        }


        static void Statistics(string path, string suffixToLowers = ".prefab", string fpDir = "", bool isAll = false)
        {
            if (string.IsNullOrEmpty(path))
                path = Application.dataPath;
            EL_Path.Init(path);

            if (string.IsNullOrEmpty(fpDir))
                fpDir = Application.dataPath;
            var _arrs3 = Directory.GetFiles(fpDir, "*.*", SearchOption.AllDirectories)
               //.Where(s => s.ToLower().EndsWith(".prefab")).ToArray();
               .Where(s => suffixToLowers.Contains(GetSuffixToLower(s))).ToArray();

            if (isAll)
                StaAll(_arrs3);
            else
                StaSame(_arrs3);

            EL_Path.Clear();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("统计资源 Over", "This is Finished", "Okey");
        }

        static void StaSame(string[] _arrs)
        {
            Dictionary<string, HResInfo> m_dicFirst = new Dictionary<string, HResInfo>();
            Dictionary<string, HResSames> m_dicSames = new Dictionary<string, HResSames>();
            FileInfo _fInfo = null;
            float nLens = EL_Path.files.Count;
            int nIndex = 0;
            EditorUtility.DisplayProgressBar(string.Format("开始对比资源{0}/{1}", nIndex, nLens), "begin", 0);
            string _crc = "";
            HResInfo _info = null;
            HResSames _sames = null;
            foreach (string fp in EL_Path.files)
            {
                nIndex++;
                EditorUtility.DisplayProgressBar(string.Format("正在对比资源{0}/{1}", nIndex, nLens), fp, nIndex / nLens);
                _fInfo = new FileInfo(fp);
                // _crc = MD5Ex.Encrypt (_fInfo.OpenRead ());
                _crc = CRCClass.GetCRC(_fInfo.OpenRead());
                if (m_dicFirst.TryGetValue(_crc, out _info))
                {
                    if (!m_dicSames.TryGetValue(_crc, out _sames))
                    {
                        _sames = new HResSames(_crc);
                        m_dicSames.Add(_crc, _sames);
                    }
                    _sames.Add(_info);
                    _info = new HResInfo(_crc, fp);
                    _sames.Add(_info);
                }
                else
                {
                    _info = new HResInfo(_crc, fp);
                    m_dicFirst.Add(_crc, _info);
                }
            }

            nIndex = 0;
            nLens = m_dicSames.Count;
            EditorUtility.DisplayProgressBar(string.Format("开始记录资源{0}/{1}", nIndex, nLens), "begin", 0);
            StringBuilder build = new StringBuilder();
            foreach (var item in m_dicSames.Values)
            {
                nIndex++;
                EditorUtility.DisplayProgressBar(string.Format("正在记录资源{0}/{1}", nIndex, nLens), item.m_code, nIndex / nLens);
                item.ToStr(build, _arrs);
            }
            EditorUtility.ClearProgressBar();
            string _val = build.ToString();
            build.Clear();
            build.Length = 0;
            string _fp = string.Format("{0}/../../_same_res.txt", Application.dataPath);
            File.WriteAllText(_fp, _val);
        }

        static void StaAll(string[] _arrs)
        {
            float nLens = EL_Path.files.Count;
            int nIndex = 0;
            EditorUtility.DisplayProgressBar(string.Format("开始对比资源{0}/{1}", nIndex, nLens), "begin", 0);
            StringBuilder build = new StringBuilder();
            List<string> _list = null;
            foreach (string fp in EL_Path.files)
            {
                nIndex++;
                EditorUtility.DisplayProgressBar(string.Format("正在对比资源{0}/{1}", nIndex, nLens), fp, nIndex / nLens);
                build.AppendLine(fp);
                _list = HResTools.GetBeDepPath(fp, _arrs);
                if (_list != null && _list.Count > 0)
                {
                    for (int i = 0; i < _list.Count; i++)
                    {
                        build.AppendLine(_list[i]);
                    }
                }
                build.AppendLine();
            }

            EditorUtility.ClearProgressBar();
            string _val = build.ToString();
            build.Clear();
            build.Length = 0;
            string _fp = string.Format("{0}/../../__res.txt", Application.dataPath);
            File.WriteAllText(_fp, _val);
        }
    }

    class HResInfo
    {
        public string m_code = "";
        public string m_assetPath = "";

        public HResInfo(string code, string fp)
        {
            this.m_code = code;
            this.m_assetPath = fp;
        }
    }

    class HResSames
    {
        public string m_code = "";
        public List<HResInfo> m_lists = new List<HResInfo>(); // 相同对象

        public HResSames(string code)
        {
            this.m_code = code;
        }

        public void Add(HResInfo info)
        {
            if (info == null || this.m_lists.Contains(info))
                return;
            this.m_lists.Add(info);
        }

        public string ToStr()
        {
            StringBuilder build = new StringBuilder();
            ToStr(build);
            string _v = build.ToString();
            build.Clear();
            build.Length = 0;
            return _v;
        }

        public void ToStr(StringBuilder build)
        {
            build.AppendFormat("=== 拥有相同的Code = {0} ==", m_code);
            build.AppendLine();
            foreach (var item in m_lists)
            {
                build.AppendLine(item.m_assetPath);
            }
            build.AppendLine();
        }

        public void ToStr(StringBuilder build, string[] orgFiles)
        {
            build.AppendFormat("=== 拥有相同的Code = {0} ==", m_code);
            build.AppendLine();
            List<string> _list = null;
            foreach (var item in m_lists)
            {
                build.AppendLine(item.m_assetPath);
                _list = HResTools.GetBeDepPath(item.m_assetPath, orgFiles);
                if (_list != null && _list.Count > 0)
                {
                    for (int i = 0; i < _list.Count; i++)
                    {
                        build.AppendLine(_list[i]);
                    }
                }
                build.AppendLine();
            }
            build.AppendLine();
        }
    }

    static class HResTools
    {
        static public List<string> GetBeDepPath(string pP1, string[] pOrgs)
        {
            if (string.IsNullOrEmpty(pP1) || pOrgs == null || pOrgs.Length <= 0)
                return null;
            string _pIt = null;
            List<string> _list = new List<string>();
            for (int j = 0; j < pOrgs.Length; j++)
            {
                _pIt = pOrgs[j];
                _pIt = _pIt.Substring(_pIt.LastIndexOf("Assets"));
                _pIt = _pIt.Replace('\\', '/');
                if (IsInOrgPath(_pIt, pP1))
                    _list.Add(_pIt);
            }
            return _list;
        }

        static public bool IsInOrgPath(string pOrg, string pP1)
        {
            pP1 = pP1.Replace('\\', '/');
            string[] pArrs = AssetDatabase.GetDependencies(pOrg, true);
            foreach (string item in pArrs)
            {
                if (item.Equals(pP1))
                    return true;
            }
            return false;
        }
    }
}