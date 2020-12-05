using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 类名 : Text文字渐变
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-08-04 00:10
/// 功能 : 
/// </summary>
[AddComponentMenu("UI/Effects/Gradual3")]
public class UGUIGradual3 : UGUIGradual
{
    public Color32 centerColor = new Color32(161, 179, 194, 255);

    private UIVertex MultiplyColor(UIVertex vertex, Color32 color)
    {
        if (m_isMultiply)
        {
            vertex.color = Multiply(vertex.color, color);
        }
        else
        {
            vertex.color = color;
        }

        return vertex;
    }

    private UIVertex CalcCenterVertex(UIVertex top, UIVertex bottom)
    {
        UIVertex center = new UIVertex();
        center.normal = (top.normal + bottom.normal) * 0.5f;
        center.position = (top.position + bottom.position) * 0.5f;
        center.tangent = (top.tangent + bottom.tangent) * 0.5f;
        center.uv0 = (top.uv0 + bottom.uv0) * 0.5f;
        center.uv1 = (top.uv1 + bottom.uv1) * 0.5f;

        if (m_isMultiply)
        {
            var color = Color32.Lerp(top.color, bottom.color, 0.5f);
            center.color = Multiply(color, centerColor);
        }
        else
        {
            center.color = centerColor;
        }

        return center;
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        // 将三角形默认的【tl->tr->br, br->bl->tl】改为【tl->tr->cr, cr->cl->tl, cl->cr->br, br->bl->cl】
        if (!IsActive() || vh.currentVertCount <= 0)
            return;

        List<UIVertex> verts = new List<UIVertex>(vh.currentVertCount);
        vh.GetUIVertexStream(verts);
        vh.Clear();

        UIVertex tl, tr, bl, br, cl, cr;
        for (int i = 0; i < verts.Count; i += 6)
        {
            // 6 point
            tl = MultiplyColor(verts[i], topColor);
            tr = MultiplyColor(verts[i + 1], topColor);
            bl = MultiplyColor(verts[i + 4], bottomColor);
            br = MultiplyColor(verts[i + 2], bottomColor);
            cl = CalcCenterVertex(verts[i], verts[i + 4]);
            cr = CalcCenterVertex(verts[i + 1], verts[i + 2]);

            // 上半
            vh.AddVert(tl);
            vh.AddVert(tr);
            vh.AddVert(cr);
            vh.AddVert(cr);
            vh.AddVert(cl);
            vh.AddVert(tl);

            // 下半
            vh.AddVert(cl);
            vh.AddVert(cr);
            vh.AddVert(br);
            vh.AddVert(br);
            vh.AddVert(bl);
            vh.AddVert(cl);
        }

        for (int i = 0; i < vh.currentVertCount; i += 12)
        {
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i + 3, i + 4, i + 5);
            vh.AddTriangle(i + 6, i + 7, i + 8);
            vh.AddTriangle(i + 9, i + 10, i + 11);
        }
    }
}
