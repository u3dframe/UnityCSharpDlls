using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UObject = UnityEngine.Object;

namespace Core.Art
{
    public enum MAttrChangeType
    {
        None,
        Set,
        Append
    }
    public enum MAttrType{
        None,
        E_Float,
		E_Bool,
		E_Color,
		E_Vector4
	}
	
	public class MAttr{
		public string attrKey;
        public MAttrChangeType attrChangeType = MAttrChangeType.Set;
		public MAttrType attrType = MAttrType.E_Float;
        public bool attrValBool = false;
		public float attrVal1 = 0,attrVal2 = 0,attrVal3 = 0,attrVal4 = 0;

        float LmtVal(float src)
        {
            src = Mathf.Max(src, 0);
            src = Mathf.Min(src, 255);
            return src > 1 ? src / 255 : src;
        }


        public Color ToColor()
        {
            float r = LmtVal(this.attrVal1);
            float g = LmtVal(this.attrVal2);
            float b = LmtVal(this.attrVal3);
            float a = LmtVal(this.attrVal4);
            return new Color(r,g,b,a);
        }

        public Color AddColor(Color src)
        {
            float r = LmtVal(this.attrVal1);
            float g = LmtVal(this.attrVal2);
            float b = LmtVal(this.attrVal3);
            float a = LmtVal(this.attrVal4);
            r = Mathf.Clamp01(src.r + r);
            g = Mathf.Clamp01(src.g + g);
            b = Mathf.Clamp01(src.b + b);
            a = Mathf.Clamp01(src.a + a);
            return new Color(r, g, b, a);
        }

        public Vector4 ToVec4()
        {
            float x = this.attrVal1;
            float y = this.attrVal2;
            float z = this.attrVal3;
            float w = this.attrVal4;
            return new Vector4(x, y, z, w);
        }

        public Vector4 AddVec4(Vector4 src)
        {
            float x = this.attrVal1 + src.x;
            float y = this.attrVal2 + src.y;
            float z = this.attrVal3 + src.z;
            float w = this.attrVal4 + src.w;
            return new Vector4(x, y, z, w);
        }
    }
	
    /// <summary>
    /// 类名 : 处理mat的属性
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2021-07-21 18:18
    /// 功能 : 
    /// </summary>
    public class EDW_MatChgAttr : EditorWindow
    {
        static bool isOpenWindowView = false;

        static protected EDW_MatChgAttr vwWindow = null;

        // 窗体宽高
        static public float width = 900;
        static public float height = 460;

        [MenuItem("Tools_Art/Mat Settings", false, 10)]
        static void AddWindow()
        {
            if (isOpenWindowView || vwWindow != null)
                return;

            try
            {
                isOpenWindowView = true;

                vwWindow = GetWindow<EDW_MatChgAttr>("Image Settings");
                float _w = Screen.width;
                float _h = Screen.height;
                float _x = (_w - width) * 0.45f;
                float _y = (_h - height) * 0.45f;
                _x = Mathf.Max(_x, 0);
                _y = Mathf.Max(_y, 0);
                Vector2 pos = new Vector2(_x, _y);
                Vector2 size = new Vector2(width, height);
                Rect rect = new Rect(pos, size);
                vwWindow.position = rect;
                vwWindow.minSize = size;
            }
            catch
            {
                ClearWindow();
                throw;
            }
        }

        static void ClearWindow()
        {
            isOpenWindowView = false;
            vwWindow = null;
        }

        UObject mObj = null;
        static string fpDir = "";
        GUIStyle s_c = new GUIStyle();
		private Vector2 m_scoll = Vector2.zero;
        static private List<MAttr> m_list = new List<MAttr>();
        Color[] _arrColors = new Color[] {
            Color.magenta,Color.white,Color.yellow,Color.white,Color.green,Color.white,Color.cyan,Color.white,Color.blue,Color.white
        };

        #region  == EditorWindow Func ===

        void OnGUI()
        {
            EditorGUILayout.Space(10);
            mObj = EditorGUILayout.ObjectField("源文件夹:", mObj, typeof(UObject), false);
            if (mObj != null)
                fpDir = AssetDatabase.GetAssetPath(mObj);
            else
                fpDir = "";
            EditorGUILayout.Space(10);
            
            Color defGui = GUI.color;
            int _index = 0;
            using (var svs = new EditorGUILayout.ScrollViewScope(m_scoll))
			{
                m_scoll = svs.scrollPosition;
                int lens = m_list.Count;
                MAttr _item = null;
                for (int i = 0; i < lens; i++)
                {
                    _item = m_list[i];
                    GUI.color = _arrColors[_index];
                    _index++;
                    _index %= _arrColors.Length;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        _item.attrKey = EditorGUILayout.TextField("key:", _item.attrKey);
                        _item.attrChangeType = (MAttrChangeType)EditorGUILayout.EnumPopup("attr_change_type:", _item.attrChangeType);
                        _item.attrType = (MAttrType)EditorGUILayout.EnumPopup("attr_type:", _item.attrType);
                    }
                    EditorGUILayout.Space(10);
                    using (new EditorGUILayout.HorizontalScope())
                    {

                        switch (_item.attrType)
                        {
                            case MAttrType.E_Float:
                                _item.attrVal1 = EditorGUILayout.FloatField("val_float:", _item.attrVal1);
                                break;
                            case MAttrType.E_Bool:
                                _item.attrValBool = EditorGUILayout.ToggleLeft("val_bool:", _item.attrValBool);
                                _item.attrChangeType = MAttrChangeType.Set;
                                break;
                            case MAttrType.E_Color:
                                _item.attrVal1 = EditorGUILayout.FloatField("val_r:", _item.attrVal1);
                                _item.attrVal2 = EditorGUILayout.FloatField("val_g:", _item.attrVal2);
                                _item.attrVal3 = EditorGUILayout.FloatField("val_b:", _item.attrVal3);
                                _item.attrVal4 = EditorGUILayout.FloatField("val_a:", _item.attrVal4);
                                break;
                            case MAttrType.E_Vector4:
                                _item.attrVal1 = EditorGUILayout.FloatField("val_x:", _item.attrVal1);
                                _item.attrVal2 = EditorGUILayout.FloatField("val_y:", _item.attrVal2);
                                _item.attrVal3 = EditorGUILayout.FloatField("val_z:", _item.attrVal3);
                                _item.attrVal4 = EditorGUILayout.FloatField("val_w:", _item.attrVal4);
                                break;
                            default:
                                break;
                        }
                    }
                    EditorGUILayout.Space(10);
                }
            }
            GUI.color = defGui;
            using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("MatAttr++"))
				{
					m_list.Add(new MAttr());
				}
				
