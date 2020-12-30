using UnityEngine;
using System.Collections;

/// <summary>
/// 类名 : 帧率Fps
/// 作者 : 未知
/// 日期 : 2020-12-30 10:17
/// 功能 : 
/// 来源 : https://blog.csdn.net/qq_40985921/article/details/86511337
/// </summary>
public class FPSDisplay : MonoBehaviour
{
	float deltaTime = 0.0f;
    [SerializeField][Range(0.01f,0.2f)] float hsRate = 0.03f;
    [SerializeField] Color tCol = new Color(1,0,100/255f,1);
	GUIStyle style = new GUIStyle();
	void Update()
	{
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
	}

	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;
        int _h = (int)(h * hsRate) + 1;
        Rect rect = new Rect(0, 0, w, _h);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = _h;
		style.normal.textColor = tCol;
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms  ,  ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);
	}
}