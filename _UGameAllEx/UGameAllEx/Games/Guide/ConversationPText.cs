using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ConversationPText : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    public int maxPosx = 100;
    public Conversation conv;
    public TextMeshProUGUI text;
    public RectTransform txtTras;
    public RectTransform imgTras;
    public RectTransform objTras;
    public ContentSizeFitter csf;
    Vector2 DefTextSD = new Vector2(750, 30);
    public void SetText(string str)
    {
        if (text && csf)
        {
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            txtTras.sizeDelta = DefTextSD;
            text.enableWordWrapping = false;
            text.text = str;
            text.ForceMeshUpdate();
            if (text.renderedWidth > 750)
            {
                text.enableWordWrapping = true;
                text.ForceMeshUpdate();
            }
            txtTras.sizeDelta = new Vector2(Mathf.Ceil(text.renderedWidth), txtTras.sizeDelta.y);
            RefText();
            if (imgTras && imgTras.sizeDelta.x > maxPosx)
            {
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; 
                RefText();
                imgTras.sizeDelta = new Vector2(maxPosx, imgTras.sizeDelta.y);
            }
        }
    }

    public void RefText()
    {
        if (csf)
        {
            csf.SetLayoutHorizontal();
            LayoutRebuilder.ForceRebuildLayoutImmediate(imgTras);
            LayoutRebuilder.ForceRebuildLayoutImmediate(objTras);
        }    
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (conv && conv.cam)
        {
            Vector3 pos = new Vector3(eventData.position.x, eventData.position.y, 0);
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, pos, conv.cam);
            if (linkIndex > -1)
            {
                TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
                conv.evt?.Invoke(linkInfo.GetLinkID());
                //Application.OpenURL(linkInfo.GetLinkID());
                //Debug.LogError(linkInfo.GetLinkID());

            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {

    }
}
