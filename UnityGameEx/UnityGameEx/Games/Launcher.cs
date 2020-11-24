using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Core;
using Core.Kernel;

public class Launcher : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameFile.InitFirst();
        _StartUpdateProcess();
    }

    void _StartUpdateProcess()
    {
        bool isUnZip = !GameFile.isEditor;
        bool isValidVer = !GameFile.isEditor;
        isValidVer = false;
        UpdateProcess updateProcess = new UpdateProcess().Init(Entry, _OnCallState, null,isUnZip, isValidVer);
        updateProcess.Start();
    }

    void Entry()
    {
        InputMgr.instance.Init();
        AssetBundleManager.instance.isDebug = true;
        LuaManager.instance.Init();
    }

    void _OnCallState(int state)
    {
        EM_Process emp = (EM_Process)state;
        Debug.LogFormat("=== state = [{0}] , [{1}] ", state, emp);
    }
}
