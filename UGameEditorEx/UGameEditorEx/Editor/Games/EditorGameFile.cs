using UnityEngine;
using UnityEditor;
#if UI_SPRITE_ATLAS
using UnityEditor.U2D;
using UnityEngine.U2D;
#endif

namespace Core
{
    using Kernel;
    /// <summary>
    /// 类名 : 编辑模式下 - 文件管理
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2020-03-26 09:29
    /// 功能 : 
    /// </summary>
    public class EditorGameFile : GameFile
    {
        // 相对目录(被打包的资源必须在该目录下，不然不会被打包)
        static public string m_rootRelative = "Builds";
        static public string m_excludeRelative = "Excludes"; // 排除打包的文件夹

        // 临时存放所有生成的文件(ab,txt等，可以进行Zip压缩，最后将生成zip文件或copy的所以文件再拷贝到流文件夹下)
        static public readonly string m_dirResCache = string.Format("{0}/../_AppBuilds/GameCache/{1}/", Application.dataPath, m_curPlatform);

        // zip 压缩文件
        static public readonly string m_fpZipCache = string.Format ("{0}resource.zip", m_dirResCache);
        static public readonly string m_fpZipCachePatch = string.Format("{0}res_patch.zip", m_dirResCache);
        static public readonly string m_fpZipObb = string.Format("{0}obb.zip", m_dirResCache);
        static public readonly string m_fpZipListCache = string.Concat(m_dirResCache, "ziplist.txt");
        static public readonly string m_fmtZipCache = string.Concat(m_dirResCache, "_zips/resource{0}.zip");
        static public readonly string m_fpZip = string.Format ("{0}resource.zip", m_dirStreaming);

        // 大小包(主子包) - 主包资源记录(ab资源) 用于打包
		static public readonly string m_fpMainRecordRes = string.Concat (Application.dataPath,"_mainRes.info");

        static public bool m_isLogType = false;
        static string m_strFolder2ABName = "Editor/Cfgs/ab_folder.txt";
        static CfgMustFiles _cfgFd2ABName = null;
        static CfgMustFiles m_cfgFd2ABName
        {
            get
            {
                if (_cfgFd2ABName == null)
                {
                    string fp = string.Concat(m_dirData, m_strFolder2ABName);
                    _cfgFd2ABName = CfgMustFiles.BuilderFp(fp);
                }
                return _cfgFd2ABName;
            }
        }

        static public void ReCfgFd2AB(string fn,bool isForce = false)
        {
            bool isEmpty = string.IsNullOrEmpty(fn);
            bool isSame = m_strFolder2ABName.Equals(fn);
            bool isOkey = !(isEmpty || isSame);
            isForce = isForce || isOkey;
            if (!isForce)
                return;

            _cfgFd2ABName = null;
            if(isOkey)
                m_strFolder2ABName = fn;
            _cfgFd2ABName = m_cfgFd2ABName;
        }

        static string m_strNoFolder2ABName = "Editor/Cfgs/ab_no_folder.txt";
        static CfgMustFiles _cfgNoFd2ABName = null;
        static CfgMustFiles m_cfgNoFd2ABName
        {
            get
            {
                if (_cfgNoFd2ABName == null)
                {
                    string fp = string.Concat(m_dirData, m_strNoFolder2ABName);
                    _cfgNoFd2ABName = CfgMustFiles.BuilderFp(fp);
                }
                return _cfgNoFd2ABName;
            }
        }

        static public void ReCfgNoFd2AB(string fn, bool isForce = false)
        {
            bool isEmpty = string.IsNullOrEmpty(fn);
            bool isSame = m_strNoFolder2ABName.Equals(fn);
            bool isOkey = !(isEmpty || isSame);
            isForce = isForce || isOkey;
            if (!isForce)
                return;

            _cfgNoFd2ABName = null;
            if (isOkey)
                m_strNoFolder2ABName = fn;
            _cfgNoFd2ABName = m_cfgNoFd2ABName;
        }

        static public bool IsInDevelop(string fp)
        {
            if (string.IsNullOrEmpty(fp))
                return false;
            return (fp.Contains(m_assets) && fp.Contains(m_edtAssetPath));
        }

        static public bool IsInBuild(string fp)
        {
            if (IsInDevelop(fp))
            {
                return fp.Contains(m_rootRelative) && !fp.Contains(m_excludeRelative);
            }
            return false;
        }

        static public bool IsShader(string fp)
        {
            if (string.IsNullOrEmpty(fp))
                return false;
            return fp.Contains("shaders/");
        }

