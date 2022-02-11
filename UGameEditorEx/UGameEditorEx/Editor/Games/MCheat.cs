using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using LitJson;

namespace Assets.Editor
{
    public class MCheat : EditorWindow
    {
        class Msg
        {
            public string name;
            public int session;
            public string request;
            public string response;
        };
        List<Msg> SLIST = new List<Msg>();
        HashSet<string> IGNORE = new HashSet<string>();
        List<string> REMOVE_IGNORE = new List<string>();

        string m_editorTexts = "";

        Vector2 v2 = new Vector2(0, 0);

        bool in_running = false;
        bool show_server = true;
        bool show_client = true;
        int m_filtertype = 0;
        string lastError = "";

        const int name_width = 200;
        const int button_width = 50;
        const int scrollbar_width = 50;


        void AddRequest(int session, string name, string request, string response)
        {
            SLIST.Add(new Msg
            {
                name = name,
                session = session,
                request = request,
                response = response,
            });
            this.Repaint();
        }

        void AddResponse(int session, string response)
        {
            for (int i = 0; i < SLIST.Count; i++)
            {
                Msg msg = SLIST[i];
                if (msg.session == session)
                {
                    msg.response = response;
                    this.Repaint();
                    break;
                }
            }
        }

        string Message_Captured(string context)
        {
            Debug.Log("Message_Captured:" + context);
            try
            {
                JsonData data = JsonMapper.ToObject(context);
                int session = 0;
                if (data.Keys.Contains("session"))
                    session = int.Parse(data["session"].ToString());
                string request = "";
                if (data.Keys.Contains("request"))
                    request = data["request"].ToString();
                string response = "";
                if (data.Keys.Contains("response"))
                    response = data["response"].ToString();

                string name = "";
                if (data.Keys.Contains("name"))
                    name = data["name"].ToString();

                if ((session > 0 && name != "") || session == 0)
                {
                    AddRequest(session, name, request, response);
                }
                else
                {
                    AddResponse(session, data["response"].ToString());
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            return "OK";
        }

        [MenuItem("Tools/MCHEAT")]
        static void ToolsShow()
        {
            GetWindow(typeof(MCheat), false, "消息捕捉", true);
        }


        public void Awake()
        {
            SetRun(in_running);
        }

        void SetRun(bool running)
        {
            in_running = running;
            if(!EditorApplication.isPlaying)
                return;
            
            lastError = "";
            string result;
            if (CsWithLua.CallLua("MgrNet.Hook", running ? "start" : "stop", out result))
            {
                in_running = result == "start";
                if (in_running)
                {
                    CsWithLua.Listen("MCheat", Message_Captured);
                }
            }
            else
            {
                lastError = result;
            }
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(in_running ? "stop" : "start", GUILayout.Width(button_width)))
            {
                SetRun(!in_running);
            }
            if (GUILayout.Button("clear", GUILayout.Width(button_width)))
            {
                lastError = "";
                SLIST.Clear();
            }
            show_server=GUILayout.Toggle(show_server,"S2C", GUILayout.Width(button_width));
            show_client = GUILayout.Toggle(show_client, "C2S", GUILayout.Width(button_width));
            //输入框
            m_editorTexts = EditorGUILayout.TextField(m_editorTexts, GUILayout.Width(name_width));

            m_filtertype = EditorGUILayout.IntPopup("过滤", m_filtertype, new[] {"关闭", "消息", "请求", "响应"}, 
                new[] {0, 1, 2, 3}, GUILayout.Width(name_width));
            //--GUILayout.Toggle(m_filtertype, "过滤", GUILayout.Width(button_width));

            EditorGUILayout.LabelField(lastError);

            EditorGUILayout.EndHorizontal();

            if (SLIST.Count > 0)
            {
                ShowMsg(SLIST[SLIST.Count - 1]);
            }

            v2 = EditorGUILayout.BeginScrollView(v2, true, true, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height - 40));

            foreach (string name in IGNORE)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("show", GUILayout.Width(button_width)))
                {
                    REMOVE_IGNORE.Add(name);
                    Debug.Log(v2.ToString() + " " + GUILayout.Height(this.position.height - 40).ToString());
                }
                EditorGUILayout.TextField(name, GUILayout.Width(name_width));
                EditorGUILayout.EndHorizontal();
            }

            foreach (string name in REMOVE_IGNORE)
            {
                IGNORE.Remove(name);
            }
            REMOVE_IGNORE.Clear();

            string pattern = null;
            if(m_filtertype>0 && m_editorTexts.Length >0)
            {
                pattern = m_editorTexts;
            }

            for (int i = 0; i < SLIST.Count; i++)
            {
                Msg msg = SLIST[i];
                if (!IGNORE.Contains(msg.name))
                {
                    bool isshow = true;
                    string input = m_filtertype == 1 ? msg.name : (m_filtertype == 2 ? msg.request : msg.response);
                    if (pattern != null && !System.Text.RegularExpressions.Regex.IsMatch(input, pattern))
                    {
                        isshow = false;
                    }
                    if (msg.session > 0 && show_client && isshow)
                        ShowMsg(SLIST[i]);
                    else if (msg.session == 0 && show_server && isshow)
                        ShowMsg(SLIST[i]);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        void ShowMsg(Msg msg)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("ignore", GUILayout.Width(button_width)))
            {
                IGNORE.Add(msg.name);
            }
            EditorGUILayout.TextField(msg.session + " " + msg.name, GUILayout.Width(name_width));
            if (GUILayout.Button(msg.session > 0 ? "send" : "", GUILayout.Width(button_width)))
            {
                if (msg.session > 0)
                {
                    lastError = "";
                    string result;
                    JsonData data = new JsonData();
                    data["name"] = msg.name;
                    data["request"] = msg.request;
                    if (CsWithLua.CallLua("MgrNet.SendCheat", data.ToJson(), out result))
                    {
                        if (!in_running)
                        {
                            SetRun(true);
                        }
                    }
                    else
                    {
                        lastError = result;
                    }
                }
            }
            float width = this.position.width - name_width - button_width * 2 - scrollbar_width;
            msg.request = EditorGUILayout.TextField(msg.request, GUILayout.Width(width / 2));
            EditorGUILayout.TextField(msg.response, GUILayout.Width(width / 2));
            EditorGUILayout.EndHorizontal();
        }
    }
}
