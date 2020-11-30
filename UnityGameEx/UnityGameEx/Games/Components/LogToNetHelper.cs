using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Core.Kernel;
using LitJson;

/// <summary>
/// 类名 : 日志提交外网
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-07-22 10:15
/// 功能 : lua 错误日志记录是放到 LuaState 的 PCall 里面的
/// </summary>
public class LogToNetHelper:MonoBehaviour
{
    class LogNetData
    {
        public string d_url = "";
        public int sendCount = 0;
        public Dictionary<string, string> m_kvs = new Dictionary<string, string>();
        public void Init(string url, string proj, params string[] kvs)
        {
            this.Clear();
            this.sendCount = 0;

            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(proj))
            {
                url = UGameFile.ReUrlEnd(url) + proj;
            }
            this.d_url = url;

            int _lens = kvs.Length;
            string _v;
            for (int i = 0; i < _lens; i += 2)
            {
                if (i + 1 >= _lens)
                    break;

                _v = kvs[i + 1];
                if (_v == null)
                    continue;

                this.m_kvs.Add(kvs[i], _v);
            }

            if (!this.m_kvs.ContainsKey("p_deviceName"))
                this.m_kvs.Add("p_deviceName", SystemInfo.deviceName);

            if (!this.m_kvs.ContainsKey("p_deviceModel"))
                this.m_kvs.Add("p_deviceModel", SystemInfo.deviceModel);

            if (!this.m_kvs.ContainsKey("p_deviceUniqueIdentifier"))
                this.m_kvs.Add("p_deviceUniqueIdentifier", SystemInfo.deviceUniqueIdentifier);

            if (!this.m_kvs.ContainsKey("p_graphicsShaderLevel"))
                this.m_kvs.Add("p_graphicsShaderLevel", SystemInfo.graphicsShaderLevel.ToString());

            if (!this.m_kvs.ContainsKey("p_identifier"))
                this.m_kvs.Add("p_identifier", Application.identifier);

            if (!this.m_kvs.ContainsKey("p_systemLanguage"))
                this.m_kvs.Add("p_systemLanguage", Application.systemLanguage.ToString());

            if (!this.m_kvs.ContainsKey("p_platform"))
                this.m_kvs.Add("p_platform", Application.platform.ToString());

            if (!this.m_kvs.ContainsKey("p_safeArea"))
                this.m_kvs.Add("p_safeArea", Screen.safeArea.ToString());

            if (!this.m_kvs.ContainsKey("p_dpi"))
                this.m_kvs.Add("p_dpi", Screen.dpi.ToString());

            if (!this.m_kvs.ContainsKey("p_create_local_time"))
                this.m_kvs.Add("p_create_local_time", DateTime.Now.ToString("yyMMddHHmmss"));
        }

