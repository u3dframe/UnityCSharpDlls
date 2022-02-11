using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// 此扩展用于coatingeffects shadder的第三个PASS “FAKESHADOW” 的启用和关闭
public class ShadowPassDrawer : MaterialPropertyDrawer
{
   
    /*
    public ShadowPassDrawer (bool show)
    {
        this.show=show;
    }
    */
   

    
/*
    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
   
        }
        */
 
     
  
      
    public override void OnGUI (Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
      
        bool value = (prop.floatValue != 0.0f);
     
        EditorGUI.BeginChangeCheck();
       

      
        value = EditorGUI.Toggle(position, label, value);

      
        if (EditorGUI.EndChangeCheck())
        {
           
            prop.floatValue = value ? 1.0f : 0.0f;
             (editor.target as Material).SetShaderPassEnabled("Always",value);
           
        }
        
    }
     
    
}
