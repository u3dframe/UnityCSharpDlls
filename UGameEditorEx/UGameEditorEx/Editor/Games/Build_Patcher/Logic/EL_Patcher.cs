using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using Core.Kernel;
using Core.Kernel.Cipher;

/// <summary>
/// 类名 : 版本信息，压缩文件逻辑
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-11-19 18:39
/// 功能 : 
/// </summary>
public class EL_Patcher
{
    PZipType m_pztype = PZipType.Main;

    GUIStyle styleYellow = new GUIStyle();
    GUIStyle styleRed = new GUIStyle();
    GUIStyle styleGreen = new GUIStyle();
    bool _isInited = false;
    string m_bundleIdentifier = "",m_bundleVersion = "",m_bundleVersionCode = "",m_bundleTemplate = "";
    bool m_isChangedBundleInfo = false;
    bool m_isNewAppDown = false;
    CfgVersion m_cfgVer{get{return CfgVersion.instance;}}
    List<ApkIpaInfo> m_lApkIpa = new List<ApkIpaInfo>();
    ApkIpaInfo m_apkIpa = null;
    int _nLensList = 0;
    Vector2 _v2Srl;    
    float _calcListY = 0,_tmpVal = 0;
    string m_descPkg = "";
    static int m_curMb = 40;
    bool m_isSyncVer = false, m_isSaveVer = false;

    private void Init()
    {
        if (_isInited)
            return;
        _isInited = true;

        m_descPkg = "所有下载地址 realurl = url + pkg + filename," +
			"比如:version真正的下载地址 = url_ver + pkg_ver + version.txt,\n"+
			"如果pkg_ver为空, = url_ver + version.txt";

        styleYellow.normal.textColor = Color.yellow;
        styleRed.normal.textColor = Color.red;
        styleGreen.normal.textColor = Color.green;

        m_bundleIdentifier =  PlayerSettings.applicationIdentifier;
		m_bundleVersion = PlayerSettings.bundleVersion;
		m_bundleVersionCode = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS ? PlayerSettings.iOS.buildNumber : PlayerSettings.Android.bundleVersionCode.ToString();

        m_isSyncVer = CfgPackage.instance.m_isSync2CfgVer;
        m_cfgVer.LoadDefault4EDT();
        m_lApkIpa.AddRange (m_cfgVer.m_lApkIpa);
    }

