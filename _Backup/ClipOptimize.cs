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

    [MenuItem("Tools/Optimise/ClipOpt")]
    [MenuItem("Assets/Optimise/ClipOpt")]
    protected static void Open()
    {
        GetWindow<ClipOptimize>();
    }

    private Vector2 m_scoll;
    private bool m_ing;
    private int m_index;

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
            m_excludeScale = EditorGUILayout.ToggleLeft("Exclude Scale", m_excludeScale);

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
            var info = string.Format("Process {0}/{1}", m_index, selects.Length);
            EditorUtility.DisplayProgressBar("Optimize Clip", info, (m_index + 1f) / selects.Length);

            var obj = selects[m_index];
            m_index++;
            var clip = obj as AnimationClip;
            if (clip == null)
                return;
            FixFloatAtClip(clip, m_excludeScale);
        }
    }

    private static void FixFloatAtClip(AnimationClip clip, bool excludeScale)
    {
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
                    key.value = float.Parse(key.value.ToString("f3"));
                    key.inTangent = float.Parse(key.inTangent.ToString("f3"));
                    key.outTangent = float.Parse(key.outTangent.ToString("f3"));
                    keyFrames[i] = key;
                }
                curve.keys = keyFrames;
                clip.SetCurve(curveDate.path, curveDate.type, curveDate.propertyName, curve);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(string.Format("CompressAnimationClip Failed !!! animationPath : {0} error: {1}", clip.name, e));
        }
    }
}