        static public bool IsFont(string fp)
        {
            if (string.IsNullOrEmpty(fp))
                return false;
            return fp.Contains("fnts/");
        }

        static public bool IsUI(string fp)
        {
            if (string.IsNullOrEmpty(fp))
                return false;
            return fp.Contains("ui/");
        }

        static public bool IsTexture(string fp)
        {
            if (string.IsNullOrEmpty(fp))
                return false;
            return fp.Contains("textures/");
        }

        static public bool IsUITexture(string fp)
        {
            if (string.IsNullOrEmpty(fp))
                return false;
            return fp.Contains("/ui_");
        }

        static public bool IsSingleTexture(string fp)
        {
            if (string.IsNullOrEmpty(fp))
                return false;
            return fp.Contains("ui_sngs/") || fp.Contains("sngs/");
        }

        static public bool IsAtlasTexture(string fp)
        {
            if (string.IsNullOrEmpty(fp))
                return false;
            return fp.Contains("ui_atlas/");
        }

        static private bool IsMustAB(string fp)
        {
            if (string.IsNullOrEmpty(fp))
                return false;
            // 单图(icon,bg，大图片的) ，shader , 字体等必须要文件
            bool _isMust = IsShader(fp) || IsFont(fp) || IsSingleTexture(fp) || IsAtlasTexture(fp) || IsUI(fp);
            return _isMust || IsMustABByOther(fp);
        }

        static private bool IsMustABByOther(string fp){
            return MgrABDataDependence.IsMustFile(fp);
        }

        static public bool IsInBuild(string fp, ref bool isMust)
        {
            isMust = false;
            if (IsInBuild(fp))
            {
                isMust = IsMustAB(fp);
                return true;
            }
            return false;
        }

        static private string ReGetABName(string fp,string strEnd,bool isUseFolder,ref string nmFDir)
        {
            string _fn = GetFileNameNoSuffix(fp);
            string _sfp = RelativePath(fp);
            string _lft = _sfp.Split('.')[0];

            if(isUseFolder){
                _fn = _fn.Split('.')[0];// 可能有多个 . 号的文件
                _lft = LeftLast(_lft, "/"+_fn, false);
                nmFDir = RightLast(_lft, "/", false);
            }
            return _lft + strEnd;
        }

        static public void ReBindAB4SngOrAtlas(TextureImporter importer)
        {
            string fp = importer.assetPath;
            string _ab = importer.assetBundleName;
            string _abV = importer.assetBundleVariant;
            if (!IsInBuild(fp))
            {
                if(!string.Equals(_ab,null) || !string.Equals(_abV,null))
                    SetABInfo(importer);
                return;
            }
            (string _end, _, string _abExtension) = GetABEndName(fp,tpTex2D);
            if (string.IsNullOrEmpty(_end) || _end.EndsWith("error"))
            {
                if(!string.Equals(_ab,null) || !string.Equals(_abV,null))
                    SetABInfo(importer);
                return;
            }
            string _ret = _ab , _nm = null;
            bool isAtlas = IsAtlasTexture(fp);
            if(isAtlas || IsSingleTexture(fp)){
                _ret = ReGetABName(fp,_end,isAtlas,ref _nm);
                if (isAtlas)
                {
        #if UI_SPRITE_ATLAS
                    _ret = null;
                    _abExtension = null;
        #else
                    // 老模式导致图片
                    if (!_nm.Equals(importer.spritePackingTag))
                    {
                        importer.spritePackingTag = _nm;
                    }
        #endif
                }
            } else {
                if(!string.IsNullOrEmpty(_ab)){
                    if(IsAtlasTexture(_ab) || IsSingleTexture(_ab)){
                        _ret = null;
                        _abExtension = null;
                    }else{
                        string _lft = _ab.Split('.')[0];
                        if(!fp.Contains(_lft)){
                            _ret = null;
                            _abExtension = null;
                        }
                    }
                }
            }
            
            if(!string.Equals(_ab,_ret) || !string.Equals(_abV,_abExtension)){
                SetABInfo(importer, _ret, _abExtension);
            }
        }

        // 设置 资源 AssetBundle 信息
        static public void SetABInfo(string assetPath, string abName = "", string abSuffix = "")
        {
            AssetImporter _ai = AssetImporter.GetAtPath(assetPath);
            if (_ai == null)
                return;
            SetABInfo(_ai, abName, abSuffix);
        }

