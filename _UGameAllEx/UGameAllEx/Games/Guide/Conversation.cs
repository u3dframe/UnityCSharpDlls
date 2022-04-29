using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Conversation : MonoBehaviour
{
    public Camera cam;
    public System.Action<string> evt;
    public RectTransform RectTras;
    public ConversationPText PText_L;
    public ConversationPText PText_R;
    public void SetPTextL(string str)
    {
        PText_L.SetText(str);
        LayoutRebuilder.ForceRebuildLayoutImmediate(RectTras);
    }

    public void SetPTextR(string str)
    {
        PText_R.SetText(str);
        LayoutRebuilder.ForceRebuildLayoutImmediate(RectTras);
    }

    public void SetCamAndEvent(Camera cm, System.Action<string> bk)
    {
        cam = cm; evt = bk;
    }
}
