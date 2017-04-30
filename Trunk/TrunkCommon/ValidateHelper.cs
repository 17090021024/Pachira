using System.Text.RegularExpressions;

namespace TrunkCommon
{
    public static class ValidateHelper
    {
        public const string PATTERN_LOCAL_IP = @"^(127\.0\.0\.1)$|^(localhost)$|^(10\.\d{1,3}\.\d{1,3}\.\d{1,3})$|^(172\.((1[6-9])|(2\d)|(3[01]))\.\d{1,3}\.\d{1,3})$|^(192\.168\.\d{1,3}\.\d{1,3})$|^::1$";
        public const string PATTERN_MOBILE_CN = @"^((\(\d{2,3}\))|(\d{3}\-))?1[34578]\d{9}$";
        public const string PATTERN_DIGIT = @"^\d+$";
        public const string PATTERN_PY = @"^[A-z \-\'_]+$";
        public const string PATTERN_TEL = @"(?<=^|\W)(0\d{2,3}-?\d{7,8}|(\+86-)?1[34578]\d{9}|0?1[34578]\d{9}|[2-9][0-9]{6,7}|[48]00[\d-]{7,10})(?=\W|$)";
        public const string PATTERN_EMAIL = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        public const string PATTERN_IDCARD = @"^(\d{15}|\d{18}|\d{17}(\d|X|x))?$";
        public const string PATTERN_CHINESE = @"[\u0391-\uFFE5]+";
        public const string PATTERN_NUMBER = @"^[-\+]?\d*(\.\d+)?$";
        public const string PATTERN_TELEPHONE = @"^((\(\d{2,3}\))|(\d{3}\-))?(\(0\d{2,3}\)|0\d{2,3}-)?[1-9]\d{6,7}(\-\d{1,6})?$";
        public const string PATTERN_ZIPCODE = @"^\d{6}$";
        public const string PATTERN_REALNAME = @"^[a-zA-Z\u0391-\uFFE5]{0,19}$";
        public const string PATTERN_ENGLISH = @"^[A-Za-z]+$";
        public const string PATTERN_BANKCARD = @"^\d[\d\-]{10,17}\d$";

        public readonly static Regex REGEX_LOCAL_IP = new Regex(PATTERN_LOCAL_IP, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public readonly static Regex REGEX_MOBILE_CN = new Regex(PATTERN_MOBILE_CN, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public readonly static Regex REGEX_DIGIT = new Regex(PATTERN_DIGIT, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public readonly static Regex REGEX_PY = new Regex(PATTERN_PY, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public readonly static Regex REGEX_EMAIL = new Regex(PATTERN_EMAIL, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public readonly static Regex REGEX_IDCARD = new Regex(PATTERN_IDCARD, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public readonly static Regex REGEX_CHINESE = new Regex(PATTERN_CHINESE, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public readonly static Regex REGEX_TEL = new Regex(PATTERN_TEL, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public readonly static Regex REGEX_NUMBER = new Regex(PATTERN_NUMBER, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public readonly static Regex REGEX_TELEPHONE = new Regex(PATTERN_TELEPHONE, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public readonly static Regex REGEX_ZIPCODE = new Regex(PATTERN_ZIPCODE, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public readonly static Regex REGEX_REALNAME = new Regex(PATTERN_REALNAME, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public readonly static Regex REGEX_ENGLISH = new Regex(PATTERN_ENGLISH, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public readonly static Regex REGEX_BANKCARD = new Regex(PATTERN_BANKCARD, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public readonly static Regex REGEX_ALLCHINESE = new Regex("^" + PATTERN_CHINESE + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 将全角转换成半角字符串
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string DoubleChar2SingleChar(string text)
        {
            if (text.IsNullOrEmpty())
                return text;

            char[] targets = new char[text.Length];
            char tmp;
            for (int i = 0; i < text.Length; i++)
            {
                tmp = text[i];
                if (tmp == 12288)
                    targets[i] = (char)32;
                else if (tmp > 65280 && tmp < 65375)
                    targets[i] = (char)(tmp - 65248);
                else
                    targets[i] = text[i];
            }

            return new string(targets);
        }

        /// <summary>
        /// 是否内网IP，空返回false
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsLocalIP(string ip)
        {
            return !string.IsNullOrEmpty(ip) && REGEX_LOCAL_IP.IsMatch(ip);
        }

        /// <summary>
        /// 是否合法手机号，空返回false
        /// </summary>
        /// <returns></returns>
        public static bool IsMobileCN(string text)
        {
            return !string.IsNullOrEmpty(text) && REGEX_MOBILE_CN.IsMatch(text);
        }

        /// <summary>
        /// 检查是否拼音，空返回false
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsPY(string text)
        {
            return !string.IsNullOrEmpty(text) && REGEX_PY.IsMatch(text);
        }

        /// <summary>
        /// 是否有效数字形式，空返回false
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsDight(string text)
        {
            return !string.IsNullOrEmpty(text) && REGEX_DIGIT.IsMatch(text);
        }

        /// <summary>
        /// 是否有效邮箱格式，空返回false
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsEmail(string text)
        {
            return !string.IsNullOrEmpty(text) && REGEX_EMAIL.IsMatch(text);
        }

        /// <summary>
        /// 是否大陆身份证号，空返回false
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsIDCard(string text)
        {
            return !string.IsNullOrEmpty(text) && REGEX_IDCARD.IsMatch(text);
        }

        /// <summary>
        /// 判断是否字符串中包含汉字，空返回false
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool ContainChinese(string text)
        {
            return !string.IsNullOrEmpty(text) && REGEX_CHINESE.IsMatch(text);
        }

        /// <summary>
        /// 是否是汉字，空返回false
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsChinese(string text)
        {
            return !string.IsNullOrEmpty(text) && REGEX_ALLCHINESE.IsMatch(text);
        }

        /// <summary>
        /// 是否是电话号码（含手机），空返回false
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsTel(string text)
        {
            return !string.IsNullOrEmpty(text) && REGEX_TEL.IsMatch(text);
        }
        
        /// <summary>
        /// 是否是座机电话号码，空返回false
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsTelephone(string text)
        {
            return !string.IsNullOrEmpty(text) && REGEX_TELEPHONE.IsMatch(text);
        }

        /// <summary>
        /// 是否是实数，空返回false
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsNumber(string text)
        {
            return !string.IsNullOrEmpty(text) && REGEX_NUMBER.IsMatch(text);
        }

        /// <summary>
        /// 是否是邮政编码，空返回false
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsZipCode(string text)
        {
            return !string.IsNullOrEmpty(text) && REGEX_ZIPCODE.IsMatch(text);
        }

        /// <summary>
        /// 是否真实姓名，空返回false
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsRealName(string text)
        {
            return !string.IsNullOrEmpty(text) && REGEX_REALNAME.IsMatch(text);
        }

        /// <summary>
        /// 是否英文，空返回false
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsEnglish(string text)
        {
            return !string.IsNullOrEmpty(text) && REGEX_ENGLISH.IsMatch(text);
        }

        /// <summary>
        /// 是否银行卡号，空返回false
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsBankcard(string text)
        {
            return !string.IsNullOrEmpty(text) && REGEX_BANKCARD.IsMatch(text);
        }
    }
}
