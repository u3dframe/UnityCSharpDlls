/*
 * 通过降低float精度，去除无用的scale曲线
 * 从而降低动画的存储占用、内存占用和加载时间.
 * 
 * 使用方法
 * 通过菜单Tools/Optimise/ClipOpt打开窗口，
 * 在Assets目录下选择要优化的动画，点击Optimize按钮，等待一段时间即可
 */

using UnityEditor;
using UnityEngine;

/// <summary>
/// 动画优化，存储占用/内存占用/加载时间
/// </summary>
public class ClipOptimize : EditorWindow
{
    private bool m_excludeScale;

    [MenuItem("Tools_Art/Optimise/ClipOpt")]
    [MenuItem("Assets/Tools_Art/Optimise/ClipOpt")]
    protected static void Open()
    {
        GetWindow<ClipOptimize>();
    }

    private Vector2 m_scoll;
    private bool m_ing;
    private int m_index;
    static private string m_fpower = "f3";
    private bool m_isChg;

    public void OnGUI()
    {
        var selects = Selection.objects;

        using (var svs = new EditorGUILayout.ScrollViewScope(m_scoll))
        {
            m_scoll = svs.scrollPosition;
            foreach (var obj in selects)
            {
                var clip = obj as AnimationClip;
                if (clip == null)
                    continue;
                EditorGUILayout.ObjectField(clip, typeof(AnimationClip), false);
            }
        }
		using (new EditorGUILayout.HorizontalScope())
        {
            m_fpower = EditorGUILayout.TextField("Format Float : ",m_fpower);
            if(!m_fpower.StartsWith("f"))
                m_fpower = "f3";
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            m_excludeScale = EditorGUILayout.ToggleLeft("Exclude Scale", m_excludeScale);
            if(!m_fpower.StartsWith("f"))
                m_fpower = "f3";

            if (GUILayout.Button("Optimize"))
            {
                m_ing = true;
            }
        }
        if (m_ing)
        {
            if (m_index >= selects.Length)
            {
                m_ing = false;
                m_index = 0;
                EditorUtility.ClearProgressBar();
                return;
            }
            m_isChg = false;
            var info = string.Format("Process {0}/{1}", m_index, selects.Length);
            EditorUtility.DisplayProgressBar("Optimize Clip", info, (m_index + 1f) / selects.Length);

            var obj = selects[m_index];
            m_index++;
            var clip = obj as AnimationClip;
            if (clip == null)
                return;
            m_isChg = m_isChg || FixFloatAtClip(clip, m_excludeScale);
            if (m_isChg)
            {
                //EditorUtility.SetDirty(clip);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }

    private static bool FixFloatAtClip(AnimationClip clip, bool excludeScale)
    {
        bool _isOkey = false;
        try
        {
            if (excludeScale)
            {
                foreach (var theCurveBinding in AnimationUtility.GetCurveBindings(clip))
                {
                    var name = theCurveBinding.propertyName.ToLower();
                    if (name.Contains("scale"))
                    {
                        AnimationUtility.SetEditorCurve(clip, theCurveBinding, null);
                        _isOkey = true;
                    }
                }
            }

            var curves = AnimationUtility.GetCurveBindings(clip);
            foreach (var curveDate in curves)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, curveDate);
                if (curve == null || curve.keys == null)
                {
                    continue;
                }
                var keyFrames = curve.keys;
                for (var i = 0; i < keyFrames.Length; i++)
                {
                    var key = keyFrames[i];
                    key.value = float.Parse(key.value.ToString(m_fpower));
                    key.inTangent = float.Parse(key.inTangent.ToString(m_fpower));
                    key.outTangent = float.Parse(key.outTangent.ToString(m_fpower));
                    keyFrames[i] = key;
                }
                curve.keys = keyFrames;
                clip.SetCurve(curveDate.path, curveDate.type, curveDate.propertyName, curve);
                _isOkey = true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogErrorFormat("CompressAnimationClip Failed !!! animationPath : {0} error: {1}", clip.name, e);
        }
        return _isOkey;
    }
}