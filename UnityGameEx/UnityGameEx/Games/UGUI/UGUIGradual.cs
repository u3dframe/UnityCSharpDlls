using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("UI/Effects/Gradual")]
public class UGUIGradual : BaseMeshEffect
{
    public Color32 topColor = Color.white;
    public Color32 bottomColor = Color.grey;

    public bool m_isMultiply = false;

    public static Color32 Multiply(Color32 a, Color32 b)
    {
        a.r = (byte)((a.r * b.r) >> 8);
        a.g = (byte)((a.g * b.g) >> 8);
        a.b = (byte)((a.b * b.b) >> 8);
        a.a = (byte)((a.a * b.a) >> 8);
        return a;
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount <= 0)
        {
            return;
        }

        List<UIVertex> vertices = new List<UIVertex>();
        vh.GetUIVertexStream(vertices);

        float topY = vertices[0].position.y;
        float bottomY = vertices[0].position.y;
        float y = 0;

        int count = vertices.Count;
        for (int i = 1; i < count; i++)
        {
            y = vertices[i].position.y;
            if (y > topY)
            {
                topY = y;
            }
            else if (y < bottomY)
            {
                bottomY = y;
            }
        }
        vertices.Clear();

        float height = topY - bottomY;
        UIVertex v = new UIVertex();
        count = vh.currentVertCount;
        Color32 a;
        for (int i = 0; i < count; ++i)
        {
            vh.PopulateUIVertex(ref v, i);
            a = Color32.Lerp(bottomColor, topColor, (v.position.y - bottomY) / height);
            if (this.m_isMultiply)
                v.color = Multiply(v.color, a);
            else
                v.color = a;

            vh.SetUIVertex(v, i);
        }
    }
}
