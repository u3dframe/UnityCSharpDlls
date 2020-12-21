using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CustomArrays
{
    public float[] Array;
    public float this[int index]
    {
        get
        {
            return Array[index];
        }
    }
    public CustomArrays()
    {
        this.Array = new float[4];
    }
    public CustomArrays(int index)
    {
        this.Array = new float[index];
    }
}


public class GboxExport : MonoBehaviour
{
#if UNITY_EDITOR
    public GameObject firstObj;
    //public float rowSpacing = 1.3526F;
    public string exPath = @"D:\\scr";


    class derive
    {
        public float max;
        public float min;
        public List<GboxChild> obj = new List<GboxChild>();
    }
    [ContextMenu("Re-InitGbox")]
    void sdaeq()
    {
        TryProminentlyNexts(new System.Collections.Generic.List<int>());
        TryClearUnrealChildObj();
        float x = 0;
        List<derive> o = new List<derive>();
        List<GameObject> lis = new List<GameObject>();
        foreach (Transform item in transform)
        {
            if (item.name != gameObject.name)
            {
                bool ishas = false;
                x = item.position.x;
                for (int i = 0; i < o.Count; i++)
                {
                    if (o[i].max >= x && o[i].min <= x)
                    {
                        ishas = true;
                        o[i].obj.Add(item.gameObject.GetComponent<GboxChild>());
                        continue;
                    }
                }
                if (ishas != true)
                {
                    derive m = new derive
                    {
                        min = x - 0.2F,
                        max = x + 0.2F
                    };
                    m.obj.Add(item.gameObject.GetComponent<GboxChild>());
                    o.Add(m);
                }
            }
        }
        o.Sort((m, b) => m.min.CompareTo(b.min));
        for (int i = 0; i < o.Count; i++)
        {
            o[i].obj.Sort((l, p) => -(l.transform.position.z.CompareTo(p.transform.position.z)));
            for (int q = 0; q < o[i].obj.Count; q++)
            {
                lis.Add(o[i].obj[q].gameObject);
                o[i].obj[q].name = lis.Count.ToString();
                o[i].obj[q].transform.SetSiblingIndex(lis.Count - 1);
            }
        }
        exserver(o);
        exclient(o);
        ExTrigger();
    }

    void exclient(List<derive> o)
    {
        StringBuilder content = new StringBuilder();
        content.Append("return { ");
        content.Append("\n\t");
        for (int i = 0; i < o.Count; i++)
        {
            for (int m = 0; m < o[i].obj.Count; m++)
            {
                content.Append(o[i].obj[m].isObstruct ? 1 : 0);
                content.Append(", ");
                if (m == o[i].obj.Count - 1)
                {
                    content.Append("\n\t");
                }
            }
        }
        content.Append("\n }");
        try
        {
            string path = "Assets/Lua/games/cfg/client/map/" + exPath + ".lua";//导出文件路径
            string foil = Path.GetDirectoryName(path);//导出文件夹路径
            if (Directory.Exists(foil) != true)
            {
                Directory.CreateDirectory(foil);
            }
            StreamWriter writer = new StreamWriter(path, false, new UTF8Encoding(false));
            writer.Write(content.ToString());
            writer.Flush();
            writer.Close();
        }
        catch { }
    }

    void exserver(List<derive> o)
    {
        StringBuilder content = new StringBuilder();
        content.Append("0");
        content.Append(Convert.ToString(o.Count, 16).PadLeft(4, '0'));
        content.Append(Convert.ToString(o[0].obj.Count, 16).PadLeft(4, '0'));
        content.Append("\n");
        for (int i = 0; i < o.Count; i++)
        {
            for (int m = 0; m < o[i].obj.Count; m++)
            {
                content.Append(o[i].obj[m].isObstruct ? 1 : 0);
            }
        }
        try
        {
            string path = "Assets/Lua/games/cfg/svr/stopinfo/" + exPath + "_stopinfo.stop";//导出文件路径
            string foil = Path.GetDirectoryName(path);//导出文件夹路径
            if (Directory.Exists(foil) != true)
            {
                Directory.CreateDirectory(foil);
            }
            StreamWriter writer = new StreamWriter(path, false, new UTF8Encoding(false));
            writer.Write(content.ToString());
            writer.Flush();
            writer.Close();
        }
        catch { }
    }

