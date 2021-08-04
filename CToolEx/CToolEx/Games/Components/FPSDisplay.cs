using UnityEngine;
using System.Collections;

/// <summary>
/// 类名 : 帧率Fps
/// 作者 : 未知
/// 日期 : 2020-12-30 10:17
/// 功能 : 
/// 来源 : https://blog.csdn.net/qq_40985921/article/details/86511337
///        https://catlikecoding.com/unity/tutorials/basics/measuring-performance/
/// </summary>
public class FPSDisplay : MonoBehaviour
{
    float deltaTime = 0.0f;
    [SerializeField, Range(0.02f, 0.05f)] float hsRate = 0.023f;
    [SerializeField] Color tCol = new Color(1, 0, 100 / 255f, 1);
    GUIStyle style = new GUIStyle();

    int frames;
    float duration, bestDuration = float.MaxValue, worstDuration;
    [SerializeField, Range(0.1f, 2f)] float sampleDuration = 1f;
    float _c_f, _c_d, _c_b, _c_w;
    void Update()
    {
        float frameDuration = Time.unscaledDeltaTime;
        deltaTime += (frameDuration - deltaTime) * 0.1f;

        frames += 1;
        duration += frameDuration;
        if (frameDuration < bestDuration)
            bestDuration = frameDuration;

        if (frameDuration > worstDuration)
            worstDuration = frameDuration;

        _c_f = frames;
        _c_d = duration;
        _c_b = bestDuration;
        _c_w = worstDuration;

        if (duration >= sampleDuration)
        {
            frames = 0;
            duration = 0f;
            worstDuration = 0f;
            bestDuration = float.MaxValue;
        }
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        int _h = (int)(h * hsRate) + 1;
        int _cw = w / 4;
        Rect rect = new Rect(0, 0, _cw, _h);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = _h;
        style.normal.textColor = tCol;
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms  ,  ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);

        msec = 1f / _c_b;
        fps = _c_f / _c_d;
        float worst = 1f / _c_w;
        text = string.Format("best({3:0.0} ms,{0:0.} fps)  avg({4:0.0} ms,{1:0.} fps)  worst({5:0.0} ms,{2:0.} fps)", msec, fps, worst, 1000f * _c_b, 1000f * _c_d / _c_f, 1000f * _c_w);
        rect = new Rect(_cw, 0, _cw * 3, _h);
        GUI.Label(rect, text, style);
    }
}