        public void Clear()
        {
            this.m_kvs.Clear();
            this.sendCount = 0;
            this.d_url = null;
        }
    }

    static LogToNetHelper _shareInstance;
	static public LogToNetHelper shareInstance{
		get{
			if (_shareInstance == null) {
				GameObject _gobj = new GameObject ("LogToNetHelper");
				GameObject.DontDestroyOnLoad (_gobj);

				_shareInstance = _gobj.AddComponent<LogToNetHelper> ();
			}
			return _shareInstance;
		}
	}
    
    string m_url = "http://127.0.0.1:8080";
    string m_proj = "log";
    string k_subject = "p_name";
    string k_step = "p_step";
    string k_msg = "p_val";

    Queue<LogNetData> m_ques = new Queue<LogNetData>();
    Queue<LogNetData> m_cache = new Queue<LogNetData>();
    bool m_isRunning = false;
    [Range(1,20)] public int m_everyMax = 5;
    public bool m_isSendJson = true;
    
    public LogToNetHelper Init(string url,string proj,bool isSendJson = true)
    {
        this.m_url = url;
        this.m_proj = proj;
        this.m_isSendJson = isSendJson;
        return this;
    }

    string _def_url()
    {
        string _cur_url = m_url;
        if (!string.IsNullOrEmpty(m_proj))
            _cur_url = UGameFile.ReUrlEnd(_cur_url) + m_proj;
        return _cur_url;
    }

    void Update()
    {
        if (!this.m_isRunning)
            return;

        if (this.m_everyMax <= 0)
            this.m_everyMax = 5;

        for (int i = 0; i < this.m_everyMax; i++)
        {
            this._Sending();
        }
    }

    void _Sending()
    {
        if(m_ques.Count <= 0)
        {
            this.m_isRunning = false;
            return;
        }

        try
        {
            LogNetData _data = m_ques.Dequeue();
            if(this.m_isSendJson)
                StartCoroutine(EntCorNetJson(_data));
            else
                StartCoroutine(EntCorNet(_data));
        }
        catch (Exception ex)
        {
            Debug.LogError("=== LogToNet error = " + ex);
        }
    }

    IEnumerator EntCorHander(UnityWebRequest request, LogNetData data)
    {
        request.timeout = 59;
        yield return request.SendWebRequest();
        data.sendCount++;
        if (request.isHttpError || request.isNetworkError)
        {
            if (data.sendCount < 3)
            {
                _AddQueue(data);
                yield break;
            }
            Debug.LogErrorFormat("=== LogToNet error,n = [{0}],{1}", data.sendCount, request.error);
        }
        data.Clear();
        this.m_cache.Enqueue(data);
    }

    IEnumerator EntCorNet(LogNetData data)
    {
        WWWForm _wf = new WWWForm();
        foreach (var item in data.m_kvs)
        {
            _wf.AddField(item.Key, item.Value);
        }

        string _cur_url = data.d_url;
        if (string.IsNullOrEmpty(_cur_url))
            _cur_url = _def_url();

        using (UnityWebRequest request = UnityWebRequest.Post(_cur_url, _wf))
        {
            request.SetRequestHeader("charset", "utf-8");
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            yield return EntCorHander(request,data);
        }
    }

    IEnumerator EntCorNetJson(LogNetData data)
    {
        string _jd = JsonMapper.ToJson(data.m_kvs);
        string _cur_url = data.d_url;
        if (string.IsNullOrEmpty(_cur_url))
            _cur_url = _def_url();

        using (UnityWebRequest request = new UnityWebRequest(_cur_url, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(_jd));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            yield return EntCorHander(request, data);
        }
    }

    LogNetData _NewData()
    {
        if (this.m_cache.Count > 0)
            return this.m_cache.Dequeue();
        else
            return new LogNetData();
    }

    void _AddQueue(LogNetData data)
    {
        this.m_ques.Enqueue(data);
        this.m_isRunning = true;
    }

    public void SendDefault(string subject, string body, int step)
    {
        if (string.IsNullOrEmpty(subject))
            return;

        if (!string.IsNullOrEmpty(body))
        {
            if ((body.Contains("=== LogToNet") || body.Contains("Excludes")))
                return;
        }

        LogNetData _data = _NewData();
        _data.Init(this.m_url, this.m_proj, k_subject, subject, k_step, step.ToString(), k_msg, body);
        _AddQueue(_data);
    }

    public void SendDefault(string subject, string body)
    {
        SendDefault(subject, body, 99999);
    }

    public void SendParams(string url, string proj,params string[] kvs)
    {
        if (string.IsNullOrEmpty(url))
            url = this.m_url;
        LogNetData _data = _NewData();
        _data.Init(url,proj,kvs);
        _AddQueue(_data);
    }

    public void SendKvs(string url, string proj,string k1, string v1, string k2, string v2, string k3, string v3, string k4, string v4, string k5, string v5)
    {
        SendParams(url, proj, k1, v1, k2, v2, k3, v3, k4, v4, k5, v5);
    }

    public void SendKvs(string url, string proj, string k1, string v1, string k2, string v2, string k3, string v3, string k4, string v4)
    {
        SendParams(url, proj, k1, v1, k2, v2, k3, v3, k4, v4);
    }

    public void SendKvs(string url, string proj, string k1, string v1, string k2, string v2, string k3, string v3)
    {
        SendParams(url, proj, k1, v1, k2, v2, k3, v3);
    }

    public void SendKvs(string url, string proj, string k1, string v1, string k2, string v2)
    {
        SendParams(url, proj, k1, v1, k2, v2);
    }
}