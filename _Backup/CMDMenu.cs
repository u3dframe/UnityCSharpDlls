using UnityEditor;
using UnityEngine;

using System.Reflection;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SDTime = System.DateTime;
using UObject = UnityEngine.Object;

/// <summary>
/// 类名 : Menu 菜单Tools里面的命名
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-11-24 11:40
/// 功能 : 
/// </summary>
public static class CMDMenu
{
	static public SelectionMode m_smAssets = SelectionMode.Assets | SelectionMode.DeepAssets;
	static public (int, int,TextureImporter) GetTextureSize(string assetPath)
	{
		var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
		return GetTextureSize(importer);
	}

	static public (int, int,TextureImporter) GetTextureSize(TextureImporter importer)
	{
		if (importer == null)
			return (0, 0,importer);
		object[] args = new object[2];
		System.Reflection.BindingFlags _bflag = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
		System.Reflection.MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", _bflag);
		mi.Invoke(importer, args);
		return ((int)args[0], (int)args[1],importer);
	}

	static public string GetAssetPath(UObject obj)
	{
		string _pIt = AssetDatabase.GetAssetPath(obj);
		_pIt = _pIt.Replace('\\', '/');
		return _pIt;
	}

	static public string GetFolder(UObject obj)
	{
		string _pIt = GetAssetPath(obj);
		if (!Directory.Exists(_pIt))
			_pIt = Path.GetDirectoryName(_pIt);
		return _pIt.Replace('\\', '/');
	}

	static public void InsetList4Min(List<string> list, string src)
	{
		string min = src, max = src;
		bool _isHas = false;
		foreach (var item in list)
		{
			min = src;
			max = src;
			if (item.Length > src.Length)
				max = item;
			else
				min = item;

			if (max.Contains(min))
			{
				_isHas = true;
				break;
			}
		}
		if (_isHas)
		{
			if (min.Equals(src))
			{
				list.Remove(max);
				InsetList4Min(list, src);
			}
		}
		else
		{
			list.Add(src);
		}
	}

	static public List<string> GetSelectFolders()
	{
		List<string> _listFolders = new List<string>();
		var _objs = Selection.GetFiltered<UObject>(SelectionMode.Assets);
		string _foder;
		foreach (var item in _objs)
		{
			_foder = GetFolder(item);
			InsetList4Min(_listFolders, _foder);
		}
		return _listFolders;
	}

	static public string[] GetSelectAssetPaths(string filter)
	{
		List<string> _listFolders = GetSelectFolders();
		if (_listFolders.Count <= 0)
			return null;
		string[] searchInFolders = _listFolders.ToArray();
		return GetRelativeAssetPaths(filter, searchInFolders);
	}

	static public string[] GetRelativeAssetPaths(string filter, string[] searchInFolders)
	{
		// filter = t:Prefab , t:Material , t:Model , t:Texture
		string[] arrs = AssetDatabase.FindAssets(filter, searchInFolders);
		if (arrs == null || arrs.Length <= 0)
			return null;
		string _assetPath;
		List<string> _list = new List<string>();
		for (int i = 0; i < arrs.Length; i++)
		{
			_assetPath = AssetDatabase.GUIDToAssetPath(arrs[i]);
			_list.Add(_assetPath);
		}
		return _list.ToArray();
	}
		