    public float DrawView()
    {
        Init();
        float _ret = EG_Helper.h28 * 2;
        EG_Helper.FEG_BeginVArea();

        EG_Helper.FEG_HeadTitMid("Patcher", Color.red);
        EG_Helper.FEG_Head("包体数据");
        _ret += EG_Helper.h28;
        EG_Helper.FEG_BeginH(0,20);
        {
            EditorGUILayout.LabelField("B_Identifier : ",EG_Helper.ToOptionW(100));
            m_bundleTemplate = EditorGUILayout.TextField(m_bundleIdentifier,EG_Helper.ToOptionW(300));
            if (!string.IsNullOrEmpty(m_bundleTemplate) && !m_bundleTemplate.Equals (m_bundleIdentifier)) {
                m_bundleIdentifier = m_bundleTemplate;
                m_isChangedBundleInfo = true;
            }

            EditorGUILayout.LabelField("B_Version : ",EG_Helper.ToOptionW(90));
            m_bundleTemplate = EditorGUILayout.TextField(m_bundleVersion,EG_Helper.ToOptionW(100));
            if (!string.IsNullOrEmpty(m_bundleTemplate) && !m_bundleTemplate.Equals (m_bundleVersion)) {
                m_bundleVersion = m_bundleTemplate;
                m_isChangedBundleInfo = true;
            }

            EditorGUILayout.LabelField("B_Code : ",EG_Helper.ToOptionW(90));
            m_bundleTemplate = EditorGUILayout.TextField(m_bundleVersionCode);
            if (!string.IsNullOrEmpty(m_bundleTemplate) && !m_bundleTemplate.Equals (m_bundleVersionCode)) {
                m_bundleVersionCode = m_bundleTemplate;
                m_isChangedBundleInfo = true;
            }
        }
        EG_Helper.FEG_EndH();
        _ret += EG_Helper.h28;

        EG_Helper.FEG_Head("Version 版本 Json 数据",Color.magenta);
        _ret += EG_Helper.h28;

        EG_Helper.FEG_ToggleRed("是否-重新整包下载???", ref m_isNewAppDown);
        if (m_isNewAppDown)
        {
            EG_Helper.FEG_BeginVArea();
            {
                EG_Helper.FEG_BeginH(0,EG_Helper.h26);
                EditorGUILayout.LabelField("大版本:",EG_Helper.ToOptionW(100));
                EditorGUILayout.LabelField(m_cfgVer.m_bigVerCode,EG_Helper.ToOptionW(200));
                if (GUILayout.Button("刷新大版本"))
                {
                    m_cfgVer.RefreshBigVerCode ();
                }
                EG_Helper.FEG_EndH();

                EG_Helper.FEG_BeginH(0,EG_Helper.h26);
                EditorGUILayout.LabelField("默认下载包体地址:",EG_Helper.ToOptionW(160));
                m_cfgVer.m_urlNewApkIpa = EditorGUILayout.TextField (m_cfgVer.m_urlNewApkIpa);
                EG_Helper.FEG_EndH();

                EG_Helper.FEG_BeginH(0,EG_Helper.h26);
                EditorGUILayout.LabelField("渠道标识_Channel:",EG_Helper.ToOptionW(200));
                EditorGUILayout.LabelField("渠道下载包体地址_Url:",EG_Helper.ToOptionW(260));
                if (GUILayout.Button("还原"))
                {
                    m_lApkIpa.Clear ();
                    m_lApkIpa.AddRange (m_cfgVer.m_lApkIpa);
                }
                if (GUILayout.Button("+"))
                {
                    m_lApkIpa.Add (new ApkIpaInfo ());
                }
                EG_Helper.FEG_EndH();

                _nLensList = m_lApkIpa.Count;
                _calcListY = ((_nLensList > 8 ? 8 : _nLensList)) * EG_Helper.h28;
                EG_Helper.FEG_BeginScroll(ref _v2Srl, _calcListY);
                {
                    for (int i = 0; i < _nLensList; i++) {
                        if (i >= m_lApkIpa.Count)
                            break;
                        m_apkIpa = m_lApkIpa [i];
                        EG_Helper.FEG_BeginH(0,EG_Helper.h20);
                        m_apkIpa.m_channel = EditorGUILayout.TextField (m_apkIpa.m_channel,EG_Helper.ToOptionW(200));
                        m_apkIpa.m_down_url = EditorGUILayout.TextField (m_apkIpa.m_down_url,EG_Helper.ToOptionW(260));
                        if (GUILayout.Button("X")) {
                            m_lApkIpa.RemoveAt (i);
                            i--;
                        }
                        EG_Helper.FEG_EndH();
                    }
                }
                EG_Helper.FEG_EndScroll();
            }
            EG_Helper.FEG_EndV();
            _ret += EG_Helper.h30 * 3 + _calcListY + 32;
        }
        _ret += EG_Helper.h28;

        EG_Helper.FEG_BeginVArea();
        EG_Helper.FEG_BeginH(0,EG_Helper.h20);
        EditorGUILayout.LabelField("新资源版本号:",EG_Helper.ToOptionW(100));
        EditorGUILayout.LabelField (m_cfgVer.m_resVerCode,styleGreen,EG_Helper.ToOptionW(200));
        if (GUILayout.Button("刷新")) {
			m_cfgVer.RefreshResVerCode ();
		}
        EG_Helper.FEG_EndH();
        _ret += EG_Helper.h20 + 11 * 2;
        EG_Helper.FEG_EndV();

        EG_Helper.FEG_BeginVArea();
        _tmpVal = EG_Helper.h20 * 2;
        EG_Helper.FEG_BeginH(0,_tmpVal);
        EditorGUILayout.LabelField (m_descPkg,EG_Helper.ToOptions(0,_tmpVal));
        EG_Helper.FEG_EndH();
        _ret += _tmpVal + 11 * 2;
        EG_Helper.FEG_EndV();

        EG_Helper.FEG_BeginVArea();
        EG_Helper.FEG_BeginH(0,EG_Helper.h20);
        EditorGUILayout.LabelField("game_ver:",EG_Helper.ToOptionW(100));
        m_cfgVer.m_gameVerCode = EditorGUILayout.TextField (m_cfgVer.m_gameVerCode,EG_Helper.ToOptions(0,20));
        EditorGUILayout.LabelField("展示在登录界面，用于检验资源是否更新了!!",styleYellow);
        EG_Helper.FEG_EndH();
        _ret += EG_Helper.h20 + 11 * 2;
        EG_Helper.FEG_EndV();
        
        EG_Helper.FEG_BeginVArea();
        EG_Helper.FEG_BeginH(0,EG_Helper.h20);
        EditorGUILayout.LabelField("url_ver:",EG_Helper.ToOptionW(100));
        m_cfgVer.m_urlVersion = EditorGUILayout.TextField (m_cfgVer.m_urlVersion,EG_Helper.ToOptions(0,20));
        EG_Helper.FEG_EndH();
        _ret += EG_Helper.h20 + 11 * 2;
        EG_Helper.FEG_EndV();

        EG_Helper.FEG_BeginVArea();
        EG_Helper.FEG_BeginH(0,EG_Helper.h20);
        EditorGUILayout.LabelField("pkg_ver:",EG_Helper.ToOptionW(100));
        m_cfgVer.m_pkgVersion = EditorGUILayout.TextField (m_cfgVer.m_pkgVersion,EG_Helper.ToOptions(0,20));
        EG_Helper.FEG_EndH();
        _ret += EG_Helper.h20 + 11 * 2;
        EG_Helper.FEG_EndV();

        EG_Helper.FEG_Toggle("是否-同步 CfgPkg 到 CfgVersion 里面？？", ref m_isSyncVer,styleYellow);
        if(m_isSyncVer){
            m_cfgVer.m_urlFilelist = "";
            m_cfgVer.m_pkgFilelist = "";
            m_cfgVer.m_pkgFiles = "";
        }else{
            EG_Helper.FEG_BeginVArea();
            EG_Helper.FEG_BeginH(0,EG_Helper.h20);
            EditorGUILayout.LabelField("url_filelist:",EG_Helper.ToOptionW(100));
            m_cfgVer.m_urlFilelist = EditorGUILayout.TextField (m_cfgVer.m_urlFilelist,EG_Helper.ToOptions(0,20));
            EG_Helper.FEG_EndH();
            _ret += EG_Helper.h20 + 11 * 2;
            EG_Helper.FEG_EndV();

            EG_Helper.FEG_BeginVArea();
            EG_Helper.FEG_BeginH(0,EG_Helper.h20);
            EditorGUILayout.LabelField("pkg_fl:",EG_Helper.ToOptionW(100));
            m_cfgVer.m_pkgFilelist = EditorGUILayout.TextField (m_cfgVer.m_pkgFilelist,EG_Helper.ToOptions(0,20));
            EG_Helper.FEG_EndH();
            _ret += EG_Helper.h20 + 11 * 2;
            EG_Helper.FEG_EndV();

            EG_Helper.FEG_BeginVArea();
            EG_Helper.FEG_BeginH(0,EG_Helper.h20);
            EditorGUILayout.LabelField("pkg_fls:",EG_Helper.ToOptionW(100));
            m_cfgVer.m_pkgFiles = EditorGUILayout.TextField (m_cfgVer.m_pkgFiles,EG_Helper.ToOptions(0,20));
            EG_Helper.FEG_EndH();
            _ret += EG_Helper.h20 + 11 * 2;
            EG_Helper.FEG_EndV();
        }
        
        EG_Helper.FEG_BeginVArea();
        EG_Helper.FEG_BeginH(0,EG_Helper.h20);
        EditorGUILayout.LabelField("zip大小:",EG_Helper.ToOptionW(100));
        m_curMb = EditorGUILayout.IntField (m_curMb,EG_Helper.ToOptions(40,20));
        EditorGUILayout.LabelField("MB",EG_Helper.ToOptionW(40));
        if (m_curMb <= 20)
			m_curMb = 20;
        BuildPatcher.m_limitZipSize = BuildPatcher.m_1mb * m_curMb;

        EditorGUILayout.LabelField("lua 编码/解码 key:",EG_Helper.ToOptionW(120));
        m_cfgVer.m_keyLua = EditorGUILayout.TextField (m_cfgVer.m_keyLua,EG_Helper.ToOptions(0,20));
        EG_Helper.FEG_EndH();
        _ret += EG_Helper.h20 + 11 * 2;
        EG_Helper.FEG_EndV();

        EG_Helper.FEG_BeginHArea(0,20);
        if (GUILayout.Button("保存版本信息")) {
			_SaveVersion();
		}
        if (GUILayout.Button("清除PacherCache文件夹")) {
			_ClearCache();
		}
        if (m_isSaveVer){
            EditorGUILayout.LabelField("Zip模式:",EG_Helper.ToOptions(40));
            m_pztype = (PZipType)EditorGUILayout.EnumPopup(m_pztype,EG_Helper.ToOptions(80));
            switch(m_pztype){
                case PZipType.Main:
                if (GUILayout.Button("Zip (主包)")) {
                    _ZipMain();
                }
                break;
                case PZipType.Main_Child:
                if (GUILayout.Button("Zip (主-子包)")) {
                    _ZipMainChild();
                }
                break;
                case PZipType.Main_Obb:
                if (GUILayout.Button("Zip (主-OBB包)")) {
                    _ZipMainObb();
                }
                break;
                case PZipType.All_Res:
                if (GUILayout.Button("Zip (全资源)")) {
                    _ZipAll();
                }
                break;
                default:
                if (GUILayout.Button("Zip (补丁)")) {
                    _ZipPatche();
                }
                break;
            }
        }
        _ret += EG_Helper.h20 + 11 * 2;
        EG_Helper.FEG_EndH();


        EG_Helper.FEG_EndV();
        return _ret - 10;
    }

