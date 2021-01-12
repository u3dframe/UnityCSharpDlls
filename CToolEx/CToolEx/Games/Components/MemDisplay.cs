using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// 类名 : 内存显示
/// 作者 : Canyon/龚阳辉
/// 日期 : 2021-01-12 14:17
/// 功能 : Profiling , GC , 外加外部语言自身的内存获取
/// 来源 : https://blog.csdn.net/qqo_aa/article/details/78489166
/// </summary>
public class MemDisplay : MonoBehaviour
{
    [SerializeField][Range(0.02f,0.05f)] float hsRate = 0.023f;
    [SerializeField] Color tCol = new Color(1,0,100/255f,1);
	GUIStyle style = new GUIStyle();

    public long m_outMemAll = 0;
    public long m_outMemFree = 0;
    public double m_kb = 1024d;

    void OnGUI()
	{
		int w = Screen.width, h = Screen.height;
        int _h = (int)(h * hsRate) + 1;
        Rect rect = new Rect(0, 0, w, _h);
		style.alignment = TextAnchor.UpperRight;
		style.fontSize = _h;
		style.normal.textColor = tCol;

        long monoAll = Profiler.GetMonoHeapSizeLong();
        long monoUsed = Profiler.GetMonoUsedSizeLong();
        long usedHeap = Profiler.usedHeapSizeLong;
        long totalMem = System.GC.GetTotalMemory(false);


        string text = string.Format("MonoAll {0:0.0} k , MonoUsed {1:0.0} k , usedHeap {2:0.0} k , Total Memory {3:0.0} k , OT_Memory {4:0.0} k , OFree_Memory {5:0.0} k"
            , (monoAll / m_kb), (monoUsed / m_kb), (usedHeap / m_kb), (totalMem / m_kb)
            , (m_outMemAll / m_kb), (m_outMemFree / m_kb));
		GUI.Label(rect, text, style);
	}
}