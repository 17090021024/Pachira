using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace TrunkCommon
{
    public static class DetectHelper
    {
        /// <summary>
        /// 空版本（0.0.0.0）
        /// </summary>
        public static readonly Version EmptyVersion = new Version(0, 0, 0, 0);
        public static readonly ClientSystem UnknownClientSystem = new ClientSystem { Type = ClientSystemType.Unknown, Version = EmptyVersion };
        public static readonly ClientBrowser UnknownClientBrowser = new ClientBrowser { Type = ClientBrowserType.Unknown, Version = EmptyVersion };
        public static readonly UserClient UnknownUserClient = new UserClient { System = UnknownClientSystem, Browser = UnknownClientBrowser };

        private static ShareLock SystemRegexCacheShareLock = new ShareLock();
        private static ShareLock BrowserRegexCacheShareLock = new ShareLock();
        private static readonly string VersionRegexString = @"[^\d_.]*(?<Version>\d+([._]\d+)?([._]\d+)?([._]\d+)?)?";
        private static Dictionary<ClientSystemType, Regex> _SystemRegexCache = null;
        private static Dictionary<ClientSystemType, Regex> SystemRegexCache
        {
            get
            {
                if (_SystemRegexCache == null)
                {
                    using (SystemRegexCacheShareLock.WriteLock)
                    {
                        _SystemRegexCache = new Dictionary<ClientSystemType, Regex>();
                        Array types = Enum.GetValues(typeof(ClientSystemType));
                        foreach (ClientSystemType t in types)
                            if (t != ClientSystemType.Unknown)
                            {
                                _SystemRegexCache.Add(t, GetRegex(t));
                            }
                    }
                }
                return _SystemRegexCache;
            }
        }
        private static Dictionary<ClientBrowserType, Regex> _BrowserRegexCache = null;
        private static Dictionary<ClientBrowserType, Regex> BrowserRegexCache
        {
            get
            {
                if (_BrowserRegexCache == null)
                {
                    using (BrowserRegexCacheShareLock.WriteLock)
                    {
                        _BrowserRegexCache = new Dictionary<ClientBrowserType, Regex>();
                        Array types = Enum.GetValues(typeof(ClientBrowserType));
                        foreach (ClientBrowserType t in types)
                        {
                            if (t != ClientBrowserType.Unknown)
                                _BrowserRegexCache.Add(t, GetRegex(t));
                        }
                    }
                }
                return _BrowserRegexCache;
            }
        }

        public static Version ParseVersion(string verStr)
        {
            int[] ver = new int[] { 0, 0, 0, 0 };
            if (verStr != null)
            {
                string[] verStrs = verStr.Split('.', '_');
                for (int i = 0; i < 4; i++)
                {
                    if (verStrs.Length > i)
                    {
                        ver[i] = verStrs[i].ToInt();
                        if (ver[i] < 0)
                            ver[i] = 0;
                    }
                    else
                        break;
                }
            }
            return new Version(ver[0], ver[1], ver[2], ver[3]);
        }

        private static Regex GetRegex(Enum e)
        {
            return new Regex(string.Concat("(?<System>", EnumHelper.GetDescription(e, string.Empty), ")", VersionRegexString), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// 检测用户客户端系统和浏览器
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public static UserClient Detect(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return UnknownUserClient;

            UserClient uc = new UserClient { System = UnknownClientSystem, Browser = UnknownClientBrowser };
            GroupCollection group = null;

            Dictionary<ClientSystemType, Regex> src = SystemRegexCache;
            using (SystemRegexCacheShareLock.ReadLock)
            {
                foreach (KeyValuePair<ClientSystemType, Regex> item in src)
                {
                    group = item.Value.Match(userAgent).Groups;
                    if (group["System"].Success)
                    {
                        uc.System = new ClientSystem { Type = item.Key };
                        uc.System.Version = group["Version"].Success ? ParseVersion(group["Version"].Value) : EmptyVersion;
                        break;
                    }
                }
            }

            Dictionary<ClientBrowserType, Regex> brc = BrowserRegexCache;
            using (BrowserRegexCacheShareLock.ReadLock)
            {
                foreach (KeyValuePair<ClientBrowserType, Regex> item in brc)
                {
                    group = item.Value.Match(userAgent).Groups;
                    if (group["System"].Success)
                    {
                        uc.Browser = new ClientBrowser { Type = item.Key };
                        uc.Browser.Version = group["Version"].Success ? ParseVersion(group["Version"].Value) : EmptyVersion;
                        break;
                    }
                }
            }

            return uc;
        }

        public static UserClient Parse(int[] array)
        {
            if (array == null || array.Length != 10)
                return null;

            return new UserClient
            {
                System = new ClientSystem
                {
                    Type = (ClientSystemType)array[0],
                    Version = new Version(array[1], array[2], array[3], array[4])
                },
                Browser = new ClientBrowser
                {
                    Type = (ClientBrowserType)array[5],
                    Version = new Version(array[6], array[7], array[8], array[9])
                }
            };
        }

        /// <summary>
        /// 是否是移动设备（含Pad设备）
        /// </summary>
        public static bool IsMobileDevice(ClientSystem system)
        {
            switch (system.Type)
            {
                case ClientSystemType.Android:
                case ClientSystemType.iPad:
                case ClientSystemType.iPod:
                case ClientSystemType.iPhone:
                case ClientSystemType.WindowsMobile:
                case ClientSystemType.WindowsPhone:
                case ClientSystemType.Symbian:
                case ClientSystemType.BlackBerry:
                case ClientSystemType.J2ME:
                case ClientSystemType.MTK:
                    return true;
            }
            return false;
        }
    }

    [Serializable]
    public class UserClient
    {
        public ClientSystem System { get; set; }
        public ClientBrowser Browser { get; set; }
        public override string ToString()
        {
            return string.Concat(System.ToString(), "/", Browser.ToString());
        }
        public int[] ToArray()
        {
            return new int[]
            {
                (int)System.Type,//0
                System.Version.Major,//1
                System.Version.Minor,//2
                System.Version.Build,//3
                System.Version.Revision,//4
                (int)Browser.Type,//5
                Browser.Version.Major,//6
                Browser.Version.Minor,//7
                Browser.Version.Build,//8
                Browser.Version.Revision,//9
            };
        }
        public string ToJson()
        {
            return string.Concat("{\"SystemType\":\"", System.Type.ToString(),
                "\",\"SystemVersion\":\"", System.Version.ToString(),
                "\",\"BrowserType\":\"", Browser.Type.ToString(),
                "\",\"BrowserVersion\":\"", Browser.Version.ToString(), "\"}");
        }
    }

    [Serializable]
    public class ClientSystem
    {
        public Version Version { get; set; }
        public ClientSystemType Type { get; set; }
        public override string ToString()
        {
            return string.Concat(Type.ToString(), "-", Version.ToString());
        }
    }

    [Serializable]
    public class ClientBrowser
    {
        public Version Version { get; set; }
        public ClientBrowserType Type { get; set; }
        public override string ToString()
        {
            return string.Concat(Type.ToString(), "-", Version.ToString());
        }
    }

    /// <summary>
    /// 客户端系统类型
    /// </summary>
    public enum ClientSystemType : int
    {
        //注意：检测时会按照以下定义的先后顺序逐一进行
        [Description(@"Windows\s?Phone|WPOS|Windows;\sU;")]
        WindowsPhone = 5,
        [Description(@"Android|Adr\s|Linux;\sU;|U;\sLinux")]
        Android = 10,
        [Description(@"iPad")]
        iPad = 20,
        [Description(@"iPod")]
        iPod = 30,
        [Description(@"iPh(one)?\sOS|iOS")]
        iPhone = 40,
        [Description(@"Windows\s?Mobile")]
        WindowsMobile = 50,
        [Description(@"Series60|Symbian|Nokia")]
        Symbian = 70,
        [Description(@"BlackBerry")]
        BlackBerry = 80,
        [Description(@"Macintosh|Mac\s?OS")]
        Mac = 90,
        [Description(@"J2ME|Jblend")]
        J2ME = 100,
        [Description(@"MAUI|MTK")]
        MTK = 110,
        [Description(@"Windows")]
        Windows = 120,
        [Description(@"Linux")]
        Linux = 130,
        [Description(@"Bot|Spider|Slurp|Transcoder")]
        Bot = 140,
        Unknown = 0
    }

    /// <summary>
    /// 客户端浏览器类型
    /// </summary>
    public enum ClientBrowserType : int
    {
        //注意：检测时会按照以下定义的先后顺序逐一进行
        [Description(@"MicroMessenger")]
        WeiXin = 10,
        [Description(@"MiuiYellowPage")]
        MiUIYP = 20,
        [Description(@"ewandroidtourpal")]
        EwAndroidTourPal = 22,
        [Description(@"QQBrowser")]
        QQBrowser = 30,
        [Description(@"UC((Web)|(Browser)|\W)")]
        UCBrowser = 40,
        [Description(@"Chrome|CriOS")]
        Chrome = 50,
        [Description(@"Firefox")]
        Firefox = 60,
        [Description(@"Opera")]
        Opera = 70,
        [Description(@"MSIE|IEMobile")]
        IE = 80,
        [Description(@"Mac\s?OS.*((Safari)|(WebKit))")]
        Safari = 90,
        [Description(@"WebKit|Safari")]
        Native = 100,
        [Description(@"SogouMobileBrowser")]
        SOUGOU = 110,
        Unknown = 0
    }
}