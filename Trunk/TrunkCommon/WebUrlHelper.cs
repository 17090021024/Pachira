using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

namespace TrunkCommon
{
    public static class WebUrlHelper
    {
        public static readonly Uri URI_EMPTY = new Uri("about:blank");

        /// <summary>
        /// 获取来源地址，如果没有或来源地址无效则返回URI_EMPTY
        /// </summary>
        public static Uri RequestUrlReferrer
        {
            get
            {
                if (HttpContext.Current == null)
                    return URI_EMPTY;

                string referer = HttpContext.Current.Request.Headers["Referer"];
                if (referer == null)
                    return URI_EMPTY;

                Uri result = null;
                return Uri.TryCreate(referer, UriKind.Absolute, out result) ? result : URI_EMPTY;
            }
        }
        /// <summary>
        /// 域名后缀（只能是首扩展）
        /// </summary>
        public static readonly string[] DomainExt = new string[]
        {
            "com","net","org","int","edu","gov","mil","arpa","asia","biz","info","name","pro","coop","aero","museum","idv","公司","中国","网络"
        };
        /// <summary>
        /// 域名后缀扩展（可以是首扩展，也可作为第二扩展）
        /// </summary>
        public static readonly string[] DomainExtX = new string[]
        {
            "ac","ad","ae","af","ag","ai","al","am","an","ao","aq","ar","as","at","au","aw","az",
            "ba","bb","bd","be","bf","bg","bh","bi","bj","bm","bn","bo","br","bs","bt","bv","bw","by","bz",
            "ca","cc","cf","cg","ch","ci","ck","cl","cm","cn","co","cq","cr","cu","cv","cx","cy","cz",
            "de","dj","dk","dm","do","dz",
            "ec","ee","eg","eh","es","et","ev",
            "fi","fj","fk","fm","fo","fr",
            "ga","gb","gd","ge","gf","gh","gi","gl","gm","gn","gp","gr","gt","gu","gw","gy",
            "hk","hm","hn","hr","ht","hu",
            "id","ie","il","in","io","iq","ir","is","it",
            "jm","jo","jp",
            "ke","kg","kh","ki","km","kn","kp","kr","kw","ky","kz",
            "la","lb","lc","li","lk","lr","ls","lt","lu","lv","ly",
            "ma","mc","md","me","mg","mh","ml","mm","mn","mo","mp","mq","mr","ms","mt","mv","mw","mx","my","mz",
            "na","nc","ne","nf","ng","ni","nl","no","np","nr","nt","nu","nz",
            "om",
            "pa","pe","pf","pg","ph","pk","pl","pm","pn","pr","pt","pw","py",
            "qa",
            "re","ro","ru","rw",
            "sa","sb","sc","sd","se","sg","sh","si","sj","sk","sl","sm","sn","so","sr","st","su","sy","sz",
            "tc","td","tf","tg","th","tj","tk","tm","tn","to","tp","tr","tt","tv","tw","tz",
            "ua","ug","uk","us","uy","va",
            "vc","ve","vg","vn","vu",
            "wf","ws",
            "ye","yu",
            "za","zm","zr","zw",
        };
        private static HashSet<string> DomainExtSet = null;
        private static HashSet<string> DomainExtXSet = null;
        /// <summary>
        /// 获取URL中的域名部分(无效返回null)
        /// </summary>
        /// <param name="level">0：完整HOST，1：一级域名，2：二级域名...</param>
        /// <returns></returns>
        public static string GetDomain(string url, int level)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            Uri uri = null;
            if (url.IndexOf("://") == -1)
                url = "http://" + url;

            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
                uri = null;
            return GetDomain(uri, level);
        }
        /// <summary>
        /// 获取URL中的域名部分(无效返回null)
        /// <param name="level">0：完整HOST，1：一级域名，2：二级域名...</param>
        /// </summary>
        public static string GetDomain(Uri uri, int level)
        {
            if (uri == null || string.IsNullOrEmpty(uri.Host) || !uri.IsAbsoluteUri)
                return null;

            if (level <= 0 || uri.HostNameType != UriHostNameType.Dns)
                return uri.Host;

            string[] domainTokens = uri.Host.ToLower().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (domainTokens == null || domainTokens.Length <= 2)//只有一个点或没有点的域名只可能是1级域名
                return uri.Host;
            else //域名中含有2个点或以上
            {
                if (DomainExtSet == null)
                {
                    DomainExtSet = new HashSet<string>();
                    foreach (string item in DomainExt)
                        DomainExtSet.Add(item);
                }
                if (DomainExtXSet == null)
                {
                    DomainExtXSet = new HashSet<string>();
                    foreach (string item in DomainExtX)
                        DomainExtXSet.Add(item);
                }
                string domainNoExt = uri.Host;
                string domainExt = null;
                int tmpIdx = 0;
                if (DomainExtSet.Contains(domainTokens[domainTokens.Length - 1]))//结尾是首扩展后缀，则前面的一定就是域名名字
                {
                    domainExt = string.Concat(".", domainTokens[domainTokens.Length - 1]);
                    tmpIdx = domainNoExt.LastIndexOf(domainExt, StringComparison.CurrentCultureIgnoreCase);
                    if (tmpIdx != -1)
                        domainNoExt = domainNoExt.Substring(0, tmpIdx);
                }
                else if (DomainExtXSet.Contains(domainTokens[domainTokens.Length - 1]))//结尾不是首扩展后缀,而是X扩展后缀,就会有两种情况了
                {
                    if (DomainExtSet.Contains(domainTokens[domainTokens.Length - 2]))//X扩展前是首扩展后缀
                        domainExt = string.Concat(".", domainTokens[domainTokens.Length - 2], ".", domainTokens[domainTokens.Length - 1]);
                    else//X扩展前如果不是首扩展，则一定就是域名名字了
                        domainExt = string.Concat(".", domainTokens[domainTokens.Length - 1]);
                    tmpIdx = domainNoExt.LastIndexOf(domainExt, StringComparison.CurrentCultureIgnoreCase);
                    if (tmpIdx != -1)
                        domainNoExt = domainNoExt.Substring(0, tmpIdx);
                }

                if (string.IsNullOrEmpty(domainExt))//未知的域名扩展，暂判断为无扩展
                    return uri.Host;
                else
                {
                    domainTokens = domainNoExt.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (domainTokens == null || domainTokens.Length <= 1 || level >= domainTokens.Length)
                        return uri.Host;
                    else
                    {
                        for (int i = 0; i < domainTokens.Length - level; i++)
                            domainTokens[i] = null;
                        return string.Join(".", domainTokens).TrimStart('.') + domainExt;
                    }
                }
            }
        }

