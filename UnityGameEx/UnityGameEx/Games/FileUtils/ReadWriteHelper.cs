using UnityEngine;
using System.Collections;
using System.IO;

namespace Core.Kernel
{
    /// <summary>
    /// 类名 : 路径 枚举
    /// 作者 : Canyon
    /// 日期 : 2017-03-07 09:29
    /// 功能 : 包路径，流文件夹路径，可读写路径
    /// </summary>
    public enum ET_FPType
    {
        UNITY_EDITOR = 0,
        UNITY_EDITOR_ANDROID = 1,
        UNITY_EDITOR_IOS = 2,
        UNITY_ANDROID = 3,
        UNITY_IOS = 4,
        UNITY_STANDALONE = 5,
    }

    /// <summary>
    /// 类名 : 读写帮助脚本
    /// 作者 : Canyon
    /// 日期 : 2017-03-07 09:29
    /// 功能 : 包路径，流文件夹路径，可读写路径
    /// </summary>
    public class ReadWriteHelper : FileEx
    {
        static public ET_FPType m_emFpType = ET_FPType.UNITY_EDITOR;
        static public readonly char[] m_cSpRow = "\r\n\t".ToCharArray();
        static public readonly char[] m_cSpComma = ",".ToCharArray();
        static public readonly char[] m_cSpEqual = "=".ToCharArray();

        // 平台
        static public readonly string platformAndroid = "Android";
        static public readonly string platformIOS = "IOS";
        static public readonly string platformWindows = "Windows";

        // 编辑器下面资源所在跟目录
        static protected readonly string m_assets = "Assets/";
        static protected readonly int m_nAssests = m_assets.Length;

        // Resources目录下面
        static protected readonly string m_fnResources = "Resources/";
        static protected readonly int m_nResources = m_fnResources.Length;

        /// <summary>
        /// 编辑器模式下是否通过加载ab资源得到
        /// </summary>
        static public bool m_isEdtiorLoadAsset = true;

        // 资源目录的根目录 (流文件夹和解压可读写文件夹下面的根路径)
        static public readonly string m_resFdRoot = "_resRoot";

        // 开发模式下，资源放的路径地址
        static public readonly string m_edtAssetPath = "_Develop";

        // 开发模式下，放到Resources目录下的资源地址
        static public readonly string m_edtResPath = "_Develop/Resources";

        // 编辑模式下Assets文件夹路径
        static public readonly string m_dirData = Application.dataPath + "/";
        static public readonly string m_dirDataNoAssets = Application.dataPath.Replace("Assets","");

        // 外部可读写的文件夹路径
        static public readonly string m_dirPersistent = Application.persistentDataPath + "/";

        // 流文件夹路径
        static public readonly string m_dirStreaming = Application.streamingAssetsPath + "/";

        // 自己封装的
        static public readonly string m_dirStreaming2 =
#if UNITY_EDITOR
				"file://"+Application.dataPath +"/StreamingAssets/";
#else
#if UNITY_ANDROID
				"jar:file://" + Application.dataPath + "!/assets/";
#elif UNITY_IOS
				"file://"+Application.dataPath +"/Raw/";
#else
                "file://" + Application.dataPath + "/StreamingAssets/";
#endif
#endif

        // 打包平台名
        /*
#if UNITY_ANDROID
		static public readonly string m_curPlatform = platformAndroid;
#elif UNITY_IOS
		static public readonly string m_curPlatform = platformIOS;
#else
        static public readonly string m_curPlatform = platformAndroid; // platformWindows
#endif
        */

        static public bool m_isEditor
        {
            get
            {
                return m_emFpType == ET_FPType.UNITY_EDITOR || m_emFpType == ET_FPType.UNITY_EDITOR_ANDROID || m_emFpType == ET_FPType.UNITY_EDITOR_IOS;
            }
        }

        static public bool m_isIOS
        {
            get
            {
                return m_emFpType == ET_FPType.UNITY_IOS || m_emFpType == ET_FPType.UNITY_EDITOR_IOS;
            }
        }

