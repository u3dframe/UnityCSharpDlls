#define USE_BPatcher
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UObject = UnityEngine.Object;

namespace Core.Kernel
{

    /// <summary>
    /// 类名 : 图片压缩格式处理
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2017-12-13 11:40
    /// 功能 : 
    /// </summary>
    public class EDW_HandlerTex : EditorWindow
    {
        static bool isOpenWindowView = false;

        static protected EDW_HandlerTex vwWindow = null;

        // 窗体宽高
        static public float width = 600;
        static public float height = 460;

        [MenuItem("Tools/Image Settings", false, 10)]
        static void AddWindow()
        {
            if (isOpenWindowView || vwWindow != null)
                return;

            try
            {
                isOpenWindowView = true;

                vwWindow = GetWindow<EDW_HandlerTex>("Image Settings");
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
        string suffixToLowers = ".png.exr.tga.tif.psd";
        int _maxTextureSize = 2048;
        int _compressionQuality = 60;
        int _mipmap = 0;
        int _alphaIs = 0;
        static bool _isASTC = true;
        static int _sRGBN = 0;
        static int _hdUI = 0;
        static bool _isMustSetMaxSize = false;

        GUIStyle s_c = new GUIStyle();

        #region  == EditorWindow Func ===

        void OnGUI()
        {
            EditorGUILayout.Space(10);
            mObj = EditorGUILayout.ObjectField("源文件夹:", mObj, typeof(UObject), false);
            if (mObj != null)
                _path = AssetDatabase.GetAssetPath(mObj);
            else
                _path = "";
            EditorGUILayout.Space(10);
            suffixToLowers = EditorGUILayout.TextField("筛查后缀名:", suffixToLowers);
            EditorGUILayout.Space(10);
            s_c.normal.textColor = new Color32(10,226,195,255);
            _isMustSetMaxSize = EditorGUILayout.ToggleLeft("是否: 同步所有图片的最大尺寸?", _isMustSetMaxSize, s_c);
            EditorGUILayout.Space(10);
            _maxTextureSize = EditorGUILayout.IntField("图片最大尺寸:", _maxTextureSize);
            EditorGUILayout.Space(10);
            _compressionQuality = EditorGUILayout.IntField("图片压缩品质(0-100):", _compressionQuality);
            _compressionQuality = Mathf.Max(_compressionQuality, 0);
            _compressionQuality = Mathf.Min(_compressionQuality, 100);
            EditorGUILayout.Space(10);
            s_c.normal.textColor = Color.yellow;
            _mipmap = EditorGUILayout.IntField("mipmapEnabled :", _mipmap);
            EditorGUILayout.LabelField("(2 = 开启,非0 = 关闭)", s_c);
            EditorGUILayout.Space(10);
            s_c.normal.textColor = Color.green;
            _alphaIs = EditorGUILayout.IntField("alphaIsTransparency:", _alphaIs);
            EditorGUILayout.LabelField("(2 = 开启,非0 = 关闭)", s_c);
            EditorGUILayout.Space(10);
            s_c.normal.textColor = Color.magenta;
            _isASTC = EditorGUILayout.ToggleLeft("是否用ASTC格式?", _isASTC, s_c);
            EditorGUILayout.Space(10);
            s_c.normal.textColor = Color.yellow;
            _sRGBN = EditorGUILayout.IntField("sRGBTexture :", _sRGBN);
            EditorGUILayout.LabelField("(2 = 开启,非0 = 关闭)", s_c);
            EditorGUILayout.Space(10);
            s_c.normal.textColor = Color.cyan;
            _hdUI = EditorGUILayout.IntField("图集类型处理:", _hdUI);
            EditorGUILayout.LabelField("(2 = 绑定ABName,非0 = 处理图片格式)", s_c);
            EditorGUILayout.Space(10);
            if (GUILayout.Button("处理图片压缩格式 Compression"))
            {
                HandlerTex(_path, suffixToLowers, _maxTextureSize, _compressionQuality, _mipmap, _alphaIs);
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


        static void HandlerTex(string fpDir, string suffixToLowers, int _maxTextureSize, int compressionQuality = 60, int mipmap = 0, int alphaIs = 0)
        {
            if (string.IsNullOrEmpty(fpDir))
                fpDir = Application.dataPath;
            var _arrs = Directory.GetFiles(fpDir, "*.*", SearchOption.AllDirectories);
            if(!string.IsNullOrEmpty(suffixToLowers))
                _arrs = _arrs.Where(s => suffixToLowers.Contains(GetSuffixToLower(s))).ToArray();
               //.Where(s => s.ToLower().EndsWith(".prefab")).ToArray();

            _HdAll(_arrs, _maxTextureSize, compressionQuality, mipmap, alphaIs);
            AssetDatabase.Refresh();

            if(EditorUtility.DisplayDialog("图片压缩格 Over", "This is Finished", "Okey"))
                vwWindow.Close();
        }

        static void _HdAll(string[] _arrs, int maxTextureSize, int compressionQuality = 60, int mipmap = 0, int alphaIs = 0)
        {
            float nLens = _arrs.Length;
            int nIndex = 0;
            EditorUtility.DisplayProgressBar(string.Format("开始处理图片压缩格式{0}/{1}", nIndex, nLens), "begin", 0);
            string _pIt = null;
            AssetImporter _aImp = null;
            TextureImporter _tImp = null;
            for (int i = 0; i < nLens; i++)
            {
                nIndex++;
                _pIt = _arrs[i];
                _pIt = _pIt.Substring(_pIt.LastIndexOf("Assets"));
                _pIt = _pIt.Replace('\\', '/');
                EditorUtility.DisplayProgressBar(string.Format("正在处理图片压缩格式{0}/{1}", nIndex, nLens), _pIt, nIndex / nLens);
                _aImp = AssetImporter.GetAtPath(_pIt);
                if (_aImp == null)
                    continue;
                _tImp = _aImp as TextureImporter;
                if (_tImp == null)
                    continue;

                _DoWith(_tImp, maxTextureSize, compressionQuality, mipmap, alphaIs);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        static void _DoWith(TextureImporter importer, int maxTextureSize, int compressionQuality = 60, int mipmap = 0, int alphaIs = 0)
        {
            if (null == importer)
                return;

            // Linear 勾选shader会自动将读到的像素作gramma矫正，即x的0.45次方，不勾选=原始图片色
            // Gramma空间下，勾选与否无关
            // 制作贴图时使用sRGB色域空间，将sRGB勾上
            if (_sRGBN != 0)
                importer.sRGBTexture = (_sRGBN == 2);
            if (mipmap != 0)
                importer.mipmapEnabled = (mipmap == 2);
            if (alphaIs != 0)
                importer.alphaIsTransparency = (alphaIs == 2);
            // importer.textureCompression = TextureImporterCompression.Compressed;
            importer.crunchedCompression = true;
            importer.compressionQuality = compressionQuality;
            importer.npotScale = TextureImporterNPOTScale.None;
            // importer.wrapMode = TextureWrapMode.Clamp;

            // ui图片处理
            importer.spritePackingTag = null;
            if (_hdUI != 0)
                TexType(importer, _hdUI);


            bool _isHasAlpha = importer.DoesSourceTextureHaveAlpha();
            int t_w = 0, t_h = 0;
            (t_w, t_h) = GetTextureImporterSize(importer);
            bool _isP2 = IsPower2(t_w,t_h);
            bool _isD4 = IsDivisible4(t_w,t_h);
            TextureImporterFormat fmtAlpha, fmtNotAlpha, curFmt;

            // platform = "Web", "iPhone", "Android", "WebGL", "Windows Store Apps", "PS4", "XboxOne", "Nintendo Switch" and "tvOS".
            // platform = "DefaultTexturePlatform", "Standalone"

            /*
            // Default
            fmtAlpha = TextureImporterFormat.RGBA32;
            fmtNotAlpha = TextureImporterFormat.RGB16;
            curFmt = _isHasAlpha ? fmtAlpha : fmtNotAlpha;
            _SaveSetting(importer, "Standalone", maxTextureSize, curFmt);
            */

            // iPhone
            // fmtAlpha = _isP2 ? TextureImporterFormat.PVRTC_RGBA4 : (_isASTC ? TextureImporterFormat.ASTC_6x6 : TextureImporterFormat.RGBA32);
            // fmtNotAlpha = _isP2 ? TextureImporterFormat.PVRTC_RGB4 : (_isASTC ? TextureImporterFormat.ASTC_RGB_6x6 : TextureImporterFormat.RGB16);
			fmtAlpha = _isP2 ? TextureImporterFormat.PVRTC_RGBA4 : (_isASTC ? TextureImporterFormat.ASTC_10x10 : TextureImporterFormat.RGBA32);
            fmtNotAlpha = _isP2 ? TextureImporterFormat.PVRTC_RGB4 : (_isASTC ? TextureImporterFormat.ASTC_RGB_10x10 : TextureImporterFormat.RGB16);
            curFmt = _isHasAlpha ? fmtAlpha : fmtNotAlpha;
            bool isChg = _SaveSetting(importer, "iPhone", maxTextureSize, curFmt, compressionQuality);

            // Android
            // 1920x1080 图片 ETC2_RGBA8 (47) = 4mb,ETC2_RGBA8Crunched (65)  = 0.6mb
            // ASTC_4x4 (48 品质最好） 比  5x5 多1百多kb 4x4 约= 2 * ASTC_6x6(50) (ASTC_RGBA_4x4 是 HDR的)
			// 2048x2014 ASTC_4x4 = 3.9mb,ASTC_5x5 = 2.5mb,ASTC_6x6 = 1.8mb,ASTC_8x8 = 1.0mb,ASTC_10x10 = 0.6mb
			// 2046x1772 ASTC_4x4 = 3.5mb,ASTC_5x5 = 2.2mb,ASTC_6x6 = 1.5mb,ASTC_8x8 = 0.9mb,ASTC_10x10 = 0.6mb
            // fmtAlpha = _isD4 ? (_isP2 ? TextureImporterFormat.ETC2_RGBA8 : TextureImporterFormat.ETC2_RGBA8Crunched) : (_isASTC ? TextureImporterFormat.ASTC_6x6 : TextureImporterFormat.RGBA32);
			// fmtNotAlpha = _isD4 ? (_isP2 ? TextureImporterFormat.ETC_RGB4Crunched : TextureImporterFormat.ETC2_RGBA8Crunched) : (_isASTC ? TextureImporterFormat.ASTC_RGB_6x6 : TextureImporterFormat.RGB16);
            fmtAlpha = _isD4 ? TextureImporterFormat.ETC2_RGBA8Crunched : (_isASTC ? TextureImporterFormat.ASTC_10x10 : TextureImporterFormat.RGBA32);
            fmtNotAlpha = _isD4 ? TextureImporterFormat.ETC_RGB4Crunched : (_isASTC ? TextureImporterFormat.ASTC_RGB_10x10 : TextureImporterFormat.RGB16);
            curFmt = _isHasAlpha ? fmtAlpha : fmtNotAlpha;
            isChg = isChg || _SaveSetting(importer, "Android", maxTextureSize, curFmt, compressionQuality);
			
			if (isChg)
			{
				EditorUtility.SetDirty(importer);
				importer.SaveAndReimport();
			}
        }

        static bool _SaveSetting(TextureImporter importer, string platform, int maxTextureSize, TextureImporterFormat fmt,int compressionQuality)
        {
			bool isChg = false;
            TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
            if (settings == null)
                return isChg;

            // "Android"  "iOS"
            bool _isAnd = "Android".Equals(platform);
            TextureImporterCompression _textureCompression = TextureImporterCompression.Compressed;
            switch (fmt)
            {

                case TextureImporterFormat.ETC2_RGBA8:
                case TextureImporterFormat.ETC2_RGBA8Crunched:
                case TextureImporterFormat.ETC_RGB4Crunched:
                    _textureCompression = TextureImporterCompression.CompressedHQ;
                    break;
            }

            if (_isAnd)
            {
				isChg = isChg || settings.androidETC2FallbackOverride != AndroidETC2FallbackOverride.UseBuildSettings;
				settings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
                switch (fmt)
                {
                    case TextureImporterFormat.ETC2_RGB4:
                    case TextureImporterFormat.ETC2_RGB4_PUNCHTHROUGH_ALPHA:
                    case TextureImporterFormat.ETC2_RGBA8:
                    case TextureImporterFormat.ETC2_RGBA8Crunched:
                        // settings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.Quality32Bit;
                        // break;
                    case TextureImporterFormat.ETC_RGB4:
                    case TextureImporterFormat.ETC_RGB4Crunched:
                        // case TextureImporterFormat.ETC_RGB4_3DS: // 弃用
                        // case TextureImporterFormat.ETC_RGBA8_3DS: // 弃用
						isChg = isChg || settings.allowsAlphaSplitting;
                        settings.allowsAlphaSplitting = false;
                        break;
                }
            }
            else
            {
                _textureCompression = TextureImporterCompression.CompressedHQ;
            }
			isChg = isChg || settings.textureCompression != _textureCompression;
			isChg = isChg || settings.compressionQuality != compressionQuality;
			isChg = isChg || !settings.overridden;
            settings.textureCompression = _textureCompression;
            settings.compressionQuality = compressionQuality;
            settings.overridden = true;
            int _max = settings.maxTextureSize;
            if(_isMustSetMaxSize || _max <= maxTextureSize)
                settings.maxTextureSize = maxTextureSize;
            isChg = isChg || settings.maxTextureSize != _max;
			isChg = isChg || settings.format != fmt;
            Debug.LogFormat("==== [{0}] = [{1}] = [{2}]",platform,fmt,importer.assetPath);
            settings.format = fmt;
            importer.SetPlatformTextureSettings(settings);
			return isChg;
        }

        //获取导入图片的宽高
        static (int, int) GetTextureImporterSize(TextureImporter importer)
        {
            if (importer != null)
            {
                object[] args = new object[2];
                System.Reflection.BindingFlags _bflag = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                System.Reflection.MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", _bflag);
                mi.Invoke(importer, args);
                return ((int)args[0], (int)args[1]);
            }
            return (0, 0);
        }

        // 2的整数次幂
        static bool IsPower2(int width,int height)
        {
            return (width == height) && (width > 0) && ((width & (width - 1)) == 0);
        }

        // 被4整除
        static bool IsDivisible4(int width,int height)
        {
            return (width > 0) && (height > 0) && (width % 4 == 0 && height % 4 == 0);
        }


        // 处理图片
        static void TexType(TextureImporter importer, int optType)
        {
#if USE_BPatcher
            string fp = importer.assetPath;
            if (!BuildPatcher.IsInDevelop(fp))
                return;

            if (!BuildPatcher.IsTexture(fp))
                return;

            bool _isUIImag = BuildPatcher.IsUITexture(fp);
            bool _isSpr = importer.textureType == TextureImporterType.Sprite;
            if (_isUIImag)
            {
                if (!_isSpr) importer.textureType = TextureImporterType.Sprite;
            }
            else
            {

                if (_isSpr) importer.textureType = TextureImporterType.Default;
            }

            if (optType == 2)
                BuildPatcher.ReBindAB4SngOrAtlas(importer);
#endif
        }

        static bool IsFirstImport(int width,int height,string assetPath)
        {
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            string _fpMeta = AssetDatabase.GetAssetPathFromTextMetaFilePath(assetPath);
            bool hasMeta = File.Exists(_fpMeta);
            return tex == null || !hasMeta || (tex.width != width && tex.height != height);
        }
    }
}