using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Core;
using Core.Kernel;
using Core.Kernel.Cipher;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

/// <summary>
/// 类名 : ab资源操作
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-12-13 11:40
/// 功能 : 将所有资源拷贝到cache目录，然后生成filelist,如果要zip压缩就进行压缩,
/// 最后Zip或者所以文件拷贝到Assets下面流文件
/// </summary>
public class BuildPatcher : BuildBasic
{
    static public long m_1mb = 1024 * 1024;
    static public long m_limitZipSize = m_1mb * 40;
    static public bool m_isBuilded { get; private set; }

    static protected string luaDir = string.Format("{0}Lua/", m_dirData);
    static protected string txtDir = string.Format("{0}CsvTxts/", m_appAssetPath);
    static CfgVersion mgrCfg { get { return CfgVersion.instance; } }
    static List<ResInfo> m_list = new List<ResInfo>();
    static List<string> m_lstZips = new List<string>();
    static bool m_isMainChild = false; // 是否是主子包资源
    static bool m_isZipAllRes = true; // 在非补丁压缩情况下，是否-压缩全部
    static bool m_isOBB = false; // 是否用OBB
    static List<string> m_lstMainRes = new List<string>();
    static List<ResInfo> m_lstOtherRes = new List<ResInfo>();
    static EL_Path _ePath = null;
    static EL_Path m_elPath { get { if (_ePath == null) _ePath = EL_Path.builder; return _ePath; } }
    static bool m_isInited = false;
    static public string m_noZipFnInAssets = "Editor/Cfgs/no_zip_files.txt";
    static CfgMustFiles m_cfgNoZip = null;
    static public string m_mainChildResInAssets = "Editor/Cfgs/main_child.txt";
    static CfgMustFiles m_cfgMChild = null;
    static public string m_cpIgnoreFiles = "Editor/Cfgs/copy_ignore_files.txt";
    static public bool m_isDevelopment = true;
    static public string m_luacExe = "D:/Program Files (x86)/lua-5.3.5_Win64_bin/luac53.exe";

    static string[] m_ignoreFiles = {
            "version.txt",
            "filelist.txt",
            "filelist2.txt",
            "must_files.txt",
			".manifest",
            ".meta",
            ".svn",
            ".DS_Store",
        };

    static bool _IsIgnoreFile(string fp)
    {
        for (int i = 0; i < m_ignoreFiles.Length; i++)
        {
            if (fp.Contains(m_ignoreFiles[i]))
            {
                return true;
            }
        }
        return false;
    }

    static bool _IsLuaFile(string fp)
    {
        return fp.EndsWith(".lua", System.StringComparison.OrdinalIgnoreCase);
    }

    static void InitBuildPath(bool isForce = false)
    {
        if (m_isInited && !isForce)
            return;
        m_isInited = true;
        string fp = string.Concat(m_dirData, m_noZipFnInAssets);
        m_cfgNoZip = CfgMustFiles.BuilderFp(fp);

        fp = string.Concat(m_dirData, m_mainChildResInAssets);
        m_cfgMChild = CfgMustFiles.BuilderFp(fp);

        fp = string.Concat(m_dirData, m_cpIgnoreFiles);
        string _cval = GetText4File(fp);
        string[] _arrs = SplitRow(_cval);
        if (_arrs != null && _arrs.Length > 0)
            m_ignoreFiles = _arrs;
    }