        static public void SetABInfo(AssetImporter assetImporter, string abName = "", string abSuffix = "")
        {
            if (!string.IsNullOrEmpty(assetImporter.assetBundleName))
            {
                // Debug.LogErrorFormat("=========is empty abname , a=[{0}]",assetImporter.assetPath);
                assetImporter.assetBundleName = null;
            }

            if (!string.IsNullOrEmpty(assetImporter.assetBundleVariant))
            {
                assetImporter.assetBundleVariant = null;
            }

            if (abName != null)
                abName = abName.Trim();
            bool isABName = !string.IsNullOrEmpty(abName);
            if (isABName)
            {
                // 资源名
                assetImporter.assetBundleName = abName.ToLower();
            }
            else
            {
                if(assetImporter is TextureImporter)
                {
                    var _tImp = assetImporter as TextureImporter;
                    _tImp.spritePackingTag = "";
                }
                return;
            }

            if (abSuffix != null)
                abSuffix = abSuffix.Trim();
            bool isABSuffix = !string.IsNullOrEmpty(abSuffix);
            if (isABSuffix)
            {
                // 资源后缀名
                assetImporter.assetBundleVariant = abSuffix.ToLower();
            }
        }

        static public void SetABInfo(UnityEngine.Object obj, string abName = "", string abSuffix = "")
        {
            string _assetPath = GetPath(obj);
            SetABInfo(_assetPath, abName, abSuffix);
        }

        static public string RelativePath(string fp)
        {
            // 去掉第一个Assets文件夹路径
            int _lens = m_rootRelative.Length + 1;
            int index = fp.LastIndexOf(m_rootRelative);
            return fp.Substring(index + _lens);
        }

        static public (string, bool, string) GetABEndName(string assetPath,System.Type objType) {
            string _suffix = GetSuffixNoPoint(assetPath);
            string _ret = null;
            bool _isMust = false;
            string abSuffix = null;
            if (IsUseDirName4ABName(assetPath))
            {
                _isMust = true;
                if (assetPath.Contains("lightmaps/"))
                    _ret = m_strLightmap;
                else if (assetPath.Contains("animations/"))
                    _ret = m_strFdAnmt;
                else
                    _ret = m_strFdDef;
            }
            if(_ret != null)
                return (_ret, _isMust, abSuffix);

            if (IsFont(assetPath))
            {
                _isMust = true;
                if (objType == tpFont)
                    _ret = m_strFnt;
                else if (objType == tpFontTMP)
                    _ret = m_strFntTMP;
                else
                    _ret = string.Concat(m_strFnt,"_", _suffix, "_error");
            }
            else if (IsShader(assetPath))
            {
                _isMust = true;
                if (objType == tpShader)
                    _ret = m_strShader;
                else
                    _ret = string.Concat(m_strShader,"_", _suffix, "_error");
            }
            else if (IsUI(assetPath))
            {
                _isMust = true;
                if (objType == tpGobj)
                    _ret = m_strUI;
                else
                    _ret = string.Concat(m_strUI,"_", _suffix, "_error");
            }
            else if (IsSingleTexture(assetPath))
            {
                _isMust = true;
                if (objType == tpTex2D)
                    _ret = m_strTex2D;
                else
                    _ret = string.Concat(m_strTex2D,"_sngs_", _suffix, "_error");
            }
            else if (IsAtlasTexture(assetPath))
            {
                _isMust = true;
#if UI_SPRITE_ATLAS
                if (objType == typeof(SpriteAtlas))
                    _ret = ".sa";
#else
                if (objType == tpTex2D)
                    _ret = m_strAtlas;
#endif
                else
                    _ret = string.Concat(m_strAtlas,"_", _suffix, "_error");
            }
            else if (objType == tpSVC)
            {
                _isMust = true;
                _ret = m_strSVC;
            }
            else if (objType == tpTimeline || "playable".Equals(_suffix, System.StringComparison.OrdinalIgnoreCase))
            {
                _ret = m_strTLine;
            }
            else if ("fbx".Equals(_suffix, System.StringComparison.OrdinalIgnoreCase))
            {
                _ret = m_strFbx;
            }
            else if (objType == tpSctObj || objType.IsSubclassOf(tpSctObj))
            {
                _ret = m_strScriptable;
            }
            else if (objType == tpGobj)
            {
                _isMust = true;
                _ret = m_strFab;
            }
            
            if (_ret == null)
            {
                _isMust = _isMust || IsMustABByOther(assetPath);
                if (objType == tpTex2D)
                {
                    _ret = m_strTex2D;
                }
                else if (objType == tpCube)
                {
                    _ret = m_strCube;
                }
                else if (objType == tpAdoClip)
                {
                    _ret = m_strAdoClip;
                }
                else if (objType == tpMat)
                {
                    _ret = m_strMat;
                }
                else if (objType == tpAnmClip)
                {
                    _ret = m_strAnmClip;
                }
            }
            if(_ret == null && m_isLogType)
                Debug.LogErrorFormat("====== ret is null , objType = [{0}], _isMust = [{1}]", objType, _isMust);
            return (_ret, _isMust, abSuffix);
        }

