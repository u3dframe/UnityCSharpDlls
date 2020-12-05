using UnityEngine;
using Core;
using Core.Kernel;

public class Launcher : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameFile.InitFirst(_StartUpdateProcess);
    }

    void _StartUpdateProcess()
    {
        bool isUnZip = !GameFile.isEditor;
        bool isValidVer = !GameFile.isEditor;
        isValidVer = false;
        string obbPath = null;
        UpdateProcess updateProcess = new UpdateProcess().Init(Entry, _OnCallState,obbPath,isUnZip, isValidVer);
        updateProcess.Start();
    }

    void Entry()
    {
        Debug.Log("=== Entry = ");
        InputMgr.instance.Init();
        UGUIEventSystem.instance.Init(false);
        AssetBundleManager.instance.isDebug = true;
        LuaManager.instance.Init();
    }

    void _OnCallState(int state,int preState)
    {
        EM_Process emp = (EM_Process)state;
        EM_Process empPre = (EM_Process)preState;
        Debug.LogFormat("=== state = [{0} , {1}] , pre_state = [{2} , {3}] ", state, emp, preState, empPre);
    }
}