    static protected void _CopyFiles(string dirSource, string dirDest, bool isInfo, List<ResInfo> list)
    {
        InitBuildPath();

        if (isInfo)
        {
            if (list == null)
                list = new List<ResInfo>();
        }

        CreateFolder(dirDest);
        m_elPath.DoClear();
        m_elPath.DoInit(dirSource);
        // Debug.Log("=== src path = " + dirSource);
        // Debug.LogError("=== dest path = " + dirDest);

        string _fpRelative = "";
        string _relStr = m_assetRelativePath;
        int indexRelative = 0;
        int lensRelative = _relStr.Length;

        ResInfo _info;
        int _fLens = 0;
        string _fpReEncode = "";
        string _fpCompareCode = "";

        float nLens = m_elPath.m_files.Count;
        int nIndex = 0;
        bool isLuaFile = false, isWorldFile = false; ;

        string _fmtFile = "正在Copy文件{0}/{1}";
        string _fmtFileLua = "正在Copy Lua文件{0}/{1}";

        foreach (string fp in m_elPath.m_files)
        {
            nIndex++;

            if (isInfo && _IsIgnoreFile(fp))
                continue;

            isLuaFile = _IsLuaFile(fp);
            isWorldFile = false;

            if (isLuaFile)
            {
                // _fpRelative = fp.Replace(dirSource, "Lua/");
                // _fpRelative = _fpRelative.Replace("Lua/Lua/", "Lua/");
                _fpRelative = RightLast(fp, "Lua/", true);
            }
            else if (IsTextInCT(fp))
            {
                isWorldFile = true;
                _fpRelative = RightLast(fp, "CsvTxts/", false);
            }
            else if (IsFileInCT(fp))
            {
                _fpRelative = RightLast(fp, "CsvTxts/", false);
            }
            else
            {
                indexRelative = fp.IndexOf(_relStr);
                if (indexRelative < 0)
                    continue;

                _fpRelative = fp.Substring(indexRelative + lensRelative);
            }

            EditorUtility.DisplayProgressBar(string.Format(isLuaFile ? _fmtFileLua : _fmtFile, nIndex, nLens), _fpRelative, nIndex / nLens);
            CreateFolder(dirDest);

            if (isLuaFile || isWorldFile)
                _fpReEncode = _EncodeWorldFile(fp, dirDest, _fpRelative, ref _fLens, ref _fpCompareCode);
            else
                _fpReEncode = _EncodeFile(fp, dirDest, _fpRelative, ref _fLens, ref _fpCompareCode);

            if (isInfo)
            {
                _info = new ResInfo(_fpReEncode, _fpCompareCode, "", _fLens, _fpRelative);
                list.Add(_info);
            }
        }

        m_elPath.DoClear();
    }

    static string _custKey = null;
    static string EncodeDestFname(string srcFn, string compareCode)
    {
        if (!string.IsNullOrEmpty(XXTEA.custKey) && string.IsNullOrEmpty(_custKey))
        {
            _custKey = XXTEA.custKey;
        }

        bool isLua = srcFn.Contains(".lua");
        bool isMovie = srcFn.Contains(".mp4") || srcFn.Contains("movies/");
        string _suffix = ".gbts";
        if (isLua)
            srcFn = srcFn.Replace("Lua/", "");
        else if (isMovie)
            _suffix = GetSuffixToLower(srcFn);

        string _ck = IsUpdateUIRes(srcFn) ? null : _custKey;
        XXTEA.SetCustKey(_ck);
        _ck = string.Format("{0}@@{1}", srcFn, compareCode);
        string _ret = XXTEA.Encrypt(_ck);
        string _c32 = CRCClass.GetCRCContent(_ret);
        return string.Format((isLua ? "Lua/{0}{1}" : "{0}{1}"), _c32,_suffix);
    }

    // static void _MvFile(string srcFile, string outFile)
    // {
    //     FileInfo fInfo = new FileInfo(srcFile);
    //     fInfo.CopyTo(outFile, true);
    // }
	
	static string _EncodeWorldFile(string srcFile, string outDir, string realName, ref int fSize, ref string compareCode)
    {
        string _readFile = srcFile;
        string resName = null, outFile = null;
        bool isLuaFile = _IsLuaFile(srcFile);
        bool _isLuac = false;
        if(isLuaFile && m_isDevelopment)
        {
            outFile = GetFileName(realName);
            outFile = string.Concat(outDir, "../../_AppLuaOut/", outFile);
            CreateFolder(outFile);
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = m_luacExe;
            info.Arguments = string.Format(" -o {0} {1}",outFile,srcFile);
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.CreateNoWindow = true;
            info.UseShellExecute = true;
            info.WorkingDirectory = outDir;
            info.ErrorDialog = true;

            Process pro = Process.Start(info);
            pro.WaitForExit();
            _readFile = outFile;
            _isLuac = true;
        }

        
        string v64 = null;
        byte[] bts = null;
        if (_isLuac)
        {
            bts = File.ReadAllBytes(_readFile);
            v64 = Encrypt(bts);
        }
        else
        {
            string val = File.ReadAllText(_readFile);
            v64 = Encrypt(val);
        }

        compareCode = CRCClass.GetCRCContent(v64);
        resName = EncodeDestFname(realName, compareCode);
        outFile = string.Concat(outDir, resName);
        CreateFolder(outFile);
        if(bts == null || IsEnCode())
            WriteFile(outFile, v64);
        else
            WriteFile(outFile, bts);

        FileInfo fInfo = new FileInfo(outFile);
        fSize = (int)fInfo.Length;
        return resName;
    }

