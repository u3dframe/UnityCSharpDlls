using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Core.Kernel;

/// <summary>
/// 类名 : 日志提交外网
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-07-22 10:15
/// 功能 : lua 错误日志记录是放到 LuaState 的 PCall 里面的
/// </summary>
public class LogToNetHelper:MonoBehaviour
{
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

    string _url = "http://127.0.0.1:8080";
    public string m_url { get { return _url; } set { _url = value; } }

    string _proj = "log";
    public string m_proj { get { return _proj; } set { _proj = value; } }

    string k_title = "name";
    string k_step = "step";
    string k_msg = "val";

    public void Send(string subject,string body,int step)
	{
        if (string.IsNullOrEmpty(subject))
            return;

        if (!string.IsNullOrEmpty(body) && body.Contains("=== LogToNet error = "))
            return;
        
		try {
			StartCoroutine(EntCorNet(subject,body,step));
		} catch (Exception ex) {
			Debug.LogError ("=== LogToNet error = " + ex);
		}
	}

    public void Send(string subject, string body)
    {
        Send(subject, body, 99999);
    }


    IEnumerator EntCorNet(string subject, string body,int step){
		WWWForm _wf = new WWWForm();
        _wf.AddField(k_title, subject);
		_wf.AddField(k_step, step);
        if (!string.IsNullOrEmpty(body))
            _wf.AddField(k_msg, body);

        string _cur_url = m_url;
        if (!string.IsNullOrEmpty(m_proj))
        {
            _cur_url = UGameFile.ReUrlEnd(m_url) + m_proj;
        }
        using (UnityWebRequest request = UnityWebRequest.Post(_cur_url, _wf))
        {
            request.SetRequestHeader("content-type", "application/x-www-form-urlencoded;charset=utf-8");
            request.timeout = 59;
            yield return request.SendWebRequest();
        }
	}

    public LogToNetHelper Init(string url,string proj)
    {
        this.m_url = url;
        this.m_proj = proj;
        return this;
    }

    public static void Send2Net(string subject, string body)
	{
		shareInstance.Send (subject, body);
	}

	public static void Send2Net(string subject, string body,int step)
	{
		shareInstance.Send (subject, body,step);
	}
}