using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GboxChild))]
[CanEditMultipleObjects]
public class PElementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();//绘制默认检视面板
        GboxChild uicom = (GboxChild)target;
        if (uicom)
        {
            if (GUILayout.Button("创建物体"))
            {
                uicom.TryCreateObj();
            }
            if (GUILayout.Button("清除物体"))
            {
                uicom.ClearBoxObjList();
            }
            if (GUILayout.Button("清除场景内虚拟物体"))
            {
                uicom.TryClearUnrealObj();
            }
            if (GUILayout.Button("清除场景内所有物体(虚拟物体和场景物体)"))
            {
                uicom.TryClearObj();
            }
            if (GUILayout.Button("点击突出显示关联格子"))
            {
                uicom.TryProminentlyNexts(true);
            }
            if (GUILayout.Button("点击恢复显示关联格子"))
            {
                uicom.TryProminentlyNexts(false);
            }
        }
    }
}