using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public delegate void DF_UWR(bool isSuccess, UnityWebRequest uwr, object pars);

namespace Core.Kernel
{
    /// <summary>
    /// 类名 : https 验证对象
    /// 作者 : Canyon
    /// 日期 : 2021-01-05 09:33
    /// 功能 : 
    /// </summary>
    public class WebVerifyCert : CertificateHandler
    {
        static public readonly WebVerifyCert NoVCert = new WebVerifyCert(false);

        public WebVerifyCert():base()
        {
        }

        public WebVerifyCert(bool isVerify) : this()
        {
            this.m_isVerify = isVerify;
        }

        public bool m_isVerify { get; set; }

        protected override bool ValidateCertificate(byte[] certificateData)
        {
            if (this.m_isVerify)
                return base.ValidateCertificate(certificateData);
            return true;
        }
    }

    /// <summary>
    /// 类名 : ut-unity 资源下载管理
    /// 作者 : Canyon
    /// 日期 : 2017-05-17 15:03
    /// 功能 : 2020-02-03 19:21 加入 UnityWebRequest 来替换 www 请求
    /// </summary>
    public class WWWMgr : MonoBehaviour
    {
        static WWWMgr _instance;
        static public WWWMgr instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject _gobj = GameMgr.mgrGobj;
                    _instance = GHelper.Get<WWWMgr>(_gobj, true);
                }
                return _instance;
            }
        }

        IEnumerator Get(string url, DF_UWR callFunc, WebVerifyCert vcert = null, object pars = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                yield break;
            }
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return UWRCoroutine(request, callFunc, vcert, pars);
            }
        }

        IEnumerator PostForm(string url, WWWForm form, DF_UWR callFunc, WebVerifyCert vcert = null, object pars = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            {
                yield return UWRCoroutine(request, callFunc, vcert, pars);
            }
        }

        IEnumerator PostJson(string url, string body, DF_UWR callFunc, WebVerifyCert vcert = null, object pars = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                yield break;
            }

            using (var request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("content-type", "application/json;charset=utf-8");
                yield return UWRCoroutine(request, callFunc, vcert, pars);
            }
        }

        private IEnumerator UWRCoroutine(UnityWebRequest request, DF_UWR callFunc, WebVerifyCert vcert = null, object pars = null)
        {
            //设置超时 链接超时返回 且isNetworkError为true
            request.timeout = 59;
            request.certificateHandler = (vcert != null) ? vcert : WebVerifyCert.NoVCert;
            request.useHttpContinue = false;
            yield return request.SendWebRequest();
            //结果回传给具体实现
            if (request.isHttpError || request.isNetworkError)
            {
                if (callFunc != null)
                {
                    callFunc(false, request, pars);
                }
            }
            else
            {
                // request.downloadHandler
                if (callFunc != null)
                {
                    callFunc(true, request, pars);
                }
            }
        }

        public void StartUWR(string url, DF_UWR callFunc, WebVerifyCert vcert = null, object extPars = null)
        {
            StartCoroutine(Get(url, callFunc, vcert, extPars));
        }

        public void StartUWRPost(string url, WWWForm form, DF_UWR callFunc, WebVerifyCert vcert = null, object extPars = null)
        {
            StartCoroutine(PostForm(url, form, callFunc, vcert, extPars));
        }

        public void StartJsonUWR(string url, string dataJson, DF_UWR callFunc, WebVerifyCert vcert = null, object extPars = null)
        {
            StartCoroutine(PostJson(url, dataJson, callFunc, vcert, extPars));
        }
    }
}