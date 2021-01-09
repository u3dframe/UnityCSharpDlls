using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerType
{
    Add,//add
    Monster,
    ChangeStop,
    Transport,
    Move,
    Door,
    rAdd,
    ChatStart,
    ChatOver,
    Del,
    BoxGet,
    ForceFight,
    ChangeDoor
}

public class GboxChild : MonoBehaviour
{
#if UNITY_EDITOR
    [Range(1, 9)]
    public int ObstructLayer = 1;
    public float CreateObjPosY = 0;
    public bool isObstruct = false;//是否阻挡，用于导出阻挡信息
    public List<GameObject> objs = new List<GameObject>();
    public List<GameObject> unrealObjs = new List<GameObject>();
    List<GameObject> unreallist = new List<GameObject>();

    //Trigger
    public int triggerId;
    public TriggerType type;
    public CustomArrays[] para;
    public float paraPosX = 0;
    public float paraPosY = 0;
    public float paraPosZ = 0;
    public float paraRotX = 0;
    public float paraRotY = 0;
    public float paraRotZ = 0;
    public float paraScale = 1;
  
    public List<int> nexts = new List<int>();
    //-----------------------
    public void InitData()
    {
        objs.Clear();
        unrealObjs.Clear();
        unreallist.Clear();
        triggerId = 0;
        ObstructLayer = 1;
        type = TriggerType.Add;
        nexts.Clear();
        para = null;
        isObstruct = false;
        CreateBoxObject();
    }


    public void TryCreateObj()
    {
        GboxExport obj = this.transform.parent.GetComponent<GboxExport>();
        if (obj)
        {
            obj.TryCreatChildObj();
        }
    }

    public void TryClearUnrealObj()
    {
        GboxExport obj = this.transform.parent.GetComponent<GboxExport>();
        if (obj)
        {
            obj.TryClearUnrealChildObj();
        }
    }

    public void TryClearObj()
    {
        GboxExport obj = this.transform.parent.GetComponent<GboxExport>();
        if (obj)
        {
            obj.TryClearChildObj();
        }
    }


    public void TryProminentlyNexts(bool isshow)
    {
        GboxExport obj = this.transform.parent.GetComponent<GboxExport>();
        if (obj)
        {
            if(isshow) obj.TryProminentlyNexts(nexts);
            else obj.TryProminentlyNexts(new List<int>());
        }
    }

    public void CreateBoxObject()
    {
        unreallist.Clear();
        List<Transform> ChildList = new List<Transform>();
        this.transform.GetComponentsInChildren<Transform>(true, ChildList);
        for (int i = ChildList.Count-1; i >= 0; i--)
        {
            if (ChildList[i].gameObject != gameObject)
            {
                DestroyImmediate(ChildList[i].gameObject, true);
            }
        }
        for (int i = 0; i < objs.Count; i++)
        {

        if(objs[i]){
            GameObject o = GameObject.Instantiate(objs[i]);
            o.transform.SetParent(transform);
            o.transform.localScale = Vector3.one;
            o.transform.localPosition = Vector3.zero;
            o.transform.localRotation = Quaternion.identity;
            o.gameObject.layer = LayerMask.NameToLayer("SceneObj");
        }}
        for (int i = 0; i < unrealObjs.Count; i++)
        { if(unrealObjs[i]){
            GameObject o = GameObject.Instantiate(unrealObjs[i]);
            o.transform.SetParent(transform);
            o.transform.localScale = Vector3.one;
            o.transform.localPosition = Vector3.zero;
            o.transform.localRotation = Quaternion.identity;
            o.gameObject.layer = LayerMask.NameToLayer("SceneObj");
            o.gameObject.name = "UnrealObjs";
            unreallist.Add(o);}
        }
    }

    public void DestroyUnRealOBj()
    {
        if (unreallist != null)
        {
            for (int i = unreallist.Count - 1; i >= 0; i--)
            {
                if (unreallist[i] != null)
                {
                    DestroyImmediate(unreallist[i], true);
                }
            }
            unreallist = new List<GameObject>();
        }
        foreach (Transform item in transform)
        {
            if (item.name == "UnrealObjs")
            {
                DestroyImmediate(item.gameObject, true);
            }
        }
    }

    public void ClearBoxObjList()
    {
        objs = new List<GameObject>();
        CreateBoxObject();
    }

#endif
}
