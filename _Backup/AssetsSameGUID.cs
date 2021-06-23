using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Core.Art
{
	public class SGUID 
	{
		public string m_guid;
		public string m_assetPath;
		
		public SGUID(string guid,string assetPath)
		{
			this.m_guid = guid;
			this.m_assetPath = assetPath;
		}
		
		public override string ToString()
		{
			return string.Format("guid = [{0}] , assetPath = [{1}]", this.m_guid , this.m_assetPath);
		}
	}
	
	public class SGUIDS 
	{
		public List<SGUID> m_list = new List<SGUID> ();
	}
	
	public class AssetsSameGUID
	{
		[MenuItem("Assets/Tools_Art/Find Same GUID")]
		[MenuItem("Tools_Art/Find Same GUID")]
        static void FindSameGUID()
        {
            string[] temp = AssetDatabase.GetAllAssetPaths();
            Dictionary<string, SGUIDS> _dic = new Dictionary<string,SGUIDS>();
            SGUIDS _it = null;
            string _guid = null;
            foreach (string s in temp)
            {
                if (string.IsNullOrEmpty(s) || !s.Contains("Assets") || s.Equals("Assets"))
                    continue;
                _guid = AssetDatabase.AssetPathToGUID(s);
                if(_dic.ContainsKey(_guid))
                {
                    _it = _dic[_guid];
                }
                else
                {
                    _it = new SGUIDS();
                    _dic.Add(_guid, _it);
                }
                _it.m_list.Add(new SGUID(_guid,s));
            }

            foreach (var val in _dic.Values)
            {
                if(val.m_list.Count > 1)
                {
					foreach (var vv in val.m_list)
						Debug.Log(vv);
                }
            }
        }
	}
}