    void _SaveVersion(){
		if (!m_isSyncVer && !m_cfgVer.IsUpdate (true)) {
			EditorUtility.DisplayDialog ("Tips", "请输入正确的[version地址,filelist地址,补丁下载地址]!!!!", "Okey");
			return;
		}

        if (m_isChangedBundleInfo) {
			m_isChangedBundleInfo = false;
			PlayerSettings.applicationIdentifier = m_bundleIdentifier;
			PlayerSettings.bundleVersion = m_bundleVersion;
			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS) {
				PlayerSettings.iOS.buildNumber = m_bundleVersionCode;
			} else {
				int pre = PlayerSettings.Android.bundleVersionCode;
				int cur = pre;
				if(int.TryParse(m_bundleVersionCode,out cur)){
					if (cur > pre) {
						PlayerSettings.Android.bundleVersionCode = cur;
					}
				}
			}
		}

		m_cfgVer.m_lApkIpa.Clear ();
		for (int i = 0; i < m_lApkIpa.Count; i++) {
			m_apkIpa = m_lApkIpa [i];
			if (string.IsNullOrEmpty (m_apkIpa.m_channel))
				continue;

			if (string.IsNullOrEmpty (m_apkIpa.m_down_url))
				continue;

			if (!m_apkIpa.m_down_url.StartsWith ("http://")) {
				continue;
			}
			m_cfgVer.m_lApkIpa.Add (m_apkIpa);
		}