        /// <summary>
        /// 获取站点物理根路径(结尾处含“\”)
        /// </summary>
        /// <returns></returns>
        public static string WebPhysicalRoot
        {
            get
            {
                return System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
            }
        }
        /// <summary>
        /// 转换为绝对物理路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string MapPhysicalPath(params string[] path)
        {
            if (path.IsNullOrEmpty())
                return WebPhysicalRoot;

            return string.Concat(path)
                .Replace("/", "\\")
                .Replace("~\\", WebPhysicalRoot);
        }

        /// <summary>
        /// 获取站点根目录(结尾处不含“/”)。注意，此路径不包含虚拟目录(含域名)
        /// </summary>
        /// <returns></returns>
        public static string WebRoot
        {
            get
            {
                Uri uri = null;
                try
                {
                    uri = HttpContext.Current.Request.Url;
                }
                catch
                {
                    uri = null;
                }
                if (uri == null)
                    return "/";

                if (uri.Port == 80)
                    return string.Concat(uri.Scheme, "://", uri.Host);
                else
                    return string.Concat(uri.Scheme, "://", uri.Host, ":", uri.Port);
            }
        }

        /// <summary>
        /// 获取站点虚拟目录根路径，没有虚拟路径则为网站根目录（结尾处不含“/”）(含域名)
        /// </summary>
        public static string WebVirtualRoot
        {
            get
            {
                return string.Concat(WebRoot, System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath.TrimEnd('/'));
            }
        }

