using UnityEngine;
using System.IO;

namespace Core.Kernel
{
    using TMPro; // Unity.TextMeshPro
    using UObject = UnityEngine.Object;
    using UResources = UnityEngine.Resources;

    /// <summary>
    /// 类名 : 读取 Resources 文件夹下面的资源
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2017-03-07 09:29
    /// 功能 : partial
    /// </summary>
    public class UGameRes : ReadWriteHelper
    {
        static public readonly System.Type tpGobj = typeof(GameObject);
        static public readonly System.Type tpTex2D = typeof(Texture2D);
        static public readonly System.Type tpSprite = typeof(Sprite);
        static public readonly System.Type tpCube = typeof(Cubemap);
        static public readonly System.Type tpFont = typeof(Font);
        static public readonly System.Type tpShader = typeof(Shader);
        static public readonly System.Type tpMat = typeof(Material);
        static public readonly System.Type tpAdoClip = typeof(AudioClip);
        static public readonly System.Type tpAnmClip = typeof(AnimationClip);
        static public readonly System.Type tpSVC = typeof(ShaderVariantCollection);
        static public readonly System.Type tpTimeline = typeof(UnityEngine.Timeline.TimelineAsset);
        static public readonly System.Type tpSctObj = typeof(ScriptableObject);
        static public readonly System.Type tpVdoClip = typeof(UnityEngine.Video.VideoClip);
        static public readonly System.Type tpFontTMP = typeof(TMP_FontAsset);

        static public readonly string m_strFnt = ".ab_fnt";
        static public readonly string m_strFntTMP = ".ab_fnttmp";
        static public readonly string m_strShader = ".ab_shader";
        static public readonly string m_strUI = ".ui";
        static public readonly string m_strFab = ".fab";
        static public readonly string m_strAtlas = ".tex_atlas";
        static public readonly string m_strTex2D = ".tex";
        static public readonly string m_strCube = ".tex_cub";
        static public readonly string m_strSVC = ".ab_svc";
        static public readonly string m_strTLine = ".pa";
        static public readonly string m_strFbx = ".ab_fbx";
        static public readonly string m_strAdoClip = ".ado";
        static public readonly string m_strAnmClip = ".ab_anm";
        static public readonly string m_strMat = ".ab_mat";
        static public readonly string m_strLightmap = ".ab_lms";
        static public readonly string m_strScriptable = ".ab_sct";
        static public readonly string m_strVdoClip = ".vdo";
        static public readonly string m_strFdAnmt = ".ab_anmt"; // animator,animation的文件夹
        static public readonly string m_strFdDef = ".ab_fd"; // 默认文件夹后缀名

        static public readonly string m_suffix_png = ".png";
        static public readonly string m_suffix_fab = ".prefab";
        static public readonly string m_suffix_light = ".exr";
        static public readonly string m_suffix_mat = ".mat";
        static public readonly string m_suffix_shader = ".shader";
        static public readonly string m_suffix_scriptable = ".asset";
        static public readonly string m_suffix_svc = ".shadervariants";

        /// <summary>
        /// 路径转为以 Assets/ 开头的
        /// </summary>
        static protected string _AssetsStart(string fp)
        {
            // 去掉第一个Assets文件夹路径
            int index = fp.IndexOf(m_assets);
            if (index >= 0)
            {
                fp = fp.Substring(index + m_nAssests);
            }
            fp = "Assets/" + fp;
            return fp;
        }

        static public T LoadInResources<T>(string path) where T : UObject
        {
            // 去掉最后一个Resources文件夹路径
            int index = path.LastIndexOf(m_fnResources);
            if (index >= 0)
            {
                path = path.Substring(index + m_nResources);
            }

            // 去掉后缀名
            string suffix = Path.GetExtension(path);
            if (!string.IsNullOrEmpty(suffix))
            {
                path = path.Replace(suffix, "");
            }

            return UResources.Load(path,typeof(T)) as T;
        }

        /// <summary>
        /// Loads the in resources.
        /// </summary>
        /// <returns>The in resources.</returns>
        /// <param name="path">路径</param>
        static public UObject LoadInResources(string path)
        {
            // 去掉最后一个Resources文件夹路径
            int index = path.LastIndexOf(m_fnResources);
            if (index >= 0)
            {
                path = path.Substring(index + m_nResources);
            }

            // 去掉后缀名
            string suffix = Path.GetExtension(path);
            if (!string.IsNullOrEmpty(suffix))
            {
                path = path.Replace(suffix, "");
            }

            return UResources.Load(path);
        }

        static public void UnLoadOne(UObject obj,bool isDestroy)
		{
			if(obj == null || !obj)
				return;
			
			if(isDestroy || obj.GetType() == tpGobj)
			{
				GameObject.DestroyImmediate(obj); // Destroy
			}
			else
			{
				UResources.UnloadAsset(obj);
			}
		}

        static public void UnLoadOne(UObject obj)
        {
            UnLoadOne(obj,false);
        }

        static public bool IsAB(string abName,string defEnd)
        {
            if (string.IsNullOrEmpty(abName) || string.IsNullOrEmpty(defEnd))
                return false;
            return abName.EndsWith(defEnd);
        }

        static public bool IsSVCAB(string abName)
        {
            return IsAB(abName, m_strSVC);            
        }

        static public bool IsShaderAB(string abName){
            return IsAB(abName, m_strShader);
        }

        static public bool IsMatAB(string abName){
            return IsAB(abName, m_strMat);
        }

        static public bool IsTex2dAB(string abName){
            if (string.IsNullOrEmpty(abName))
                return false;
            return abName.EndsWith(m_strTex2D) || abName.EndsWith(m_strCube);
        }

        static public bool IsAudioClipAB(string abName)
        {
            return IsAB(abName, m_strAdoClip);
        }

        static public bool IsScriptableAB(string abName)
        {
            return IsAB(abName, m_strScriptable);
        }

        static public bool IsVdoClipAB(string abName)
        {
            return IsAB(abName, m_strVdoClip);
        }
    }
}