        static public (string, bool, string) GetABEndBy(UnityEngine.Object obj) {
            if(obj == null)
                return (null,false,null);

            string _assetPath = GetPath(obj);
            System.Type _objType = obj.GetType();
            return GetABEndName(_assetPath,_objType);
        }

        static public (string, bool, string) GetABEndBy(string assetPath)
        {
            return GetABEndBy(Load4Develop(assetPath));
        }

        static string GetNameEndAndExtension(UnityEngine.Object obj, ref string abSuffix)
        {
            string _assetPath = GetPath(obj);
            string _suffix = GetSuffixNoPoint(_assetPath);
            System.Type _objType = obj.GetType();
            (string _str, bool _isMust, string _abExtension) = GetABEndName(_assetPath,_objType);
            // Debug.LogErrorFormat("=====[{0}] =[{1}] =[{2}] =[{3}] =[{4}]",_str,_isMust,_abExtension,_assetPath,_objType);
            abSuffix = _abExtension;
            if (_isMust)
            {
                return _str;
            }
            // var _obj = MgrABDataDependence.GetData(obj);
            int _count = MgrABDataDependence.GetCount(obj);
            bool _isBl = !string.IsNullOrEmpty(_str);
            if (_isBl && _count > 1)
            {
                // Debug.Log(_obj);
                return _str;
            }

            if (_isBl)
                return string.Format("{0}_{1}_error", _str, _suffix);

            return string.Format(".{0}_error", _suffix);
        }

        static private bool IsUseDirName4ABName(string fp){
            return m_cfgFd2ABName.IsHas(fp) && !m_cfgNoFd2ABName.IsHas(fp);
        }

        static public string GetAbName(UnityEngine.Object obj, ref string abSuffix)
        {
            string fp = GetPath(obj);
            abSuffix = null;
            if (!IsInBuild(fp))
            {
                string _sErr = string.Format("只能导出Assets目录下面的[{0}]目录下的[{1}]目录里面的资源,请讲文件[{2}]移至[{1}]目录下面", m_edtAssetPath, m_rootRelative,fp);
                // throw new System.Exception(_sErr);
                Debug.LogError(_sErr);
                return "error";
            }

            bool _isUsefFolder = IsUseDirName4ABName(fp);
            string _end = GetNameEndAndExtension(obj, ref abSuffix);
            string _ret = null, _nm = null;
            if (IsAtlasTexture(fp) && obj.GetType() == tpTex2D)
            {
                TextureImporter _tImpt = TextureImporter.GetAtPath(fp) as TextureImporter;
                if (string.IsNullOrEmpty(_tImpt.assetBundleName))
                {
                    ReBindAB4SngOrAtlas(_tImpt);
                    // _ret = _lft.Replace(_fn, _tImpt.spritePackingTag) + _end; // 新版本
                    // _ret =  string.Format("ui/atlas/{0}{1}" ,_tImpt.spritePackingTag , _end); // old版本
                    // Debug.LogFormat("====== abname = [{0}] , spritePackingTag = [{1}]",_ret,_tImpt.spritePackingTag);
                }
                _ret = _tImpt.assetBundleName;
            }else{
                _ret = ReGetABName(fp,_end,_isUsefFolder,ref _nm);
            }

            return _ret.ToLower();
        }

        static public string GetAbName(string fp, ref string abSuffix)
        {
            UnityEngine.Object obj = Load4Develop(fp);
            return GetAbName(obj, ref abSuffix);
        }

        static public string GetFullPath4Ed(string assetPath)
        {
            return string.Format("{0}{1}", m_dirDataNoAssets, assetPath);
        }

        static public bool IsExistsInAssets(string assetPath)
        {
            string _mt = GetFullPath4Ed(assetPath);
            return IsFile(_mt);
        }
    }
}