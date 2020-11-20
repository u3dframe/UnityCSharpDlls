using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;

public class EditorUGUIEx
{
	static string _fpInAsset4Font = "Assets/_Develop/Builds/fnts/font.ttf";
    static string _fpInAsset4Item = "Assets/_Develop/Builds/prefabs/ui/commons/ui_item.prefab";
    static Font font;
    static int layerUI = LayerMask.NameToLayer("UI");
#if UNITY_2017_1_OR_NEWER
    static bool needInCanvas = false;
#else
    static bool needInCanvas = true;
#endif

    // 重写Hierarchy的右键菜单
    // https://www.xuanyusong.com/archives/3893
    // [InitializeOnLoadMethod]
    // static void StartInitializeOnLoadMethod()
    // {
    //     EditorApplication.hierarchyWindowItemOnGUI += _OnHierarchyGUI;
    // }

    // static void _OnHierarchyGUI(int instanceID, Rect selectionRect)
    // {
    //     if (Event.current != null && selectionRect.Contains(Event.current.mousePosition)
    //         && Event.current.button == 1 && Event.current.type <= EventType.MouseUp)
    //     {
    //         GameObject selectedGameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
    //         //这里可以判断selectedGameObject的条件
    //         if (selectedGameObject)
    //         {
    //             Vector2 mousePosition = Event.current.mousePosition;
    //             EditorUtility.DisplayPopupMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), "GameObject/UGUI",null);
    //             Event.current.Use();
    //         }			
    //     }
    // }
	
	static Component CreateUIObject(Type type, string name, bool raycastTarget = false) {
        Component com = null;
		Transform _active = Selection.activeTransform;
        bool _isBl = (!needInCanvas) || (_active != null && _active.GetComponentInParent<Canvas>() != null);
        if (_isBl)
        {
            GameObject go = new GameObject(name,type);
            go.GetComponent<MaskableGraphic>().raycastTarget = raycastTarget;
            go.transform.SetParent(_active,false);
            go.layer = layerUI;
            Selection.activeGameObject = go;
            com = go.GetComponent(type);

            if(_active)
                EditorUtility.SetDirty(_active);
        }
        else
        {
            throw new System.Exception("必须在画布下创建UI");
        }
        return com;
    }
	
    [MenuItem("GameObject/UGUI2/Image", priority = 0)]
    static void NewImage()
    {
        CreateUIObject(typeof(Image), "image");
    }

    [MenuItem("GameObject/UGUI2/Text")]
    static Text NewText()
    {
		if (font == null)
        {
            font = AssetDatabase.LoadAssetAtPath(_fpInAsset4Font, typeof(Font)) as Font;
        }
        Text text = CreateUIObject(typeof(Text), "text") as Text;
		GameObject _gobj = text.gameObject;
		
        text.font = font;
        text.fontSize = 28;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = "Text";
        text.rectTransform.sizeDelta = new Vector2(100, 30);
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 24;
        text.resizeTextMaxSize = 28;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        _gobj.AddComponent<Outline>(); // 不加描边会模糊
		_gobj.AddComponent<UGUILocalize>();
        return text;
    }

    [MenuItem("GameObject/UGUI2/Button")]
    static void NewButton()
    {
        Image btn = CreateUIObject(typeof(Image),"button",true) as Image;
        // btn.AddComponent<UGUIButton>();
        btn.rectTransform.sizeDelta = new Vector2(100, 50);
        NewText();
        Selection.activeGameObject = btn.gameObject;
    }

    [MenuItem("GameObject/UGUI2/Item")]
    static void LoadUIItem()
    {
        Transform _active = Selection.activeTransform;
        if(_active == null) return;

		GameObject _obj = AssetDatabase.LoadAssetAtPath(_fpInAsset4Item, typeof(GameObject)) as GameObject;
		if(_obj == null) return;
		GameObject go = PrefabUtility.InstantiatePrefab(_obj) as GameObject;
        go.name = "ui_item";
        go.transform.SetParent(_active,false);
		go.SetActive(true);
    }

    [MenuItem("GameObject/UGUI2/ItemNew")]
    static void NewUIItem()
    {
        Transform _active = Selection.activeTransform;
        if(_active == null) return;
        
		GameObject _obj = AssetDatabase.LoadAssetAtPath(_fpInAsset4Item, typeof(GameObject)) as GameObject;
		if(_obj == null) return;
		
        GameObject gobj = GameObject.Instantiate(_obj,_active,false) as GameObject;
		if(gobj == null) return;
		gobj.name = "ui_item";
        gobj.layer = layerUI;
		gobj.SetActive(true);
    }

    [MenuItem("GameObject/UGUI2/InputField")]
    static void NewInputField()
    {
        Image img = CreateUIObject(typeof(Image),"inputfield",true) as Image;
        img.rectTransform.sizeDelta = new Vector2(140, 50);
        InputField inp = img.gameObject.AddComponent<InputField>();;
        Text _t1 = NewText();
        _t1.name = "placeholder";
        inp.placeholder = _t1;
        Selection.activeGameObject = inp.gameObject;

        _t1 = NewText();
        _t1.supportRichText = false;
        inp.textComponent = _t1;
        Selection.activeGameObject = inp.gameObject;
    }
}