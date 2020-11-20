using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// 类名 : E-Editor,G-Gui 绘制帮助Class
/// 作者 : Canyon
/// 日期 : 2017-03-07 09:29
/// 功能 : 
/// F - Func,G-Guilayout,V - Vertical,H-Horizontal,E - Editor
/// 外观 - EditorStyles.textArea 感觉还比较不错
/// </summary>
public static class EG_Helper {
    public const int h30 = 30;
    public const int h28 = 28;
    public const int h26 = 26;
    public const int h24 = 24;
    public const int h22 = 22;
    public const int h20 = 20;
    public const int h18 = 18;

    static List<GUILayoutOption> list = new List<GUILayoutOption>();

    static public GUILayoutOption[] ToOptions(float minW = 0, float minH = 10,bool isMinW = true,bool isMinH = true)
    {
        list.Clear();
        if (minH > 0)
        {
            if(isMinH)
                list.Add(GUILayout.MinHeight(minH));
            else
                list.Add(GUILayout.MaxHeight(minH));
        }

        if (minW > 0)
        {
            if(isMinW)
                list.Add(GUILayout.MinWidth(minW));
            else
                list.Add(GUILayout.MaxWidth(minW));
        }
        return list.ToArray();
    }

    static public GUILayoutOption[] ToOptionW(float minW = 0,bool isMinW = false) {
        return ToOptions(minW,0,isMinW);
    }

    static public GUILayoutOption[] ToOptionH(float minH = 0,bool isMinH = false) {
        return ToOptions(0,minH,false,isMinH);
    }

    #region == GUILayout Func ==

    static public void FG_BeginVAsArea(float minW = 0, float minH = 10)
    {
        GUILayoutOption[] arrs = ToOptions(minW, minH);
        GUILayout.BeginVertical("As TextArea", arrs);
        FG_Space(2);
    }

    static public void FG_BeginVArea(float minW = 0, float minH = 10)
    {
        GUILayoutOption[] arrs = ToOptions(minW, minH);
        GUILayout.BeginVertical(EditorStyles.textArea,arrs);
        FG_Space(2);
    }

    static public void FG_BeginV(float minW = 0, float minH = 10)
    {
        GUILayoutOption[] arrs = ToOptions(minW, minH);
        GUILayout.BeginVertical(arrs);
        FG_Space(2);
    }

    static public void FG_EndV()
    {
        FG_Space(3);
        GUILayout.EndVertical();
        FG_Space(5);
    }

    static public void FG_BeginHAsArea(float minW = 0, float minH = 10)
    {
        GUILayoutOption[] arrs = ToOptions(minW, minH);
        GUILayout.BeginHorizontal("As TextArea", arrs);
        FG_Space(2);
    }

    static public void FG_BeginHArea(float minW = 0, float minH = 10)
    {
        GUILayoutOption[] arrs = ToOptions(minW, minH);
        GUILayout.BeginHorizontal(EditorStyles.textArea, arrs);
        FG_Space(2);
    }

    static public void FG_BeginH(float minW = 0, float minH = 10)
    {
        GUILayoutOption[] arrs = ToOptions(minW, minH);
        GUILayout.BeginHorizontal(arrs);
        FG_Space(2);
    }

    static public void FG_EndH()
    {
        FG_Space(3);
        GUILayout.EndHorizontal();
        FG_Space(5);
    }

    static public void FG_Label(object obj)
    {
        GUILayout.Label(obj.ToString());
    }

    static public void FG_Space(float space)
    {
        GUILayout.Space(space);
    }

    #endregion

    #region == EditorGUILayout Func ==

    static public void FEG_BeginVAsArea(float minW = 0,float minH = 10)
    {

        GUILayoutOption[] arrs = ToOptions(minW, minH);
        EditorGUILayout.BeginVertical("As TextArea",arrs);
        FG_Space(2);
    }

    static public void FEG_BeginVArea(float minW = 0, float minH = 10)
    {
        GUILayoutOption[] arrs = ToOptions(minW, minH);
        EditorGUILayout.BeginVertical(EditorStyles.textArea, arrs);
        FG_Space(2);
    }

    static public void FEG_BeginV(float minW = 0, float minH = 10)
    {
        GUILayoutOption[] arrs = ToOptions(minW, minH);
        EditorGUILayout.BeginVertical(arrs);
        FG_Space(2);
    }

    static public void FEG_EndV()
    {
        FG_Space(3);
        EditorGUILayout.EndVertical();
        FG_Space(5);
    }

    // Unable to find style 'As TextArea' in skin 'DarkSkin' repaint
    static public void FEG_BeginHAsArea(float minW = 0, float minH = 10)
    {
        GUILayoutOption[] arrs = ToOptions(minW, minH);
        EditorGUILayout.BeginHorizontal("TextArea", arrs); // 
        FG_Space(2);
    }

    static public void FEG_BeginHArea(float minW = 0, float minH = 10)
    {
        GUILayoutOption[] arrs = ToOptions(minW, minH);
        EditorGUILayout.BeginHorizontal(EditorStyles.textArea, arrs);
        FG_Space(2);
    }

    static public void FEG_BeginH(float minW = 0, float minH = 10)
    {
        GUILayoutOption[] arrs = ToOptions(minW, minH);
        EditorGUILayout.BeginHorizontal(arrs);
        FG_Space(2);
    }