    static string _EncodeFile(string srcFile, string outDir, string realName, ref int fSize, ref string compareCode)
    {
        FileInfo fInfo = new FileInfo(srcFile);
        compareCode = CRCClass.GetCRC(fInfo.OpenRead());
        string resName = EncodeDestFname(realName, compareCode);
        string outFile = string.Concat(outDir, resName);
        CreateFolder(outFile);
        fInfo.CopyTo(outFile, true);
        fInfo = new FileInfo(outFile);
        fSize = (int)fInfo.Length;
        return resName;
    }

    static void _Copy2Cache(List<ResInfo> list)
    {
        m_isDevelopment = m_isDevelopment && IsFile(m_luacExe);
        DelFolder(m_dirResCache);
        _CopyFiles(m_dirRes, m_dirResCache, true, list);
        // lua文件 可先byte，删除原来文件，最后在生成resInfo
        // string _destLua = string.Concat(m_dirResCache, "Lua/");
        // _destLua = _destLua.Replace("\\", "/");
        // DelFolder(_destLua);
        // _destLua = m_dirResCache;
        // _CopyFiles (LuaConst.toluaDir, _destLua, true, list);
        // _CopyFiles (LuaConst.luaDir, _destLua, true, list);

        _CopyFiles(luaDir, m_dirResCache, true, list);
        _CopyFiles(txtDir, m_dirResCache, true, list);
    }

