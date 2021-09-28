using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 类名 : 资源打包相关的窗体
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-03-26 09:29
/// 功能 : 
/// </summary>
namespace Core.Kernel{
	public class EDW_ABBuilderPatcher : EditorWindow {
		static bool isOpenWindowView = false;
		static protected EDW_ABBuilderPatcher vwWindow = null;

		// 窗体宽高
		static public float width = 688;
		static public float height = 198;

		[MenuItem("Tools/BuildAB_Patcher",false,0)]
		static void AddWindow()
		{
			if (isOpenWindowView || vwWindow != null)
				return;

			try
			{
				isOpenWindowView = true;
				// 大小不能拉伸
				// vwWindow = GetWindowWithRect<EDW_Skill>(rect, true, "SkillEditor");

				// 窗口，只能单独当成一个窗口,大小可以拉伸
				//vwWindow = GetWindow<EDW_Skill>(true,"SkillEditor");

				// 这个合并到其他窗口中去,大小可以拉伸
				vwWindow = GetWindow<EDW_ABBuilderPatcher>("BuidAB_Patcher");

				Vector2 pos = new Vector2(100,20);
				Vector2 size = new Vector2(width,height);
				Rect rect = new Rect(pos,size);
				vwWindow.position = rect;
				vwWindow.minSize = size;

                EditorApplication.wantsToQuit += delegate () {
                    OnClearSWindow();
                    return true;
                };
			}
			catch (System.Exception)
			{
				OnClearSWindow();
				throw;
			}
		}

		static void OnClearSWindow()
		{
			isOpenWindowView = false;
			vwWindow = null;
		}

		void OnEnable(){
			Core.GameFile.CurrDirRes();
			m_Object = new SerializedObject (this);
			m_Property = m_Object.FindProperty ("_assetList");
		}

		void OnDestroy()
		{
			OnClearSWindow();
		}

		// 在给定检视面板每秒10帧更新
		void OnInspectorUpdate()
		{
			Repaint();
		}

		//序列化对象
		SerializedObject m_Object;

		//序列化属性
		SerializedProperty m_Property;

		EM_EDWindows m_curWins = EM_EDWindows.BuildRes;

		[SerializeField]
		protected List<UnityEngine.Object> _assetList = new List<UnityEngine.Object>();

		
		EL_Protobuf builderProtobuf = new EL_Protobuf(); // 编译协议
		EL_AssetRes builderRes = new EL_AssetRes(); // 编译 资源
		EL_Patcher builderPatcher = new EL_Patcher(); // zip，版本信息生成逻辑

		float m_curH = 0; // 计算高度

		void OnGUI(){
			Rect _pos = this.position;
			float _tH = 0;
			EG_Helper.FEG_BeginV ();
			{
				EG_Helper.FEG_BeginH ();
				{
					GUIStyle style = EditorStyles.label;
					style.alignment = TextAnchor.MiddleCenter;
					string txtDecs = "类名 : 资源打包_Patcher工具\n"
						+ "作者 : Canyon\n"
						+ "日期 : 2020-03-07 09:29\n"
						+ "描述 : 暂无\n";
					GUILayout.Label(txtDecs, style);
					style.alignment = TextAnchor.MiddleLeft;
				}
				EG_Helper.FEG_EndH ();
				
				m_curWins = (EM_EDWindows) EditorGUILayout.EnumPopup("cur windows : ",m_curWins);
				EG_Helper.FG_Space(5);

				_tH = 4 * 25;

				switch(m_curWins){
					case EM_EDWindows.BuildProtobuf:
						_tH += builderProtobuf.DrawView ();
						break;
					case EM_EDWindows.Patcher:
						_tH += builderPatcher.DrawView();
						break;
					case EM_EDWindows.BuildRes:
						_tH += builderRes.DrawView(this.m_Object,this.m_Property,this._assetList);
						break;
					default:
						break;
				}
			}
			EG_Helper.FEG_EndV ();

			if(_tH != m_curH){
				m_curH = _tH;
				_pos.y = (_pos.y > 20 ? _pos.y : 20);
				_pos.height = _tH;
				this.position = _pos;
				Repaint();
			}
		}
	}
}