    static public void FEG_EndH()
    {
        FG_Space(3);
        EditorGUILayout.EndHorizontal();
        FG_Space(5);
    }

    static public void FEG_BeginToggleGroup(string title,ref bool toggle)
    {
        toggle = EditorGUILayout.BeginToggleGroup(title, toggle);
        FG_Space(2);
    }

    static public void FEG_EndToggleGroup()
    {
        FG_Space(3);
        EditorGUILayout.EndToggleGroup();
        FG_Space(5);
    }

    static public void FEG_Toggle(string title,ref bool toggle,GUIStyle style, bool isLeft = true)
    {
        if(isLeft)
            toggle = EditorGUILayout.ToggleLeft(title, toggle,style);
        else
            toggle = EditorGUILayout.Toggle(title, toggle,style);
        FG_Space(5);
    }

    static public void FEG_Toggle(string title,ref bool toggle, bool isLeft = true)
    {
        GUIStyle _st = new GUIStyle();
        FEG_Toggle(title,ref toggle,_st,isLeft);
    }

    static public void FEG_ToggleRed(string title,ref bool toggle, bool isLeft = true)
    {
        GUIStyle _st = new GUIStyle();
        _st.normal.textColor = Color.red; // yellow
        FEG_Toggle(title,ref toggle,_st,isLeft);
    }

    static public void FEG_BeginScroll(ref Vector2 scrollPos,float minH = 70,float minW = 0,int hvScrollType = 0)
    {
        GUILayoutOption[] arrs = ToOptions(minW, minH);
        bool isHScroll = hvScrollType != 2 && hvScrollType != 0;
        bool isVScroll = hvScrollType != 1 && hvScrollType != 0;
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, isHScroll, isVScroll,arrs);
        FG_Space(2);
    }

    static public void FEG_EndScroll()
    {
        FG_Space(3);
        EditorGUILayout.EndScrollView();
        FG_Space(5);
    }

    // val = [0~1]
    static public bool FEG_BeginFadeGroup(float val,float minW = 0, float minH = 30)
    {
        bool ret = false;
        FEG_BeginV(minW, minH);
        ret = EditorGUILayout.BeginFadeGroup(val);
        FG_Space(2);
        return ret;
    }

    static public void FEG_EndFadeGroup()
    {
        EditorGUILayout.EndFadeGroup();
        FEG_EndV();
    }

    #endregion

	#region == Head Func ==

	static public void FEG_HeadTitMid(string title,bool isAdd = false,System.Action callFunc = null){
		Color bgColor = Color.black;
		FEG_HeadTitMid (title, bgColor,isAdd, callFunc);
	}

	static public void FEG_HeadTitMid(string title,Color bgColor,bool isAdd = false,System.Action callFunc = null){
		GUIStyle titStyle = EditorStyles.textArea;
		titStyle.alignment = TextAnchor.MiddleCenter;
		FEG_Head (title, bgColor,titStyle, isAdd, callFunc);
	}

	static public void FEG_Head(string title,bool isAdd = false,System.Action callFunc = null){
		Color bgColor = Color.black;
		FEG_Head (title, bgColor,isAdd, callFunc);
	}

	static public void FEG_Head(string title,Color bgColor,bool isAdd = false,System.Action callFunc = null){
		GUIStyle titStyle = EditorStyles.textArea;
		FEG_Head (title, bgColor,titStyle, isAdd, callFunc);
	}

	static public void FEG_Head(string title,Color bgColor,GUIStyle titStyle,bool isAdd = false,System.Action callFunc = null){
		Color titColor = Color.white;
		FEG_Head (title, bgColor, titColor,titStyle, isAdd, callFunc);
	}
		
	static public void FEG_Head(string title,Color bgColor,Color titColor,GUIStyle titStyle,bool isAdd = false,System.Action callFunc = null){
		Color def = GUI.backgroundColor;
		Color defGui = GUI.color;
        Color defSt = titStyle.normal.textColor;

		FEG_BeginH ();
		GUI.backgroundColor = bgColor;
		GUI.color = titColor;
        titStyle.normal.textColor = titColor;
		EditorGUILayout.LabelField(title, titStyle,ToOptions(0,20));

		GUI.backgroundColor = def;
		titStyle.alignment = TextAnchor.MiddleLeft;
        titStyle.normal.textColor = defSt;

		if (isAdd) {
			GUI.color = Color.green;
			if (GUILayout.Button ("+", GUILayout.Width (50))) {
				if (callFunc != null) {
					callFunc ();
				}
			}
		}
		FEG_EndH();
		GUI.color = defGui;
	}

	#endregion

    /// <summary>
	/// 创建当前行对象的位置
	/// </summary>
	static public Rect CreateRect(ref int nX,int nY,int nWidth,int nHeight = 20){
		Rect rect = new Rect (nX, nY, nWidth, nHeight);
		nX += nWidth + 5;
		return rect;
	}

	/// <summary>
	/// 设置下一行的开始位置
	/// </summary>
	static public void NextLine(ref int nX,ref int nY,int addHeight = 30,int resetX = 10){
		nX = resetX;
		nY += addHeight;
	}
}
