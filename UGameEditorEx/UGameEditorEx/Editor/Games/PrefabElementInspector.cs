using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 类名 : 预制体 PrefabElement 的 自定义Inspector界面
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2021-09-28 16:21
/// 功能 : 
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(PrefabElement))]
public class PrefabElementInspector : Editor
{
    PrefabElement m_obj;
    bool _isTimeline = false;
    bool _isCity = false;
    bool _isOutside = false;
    bool _isCard = false;
    bool _isScene = false;
	
    void OnEnable()
    {
        m_obj = target as PrefabElement;
        if(m_obj != null)
        {
            string _name = m_obj.name;
            _isTimeline = _name.StartsWith("tl_");
            _isCity = _name.StartsWith("map_maincity");
            _isOutside = _name.StartsWith("map_outside");
            _isCard = _name.StartsWith("map_card");
            _isScene = _name.StartsWith("map_");
        }
    }
	
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        if(_isTimeline && GUILayout.Button("ReBind Node 4 Timeline",EG_Helper.ToOptionH(30)))
        {
            this._ReBindNode4Timeline();
        }
        else if(_isCity && GUILayout.Button("ReBind Node 4 MainCity",EG_Helper.ToOptionH(30)))
        {
            this._ReBindNode4MainCity();
        }else if(_isOutside && GUILayout.Button("ReBind Node 4 Outside",EG_Helper.ToOptionH(30)))
        {
            this._ReBindNode4Outside();
        }else if(_isCard && GUILayout.Button("ReBind Node 4 Card",EG_Helper.ToOptionH(30)))
        {
            this._ReBindNode4Card();
        }else if(_isScene && (!_isCity && !_isOutside && !_isCard) && GUILayout.Button("ReBind Node 4 FightScene",EG_Helper.ToOptionH(30)))
        {
            this._ReBindNode4FightScene();
        }
    }

    void _ReBindNode(string[] _nodes){
        UtilityHelper.Is_App_Quit = false;
        List<GameObject> list = new List<GameObject>();
        GameObject _gobj = null;
        foreach (var item in _nodes)
        {
            _gobj = UtilityHelper.ChildRecursion(this.m_obj.gameObject,item);
            if (null == _gobj) continue;
            if (!list.Contains(_gobj))
            {
                list.Add(_gobj);
            }
        }
        GameObject[] gobjs = list.ToArray();
        this.m_obj.SetChildGobjs(gobjs);

        string _assetPath = AssetDatabase.GetAssetPath(this.m_obj);
        PrefabElement pe = (PrefabElement)GameObject.Instantiate(this.m_obj,Vector3.zero,Quaternion.identity);
        GameObject gobj = pe.gameObject;
        try
        {
            if(string.IsNullOrEmpty(_assetPath))
                _assetPath = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(this.m_obj.gameObject).prefabAssetPath;
            bool _isOkey = false;
            PrefabUtility.SaveAsPrefabAsset(gobj,_assetPath,out _isOkey);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
        GameObject.DestroyImmediate(gobj);
    }

    void _ReBindNode4Timeline(){
        // light_monster
        string[] _nodes = {
            "Camera","light_hero","Caster",
            "STarget1","STarget2","STarget3","STarget4","STarget5",
            "Target1","Target2","Target3","Target4","Target5"
        };
        this._ReBindNode(_nodes);
    }

    void _ReBindNode4MainCity(){
        string[] _nodes = {
            "MainCamera","rotation","maincity_alliance","maincity_parameter","maincity_store","maincity_hero","maincity_recover","maincity_card",
            "maincity_library","maincity_talent", "maincity_alliance_head", "maincity_parameter_head","maincity_store_head","maincity_hero_head",
            "maincity_recover_head","maincity_card_head","maincity_library_head","maincity_talent_head","GameObject"
        };
        this._ReBindNode(_nodes);
    }

    void _ReBindNode4Outside(){
        string[] _nodes = {
            "MainCamera","outside_rewardtask","outside_fissure","outside_simulation","outside_train","outside_arena",
            "outside_rewardtask_head","outside_fissure_head","outside_simulation_head","outside_train_head","outside_arena_head"
        };
        this._ReBindNode(_nodes);
    }

    void _ReBindNode4Card(){
        string[] _nodes = {
            "MainCamera","xiangji","card_skin","ef_sc_card_logo","texiao",
            "Dummy001","Dummy002","Dummy003","Dummy004","Dummy005","Dummy006","Dummy007","Dummy008",
            "ef_sc_card_click1","ef_sc_card_click2","ef_sc_card_click3","ef_sc_card_click4","ef_sc_card_click5","ef_sc_card_click6","ef_sc_card_click7","ef_sc_card_click8",
            "ef_sc_card_glowplane_01","ef_sc_card_glowline","ef_sc_doors",
            "ef_sc_card_firelight_01","ef_sc_card_firelight_02","ef_sc_card_monster_02","ef_sc_card_monster_03","ef_sc_card_monster_04"

        };
        this._ReBindNode(_nodes);
    }

    void _ReBindNode4FightScene(){
        string[] _nodes = {
            "MainCamera","gbox","Lights","Scene","Effects","Probes",
            "victory_hide","victory_show","victory_camera","victory_location"
        };
        this._ReBindNode(_nodes);
    }
}