		m_isSaveVer = true;
		m_cfgVer.SaveDefault ();
		XXTEA.SetCustKey(m_cfgVer.m_keyLua);
        
        bool isChgVer = !m_cfgVer.m_urlVersion.Equals(CfgPackage.instance.m_urlVersion) && !m_cfgVer.m_urlVersion.Equals(BuildPatcher.m_bk_url);
        if(isChgVer){
            CfgPackage.instance.m_urlVersion = m_cfgVer.m_urlVersion;
        }

        if(CfgPackage.instance.m_urlVersion.Equals(BuildPatcher.m_url_editor)){
            isChgVer = false;
            CfgPackage.instance.m_urlVersion = BuildPatcher.m_bk_url;
        }

        if(!m_cfgVer.m_pkgVersion.Equals(CfgPackage.instance.m_uprojVer)){
            CfgPackage.instance.m_uprojVer = m_cfgVer.m_pkgVersion;
            isChgVer = true;
        }

        if(m_isSyncVer != CfgPackage.instance.m_isSync2CfgVer){
            CfgPackage.instance.m_isSync2CfgVer = m_isSyncVer;
            isChgVer = true;
        }
        
        if(isChgVer){
            CfgPackage.SaveEditor(null);
        }
	}

    void _ClearCache(){
		BuildPatcher.ClearCache ();
	}

    void _ZipMain(){
		if (BuildPatcher.IsHasSpace()) {
			EditorUtility.DisplayDialog ("Tip", "有资源名带空格!!!!", "Okey");
			return;
		}
		BuildPatcher.ZipBuild (true);
	}

	void _ZipMainObb(){
		if (BuildPatcher.IsHasSpace()) {
			EditorUtility.DisplayDialog ("Tip", "有资源名带空格!!!!", "Okey");
			return;
		}
		if(EditorUtility.DisplayDialog("tip","是否确定出一个很小包，其他资源压入obb.zip里面?","Sure_Okey","Cancel"))
			BuildPatcher.ZipBuild (true,false,false,true);
	}

	void _ZipMainChild(){
		if (BuildPatcher.IsHasSpace()) {
			EditorUtility.DisplayDialog ("Tip", "有资源名带空格!!!!", "Okey");
			return;
		}
		if (!BuildPatcher.IsFile (BuildPatcher.m_fpMainRecordRes)) {
			EditorUtility.DisplayDialog ("Tip", "还未记录主包资源!!!!", "Okey");
			return;
		}
		BuildPatcher.ZipBuild (true,true);
	}

	void _ZipPatche(){
		if (BuildPatcher.IsHasSpace()) {
			EditorUtility.DisplayDialog ("Tip", "有资源名带空格!!!!", "Okey");
			return;
		}
		BuildPatcher.ZipPatche ();
	}

	void _ZipAll(){
		if (BuildPatcher.IsHasSpace()) {
			EditorUtility.DisplayDialog ("Tip", "有资源名带空格!!!!", "Okey");
			return;
		}
		BuildPatcher.ZipAll ();
	}
}