    #region Trigger
    public int mapId;
    public int triggerId;
    public int[] loadNexts;
    public int[] FindTarigerIDs;
    class GboxChildlist
    {
        public int index;
        public GboxChild obj;
        public void SetData(int idx, GboxChild gbox)
        {
            obj = gbox;
            index = idx;
        }
    }
    //[ContextMenu("Re-ExTrigger")]
    void ExTrigger()
    {
        StringBuilder content = new StringBuilder();
        content.AppendLine("return {");
        //Load
        string str = "";
        for (int i = 0; i < loadNexts.Length; i++)
        {
            str += loadNexts[i];
            if (i < loadNexts.Length - 1) { str += ","; }
        }
        content.AppendLine(string.Format("\t[{0}] = {{id = {1}, type = \"load\", nexts = {{{2}}}}},", triggerId, triggerId, str));
        //Child
        List<GboxChildlist> o = new List<GboxChildlist>();
        foreach (Transform item in transform)
        {
            if (item.name != gameObject.name)
            {
                var index = int.Parse(item.name);
                var gobox = item.gameObject.GetComponent<GboxChild>();
                o.Add(new GboxChildlist() { obj = gobox, index = index });
            }
        }
        string str1, Para, Nexts, Typex = "";
        o.Sort((b, m) => m.index.CompareTo(b.index));
        for (int i = 0; i < o.Count; i++)
        {
            if (o[i].obj.triggerId != 0)
            {
                Typex = GetTriggerTypeStr(o[i].obj.type);
                //Para
                Para = ",para = {";
                bool hasPData = false;
                for (int s = 0; s < o[i].obj.para.Length; s++)
                {
                    Para += "{";
                    for (int b = 0; b < o[i].obj.para[s].Array.Length; b++)
                    {
                        hasPData = true;
                        Para += o[i].obj.para[s].Array[b];
                        if (o[i].obj.para[s].Array.Length - 1 > b) { Para += ","; }
                    }
                    Para += "}";
                    if (o[i].obj.para.Length - 1 > s) { Para += ","; }
                }
                Para += "}";
                //Nexts
                Nexts = ",nexts = {";
                bool hasNData = false;
                for (int s = 0; s < o[i].obj.nexts.Count; s++)
                {
                    hasNData = true;
                    Nexts += o[i].obj.nexts[s];
                    if (o[i].obj.nexts.Count - 1 > s) { Nexts += ","; }
                }
                Nexts += "}";
                //整合
                str1 = string.Format("id = {0}, type = {1}", o[i].obj.triggerId, "\"" + Typex + "\"");
                if (hasPData) { str1 += Para; }
                if (hasNData) { str1 += Nexts; }
                str1 = string.Format("\t[{0}] = {{{1}}}", o[i].obj.triggerId, str1);
                if (i + 1 < o.Count)
                {
                    str1 += ",";
                }
                content.AppendLine(str1);
            }
        }
        content.AppendLine("}");
        string path = "Assets/Lua/games/cfg/svr/trigger/" + mapId + ".lua";//导出文件路径
        string foil = Path.GetDirectoryName(path);//导出文件夹路径
        try
        {
            if (Directory.Exists(foil) != true)
            {
                Directory.CreateDirectory(foil);
            }
            StreamWriter writer = new StreamWriter(path, false, new UTF8Encoding(false));
            writer.Write(content.ToString());
            writer.Flush();
            writer.Close();
        }
        catch { Debug.LogError("导出报错？  >> " + foil); }
        ExClientTrigger();
    }
    void ExClientTrigger()
    {
        StringBuilder content = new StringBuilder();
        content.AppendLine("return {");
        //Child
        List<GboxChildlist> o = new List<GboxChildlist>();
        foreach (Transform item in transform)
        {
            if (item.name != gameObject.name)
            {
                var index = int.Parse(item.name);
                var gobox = item.gameObject.GetComponent<GboxChild>();
                o.Add(new GboxChildlist() { obj = gobox, index = index });
            }
        }
        for (int i = 0; i < o.Count; i++)
        {
            if (o[i].obj.type == TriggerType.Add && o[i].obj.triggerId != 0)
            {
                if (o[i].obj.para.Length > 0  && o[i].obj.para[0].Array.Length > 2)
                {
                    content.AppendLine(string.Format("[{0}] = {{Pos = {{{1},{2},{3}}}, Rot = {{{4},{5},{6}}}, Scl = {7} }},", o[i].obj.para[0].Array[1], o[i].obj.paraPosX, o[i].obj.paraPosY, o[i].obj.paraPosZ, o[i].obj.paraRotX, o[i].obj.paraRotY, o[i].obj.paraRotZ, o[i].obj.paraScale));
                }
                else
                    Debug.LogError(string.Format("第{0}号格子配置有误， Add类型缺少para[2]的唯一ID", o[i].index));
            }
        }
        content.AppendLine("}");
        string path = "Assets/Lua/games/cfg/client/trigger/" + mapId + ".lua";//导出文件路径
        string foil = Path.GetDirectoryName(path);//导出文件夹路径
        try
        {
            if (Directory.Exists(foil) != true)
            {
                Directory.CreateDirectory(foil);
            }
            StreamWriter writer = new StreamWriter(path, false, new UTF8Encoding(false));
            writer.Write(content.ToString());
            writer.Flush();
            writer.Close();
        }
        catch { Debug.LogError("导出报错？  >> " + foil); }
    }