    static void _MakeNewFilelist(List<ResInfo> list, string flname = "filelist.txt")
    {
        try
        {
            string _fp = string.Concat(m_dirResCache, flname);
            CreateFolder(_fp);

            bool _isDefaultFilelist = _fp.EndsWith("filelist.txt");
            CfgFileList _cl = _isDefaultFilelist ? CfgFileList.Builder() : null;

            using (FileStream stream = new FileStream(_fp, FileMode.OpenOrCreate))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    ResInfo _it_ = null;
                    for (int i = 0; i < list.Count; i++)
                    {
                        _it_ = list[i];
                        writer.WriteLine(_it_.ToString());
                        if (_cl != null)
                            _cl.Add(_it_);
                    }
                }
            }
            if (_isDefaultFilelist)
            {
                mgrCfg.m_codeFilelist = CRCClass.GetCRC(_fp);
                mgrCfg.SaveDefault();
                if (_cl != null)
                    _cl.Save2Default();
            }
        }
        catch
        {
        }
    }

    static bool _IsMovieRes(string resName)
    {
        return resName.EndsWith(".mp4");
    }

    static bool _IsMainChildRes(string resName)
    {
        resName = resName.ToLower();
        // 平台判断
        string _pl = m_curPlatform.ToLower();
        if (resName.Equals(_pl))
            return true;

        if (IsTextInCT(resName))
            return true;

        if (m_cfgMChild != null)
        {
            if (m_cfgMChild.IsHas(resName))
                return true;
        }

        string _tmp = "";
        for (int i = 0; i < m_lstMainRes.Count; i++)
        {
            _tmp = m_lstMainRes[i];
            if (_tmp.IndexOf(resName) != -1)
            {
                return true;
            }
        }

        return false;
    }

    // 是否 - 不需要zip的文件(结合 - m_isZipAll 和 isNotPatch)
    static bool _IsNoZipRes(string resName)
    {
        if (m_cfgNoZip != null)
            return m_cfgNoZip.IsHas(resName);
        return false;
    }

    // isNotPatch : 是否 - 不是补丁zip
    static void _ZipFiles(List<ResInfo> list, bool isNotPatch, string packageZip = "")
    {
        string _destFile = isNotPatch ? m_fpZipCache : m_fpZipCachePatch;
        DelFile(_destFile);
        CreateFolder(_destFile);

        Queue<ZipClass> _listZip = new Queue<ZipClass>();
        ZipClass zip = null;
        ResInfo _info;
        string _nmJugde = "";
        string _zipName = "";
        string _fpRes = "";

        int _index = 0;
        long _curSize = 0;
        int _allZipSize = 0;

        if (isNotPatch)
        {
            _destFile = string.Format(m_fmtZipCache, _index);
            m_lstZips.Add(_index.ToString());
        }

        zip = new ZipClass(_destFile);
        _listZip.Enqueue(zip);

        for (int i = 0; i < list.Count; i++)
        {
            _info = list[i];
            if (isNotPatch)
            {
                _nmJugde = _info.m_curName;
                if (IsUpdateUIRes(_nmJugde) || _IsMovieRes(_nmJugde))
                    continue;

                if (m_isMainChild && !_IsMainChildRes(_nmJugde))
                {
                    m_lstOtherRes.Add(_info);
                    continue;
                }

                // 在非全部压缩的情况下
                if (!m_isZipAllRes && _IsNoZipRes(_nmJugde))
                    continue;

                _allZipSize++;

                _curSize += _info.m_size;
                if (_curSize >= m_limitZipSize)
                {
                    _curSize = 0;
                    _index++;
                    _destFile = string.Format(m_fmtZipCache, _index);
                    m_lstZips.Add(_index.ToString());
                    zip = new ZipClass(_destFile);
                    _listZip.Enqueue(zip);
                }
            }

            _zipName = _info.m_resName;
            if (!string.IsNullOrEmpty(packageZip))
                _zipName = string.Concat(packageZip, "/", _zipName);

            _fpRes = string.Concat(m_dirResCache, _info.m_resName);
            if (IsFile(_fpRes))
                zip.AddFile(_fpRes, _zipName);
        }

        _zipName = "version.txt";
        _fpRes = curInstance.GetFilePath(_zipName);
        if (!isNotPatch)
        {
            if (!string.IsNullOrEmpty(mgrCfg.m_pkgVersion))
                _zipName = string.Concat(mgrCfg.m_pkgVersion, "/", _zipName);
        }
        if (IsFile(_fpRes))
            zip.AddFile(_fpRes, _zipName);

        _zipName = "filelist.txt";
        _fpRes = string.Concat(m_dirResCache, _zipName);
        if (!isNotPatch)
        {
            if (!string.IsNullOrEmpty(mgrCfg.m_pkgFilelist))
                _zipName = string.Concat(mgrCfg.m_pkgFilelist, "/", _zipName);
        }
        if (IsFile(_fpRes))
            zip.AddFile(_fpRes, _zipName);

        _zipName = "must_files.txt";
        _fpRes = curInstance.GetFilePath("must_files.txt");
        if (!isNotPatch)
        {
            if (!string.IsNullOrEmpty(mgrCfg.m_pkgFilelist))
                _zipName = string.Concat(mgrCfg.m_pkgFilelist, "/", _zipName);
        }
        if (IsFile(_fpRes))
            zip.AddFile(_fpRes, _zipName);

        if (m_isMainChild && isNotPatch)
        {
            _zipName = "filelist2.txt";
            _fpRes = string.Concat(m_dirResCache, _zipName);
            _MakeNewFilelist(m_lstOtherRes, _zipName);
            if (IsFile(_fpRes))
                zip.AddFile(_fpRes, _zipName);
        }

        m_lstZips.Add(_allZipSize.ToString());
        _WriteZipListTxt(string.Join(",", m_lstZips.ToArray()));
        _ExcuteZips(_listZip);
    }

    // 压缩OBB
    static void _ZipOBB(List<ResInfo> list, string packageZip = "")
    {
        string _destFile = m_fpZipObb;
        DelFile(_destFile);
        CreateFolder(_destFile);

        ZipClass zip = null;
        ResInfo _info;
        string _nmJugde = "";
        string _zipName = "";
        string _fpRes = "";
        zip = new ZipClass(_destFile);
        for (int i = 0; i < list.Count; i++)
        {
            _info = list[i];
            _nmJugde = _info.m_curName;
            if (IsUpdateUIRes(_nmJugde) || _IsMovieRes(_nmJugde))
                continue;

            if (m_isMainChild && !_IsMainChildRes(_nmJugde))
            {
                m_lstOtherRes.Add(_info);
                continue;
            }

            if (!_IsNoZipRes(_nmJugde))
                continue;

            _zipName = _info.m_resName;
            if (!string.IsNullOrEmpty(packageZip))
                _zipName = string.Concat(packageZip, "/", _zipName);

            _fpRes = string.Concat(m_dirResCache, _info.m_resName);
            if (IsFile(_fpRes))
                zip.AddFile(_fpRes, _zipName);
        }
        _ZipOne(ref zip);
    }

    static void _ExcuteZips(Queue<ZipClass> list)
    {
        ZipClass zip = null;
        while (list.Count > 0)
        {
            if (zip == null)
            {
                zip = list.Peek();
                if (_ZipOne(ref zip))
                {
                    zip = null;
                    list.Dequeue();
                }
            }
        }
    }

    static bool _ZipOne(ref ZipClass zip)
    {
        ZipState state = zip.m_zipState;
        try
        {
            zip.Begin();
            while (!zip.m_bFinished)
            {
                state = zip.m_zipState;
                EditorUtility.DisplayProgressBar(string.Format("正在压缩资源{0}/{1}", state.m_nZipedFileCount, state.m_nAllFileCount),
                    state.m_strCurFileName,
                    (float)state.m_nZipedFileCount / state.m_nAllFileCount);
            }

            if (zip.error != null)
            {
                zip.Close();
                throw zip.error;
            }
            zip.Close();
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogErrorFormat("=== zip error , filename = [{0}] , filepath = [{1}] , err = [{2}]", state.m_strCurFileName,state.m_strCurFilePath, ex);
            throw ex;
        }
    }

    static void _WriteZipListTxt(string content)
    {
        string _destFile = m_fpZipListCache;
        DeleteFile(_destFile, true);
        CreateFolder(_destFile);
        WriteFile(_destFile,content);
    }

    static void _ToSteamingAssets(bool isZip) { 
        EditorUtility.DisplayProgressBar("Patachering", "正在CopyToStreaming...", 0.9f);
        DelFolder(m_dirStreaming);
        CreateFolder(m_appContentPath);
        string _fpsrc = "" , _fpdest = "";
        if (isZip)
        {
            if (File.Exists(m_fpZipCache))
            {
                CreateFolder(m_fpZip);
                File.Copy(m_fpZipCache, m_fpZip, true);
            }

            int _lens = m_lstZips.Count - 1;
            if (_lens > 0)
            {
                for (int i = 0; i < _lens; i++)
                {
                    _fpsrc = string.Format(m_fmtZipCache, m_lstZips[i]);
                    _fpdest = string.Format(m_fmtZip, m_lstZips[i]);
                    CreateFolder(_fpdest);
                    File.Copy(_fpsrc, _fpdest, true);
                }
                m_lstZips.Clear();

                if (File.Exists(m_fpZipListCache))
                    File.Copy(m_fpZipListCache, m_fpZipList, true);
            }

            ResInfo _info;
            string _nmJugde = "";
            bool _isOkey = false;
            for (int i = 0; i < m_list.Count; i++)
            {
                _info = m_list[i];
                _nmJugde = _info.m_curName;
                _isOkey = IsUpdateUIRes(_nmJugde) || _IsMovieRes(_nmJugde);
                _isOkey = _isOkey || (m_isMainChild && _IsMainChildRes(_nmJugde) && _IsNoZipRes(_nmJugde));
                _isOkey = _isOkey || (!m_isMainChild && !m_isZipAllRes && !m_isOBB && _IsNoZipRes(_nmJugde));
                if (_isOkey)
                {
                    _fpsrc = string.Concat(m_dirResCache, _info.m_resName);
                    _fpdest = string.Concat(m_appContentPath, _info.m_resName);
                    CreateFolder(_fpdest);
                    File.Copy(_fpsrc, _fpdest, true);
                }
            }
        }
        else
        {
            _CopyFiles(m_dirResCache, m_appContentPath, false, null);
        }

        // 拷贝版本文件
        _fpsrc = curInstance.GetFilePath("version.txt");
        _fpdest = string.Concat(m_appContentPath, "version.txt");
        File.Copy(_fpsrc, _fpdest, true);
    }

    static void _CopyFilelistToRes()
    {
        string _srcFp = string.Concat(m_dirResCache, "filelist.txt");
        string _destFp = string.Concat(m_dirRes, "filelist.txt");
        if (File.Exists(_srcFp))
            File.Copy(_srcFp, _destFp, true);
    }

    static void _Build(bool isZip, bool isNotPatch, bool isZipAllRes = true, bool isZipObb = false)
    {
        ReInit();
        m_isZipAllRes = isZipAllRes; // 是否 - 全部资源都可以被zip
        m_isOBB = isZipObb;
        EditorUtility.DisplayProgressBar("Patachering", "开始处理...", 0);
        _Copy2Cache(m_list);
        _MakeNewFilelist(m_list);
        if (isZip)
        {
            _ZipFiles(m_list, isNotPatch);
        }

        if (isZipObb)
        {
            _ZipOBB(m_list);
        }

        EditorUtility.ClearProgressBar();

        if (isNotPatch)
        {
            // 不是补丁
            _ToSteamingAssets(isZip);
            _CopyFilelistToRes();
            AssetDatabase.Refresh();
            // System.Threading.Tasks.Task.Factory.StartNew(_TaskDelay, isZip);
        }

        m_isBuilded = true;

        EditorUtility.ClearProgressBar();
    }

    static async void _TaskDelay(object obj)
    {
        await System.Threading.Tasks.Task.Delay(1000);
        // 不是补丁
        bool isZip = (bool)obj;
        _ToSteamingAssets(isZip);
        _CopyFilelistToRes();
        EditorUtility.ClearProgressBar();
        m_isBuilded = true;
    }

    // 子包资源压缩
    static void _BuildSecond()
    {
        ResInfo _info;
        string _nmJugde = "";
        List<ResInfo> list = new List<ResInfo>();
        for (int i = 0; i < m_list.Count; i++)
        {
            _info = m_list[i];
            _nmJugde = _info.m_curName;
            if (IsUpdateUIRes(_nmJugde))
            {
                continue;
            }

            if (_IsMainChildRes(_nmJugde))
            {
                continue;
            }
            list.Add(_info);
        }
        _ZipFiles(list, false, mgrCfg.m_pkgFiles);
        EditorUtility.ClearProgressBar();
    }

    static void ZipBuild(bool isZip, bool isMainChild = false, bool isZipAllRes = false, bool isZipObb = false)
    {
        m_isMainChild = isMainChild;
        ClearAll();

        if (isMainChild)
        {
            string v = File.ReadAllText(m_fpMainRecordRes);
            string[] arrs = v.Split("\r\n\t".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in arrs)
            {
                m_lstMainRes.Add(item.ToLower());
            }
        }

        _Build(isZip, true, isZipAllRes, isZipObb);

        if (isMainChild)
            _BuildSecond();
        else
            ZipPatche();

        SaveAssets(null);
    }

    static public void ZipMain()
    {
        try
        {
            ZipBuild(true);
        }
        catch (System.Exception)
        {
            EditorUtility.ClearProgressBar();
            throw;
        }
    }

    static public void ZipMainChild()
    {
        try
        {
            ZipBuild(true, true);
        }
        catch (System.Exception)
        {
            EditorUtility.ClearProgressBar();
            throw;
        }
    }

    static public void ZipMainObb()
    {
        try
        {
            ZipBuild(true, false, false, true);
        }
        catch (System.Exception)
        {
            EditorUtility.ClearProgressBar();
            throw;
        }
    }

    static public void ZipPatche()
    {
        try
        {
            if (!m_isBuilded)
                _Build(false, false);

            EditorUtility.DisplayProgressBar("Build - 补丁", "开始比对...", 0);

            CompareFiles comFile = new CompareFiles();
            comFile.m_cfgOld.Load(CfgFileList.m_defFileName);
            comFile.m_cfgNew.LoadFP(string.Concat(m_dirResCache, "filelist.txt"));
            comFile.DoCompare();

            var ups = comFile.m_updates;
            m_list.Clear();
            foreach (var item in ups.Values)
            {
                m_list.Add(item);
            }

            _ZipFiles(m_list, false, mgrCfg.m_pkgFiles);

            _CopyFilelistToRes();

            EditorUtility.ClearProgressBar();
        }
        catch (System.Exception)
        {
            EditorUtility.ClearProgressBar();
            throw;
        }
    }

    static public void ZipAll()
    {
        try
        {
            if (!m_isBuilded)
                _Build(false, false);

            EditorUtility.DisplayProgressBar("Build - 全资源", "压缩所有文件", 0);
            string _fp = string.Concat(m_dirResCache, "filelist.txt");
            CfgFileList flt = CfgFileList.BuilderFp(_fp);

            m_list.Clear();
            m_list.AddRange(flt.GetList(false));

            _ZipFiles(m_list, false, mgrCfg.m_pkgFiles);

            _CopyFilelistToRes();

            EditorUtility.ClearProgressBar();
        }
        catch (System.Exception)
        {
            EditorUtility.ClearProgressBar();
            throw;
        }
    }

    static public void ClearCache()
    {
        ClearAll();
        DelFolder(m_dirResCache);
    }

    static void ReInit()
    {
        m_list.Clear();
        m_lstZips.Clear();
        m_isBuilded = false;
        m_isZipAllRes = true;
        m_elPath.DoClear();
    }

    static public void ClearAll()
    {
        ReInit();
        m_lstMainRes.Clear();
        m_lstOtherRes.Clear();
    }

    static public void SaveVerZip(string gmVer, string keyLua, string url, string proj, PZipType pztype = PZipType.Main, bool isSync2CfgVer = true, string flUrl = "", string flPkg = "", string flsPkg = "")
    {
        GameFile.CurrDirRes();
        XXTEA.SetCustKey(keyLua);

        CfgVersion _cfgVer = CfgVersion.instance;
        CfgPackage _cfgPkg = CfgPackage.instance;

        _cfgVer.LoadDefault4EDT();
        _cfgVer.m_gameVerCode = gmVer;
        _cfgVer.m_keyLua = keyLua;
        _cfgVer.m_urlVersion = url;
        _cfgVer.m_pkgVersion = proj;
        _cfgVer.m_urlFilelist = flUrl;
        _cfgVer.m_pkgFilelist = flPkg;
        _cfgVer.m_pkgFiles = flsPkg;

        bool isChgVer = !string.IsNullOrEmpty(url) && !url.Equals(_cfgPkg.m_urlVersion) && !url.Equals(m_bk_url);
        if (isChgVer)
        {
            _cfgPkg.m_urlVersion = url;
        }

        if (_cfgPkg.m_urlVersion.Equals(m_url_editor))
        {
            isChgVer = false;
            _cfgPkg.m_urlVersion = m_bk_url;
        }

        if (!string.IsNullOrEmpty(proj) && !proj.Equals(_cfgPkg.m_uprojVer))
        {
            _cfgPkg.m_uprojVer = proj;
            isChgVer = true;
        }

        if (isSync2CfgVer != _cfgPkg.m_isSync2CfgVer)
        {
            _cfgPkg.m_isSync2CfgVer = isSync2CfgVer;
            isChgVer = true;
        }

        if (isChgVer)
        {
            CfgPackage.SaveEditor(null);
        }

        if (isSync2CfgVer)
        {
            _cfgVer.m_urlFilelist = "";
            _cfgVer.m_pkgFilelist = "";
            _cfgVer.m_pkgFiles = "";
        }

        _cfgVer.RefreshResVerCode();
        _cfgVer.SaveDefault();

        switch (pztype)
        {
            case PZipType.Main:
                ZipMain();
                break;
            case PZipType.Main_Child:
                ZipMainChild();
                break;
            case PZipType.Main_Obb:
                ZipMainObb();
                break;
            case PZipType.All_Res:
                ZipAll();
                break;
            default:
                ZipPatche();
                break;
        }
    }

    static public void ReLoadCfgs()
    {
        InitBuildPath(true);
        ReCfgFd2AB("Editor/Cfgs/ab_folder.txt", true);
        ReCfgNoFd2AB("Editor/Cfgs/ab_no_folder.txt", true);
        MgrABDataDependence.instance.InitIgnoreAndMust(true);
    }
}