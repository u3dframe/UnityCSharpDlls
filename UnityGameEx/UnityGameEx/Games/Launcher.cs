using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class Launcher : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InitPars();
        StartCoroutine(InitData(Entry));
    }

    void InitPars(){
        GHelper.Is_App_Quit = false;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
    }

    static IEnumerator InitBaseFolder(string zip, string tgtDir)
    {
        if (!Directory.Exists(tgtDir))
            Directory.CreateDirectory(tgtDir);

        using (var req = UnityWebRequest.Get(zip))
        {
            yield return req.SendWebRequest();
            SharpZipLib.Zipper.DeCompress(req.downloadHandler.data, tgtDir);
        }
    }
    
    IEnumerator InitData(System.Action cfEnd)
    {
#if !UNITY_EDITOR
        var inited = Path.Combine(Application.persistentDataPath, "__inited");
        if (!File.Exists(inited))
        {
            var zip = Path.Combine(Application.streamingAssetsPath,"base.zip");
            var disDir = Path.Combine(Application.persistentDataPath, "_resRoot");
            yield return InitBaseFolder(zip, disDir);
            var stream = File.Open(inited, FileMode.CreateNew);
            var d = System.Text.Encoding.UTF8.GetBytes(System.DateTime.Now.ToString());
            stream.Write(d,0,d.Length);
            stream.Close();
        }
#endif
		if(cfEnd != null){
            cfEnd();
        }
        yield break;
    }

    void Entry(){
        InputMgr.instance.Init();
        GameMgr.instance.InitAll();
        LuaManager.instance.Init();
    }
}