				if (GUILayout.Button("MatAttr--"))
				{
                    int lens = m_list.Count;
                    if(lens > 0)
                    {
                        m_list.RemoveAt(lens - 1);
                    }
				}
			}
            EditorGUILayout.Space(10);
            if (GUILayout.Button("处理 材质Mat"))
            {
                Handler();
            }
            if (GUILayout.Button("CloseWindows"))
            {
                if(vwWindow != null)
                    vwWindow.Close();
            }
        }

        // 在给定检视面板每秒10帧更新
        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnDestroy()
        {
            ClearWindow();
        }

        #endregion

        static string GetSuffixToLower(string path)
        {
            string _suffix = System.IO.Path.GetExtension(path);
            return _suffix.ToLower();
        }


        static void Handler()
        {
            if (m_list.Count <= 0)
            {
                EditorUtility.DisplayDialog("Empty", "没有添加可修改的属性", "Okey");
                return;
            }
            if (string.IsNullOrEmpty(fpDir))
                fpDir = Application.dataPath;
            var _arrs = Directory.GetFiles(fpDir, "*.*", SearchOption.AllDirectories);
            _arrs = _arrs.Where(s => s.ToLower().EndsWith(".mat")).ToArray();
            try
            {
				_HdAll(_arrs);
			}catch{}
            
			EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if(EditorUtility.DisplayDialog("End Over", "This is Finished", "Okey"))
                vwWindow.Close();
        }
		
		static void _HdAll(string[] _arrs)
		{
			float nLens = _arrs.Length;
			int nIndex = 0;
            EditorUtility.DisplayProgressBar(string.Format("开始处理材质{0}/{1}", nIndex, nLens), "begin", 0);
			string _pIt = null;
            Material _mat = null;
			for (int i = 0; i < nLens; i++)
            {
				nIndex++;
                _pIt = _arrs[i];
				_pIt = _pIt.Substring(_pIt.LastIndexOf("Assets"));
                _pIt = _pIt.Replace('\\', '/');
				EditorUtility.DisplayProgressBar(string.Format("正在处理材质{0}/{1}", nIndex, nLens), _pIt, nIndex / nLens);

                _mat = AssetDatabase.LoadAssetAtPath<Material>(_pIt);
                if (_mat == null)
                    continue;
                _HdOne(_mat);
            }
		}

        static void _HdOne(Material mat)
        {
            int _lens = m_list.Count;
            MAttr _mAttr = null;
            Color _c = Color.white;
            Vector4 _v4 = Vector4.zero;
            float _vf = 0;
            bool _isSet = false;
            for (int i = 0; i < _lens; i++)
            {
                _mAttr = m_list[i];
                if (_mAttr.attrChangeType == MAttrChangeType.None)
                    continue;
                if (_mAttr.attrType == MAttrType.None)
                    continue;
                if (!mat.HasProperty(_mAttr.attrKey))
                    continue;
                _isSet = (_mAttr.attrChangeType == MAttrChangeType.Set);
                switch (_mAttr.attrType)
                {
                    case MAttrType.E_Color:
                        if(_isSet)
                        {
                            _c = _mAttr.ToColor();
                        }
                        else
                        {
                            _c = mat.GetColor(_mAttr.attrKey);
                            _c = _mAttr.AddColor(_c);
                        }
                        mat.SetColor(_mAttr.attrKey, _c);
                        break;
                    case MAttrType.E_Vector4:
                        if (_isSet)
                        {
                            _v4 = _mAttr.ToVec4();
                        }
                        else
                        {
                            _v4 = mat.GetVector(_mAttr.attrKey);
                            _v4 = _mAttr.AddVec4(_v4);
                        }
                        mat.SetVector(_mAttr.attrKey, _v4);
                        break;
                    case MAttrType.E_Float:
                        _vf = _mAttr.attrVal1;
                        if (!_isSet)
                        {
                            _vf += mat.GetFloat(_mAttr.attrKey);
                        }
                        mat.SetFloat(_mAttr.attrKey, _vf);
                        break;
                    case MAttrType.E_Bool:
                        mat.SetInt(_mAttr.attrKey, _mAttr.attrValBool ? 1 : 0);
                        break;
                }
                EditorUtility.SetDirty(mat);
            }

        }
    }
}