    string GetTriggerTypeStr(TriggerType triggerType)
    {
        switch (triggerType)
        {
            case TriggerType.Add: return "add";
            case TriggerType.Door: return "door";
            case TriggerType.Move: return "move";
            case TriggerType.Monster: return "monster";
            case TriggerType.Transport: return "transport";
            case TriggerType.ChangeStop: return "change_stop";
            case TriggerType.rAdd: return "radd";
            case TriggerType.ChatStart: return "chat_start";
            case TriggerType.ChatOver: return "chat_over";
            case TriggerType.Del: return "del";
            case TriggerType.BoxGet:return "box_get";
            case TriggerType.ForceFight:return "force_fight";
            default: return "";
        }
    }

    #endregion
    public void TryCreatChildObj()
    {
        System.Collections.Generic.List<GboxChild> ChildList = new System.Collections.Generic.List<GboxChild>();
        this.transform.GetComponentsInChildren<GboxChild>(true, ChildList);
        for (int i = ChildList.Count - 1; i >= 0; i--)
        {
            ChildList[i].CreateBoxObject();
        }
    }

    public void TryClearChildObj()
    {
        System.Collections.Generic.List<GboxChild> ChildList = new System.Collections.Generic.List<GboxChild>();
        this.transform.GetComponentsInChildren<GboxChild>(true, ChildList);
        for (int i = ChildList.Count - 1; i >= 0; i--)
        {
            ChildList[i].objs.Clear();
            ChildList[i].unrealObjs.Clear();
            ChildList[i].CreateBoxObject();
        }
    }

    public void TryClearUnrealChildObj()
    {
        System.Collections.Generic.List<GboxChild> ChildList = new System.Collections.Generic.List<GboxChild>();
        this.transform.GetComponentsInChildren<GboxChild>(true, ChildList);
        for (int i = ChildList.Count - 1; i >= 0; i--)
        {
            ChildList[i].DestroyUnRealOBj();
        }
    }

    public void TryProminentlyNexts(System.Collections.Generic.List<int> nexts)
    {
        System.Collections.Generic.List<GboxChild> ChildList = new System.Collections.Generic.List<GboxChild>();
        this.transform.GetComponentsInChildren<GboxChild>(true, ChildList);
        for (int i = ChildList.Count - 1; i >= 0; i--)
        {
            bool isset = false;
            for (int s = 0; s < nexts.Count; s++)
            {
                if (ChildList[i].triggerId == nexts[s] && nexts[s] != 0)
                {
                    isset = true;
                    var pos = ChildList[i].gameObject.transform.localPosition;
                    ChildList[i].gameObject.transform.localPosition = new Vector3(pos.x, 1, pos.z);
                }
            }
            if (isset == false)
            {
                var pos = ChildList[i].gameObject.transform.localPosition;
                ChildList[i].gameObject.transform.localPosition = new Vector3(pos.x, 0, pos.z);
            }
        }

    }
    [ContextMenu("FindTriggerIds")]
    public void TryFindTriggerIds()
    {
        if (FindTarigerIDs != null)
        {
            System.Collections.Generic.List<GboxChild> ChildList = new System.Collections.Generic.List<GboxChild>();
            this.transform.GetComponentsInChildren<GboxChild>(true, ChildList);
            for (int i = ChildList.Count - 1; i >= 0; i--)
            {
                bool isset = false;
                for (int s = 0; s < FindTarigerIDs.Length; s++)
                {
                    if (ChildList[i].triggerId == FindTarigerIDs[s] && FindTarigerIDs[s] != 0)
                    {
                        isset = true;
                        var pos = ChildList[i].gameObject.transform.localPosition;
                        ChildList[i].gameObject.transform.localPosition = new Vector3(pos.x, 1, pos.z);
                    }
                }
                if (isset == false)
                {
                    var pos = ChildList[i].gameObject.transform.localPosition;
                    ChildList[i].gameObject.transform.localPosition = new Vector3(pos.x, 0, pos.z);
                }
            }
        }
    }

    [ContextMenu("InitMapData（慎点）")]
    public void InitMapData()
    {
        System.Collections.Generic.List<GboxChild> ChildList = new System.Collections.Generic.List<GboxChild>();
        this.transform.GetComponentsInChildren<GboxChild>(true, ChildList);
        for (int i = ChildList.Count - 1; i >= 0; i--)
        {
            ChildList[i].InitData();

        }
    }
#endif
}
