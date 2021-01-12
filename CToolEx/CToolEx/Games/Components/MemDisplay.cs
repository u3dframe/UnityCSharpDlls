using UnityEngine;
using UnityEngine.Profiling;
using MemBType = Core.MemBType;

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
    public long m_luaUseMem = 0;
    public MemBType m_eBType = MemBType.MB;
    public double m_def_kb = 1024;
    private double m_kb = 1d;
    private string m_key_byte = "b";

    void OnGUI()
	{
		int w = Screen.width, h = Screen.height;
        int _h = (int)(h * hsRate) + 1;
        Rect rect = new Rect(0,30, w, _h);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = _h;
		style.normal.textColor = tCol;

        long monoAll = Profiler.GetMonoHeapSizeLong();
        long monoUsed = Profiler.GetMonoUsedSizeLong();
        long usedHeap = Profiler.usedHeapSizeLong;
        long totalMem = System.GC.GetTotalMemory(false);

        switch (m_eBType)
        {
            case MemBType.KB:
                m_kb = m_def_kb;
                m_key_byte = "kb";
                break;
            case MemBType.MB:
                m_kb = m_def_kb * m_def_kb;
                m_key_byte = "mb";
                break;
            case MemBType.GB:
                m_kb = m_def_kb * m_def_kb * m_def_kb;
                m_key_byte = "gb";
                break;
            default:
                m_kb = 1d;
                m_key_byte = "b";
                break;
        }

        string text = string.Format("M_All {1:0.0} {0} ,M_Used {2:0.0} {0} ,usedHeap {3:0.0} {0} ,GC_Total {4:0.0} {0} ,O_T {5:0.0} {0} ,O_Free {6:0.0} {0} ,LuaMem {7:0.0} {0}"
            , m_key_byte
            , (monoAll / m_kb), (monoUsed / m_kb), (usedHeap / m_kb), (totalMem / m_kb)
            , (m_outMemAll / m_kb), (m_outMemFree / m_kb), (m_luaUseMem / m_kb));
		GUI.Label(rect, text, style);
	}
}