        /// <summary>
        /// 判断上一次请求地址是否是其它主机（非本服务器Host名称）,如果返回true，Request.UrlReferrer.Host即为上次请求的主机名称。主要用于判断是否是从其他网站跳转过来的
        /// </summary>
        /// <returns></returns>
        public static bool IsReferrerdByOtherHost
        {
            get
            {
                Uri refererUri = RequestUrlReferrer;
                return refererUri != null && !refererUri.Host.Equals(HttpContext.Current.Request.Url.Host, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        /// <summary>
        /// 获取文件后缀名
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public static string GetExtension(string virtualPath)
        {
            if (virtualPath == null)
                return string.Empty;

            int startIndex = -1;
            int endIndex = virtualPath.Length;
            char ch;
            for (int i = endIndex - 1; i >= 0; i--)
            {
                ch = virtualPath[i];
                if (ch == '.')
                {
                    startIndex = i;
                    break;
                }
                if (ch == '/')
                    return string.Empty;
                if (!char.IsLetterOrDigit(ch))
                    endIndex = i;
            }
            if (startIndex == -1 || startIndex >= endIndex - 1)
                return string.Empty;
            return virtualPath.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// 静态文件后缀名
        /// </summary>
        public const string WebStaticResourcesExtension = "|.css|.js|.htm|.html|.xhtm|.png|.jpg|.jpeg|.gif|.bmp|.ico|.icon|.txt|.ini|.prop|.log|.xml|.config|.zip|.7z|.rar|.apk|.ipa|.mp3|.avi|.flv|.mpeg|.mpg|";

        /// <summary>
        /// 资源文件是否为静态文件
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsWebStaticResources(string url)
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null)
                return true;

            url = GetExtension(url);
            if (string.IsNullOrEmpty(url))
                return false;

            return WebStaticResourcesExtension.IndexOf(url, StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        /// <summary>
        /// 资源文件是否为静态文件
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsWebStaticResources()
        {
            return IsWebStaticResources(null);
        }

        /// <summary>
        /// 将NVC转成JSON格式字符串
        /// </summary>
        /// <param name="nvc"></param>
        /// <returns></returns>
        public static string ToJson(this NameValueCollection nvc)
        {
            if (nvc == null)
                return string.Empty;

            StringBuilder paramBuffer = new StringBuilder(32 + nvc.Count * 16);
            paramBuffer.Append("{");
            string value = null;
            for (int i = 0; i < nvc.Count; i++)
            {
                value = nvc[i];
                if (i != 0)
                    paramBuffer.Append(",");
                paramBuffer.Append("\"").Append(nvc.Keys[i]).Append("\":\"").Append(value == null ? "null" : value.Replace("\"", "\\\"")).Append("\"");
            }
            paramBuffer.Append("}");
            return paramBuffer.ToString();
        }

        public static string GetNvpString(this NameValueCollection parameters, bool escapeValue)
        {
            if (parameters == null)
                return string.Empty;

            StringBuilder paramBuffer = new StringBuilder(32 + parameters.Count * 16);
            for (int i = 0; i < parameters.Count; i++)
            {
                paramBuffer
                    .Append(parameters.Keys[i])
                    .Append('=')
                    .Append(escapeValue ? HttpUtility.UrlEncode(parameters[i]) : parameters[i])
                    .Append('&');
            }

            if (paramBuffer.Length > 0)
                return paramBuffer.ToString(0, paramBuffer.Length - 1);
            else
                return string.Empty;
        }

        /// <summary>
        /// 通过参数集合生成nvp字符串
        /// </summary>
        /// <param name="escapeValue">是否转码值</param>
        public static string GetNvpString(this IEnumerable<KeyValuePair<string, string>> parameters, bool escapeValue)
        {
            if (parameters == null)
                return string.Empty;

            StringBuilder paramBuffer = new StringBuilder(128);
            foreach (KeyValuePair<string, string> p in parameters)
            {
                paramBuffer
                    .Append(p.Key)
                    .Append('=')
                    .Append(escapeValue ? HttpUtility.UrlEncode(p.Value) : p.Value)
                    .Append('&');
            }

            if (paramBuffer.Length > 0)
                return paramBuffer.ToString(0, paramBuffer.Length - 1);
            else
                return string.Empty;
        }

        /// <summary>
        /// 通过CookieCollection集合生成nvp字符串
        /// </summary>
        /// <param name="escapeValue">是否转码值</param>
        public static string GetNvpString(this System.Web.HttpCookieCollection cookies, bool escapeValue)
        {
            if (cookies.Count == 0)
                return string.Empty;
            StringBuilder buffer = new StringBuilder(32 + cookies.Count * 16);
            foreach (object item in cookies)
            {
                if (item.GetType() == typeof(string))
                {
                    buffer.Append(item);
                    buffer.Append('=');
                    buffer.Append(escapeValue ? HttpUtility.UrlEncode((string)item) : (string)item);
                    buffer.Append('&');
                }
                else if (item.GetType() == typeof(System.Web.HttpCookie))
                {
                    buffer.Append(((System.Web.HttpCookie)item).Name);
                    buffer.Append('=');
                    buffer.Append(escapeValue ? HttpUtility.UrlEncode(((System.Web.HttpCookie)item).Value) : ((System.Web.HttpCookie)item).Value);
                    buffer.Append('&');
                }
            }
            return buffer.ToString(0, buffer.Length - 1);
        }

        /// <summary>
        /// 排序nvc
        /// </summary>
        /// <param name="nvc"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public static string GetOrderNvpString(this NameValueCollection nvc, bool asc, bool escapeValue, bool removeEmpty)
        {
            if (nvc == null || nvc.Count == 0)
                return string.Empty;

            string[] sortedKeys = nvc.AllKeys;
            Array.Sort<string>(sortedKeys, delegate(string a, string b)
            {
                return asc ? a.CompareTo(b) : b.CompareTo(a);
            });

            string tKey = null;
            StringBuilder paramBuffer = new StringBuilder(32 + sortedKeys.Length * 16);
            for (int i = 0; i < sortedKeys.Length; i++)
            {
                tKey = sortedKeys[i];
                if (!removeEmpty || !string.IsNullOrEmpty(tKey))
                {
                    paramBuffer
                        .Append(sortedKeys[i])
                        .Append('=')
                        .Append(escapeValue ? HttpUtility.UrlEncode(nvc[tKey]) : nvc[tKey])
                        .Append('&');
                }
            }

            if (paramBuffer.Length > 0)
                return paramBuffer.ToString(0, paramBuffer.Length - 1);
            else
                return string.Empty;
        }

        /// <summary>
        /// 将nvp格式的查询字符串解析为NameValueCollection集合,并自动解码
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static NameValueCollection ParseQueryString(this string query)
        {
            return ParseQueryString(query, Encoding.UTF8);
        }
        /// <summary>
        /// 将nvp格式的查询字符串解析为NameValueCollection集合,并自动解码
        /// </summary>
        /// <param name="query"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static NameValueCollection ParseQueryString(this string query, Encoding encoding)
        {
            if (string.IsNullOrEmpty(query))
                return new NameValueCollection();
            return System.Web.HttpUtility.ParseQueryString(query, encoding);
        }

        /// <summary>
        /// 将nvp格式的查询字符串解析为NameValueCollection集合,不会解码
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static NameValueCollection ToNVC(string query)
        {
            NameValueCollection nvc = new NameValueCollection();

            int num = (query == null) ? 0 : query.Length;
            int tmp = -1;
            int startIndex = 0;
            char ch = char.MinValue;
            string key = null;
            string value = null;

            for (int i = 0; i < num; i++)
            {
                startIndex = i;
                tmp = -1;
                while (i < num)
                {
                    ch = query[i];
                    if (ch == '=')
                    {
                        if (tmp < 0)
                            tmp = i;
                    }
                    else if (ch == '&')
                        break;
                    i++;
                }

                key = null;
                value = null;

                if (tmp >= 0)
                {
                    key = query.Substring(startIndex, tmp - startIndex);
                    value = query.Substring(tmp + 1, (i - tmp) - 1);
                }
                else
                    value = query.Substring(startIndex, i - startIndex);

                nvc.Add(key, value);

                if ((i == (num - 1)) && (query[i] == '&'))
                    nvc.Add(null, string.Empty);
            }

            return nvc;
        }

        /// <summary>
        /// 获取nvc中的值
        /// </summary>
        /// <param name="nvc"></param>
        /// <param name="name"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static string GetQueryStringValue(this NameValueCollection nvc, string name, string defValue)
        {
            if (nvc == null || nvc.Count == 0 || string.IsNullOrEmpty(name))
                return defValue;

            return nvc[name] ?? defValue;
        }
        /// <summary>
        /// 获取nvp字符串中的值
        /// </summary>
        /// <param name="nvpString"></param>
        /// <param name="name"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static string GetQueryStringValue(string nvpString, string name, string defValue)
        {
            if (string.IsNullOrEmpty(nvpString) || string.IsNullOrEmpty(name))
                return defValue;

            if (nvpString.Contains("?"))
            {
                nvpString = nvpString.Split('?')[1];
            }

            return GetQueryStringValue(ParseQueryString(nvpString), name, defValue);
        }

        /// <summary>
        /// 将带有~的路径转为实际web绝对路径(不含域名部分)
        /// </summary>
        public static string GetAbsolutUrl(params object[] url)
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null)
                return string.Empty;
            if (url.IsNullOrEmpty())
                return HttpContext.Current.Request.ApplicationPath;
            return string.Concat(url).Replace("~", HttpContext.Current.Request.ApplicationPath.TrimEnd('/'));
        }
        /// <summary>
        /// 将带有~的路径转为实际web完整绝对路径(含域名部分)
        /// </summary>
        public static string GetFullAbsolutUrl(params object[] url)
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null)
                return string.Empty;
            if (url.IsNullOrEmpty())
                return WebVirtualRoot;

            string urlstr = string.Concat(url);
            if (string.IsNullOrEmpty(urlstr))
                return WebVirtualRoot;

            if (urlstr.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
                return urlstr;

            if (urlstr.StartsWith("~"))
                return urlstr.Replace("~", WebVirtualRoot);

            return string.Concat(WebVirtualRoot, urlstr[0] == '/' ? string.Empty : "/", urlstr);
        }

        /// <summary>
        /// URL地址中是否含有此参数
        /// </summary>
        public static bool ExistsNVP(string url, string name)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            return url.IndexOf(string.Concat('?', name, '='), StringComparison.CurrentCultureIgnoreCase) != -1
                || url.IndexOf(string.Concat('&', name, '='), StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        /// <summary>
        /// 给URL地址添加参数
        /// </summary>
        public static string AppendNVP(string url, string name, string value)
        {
            if (url == null)
                return string.Concat("?", name, "=", HttpUtility.UrlEncode(value));

            if (ExistsNVP(url, name))
                url = RemoveNVP(url, name);

            url = url.TrimEnd('?', '#', '&');
            return string.Concat(url, url.IndexOf('?') == -1 ? '?' : '&', name, (string.IsNullOrEmpty(name) ? string.Empty : "="), HttpUtility.UrlEncode(value));
        }

        /// <summary>
        /// 给URL地址添加一个或多个参数
        /// </summary>
        public static string AppendNVP(string url, NameValueCollection nvc)
        {
            return AppendNVP(url, nvc, null);
        }
        /// <summary>
        /// 给URL地址添加一个或多个限制前缀的参数
        /// </summary>
        public static string AppendNVP(string url, NameValueCollection nvc, string startsWith)
        {
            if (nvc == null || nvc.Count == 0)
                return url;

            StringBuilder buffer = new StringBuilder(nvc.Count * 16);
            for (int i = 0; i < nvc.Count; i++)
            {
                if (string.IsNullOrEmpty(startsWith) || nvc.Keys[i].StartsWith(startsWith, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (ExistsNVP(url, nvc.Keys[i]))
                        url = RemoveNVP(url, nvc.Keys[i]);
                    if (string.IsNullOrEmpty(nvc.Keys[i]))
                        buffer.Append(HttpUtility.UrlEncode(nvc[i])).Append("&");
                    else
                        buffer.Append(nvc.Keys[i]).Append("=").Append(HttpUtility.UrlEncode(nvc[i])).Append("&");
                }
            }

            if (buffer.Length == 0)
            {
                return url;
            }
            else
            {
                if (url == null)
                    return buffer.ToString(0, buffer.Length - 1);

                url = url.TrimEnd('?', '#', '&');
                return string.Concat(url, url.IndexOf('?') == -1 ? '?' : '&', buffer.ToString(0, buffer.Length - 1));
            }
        }

        /// <summary>
        /// 从URL地址中删除参数
        /// </summary>
        /// <returns></returns>
        public static string RemoveNVP(string url, string name)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            int[] idx = new int[2];
            idx[0] = url.IndexOf('?');
            if (idx[0] == -1)
                return url;

            idx[0] = url.IndexOf(name + "=", idx[0], StringComparison.CurrentCultureIgnoreCase);
            if (idx[0] == -1)
                return url;

            idx[1] = url.IndexOf('&', idx[0]);
            if (idx[1] == -1)
                return url.Substring(0, idx[0] - 1);
            else
            {
                if (url[idx[0] - 1] == '?')
                    return string.Concat(url.Substring(0, idx[0]), url.Substring(idx[1] + 1, url.Length - idx[1] - 1));
                else
                    return string.Concat(url.Substring(0, idx[0] - 1), url.Substring(idx[1], url.Length - idx[1]));
            }
        }
    }
}