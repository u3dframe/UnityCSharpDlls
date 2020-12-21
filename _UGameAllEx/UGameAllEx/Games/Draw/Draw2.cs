using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Draw2Item
{
    public bool isUpTime;
    public float vUpTime;
    public Shader shader;
    public GameObject go;
    public MeshFilter mf;
    public MeshRenderer mr;
}

public class Draw2 : MonoBehaviour
{
    Vector3 BasePosition = Vector3.zero;
    List<Draw2Item> lst = new List<Draw2Item>();
    public void SetBasePos(Vector3  pos)
    {
        var s = Mathf.Pow(1, 3);
        BasePosition = pos + new Vector3(-s, 0, s);
    }

    private Draw2Item GetNewItem()
    {
        for (int i = 0; i < lst.Count; i++)
        {
            if (lst[i].isUpTime && lst[i].vUpTime < 0)
            {
                lst[i].isUpTime = false;
                return lst[i];
            }
        }
        Draw2Item nItem = new Draw2Item();
        nItem.go = new GameObject("DrawMesh");
        nItem.mf = nItem.go.AddComponent<MeshFilter>();
        nItem.mr = nItem.go.AddComponent<MeshRenderer>();
        nItem.go.layer = LayerMask.NameToLayer("SceneObj");
        nItem.go.transform.position = new Vector3(0, 0.15F, 0);
        nItem.shader = Shader.Find("S_E/EffectCombine(Blend)");
        lst.Add(nItem);
        return nItem;
    }
    private void CreateMesh(List<Vector3> vertices, float sTime)
    {
        int[] triangles;
        int triangleAmount = vertices.Count - 2;
        triangles = new int[3 * triangleAmount];
        //根据三角形的个数，来计算绘制三角形的顶点顺序（索引）    
        //顺序必须为顺时针或者逆时针    
        for (int i = 0; i < triangleAmount; i++)
        {
            triangles[3 * i] = 0;//固定第一个点    
            triangles[3 * i + 1] = i + 1;
            triangles[3 * i + 2] = i + 2;
        }
        Mesh mesh = new Mesh(); 
        Draw2Item item = GetNewItem();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;
        item.mf.mesh = mesh;
        item.mr.material.shader = item.shader;
        item.mr.material.renderQueue = 3000;
        item.mr.material.color = new Color(1, 0, 0, 0.25F);
        item.vUpTime = sTime;
        item.isUpTime = sTime > 0;
        item.go.SetActive(true);
    }
    
    //扇园形
    public void DrawSectorSolid(float dTime, Vector3 forward, Vector3 center, float angle, float radius)
    {
        int pointAmount = 100;//点的数目，值越大曲线越平滑  
        float eachAngle = angle / pointAmount;
        List<Vector3> vertices = new List<Vector3>();
        if (angle < 360)
        {
            vertices.Add(center + BasePosition);
            for (int i = 1; i < pointAmount - 1; i++)
            {
                vertices.Add(Quaternion.Euler(0f, -angle / 2 + eachAngle * (i - 1), 0f) * forward * radius + center + BasePosition);
            }
        }
        else
        {
            for (int i = 0; i <= pointAmount; i++)
            {
                vertices.Add(Quaternion.Euler(0f, eachAngle * i, 0f) * forward * radius + center + BasePosition);
            }
        }
        CreateMesh(vertices, dTime);
    }
    //多边形
    public void DrawQuadrilateralSolid(float dTime, List<Vector3> vertices)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] += BasePosition;
        }
        CreateMesh(vertices, dTime);
    }

    void Update()
    {
        for (int i = 0; i < lst.Count; i++)
        {
            if (lst[i].isUpTime)
            {
                lst[i].vUpTime -= Time.deltaTime;
                if (lst[i].vUpTime < 0)
                {
                    lst[i].go.SetActive(false);
                }
            }
        }
    }
}
