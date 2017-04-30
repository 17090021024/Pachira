using System;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web;

namespace TrunkCommon
{
    public static class WebHelper
    {
        /// <summary>
        /// 数据表转html table
        /// </summary>
        public static string ToHtmlTable(this DataTable table, string tableAttribute, bool autoTHead)
        {
            if (table == null)
                return null;

            StringBuilder html = new StringBuilder(1024);
            html.Append("<table");
            if (!string.IsNullOrEmpty(tableAttribute))
                html.Append(" ").Append(tableAttribute);
            html.Append(">");
            if (autoTHead)
            {
                html.Append("<thead><tr>");
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    html.Append("<td class=\"col_").Append(i).Append("\">");
                    html.Append(table.Columns[i].ColumnName);
                    html.Append("</td>");
                }
                html.Append("</tr></thead>");
            }
            html.Append("<tbody>");
            for (int i = 0; i < table.Rows.Count; i++)
            {
                html.Append("<tr>");
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    html.Append("<td class=\"col_").Append(j).Append("\">");
                    html.Append(table.Rows[i][j]);
                    html.Append("</td>");
                }
                html.Append("</tr>");
            }
            html.Append("</tbody></table>");
            return html.ToString();
        }

        private static byte[] PostParam(string postString)
        {
            StringBuilder postParam = new StringBuilder();
            Char[] reserved = { '?', '=', '&' };
            byte[] SomeBytes = null;
            if (postString != null)
            {
                int i = 0, j;
                while (i < postString.Length)
                {
                    j = postString.IndexOfAny(reserved, i);
                    if (j == -1)
                    {
                        postParam.Append(HttpUtility.UrlEncode(postString.Substring(i, postString.Length - i)));
                        break;
                    }
                    postParam.Append(HttpUtility.UrlEncode(postString.Substring(i, j - i)));
                    postParam.Append(postString.Substring(j, 1));
                    i = j + 1;
                }
            }
            SomeBytes = Encoding.UTF8.GetBytes(postParam.ToString());
            return SomeBytes;
        }

        public enum RequestMethod
        {
            GET,
            POST,
            PUT,
            PATCH,
            DELETE,
            COPY,
            HEAD,
            OPTIONS,
            LINK,
            UNLINK,
            PURGE,
        }

        //public static HttpWebRequest BuildHttpWebRequest(HttpRequestBase request, string url, int timeoutInSecond)
        //{
        //    if (request == null)
        //        return null;

        //    HttpWebRequest httpReq = WebRequest.Create(url) as HttpWebRequest;
        //    if (request.IsSecureConnection)
        //    {
        //        ServicePointManager.ServerCertificateValidationCallback = delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; };
        //        httpReq.ProtocolVersion = HttpVersion.Version10;
        //    }

        //    //注意：HttpWebRequire.Method默认为Get，
        //    //在写入请求前必须把HttpWebRequire.Method设置为Post,
        //    //否则在使用BeginGetRequireStream获取请求数据流的时候，系统就会发出“无法发送具有此谓词类型的内容正文”的异常。
        //    httpReq.Method = request.HttpMethod;

        //    // Copy unrestricted headers (including cookies, if any)
        //    //foreach (string headerKey in request.Headers.AllKeys)
        //    //{
        //    //    switch (headerKey)
        //    //    {
        //    //        case "Connection":
        //    //        case "Content-Length":
        //    //        case "Date":
        //    //        case "Expect":
        //    //        case "Host":
        //    //        case "If-Modified-Since":
        //    //        case "Range":
        //    //        case "Transfer-Encoding":
        //    //        case "Proxy-Connection":
        //    //            // Let IIS handle these
        //    //            break;
        //    //        case "Accept":
        //    //        case "Content-Type":
        //    //        case "Referer":
        //    //        case "User-Agent":
        //    //        //case "Cookie":
        //    //        case "Cache-Control":
        //    //            // Restricted - copied below
        //    //            break;
        //    //        default:
        //    //            httpReq.Headers[headerKey] = request.Headers[headerKey];
        //    //            break;
        //    //    }
        //    //}
        //    httpReq.ServicePoint.Expect100Continue = false;
        //    httpReq.Timeout = timeoutInSecond * 1000;
        //    // Copy restricted headers
        //    if (!request.AcceptTypes.IsNullOrEmpty())
        //    {
        //        httpReq.Accept = string.Join(",", request.AcceptTypes);
        //    }
        //    if (request.UrlReferrer != null)
        //    {
        //        httpReq.Referer = request.UrlReferrer.AbsoluteUri;
        //    }
        //    httpReq.UserAgent = request.UserAgent;
        //    httpReq.ContentLength = request.ContentLength;
        //    httpReq.ContentType = request.ContentType;
        //    httpReq.KeepAlive = !"close".Equals(request.Headers["Connection"], StringComparison.OrdinalIgnoreCase);//keep-alive
        //    httpReq.KeepAlive = false;
        //    httpReq.Connection = httpReq.Connection;
        //    DateTime ifModifiedSince;
        //    if (DateTime.TryParse(request.Headers["If-Modified-Since"], out ifModifiedSince))
        //    {
        //        httpReq.IfModifiedSince = ifModifiedSince;
        //    }
        //    string transferEncoding = request.Headers["Transfer-Encoding"];
        //    if (transferEncoding != null)
        //    {
        //        httpReq.SendChunked = true;
        //        httpReq.TransferEncoding = transferEncoding;
        //    }

        //    // Copy content (if content body is allowed)
        //    if (request.HttpMethod != WebRequestMethods.Http.Get && request.HttpMethod != WebRequestMethods.Http.Head && request.ContentLength > 0)
        //    {
        //        using (Stream httpReqStream = httpReq.GetRequestStream())
        //        {
        //            request.InputStream.CopyTo(httpReqStream, request.ContentLength);
        //            httpReqStream.Close();
        //        }
        //    }

        //    return httpReq;
        //}

        public static HttpWebRequest BuildHttpWebRequest(string url, NameValueCollection param, RequestMethod method, int timeoutInSecond, Encoding encoding, string userAgent, CookieContainer cookies, string referer, string acceptTypes, string host, NameValueCollection headers)
        {
            encoding = encoding ?? Encoding.UTF8;
            HttpWebRequest req = null;
            string fullurl = string.Empty;
            if (method != RequestMethod.POST && param != null && param.Count > 0)
                fullurl = WebUrlHelper.AppendNVP(url, param);
            else
                fullurl = url;

            req = WebRequest.Create(fullurl) as HttpWebRequest;
            if (fullurl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; };
                req.ProtocolVersion = HttpVersion.Version10;
            }

            req.Method = method.ToString();
            if (timeoutInSecond > 99)
            {
                req.Timeout = timeoutInSecond;
            }
            else
            {
                req.Timeout = timeoutInSecond * 1000;
            }
            req.ReadWriteTimeout = req.Timeout;
            req.ServicePoint.Expect100Continue = false;
            if (!string.IsNullOrEmpty(host))
                req.Host = host;
            if (!string.IsNullOrEmpty(userAgent))
                req.UserAgent = userAgent;
            if (cookies != null)
                req.CookieContainer = cookies;
            if (!string.IsNullOrEmpty(referer))
                req.Referer = referer;
            if (!string.IsNullOrEmpty(acceptTypes))
                req.Accept = acceptTypes;
            if (!headers.IsNullOrEmpty())
                req.Headers.Add(headers);
            if (method == RequestMethod.POST || method == RequestMethod.PUT)
            {
                req.ContentType = "application/x-www-form-urlencoded";
                if (param != null && param.Count > 0)
                {
                    byte[] byteArray = encoding.GetBytes(param.GetNvpString(true));
                    req.ContentLength = byteArray.Length;
                    using (System.IO.Stream stream = req.GetRequestStream())
                    {
                        stream.Write(byteArray, 0, byteArray.Length);
                        stream.Close();
                    }
                }
            }

            return req;
        }

        public static HttpWebRequest BuildHttpWebRequest4Json(string url, string param, RequestMethod method, int timeoutInSecond, Encoding encoding, string userAgent, CookieContainer cookies, string referer, string acceptTypes, string host, NameValueCollection headers)
        {
            encoding = encoding ?? Encoding.UTF8;
            HttpWebRequest req = null;
            string fullurl = url;

           // req = WebRequest.Create(fullurl) as HttpWebRequest;
            req=(HttpWebRequest)WebRequest.Create(url);
            if (fullurl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; };
                req.ProtocolVersion = HttpVersion.Version10;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            }

            req.Method = method.ToString();
            if (timeoutInSecond > 99)
            {
                req.Timeout = timeoutInSecond;
            }
            else
            {
                req.Timeout = timeoutInSecond * 1000;
            }
            req.KeepAlive = false;
            //req.ReadWriteTimeout = req.Timeout;
            //req.ServicePoint.Expect100Continue = false;
            if (!string.IsNullOrEmpty(host))
                req.Host = host;
            if (!string.IsNullOrEmpty(userAgent))
                req.UserAgent = userAgent;
            if (cookies != null)
                req.CookieContainer = cookies;
            if (!string.IsNullOrEmpty(referer))
                req.Referer = referer;
            if (!string.IsNullOrEmpty(acceptTypes))
                req.Accept = acceptTypes;
            if (!headers.IsNullOrEmpty())
                req.Headers.Add(headers);
            System.Net.ServicePointManager.Expect100Continue = false;
            System.Net.ServicePointManager.DefaultConnectionLimit = 500;
            if (method == RequestMethod.POST || method == RequestMethod.PUT)
                {
                    req.ContentType = "application/json;charset=utf-8";
                    if (param != null)
                    {
                        byte[] byteArray = encoding.GetBytes(param);
                        req.ContentLength = byteArray.Length;
                        //System.Net.ServicePointManager.DefaultConnectionLimit = 50;
                        using (System.IO.Stream stream = req.GetRequestStream())
                        {
                            stream.Write(byteArray, 0, byteArray.Length);
                            stream.Close();
                        }
                    }
                }
           
    

            return req;
        }


        /// <summary>
        /// 请求相应URL地址，错误会抛出异常
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="param">需要传递的参数，形式必须是有效的QueryString形式。(没有请填null或string.Empty)</param>
        /// <param name="method">请求方式：POST、GET</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="timeoutInSecond">请求超时时间(秒)</param>
        /// <param name="userAgent">定制用户代理信息，如：Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0)</param>
        /// <returns>应答信息</returns>
        public static string HttpRequest(string url, NameValueCollection param, RequestMethod method, int timeoutInSecond, Encoding encoding, string userAgent, CookieContainer cookies, string referer, string acceptTypes, string host, NameValueCollection headers)
        {
            HttpStatusCode responseCode = HttpStatusCode.InternalServerError;
            return HttpRequest(url, param, method, timeoutInSecond, encoding, userAgent, cookies, referer, acceptTypes, host, headers, out responseCode);
        }

        //public static string HttpRequest(string url, string param, RequestMethod method, int timeoutInSecond, Encoding encoding, string userAgent, NameValueCollection headers, out HttpStatusCode responseCode)
        //{
        //    return HttpRequest(url, param == null ? null : WebUrlHelper.ToNVC(param), method, timeoutInSecond, encoding, userAgent, headers, out responseCode);
        //}

        public static string HttpRequest(string url, NameValueCollection param, RequestMethod method, int timeoutInSecond, Encoding encoding, string userAgent, CookieContainer cookies, string referer, string acceptTypes, string host, NameValueCollection headers, out HttpStatusCode responseCode)
        {
            Stream stream = null;
            HttpWebResponse resp = null;
            try
            {
                resp = HttpResponse(url, param, method, timeoutInSecond, encoding, userAgent, cookies, referer, acceptTypes, host, headers);
                if (resp == null)
                {
                    responseCode = HttpStatusCode.InternalServerError;
                    return null;
                }
                responseCode = resp.StatusCode;

                if (resp.ContentEncoding.Contains("gzip", false))
                    stream = new GZipStream(resp.GetResponseStream(), CompressionMode.Decompress);
                else if (resp.ContentEncoding.Contains("deflate", false))
                    stream = new DeflateStream(resp.GetResponseStream(), CompressionMode.Decompress);
                else
                    stream = resp.GetResponseStream();

                using (StreamReader sr = new System.IO.StreamReader(stream, encoding))
                {
                    url = sr.ReadToEnd();
                    sr.Close();
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
                if (resp != null)
                {
                    resp.Close();
                    resp = null;
                }
            }
            return url;
        }
        public static string HttpRequest4Json(string url, string param, RequestMethod method, int timeoutInSecond, Encoding encoding, string userAgent, CookieContainer cookies, string referer, string acceptTypes, string host, NameValueCollection headers)
        {
            HttpStatusCode responseCode = HttpStatusCode.InternalServerError;
            return HttpRequest4Json(url, param, method, timeoutInSecond, encoding, userAgent, cookies, referer, acceptTypes, host, headers, out responseCode);
        }
        public static string HttpRequest4Json(string url, string param, RequestMethod method, int timeoutInSecond, Encoding encoding, string userAgent, CookieContainer cookies, string referer, string acceptTypes, string host, NameValueCollection headers, out HttpStatusCode responseCode)
        {
            Stream stream = null;
            HttpWebResponse resp = null;
            try
            {
                resp = HttpResponse4Json(url, param, method, timeoutInSecond, encoding, userAgent, cookies, referer, acceptTypes, host, headers);
                if (resp == null)
                {
                    responseCode = HttpStatusCode.InternalServerError;
                    return null;
                }
                responseCode = resp.StatusCode;

                if (resp.ContentEncoding.Contains("gzip", false))
                    stream = new GZipStream(resp.GetResponseStream(), CompressionMode.Decompress);
                else if (resp.ContentEncoding.Contains("deflate", false))
                    stream = new DeflateStream(resp.GetResponseStream(), CompressionMode.Decompress);
                else
                    stream = resp.GetResponseStream();

                using (StreamReader sr = new System.IO.StreamReader(stream, encoding))
                {
                    url = sr.ReadToEnd();
                    sr.Close();
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
                if (resp != null)
                {
                    resp.Close();
                    resp = null;
                }
            }
            return url;
        }

        /// <summary>
        /// 请求相应URL地址，返回应答对象，错误会抛出异常
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="method"></param>
        /// <param name="timeoutInSecond"></param>
        /// <param name="encoding"></param>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public static HttpWebResponse HttpResponse(string url, NameValueCollection param, RequestMethod method, int timeoutInSecond, Encoding encoding, string userAgent, CookieContainer cookies, string referer, string acceptTypes, string host, NameValueCollection headers)
        {
            return HttpResponse(BuildHttpWebRequest(url, param, method, timeoutInSecond, encoding, userAgent, cookies, referer, acceptTypes, host, headers));
        }

        /// <summary>
        /// 请求相应URL地址，返回应答对象，错误会抛出异常
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="method"></param>
        /// <param name="timeoutInSecond"></param>
        /// <param name="encoding"></param>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public static HttpWebResponse HttpResponse4Json(string url, string param, RequestMethod method, int timeoutInSecond, Encoding encoding, string userAgent, CookieContainer cookies, string referer, string acceptTypes, string host, NameValueCollection headers)
        {
            return HttpResponse(BuildHttpWebRequest4Json(url, param, method, timeoutInSecond, encoding, userAgent, cookies, referer, acceptTypes, host, headers));
        }

        public static HttpWebResponse HttpResponse(HttpWebRequest request)
        {
            HttpWebResponse resp = null;
            try
            {
                resp = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                if (resp != null)
                {
                    resp.Close();
                }
                return ex.Response as HttpWebResponse;
            }
            return resp;
        }

        #region 异步请求相应URL地址
        public static void HttpAsyncRequest(HttpWebRequest request, Encoding responseEncoding, int timeoutInSecond, ResponseResult responseResult)
        {
            HttpAsyncRequestState requestState = new HttpAsyncRequestState()
            {
                Request = request,
                RespResult = responseResult,
                RespEncoding = responseEncoding,
            };

            try
            {
                // Start the asynchronous request.
                IAsyncResult asyncResult = request.BeginGetResponse(HttpAsyncRequest_RespCallback, requestState);

                ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, new WaitOrTimerCallback(HttpAsyncRequest_TimeoutCallback), requestState, timeoutInSecond * 1000, true);
            }
            catch (WebException ex)
            {
                if (requestState.RespResult != null)
                {
                    requestState.RespResult(null, ex.Response as HttpWebResponse, ex);
                }
            }
        }
        /// <summary>
        /// 异步请求相应URL地址
        /// </summary>
        public static void HttpAsyncRequest(string url, NameValueCollection param, RequestMethod method, int timeoutInSecond, Encoding encoding, string userAgent, CookieContainer cookies, string referer, string acceptTypes, string host, NameValueCollection headers, ResponseResult responseResult)
        {
            HttpAsyncRequest(BuildHttpWebRequest(url, param, method, timeoutInSecond, encoding, userAgent, cookies, referer, acceptTypes, host, headers), encoding, timeoutInSecond, responseResult);
        }
        /// <summary>
        /// 异步请求应答返回委托
        /// </summary>
        /// <param name="responseResult"></param>
        /// <param name="response"></param>
        /// <param name="ex"></param>
        public delegate void ResponseResult(string responseResult, WebResponse response, Exception ex);
        private class HttpAsyncRequestState
        {
            public ResponseResult RespResult = null;
            public HttpWebRequest Request = null;
            public Encoding RespEncoding = null;
        }
        private static void HttpAsyncRequest_RespCallback(IAsyncResult asyncResult)
        {
            WebResponse resp = null;
            HttpAsyncRequestState requestState = null;
            try
            {
                string result = null;

                requestState = asyncResult.AsyncState as HttpAsyncRequestState;
                resp = requestState.Request.EndGetResponse(asyncResult);

                using (StreamReader sr = new StreamReader(resp.GetResponseStream(), requestState.RespEncoding))
                {
                    result = sr.ReadToEnd();
                    sr.Close();
                }

                if (requestState.RespResult != null)
                {
                    requestState.RespResult(result, resp, null);
                }
            }
            catch (Exception ex)
            {
                if (requestState != null && requestState.RespResult != null)
                {
                    requestState.RespResult(null, resp, ex);
                }
            }
            finally
            {
                if (resp != null)
                {
                    resp.Close();
                    resp = null;
                }
            }
        }
        private static void HttpAsyncRequest_TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                HttpAsyncRequestState requestState = state as HttpAsyncRequestState;
                if (requestState != null)
                {
                    requestState.Request.Abort();
                }
            }
        }

        #endregion

        /// <summary>
        /// 发送请求并跳转至末页
        /// </summary>
        /// <param name="response"></param>
        /// <param name="url"></param>
        /// <param name="postParams"></param>
        /// <param name="target"></param>
        public static void FormSubmit(HttpResponse response, string url, NameValueCollection postParams, string target, bool endResponse)
        {
            response.ClearContent();
            response.Write("<!DOCTYPE html><html><head><style type=\"text/css\">html,body{margin:0;padding:0;overflow:hidden}#s{display:none}</style></head><body><form id=\"f\" name=\"f\" method=\"post\" action=\"");
            response.Write(url);
            response.Write("\" target=\"");
            response.Write(target);
            response.Write("\">");
            for (int i = 0; i < postParams.Count; i++)
            {
                response.Write("<input type=\"hidden\" name=\"");
                response.Write(postParams.GetKey(i));
                response.Write("\" value=\"");
                response.Write(postParams.Get(i));
                response.Write("\" />");
            }
            response.Write("<input id=\"s\" type=\"submit\" value=\"\" onfocus=\"this.blur()\" /></form><script type=\"text/javascript\">document.getElementById(\"f\").submit();</script></body></html>");
            if (endResponse)
                response.End();
        }


        /// <summary>
        /// 返回Request的Header内容
        /// </summary>
        /// <param name="format">格式，{0}为Key{1}为Value</param>
        public static string GetRequestHeader(string format)
        {
            StringBuilder buffer = new StringBuilder(256);
            for (int i = 0; i < HttpContext.Current.Request.Headers.Count; i++)
                buffer.AppendFormat(format, HttpContext.Current.Request.Headers.Keys[i], HttpContext.Current.Request.Headers[i]);
            return buffer.ToString();
        }

        /// <summary>
        /// 获取request对象参数的字符串
        /// </summary>
        /// <param name="format">格式，{0}为Key{1}为Value</param>
        public static string GetRequestParams(string format)
        {
            HttpRequest request = HttpContext.Current.Request;
            StringBuilder buffer = new StringBuilder(1024);
            for (int i = 0; i < request.Params.Count; i++)
                buffer.AppendFormat(format, request.Params.Keys[i], request.Params[i]);
            return buffer.ToString();
        }

        /// <summary>
        /// 获取request对象QueryString,Form,Cookies,AbsoluteUri,Time,HttpMethod
        /// </summary>
        /// <param name="format">格式，{0}为Key{1}为Value</param>
        public static string GetRequestInfo(string format)
        {
            HttpRequest request = HttpContext.Current.Request;
            StringBuilder buffer = new StringBuilder(1024);
            buffer.AppendFormat(format, "Time", DateTime.Now);
            buffer.AppendFormat(format, "ClientIp", string.Join(",", GetClientIP()));
            buffer.AppendFormat(format, "ServerIp", GetServerIP());
            buffer.AppendFormat(format, "HttpMethod", request.HttpMethod);
            buffer.AppendFormat(format, "AbsoluteUri", HttpUtility.UrlDecode(request.Url.AbsoluteUri));
            buffer.AppendFormat(format, "QueryString", HttpUtility.UrlDecode(request.QueryString.ToString()));
            buffer.AppendFormat(format, "Form", HttpUtility.UrlDecode(request.Form.ToString()));
            buffer.AppendFormat(format, "Headers", HttpUtility.UrlDecode(request.Headers.ToString()));
            return buffer.ToString();
        }


        /// <summary>
        /// 返回访问者IP地址数组，如果没有则返回new string[1]
        /// </summary>
        /// <returns></returns>
        public static string[] GetClientIP()
        {
            HttpRequest request = HttpContext.Current.Request;

            string tmp = request.Headers["X-FORWARDED-FOR"];

            if (string.IsNullOrEmpty(tmp))
            {
                tmp = request.ServerVariables["REMOTE_ADDR"];
            }

            if (string.IsNullOrEmpty(tmp))
            {
                tmp = request.ServerVariables["HTTP_VIA"];
            }

            if (string.IsNullOrEmpty(tmp))
            {
                tmp = request.UserHostAddress;
            }

            if (!string.IsNullOrEmpty(tmp))
            {
                return tmp.Split(new char[] { ',', ';', '-', '_', ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);
            }

            return new string[1];
        }

        /// <summary>
        /// 返回用户真实IP（只会一个）
        /// </summary>
        /// <returns></returns>
        public static string GetUserClientIP()
        {
            return GetClientIP()[0];
        }

        /// <summary>
        /// 获取Web服务器的IP
        /// </summary>
        /// <returns></returns>
        public static string GetServerIP()
        {
            if (ServerIPCache != null)
                return ServerIPCache;

            if (HttpContext.Current == null)
                return string.Empty;

            return ServerIPCache = HttpContext.Current.Request.ServerVariables["LOCAL_ADDR"];
        }
        private static string ServerIPCache = null;

        /// <summary>
        /// 获取16进制的计算机IP
        /// </summary>
        /// <param name="x0Mode"></param>
        /// <returns></returns>
        public static string GetServerIP(bool x0Mode)
        {
            if (x0Mode)
            {
                if (ServerIPCacheX0 != null)
                    return ServerIPCacheX0;

                string ip = GetServerIP();
                if (!ip.IsNullOrEmpty())
                {
                    byte[] ipAddress = IPAddress.Parse(ip).GetAddressBytes();
                    return ServerIPCacheX0 = string.Concat(ipAddress[0].ToString("X2"), ipAddress[1].ToString("X2"), ipAddress[2].ToString("X2"), ipAddress[3].ToString("X2"));
                }
                return string.Empty;
            }
            else
                return GetServerIP();
        }
        private static string ServerIPCacheX0 = null;

        #region Cookie操作
        /// <summary>
        /// 获取cookie中的单个值，如果cookie不存在则返回null
        /// </summary>
        public static string GetCookie(string cookieName)
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null || cookieName == null)
                return null;
            System.Web.HttpCookie cookie = null;
            foreach (string item in HttpContext.Current.Response.Cookies)
            {
                if (cookieName.Equals(item, StringComparison.CurrentCultureIgnoreCase))
                {
                    cookie = HttpContext.Current.Response.Cookies[item];
                    break;
                }
            }

            if (cookie == null)
                cookie = HttpContext.Current.Request.Cookies[cookieName];

            if (cookie == null)
                return null;
            else
                return HttpUtility.UrlDecode(cookie.Value);
        }

        /// <summary>
        /// 新建或更新cookie值，如果expires为DateTime.MinValue,则为浏览器生命期cookie
        /// </summary>
        public static void SetCookie(string cookieName, string value, DateTime expires, string domain, string path, bool secure, bool httpOnly)
        {
            if (HttpContext.Current == null || cookieName == null)
                return;

            System.Web.HttpCookie cookie = new System.Web.HttpCookie(cookieName);
            cookie.Expires = expires;
            cookie.Value = HttpUtility.UrlEncode(value);
            cookie.Path = string.IsNullOrEmpty(path) ? "/" : path;
            cookie.Domain = domain;
            cookie.Secure = secure;
            cookie.HttpOnly = httpOnly;
            HttpContext.Current.Response.Cookies.Set(cookie);
        }

        /// <summary>
        /// 删除Cookie
        /// </summary>
        public static void DelCookie(string cookieName, string domain, string path, bool secure)
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null || cookieName == null)
                return;
            System.Web.HttpCookie cookie = HttpContext.Current.Request.Cookies[cookieName];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1d);
                cookie.Path = string.IsNullOrEmpty(path) ? "/" : path;
                cookie.Domain = domain;
                cookie.Secure = secure;
                HttpContext.Current.Response.Cookies.Set(cookie);
            }
        }

        /// <summary>
        /// 检查cookie是否存在
        /// </summary>
        public static bool ExistsCookie(string cookieName)
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null || cookieName == null)
                return false;

            foreach (string item in HttpContext.Current.Response.Cookies)
            {
                if (cookieName.Equals(item, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }

            return HttpContext.Current.Request.Cookies[cookieName] != null;
        }
        /// <summary>
        /// 清除该域名下的所有cookie
        /// </summary>
        /// <param name="domain"></param>
        public static void ClearCookie(string domain)
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null)
                return;

            HttpCookie cookie = null;
            for (int i = 0; i < HttpContext.Current.Request.Cookies.Count; i++)
            {
                cookie = HttpContext.Current.Request.Cookies[i];
                DelCookie(cookie.Name, domain, null, cookie.Secure);
            }
        }

         public static string GetResponseText(string url, int timeOut, string data, string method, string[] headers)
        {
            string responseText = null;
            //得到最终请求方法
            try
            {
                if (method != null && (method.Equals("POST", StringComparison.InvariantCultureIgnoreCase)
                            || method.Equals("PUT", StringComparison.InvariantCultureIgnoreCase)
                            || method.Equals("DELETE", StringComparison.InvariantCultureIgnoreCase)))
                {
                    method = method.ToUpper();
                }
                else
                {
                    method = "GET";
                }

                //得到最终Url
                if (method == "GET" && !string.IsNullOrEmpty(data))
                {
                    if (url.IndexOf('?') > 0)
                    {
                        url = url + "&" + data;
                    }
                    else
                    {
                        url = url + "?" + data;
                        if (url.IndexOf("\"") > 0)
                        {
                            url = url.Replace("\"", "");
                        }
                    }
                }


                //创建HttpRequest请求对象
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //真实代码
                request.Timeout = timeOut;

                request.Method = method;
                //添加请求头
                if (method == "POST")
                {
                    request.ContentType = "application/json";
                }

                //为POST请求添加请求正文
                if (method == "POST" && !string.IsNullOrEmpty(data))
                {

                    byte[] bytes = Encoding.UTF8.GetBytes(data);
                    request.ContentLength = bytes.Length;
                    using (Stream reqStream = request.GetRequestStream())
                    {
                        reqStream.Write(bytes, 0, bytes.Length);
                        reqStream.Close();
                    }
                }
                //得到响应流
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            responseText = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "respose异常"+"2222222"+ex.Message+"}}}}}}}"+ex.StackTrace;
            }
            return responseText;
        }
        #endregion
    }//end class
}//end namespace