        static public string m_curPlatform
        {
            get
            {
                switch (m_emFpType)
                {
                    case ET_FPType.UNITY_IOS:
                    case ET_FPType.UNITY_EDITOR_IOS:
                        return platformIOS;
                    case ET_FPType.UNITY_STANDALONE:
                        return platformWindows;
                }
                return platformAndroid;
            }
        }

        // 资源相对路径
        static public readonly string m_assetRelativePath = string.Format("{0}/{1}/", m_resFdRoot, m_curPlatform);

        // 编辑模式下资源根目录
        static string _m_appAssetPath = "";
        static public string m_appAssetPath
        {
            get
            {
                if (string.IsNullOrEmpty(_m_appAssetPath))
                {
                    _m_appAssetPath = ReplaceSeparator(string.Format("{0}{1}/", m_dirData, m_edtAssetPath));
                }
                return _m_appAssetPath;
            }
        }

        // 游戏包内资源目录 - 流文件目录
        static string _m_appContentPath = "";
        static public string m_appContentPath
        {
            get
            {
                if (string.IsNullOrEmpty(_m_appContentPath))
                {
                    _m_appContentPath = ReplaceSeparator(string.Format("{0}{1}", m_dirStreaming, m_assetRelativePath));
                }
                return _m_appContentPath;
            }
        }

        // 解压的资源目录
        static string _m_appUnCompressPath = "";
        static public string m_appUnCompressPath
        {
            get
            {
                if (string.IsNullOrEmpty(_m_appUnCompressPath))
                {
                    string _dir = m_dirPersistent;
                    if (m_isEditor)
                    {
                        _dir = ReplaceSeparator(Application.dataPath);
                        int i = _dir.LastIndexOf('/');
                        {
                            // 将文件放到工程外部，与工程同级目录下面
                            _dir = _dir.Substring(0, i);
                            i = _dir.LastIndexOf('/');
                        }
                        _dir = _dir.Substring(0, i + 1);
                    }
                    _m_appUnCompressPath = string.Format("{0}{1}", _dir,m_assetRelativePath);
                    _m_appUnCompressPath = ReplaceSeparator(_m_appUnCompressPath);
                }
                return _m_appUnCompressPath;
            }
        }

        static public string GetStreamingFilePath(string fn)
        {
            return string.Concat(m_appContentPath, fn);
        }

        static public string m_dirRes
        {
            get
            {
                if (m_isEditor)
                    return m_isEdtiorLoadAsset ? m_appUnCompressPath : m_appAssetPath;
                return m_appUnCompressPath;
            }
        }

        static public string ReWwwUrl(string fp)
        {
            if(m_isEditor || m_isIOS)
			    fp = string.Concat ("file://", fp);
            return fp;
        }

        static public string ReUrlEnd(string url)
        {
            int _index = url.LastIndexOf("/");
            if (_index == url.Length - 1)
            {
                return url;
            }
            return string.Concat(url, "/");
        }

        static public string ReUrlTime(string url)
        {
            return string.Concat(url, "?time=", System.DateTime.Now.Ticks);
        }

        static public string ReUrlTime(string url, string fn)
        {
            url = ReUrlEnd(url);
            return string.Concat(url, fn, "?time=", System.DateTime.Now.Ticks);
        }

        static public string ReUrlTime(string url, string proj, string fn)
        {
            if (!string.IsNullOrEmpty(proj))
            {
                url = ReUrlEnd(url);
                url = string.Concat(url, proj);
            }
            return ReUrlTime(url, fn);
        }

        static public string[] Split(string val, char[] spt, bool isRmEmpty)
        {
            if (string.IsNullOrEmpty(val) || spt == null || spt.Length <= 0)
                return null;
            System.StringSplitOptions _sp = System.StringSplitOptions.None;
            if (isRmEmpty) _sp = System.StringSplitOptions.RemoveEmptyEntries;
            return val.Split(spt, _sp);
        }

        static public string[] SplitRow(string val)
        {
            return Split(val, m_cSpRow, true);
        }

        static public string[] SplitComma(string val)
        {
            return Split(val, m_cSpComma, false);
        }

        static public bool IsNullOrEmpty(string[] arrs)
        {
            if (arrs == null || arrs.Length <= 0)
                return true;
            return false;
        }
    }
}