    [MenuItem("Tools_Art/_Opts/ExportCSV4ShaderVariantCount", false, 5)]
    public static void GetAllShaderVariantCount()
    {
        string _fpDll = string.Concat(EditorApplication.applicationContentsPath, @"\Managed\UnityEditor.dll");
        Assembly asm = Assembly.LoadFile(_fpDll);
        System.Type t2 = asm.GetType("UnityEditor.ShaderUtil");
        MethodInfo method = t2.GetMethod("GetVariantCount", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var shaderList = AssetDatabase.FindAssets("t:Shader");

        var output = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
        string pathF = string.Format("{0}/ShaderVariantCount_{1}.csv", output,SDTime.UtcNow.ToString("yyyyMMddHHmmss"));
        using(FileStream fs = new FileStream(pathF, FileMode.OpenOrCreate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            {
                EditorUtility.DisplayProgressBar("Shader统计文件", "正在写入统计文件中...", 0f);
                int ix = 0;
                sw.WriteLine("ShaderFile, VariantCount");
                foreach (var i in shaderList)
                {
                    EditorUtility.DisplayProgressBar("Shader统计文件", "正在写入统计文件中...", ix / shaderList.Length);
                    var path = AssetDatabase.GUIDToAssetPath(i);
                    Shader s = AssetDatabase.LoadAssetAtPath(path, typeof(Shader)) as Shader;
                    var variantCount = method.Invoke(null, new System.Object[] { s, true });
                    sw.WriteLine(path + "," + variantCount.ToString());
                    ++ix;
                }
                EditorUtility.ClearProgressBar();
                sw.Close();
                fs.Close();
            }
        }
    }

	[MenuItem("Tools_Art/_Opts/Check_BigTexture",false,5)]
    static public void CMD_CheckBigTexture(){
        EditorUtility.DisplayProgressBar("CheckBigTexture", "Checking", 0.1f);
        string[] searchInFolders = {
            "Assets"
        };
        string[] _tes = AssetDatabase.FindAssets("t:Texture",searchInFolders);
        string _assetPath;
        System.Text.StringBuilder _sbd = new System.Text.StringBuilder();
        _sbd.Append("Texture's Size  >  256x256").AppendLine();
        int _len = _tes.Length;
		TextureImporterPlatformSettings _tseting;
        for (int i = 0; i < _len; i++)
        {
            _assetPath = AssetDatabase.GUIDToAssetPath(_tes[i]);
            var _size = GetTextureSize(_assetPath);
            EditorUtility.DisplayProgressBar("CheckBigTexture ("+i+"/"+_len+")", _assetPath, i / (float)_len);
            if(_size.Item1 > 256 && _size.Item2 > 256){
                _sbd.AppendFormat("path = {0} , w x h = {1} x {2} , maxTextureSize = {3}",_assetPath,_size.Item1,_size.Item2,_size.Item3.maxTextureSize);
				_sbd.AppendLine();
				_tseting = _size.Item3.GetPlatformTextureSettings("Android");
				_sbd.AppendFormat("Android = [max = {0} , fmt = {1} , cQ = {2}]",_tseting.maxTextureSize,_tseting.format,_tseting.compressionQuality,_tseting.allowsAlphaSplitting);
				_sbd.AppendLine();
				_tseting = _size.Item3.GetPlatformTextureSettings("iOS");
				_sbd.AppendFormat("iOS     = [max = {0} , fmt = {1} , cQ = {2}]",_tseting.maxTextureSize,_tseting.format,_tseting.compressionQuality,_tseting.allowsAlphaSplitting);
				_sbd.AppendLine();
				_sbd.AppendLine();
            }
			System.Threading.Thread.Sleep(0);
        }
        string _cont = _sbd.ToString();
        _sbd.Clear();
        _sbd.Length = 0;
        EditorUtility.ClearProgressBar();

		// string _fdir = Application.dataPath + "/../"; // m_dirRes
		string _fdir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + "/";;
        string _fp = string.Format("{0}big_texture_{1}.txt",_fdir,SDTime.UtcNow.ToString("MMddHHmmss"));
		File.WriteAllText(_fp,_cont);
        Debug.LogErrorFormat("===== write to {0}",_fp);
    }
	
	static private void RemoveElement(Material mat, string spName, SerializedProperty saveProperty)
	{
		SerializedProperty property = saveProperty.FindPropertyRelative(spName);
		bool _isTexture = "m_TexEnvs".Equals(spName);
		for (int i = property.arraySize - 1; i >= 0; i--)
		{
			var prop = property.GetArrayElementAtIndex(i);
			string propertyName = prop.displayName;
			if (!mat.HasProperty(propertyName))
			{
				property.DeleteArrayElementAtIndex(i);
				continue;
			}

			if (_isTexture)
			{
			}
		}
	}

	[MenuItem("Assets/Tools_Art/_Opts/Rmv Select Mat's OverdueProperties", false, 5)]
	static void HandlerOverdueMatProperties4Select()
	{
		EditorUtility.DisplayProgressBar("Rmv Mat Properties", "Start ...", 0.0f);
		// var arrs = Selection.GetFiltered<Material>(m_smAssets); // 太卡了
		var arrs = GetSelectAssetPaths("t:Material");
		if (arrs == null || arrs.Length <= 0)
		{
			Debug.LogError("=== Rmv Mat Properties = is not select materials");
			EditorUtility.ClearProgressBar();
			return;
		}
		EditorUtility.DisplayProgressBar("Rmv Mat Properties", "begin ...", 0);
		string _assetPath;
		int _lens = arrs.Length;
		for (int i = 0; i < _lens; i++)
		{
			_assetPath = arrs[i];
			Material mat = AssetDatabase.LoadAssetAtPath<Material>(_assetPath);
			EditorUtility.DisplayProgressBar("Rmv Mat Properties ("+i+"/"+_lens+")",_assetPath, (i + 1) / (float)_lens);
			SerializedObject so = new SerializedObject(mat);
			SerializedProperty m_SavedProperties = so.FindProperty("m_SavedProperties");
			RemoveElement(mat, "m_TexEnvs", m_SavedProperties);
			RemoveElement(mat, "m_Floats", m_SavedProperties);
			RemoveElement(mat, "m_Colors", m_SavedProperties);
			so.ApplyModifiedProperties();
			System.Threading.Thread.Sleep(0);
		}
		EditorUtility.ClearProgressBar();
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		EditorUtility.UnloadUnusedAssetsImmediate();
		EditorUtility.DisplayDialog("Rmv Mat Properties Finished", "Rmv Mat's OverdueProperties", "Okey");
	}
	
	static private bool RemoveMatMonoOrNoDependency(string fp)
	{
		if (!fp.EndsWith(".mat"))
			return false;
		bool _isChg = false, _isHas = false, _isDelNext = false;
		string[] _lines = File.ReadAllLines(fp);
		List<string> _list = new List<string>();
		string _cur, _next, _curTrim;
		int _lens = _lines.Length;
		string RegexStr = @"guid: \w+", _guid, _assetPath4GUID;
		Match _match = null; // 单行匹配
		for (int i = 0; i < _lens; i++)
		{
			_cur = _lines[i];
			_curTrim = _cur.Trim();
			if (_curTrim.StartsWith("---"))
				_isHas = false;

			if (_isHas)
				continue;

			if (_curTrim.StartsWith("MonoBehaviour:"))
				_isHas = true;
			_next = null;
			if ((i + 1) < _lens)
			{
				_next = _lines[i + 1].Trim();
				if (_next.StartsWith("MonoBehaviour:"))
					_isHas = true;
			}
			_isChg = _isChg || _isHas;
			if (_isHas)
				continue;
			if (_isDelNext)
			{
				_isDelNext = false;
				continue;
			}
			if (Regex.IsMatch(_cur, RegexStr))
			{
				_match = Regex.Match(_cur, RegexStr);
				_guid = _match.Value;
				_guid = _guid.Replace("guid: ", "").Trim();
				_assetPath4GUID = AssetDatabase.GUIDToAssetPath(_guid);
				if (string.IsNullOrEmpty(_assetPath4GUID))
				{
					int _index = _cur.IndexOf("{");
					int _index2 = _cur.LastIndexOf("}");
					_cur = _cur.Substring(0, _index);
					_cur = string.Concat(_cur, "{fileID: 0}");
					_isChg = true;
					_isDelNext = _index2 <= 0;
				}
			}
			_list.Add(_cur);
		}
		if (_isChg)
		{
			string[] _line2 = _list.ToArray();
			File.WriteAllLines(fp, _line2);
		}
		return _isChg;
	}
	
	[MenuItem("Assets/Tools_Art/_Opts/Rmv Select Mat's Mono Or NoDependency", false, 5)]
	static void RmvSelectMatsMonoOrNoDependency()
	{
		EditorUtility.DisplayProgressBar("Rmv Mat's MonoBehaviour", "Start ...", 0.0f);
		var arrs = GetSelectAssetPaths("t:Material");
		if (arrs == null || arrs.Length <= 0)
		{
			EditorUtility.ClearProgressBar();
			Debug.LogError("=== Rmv Mat's MonoBehaviour = is not select folders");
			return;
		}
		EditorUtility.DisplayProgressBar("Rmv Mat's MonoBehaviour", "begin ...", 0);
		int _lens = arrs.Length;
		string dirDataNoAssets = Application.dataPath.Replace("Assets", "");
		dirDataNoAssets = dirDataNoAssets.Replace('\\', '/');
		string _assetPath, _filePath;
		bool _isChg = false, _isCurChg = false;
		for (int i = 0; i < _lens; i++)
		{
			_assetPath = arrs[i];
			EditorUtility.DisplayProgressBar("Rmv Mat's MonoBehaviour ("+i+"/"+_lens+")", _assetPath, (i + 1) / (float)_lens);
			_filePath = dirDataNoAssets + _assetPath;
			_isCurChg = RemoveMatMonoOrNoDependency(_filePath);
			_isChg = _isChg || _isCurChg;
			System.Threading.Thread.Sleep(0);
		}
		EditorUtility.ClearProgressBar();
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		EditorUtility.UnloadUnusedAssetsImmediate();
		EditorUtility.DisplayDialog("Rmv Mat's MonoBehaviour Finished", "this is over", "Okey");
	}
	
	[MenuItem("Tools_Art/_Opts/Check_SpriteAtlas",false,5)]
    static public void CMD_CheckSpriteAtlas(){
        EditorUtility.DisplayProgressBar("CheckSpriteAtlas", "Checking", 0.1f);
		string[] searchInFolders = {
            "Assets"
        };
		var arrs = GetRelativeAssetPaths("t:SpriteAtlas",searchInFolders);
		if (arrs == null || arrs.Length <= 0)
		{
			EditorUtility.ClearProgressBar();
			Debug.LogError("=== CheckSpriteAtlas = is not select folders");
			return;
		}
		
        string _assetPath;
		int _len = arrs.Length;
		UnityEngine.U2D.SpriteAtlas _it;
		Dictionary<UObject,List<string>> _dicPath = new Dictionary<UObject, List<string>>();
        for (int i = 0; i < _len; i++)
        {
            _assetPath = arrs[i];
			_it = AssetDatabase.LoadAssetAtPath<UnityEngine.U2D.SpriteAtlas>(_assetPath);
            EditorUtility.DisplayProgressBar("CheckSpriteAtlas ("+i+"/"+_len+")", _assetPath, i / (float)_len);
			var objs = UnityEditor.U2D.SpriteAtlasExtensions.GetPackables(_it);
			foreach (var item in objs)
			{
				if(_dicPath.ContainsKey(item))
				{
					var list = _dicPath[item];
					list.Add(_assetPath);
				}else{
					List<string> list = new List<string>();
					_dicPath.Add(item,list);
					list.Add(_assetPath);
				}
			}
			System.Threading.Thread.Sleep(0);
        }

		System.Text.StringBuilder _sbd = new System.Text.StringBuilder();
        _sbd.Append("More in SpriteAtlas").AppendLine();
		foreach (var key in _dicPath.Keys)
		{
			var list = _dicPath[key];
			if(list.Count > 1)
			{
				_sbd.AppendFormat("{0} === has more SpriteAtlas",AssetDatabase.GetAssetPath(key)).AppendLine();
				foreach (var item in list)
				{
					_sbd.AppendFormat(item).AppendLine();
				}
				_sbd.AppendLine();
			}
			System.Threading.Thread.Sleep(0);
		}

        string _cont = _sbd.ToString();
        _sbd.Clear();
        _sbd.Length = 0;
        EditorUtility.ClearProgressBar();
		EditorUtility.UnloadUnusedAssetsImmediate();
		string _fdir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + "/";;
        string _fp = string.Format("{0}more_spriteAtlas_{1}.txt",_fdir,SDTime.UtcNow.ToString("MMddHHmmss"));
		File.WriteAllText(_fp,_cont);
        Debug.LogErrorFormat("===== write to {0}",_fp);
    }

	static void CleanupShapeModuleMesh(ref ParticleSystem.ShapeModule shape, ref bool isChg, int ntype = 3)
        {
            bool isMesh = ntype != 0 || ntype == 3;
            if (isMesh)
            {
                isChg = isChg || shape.mesh != null;
                shape.mesh = null;
            }
            bool isMeshRenderer = ntype != 1 || ntype == 3;
            if (isMeshRenderer)
            {
                isChg = isChg || shape.meshRenderer != null;
                shape.meshRenderer = null;
            }
            bool isSkinMeshRenderer = ntype != 2 || ntype == 3;
            if (isSkinMeshRenderer)
            {
                isChg = isChg || shape.skinnedMeshRenderer != null;
                shape.skinnedMeshRenderer = null;
            }
        }

        static void CleanupShapeModuleSprite(ref ParticleSystem.ShapeModule shape, ref bool isChg, int ntype = 3)
        {
            bool isSprite = ntype != 0 || ntype == 3;
            if (isSprite)
            {
                isChg = isChg || shape.sprite != null;
                shape.sprite = null;
            }
            bool isSpriteRenderer = ntype != 1 || ntype == 3;
            if (isSpriteRenderer)
            {
                isChg = isChg || shape.spriteRenderer != null;
                shape.spriteRenderer = null;
            }
        }

        static bool CleanupEmptyComp(GameObject gobj)
        {
            string _newPath = AssetDatabase.GetAssetPath(gobj);
            GameObject gobjClone = PrefabUtility.InstantiatePrefab(gobj) as GameObject;
            bool _isChg = false;
            var _arrs1 = gobjClone.GetComponentsInChildren<Animator>(true);
            foreach (var item in _arrs1)
            {
                if (item != null && item.runtimeAnimatorController == null)
                {
                    GameObject.DestroyImmediate(item);
                    _isChg = true;
                }
            }

            var _arrs2 = gobjClone.GetComponentsInChildren<Animation>(true);
            foreach (var item in _arrs2)
            {
                if (item != null && item.GetClipCount() <= 0)
                {
                    GameObject.DestroyImmediate(item);
                    _isChg = true;
                }
            }

            var _arrs3 = gobjClone.GetComponentsInChildren<UnityEngine.Playables.PlayableDirector>(true);
            foreach (var item in _arrs3)
            {
                if (item != null && item.playableAsset == null)
                {
                    GameObject.DestroyImmediate(item);
                    _isChg = true;
                }
            }

            var _arrs4 = gobjClone.GetComponentsInChildren<ParticleSystem>(true);
            ParticleSystemRenderer _rer_ = null;
            bool _isTrail;
            foreach (var item in _arrs4)
            {
                _isTrail = false;
                if (item != null)
                {
                    var _shapeModule = item.shape;
                    switch (_shapeModule.shapeType)
                    {
                        case ParticleSystemShapeType.Mesh:
                            CleanupShapeModuleMesh(ref _shapeModule, ref _isChg, 0);
                            break;
                        case ParticleSystemShapeType.MeshRenderer:
                            CleanupShapeModuleMesh(ref _shapeModule, ref _isChg, 1);
                            break;
                        case ParticleSystemShapeType.SkinnedMeshRenderer:
                            CleanupShapeModuleMesh(ref _shapeModule, ref _isChg, 2);
                            break;
                        case ParticleSystemShapeType.Sprite:
                            CleanupShapeModuleSprite(ref _shapeModule, ref _isChg, 0);
                            break;
                        case ParticleSystemShapeType.SpriteRenderer:
                            CleanupShapeModuleSprite(ref _shapeModule, ref _isChg, 1);
                            break;
                        default:
                            CleanupShapeModuleMesh(ref _shapeModule, ref _isChg);
                            CleanupShapeModuleSprite(ref _shapeModule, ref _isChg);
                            break;
                    }

                    _rer_ = item.GetComponent<ParticleSystemRenderer>();
                    if (_rer_ != null)
                    {
                        _isTrail = (item.trails.enabled && _rer_.trailMaterial != null);

                        switch (_rer_.renderMode)
                        {
                            case ParticleSystemRenderMode.Mesh:
                                break;
                            default:
                                if (!_isTrail && _rer_.renderMode == ParticleSystemRenderMode.None && _rer_.sharedMaterial != null)
                                {
                                    // 清除过 sharedMaterials 最终还是会记录两个 空数据
                                    _rer_.sharedMaterial = null;
                                    _isChg = _isChg || true;
                                }
                                _isChg = _isChg || _rer_.mesh != null;
                                _rer_.mesh = null;
                                break;
                        }
                    }

                    if ((_rer_ == null) || (!_rer_.enabled) || ((_rer_.sharedMaterial == null) && !_isTrail))
                    {
                        if (_rer_ != null)
                        {
                            _rer_.sharedMaterial = null;
                            _rer_.sharedMaterials = new Material[0];
                        }
                        GameObject.DestroyImmediate(item);
                        _isChg = _isChg || true;
                    }
                }
            }

            bool _isBl = gobj.isStatic;
            if (_isBl)
            {
                gobjClone.isStatic = false;
                _isChg = true;
            }

            _isBl = gobj.activeSelf;
            if (!_isBl)
            {
                gobjClone.SetActive(true);
                _isChg = true;
            }

            if (_isChg)
            {
                PrefabUtility.SaveAsPrefabAsset(gobjClone, _newPath);
                gobjClone.hideFlags = HideFlags.HideAndDontSave;
            }

            GameObject.DestroyImmediate(gobjClone); // 删除掉实例化的对象
            return _isChg;
        }

		static public List<GameObject> GetGobjs(params string[] fpdir)
        {
            string[] arrs = GetRelativeAssetPaths("t:Prefab", fpdir);
            if (arrs == null || arrs.Length <= 0)
                return null;
            string _prefabPath = "";
            GameObject _prefab = null;
            List<GameObject> _list = new List<GameObject>();
            for (int i = 0; i < arrs.Length; i++)
            {
                _prefabPath = arrs[i];
                _prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
                if (_prefab != null)
                    _list.Add(_prefab);
				System.Threading.Thread.Sleep(0);
            }
            return _list;
        }

		static public GameObject[] GetSelectGobjs()
        {
            List<GameObject> _listObjs = new List<GameObject>();
            List<string> _listFolders = GetSelectFolders();
            string[] searchInFolders = _listFolders.ToArray();
            var _arrGobjs = GetGobjs(searchInFolders);
            if(_arrGobjs != null)
            {
                GameObject _gobj = null;
                for (int i = 0; i < _arrGobjs.Count; i++)
                {
                    _gobj = _arrGobjs[i];
                    if (!_listObjs.Contains(_gobj))
                        _listObjs.Add(_gobj);
                }
            }
            return _listObjs.ToArray();
        }

        [MenuItem("Assets/Tools_Art/_Opts/Rmv Select Prefab's UnUsed Comp")]
        static void RemoveUnusedComp4Prefab()
        {
            EditorUtility.DisplayProgressBar("Rmv Select Fab's UnUsed Comp", "Start ...", 0.0f);
            var arrs = GetSelectGobjs();
            if (arrs == null || arrs.Length <= 0)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("=== Rmv Select Fab's UnUsed Comp = is not select Prefabs");
                return;
            }
            EditorUtility.DisplayProgressBar("Rmv Select Fab's UnUsed Comp", "begin ...", 0);
            int _lens = arrs.Length;
            GameObject _it = null;
            for (int i = 0; i < _lens; i++)
            {
                _it = arrs[i];
                EditorUtility.DisplayProgressBar("Rmv Select Fab's UnUsed Comp ("+i+"/"+_lens+")", _it.name, (i + 1) / (float)_lens);
                CleanupEmptyComp(_it);
				System.Threading.Thread.Sleep(0);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
			EditorUtility.UnloadUnusedAssetsImmediate();
            EditorUtility.DisplayDialog("Rmv Select Fab's UnUsed Comp Finished", "this is over", "Okey");
        }

		static private bool RemoveFabNoDependency(string fp)
        {
            if (!fp.EndsWith(".prefab"))
                return false;
            bool _isChg = false, _isDelNext = false;
            string[] _lines = File.ReadAllLines(fp);
            List<string> _list = new List<string>();
            string _cur;
            int _lens = _lines.Length;
            string RegexStr = @"guid: \w+", _guid, _assetPath4GUID;
            Match _match = null; // 单行匹配
            for (int i = 0; i < _lens; i++)
            {
                _cur = _lines[i];
                if (_isDelNext)
                {
                    _isDelNext = false;
                    continue;
                }
                if (Regex.IsMatch(_cur, RegexStr))
                {
                    _match = Regex.Match(_cur, RegexStr);
                    _guid = _match.Value;
                    _guid = _guid.Replace("guid: ", "").Trim();
                    _assetPath4GUID = AssetDatabase.GUIDToAssetPath(_guid);
                    if (string.IsNullOrEmpty(_assetPath4GUID))
                    {
                        int _index = _cur.IndexOf("{");
                        int _index2 = _cur.LastIndexOf("}");
                        _cur = _cur.Substring(0, _index);
                        _cur = string.Concat(_cur, "{fileID: 0}");
                        _isChg = true;
                        _isDelNext = _index2 <= 0;
                    }
                }
                _list.Add(_cur);
            }
            if (_isChg)
            {
                string[] _line2 = _list.ToArray();
                File.WriteAllLines(fp, _line2);
            }
            return _isChg;
        }

        [MenuItem("Assets/Tools_Art/_Opts/Rmv Select Prefab's NoDependency")]
        static void RmvSelectFabNoDependency()
        {
            EditorUtility.DisplayProgressBar("Rmv Select Fab's NoDependency", "Start ...", 0.0f);
            var arrs = GetSelectAssetPaths("t:Prefab");
            if (arrs == null || arrs.Length <= 0)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("=== Rmv Select Fab's NoDependency = is not select folders");
                return;
            }
            EditorUtility.DisplayProgressBar("Rmv Select Fab's NoDependency", "begin ...", 0);
            int _lens = arrs.Length;
            string dirDataNoAssets = Application.dataPath.Replace("Assets", "");
            dirDataNoAssets = dirDataNoAssets.Replace('\\', '/');
            string _assetPath, _filePath;
            bool _isChg = false, _isCurChg = false;
            for (int i = 0; i < _lens; i++)
            {
                _assetPath = arrs[i];
                EditorUtility.DisplayProgressBar("Rmv Select Fab's NoDependency ("+i+"/"+_lens+")", _assetPath, (i + 1) / (float)_lens);
                _filePath = dirDataNoAssets + _assetPath;
                _isCurChg = RemoveFabNoDependency(_filePath);
                _isChg = _isChg || _isCurChg;
                System.Threading.Thread.Sleep(0);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
			EditorUtility.UnloadUnusedAssetsImmediate();
            EditorUtility.DisplayDialog("Rmv Select Fab's NoDependency", "this is over", "Okey");
        }
}