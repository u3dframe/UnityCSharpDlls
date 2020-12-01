using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 类名 : FBX 导入设置
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-04-11 11:06
/// 功能 : 还未测试
/// </summary>
public class FBXImporter : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        ModelImporter importer = assetImporter as ModelImporter;
        if (importer != null)
        {
            importer.globalScale = 1.0f;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            // importer.isReadable = false;
#if UNITY_2019
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
#else
            importer.importMaterials = false;
#endif
        }
    }

    void OnPostprocessModel(GameObject gobj)
    {
        // BindModelCollider(gobj);
        // EmptyModelMaterial(gobj); // 导致很多问题,美术控制
        EmptyModelAnimation(gobj);
    }

    void BindModelCollider(GameObject gobj)
    {
        GameObject box = FindRecursively(gobj, "bbox");
        BindT<BoxCollider>(box);

        box = FindRecursively(gobj, "mbox");
        BindT<MeshCollider>(box);
        // if (box != null)
        // {
        // Renderer renderer = box.GetComponent(typeof(Renderer)) as Renderer;
        // if (renderer != null)
        // {
        //     UnityEngine.Object.DestroyImmediate(renderer, true);
        // }
        // }
    }

    void EmptyModelAnimation(GameObject gobj)
    {
        ModelImporter importer = assetImporter as ModelImporter;
        if (importer != null)
        {
            bool isReimport = false;
            if (importer.defaultClipAnimations.Length <= 0)
            {
                SkinnedMeshRenderer skinnedRenderer = gobj.GetComponentInChildren<SkinnedMeshRenderer>();
                if (skinnedRenderer == null)
                {
                    if (importer.animationType != ModelImporterAnimationType.None)
                    {
                        importer.animationType = ModelImporterAnimationType.None;
                        isReimport = true;
                    }
                    if (importer.importAnimation)
                    {
                        importer.importAnimation = false;
                        isReimport = true;
                    }
                }
                else
                {
                    if (importer.importAnimation)
                    {
                        importer.importAnimation = false;
                        isReimport = true;
                    }
                }
            }
            else
            {
                if (!importer.importAnimation)
                {
                    importer.importAnimation = true;
                    isReimport = true;
                }
            }
            if (isReimport)
            {
                importer.SaveAndReimport();
            }
        }
    }

    void EmptyModelMaterial(GameObject gobj)
    {
        Renderer[] _arrs = gobj.GetComponentsInChildren<Renderer>(true);
        if (_arrs == null || _arrs.Length <= 0)
            return;

        foreach (Renderer render in _arrs)
        {
            render.sharedMaterials = new Material[0];
        }
    }

    GameObject FindRecursively(GameObject gobj, string strName)
    {
        if (gobj == null)
        {
            return null;
        }
        if (gobj.name.Equals(strName))
        {
            return gobj;
        }

        foreach (Transform trsf in gobj.transform)
        {
            GameObject obj = FindRecursively(trsf.gameObject, strName);
            if (obj != null)
            {
                return obj;
            }
        }
        return null;
    }

    void BindT<T>(GameObject gobj) where T : Component
    {
        if (gobj == null)
            return;

        T _ret = gobj.GetComponent<T>();
        if (_ret == null)
        {
            gobj.AddComponent<T>();
        }
    }
    
    // [MenuItem("Assets/Tools/Re - Import All Fbx")]
    static void ReImportAllFbx()
	{
		string _fd = Application.dataPath;
		string[] _arrs = Directory.GetFiles(_fd,"*.fbx",SearchOption.AllDirectories);
		Object obj = null;
		string fpAsset = "";
		for (int i = 0; i < _arrs.Length; i++) {
			fpAsset = _arrs [i];
			obj = BuildPatcher.Load4Develop (fpAsset);
			if (obj == null)
				continue;
            fpAsset = BuildPatcher.GetPath(obj);
            AssetDatabase.ImportAsset(fpAsset);
		}
	}

    //  [MenuItem("Assets/Tools/Re - Import Select Fbx")]
	static void ReImportSelectFbx()
    {
        Object[] _arrs = Selection.GetFiltered(typeof(GameObject), SelectionMode.DeepAssets);
        Object _one = null;
        string _assetPath = null;
        for (int i = 0; i < _arrs.Length; ++i)
        {
            _one = _arrs[i];
            _assetPath = BuildPatcher.GetPath(_one);
            if (_assetPath.EndsWith(".fbx",System.StringComparison.OrdinalIgnoreCase))
            {
                AssetDatabase.ImportAsset(_assetPath);
            }
        }
    }
}