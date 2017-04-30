using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TrunkCommon
{
    public static class StringHelper
    {
        /// <summary>
        /// 利用正则表达式实现类似SQL语发中的Like关键字语法。
        /// </summary>
        /// <param name="input">原字符串</param>
        /// <param name="pattern">要匹配的正则（如：.*表示任意个数字符，.表示一个字符等）</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns>是否匹配</returns>
        public static bool Like(this string input, string pattern, bool ignoreCase)
        {
            if (ignoreCase)
                return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
            else
                return Regex.IsMatch(input, pattern);
        }

        /// <summary>
        /// javascript版hash字符串的方法
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int JSHashCode(string s)
        {
            if (string.IsNullOrEmpty(s))
                return 0;
            int hash = 1315423911, i, ch;
            for (i = s.Length - 1; i >= 0; i--)
            {
                ch = s[i];
                hash ^= ((hash << 5) + ch + (hash >> 2));
            }
            return (hash & 0x7FFFFFFF);
        }

        /// <summary>
        /// 返回第一个不为null、Empty的值，如果没找到则返回null
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string NVL(params string[] values)
        {
            foreach (string item in values)
                if (!string.IsNullOrEmpty(item))
                    return item;
            return null;
        }

        /// <summary>
        /// 查找字符串是否存在于字符串数组中
        /// </summary>
        /// <param name="input">要找的字符串</param>
        /// <param name="strs">字符串数组</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        public static bool In(this string input, bool ignoreCase, params string[] strs)
        {
            if (strs == null)
                return input == null;
            else
            {
                if (ignoreCase)
                {
                    foreach (string str in strs)
                    {
                        if (str.Equals(input, StringComparison.CurrentCultureIgnoreCase))
                            return true;
                    }
                }
                else
                {
                    foreach (string str in strs)
                    {
                        if (str == input)
                            return true;
                    }
                }
                return false;
            }
        }

        public static string UriEncode(this string url)
        {
            return url.IsNullOrEmpty() ? string.Empty : HttpUtility.UrlEncode(url);
        }

        public static string UriUnescape(this string url)
        {
            return url.IsNullOrEmpty() ? string.Empty : HttpUtility.UrlDecode(url);
        }

        /// <summary>
        /// 删除指定字符串中的某段字符串
        /// </summary>
        public static string Remove(this string input, string start, string end, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            start = Regex.Escape(start);
            if (string.IsNullOrEmpty(end))
            {
                int idx = input.IndexOf(start);
                return (idx == -1) ? input : input.Substring(0, idx);
            }
            else
                end = Regex.Escape(end);
            return Regex.Replace(input, string.Concat(start, "(?:(?!", end, ").)*", end), string.Empty, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }
        /// <summary>
        /// 去除字符串结尾处的指定字符串
        /// </summary>
        /// <param name="input"></param>
        /// <param name="trimStr"></param>
        /// <returns></returns>
        public static string TrimEnd(this string input, params string[] trimStr)
        {
            if (trimStr == null || trimStr.Length == 0 || string.IsNullOrEmpty(input))
                return input;

            foreach (string item in trimStr)
            {
                int index = input.LastIndexOf(item);
                while (index != -1 && index + item.Length == input.Length)
                {
                    input = input.Substring(0, index);
                    index = input.LastIndexOf(item);
                }
            }
            return input;
        }
        /// <summary>
        /// 去除字符串开始处的指定字符串
        /// </summary>
        /// <param name="input"></param>
        /// <param name="trimStr"></param>
        /// <returns></returns>
        public static string TrimStart(this string input, params string[] trimStr)
        {
            if (trimStr == null || trimStr.Length == 0 || string.IsNullOrEmpty(input))
                return input;

            foreach (string item in trimStr)
            {
                int index = -1;
                while ((index = input.IndexOf(item)) == 0)
                    input = input.Substring(item.Length, input.Length - item.Length);
            }
            return input;
        }
        /// <summary>
        /// 查找关键词，只有该字符串两侧不是字母、数字、下划线的情况下才算做一个关键词。但如果是单字符符号、空白字符、分隔符、标点符号，则直接查找
        /// </summary>
        /// <returns></returns>
        public static bool ExistsKeyWord(this string input, params string[] keyword)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            foreach (string item in keyword)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                else if (item.Length == 1 &&
                    (char.IsSymbol(item[0])
                    || char.IsPunctuation(item[0])
                    || char.IsSeparator(item[0])
                    || char.IsWhiteSpace(item[0]))
                    && input.IndexOf(item[0]) != -1)
                    return true;
                else if (Regex.IsMatch(input, string.Concat("\\W", item, "\\W")))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 利用词典对关键词进行替换
        /// </summary>
        public static string Change(this string input, IEnumerable<KeyValuePair<string, string>> changeList)
        {
            if (changeList == null)
                return input;

            foreach (KeyValuePair<string, string> item in changeList)
                if (input == item.Key)
                    return item.Value;
            return input;
        }

        /// <summary>
        /// 利用词典对关键词进行替换
        /// </summary>
        public static string Change(this string input, string target, params string[] changeList)
        {
            if (changeList == null)
                return input;

            foreach (string item in changeList)
                if (input == item)
                    return target;
            return input;
        }

        /// <summary>
        /// 高亮显示关键词
        /// </summary>
        /// <param name="input">原字符串</param>
        /// <param name="keywordRegex">匹配关键词的正则</param>
        /// <param name="format">格式模板$0表示关键词，如abc$0def</param>
        /// <returns>高亮处理后的字符串</returns>
        public static string HightLightKeyword(this string input, string keywordRegex, string format)
        {
            return Regex.Replace(input, keywordRegex, format);
        }
        /// <summary>
        /// 高亮显示电话号码（含手机号）
        /// </summary>
        /// <param name="orgStr">原字符串</param>
        /// <param name="format">格式模板$0表示电话号码，如abc$0def</param>
        /// <returns>高亮处理后的字符串</returns>
        public static string HightLightTel(string orgStr, string format)
        {
            return HightLightKeyword(orgStr, ValidateHelper.PATTERN_TEL, format);
        }

        /// <summary>
        /// 转义字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Escape(this string str)
        {
            return Uri.EscapeDataString(str);
        }

        /// <summary>
        /// 非转义字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Unescape(this string str)
        {
            return Uri.UnescapeDataString(str);
        }

        /// <summary>
        /// 阿拉伯数字转换为中文数字
        /// </summary>
        public static readonly char[] ChineseNum = { '零', '一', '二', '三', '四', '五', '六', '七', '八', '九' };
        /// <summary>
        /// 中文数字转换为阿拉伯数字
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static char ChineseToArabic(char c)
        {
            switch (c)
            {
                case '零': return '0';
                case '一': return '1';
                case '二': return '2';
                case '三': return '3';
                case '四': return '4';
                case '五': return '5';
                case '六': return '6';
                case '七': return '7';
                case '八': return '8';
                case '九': return '9';
                default: return char.MinValue;
            }
        }

        /// <summary>
        /// 阿拉伯数字转换为中文大写形式
        /// </summary>
        public static readonly string[] CapitalChineseNum = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
        /// <summary>
        /// 将数字金额转换为大写中文形式
        /// </summary>
        /// <param name="money">数字金额</param>
        /// <returns>大写中文形式</returns>
        public static string DecimalToCHNMoney(decimal money)
        {
            string[] Wstr = { "分", "角", "圆", "拾", "佰", "仟", "万", "拾", "佰", "仟", "亿", "拾", "佰", "仟", "兆", "拾", "佰", "仟" };
            long Money = Convert.ToInt64(money * 100);
            string MoneyStr = Money.ToString();
            string BigMoney = "";
            int di, dj = MoneyStr.Length;
            for (di = 0; di < dj; di++)
            {
                int tm = Convert.ToInt32(MoneyStr.Substring(dj - di - 1, 1));
                BigMoney += Wstr[di] + CapitalChineseNum[tm];
            }
            string bm = string.Empty;
            foreach (char a in BigMoney.ToCharArray())
            {
                bm = a + bm;
            }
            for (di = 0; di < dj; di++)
            {
                bm = bm.Replace("零仟零佰零拾零圆", "圆");
                bm = bm.Replace("零仟零佰零拾零万", "万");
                bm = bm.Replace("零零", "零");
                bm = bm.Replace("零圆", "圆");
                bm = bm.Replace("零拾", "零");
                bm = bm.Replace("零佰", "零");
                bm = bm.Replace("零仟", "零");
                bm = bm.Replace("零万", "万");
                bm = bm.Replace("零亿", "亿");
                bm = bm.Replace("零角零分", "整");
                bm = bm.Replace("零分", "");
                bm = bm.Replace("零角", "");
            }
            return bm;
        }

        public static string DecimalToChinese(decimal number)
        {
            string chineseChar = "零一二三四五六七八九十";
            string unitChar = "点十百千万十百千亿十百千兆十百千";
            string strNumber = number.ToString();
            string[] tmp = strNumber.Split('.');
            strNumber = tmp[0];
            string extNumber = tmp.Length > 1 ? tmp[1] : string.Empty;
            StringBuilder result = new StringBuilder();
            int num = 0;
            bool last0 = false;
            for (int i = 0; i < strNumber.Length; i++)
            {
                num = strNumber[i].ToInt();

                if (num == 0)
                {
                    if ((strNumber.Length - i - 1) % 4 == 0)
                    {
                        if (last0)
                            result.Remove(result.Length - 1, 1);
                        result.Append(unitChar[strNumber.Length - i - 1]);
                    }

                    if (!last0)
                        result.Append(chineseChar[num]);
                    last0 = true;
                }
                else
                {
                    last0 = false;
                    result.Append(chineseChar[num]);
                    result.Append(unitChar[strNumber.Length - i - 1]);
                }
            }

            for (int i = 0; i < extNumber.Length; i++)
            {
                result.Append(chineseChar[extNumber[i].ToInt()]);
            }

            return result.ToString().TrimEnd(unitChar[0]);
        }

        /// <summary>
        /// 剪裁字符串,可根据字节为单位剪裁
        /// </summary>
        public static string ClipString(this string s, int count, bool unitByByte, bool autoSuspension)
        {
            if (count < 1 || string.IsNullOrEmpty(s))
                return string.Empty;
            if (unitByByte)
            {
                int bytec = 0;
                for (int i = 0; i < s.Length; i++)
                {
                    bytec += (s[i] < 0xFF) ? 1 : 2;
                    if (bytec >= count)
                    {
                        if (autoSuspension)
                        {
                            if (s.Length - i <= 1)
                                return s;
                            else
                                return s.Substring(0, i) + "…";
                        }
                        else
                            return s.Substring(0, i);
                    }
                }
                return s;
            }
            else
            {
                if (s.Length <= count)
                    return s;
                if (autoSuspension)
                    return s.Substring(0, count) + "…";
                else
                    return s.Substring(0, count);
            }
        }

        /// <summary>
        /// 替换相应字符方法，如手机号的136*****0518实现
        /// </summary>
        /// <param name="orgStr">要进行处理的原字符串</param>
        /// <param name="shiftChar">要替换成的字符</param>
        /// <param name="bofIndex">起始索引</param>
        /// <param name="eofIndex">结束索引</param>
        public static string ShiftString(this string orgStr, char shiftChar, int bofIndex, int eofIndex)
        {
            if (string.IsNullOrEmpty(orgStr))
                return string.Empty;
            if (bofIndex < 0)
                bofIndex = 0;
            if (eofIndex > orgStr.Length - 1)
                eofIndex = orgStr.Length - 1;
            char[] charArray = orgStr.ToCharArray();
            for (int i = bofIndex; i <= eofIndex; i++)
                charArray[i] = shiftChar;
            return new string(charArray);
        }

        /// <summary>
        /// 标准62个字符:数字+字母字符0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ
        /// </summary>
        public const string STANDARDBASE62CHARS = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        /// <summary>
        /// 标准64个字符:数字+字母字符ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/
        /// </summary>//                           !"#$%'()*+,-.0123456789:;=?@abcdefghijklmnopqrstuvwxyz[]^_`{}~
        //public const string STANDARDBASE64CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        /// <summary>
        /// 标准base32字符
        /// </summary>
        public const string STANDARDBASE32CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        /// <summary>
        /// 文件名专用41个字符：0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ+-_@!
        /// </summary>
        //public const string STANDARDBASE41CHARS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ+-_@!";
        /// <summary>
        /// 标准base64字符映射表（用于根据字符找到它在base64字符串中的索引位置）
        /// </summary>
        //public static readonly byte[] STANDARDBASE64CHARSMAP = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 62, 0, 0, 0, 63, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 0, 0, 0, 0, 0, 0, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 0, 0, 0, 0, 0 };
        //                                                                                                                                                   !  "  #  $  %  &  '  (  )  *  +   ,  -  .  /   0   1   2   3   4   5   6   7   8   9   :  ;  <  =  >  ?  @  A  B  C  D  E  F  G  H  I  J  K   L   M   N   O   P   Q   R   S   T   U   V   W   X   Y   Z   [  \  ]  ^  _  `  a   b   c   d   e   f   g   h   i   j   k   l   m   n   o   p   q   r   s   t   u   v   w   x   y   z   {  |  }  ~  
        //                                                0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 37 38 39 40 41 42 43  44 45 46 47  48  49  50  51  52  53  54  55  56  57  58 59 60 61 62 63 64 65 66 67 68 69 70 71 72 73 74 75  76  77  78  79  80  81  82  83  84  85  86  87  88  89  90  91 92 93 94 95 96 97  98  99  100 101 102 103 104 105 106 107 108 109 110 111 112 113 114 115 116 117 118 119 120 121 122 123124125126127
        private static readonly string[] _base256 = { "00000000", "00000001", "00000010", "00000011", "00000100", "00000101", "00000110", "00000111", "00001000", "00001001", "00001010", "00001011", "00001100", "00001101", "00001110", "00001111", "00010000", "00010001", "00010010", "00010011", "00010100", "00010101", "00010110", "00010111", "00011000", "00011001", "00011010", "00011011", "00011100", "00011101", "00011110", "00011111", "00100000", "00100001", "00100010", "00100011", "00100100", "00100101", "00100110", "00100111", "00101000", "00101001", "00101010", "00101011", "00101100", "00101101", "00101110", "00101111", "00110000", "00110001", "00110010", "00110011", "00110100", "00110101", "00110110", "00110111", "00111000", "00111001", "00111010", "00111011", "00111100", "00111101", "00111110", "00111111", "01000000", "01000001", "01000010", "01000011", "01000100", "01000101", "01000110", "01000111", "01001000", "01001001", "01001010", "01001011", "01001100", "01001101", "01001110", "01001111", "01010000", "01010001", "01010010", "01010011", "01010100", "01010101", "01010110", "01010111", "01011000", "01011001", "01011010", "01011011", "01011100", "01011101", "01011110", "01011111", "01100000", "01100001", "01100010", "01100011", "01100100", "01100101", "01100110", "01100111", "01101000", "01101001", "01101010", "01101011", "01101100", "01101101", "01101110", "01101111", "01110000", "01110001", "01110010", "01110011", "01110100", "01110101", "01110110", "01110111", "01111000", "01111001", "01111010", "01111011", "01111100", "01111101", "01111110", "01111111", "10000000", "10000001", "10000010", "10000011", "10000100", "10000101", "10000110", "10000111", "10001000", "10001001", "10001010", "10001011", "10001100", "10001101", "10001110", "10001111", "10010000", "10010001", "10010010", "10010011", "10010100", "10010101", "10010110", "10010111", "10011000", "10011001", "10011010", "10011011", "10011100", "10011101", "10011110", "10011111", "10100000", "10100001", "10100010", "10100011", "10100100", "10100101", "10100110", "10100111", "10101000", "10101001", "10101010", "10101011", "10101100", "10101101", "10101110", "10101111", "10110000", "10110001", "10110010", "10110011", "10110100", "10110101", "10110110", "10110111", "10111000", "10111001", "10111010", "10111011", "10111100", "10111101", "10111110", "10111111", "11000000", "11000001", "11000010", "11000011", "11000100", "11000101", "11000110", "11000111", "11001000", "11001001", "11001010", "11001011", "11001100", "11001101", "11001110", "11001111", "11010000", "11010001", "11010010", "11010011", "11010100", "11010101", "11010110", "11010111", "11011000", "11011001", "11011010", "11011011", "11011100", "11011101", "11011110", "11011111", "11100000", "11100001", "11100010", "11100011", "11100100", "11100101", "11100110", "11100111", "11101000", "11101001", "11101010", "11101011", "11101100", "11101101", "11101110", "11101111", "11110000", "11110001", "11110010", "11110011", "11110100", "11110101", "11110110", "11110111", "11111000", "11111001", "11111010", "11111011", "11111100", "11111101", "11111110", "11111111" };
        private static readonly byte[] _base32Lookup = { 
            0xFF, 0xFF, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, // '0', '1', '2', '3', '4', '5', '6', '7'
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // '8', '9', ':', ';', '<', '=', '>', '?'
            0xFF, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, // '@', 'A', 'B', 'C', 'D', 'E', 'F', 'G'
            0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, // 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O'
            0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, // 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W'
            0x17, 0x18, 0x19, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 'X', 'Y', 'Z', '[', '\', ']', '^', '_'
            0xFF, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, // '`', 'a', 'b', 'c', 'd', 'e', 'f', 'g'
            0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, // 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o'
            0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, // 'p', 'q', 'r', 's', 't', 'u', 'v', 'w'
            0x17, 0x18, 0x19, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF // 'x', 'y', 'z', '{', '|', '}', '~', 'DEL'
        };

        /// <summary>
        /// 转义成base32字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToBase32String(byte[] bytes)
        {
            int i = 0, index = 0, digit = 0;
            int currByte, nextByte;
            StringBuilder base32 = new StringBuilder((bytes.Length + 7) * 8 / 5);

            while (i < bytes.Length)
            {
                currByte = (bytes[i] >= 0) ? bytes[i] : (bytes[i] + 256); // unsign
                /* Is the current digit going to span a byte boundary? */
                if (index > 3)
                {
                    if ((i + 1) < bytes.Length)
                    {
                        nextByte = (bytes[i + 1] >= 0) ? bytes[i + 1] : (bytes[i + 1] + 256);
                    }
                    else
                    {
                        nextByte = 0;
                    }

                    digit = currByte & (0xFF >> index);
                    index = (index + 5) % 8;
                    digit <<= index;
                    digit |= nextByte >> (8 - index);
                    i++;
                }
                else
                {
                    digit = (currByte >> (8 - (index + 5))) & 0x1F;
                    index = (index + 5) % 8;
                    if (index == 0)
                    {
                        i++;
                    }
                }
                base32.Append(STANDARDBASE32CHARS[digit]);
            }

            return base32.ToString();
        }

        /// <summary>
        /// 将base32转回byte[]
        /// </summary>
        /// <param name="base32"></param>
        /// <returns></returns>
        public static byte[] FromBase32String(string base32)
        {
            int lookup;
            byte digit;
            byte[] bytes = new byte[base32.Length * 5 / 8];
            for (int i = 0, index = 0, offset = 0; i < base32.Length; i++)
            {
                lookup = (byte)base32[i] - 48;
                /* Skip chars outside the lookup table */
                if (lookup < 0 || lookup >= _base32Lookup.Length)
                    continue;
                digit = _base32Lookup[lookup];
                /* If this digit is not in the table, ignore it */
                if (digit == 0xFF)
                    continue;

                if (index <= 3)
                {
                    index = (index + 5) % 8;
                    if (index == 0)
                    {
                        bytes[offset] |= digit;
                        offset++;
                        if (offset >= bytes.Length)
                            break;
                    }
                    else
                        bytes[offset] |= (byte)(digit << (8 - index));
                }
                else
                {
                    index = (index + 5) % 8;
                    bytes[offset] |= (byte)(digit >> index);
                    offset++;
                    if (offset >= bytes.Length)
                        break;
                    bytes[offset] |= (byte)(digit << (8 - index));
                }
            }
            return bytes;
        }

        /// <summary>
        /// byte[]转化为16进制字符串
        /// </summary>
        /// <param name="bs">byte数组</param>
        /// <returns>16进制字符串</returns>
        public static string ToBase16String(byte[] bs)
        {
            StringBuilder sb = new StringBuilder(bs.Length * 2);
            foreach (byte b in bs)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        /// <summary>
        /// 将txt文本转换成html显示格式，如\n转换成＜br /＞
        /// </summary>
        /// <param name="text">原字符串</param>
        /// <returns>html字符串</returns>
        public static string TextToHtml(string text)
        {
            text = text.Replace(">", "&gt;");
            text = text.Replace("<", "&lt;");
            text = text.Replace(" ", "&nbsp;");
            text = text.Replace("\n", "<br />");
            text = text.Replace("\\n", "<br />");
            text = text.Replace("\r", "<br />");
            text = text.Replace("\\r", "<br />");
            text = text.Replace("\t", "　　");
            text = text.Replace("\\t", "　　");
            return text;
        }

        /// <summary>
        /// 去掉HTML标签
        /// </summary>
        /// <param name="html">HTML文本</param>
        public static string TrimHTML(string html)
        {
            return Regex.Replace(html, @"<(\S*?)[^>]*>.*?</\1>|<.*? />", string.Empty);
        }

        public enum CharType
        {
            Digit,
            Letter,
            LowerLetter,
            UpperLetter,
            DigitAndLetter,
            DigitAndLowerLetter,
            DigitAndUpperLetter,
            Symbol,
            Any,
        }

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="length">生成字符串长度</param>
        /// <param name="charType">字符类型</param>
        /// <returns>随机字符串</returns>
        public static string CreateRandomString(int length, CharType charType)
        {
            Random rd = new Random();
            char[] cs = new char[length];
            switch (charType)
            {
                default:
                case CharType.Digit:
                    for (int i = 0; i < length; i++)
                        cs[i] = STANDARDBASE62CHARS[rd.Next(10)];
                    break;
                case CharType.LowerLetter:
                    for (int i = 0; i < length; i++)
                        cs[i] = STANDARDBASE62CHARS[rd.Next(10, 36)];
                    break;
                case CharType.UpperLetter:
                    for (int i = 0; i < length; i++)
                        cs[i] = STANDARDBASE62CHARS[rd.Next(36, 62)];
                    break;
                case CharType.Letter:
                    for (int i = 0; i < length; i++)
                        cs[i] = STANDARDBASE62CHARS[rd.Next(10, 62)];
                    break;
                case CharType.DigitAndLowerLetter:
                    for (int i = 0; i < length; i++)
                        cs[i] = STANDARDBASE62CHARS[rd.Next(36)];
                    break;
                case CharType.DigitAndUpperLetter:
                    for (int i = 0; i < length; i++)
                    {
                        int j = rd.Next(0, 36);
                        if (j >= 10)
                            j += 26;
                        cs[i] = STANDARDBASE62CHARS[j];
                    }
                    break;
                case CharType.Any:
                case CharType.DigitAndLetter:
                    for (int i = 0; i < length; i++)
                        cs[i] = STANDARDBASE62CHARS[rd.Next(62)];
                    break;
            }
            return new string(cs);
        }

        /// <summary>
        /// 去除重复字符串并拼接(如：{"北京","北京","海淀"}->"北京 海淀")
        /// </summary>
        /// <returns>拼接后的字符串</returns>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <param name="separator">分隔符</param>
        /// <param name="strs"></param>
        public static string DistinctString(string separator, bool ignoreCase, params string[] strs)
        {
            if (strs == null || strs.Length == 0)
                return string.Empty;

            for (int i = 1; i < strs.Length; i++)
                for (int j = 0; j < i; j++)
                    if (strs[i] != null && strs[j] != null && strs[i].Equals(strs[j], ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture))
                        strs[i] = null;

            return JoinString(separator, true, int.MaxValue, strs);
        }

        /// <summary>
        /// 去除重复包含的字符串并拼接(如：{"北京","北京西站","海淀"}->"北京西站 海淀")
        /// </summary>
        /// <param name="separator"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static string DistinctContainsString(string separator, bool ignoreCase, int countLimit, params string[] strs)
        {
            if (strs == null || strs.Length == 0)
                return string.Empty;

            for (int i = 0; i < strs.Length; i++)
                for (int j = 0; j < strs.Length; j++)
                    if (i != j && strs[i] != null && strs[j] != null && strs[j].Contains(strs[i], ignoreCase))
                        strs[i] = null;

            return JoinString(separator, true, countLimit, strs);
        }

        /// <summary>
        /// 是否包含字串
        /// </summary>
        public static bool Contains(this string str, string value, bool caseSensitive)
        {
            return str != null && str.IndexOf(value, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        /// <summary>
        /// 判断是否包含某个字符串
        /// </summary>
        public static bool ContainsAny(this string str, bool caseSensitive, params string[] values)
        {
            if (str == null || values == null)
                return false;

            foreach (string item in values)
                if (str.IndexOf(item, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase) != -1)
                    return true;

            return false;
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return str == null || str.Trim().Length == 0;
        }
        /// <summary>
        /// 空判断，则返回默认值
        /// </summary>
        /// <returns></returns>
        public static string IsNullOrWhiteSpaceValue(this string str, string def)
        {
            return str.IsNullOrWhiteSpace() ? def : str;
        }

        /// <summary>
        /// 空判断，则返回默认值
        /// </summary>
        /// <returns></returns>
        public static string IsNullOrEmptyValue(this string str, string def)
        {
            return str.IsNullOrEmpty() ? def : str;
        }

        /// <summary>
        /// 分割字符串为字符串数组
        /// </summary>
        /// <param name="splitChar">分隔符</param>
        /// <param name="removeEmptyEntries">是否去除空项</param>
        public static string[] Split(this string str, char splitChar, bool removeEmptyEntries)
        {
            if (removeEmptyEntries)
                return str.Split(new char[] { splitChar }, StringSplitOptions.RemoveEmptyEntries);
            else
                return str.Split(splitChar);
        }
        /// <summary>
        /// 分割字符串为字符串数组
        /// </summary>
        /// <param name="splitString">分隔串</param>
        /// <param name="removeEmptyEntries">是否去除空项</param>
        public static string[] Split(this string str, string splitString, bool removeEmptyEntries)
        {
            if (removeEmptyEntries)
                return str.Split(new string[] { splitString }, StringSplitOptions.RemoveEmptyEntries);
            else
                return str.Split(new string[] { splitString }, StringSplitOptions.None);
        }

        /// <summary>
        /// 高级链接字符串数组方法
        /// </summary>
        /// <param name="separator">分隔符</param>
        /// <param name="ignoreEmpty">是否忽略空或null字符串</param>
        /// <param name="strs">要连接的字符串数组</param>
        /// <param name="countLimit">最大连接数量限制</param>
        /// <returns></returns>
        public static string JoinString(string separator, bool ignoreEmpty, int countLimit, params string[] strs)
        {
            if (strs == null || strs.Length == 0)
                return string.Empty;

            if (ignoreEmpty)
            {
                int i = 0, emptyCount = 0;
                while (i < strs.Length - emptyCount)
                {
                    if (string.IsNullOrEmpty(strs[i]))
                    {
                        emptyCount++;
                        for (int j = i; j < strs.Length - emptyCount; j++)
                            strs[j] = strs[j + 1];
                    }
                    else
                        i++;
                }
                return string.Join(separator, strs, 0, Math.Min(i, countLimit));
            }
            else
                return string.Join(separator, strs, 0, Math.Min(strs.Length, countLimit));
        }

        #region 汉字转拼音
        #region 拼音数据
        public static readonly string[] PIN_YIN_NAME = new string[]
        {
            "","A","Ai","An","Ang","Ao","Ba","Bai","Ban","Bang","Bao","Bei","Ben",
            "Beng","Bi","Bian","Biao","Bie","Bin","Bing","Bo","Bu","Ba","Ca","Cai","Can",//25
            "Cang","Cao","Ce","Cen","Ceng","Cha","Chai","Chan","Chang","Chao","Che","Chen","Cheng",//38
            "Chi","Chong","Chou","Chu","Chua","Chuai","Chuan","Chuang","Chui","Chun","Chuo","Ci","Cong",//51
            "Cou","Cu","Cuan","Cui","Cun","Cuo","Da","Dai","Dan","Dang","Dao","De",//63
            "Deng","Di","Dia","Dian","Diao","Die","Ding","Diu","Dong","Dou","Du","Duan","Dui",//76
            "Dun","Duo","E","Ei","En","Eng","Er","Fa","Fan","Fang","Fei","Fen","Feng","Fo",
            "Fou","Fu","Ga","Gai","Gan","Gang","Gao","Ge","Gei","Gen","Geng","Gong",
            "Gou","Gu","Gua","Guai","Guan","Guang","Gui","Gun","Guo","Ha","Hai","Han",
            "Hang","Hao","He","Hei","Hen","Heng","Hong","Hou","Hu","Hua","Huai","Huan",
            "Huang","Hui","Hun","Huo","Ji","Jia","Jian","Jiang","Jiao","Jie","Jin","Jing",
            "Jiong","Jiu","Ju","Juan","Jue","Jun","Ka","Kai","Kan","Kang","Kao","Ke",
            "Ken","Keng","Kong","Kou","Ku","Kua","Kuai","Kuan","Kuang","Kui","Kun","Kuo",
            "La","Lai","Lan","Lang","Lao","Le","Lei","Leng","Li","Lia","Lian","Liang",
            "Liao","Lie","Lin","Ling","Liu","Lo","Long","Lou","Lu","Lv","Luan","Lue","Lun",
            "Luo","Ma","Mai","Man","Mang","Mao","Me","Mei","Men","Meng","Mi","Mian",
            "Miao","Mie","Min","Ming","Miu","Mo","Mou","Mu","Na","Nai","Nan","Nang",
            "Nao","Ne","Nei","Nen","Neng","Ni","Nian","Niang","Niao","Nie","Nin","Ning",
            "Niu","Nong","Nou","Nu","Nv","Nuan","Nue","Nuo","Nun","O","Ou","Pa","Pai","Pan",
            "Pang","Pao","Pei","Pen","Peng","Pi","Pian","Piao","Pie","Pin","Ping","Po","Pou",
            "Pu","Qi","Qia","Qian","Qiang","Qiao","Qie","Qin","Qing","Qiong","Qiu","Qu",
            "Quan","Que","Qun","Ran","Rang","Rao","Re","Ren","Reng","Ri","Rong","Rou",
            "Ru","Ruan","Rui","Run","Ruo","Sa","Sai","San","Sang","Sao","Se","Sen",
            "Seng","Sha","Shai","Shan","Shang","Shao","She","Shen","Sheng","Shi","Shou","Shu",
            "Shua","Shuai","Shuan","Shuang","Shui","Shun","Shuo","Si","Song","Sou","Su","Suan",
            "Sui","Sun","Suo","Ta","Tai","Tan","Tang","Tao","Te","Teng","Ti","Tian",
            "Tiao","Tie","Ting","Tong","Tou","Tu","Tuan","Tui","Tun","Tuo","Wa","Wai",
            "Wan","Wang","Wei","Wen","Weng","Wo","Wu","Xi","Xia","Xian","Xiang","Xiao",
            "Xie","Xin","Xing","Xiong","Xiu","Xu","Xuan","Xue","Xun","Ya","Yan","Yang",
            "Yao","Ye","Yi","Yin","Ying","Yo","Yong","You","Yu","Yuan","Yue","Yun",
            "Za", "Zai","Zan","Zang","Zao","Ze","Zei","Zen","Zeng","Zha","Zhai","Zhan",
            "Zhang","Zhao","Zhe","Zhen","Zheng","Zhi","Zhong","Zhou","Zhu","Zhua","Zhuai","Zhuan",
            "Zhuang","Zhui","Zhun","Zhuo","Zi","Zong","Zou","Zu","Zuan","Zui","Zun","Zuo"
        };

        public static short[] GBK_PIN_YIN_CODE = new short[]
        {
            367,291,343,114,199,41,296,71,261,174,366,19,140,131,136,159,33,398,6,138,92,131,361,341,140,361,237,123,295,362,335,94,140,343,92,73,296,211,132,141,296,193,123,23,354,357,92,288,208,254,313,367,391,385,388,109,95,185,177,361,143,98,296,0,367,306,311,100,356,252,356,58,360,345,138,360,366,60,173,78,196,131,131,336,6,19,90,318,19,344,31,121,95,20,35,34,291,270,85,35,254,340,90,89,26,336,65,389,240,367,68,77,341,361,348,131,2,137,243,60,92,317,402,370,36,344,355,348,376,344,227,217,347,352,390,402,13,243,361,306,2,387,114,190,391,21,262,14,292,50,358,382,14,344,262,296,10,128,117,345,98,84,203,19,120,130,109,263,50,349,294,332,94,164,361,108,362,198,352,3,183,83,187,31,39,355,390,275,56,306,59,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,223,137,139,38,386,406,41,184,141,298,325,294,330,20,210,346,330,367,342,352,108,155,341,361,92,174,129,138,344,282,240,342,92,343,351,65,36,388,164,306,130,346,133,16,42,87,356,11,367,348,123,34,388,19,140,359,402,174,335,164,26,400,0,98,322,298,196,316,47,349,242,252,95,138,311,257,86,388,400,187,307,170,129,72,399,12,341,141,209,24,133,360,288,257,363,38,254,276,40,48,337,367,19,321,357,89,317,340,79,36,295,147,65,31,11,347,127,359,382,41,357,366,352,380,50,92,92,388,199,131,361,347,355,24,75,28,386,327,11,373,136,337,308,306,309,343,368,273,227,370,134,189,67,317,116,136,342,290,254,264,26,42,282,11,346,365,359,316,313,358,84,19,136,372,104,282,169,365,27,400,13,302,45,367,381,252,291,46,138,114,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,383,259,357,65,347,184,11,16,137,173,175,191,254,344,165,363,72,405,290,256,139,330,405,251,167,34,108,252,320,33,337,131,20,128,48,133,33,135,88,344,367,79,177,20,104,344,344,202,360,137,132,256,89,390,2,281,361,144,225,33,361,61,157,0,133,42,135,288,372,25,18,3,315,41,32,165,217,137,254,197,341,223,260,217,34,176,184,159,10,65,16,373,388,306,366,116,259,37,171,320,337,181,42,33,267,298,347,171,188,373,231,317,357,188,211,373,350,76,76,328,34,217,83,137,357,296,296,254,102,116,294,73,7,102,127,336,208,174,367,114,322,-1,131,361,193,266,357,193,28,139,105,139,193,390,103,352,362,147,361,198,296,197,141,154,177,92,347,198,315,96,123,237,92,202,253,133,308,195,328,104,138,259,72,95,362,2,171,26,203,397,55,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,306,78,137,177,342,74,85,85,141,42,387,89,207,388,89,89,146,95,248,157,114,270,46,131,254,56,131,60,335,143,171,92,262,68,202,178,389,17,136,136,290,33,101,46,160,78,83,136,50,138,73,188,376,368,57,150,288,46,57,171,87,33,252,0,46,399,96,10,78,259,398,133,131,357,130,78,329,341,105,295,133,380,146,46,45,33,394,140,171,242,290,154,105,124,380,398,173,141,179,109,35,109,133,133,317,131,133,133,388,33,403,198,171,391,356,138,152,347,388,361,391,205,167,142,154,104,333,346,206,159,136,176,150,137,97,202,39,166,365,150,142,259,183,21,32,146,199,72,352,341,361,355,339,295,167,183,245,296,131,255,35,263,345,361,256,85,142,326,141,60,190,355,355,184,171,36,345,263,370,140,10,192,14,94,94,361,242,141,318,98,79,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,102,58,140,102,212,140,361,134,148,123,262,85,257,26,159,123,367,109,128,60,160,173,173,310,74,140,262,342,150,357,262,355,218,53,296,342,335,335,347,60,39,300,159,342,347,136,4,260,292,39,309,264,160,131,79,342,282,337,381,114,356,65,381,0,181,356,257,356,296,192,321,293,325,404,87,306,368,171,67,258,5,109,357,171,3,165,171,357,357,368,121,177,274,262,150,169,74,394,25,25,25,361,59,297,143,6,279,296,398,343,308,131,337,277,51,105,261,39,74,121,201,192,307,399,56,360,234,130,342,41,258,136,250,124,121,341,213,354,275,122,97,144,184,98,195,252,341,135,370,39,295,331,338,2,296,142,241,202,206,266,65,296,390,36,39,361,39,248,50,352,39,116,356,262,344,332,250,17,92,358,117,113,58,333,406,21,314,344,130,252,83,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,79,69,391,298,346,156,296,323,3,353,334,167,326,350,366,69,218,349,249,116,368,145,327,192,402,117,344,167,249,174,6,201,184,311,92,114,305,366,104,104,11,114,361,254,328,344,335,171,261,36,375,356,73,252,65,258,0,102,0,0,174,0,288,0,0,337,137,252,333,255,3,136,67,367,322,164,342,2,226,398,296,120,24,345,397,341,338,288,104,252,252,60,333,50,328,55,117,356,252,385,174,344,363,105,376,111,188,0,65,263,33,0,317,3,60,367,385,390,32,334,367,373,359,199,123,370,126,368,131,40,337,36,352,127,380,357,174,367,283,146,256,357,60,241,25,364,337,363,0,11,0,375,130,321,359,346,255,285,365,121,347,189,31,113,275,104,131,13,341,254,399,183,333,0,313,0,281,0,208,255,14,5,173,311,385,205,316,135,40,135,146,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,316,290,2,346,182,346,123,128,111,234,344,376,34,249,63,190,123,93,317,98,281,135,39,124,190,266,167,346,131,160,346,116,92,256,42,33,316,355,341,405,85,39,128,25,46,53,60,367,162,342,252,116,173,352,128,362,355,221,306,357,363,58,60,0,398,225,128,347,256,368,2,365,157,367,241,62,119,77,61,348,243,362,404,223,65,114,314,130,343,360,78,243,390,136,137,34,355,194,388,183,128,239,366,221,362,123,195,121,385,171,179,113,211,205,357,171,183,181,205,60,37,247,243,345,194,342,78,155,357,33,363,0,163,314,346,49,126,130,394,221,346,23,171,33,32,171,361,188,371,309,342,378,133,221,391,165,221,0,180,128,362,222,329,148,368,139,244,370,51,128,79,111,326,328,111,265,272,111,315,111,362,126,114,187,111,45,337,368,263,155,251,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,368,368,356,329,328,328,329,186,128,361,185,185,356,328,325,251,183,157,141,397,254,254,398,61,253,343,290,221,205,132,388,355,361,258,88,273,331,88,14,349,165,138,131,39,349,8,317,262,398,105,315,291,358,90,183,261,103,354,6,178,391,92,123,0,388,5,59,239,202,131,117,184,50,39,362,122,76,384,92,108,359,78,109,31,358,347,297,19,121,58,145,61,115,3,349,344,9,92,6,361,362,114,352,47,258,89,86,264,365,144,343,65,166,142,258,385,176,124,21,139,360,65,356,263,311,387,335,141,187,38,153,291,72,316,24,42,13,344,78,361,388,76,252,141,217,150,161,217,133,96,367,356,104,170,86,356,161,3,294,212,328,38,126,14,173,111,394,10,367,206,136,231,2,101,400,367,127,79,359,10,50,195,34,332,250,89,389,136,137,120,96,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,48,133,248,169,349,127,75,335,353,342,131,157,363,365,146,309,296,198,339,38,328,264,389,171,242,9,374,76,322,341,387,355,386,2,102,357,147,338,347,179,113,166,34,242,13,37,183,183,234,254,195,205,394,302,182,16,28,388,383,147,67,37,398,0,131,111,255,258,65,55,357,314,379,254,337,396,256,290,290,87,251,157,329,85,264,77,56,65,295,78,78,316,64,341,88,127,316,58,360,391,133,5,255,256,151,328,243,67,134,360,20,316,165,141,125,61,267,254,355,344,342,2,356,62,276,137,169,159,183,357,316,337,125,181,181,277,171,177,33,355,357,169,6,335,0,395,361,190,391,395,123,161,123,352,161,297,192,77,297,361,104,134,238,381,15,311,265,178,57,350,347,212,343,368,193,36,259,0,103,252,197,197,37,376,143,97,318,315,318,14,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,370,93,6,81,321,239,142,347,67,358,126,146,293,12,127,87,68,355,152,5,339,315,341,5,134,173,78,370,134,296,88,130,14,185,36,70,254,140,229,31,344,85,131,121,60,92,138,136,338,389,235,115,389,370,91,39,369,395,357,210,348,88,89,0,368,74,20,398,384,217,368,327,353,388,79,205,14,294,79,117,352,84,387,202,8,92,178,399,266,290,358,191,399,141,270,72,131,94,345,130,142,103,133,133,218,388,386,344,120,108,355,123,357,203,176,240,356,366,357,294,296,333,263,307,121,344,292,295,347,192,92,207,150,53,53,325,349,210,367,249,311,388,114,395,248,330,344,341,357,341,2,357,367,306,171,141,303,252,344,398,72,183,2,79,182,199,51,250,141,24,178,346,298,252,128,92,340,277,316,87,87,322,217,263,138,138,254,67,349,123,107,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,164,362,41,49,92,187,230,129,362,356,141,171,67,344,124,124,61,359,210,49,132,367,367,337,321,274,195,60,276,259,128,340,254,48,200,92,136,75,342,389,127,199,357,363,353,136,337,387,261,296,347,78,173,193,266,306,244,337,333,53,123,257,10,0,352,327,109,401,359,368,273,275,179,195,237,5,189,41,294,386,368,136,273,203,349,309,220,318,238,166,212,10,243,361,367,353,191,361,148,365,217,404,137,394,28,114,167,385,123,123,5,215,189,246,104,341,256,332,382,200,344,344,205,175,173,124,109,64,388,352,361,124,342,160,268,357,33,135,195,92,85,357,361,118,135,92,296,311,255,173,353,348,220,72,361,25,2,219,223,205,68,41,137,50,367,247,273,275,209,357,315,363,25,369,199,14,294,349,217,74,179,368,165,357,178,135,219,165,344,363,302,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,347,263,198,171,185,357,298,165,143,189,348,306,315,366,217,312,194,51,254,356,217,399,171,354,20,209,221,363,185,273,260,132,274,359,361,138,397,391,121,296,361,323,296,38,265,102,346,380,296,294,127,198,154,24,373,368,259,117,386,133,223,19,126,0,258,296,223,137,223,388,367,10,158,223,184,258,123,296,223,294,337,347,158,128,144,361,361,10,258,40,10,89,186,76,249,394,92,150,134,394,355,298,76,62,136,198,198,298,291,205,175,344,344,161,336,175,192,336,336,336,359,78,160,389,95,104,393,95,95,342,14,136,322,68,296,386,342,217,342,0,191,182,248,321,87,347,184,30,347,141,143,175,143,298,347,217,333,344,171,357,62,128,181,270,341,114,294,42,0,369,8,359,4,341,136,131,31,335,254,252,136,406,406,358,141,96,354,249,171,323,402,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,92,332,243,5,159,262,249,323,178,39,248,0,9,326,354,15,117,150,188,79,221,69,183,81,83,94,263,361,207,296,3,337,126,198,171,131,326,337,366,104,176,359,135,79,347,21,265,89,212,171,366,344,273,62,294,38,328,101,97,343,362,341,166,0,147,344,264,40,314,177,124,141,164,202,161,161,55,104,356,96,187,187,178,78,387,362,72,114,387,337,243,136,402,143,72,362,399,376,127,367,61,89,261,358,321,361,388,296,79,391,382,184,357,195,114,131,126,325,193,367,400,165,136,221,357,337,400,31,311,150,258,252,328,76,342,339,26,317,273,136,146,179,341,256,13,67,254,365,221,57,296,279,307,400,134,135,148,33,69,29,70,328,182,382,382,5,27,262,255,55,404,62,62,342,367,243,181,345,30,20,258,135,357,167,382,175,175,258,78,405,256,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,143,359,135,359,143,290,361,354,212,360,360,361,221,344,131,347,150,342,65,5,404,337,273,62,178,136,367,369,362,275,136,176,342,181,181,273,342,141,33,363,160,357,212,263,35,54,185,67,67,221,357,357,357,160,357,126,127,349,35,176,0,0,0,0,261,388,384,361,137,312,263,0,92,371,70,221,88,235,388,123,60,59,333,92,205,361,361,248,253,142,275,300,303,256,386,296,265,59,109,248,383,282,335,59,288,252,74,122,386,352,337,361,9,248,69,102,237,318,198,132,320,128,389,294,16,111,376,207,9,138,33,388,123,14,14,383,198,256,33,88,197,9,62,201,42,136,344,165,133,19,95,131,325,376,108,194,258,331,356,348,136,395,356,50,326,361,388,323,388,346,328,109,155,197,366,21,19,38,164,131,307,298,259,367,200,308,28,345,87,140,79,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,109,179,288,308,388,21,259,140,140,182,362,59,183,361,42,328,306,254,200,34,341,87,108,0,157,14,285,177,175,183,131,363,344,325,365,171,325,355,65,249,128,209,128,88,262,357,374,361,83,282,83,296,68,123,341,321,134,6,294,383,318,92,65,0,35,386,361,263,292,353,138,68,383,134,255,242,14,293,316,133,98,84,14,154,133,17,346,60,111,255,121,198,111,335,143,131,183,128,128,361,361,361,369,369,338,357,367,39,68,365,245,39,398,332,131,389,336,65,178,92,336,342,336,38,388,138,51,388,51,72,131,388,51,390,164,347,136,133,296,343,15,92,337,86,342,387,245,39,63,387,17,389,36,359,128,195,181,267,10,262,168,270,70,94,270,114,252,296,342,85,363,202,338,341,131,342,136,335,51,298,296,55,315,397,258,129,350,159,344,148,141,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,361,10,202,240,141,8,224,382,123,262,41,65,328,366,92,342,14,366,353,51,19,127,354,243,298,342,316,365,205,0,296,342,167,120,159,206,388,347,323,69,116,109,352,346,306,123,261,128,39,361,350,106,177,39,291,394,369,242,63,128,346,192,365,0,142,14,369,298,328,136,385,177,135,257,321,21,254,342,196,361,120,38,341,171,174,51,361,171,212,79,341,51,63,34,196,171,107,107,252,153,322,187,342,147,110,217,77,111,382,368,137,131,177,352,117,263,316,321,221,47,129,348,79,313,400,133,365,63,212,370,324,48,260,28,15,129,400,296,11,353,337,98,337,14,353,126,199,365,252,291,362,60,193,253,150,2,257,357,276,400,281,363,264,121,370,309,252,359,307,127,131,104,141,46,217,347,146,387,365,27,355,294,20,146,347,129,365,358,171,284,318,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,357,352,315,370,203,102,51,245,207,111,39,25,25,25,55,202,383,326,5,302,107,264,375,140,173,234,307,258,362,184,291,329,191,254,293,259,39,385,184,142,252,252,367,248,175,51,366,40,388,326,38,252,262,242,11,260,135,39,173,248,160,128,38,0,348,362,342,342,60,316,78,77,309,143,28,346,85,88,167,167,344,202,175,341,25,143,53,344,316,295,243,361,344,225,60,316,138,307,135,337,353,258,258,141,27,151,363,361,177,285,144,125,2,357,162,343,39,367,362,59,197,217,197,76,252,205,344,196,41,388,231,357,358,20,388,159,159,366,92,179,201,38,128,33,165,125,353,267,33,131,141,126,293,173,210,198,317,143,395,395,369,98,25,72,69,24,132,388,160,382,255,98,133,367,357,123,382,342,342,262,123,123,79,296,321,193,67,291,361,361,357,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,261,171,85,275,372,383,65,95,341,114,294,31,254,252,341,0,270,31,258,132,367,5,385,388,312,0,357,159,15,141,338,123,369,258,294,387,370,335,217,250,250,6,5,376,370,19,381,210,251,318,386,380,358,117,217,293,39,243,132,195,262,42,132,0,382,239,208,254,39,7,388,141,162,253,254,119,222,102,56,39,347,285,128,7,388,162,78,78,225,386,117,135,72,275,176,380,183,293,143,176,141,356,344,367,303,20,101,321,38,313,254,195,225,141,242,133,290,311,132,380,89,135,261,39,298,250,376,307,367,398,335,21,405,393,381,183,308,303,9,337,335,258,293,202,196,92,62,340,142,369,307,37,328,12,279,53,343,252,38,284,187,259,96,65,129,252,298,254,178,356,387,174,105,361,352,357,70,24,19,321,401,151,297,136,203,101,197,284,242,231,133,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,152,366,133,357,210,353,36,337,281,221,296,307,386,355,127,15,358,126,357,403,352,253,131,65,163,163,37,140,328,128,100,72,344,347,254,31,11,359,337,0,338,400,100,103,258,273,264,41,312,312,273,9,79,359,62,388,231,347,261,344,137,201,361,0,47,290,314,380,237,171,318,123,388,333,343,338,255,322,386,79,231,263,380,341,293,148,273,280,312,313,140,40,46,111,89,328,31,307,173,38,39,107,183,182,400,342,42,380,255,317,124,388,255,109,363,5,388,36,191,25,154,39,293,394,135,33,152,16,134,359,103,254,131,246,167,77,344,276,109,373,361,355,38,212,121,108,314,218,177,38,128,135,131,27,316,60,20,36,143,308,12,92,398,332,251,258,77,124,135,314,359,340,133,365,211,183,398,376,251,49,131,61,285,259,136,157,60,347,253,5,141,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,360,79,197,198,131,315,62,165,141,360,275,360,360,217,130,136,18,223,98,388,388,162,198,133,347,176,316,7,308,183,186,268,321,358,169,280,298,54,218,344,144,171,164,126,363,183,181,254,254,254,165,133,363,195,33,363,54,347,293,188,195,198,39,0,54,185,316,289,67,333,61,135,165,188,109,109,252,355,149,361,114,8,249,67,154,88,206,117,28,297,111,352,135,37,386,78,367,7,352,78,221,67,264,361,150,135,252,327,346,78,358,343,202,391,2,65,386,298,175,262,350,361,135,290,135,398,74,173,14,315,346,346,354,252,252,143,381,0,8,367,165,337,295,132,132,367,135,323,73,388,255,262,382,398,398,398,258,177,398,42,75,391,33,115,240,179,0,252,243,179,92,86,138,384,361,292,133,0,388,85,245,85,382,157,311,367,131,131,130,347,323,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,315,65,352,33,358,296,202,202,397,48,341,11,8,136,295,123,86,139,353,88,258,123,357,376,86,294,141,358,86,353,240,386,178,116,205,15,350,129,400,296,87,69,338,72,2,19,4,181,159,323,296,127,353,160,352,135,137,388,326,121,94,345,114,0,144,344,161,390,342,295,385,385,114,116,335,322,398,404,390,290,217,342,252,387,361,3,335,177,34,336,346,87,353,361,370,128,352,202,360,363,298,337,259,193,210,133,358,48,359,313,251,135,146,116,339,34,252,116,357,171,131,196,373,347,116,205,51,217,383,128,114,353,45,175,316,138,246,177,342,361,136,127,59,360,360,171,316,326,346,87,294,384,116,361,345,349,290,135,10,138,357,2,360,275,197,171,37,159,69,0,359,130,184,273,181,188,185,289,317,357,391,369,361,123,298,27,295,379,25,352,128,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,362,257,88,243,88,178,155,0,228,323,166,404,203,127,331,252,363,400,336,46,166,167,181,247,391,0,270,6,78,62,171,109,140,14,38,50,275,254,367,341,355,85,399,270,385,65,192,39,361,94,102,388,134,290,335,140,192,306,368,87,72,254,48,0,298,124,348,41,41,307,369,137,131,193,243,4,88,361,92,210,123,360,391,348,359,79,384,370,404,295,78,183,307,210,386,306,342,106,10,205,347,360,360,296,221,14,78,39,163,117,8,85,389,59,50,358,92,298,292,307,294,210,365,388,402,60,104,251,5,92,20,406,14,380,306,391,240,296,106,31,359,140,179,195,171,273,375,0,365,301,252,386,361,345,386,171,285,147,19,270,7,270,19,399,41,361,50,405,404,83,83,367,84,102,382,171,362,388,296,92,275,361,155,260,283,354,78,47,206,373,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,363,136,179,382,355,280,37,11,325,162,138,20,12,277,342,166,179,89,252,338,144,95,309,261,366,181,242,395,65,353,31,375,366,14,65,114,388,270,11,344,126,335,231,132,323,131,346,184,129,29,88,307,306,73,258,363,141,161,398,33,337,138,171,0,19,343,90,318,388,164,173,398,171,252,19,187,307,254,252,24,33,63,87,236,9,129,34,375,176,367,367,104,110,72,96,0,57,85,38,382,252,368,357,367,263,361,270,252,398,309,164,401,401,88,37,259,221,335,183,116,136,41,141,141,38,55,153,356,11,398,399,18,242,70,42,34,196,124,133,109,342,62,109,67,388,263,0,92,101,242,290,361,294,78,92,128,337,132,400,133,361,342,357,357,382,367,117,340,244,14,359,111,352,279,358,163,357,12,128,160,136,160,306,89,332,388,207,193,123,123,325,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,367,366,51,353,358,386,244,69,136,360,77,367,337,195,65,131,136,274,127,168,263,345,296,316,370,294,0,0,132,361,33,92,231,198,104,137,314,359,288,368,399,203,309,132,359,136,127,95,254,189,368,273,296,388,338,325,273,317,381,306,295,150,342,0,104,252,149,312,237,318,98,48,67,226,131,103,255,344,195,352,96,398,332,256,358,67,132,388,62,181,283,342,131,173,128,365,254,111,94,94,301,124,294,55,242,35,134,123,126,109,347,361,97,148,109,109,191,385,395,168,166,37,51,39,351,259,302,326,107,376,309,169,183,174,198,182,35,309,150,16,183,140,385,380,298,166,207,358,323,242,391,288,263,133,51,131,357,354,388,355,388,404,51,251,298,124,160,386,290,342,48,67,87,205,341,268,177,179,256,344,278,85,382,167,370,304,330,38,38,197,309,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,143,67,128,131,231,78,223,277,46,30,88,260,266,120,258,104,179,167,97,342,295,399,282,131,73,138,133,42,58,298,134,177,212,362,128,290,404,353,38,95,141,404,361,251,89,128,61,131,311,248,38,42,392,109,131,136,132,381,133,255,62,361,16,0,307,293,171,362,41,315,199,252,329,18,130,131,254,198,223,361,97,147,362,276,259,357,252,198,384,109,48,131,160,249,64,42,98,199,366,388,127,254,169,176,280,183,171,54,42,201,128,234,184,388,97,74,368,171,87,398,308,173,134,42,259,391,184,357,171,391,37,136,79,309,125,221,367,181,164,135,344,109,141,261,178,363,290,362,366,363,267,225,20,33,165,141,302,293,404,51,263,262,26,0,367,188,171,54,185,317,143,357,165,165,391,188,171,6,211,367,178,108,361,254,352,35,42,252,146,362,143,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,342,352,343,160,166,348,308,2,43,44,258,254,348,147,45,105,362,367,346,360,358,316,265,234,123,346,126,352,241,342,346,352,0,114,42,361,79,367,49,126,21,21,141,254,39,285,39,285,389,311,311,171,376,367,171,109,79,385,338,205,359,295,352,0,143,253,261,309,259,361,327,360,25,196,69,386,370,338,41,321,137,291,362,68,140,160,54,60,74,134,173,18,74,133,133,391,259,361,288,264,150,346,355,128,264,131,234,128,75,361,346,107,195,2,136,243,33,0,0,243,176,132,281,207,332,355,83,273,344,141,261,231,0,240,141,78,14,282,193,311,298,332,150,314,282,298,207,193,326,273,382,284,382,197,183,69,143,252,272,214,18,294,252,342,259,356,72,358,370,38,19,61,303,168,217,85,109,235,376,199,68,398,45,126,85,58,332,192,261,39,254,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,341,341,328,140,388,388,254,138,138,277,144,121,263,15,114,389,86,143,127,224,352,352,370,337,60,366,123,130,224,195,198,40,238,14,388,237,399,371,179,376,89,195,371,332,386,386,39,84,322,306,159,139,141,69,132,389,128,12,376,391,366,104,123,0,248,50,65,309,38,306,141,354,295,354,92,131,107,60,362,390,296,128,372,38,337,122,56,306,83,349,92,285,388,362,341,149,188,357,321,206,169,361,198,263,249,346,347,309,159,257,141,248,350,388,127,203,341,262,361,138,296,0,33,137,176,261,337,114,192,65,20,73,121,356,38,165,114,171,101,173,48,121,361,326,167,343,40,139,311,38,240,344,286,161,248,114,138,218,328,346,325,79,370,171,303,306,169,303,74,370,120,349,297,49,383,153,335,332,72,340,141,293,174,129,314,257,63,142,123,107,114,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,68,242,116,34,252,86,39,183,141,169,385,248,322,352,217,183,198,138,178,187,262,367,294,123,123,368,164,259,254,388,362,20,12,368,202,279,87,368,136,131,293,285,252,243,342,368,274,126,133,229,261,325,85,340,65,337,275,28,117,357,102,198,193,0,363,357,259,295,195,372,129,209,109,39,79,173,252,252,195,322,52,337,25,128,249,352,131,133,89,361,362,296,136,387,316,367,14,202,296,328,295,365,141,72,229,140,261,317,181,130,368,210,237,366,263,395,174,33,344,48,117,399,191,363,163,89,182,337,343,363,249,137,109,313,368,173,359,197,397,38,315,314,333,103,284,380,296,187,189,195,372,341,338,255,376,296,2,370,362,309,313,296,2,237,42,339,26,201,98,354,127,342,65,388,363,136,98,311,135,128,362,97,181,48,346,367,351,123,14,16,388,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,134,154,294,291,198,5,183,123,366,33,85,365,110,191,259,367,131,356,35,342,131,182,181,137,111,307,388,94,255,135,51,48,394,234,320,360,342,198,317,291,114,173,333,39,95,89,399,148,242,383,395,352,130,133,357,302,346,55,321,134,51,363,121,0,348,298,126,51,161,352,388,246,298,135,249,345,128,136,341,235,131,337,309,254,342,342,312,77,202,278,309,386,51,361,385,335,290,355,160,360,380,282,118,14,33,298,251,177,337,285,285,64,139,124,135,167,56,121,114,370,179,121,92,116,117,344,133,290,342,367,183,223,367,177,199,61,126,376,347,367,57,346,178,191,50,365,128,25,173,67,360,353,386,191,60,361,311,243,141,0,258,398,225,340,258,88,285,288,130,42,314,307,70,285,391,173,217,296,298,198,223,363,363,197,137,252,131,404,340,318,362,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,362,76,50,130,138,165,355,146,337,18,104,254,363,18,162,87,26,194,133,76,188,54,184,171,366,358,183,306,388,363,74,336,128,347,237,294,16,33,205,179,133,285,38,104,18,130,344,183,258,273,171,38,346,363,311,76,125,354,391,181,164,76,85,0,123,164,298,178,363,217,131,173,133,363,177,361,133,33,59,267,133,165,85,302,368,376,89,293,169,165,51,262,365,254,84,143,357,363,280,54,185,357,171,198,290,316,61,135,33,363,116,391,165,165,211,335,185,263,344,357,95,357,367,108,346,346,128,121,394,380,347,39,372,252,389,88,224,338,251,361,243,146,237,146,89,207,175,108,348,108,108,15,58,343,387,391,150,384,92,6,347,347,178,42,91,315,358,326,290,337,127,390,135,352,120,263,126,351,351,344,362,341,390,359,296,337,326,354,372,146,343,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,128,387,249,128,108,36,128,37,0,176,92,139,36,251,325,398,325,113,166,290,352,39,273,123,342,298,130,355,155,142,346,342,395,144,65,36,252,0,184,357,141,177,161,129,331,342,55,341,121,35,92,340,51,89,248,260,279,342,260,348,361,135,367,0,96,243,350,336,295,34,292,363,0,101,337,37,117,160,389,75,128,89,173,349,135,14,363,391,337,329,290,342,353,229,33,357,139,139,195,337,380,137,260,274,126,85,261,311,358,176,136,284,105,370,210,296,174,103,317,35,370,20,360,347,342,341,370,117,117,342,370,209,290,260,359,198,173,363,341,102,357,255,14,16,51,183,133,182,242,311,361,326,143,400,123,361,388,337,179,266,234,269,139,161,54,379,133,342,342,361,346,39,127,33,360,254,357,355,346,144,64,77,294,256,88,306,367,177,326,292,88,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,355,165,195,317,361,139,196,138,0,363,361,354,165,315,25,342,264,51,254,128,391,178,367,361,347,384,128,58,0,19,276,117,355,137,41,318,305,117,165,16,273,176,205,279,184,176,5,355,159,305,175,171,183,143,175,355,342,347,181,360,25,267,369,0,165,51,40,107,141,36,198,317,165,391,165,178,367,384,387,239,38,337,114,143,360,402,83,46,150,374,69,255,365,255,237,35,133,46,367,380,199,237,20,46,366,74,38,224,140,314,270,192,86,96,357,261,11,306,41,98,207,65,263,263,399,192,152,254,171,250,96,388,12,263,48,132,89,150,141,42,342,11,188,136,189,282,337,193,77,326,256,342,171,74,176,236,245,20,342,41,337,268,41,6,131,32,398,117,148,240,122,356,362,126,395,159,389,207,243,141,39,295,332,178,243,217,359,366,354,60,20,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,155,344,353,117,384,136,355,290,296,326,72,192,296,343,11,388,357,288,114,138,236,87,346,243,252,16,362,164,133,255,161,357,400,388,387,337,176,0,400,109,89,340,368,45,329,132,117,132,15,366,359,312,249,203,308,189,368,59,367,296,116,255,361,0,386,26,116,191,134,193,33,5,97,55,88,14,14,127,21,177,352,395,346,305,346,297,77,135,98,142,74,128,124,344,344,223,15,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,130,275,197,176,212,108,297,183,314,344,198,267,212,188,344,252,143,200,50,183,309,261,93,168,6,121,65,45,95,367,252,367,34,121,341,92,136,356,196,9,369,143,196,143,362,60,247,255,361,95,248,92,353,50,103,132,292,50,266,295,294,315,402,0,179,14,386,137,135,133,171,108,344,390,357,351,358,352,309,258,151,355,10,345,343,109,40,352,240,167,61,363,338,79,38,65,341,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,341,38,144,195,11,325,344,42,114,353,357,353,351,92,342,178,171,137,313,89,335,67,247,382,285,202,367,164,202,336,367,322,42,240,38,79,34,13,183,107,68,240,255,398,258,84,137,74,136,128,367,195,48,353,321,349,59,274,202,133,337,276,126,0,347,45,133,394,34,173,263,75,356,127,179,296,273,313,359,338,341,386,137,363,189,318,179,171,166,322,255,57,143,384,18,298,34,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,161,394,51,361,252,138,313,261,5,173,196,362,124,337,341,64,351,379,355,262,61,177,175,143,309,109,138,85,137,179,131,128,138,2,375,61,135,110,316,128,126,285,311,322,42,367,137,183,244,298,404,165,342,399,353,276,340,94,169,74,171,388,274,0,171,373,260,321,109,311,163,181,183,171,165,363,198,345,260,107,62,373,126,20,20,388,171,0,345,0,8,241,86,60,0,0,333,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,123,361,0,142,34,39,0,61,197,396,248,15,390,0,50,363,252,344,182,65,234,394,177,341,60,339,363,357,322,114,34,259,294,33,33,277,309,294,183,89,380,92,248,70,397,197,14,179,312,179,207,322,92,101,136,92,322,207,0,134,333,58,210,0,207,207,28,322,94,14,58,41,186,85,361,124,293,367,207,144,361,179,69,124,61,396,134,38,34,331,169,31,179,69,177,134,41,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,243,69,69,257,60,298,388,209,14,96,390,343,352,37,354,252,343,55,14,400,354,388,84,388,87,141,290,221,40,326,136,337,128,296,39,195,138,346,328,192,346,310,251,78,297,361,242,383,107,189,177,131,322,360,39,14,202,76,150,356,53,294,389,0,39,367,89,358,37,328,111,338,126,155,362,143,342,107,361,46,128,230,67,383,294,53,361,400,308,285,68,182,182,258,92,175,256,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,179,167,344,87,60,362,117,344,107,109,225,367,337,365,169,171,298,60,177,19,17,131,39,358,136,387,0,171,130,164,131,353,363,362,365,316,67,188,185,185,20,0,6,84,84,257,131,375,193,235,50,178,205,131,242,97,79,114,14,41,254,2,346,0,116,127,116,376,55,116,346,360,116,135,2,349,127,188,245,117,135,95,239,261,264,380,104,144,144,390,380,0,382,74,252,363,11,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,3,3,3,3,3,3,3,3,3,4,4,4,5,5,5,5,5,5,5,5,5,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,7,7,7,7,7,7,7,7,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,9,9,9,9,9,9,9,9,9,9,9,9,10,10,10,10,10,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,384,117,117,20,335,367,85,92,261,62,183,382,171,137,352,133,237,3,183,352,41,61,3,104,171,95,192,252,368,344,348,342,89,202,203,296,129,237,86,193,344,296,358,387,359,294,130,58,386,159,352,294,195,201,391,386,296,41,217,399,35,86,198,0,181,326,388,65,213,203,304,386,358,205,389,205,195,292,114,126,38,57,142,79,191,344,342,161,290,322,126,335,170,296,260,176,387,10,10,10,10,10,10,10,10,10,10,10,10,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,12,12,12,12,13,13,13,13,13,13,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,11,14,14,15,15,15,15,15,15,15,15,15,15,15,15,16,16,16,16,17,17,17,17,18,18,18,18,18,18,19,19,19,19,19,19,19,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,171,164,311,142,243,129,183,97,390,367,129,189,343,349,128,110,372,48,133,195,122,353,322,352,84,340,109,198,339,131,61,294,260,205,88,376,383,361,68,154,205,304,51,182,39,191,109,126,304,342,251,294,175,36,344,360,354,206,109,133,360,2,128,0,133,384,195,41,284,38,355,359,130,197,199,247,199,169,159,353,199,130,183,197,181,107,191,342,317,147,391,258,143,305,376,143,294,19,19,20,20,20,20,20,20,20,20,20,20,20,6,20,20,20,20,20,20,20,21,20,21,21,21,21,21,21,21,21,21,23,24,24,24,24,24,24,24,24,24,24,24,25,25,25,25,25,25,25,26,26,26,26,26,27,27,27,27,27,28,28,28,28,28,30,30,31,31,31,31,31,31,31,31,31,31,31,23,32,32,33,25,33,33,33,33,33,33,33,33,34,34,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,122,363,390,135,379,369,6,70,252,399,341,385,253,153,60,143,171,92,202,79,148,388,136,88,79,385,311,333,137,205,402,367,150,332,79,207,391,227,248,178,239,168,20,249,365,159,152,263,391,159,79,253,183,109,2,344,357,248,167,121,259,337,256,0,0,152,150,33,166,121,367,346,343,192,181,326,36,36,357,288,161,104,0,124,183,37,230,307,398,152,357,396,153,38,252,400,259,34,34,34,34,34,34,34,34,34,34,34,35,35,35,35,35,35,35,35,35,36,36,36,36,36,36,37,37,37,37,37,37,37,37,37,37,38,37,38,38,38,38,38,38,38,38,38,38,38,38,38,39,39,39,39,39,39,39,39,39,39,39,36,39,39,39,39,40,40,40,40,40,41,41,41,41,41,41,41,41,41,41,41,41,42,42,42,42,42,42,42,42,42,42,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,177,144,237,202,382,117,342,170,362,341,252,187,96,276,357,70,332,363,150,337,305,3,75,343,61,65,212,322,252,77,89,264,264,189,102,309,79,179,306,317,124,243,160,322,343,252,173,94,370,396,163,0,359,394,33,252,256,242,179,183,147,46,37,0,362,169,245,252,391,55,400,49,187,131,290,167,379,133,342,70,67,237,371,256,65,171,133,342,383,256,344,367,396,256,130,376,169,42,42,42,42,42,42,44,45,45,45,45,45,45,45,46,46,46,46,46,46,47,47,47,47,47,48,48,48,48,48,48,48,49,35,50,50,50,50,50,50,50,50,50,50,50,50,51,51,51,51,51,51,52,53,53,53,53,54,54,54,55,55,55,55,55,55,55,55,56,56,56,57,57,57,57,57,57,58,58,58,58,58,58,59,59,59,59,59,59,59,59,59,59,59,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,136,42,360,264,61,361,243,243,367,247,252,2,150,133,367,276,239,50,20,358,189,348,159,169,169,388,171,171,85,264,239,363,171,181,181,205,302,107,165,23,357,271,369,314,189,347,359,39,86,76,40,0,361,296,366,388,323,92,14,310,195,32,184,0,367,341,391,109,343,388,97,97,303,137,294,94,161,65,318,104,107,404,178,183,62,388,14,42,128,366,362,399,130,386,368,341,344,59,60,60,60,33,60,60,60,60,60,60,58,60,60,60,60,61,61,61,61,61,62,62,62,62,62,62,62,62,62,62,62,62,63,63,63,64,64,64,64,64,64,64,65,65,65,65,65,65,65,65,65,65,65,65,63,65,65,65,65,65,65,67,67,67,67,67,67,67,67,67,67,67,67,67,67,67,67,68,68,68,68,68,68,68,68,68,69,69,69,69,69,69,69,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,291,388,361,195,306,65,11,386,365,131,97,317,306,189,314,92,353,252,367,131,306,33,60,109,311,171,225,198,62,171,369,296,373,169,274,347,328,270,399,31,361,344,218,261,88,116,370,388,138,388,367,155,8,243,217,171,366,243,20,178,218,406,388,0,141,130,326,296,130,117,362,399,388,270,74,391,225,92,97,355,303,161,95,138,321,298,356,187,183,104,406,397,9,252,388,161,170,70,70,70,70,70,70,70,70,70,71,72,72,72,72,72,72,72,72,72,72,73,73,73,73,73,73,73,73,74,74,74,74,73,74,74,74,74,74,74,74,74,74,75,75,75,75,75,75,76,76,76,76,77,77,77,77,77,77,77,77,77,78,78,78,78,78,78,78,78,78,78,78,78,79,79,79,79,79,79,79,79,79,79,79,79,79,81,83,83,83,83,83,83,83,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,242,19,402,367,186,0,361,342,15,131,92,243,231,136,389,400,352,38,62,344,399,367,352,388,149,104,273,311,273,131,148,290,198,388,131,140,309,131,363,338,261,0,361,127,257,131,268,251,135,398,389,404,184,311,225,285,128,267,231,367,247,399,330,0,338,38,130,159,184,239,285,398,171,54,333,367,294,138,359,167,359,10,19,333,391,239,68,341,109,46,359,346,38,154,60,398,352,83,84,84,84,84,84,84,84,84,85,85,85,85,85,85,85,85,85,85,85,85,85,85,85,85,85,86,86,86,86,86,86,86,86,86,86,86,87,87,87,87,87,87,87,87,87,87,87,87,88,88,88,88,88,88,88,88,88,88,88,88,88,88,88,89,89,89,89,89,89,89,89,89,89,89,89,89,89,89,90,91,92,92,92,92,92,92,92,92,92,92,92,92,92,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,309,107,398,340,333,356,260,359,359,323,35,322,68,141,175,342,341,160,46,384,158,54,38,55,175,375,54,256,260,73,375,181,257,42,296,92,254,42,121,252,116,295,88,200,154,391,178,181,19,7,306,121,386,68,361,298,138,262,248,171,394,30,64,0,56,334,138,147,138,137,242,39,192,391,335,135,310,258,356,277,368,115,29,14,361,72,290,343,391,208,104,257,202,10,28,84,178,92,92,92,92,92,92,92,92,92,92,92,251,92,92,92,92,92,92,92,92,92,92,92,92,92,92,92,92,92,92,92,92,93,93,94,94,94,94,94,94,95,95,95,95,95,95,95,95,95,95,95,96,96,96,96,96,96,96,96,96,97,97,97,97,97,97,97,97,97,97,98,98,98,98,98,98,98,98,98,98,98,112,98,98,98,98,98,99,100,100,101,101,101,101,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,227,92,85,95,85,296,193,296,260,202,188,109,262,39,362,359,14,162,135,312,275,131,115,389,164,61,28,92,328,171,166,141,107,133,114,365,343,388,38,310,391,406,325,28,97,95,159,355,0,346,250,164,401,236,243,98,39,106,367,133,384,123,387,0,288,390,183,177,265,92,380,104,254,144,47,28,402,20,353,15,312,344,248,349,123,296,369,48,184,341,72,305,131,136,349,195,85,101,101,101,102,102,102,102,102,102,102,102,102,102,102,102,102,102,102,103,103,103,103,103,103,103,103,103,104,104,104,104,104,104,104,104,104,104,104,104,104,104,104,104,104,104,105,105,105,105,105,105,106,106,106,107,107,107,107,107,107,107,107,107,107,107,108,108,108,109,109,109,109,109,109,109,109,109,109,109,109,109,109,109,109,110,110,110,111,111,111,111,111,111,112,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,45,89,391,121,257,261,200,254,104,296,355,117,317,369,41,279,387,221,254,346,102,238,74,398,42,289,391,26,181,133,21,128,14,65,51,357,286,394,243,245,367,394,376,289,109,361,123,33,154,248,375,131,182,131,218,311,54,68,313,168,174,346,20,0,198,306,317,175,60,92,133,202,160,59,135,312,167,346,183,296,373,252,236,252,243,95,141,183,183,357,61,281,392,103,254,173,296,113,113,113,113,113,113,113,114,114,114,114,114,114,114,114,114,114,114,114,114,114,114,114,114,114,114,12,115,115,116,116,116,116,116,116,116,116,116,117,117,117,117,117,117,117,117,117,117,116,117,117,117,117,117,117,117,118,118,119,119,119,119,120,120,120,120,120,121,121,121,121,121,121,121,121,121,122,122,122,122,122,122,122,123,123,123,123,123,123,123,123,123,123,123,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,165,160,367,369,116,386,315,321,221,41,361,252,320,394,390,8,308,390,254,398,320,183,183,133,332,363,367,164,181,257,173,165,254,369,389,141,173,15,75,403,171,296,188,363,369,398,367,85,294,385,294,228,117,217,56,383,254,381,243,8,341,288,138,0,274,14,311,362,385,198,315,123,95,141,205,390,171,309,121,326,285,177,395,7,167,83,262,117,344,92,171,369,183,141,252,7,383,123,123,123,123,123,123,123,124,124,124,124,124,124,124,124,124,125,125,125,125,125,126,126,126,113,126,126,126,126,126,126,126,126,126,126,127,127,127,127,127,127,127,127,127,127,127,127,127,127,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,129,129,129,129,129,129,130,130,130,130,130,130,130,130,130,130,131,131,131,131,131,131,131,131,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,124,124,282,317,15,199,400,133,171,347,92,231,11,104,351,132,395,317,282,88,134,207,282,282,231,342,174,157,20,126,298,131,344,329,221,171,406,65,221,323,165,306,140,102,387,140,366,131,31,390,355,369,121,367,117,335,270,338,261,208,399,327,224,0,91,136,298,48,243,386,288,121,388,131,88,370,270,60,137,86,402,140,371,6,92,388,252,41,121,371,342,92,361,294,20,391,262,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,131,132,132,132,132,132,132,132,132,132,132,132,132,132,132,132,132,132,133,133,133,133,133,133,133,133,133,133,133,133,133,133,133,133,133,133,133,133,133,133,133,133,147,133,133,133,133,133,133,133,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,178,391,292,95,358,92,332,386,59,42,296,389,353,402,139,8,262,205,298,404,159,138,270,115,361,136,391,41,156,205,143,159,123,50,126,101,318,347,155,135,263,94,188,353,13,344,92,99,326,273,68,362,169,347,142,94,69,326,306,134,345,128,143,0,133,142,39,199,386,184,38,261,298,9,326,292,126,344,101,351,321,351,347,121,342,92,325,311,76,161,92,138,123,388,344,139,89,133,133,133,133,133,133,133,133,134,134,134,134,134,134,134,134,134,134,134,134,134,135,135,135,135,135,135,135,135,135,135,135,135,135,135,135,135,135,135,135,135,135,135,135,135,135,135,135,135,136,136,136,136,136,136,136,136,136,141,136,136,136,136,136,136,136,136,136,131,136,136,136,136,136,136,136,137,137,137,137,137,137,137,137,137,137,137,137,137,137,137,137,137,137,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,131,352,270,400,294,78,171,184,174,41,263,292,252,397,252,335,259,344,297,337,318,335,96,336,13,396,24,111,55,187,179,252,382,14,35,178,199,252,257,322,400,129,401,342,399,349,174,137,87,277,202,367,51,85,184,352,363,291,252,352,345,133,150,0,344,276,199,131,75,389,65,202,193,368,347,10,306,261,15,126,101,400,199,337,92,337,327,103,200,347,173,400,244,110,362,321,105,137,137,138,138,138,138,138,138,138,138,138,138,138,138,138,138,138,138,138,138,138,138,138,138,138,138,138,139,139,140,140,140,140,140,140,140,140,140,140,140,140,140,140,140,140,140,141,141,141,141,141,141,141,141,141,141,141,141,141,141,141,141,141,141,141,141,141,141,141,141,141,142,142,142,142,142,142,142,143,143,143,143,143,143,135,143,143,143,144,144,144,144,144,144,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,388,370,38,33,59,343,368,400,352,295,337,101,0,363,137,361,396,217,9,123,237,390,133,57,263,302,370,343,311,342,273,318,92,370,386,97,273,123,379,320,344,309,386,400,318,127,24,14,89,53,171,313,357,342,400,169,142,254,191,388,184,207,245,0,173,353,400,131,344,311,184,13,361,284,204,255,129,344,131,288,351,266,353,311,256,379,406,388,290,282,177,143,85,175,35,405,133,144,144,144,144,144,145,145,145,98,146,146,146,146,146,147,147,147,147,147,147,148,148,148,148,148,148,148,149,149,149,149,150,150,150,150,150,150,150,150,150,150,150,150,150,150,150,151,151,151,151,152,152,153,153,153,153,154,154,154,154,155,155,155,155,155,155,155,156,156,156,156,156,157,157,157,157,158,158,159,159,159,159,159,159,159,159,160,160,160,160,160,160,160,109,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,268,33,277,351,128,124,403,342,255,370,58,295,128,131,285,133,134,126,284,51,136,135,14,33,361,225,311,361,289,275,131,18,254,165,92,355,252,242,171,205,169,347,403,159,366,352,169,344,33,135,183,33,363,290,345,344,404,403,188,342,165,169,173,0,121,270,391,386,69,342,363,318,370,344,311,347,91,91,252,20,248,345,384,96,363,107,405,316,26,252,339,363,169,316,183,306,96,160,160,160,161,161,161,161,162,162,162,162,163,163,163,163,163,163,163,164,164,164,165,165,165,165,165,165,165,165,165,165,165,165,165,165,165,166,166,166,166,166,166,166,167,167,167,167,167,167,167,167,167,168,168,169,169,169,169,169,169,169,169,169,169,169,170,170,170,171,171,171,171,171,171,171,171,171,171,171,171,171,171,171,171,171,171,171,171,171,171,171,171,171,171,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,294,104,391,141,197,202,105,161,142,92,294,106,398,367,3,84,306,189,179,6,84,35,337,14,131,40,179,142,198,384,188,243,131,131,185,198,58,345,366,366,88,6,358,104,366,374,97,342,391,361,273,265,255,126,344,361,358,153,344,367,101,368,0,85,290,88,290,173,226,255,121,40,88,121,39,55,92,343,241,163,243,183,388,367,342,347,342,150,128,128,298,288,121,134,390,288,171,171,171,171,171,171,171,171,174,173,173,173,173,173,173,173,173,173,173,173,173,173,173,174,174,174,174,174,174,174,174,174,174,174,175,175,175,175,175,175,175,175,168,175,175,175,175,176,176,176,176,176,177,177,177,177,177,177,177,177,177,177,177,177,178,178,178,178,178,178,178,178,178,178,178,178,178,178,179,179,179,179,179,179,179,179,179,179,179,181,181,181,181,181,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,39,353,39,400,335,128,122,116,245,173,262,5,177,241,256,5,128,353,62,167,103,103,103,69,276,209,75,325,243,39,42,132,141,42,187,131,376,182,131,167,130,366,125,361,258,370,60,121,388,0,386,36,387,366,330,323,228,356,385,0,116,295,176,0,138,14,65,111,338,352,248,51,70,0,325,141,51,160,173,173,339,160,173,173,51,295,307,325,160,221,388,60,223,257,133,325,325,181,181,181,181,182,182,182,182,182,182,183,183,183,183,183,183,183,183,183,183,183,183,183,183,183,183,183,183,183,183,184,184,184,184,184,184,184,184,184,184,184,184,184,184,185,185,185,185,185,185,186,186,187,187,187,187,187,187,187,188,188,188,188,188,188,188,188,188,188,188,188,189,189,189,189,189,189,189,189,189,190,190,190,190,190,190,191,191,191,191,191,191,191,191,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,181,384,306,309,384,361,261,151,27,252,65,126,39,270,275,368,31,114,88,240,89,366,266,338,252,14,342,342,151,69,87,6,20,352,322,399,155,296,217,248,50,92,344,207,262,150,39,358,238,39,350,83,120,399,109,387,323,55,195,347,347,190,344,0,222,338,338,351,138,366,120,185,290,325,195,48,294,254,319,404,53,351,348,332,38,330,73,212,243,104,171,383,311,136,174,303,16,191,192,192,192,192,192,192,193,193,193,193,193,193,193,193,193,193,193,193,194,195,195,195,195,195,195,195,195,195,195,195,195,195,195,195,195,196,196,196,197,197,197,197,197,197,197,197,198,198,198,198,198,198,198,198,198,198,198,198,198,198,199,199,199,199,199,199,199,199,199,200,200,200,200,200,200,200,200,201,201,202,202,202,202,202,202,203,203,203,203,203,203,204,205,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,187,244,111,263,47,60,214,209,67,294,49,141,72,243,111,340,195,276,394,39,188,234,65,3,212,301,370,389,274,328,337,135,132,75,14,34,344,333,297,317,309,396,361,175,131,243,347,184,234,34,183,130,238,44,134,92,394,183,135,363,184,354,29,0,326,217,175,55,160,346,331,85,388,135,123,55,0,345,311,88,392,60,157,225,141,42,142,98,173,331,252,55,18,355,83,262,374,205,205,205,205,205,205,205,205,205,205,205,205,205,205,205,205,206,206,206,207,207,207,207,207,207,207,207,207,207,207,207,207,207,207,208,208,208,208,208,208,208,209,209,209,209,209,210,210,210,211,212,212,212,212,212,213,214,214,215,216,217,217,217,217,217,217,217,217,217,217,217,218,218,218,218,218,218,218,219,219,220,220,221,221,221,221,221,221,221,222,223,223,223,223,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,344,16,349,161,163,357,183,130,371,188,262,374,185,185,373,254,340,139,177,108,135,131,97,41,199,221,98,133,388,388,351,315,344,31,264,367,349,141,140,348,293,140,266,296,316,251,123,107,252,322,343,62,85,235,315,85,45,183,178,343,260,89,156,0,92,375,89,171,367,166,0,20,254,141,127,400,15,207,69,73,9,31,361,26,182,59,354,323,64,61,255,183,361,131,133,340,252,223,223,224,224,224,224,225,225,225,225,227,227,227,228,229,230,230,231,231,231,231,233,234,234,234,234,234,234,234,235,235,235,235,235,235,236,236,236,236,236,236,237,237,237,237,237,237,237,237,238,238,238,238,238,239,239,10,239,239,239,239,240,240,240,240,240,240,240,240,240,241,241,242,242,242,242,242,242,242,242,242,242,242,242,242,242,243,243,243,243,243,243,243,243,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,183,183,33,302,133,248,357,357,27,131,325,323,135,242,361,31,199,95,367,343,399,128,311,388,331,337,252,338,270,92,391,124,143,131,5,42,193,271,115,254,366,361,257,243,321,65,131,365,11,361,391,217,235,19,351,359,344,121,380,72,69,221,123,0,195,277,105,17,337,92,399,388,131,51,0,354,98,171,273,32,37,367,351,399,176,341,78,109,50,103,108,135,367,399,240,207,389,243,243,243,243,243,243,243,243,243,244,244,244,244,245,245,245,245,246,246,247,247,247,247,247,248,248,248,248,248,248,248,248,248,249,249,249,249,249,249,249,249,250,251,251,251,251,251,251,251,251,251,251,251,251,251,251,251,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,252,253,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,254,391,126,92,94,314,45,138,83,3,256,252,248,254,167,298,395,58,28,367,74,337,171,73,92,270,362,251,370,311,38,37,341,17,342,101,391,205,395,406,332,261,313,37,242,197,349,138,36,144,357,57,114,57,132,336,309,274,292,344,338,136,210,0,207,328,0,192,50,65,262,72,54,163,183,337,221,161,251,372,97,111,187,41,47,382,196,171,10,258,142,258,65,288,137,384,315,253,253,254,254,254,254,254,254,254,254,254,254,254,254,254,254,254,254,254,254,254,254,254,254,255,255,255,255,255,255,255,255,256,256,256,256,256,256,256,256,256,256,256,256,256,256,256,257,257,257,257,257,258,258,258,258,258,258,258,258,258,258,258,259,259,259,259,259,259,259,259,259,259,259,259,259,260,260,261,261,261,261,261,261,261,261,262,262,262,262,262,262,262,262,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,101,124,258,3,336,13,390,133,177,316,322,62,123,117,48,34,126,87,164,288,142,361,292,50,107,28,386,376,140,14,361,197,0,239,70,135,150,345,335,141,367,92,173,353,210,28,340,48,292,244,207,3,363,130,162,134,199,406,406,402,274,342,360,0,3,262,133,92,184,133,88,121,121,122,357,328,399,259,198,127,294,94,390,254,337,20,337,131,75,359,311,263,386,129,296,165,400,262,262,262,262,262,263,263,263,263,263,263,263,263,263,263,263,264,264,264,264,264,264,264,264,265,265,266,266,266,266,267,267,267,267,267,268,268,268,269,269,270,270,270,270,270,270,270,270,270,270,271,271,272,273,273,273,273,273,273,273,273,273,273,274,274,274,275,275,275,275,275,275,275,275,275,275,276,276,277,277,277,278,278,279,279,280,280,280,281,281,281,281,282,282,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,359,368,195,370,298,394,107,266,354,0,124,365,308,362,296,48,296,370,386,275,171,264,126,171,141,342,42,298,328,179,130,67,254,402,249,57,368,42,367,237,251,208,342,88,370,279,26,198,312,203,308,179,342,104,166,94,57,317,188,353,398,109,400,0,110,406,323,28,240,0,294,166,178,323,351,193,326,42,3,173,51,248,261,137,48,136,337,330,27,367,361,399,14,183,21,383,169,282,282,283,283,283,284,284,284,284,285,285,285,286,287,288,288,288,31,288,288,288,288,288,289,289,290,290,290,290,290,290,290,290,290,290,290,290,290,290,290,290,291,291,291,291,291,291,291,34,292,292,292,292,292,292,292,292,292,292,292,293,293,293,293,293,293,293,293,293,293,293,293,294,294,294,294,294,294,294,294,294,294,294,294,294,294,294,294,295,295,295,295,295,295,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,134,191,357,178,342,245,110,114,65,53,293,291,65,355,20,65,57,282,353,123,5,198,184,389,249,134,198,51,220,128,144,362,290,362,111,37,123,288,154,254,189,26,376,171,337,131,355,295,197,234,33,67,277,169,367,256,42,124,133,190,370,10,366,0,183,268,79,321,87,143,87,275,88,160,304,356,352,92,61,341,72,306,346,342,280,370,292,133,312,178,367,343,306,225,353,370,367,295,295,295,295,295,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,296,297,297,297,297,297,297,297,297,297,297,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,298,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,346,116,2,128,128,131,50,345,185,201,170,134,25,294,255,173,150,368,58,388,317,382,312,344,85,70,347,104,391,133,149,280,355,359,7,76,247,368,223,41,190,245,252,375,37,386,83,217,363,51,346,252,84,133,352,160,15,68,198,165,137,260,257,0,0,254,309,184,361,352,347,361,163,169,135,65,388,11,359,205,126,16,308,316,330,0,256,337,179,128,234,97,370,10,171,391,380,298,299,299,300,300,300,300,301,301,302,302,302,303,303,303,303,304,304,304,304,305,305,305,305,306,306,306,306,306,306,306,306,306,306,306,306,50,306,306,306,307,307,307,307,307,307,307,307,308,308,308,308,309,309,309,309,309,309,309,309,309,351,309,309,310,310,310,311,311,311,311,311,311,311,311,311,311,311,312,312,312,313,313,313,313,313,313,313,313,314,314,314,314,314,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,177,353,258,164,376,341,277,277,252,183,309,330,192,370,248,367,355,131,139,353,261,309,139,89,20,267,361,344,367,141,344,173,362,255,363,181,327,124,369,178,359,195,114,160,165,131,61,191,169,169,128,307,388,337,160,125,171,131,169,125,188,131,0,160,183,133,0,320,169,263,346,361,185,196,17,306,346,42,123,352,57,92,352,183,123,116,135,141,10,357,382,382,160,18,285,298,314,314,314,314,315,315,315,315,315,315,315,315,315,316,316,316,316,316,316,316,316,316,316,316,316,316,316,316,316,316,316,317,317,317,317,317,317,317,317,317,317,317,317,317,318,318,318,318,318,318,318,318,318,318,318,319,320,320,320,320,321,321,321,321,321,321,321,321,321,321,321,321,321,321,321,322,322,322,322,322,322,322,322,323,323,323,323,323,324,324,324,325,325,325,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,68,261,38,0,143,385,293,367,114,399,345,86,79,6,39,254,338,369,369,144,252,326,388,368,264,128,258,252,389,207,336,88,88,115,92,266,239,217,319,248,39,141,171,92,380,103,243,243,344,68,17,19,382,324,104,363,98,50,128,192,92,176,361,0,344,171,352,248,257,293,361,336,205,257,109,260,167,349,136,261,132,330,36,11,114,353,294,92,344,174,14,353,366,136,60,67,128,325,325,325,325,325,325,325,326,326,326,326,326,326,326,326,326,326,326,326,326,327,327,327,327,328,328,328,328,328,328,328,328,328,328,328,329,329,330,330,330,330,330,330,331,331,331,332,332,332,332,332,332,332,332,332,332,332,333,333,333,333,333,333,333,334,334,335,335,335,335,335,335,335,335,335,335,335,335,335,335,335,335,335,336,336,336,336,336,336,336,336,336,336,337,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,307,141,252,367,144,342,187,171,69,318,161,114,114,9,87,337,331,370,313,254,277,217,337,174,72,79,8,398,336,25,198,0,163,136,325,193,352,199,136,296,353,357,274,337,92,368,195,337,276,347,343,363,296,40,317,391,400,39,368,197,163,74,261,0,342,340,5,262,363,134,8,306,50,342,339,173,237,273,131,341,351,114,306,243,124,361,74,209,343,123,109,189,361,338,363,319,389,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,337,338,338,338,338,338,338,338,338,338,338,339,339,339,340,340,340,340,340,340,340,340,340,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,341,342,342,342,342,342,342,342,342,342,342,342,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,26,0,252,323,291,65,183,337,388,37,262,243,367,33,182,258,362,134,338,346,191,385,385,189,367,175,51,171,191,346,34,205,404,306,261,319,388,242,256,262,17,175,109,342,131,394,87,175,143,143,355,33,135,212,346,206,40,355,306,42,38,61,171,0,60,361,138,58,252,50,345,293,258,363,32,171,377,353,391,376,347,192,347,252,273,133,116,398,136,18,117,85,169,136,163,202,171,342,342,342,342,342,342,342,342,342,342,342,342,342,342,342,342,342,342,342,342,342,342,342,342,343,343,343,343,343,343,343,343,343,343,288,343,343,344,344,344,344,344,344,344,344,344,344,344,344,344,344,344,344,344,344,344,344,344,344,344,344,344,344,345,345,345,345,345,345,345,345,345,345,345,345,345,345,345,345,345,345,345,345,346,346,346,346,346,346,346,346,346,346,346,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,261,221,183,74,346,42,181,171,181,238,360,243,291,104,363,298,342,25,262,263,25,191,136,298,398,127,228,91,389,190,83,146,201,342,147,368,262,178,353,298,326,345,344,123,337,62,40,337,62,397,95,367,361,92,88,136,389,60,361,389,136,388,347,0,266,388,144,32,128,266,332,178,239,359,406,14,292,141,117,354,386,361,235,92,65,92,110,388,388,266,361,332,143,103,353,33,262,346,346,346,346,346,346,346,347,347,347,347,347,347,132,347,347,347,347,347,347,347,347,347,347,347,347,347,347,348,348,348,348,348,348,348,348,348,348,349,349,349,349,349,349,349,349,349,115,349,349,349,349,349,350,350,350,350,350,350,350,351,351,351,351,351,351,351,351,351,352,352,352,352,352,352,352,352,352,352,352,352,352,42,352,352,352,352,352,353,353,353,353,353,353,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,367,217,20,0,231,252,155,270,134,405,205,136,83,275,391,109,362,145,349,0,161,220,298,132,161,171,142,294,136,386,179,131,361,21,395,303,265,171,351,33,161,318,368,178,39,34,41,174,87,87,110,357,74,388,252,252,111,151,252,321,296,92,40,0,347,69,161,329,351,351,368,92,327,329,357,128,239,60,370,314,103,125,273,368,209,139,313,237,283,220,363,136,125,155,173,171,296,353,353,353,353,354,354,354,354,354,354,355,355,355,355,355,355,355,355,355,355,355,355,355,355,356,356,356,356,356,356,356,356,356,356,356,356,356,356,356,356,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,357,358,358,358,358,358,358,358,358,358,358,358,358,358,358,358,358,358,359,359,359,359,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,184,221,69,347,344,337,16,27,131,286,250,14,92,133,394,133,55,131,33,371,85,20,345,355,17,268,191,165,5,376,128,27,311,225,33,173,61,298,382,14,165,92,388,0,298,333,296,11,347,20,37,164,181,342,344,165,385,59,141,54,296,133,361,0,165,356,342,89,92,0,6,117,131,131,133,107,15,357,109,143,244,193,198,198,201,296,306,33,386,143,198,323,173,359,388,144,342,359,359,359,359,359,359,359,359,359,359,359,360,360,360,360,360,360,360,360,360,360,360,360,360,360,360,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,361,362,362,362,362,362,362,362,362,362,362,362,362,362,362,362,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,290,337,342,322,367,165,79,74,258,238,131,203,363,103,262,382,137,107,64,133,185,262,133,337,135,262,188,165,294,131,107,357,322,261,137,53,388,35,131,60,65,347,263,98,296,109,136,129,261,349,217,252,183,380,14,349,291,102,388,123,42,342,361,0,183,143,342,357,342,357,70,92,261,261,135,131,85,355,68,121,32,318,352,136,60,270,355,362,290,252,332,131,355,362,79,88,356,362,363,363,363,363,363,363,363,363,363,363,363,363,363,363,363,363,363,363,364,365,365,365,365,365,365,365,365,365,365,365,365,365,365,365,366,366,366,366,366,366,366,366,366,366,366,366,366,366,366,366,366,366,366,366,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,367,352,367,367,367,367,367,367,367,367,367,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,359,307,294,362,342,143,208,213,37,366,388,350,86,348,200,293,357,280,397,352,361,361,309,39,117,294,117,352,386,391,387,103,399,36,104,92,133,69,178,321,358,212,237,390,95,361,141,359,380,332,361,262,384,248,14,350,42,20,58,402,318,391,50,0,385,365,352,355,361,127,98,296,31,346,296,119,74,103,109,263,128,136,124,94,345,337,294,41,326,198,203,186,128,357,350,105,83,367,367,367,367,367,367,368,368,368,368,368,368,368,368,368,368,368,368,368,368,368,368,368,368,368,368,369,369,369,369,359,369,369,369,369,369,370,370,370,370,370,370,370,370,370,370,370,370,371,371,371,372,372,372,372,372,372,372,373,373,373,373,374,374,374,375,375,375,375,375,375,375,375,375,375,375,375,375,375,376,376,376,376,377,378,379,379,30,379,380,380,380,380,380,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,19,323,39,169,391,159,156,341,131,388,270,53,166,79,159,80,325,60,11,33,366,152,256,258,299,3,367,346,38,136,344,341,341,97,307,21,128,138,305,386,305,74,124,34,303,136,150,262,51,346,311,336,344,87,164,314,361,217,362,323,243,398,33,0,37,397,131,252,316,396,337,141,259,72,387,57,401,254,398,174,133,42,343,187,294,16,124,244,367,69,352,244,296,353,296,129,105,380,380,380,380,380,371,380,380,380,381,381,381,381,381,381,382,382,382,382,382,382,382,218,382,382,382,382,382,382,382,382,382,383,383,383,383,383,383,383,383,383,383,383,383,383,383,383,384,384,384,384,384,384,384,384,384,384,385,385,385,385,385,385,385,385,385,385,386,386,386,386,386,386,386,386,386,386,386,386,386,386,386,386,387,387,387,387,387,387,387,387,387,387,387,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,79,389,65,347,92,251,325,133,252,367,399,394,342,128,362,3,344,210,37,89,391,358,357,127,353,98,231,352,206,360,337,349,320,390,290,239,76,127,130,98,363,198,308,198,342,255,386,130,306,309,9,39,254,361,134,368,347,346,318,359,359,388,352,0,245,51,171,205,205,291,385,204,133,376,402,173,182,25,352,110,36,298,5,5,137,385,39,346,134,191,35,344,124,33,352,379,285,386,387,387,387,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,388,389,389,389,389,389,389,389,389,389,389,389,390,390,390,390,390,390,390,390,390,390,390,390,390,390,391,391,391,391,391,391,391,391,391,391,391,391,391,391,391,391,391,391,391,391,391,391,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,342,380,76,387,212,165,79,363,143,131,405,256,20,128,394,205,378,380,296,256,316,378,251,295,353,375,316,61,311,344,131,135,382,211,361,2,382,128,124,361,361,290,267,226,254,76,314,123,41,116,361,363,133,367,133,128,74,385,353,373,169,294,337,0,33,171,330,15,385,357,79,41,337,41,359,33,267,362,165,37,347,221,126,373,361,61,382,357,74,332,270,350,350,14,41,294,352,391,391,391,391,392,384,393,394,394,393,394,394,394,395,395,395,395,395,395,395,396,396,396,396,396,396,397,397,398,398,398,398,398,398,398,398,385,398,398,399,399,399,399,399,399,399,399,372,399,399,399,399,399,399,400,400,400,400,400,400,400,401,401,401,401,402,402,402,402,402,402,402,402,403,403,404,404,404,404,405,405,406,406,406,380,406,406,406,406,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,254,121,343,131,121,114,121,342,342,175,114,74,181,252,171,64,14,298,344,89,388,388,357,357,42,128,331,361,361,357,6,122,79,42,133,161,94,141,92,342,18,391,132,88,342,123,338,126,65,400,88,361,3,243,208,243,103,208,366,205,306,126,161,0,117,3,171,217,14,367,132,329,193,342,361,141,42,316,126,143,11,386,368,92,24,102,319,361,115,335,247,130,85,316,107,376,388,42,131,341,94,218,280,243,100,38,98,212,79,298,367,246,14,332,359,359,388,65,348,362,160,367,97,318,67,131,209,221,131,252,198,20,285,104,376,293,57,357,143,306,360,357,262,249,109,160,15,376,105,366,28,361,338,138,155,109,146,163,131,290,335,157,245,143,256,130,361,326,336,60,70,383,168,280,98,207,270,367,243,356,333,341,34,26,148,391,223,145,366,361,103,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,83,391,296,14,399,83,109,244,15,190,59,295,159,87,324,361,39,193,117,12,183,177,128,94,244,399,132,352,377,135,94,374,133,363,355,386,293,18,18,261,293,45,374,390,164,373,50,37,291,322,240,101,344,190,133,311,92,316,51,51,388,131,383,0,74,137,350,48,370,10,372,164,89,26,131,295,361,403,92,103,281,376,175,361,7,37,335,388,396,16,370,379,60,373,357,251,290,326,332,217,132,131,83,366,156,147,391,361,323,32,135,225,206,41,357,171,261,171,367,248,365,306,89,254,279,236,398,298,188,340,14,321,107,153,141,88,357,347,131,337,400,184,317,18,231,39,342,138,133,135,140,395,353,60,326,331,293,254,402,369,54,65,342,355,121,111,33,160,10,251,121,92,92,309,306,338,357,20,110,193,347,185,250,19,363,188,169,174,123,176,344,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,335,363,137,102,344,374,14,74,298,357,0,353,181,95,374,370,14,390,89,18,342,350,342,38,38,343,317,401,171,290,260,258,344,50,143,258,39,50,37,69,65,342,382,143,136,39,42,130,39,323,78,95,313,53,342,384,309,362,262,133,264,49,55,0,183,61,53,399,321,262,39,108,256,256,135,375,321,83,402,155,150,143,37,86,369,235,252,39,361,133,178,195,155,50,253,391,262,307,248,389,203,357,136,121,290,234,141,213,104,117,65,384,262,361,159,169,105,136,128,294,103,263,387,129,352,256,97,159,80,401,398,337,367,294,33,311,37,133,354,360,79,367,353,3,65,399,244,205,61,309,296,198,385,133,378,256,143,357,382,37,60,137,406,341,254,138,8,357,406,11,349,94,388,221,401,47,243,337,127,337,342,114,260,159,192,341,86,19,243,11,360,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,324,237,262,217,361,131,78,78,42,405,296,345,39,244,388,136,24,58,141,298,328,53,138,221,346,21,56,207,298,135,256,0,133,252,337,53,136,252,221,141,221,187,183,170,335,263,53,257,252,53,400,242,387,367,77,48,365,388,380,37,332,317,92,0,402,269,31,330,255,33,67,321,131,179,25,14,40,183,175,317,69,309,342,160,131,388,255,388,191,400,173,375,266,330,141,344,389,65,315,132,388,391,157,257,355,370,171,363,97,342,92,243,316,357,142,357,362,383,249,290,401,178,89,42,126,190,262,292,117,98,197,352,347,308,347,143,133,290,61,34,306,15,12,261,12,79,84,298,131,365,117,352,341,98,386,159,243,361,171,252,8,95,181,67,183,36,65,332,217,207,5,356,69,72,146,290,291,212,94,362,38,296,111,355,176,368,388,3,361,243,218,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,20,53,143,177,314,256,135,175,77,107,58,14,14,141,256,77,41,131,341,369,218,388,188,388,42,75,337,181,177,344,337,165,267,280,221,314,262,252,54,57,342,160,252,177,60,88,321,78,102,166,270,188,2,131,141,153,0,357,195,148,262,182,167,0,78,388,357,321,62,363,367,36,380,109,144,369,348,59,353,85,270,290,159,298,331,37,59,79,208,252,193,276,159,254,394,121,123,242,328,284,59,155,69,362,170,122,98,368,191,365,174,39,348,243,361,27,135,209,74,254,131,335,350,252,345,92,357,370,92,131,171,79,141,243,388,277,344,34,51,258,341,254,252,290,15,391,154,361,205,95,246,181,6,207,141,266,259,39,92,178,220,362,193,363,260,202,323,254,321,268,14,399,141,326,128,391,325,256,92,270,349,263,128,355,203,252,135,40,134,188,363,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,103,159,39,178,59,5,386,85,159,358,242,11,104,104,239,391,273,79,6,390,388,359,150,361,388,296,248,83,102,141,135,108,183,146,48,390,372,388,293,174,367,292,366,126,265,385,335,92,259,390,217,178,385,382,174,399,128,336,49,124,147,361,242,0,254,110,218,248,107,11,187,236,174,276,274,131,358,344,45,52,48,98,366,121,298,92,399,92,370,12,218,367,338,147,104,386,343,355,100,137,190,312,121,390,147,14,296,340,366,79,195,366,171,328,344,245,311,366,65,294,107,166,363,48,138,252,342,307,137,209,252,6,298,34,324,367,126,14,92,328,60,55,357,402,61,133,335,363,104,114,253,89,294,345,337,33,146,252,160,342,79,10,235,325,182,236,353,132,386,296,275,205,81,11,339,116,131,171,238,133,305,166,363,367,309,197,73,342,173,53,177,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,368,183,346,35,394,337,129,354,385,135,382,21,167,88,85,177,98,285,147,126,361,131,396,83,367,133,121,169,240,171,171,183,177,59,390,174,370,50,404,15,163,50,361,8,15,0,8,50,15,15,225,225,386,401,361,271,85,296,275,33,95,367,361,0,252,380,108,328,397,321,345,321,79,389,36,39,39,37,391,318,332,262,128,72,366,209,361,136,176,355,77,139,366,261,65,138,306,262,154,352,175,128,355,143,277,404,131,197,85,252,121,347,121,337,361,339,308,14,116,315,275,355,344,97,171,130,262,120,85,221,198,225,361,159,173,58,361,342,374,239,366,175,93,95,321,196,329,37,92,247,224,136,135,373,361,184,144,322,360,2,208,131,111,7,141,250,176,254,107,69,380,356,258,367,3,353,19,160,368,298,81,44,133,305,382,231,283,188,363,388,114,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,385,341,173,313,128,104,164,12,57,391,242,366,390,137,367,49,321,314,368,231,61,311,357,39,321,296,386,366,370,79,111,58,337,210,359,41,355,314,65,39,368,309,189,107,383,296,23,39,309,77,65,182,39,57,268,254,353,367,361,79,175,296,190,0,113,382,320,83,15,15,39,368,366,188,171,325,252,290,367,275,335,92,115,18,348,294,368,56,130,9,141,15,333,159,109,296,155,385,347,183,405,54,95,126,243,349,398,130,403,211,361,330,59,296,21,39,131,154,62,168,380,1,359,92,207,361,59,171,79,14,11,111,258,362,371,145,93,105,178,72,223,78,212,366,306,159,131,294,128,58,176,361,346,14,50,108,128,351,361,236,157,78,131,201,198,380,225,100,206,190,39,167,101,81,380,313,375,342,406,131,89,376,231,200,177,394,384,318,123,55,288,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,113,117,131,122,349,342,109,231,166,357,38,73,184,92,367,166,132,101,20,342,262,346,252,259,390,401,248,169,217,366,250,345,141,365,256,361,195,279,11,298,367,370,122,160,345,345,308,317,203,342,275,42,399,401,360,341,345,370,116,365,193,35,183,0,175,394,123,256,191,256,352,64,14,355,14,379,337,387,193,177,60,197,360,27,157,89,197,159,173,373,33,366,252,357,33,57,126,364,60,20,70,166,171,299,49,69,58,210,171,160,136,365,160,140,308,362,39,136,182,155,340,128,258,5,309,74,150,221,117,37,313,98,1,81,116,66,2,361,313,113,326,39,240,169,27,245,252,363,13,308,65,198,242,143,175,251,44,135,233,258,183,30,64,116,137,354,361,281,243,275,31,130,211,337,133,210,187,123,178,366,367,259,367,126,337,388,240,317,41,376,111,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,342,373,171,367,357,404,193,386,73,386,368,92,322,253,249,41,404,49,366,362,251,404,113,396,183,165,357,318,382,60,396,344,74,316,41,48,370,84,150,308,41,57,370,365,4,380,134,245,37,367,171,375,361,134,316,84,225,296,173,361,219,275,41,0,357,178,198,198,219,348,135,289,198,357,15,296,296,171,137,261,361,175,62,384,70,249,261,6,386,388,6,185,92,209,68,290,135,337,340,191,383,92,85,131,252,254,252,262,356,344,5,29,165,6,123,150,72,132,351,59,103,193,202,361,72,256,355,387,167,164,307,357,104,346,111,153,143,273,359,334,372,337,367,57,182,399,195,295,307,131,383,177,64,18,361,67,39,238,53,355,358,122,164,342,34,127,359,388,135,262,282,85,261,3,108,189,224,370,343,239,87,273,157,297,312,14,142,171,367,344,362,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,154,45,399,85,124,124,114,96,252,192,270,65,306,342,361,32,296,328,342,228,254,261,133,384,360,137,235,86,37,349,327,369,389,92,21,208,348,79,143,77,103,362,254,8,280,270,35,224,88,370,361,258,243,111,121,362,144,68,361,389,342,94,272,0,130,315,148,368,183,0,338,78,399,217,328,296,202,104,150,178,19,50,104,20,243,367,306,406,21,366,322,132,386,296,402,388,141,310,361,111,188,217,293,53,198,123,31,337,337,195,212,383,138,143,175,347,355,126,45,130,312,362,72,296,349,331,342,270,367,39,361,345,20,367,129,380,308,205,351,137,282,394,211,243,341,109,239,351,345,332,3,367,14,101,5,258,33,347,177,363,298,62,56,33,341,388,234,40,341,146,34,46,307,15,224,123,42,242,58,358,406,217,92,35,361,361,326,357,28,146,355,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,403,361,332,353,384,10,117,285,295,42,296,20,391,39,371,249,326,254,92,381,193,254,92,171,369,243,358,8,20,136,103,298,387,207,221,342,65,132,207,316,126,361,306,159,145,11,398,349,121,135,39,83,98,248,296,206,112,362,144,390,40,139,326,0,205,169,131,306,352,270,405,388,305,39,342,349,263,243,324,391,122,203,156,68,322,344,351,144,31,167,131,243,275,198,361,362,108,150,370,11,307,254,160,161,361,321,263,257,349,87,34,336,41,123,55,370,160,79,170,396,256,14,309,257,365,138,256,40,42,177,197,322,128,301,357,337,121,202,148,314,184,161,140,166,367,34,342,338,129,357,262,264,117,322,264,147,395,237,255,282,252,306,31,89,368,207,199,77,198,104,15,338,115,337,168,95,298,181,183,358,306,78,178,193,188,353,237,332,121,202,138,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,71,366,285,149,133,306,0,68,114,277,388,152,261,346,221,366,374,321,57,105,121,389,73,184,195,166,133,348,370,11,309,367,33,325,20,114,132,121,142,89,33,335,388,306,353,124,367,323,159,398,186,349,258,294,114,186,360,42,379,141,344,79,0,192,251,171,237,277,38,97,171,319,19,391,386,328,179,221,141,34,368,133,96,68,318,34,187,150,178,243,183,171,255,250,142,202,126,337,176,132,386,362,128,391,131,352,128,318,355,134,179,123,355,275,309,341,164,337,398,142,29,9,342,195,126,391,252,342,307,74,398,243,199,95,87,51,294,107,183,301,347,357,199,261,308,127,352,241,133,353,340,195,357,386,150,293,192,363,251,171,275,314,129,14,351,92,317,238,203,127,363,346,165,27,123,188,126,173,391,361,183,353,95,298,306,290,292,326,33,164,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,404,242,3,243,344,356,396,169,1,153,314,161,74,214,47,399,387,12,221,400,76,316,70,252,254,49,131,367,137,107,193,34,322,342,133,318,104,57,298,386,184,197,183,124,16,93,164,151,86,341,209,133,123,63,344,0,130,174,84,196,146,363,39,0,173,111,344,74,328,337,335,92,274,131,79,144,37,321,380,123,358,75,343,367,152,295,127,337,92,384,31,257,296,121,160,231,256,311,171,60,33,173,275,251,14,116,398,114,347,363,369,88,116,6,10,109,61,198,366,37,223,254,254,341,175,254,126,133,133,401,356,341,139,376,361,83,132,138,59,122,238,21,171,261,346,321,265,160,337,126,183,45,127,261,343,5,103,314,179,344,177,141,347,200,311,163,355,128,329,388,149,388,131,79,33,342,141,33,138,227,198,92,14,367,42,305,87,357,341,367,14,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,256,122,327,400,126,347,202,133,75,133,306,160,123,353,385,136,386,15,389,399,351,360,195,236,2,94,254,195,313,58,9,343,173,313,146,402,314,116,339,273,317,313,255,98,305,47,20,237,280,243,283,96,399,341,363,127,323,179,146,312,288,308,335,0,97,386,386,188,361,368,317,221,342,132,98,189,142,307,402,313,0,89,338,208,183,313,234,402,329,351,107,353,173,308,191,205,188,137,399,109,224,367,306,58,390,290,257,356,268,298,185,135,247,31,171,247,333,344,313,65,337,79,138,16,136,34,14,33,227,5,368,325,341,103,205,243,2,247,39,171,357,255,245,34,169,383,342,290,14,220,205,302,93,93,92,227,399,136,143,10,374,306,92,401,361,227,315,346,124,244,171,252,150,396,25,388,341,5,179,290,16,51,33,131,345,135,367,390,98,335,159,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,14,337,179,65,282,400,361,183,152,255,55,252,34,317,191,365,33,89,138,16,298,182,351,51,181,373,373,27,171,343,342,148,302,13,383,254,38,183,124,131,251,311,255,249,177,285,351,282,38,160,306,179,212,127,246,311,85,256,263,358,317,345,143,0,135,405,175,257,167,76,33,373,252,133,389,64,356,363,77,143,116,373,251,324,0,383,70,290,146,133,87,311,183,142,128,367,173,370,243,298,95,347,92,390,92,42,59,155,115,134,101,346,321,178,252,87,291,110,78,297,179,263,335,399,150,345,321,200,128,306,244,103,396,202,137,386,275,97,171,361,133,18,245,191,169,200,284,347,175,379,134,254,284,126,403,359,131,45,372,365,70,131,337,88,202,143,150,181,67,59,249,202,132,83,102,352,356,120,359,188,342,128,173,252,363,252,123,161,357,51,335,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,398,284,133,398,169,14,324,126,360,78,111,38,262,88,58,361,2,400,355,68,391,120,396,131,221,117,130,259,18,363,160,223,352,133,133,254,31,388,201,171,169,131,403,159,291,242,163,74,305,49,184,16,10,183,344,158,181,79,183,133,165,20,254,0,359,33,345,133,342,107,26,221,169,54,262,237,188,403,185,375,361,143,317,391,165,280,358,349,239,349,192,356,388,344,367,252,34,37,141,193,367,368,343,212,2,317,137,127,363,55,51,353,383,251,25,262,183,14,373,338,337,370,318,341,292,252,31,189,171,243,200,359,277,133,42,38,51,346,86,235,391,209,388,385,181,140,248,183,343,346,366,388,332,388,178,103,65,171,332,38,149,167,356,268,388,386,108,252,325,162,140,124,120,109,136,185,142,3,352,85,104,92,143,399,313,178,42,88,74,254,384,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,388,363,127,399,20,317,273,13,179,126,163,33,383,34,140,5,69,143,175,198,196,189,301,290,290,196,357,14,114,14,290,146,148,13,121,278,282,344,133,133,202,343,303,73,380,212,382,242,343,178,107,14,278,117,335,98,98,84,42,121,109,202,0,0,161,166,184,325,288,141,369,369,33,262,177,34,289,161,357,338,357,357,129,367,338,345,10,345,262,359,338,8,3,337,362,162,264,188,47,174,111,133,65,141,52,294,210,380,173,165,131,247,141,261,75,47,37,184,31,141,353,195,363,386,87,314,312,347,97,55,97,305,18,273,391,347,137,255,252,42,38,391,123,95,369,259,332,143,256,258,183,405,342,141,368,169,357,177,20,31,366,5,205,53,291,322,370,173,245,60,131,18,361,270,79,104,150,183,388,361,386,123,171,359,296,388,263,183,385,218,336,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,165,74,263,0,322,221,314,146,117,264,46,107,73,252,160,317,107,245,147,342,128,33,243,61,126,314,338,0,114,146,74,128,314,126,168,348,94,388,362,358,73,2,295,240,152,370,388,243,38,2,262,65,178,388,19,78,298,311,83,109,367,144,256,0,349,48,92,343,290,295,21,386,344,62,87,361,76,187,362,141,37,183,295,344,362,391,358,271,343,40,357,362,367,65,337,221,76,49,399,52,183,177,337,133,255,132,131,131,147,64,94,133,374,234,178,21,13,379,243,249,93,163,95,116,316,97,376,348,370,109,117,373,193,367,34,217,252,295,360,35,357,128,21,114,109,353,160,2,203,331,355,359,342,211,12,296,159,361,388,399,94,137,386,164,261,131,60,92,33,131,342,65,367,103,137,262,133,134,247,193,104,341,104,131,141,133,244,149,257,313,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,136,94,357,128,370,341,317,131,62,5,342,362,280,268,177,330,64,243,311,367,344,88,217,83,131,62,362,388,181,342,171,171,117,388,361,258,254,126,103,144,123,371,367,41,311,114,130,302,107,42,371,365,131,342,179,171,210,354,371,131,131,208,0,91,285,207,88,238,370,39,358,3,341,67,61,123,68,207,37,380,178,252,390,121,382,362,391,331,178,72,363,197,178,178,121,362,98,20,193,207,55,133,282,298,34,183,251,262,246,62,344,45,72,356,362,150,370,85,39,135,74,69,366,368,111,369,340,273,127,138,276,315,102,397,208,359,254,181,72,145,183,132,294,390,406,105,386,262,388,138,108,72,357,157,280,113,244,386,198,331,188,57,239,335,220,138,357,87,367,400,70,133,52,210,199,333,79,298,38,363,98,184,18,320,388,44,104,197,284,290,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,190,190,370,179,197,18,341,337,162,342,361,60,320,367,181,59,131,238,358,337,0,342,131,197,197,169,171,311,2,87,59,181,178,361,89,171,10,117,117,117,19,259,322,386,38,259,174,138,322,87,199,10,322,128,360,70,31,254,270,65,74,341,270,0,258,224,362,280,208,205,402,8,361,359,318,14,136,121,239,19,362,280,318,136,3,119,102,253,325,15,311,323,353,153,13,314,291,173,177,367,342,361,288,348,342,16,280,141,308,16,16,298,103,104,123,87,131,165,367,240,193,382,138,217,179,361,358,337,77,255,296,123,391,353,315,360,358,341,114,196,35,357,123,367,337,75,10,353,15,330,179,191,291,370,361,367,85,311,344,143,54,306,318,352,342,171,123,139,123,87,296,306,344,388,262,123,92,406,198,388,50,386,323,252,33,342,398,342,267,319,316,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,14,162,163,380,9,82,261,261,293,292,207,133,65,0,318,314,347,237,98,19,162,317,182,128,256,354,131,133,134,33,58,123,344,254,74,333,133,165,337,270,92,195,263,98,337,256,114,34,162,274,370,293,337,98,92,318,11,370,97,14,337,311,74,0,333,74,92,140,344,347,344,131,371,168,242,127,363,370,242,3,362,345,123,347,70,259,160,345,304,114,352,361,352,104,307,160,252,76,132,128,228,222,358,399,264,254,202,319,252,76,193,196,395,367,367,314,354,200,131,95,61,124,36,77,356,398,15,89,84,2,171,181,380,326,65,163,332,92,349,192,343,256,381,72,212,98,340,252,76,11,70,37,390,136,65,353,15,385,110,283,259,262,77,64,134,23,197,20,147,388,92,92,352,199,154,77,200,60,295,368,361,311,399,39,206,164,133,65,313,356,217,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,96,367,335,8,77,65,67,237,249,178,36,138,169,117,256,3,79,337,136,162,294,361,361,150,76,15,248,169,92,132,327,128,160,132,188,325,38,138,370,123,114,138,330,330,247,164,330,399,399,47,70,164,290,114,254,150,55,344,258,361,281,321,79,0,79,357,129,147,365,394,357,344,348,361,368,283,67,67,134,160,169,167,245,393,191,53,359,116,256,104,355,258,128,33,275,197,18,311,243,277,308,160,193,150,203,245,38,147,177,104,70,14,263,322,85,386,293,335,329,92,96,104,171,357,243,165,171,131,379,117,107,142,137,93,361,249,384,175,328,45,290,196,32,228,21,315,141,8,254,86,148,327,130,235,367,387,104,150,249,21,20,369,207,316,67,305,296,353,332,14,217,243,78,149,167,83,366,38,132,212,360,38,68,362,146,391,325,71,124,263,112,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,344,247,183,177,221,263,325,139,250,363,367,169,89,71,105,92,343,382,16,280,6,315,176,105,353,292,141,16,306,337,358,359,308,146,308,85,179,342,179,245,245,179,16,16,16,175,16,285,89,351,358,382,306,359,179,87,85,87,296,25,131,70,306,0,332,382,331,270,367,142,39,362,85,85,312,362,327,361,406,14,136,318,10,50,324,306,10,296,78,113,270,322,135,117,19,359,326,288,359,387,285,40,317,3,275,167,164,319,152,379,171,97,79,57,186,179,146,133,166,258,141,1,255,231,12,63,150,161,104,130,240,142,344,399,257,146,306,79,31,308,126,2,182,255,87,195,205,98,142,208,179,361,132,18,16,317,191,188,365,402,353,65,33,143,251,183,77,165,251,54,255,64,130,398,361,31,16,389,294,57,388,14,399,205,298,184,131,92,166,150,270,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,50,345,358,142,83,168,342,20,214,79,21,144,73,309,367,296,359,129,111,296,133,396,19,344,21,360,316,87,383,337,107,79,229,129,123,127,128,382,122,117,349,88,337,104,31,307,317,20,97,342,160,179,308,318,360,338,205,317,191,14,367,351,137,0,282,160,394,290,39,60,360,252,268,38,337,345,382,88,113,197,357,205,33,345,188,373,211,70,332,78,117,168,342,73,144,111,104,386,131,285,218,92,267,109,135,116,342,249,69,123,365,140,368,10,386,104,72,183,262,39,306,83,388,105,351,185,20,171,123,367,344,321,341,200,3,11,48,123,79,50,195,341,359,133,363,385,179,175,135,140,367,123,183,107,19,70,136,171,290,171,366,95,150,60,380,239,391,353,132,356,361,388,167,341,57,344,288,391,87,104,337,367,367,60,163,361,122,57,182,132,284,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,360,20,88,20,217,14,249,328,114,87,133,3,2,344,370,88,247,348,189,367,89,114,65,332,385,39,355,391,296,240,348,272,280,370,338,388,60,184,366,20,10,143,332,361,262,251,262,139,249,384,368,242,390,141,391,227,141,243,374,132,178,386,315,0,92,358,296,14,332,332,306,179,189,244,318,388,273,320,72,355,263,294,139,83,113,20,391,362,188,390,60,113,179,141,307,258,192,39,205,8,131,127,16,188,363,381,181,362,41,8,164,361,67,243,67,262,361,307,342,260,397,15,359,323,73,150,367,355,141,367,361,31,208,270,137,195,237,61,253,98,151,173,38,173,133,16,42,342,243,141,78,58,11,10,184,15,165,39,385,255,275,237,356,352,144,56,137,169,399,35,306,130,167,317,234,182,134,226,205,69,70,60,178,223,111,160,5,316,114,252,115,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,174,114,328,353,330,144,79,38,349,2,183,396,390,293,244,161,318,164,400,150,252,252,357,87,284,357,98,359,341,244,51,244,254,87,127,254,130,367,321,263,343,400,143,274,306,105,332,109,308,254,38,388,179,9,320,342,27,74,357,368,401,284,290,0,252,39,302,183,342,188,383,205,5,25,16,51,262,14,388,367,352,124,20,309,346,177,33,77,179,332,30,67,135,324,357,188,382,136,117,363,150,114,79,394,221,191,283,116,275,247,123,254,261,131,32,128,98,197,92,243,277,344,116,136,102,73,362,39,114,104,150,171,366,266,380,261,178,38,366,260,132,212,388,306,262,325,162,252,135,358,206,294,385,292,341,171,42,92,255,259,252,342,367,87,111,111,361,243,323,263,335,166,197,48,273,210,92,160,150,92,308,367,366,182,261,15,206,258,5,191,192,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,138,361,360,385,247,390,357,181,184,320,345,131,302,141,342,126,171,245,272,362,245,349,258,400,309,143,302,335,335,337,367,95,361,148,10,11,50,321,256,156,330,101,244,367,311,249,346,238,143,57,205,175,182,346,374,321,18,158,183,97,256,149,256,0,167,284,161,321,86,351,266,60,161,18,84,243,84,321,10,14,274,92,83,273,262,102,162,242,392,292,313,321,171,18,321,242,307,189,368,342,39,317,238,296,127,27,245,317,342,345,389,383,300,193,242,128,237,290,130,197,33,173,201,171,74,262,91,363,259,343,296,391,367,131,74,131,133,384,399,123,260,249,58,295,406,103,171,306,323,132,15,39,154,14,344,357,263,387,144,296,96,235,292,346,259,376,257,391,279,254,332,14,60,153,368,346,386,160,127,122,103,87,171,14,39,309,201,73,183,75,109,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,387,400,304,133,47,123,163,252,173,386,242,189,282,191,191,287,352,176,254,254,211,157,223,18,267,73,73,212,121,342,73,114,73,73,140,367,367,357,171,348,109,400,179,347,291,195,252,252,93,352,367,47,252,174,95,245,14,252,352,41,357,382,367,0,62,270,136,6,121,332,68,131,352,124,257,313,115,331,205,136,294,8,368,14,183,338,123,183,371,86,88,208,366,244,205,117,343,67,373,64,20,164,390,367,367,40,342,221,228,45,290,361,14,389,8,86,98,183,390,376,342,292,337,197,297,27,40,197,258,220,132,261,288,14,65,255,313,136,317,342,344,198,6,171,323,342,50,25,177,400,282,122,373,50,352,274,261,134,100,131,361,178,342,391,87,133,244,117,361,198,388,252,259,359,62,92,262,140,257,176,399,373,210,385,134,39,70,95,390,361,104,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,347,114,243,178,332,6,261,248,92,14,131,337,262,68,20,366,110,141,218,387,315,10,92,380,141,104,296,72,41,314,253,298,122,386,83,3,337,384,391,362,176,98,326,321,252,19,337,135,155,109,344,98,128,167,92,149,351,78,144,321,199,292,380,0,313,258,367,214,385,110,101,309,341,261,294,251,126,41,171,288,288,149,197,38,171,401,342,365,294,399,252,387,345,214,48,131,68,406,332,344,203,388,357,289,38,328,169,161,240,123,321,352,113,317,167,21,135,342,141,171,355,296,57,77,260,354,53,17,20,314,133,92,255,388,92,290,188,332,132,20,315,160,256,14,344,344,131,135,174,131,49,125,39,388,67,20,388,133,69,44,389,141,78,57,244,274,221,237,252,42,143,251,85,53,391,177,33,176,403,347,388,68,205,351,205,243,123,143,291,104,399,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,257,104,390,72,164,87,217,361,161,183,140,34,138,187,178,401,171,197,400,131,218,123,367,65,296,284,126,321,122,349,391,163,400,131,15,15,126,263,377,337,337,367,48,274,69,127,173,357,261,261,133,14,79,358,92,281,133,343,332,123,296,279,353,0,338,254,116,341,86,284,179,189,296,296,107,399,320,314,359,79,365,254,252,338,279,294,173,5,168,128,202,131,323,262,133,284,191,102,309,388,399,259,174,367,171,338,325,131,240,87,288,362,2,344,190,37,141,10,323,399,362,367,49,340,199,368,332,396,312,144,141,188,262,41,260,185,341,373,206,5,179,11,348,366,86,6,248,218,183,309,92,122,315,109,136,337,83,131,135,345,355,101,171,173,133,296,323,110,288,126,131,387,178,401,87,161,34,104,217,218,68,296,399,88,69,79,261,92,127,15,284,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,342,261,16,131,131,391,134,351,394,365,383,148,354,17,367,262,345,20,135,355,309,127,405,332,290,85,109,177,355,200,342,379,345,88,107,122,157,377,284,290,95,109,295,171,34,169,298,2,275,131,367,123,298,171,176,188,201,386,345,79,183,107,171,0,344,62,131,331,6,117,366,380,20,141,149,326,377,122,157,380,144,365,252,284,14,34,321,338,337,263,134,86,254,337,342,123,95,5,252,314,107,359,168,16,354,191,202,365,109,290,405,171,58,358,58,256,191,133,141,274,103,11,136,327,155,104,65,122,98,150,14,182,253,158,18,74,195,6,357,174,346,336,39,345,357,324,318,365,245,161,193,266,323,131,399,351,263,140,18,126,176,194,128,198,131,144,391,198,252,5,293,177,59,42,366,343,361,262,74,171,259,25,3,88,366,341,357,342,261,114,380,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,290,107,220,361,92,171,140,21,357,92,68,131,89,275,114,296,89,203,10,368,39,123,258,109,88,338,254,296,367,91,359,143,143,243,126,386,10,357,356,387,86,89,338,234,59,98,275,178,201,92,332,202,171,15,388,98,368,50,262,346,39,60,141,0,5,104,389,367,358,367,356,123,367,322,354,76,341,83,105,2,388,357,120,346,132,176,391,345,361,121,183,275,206,98,270,346,351,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,390,39,98,120,218,79,185,132,131,328,142,332,21,341,142,367,20,144,144,14,342,144,141,328,138,321,79,79,159,123,341,294,39,135,237,183,243,298,92,356,398,242,261,254,11,68,183,264,133,141,328,356,368,252,171,360,396,153,78,161,295,252,138,0,361,361,259,399,164,72,252,48,101,141,262,361,405,131,298,0,39,200,274,356,261,39,123,39,79,136,193,92,48,328,357,117,368,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,244,161,195,123,363,45,341,0,72,26,86,123,363,368,344,339,296,117,42,317,343,279,179,131,104,254,312,114,50,50,361,359,357,131,171,322,154,321,306,361,328,189,346,97,322,37,131,329,385,5,346,361,234,39,385,179,365,184,14,302,398,367,341,0,143,362,321,306,135,361,124,14,363,309,127,85,135,175,357,97,140,344,344,328,190,405,367,363,183,329,344,354,361,243,298,188,342,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,361,131,376,367,382,360,358,14,223,123,198,363,197,65,369,367,169,21,183,117,181,302,369,363,107,103,171,185,296,26,346,178,354,120,390,341,161,254,101,368,309,117,261,361,339,179,131,361,361,126,382,192,302,183,137,178,133,344,57,133,133,357,366,0,53,239,53,239,53,133,198,367,179,37,265,177,217,140,265,138,171,345,357,132,198,171,383,138,252,178,357,53,190,117,35,92,199,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,199,92,239,262,262,206,92,344,164,199,39,89,262,199,194,205,401,232,88,127,137,108,322,327,124,159,121,39,118,361,60,342,331,205,133,67,357,195,357,360,61,357,357,357,386,357,59,25,333,195,382,357,74,183,88,202,368,53,262,35,333,391,388,0,197,5,17,332,14,35,198,386,399,104,72,88,368,34,97,27,368,317,320,298,87,338,87,68,332,389,262,295,296,296,325,342,138,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,129,342,357,328,306,344,169,359,128,341,155,347,117,351,339,380,225,211,252,381,131,399,131,131,39,37,37,117,356,362,347,10,376,347,399,39,357,141,323,178,178,42,263,296,151,221,140,359,49,370,367,42,252,217,28,401,262,370,357,234,79,340,361,0,57,401,67,42,137,356,117,357,181,181,340,238,0,181,181,102,147,58,178,58,109,261,17,47,117,143,347,369,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
        };
        #endregion

        public static readonly Encoding GBKEncoding = Encoding.GetEncoding("GBK");

        /// <summary>
        /// 获取汉字拼音
        /// </summary>
        /// <param name="chinese"></param>
        /// <returns></returns>
        public static string GetChinesePY(string chinese)
        {
            if (chinese.IsNullOrEmpty())
                return string.Empty;

            byte[] gbkBytes = null;
            string[] pyArray = new string[chinese.Length];
            int code = 0;
            char ch = char.MinValue;
            char prev = char.MinValue;
            char next = char.MinValue;
            for (int i = 0; i < chinese.Length; i++)
            {
                ch = chinese[i];
                if (chinese.Length > i + 1)
                    next = chinese[i + 1];
                else
                    next = char.MinValue;

                if (ch < 0x4e00 || ch > 0x9fa5)
                    pyArray[i] = ch.ToString();
                else
                {
                    gbkBytes = GBKEncoding.GetBytes(ch.ToString());
                    code = gbkBytes[0] * 256 + gbkBytes[1] - 33088;
                    if (code < 0 || code >= GBK_PIN_YIN_CODE.Length)
                        pyArray[i] = ch.ToString();
                    else
                    {
                        #region 特殊字处理
                        switch (code)
                        {
                            case 21912://重
                                if ("庆复新申阳逢叠唱峦".IndexOf(next) != -1)
                                    pyArray[i] = "Chong";
                                break;
                            case 13692://都
                                if ("京丰武成昌新首于宜".IndexOf(prev) != -1 || "江匀".IndexOf(next) != -1)
                                    pyArray[i] = "Du";
                                break;
                            case 19829://蔚
                                if ("县市".IndexOf(next) != -1)
                                    pyArray[i] = "Yu";
                                break;
                            case 26293://珲
                                if ("春".IndexOf(next) != -1)
                                    pyArray[i] = "Hun";
                                break;
                            case 19102://宿
                                if ("住留露舍寄".IndexOf(prev) != -1 || "迁州营愿".IndexOf(next) != -1)
                                    pyArray[i] = "Su";
                                break;
                            case 16278://乐
                                if ("音声".IndexOf(prev) != -1 || "清器".IndexOf(next) != -1)
                                    pyArray[i] = "Yue";
                                break;
                            case 12214://埠
                                if ("埠".IndexOf(next) != -1)
                                    pyArray[i] = "Beng";
                                break;
                            case 16569://六
                                if ("安合".IndexOf(next) != -1)
                                    pyArray[i] = "Lu";
                                break;
                            case 12713://查
                                if ("济".IndexOf(next) != -1)
                                    pyArray[i] = "Zha";
                                break;
                            case 20099://厦
                                if ("门".IndexOf(next) != -1)
                                    pyArray[i] = "Xia";
                                break;
                            case 23145://郓
                                if ("城".IndexOf(next) != -1)
                                    pyArray[i] = "Yun";
                                break;
                            case 12130://阿
                                if ("东".IndexOf(prev) != -1 || "阿".IndexOf(next) != -1)
                                    pyArray[i] = "E";
                                break;
                            case 23486://荥
                                if ("阳".IndexOf(next) != -1)
                                    pyArray[i] = "Xing";
                                break;
                            case 12696://藏
                                if ("三西青".IndexOf(prev) != -1 || "族".IndexOf(next) != -1)
                                    pyArray[i] = "Zang";
                                break;
                            case 27496://歙
                                if ("县砚".IndexOf(next) != -1)
                                    pyArray[i] = "She";
                                break;
                            case 18802://什
                                if ("么".IndexOf(next) != -1)
                                    pyArray[i] = "Shen";
                                break;
                        }
                        #endregion

                        if (pyArray[i] == null)
                        {
                            code = GBK_PIN_YIN_CODE[code];
                            pyArray[i] = PIN_YIN_NAME[code];
                        }
                    }
                }

                prev = ch;
            }
            return string.Concat(pyArray);
        }
        #endregion

        #region json 格式化
        public static T ToObject<T>(this string jSon)
        {
            return JsonConvert.DeserializeObject<T>(jSon, JsonDateTimeConverter.Entity);
        }

        public static object ToObject(this string jSon)
        {
            if (jSon == null)
                return null;
            return JsonConvert.DeserializeObject(jSon);
        } 
        #endregion

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="length">生成字符串长度</param>
        /// <param name="mode">模式：1仅数字，2仅小写字母，3仅大写字母，4大小写字母，5数字和小写字母，6.数字和大写字母，7数字和大小写字母</param>
        /// <returns>随机字符串</returns>
        public static string CreateRandomString(int length, byte mode)
        {
            Random rd = new Random();
            char[] cs = new char[length];
            switch (mode)
            {
                default:
                case 1:
                    for (int i = 0; i < length; i++)
                        cs[i] = STANDARDBASE62CHARS[rd.Next(10)];
                    break;
                case 2:
                    for (int i = 0; i < length; i++)
                        cs[i] = STANDARDBASE62CHARS[rd.Next(10, 36)];
                    break;
                case 3:
                    for (int i = 0; i < length; i++)
                        cs[i] = STANDARDBASE62CHARS[rd.Next(36, 62)];
                    break;
                case 4:
                    for (int i = 0; i < length; i++)
                        cs[i] = STANDARDBASE62CHARS[rd.Next(10, 62)];
                    break;
                case 5:
                    for (int i = 0; i < length; i++)
                        cs[i] = STANDARDBASE62CHARS[rd.Next(36)];
                    break;
                case 6:
                    for (int i = 0; i < length; i++)
                    {
                        int j = rd.Next(0, 36);
                        if (j >= 10)
                            j += 26;
                        cs[i] = STANDARDBASE62CHARS[j];
                    }
                    break;
                case 7:
                    for (int i = 0; i < length; i++)
                        cs[i] = STANDARDBASE62CHARS[rd.Next(62)];
                    break;
            }
            return new string(cs);
        }
    }

}