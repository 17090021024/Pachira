using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace TrunkCommon
{
    public static class ConvertHelper
    {
        #region 转换string到目标类型
        public static byte ToByte(this string s, byte defVal)
        {
            if (string.IsNullOrEmpty(s)) return defVal;
            double i = 0d;
            return (byte)(double.TryParse(s, out i) ? i : defVal);
        }

        public static byte ToByte(this string s)
        {
            return s.ToByte(0);
        }

        public static byte? ToNullByte(this string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            double i = 0d;
            return double.TryParse(s, out i) ? (byte)i : (byte?)null;
        }

        public static short ToShort(this string s, short defVal)
        {
            if (string.IsNullOrEmpty(s)) return defVal;
            double i = 0d;
            return (short)(double.TryParse(s, out i) ? i : defVal);
        }

        public static short ToShort(this string s)
        {
            return s.ToShort(0);
        }

        public static short? ToNullShort(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            double i = 0d;
            return double.TryParse(s, out i) ? (short)i : (short?)null;
        }

        public static bool ToBool(this string s, bool defVal)
        {
            if (string.IsNullOrEmpty(s)) return defVal;
            switch (s)
            {
                case "1":
                    return true;
                case "0":
                    return false;
                default:
                    bool i = false;
                    return bool.TryParse(s, out i) ? i : defVal;
            }
        }

        public static bool ToBool(this string s)
        {
            return s.ToBool(false);
        }

        public static bool? ToNullBool(this string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            if (s == "1")
                return true;
            else if (s == "0")
                return false;
            else
            {
                bool i = false;
                return bool.TryParse(s, out i) ? i : (bool?)null;
            }
        }

        public static int ToInt(this string s, int defVal)
        {
            if (string.IsNullOrEmpty(s)) return defVal;
            double i = 0d;
            return (int)(double.TryParse(s, out i) ? i : defVal);
        }

        public static int ToInt(this string s)
        {
            return s.ToInt(0);
        }

        public static int? ToNullInt(this string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            double i = 0d;
            return double.TryParse(s, out i) ? (int)i : (int?)null;
        }

        public static long ToLong(this string s, long defVal)
        {
            if (string.IsNullOrEmpty(s)) return defVal;
            decimal i = 0m;
            return (long)(decimal.TryParse(s, out i) ? i : defVal);
        }

        public static long ToLong(this string s)
        {
            return s.ToLong(0L);
        }

        public static long? ToNullLong(this string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            decimal i = 0m;
            return decimal.TryParse(s, out i) ? (long)i : (long?)null;
        }

        public static float ToFloat(this string s, float defVal)
        {
            if (string.IsNullOrEmpty(s)) return defVal;
            double i = 0d;
            return (float)(double.TryParse(s, out i) ? i : defVal);
        }

        public static float ToFloat(this string s)
        {
            return s.ToFloat(0f);
        }

        public static float? ToNullFloat(this string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            double i = 0d;
            return double.TryParse(s, out i) ? (float)i : (float?)null;
        }

        public static double ToDouble(this string s, double defVal)
        {
            if (string.IsNullOrEmpty(s)) return defVal;
            double i = 0d;
            return double.TryParse(s, out i) ? i : defVal;
        }

        public static double ToDouble(this string s)
        {
            return s.ToDouble(0d);
        }

        public static double? ToNullDouble(this string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            double i = 0d;
            return double.TryParse(s, out i) ? i : (double?)null;
        }

        public static decimal ToDecimal(this string s, decimal defVal)
        {
            if (string.IsNullOrEmpty(s)) return defVal;
            decimal i = 0m;
            return decimal.TryParse(s, out i) ? i : defVal;
        }

        public static decimal ToDecimal(this string s)
        {
            return s.ToDecimal(0m);
        }

        public static decimal? ToNullDecimal(this string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            decimal i = 0m;
            return decimal.TryParse(s, out i) ? i : (decimal?)null;
        }

        public static char ToChar(this string s, char defVal)
        {
            if (string.IsNullOrEmpty(s)) return defVal;
            char c = defVal;
            return char.TryParse(s, out c) ? c : defVal;
        }

        public static char ToChar(this string s)
        {
            return s.ToChar(char.MinValue);
        }

        public static char? ToNullChar(this string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            char c = char.MinValue;
            return char.TryParse(s, out c) ? c : (char?)null;
        }

        public static DateTime ToDateTime(this string s, DateTime defVal)
        {
            if (string.IsNullOrEmpty(s)) return defVal;
            if (s.IndexOfAny(new char[] { '-', ':' }) == -1)
            {
                //yyyyMMddHHmmss
                //yyyyMMddHHmm
                //yyyyMMddHH
                //yyyyMMdd
                switch (s.Length)
                {
                    case 8:
                        s = s.Insert(4, "-").Insert(7, "-");
                        break;
                    case 10:
                        s = s.Insert(4, "-").Insert(7, "-").Insert(10, " ") + ":00:00";
                        break;
                    case 12:
                        s = s.Insert(4, "-").Insert(7, "-").Insert(10, " ").Insert(13, ":") + ":00";
                        break;
                    case 14:
                        s = s.Insert(4, "-").Insert(7, "-").Insert(10, " ").Insert(13, ":").Insert(16, ":");
                        break;
                }
            }
            DateTime i = defVal;
            return DateTime.TryParse(s, out i) ? i : defVal;
        }

        public static DateTime ToDateTime(this string s)
        {
            return s.ToDateTime(DateTime.MinValue);
        }

        public static DateTime? ToNullDateTime(this string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            if (s.IndexOfAny(new char[] { '-', ':' }) == -1)
            {
                //yyyyMMddHHmmss
                //yyyyMMddHHmm
                //yyyyMMddHH
                //yyyyMMdd
                switch (s.Length)
                {
                    case 8:
                        s = s.Insert(4, "-").Insert(7, "-");
                        break;
                    case 10:
                        s = s.Insert(4, "-").Insert(7, "-").Insert(10, " ") + ":00:00";
                        break;
                    case 12:
                        s = s.Insert(4, "-").Insert(7, "-").Insert(10, " ").Insert(13, ":") + ":00";
                        break;
                    case 14:
                        s = s.Insert(4, "-").Insert(7, "-").Insert(10, " ").Insert(13, ":").Insert(16, ":");
                        break;
                }
            }
            DateTime i = DateTime.MinValue;
            return DateTime.TryParse(s, out i) ? i : (DateTime?)null;
        }

        public static Guid ToGuid(this string s, Guid defVal)
        {
            if (string.IsNullOrEmpty(s))
                return defVal;
            Guid i = defVal;
            return Guid.TryParse(s, out i) ? i : defVal;
        }

        public static Guid ToGuid(this string s)
        {
            return s.ToGuid(Guid.Empty);
        }

        public static Guid? ToNullGuid(this string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            Guid i = Guid.Empty;
            return Guid.TryParse(s, out i) ? i : (Guid?)null;
        }
        //private static readonly Regex GuidRegex38 = new Regex(@"^\{[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}\}$");
        //private static readonly Regex GuidRegex36 = new Regex(@"^[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}$");
        //private static readonly Regex GuidRegex32 = new Regex(@"^[0-9a-fA-F]{32}$");
        //public static Guid ToGuid(string s, Guid defVal)
        //{
        //    if (string.IsNullOrEmpty(s))
        //        return defVal;
        //    switch (s.Length)
        //    {
        //        case 32: return GuidRegex32.IsMatch(s) ? new Guid(s) : defVal;
        //        case 36: return GuidRegex36.IsMatch(s) ? new Guid(s) : defVal;
        //        case 38: return GuidRegex38.IsMatch(s) ? new Guid(s) : defVal;
        //        default: return defVal;
        //    }
        //}
        //public static Guid? GuidParse(string s)
        //{
        //    if (string.IsNullOrEmpty(s))
        //        return null;
        //    switch (s.Length)
        //    {
        //        case 32: return GuidRegex32.IsMatch(s) ? new Guid(s) : (Guid?)null;
        //        case 36: return GuidRegex36.IsMatch(s) ? new Guid(s) : (Guid?)null;
        //        case 38: return GuidRegex38.IsMatch(s) ? new Guid(s) : (Guid?)null;
        //        default: return null;
        //    }
        //}

        public static IPAddress ToIP(this string s, IPAddress defVal)
        {
            if (string.IsNullOrEmpty(s)) return defVal;
            IPAddress i = defVal;
            return IPAddress.TryParse(s, out i) ? i : defVal;
        }

        public static IPAddress ToIP(this string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            IPAddress i = null;
            return IPAddress.TryParse(s, out i) ? i : null;
        }
        #endregion

        #region 转换object到目标类型
        public static string ToString(this object obj, string defVal)
        {
            return obj == null ? defVal : obj.ToString();
        }

        public static byte ToByte(this object obj, byte defVal)
        {
            return obj == null ? defVal : ToByte(obj.ToString(), defVal);
        }

        public static byte ToByte(this object obj)
        {
            return obj.ToByte(0);
        }

        public static byte? ToNullByte(this object obj)
        {
            return obj == null ? null : ToNullByte(obj.ToString());
        }

        public static short ToShort(this object obj, short defVal)
        {
            return obj == null ? defVal : ToShort(obj.ToString(), defVal);
        }

        public static short ToShort(this object obj)
        {
            return obj.ToShort(0);
        }

        public static short? ToNullShort(this object obj)
        {
            return obj == null ? null : ToNullShort(obj.ToString());
        }

        public static bool ToBool(this object obj, bool defVal)
        {
            return obj == null ? defVal : ToBool(obj.ToString(), defVal);
        }

        public static bool ToBool(this object obj)
        {
            return obj.ToBool(false);
        }

        public static bool? ToNullBool(this object obj)
        {
            return obj == null ? null : ToNullBool(obj.ToString());
        }

        public static int ToInt(this object obj, int defVal)
        {
            return obj == null ? defVal : ToInt(obj.ToString(), defVal);
        }

        public static int ToInt(this object obj)
        {
            return obj.ToInt(0);
        }

        public static int? ToNullInt(this object obj)
        {
            return obj == null ? null : ToNullInt(obj.ToString());
        }

        public static long ToLong(this object obj, long defVal)
        {
            return obj == null ? defVal : ToLong(obj.ToString(), defVal);
        }

        public static long ToLong(this object obj)
        {
            return obj.ToLong(0L);
        }

        public static long? ToNullLong(this object obj)
        {
            return obj == null ? null : ToNullLong(obj.ToString());
        }

        public static float ToFloat(this object obj, float defVal)
        {
            return obj == null ? defVal : ToFloat(obj.ToString(), defVal);
        }

        public static float ToFloat(this object obj)
        {
            return obj.ToFloat(0f);
        }

        public static float? ToNullFloat(this object obj)
        {
            return obj == null ? null : ToNullFloat(obj.ToString());
        }

        public static double ToDouble(this object obj, double defVal)
        {
            return obj == null ? defVal : ToDouble(obj.ToString(), defVal);
        }

        public static double ToDouble(this object obj)
        {
            return obj.ToDouble(0d);
        }

        public static double? ToNullDouble(this object obj)
        {
            return obj == null ? null : ToNullDouble(obj.ToString());
        }

        public static decimal ToDecimal(this object obj, decimal defVal)
        {
            return obj == null ? defVal : ToDecimal(obj.ToString(), defVal);
        }

        public static decimal ToDecimal(this object obj)
        {
            return obj.ToDecimal(0m);
        }

        public static decimal? ToNullDecimal(this object obj)
        {
            return obj == null ? null : ToNullDecimal(obj.ToString());
        }

        public static char ToChar(this object obj, char defVal)
        {
            return obj == null ? defVal : ToChar(obj.ToString(), defVal);
        }

        public static char ToChar(this object obj)
        {
            return obj.ToChar(char.MinValue);
        }

        public static char? ToNullChar(this object obj)
        {
            return obj == null ? null : ToNullChar(obj.ToString());
        }

        public static DateTime ToDateTime(this object obj, DateTime defVal)
        {
            if (obj == null)
                return defVal;

            if (obj.GetType() == TYPE_String)
                return ToDateTime((string)obj, defVal);

            if (obj.GetType() == TYPE_DateTime)
                return (DateTime)obj;

            return defVal;
        }

        public static DateTime ToDateTime(this object obj)
        {
            return obj.ToDateTime(DateTime.MinValue);
        }

        public static DateTime? ToNullDateTime(this object obj)
        {
            if (obj == null)
                return null;

            if (obj.GetType() == TYPE_String)
                return ToNullDateTime((string)obj);

            if (obj.GetType() == TYPE_DateTime)
                return (DateTime)obj;

            return null;
        }

        public static Guid ToGuid(this object obj, Guid defVal)
        {
            return obj == null ? defVal : ToGuid(obj.ToString(), defVal);
        }

        public static Guid ToGuid(this object obj)
        {
            return obj.ToGuid(Guid.Empty);
        }

        public static Guid? ToNullGuid(this object obj)
        {
            return obj == null ? null : ToNullGuid(obj.ToString());
        }

        public static IPAddress ToIP(this object obj, IPAddress defVal)
        {
            return obj == null ? defVal : ToIP(obj.ToString(), defVal);
        }

        public static IPAddress ToIP(this object obj)
        {
            return obj == null ? null : ToIP(obj.ToString());
        }
        #endregion
        #region 特殊转换
        /// <summary>
        /// 标准化价格显示
        /// </summary>
        /// <param name="v">原始价格数字</param>
        /// <param name="decimals">小数位数</param>
        /// <returns></returns>
        public static string ToMoneyString(this decimal v, int decimals)
        {
            int i = (int)(Math.Pow(10, decimals));
            return (Math.Round(v * i, 0) / i).ToString();
        }
        /// <summary>
        /// 标准化价格显示
        /// </summary>
        /// <param name="v">原始价格数字</param>
        /// <returns></returns>
        public static string ToMoneyString(this decimal v)
        {
            return v.ToMoneyString(2);
        }
        /// <summary>
        /// 标准化价格显示
        /// </summary>
        /// <param name="v">原始价格数字</param>
        /// <param name="decimals">小数位数</param>
        /// <returns></returns>
        public static string ToMoneyString(this double v, int decimals)
        {
            int i = (int)(Math.Pow(10, decimals));
            return (Math.Round(v * i, 0) / i).ToString();
        }
        /// <summary>
        /// 标准化价格显示
        /// </summary>
        /// <param name="v">原始价格数字</param>
        /// <returns></returns>
        public static string ToMoneyString(this double v)
        {
            return v.ToMoneyString(2);
        }

        public static T ToEnum<T>(this object obj, T defVal) where T : struct
        {
            return obj == null ? defVal : ToEnum(obj.ToString(), defVal);
        }

        public static T ToEnum<T>(this string s, T defVal) where T : struct
        {
            if (string.IsNullOrEmpty(s)) return defVal;
            T i = defVal;
            return Enum.TryParse<T>(s, out i) ? (T)i : defVal;
        }
        #endregion

        public readonly static Type TYPE_String = typeof(string);
        public readonly static Type TYPE_DateTime = typeof(DateTime);
        public readonly static Type TYPE_DateTimeNull = typeof(DateTime?);
        public readonly static Type TYPE_GuidNull = typeof(Guid?);
        public readonly static Type TYPE_IntNull = typeof(int?);
        public readonly static Type TYPE_ShortNull = typeof(short?);
        public readonly static Type TYPE_BoolNull = typeof(bool?);
        public readonly static Type TYPE_LongNull = typeof(long?);
        public readonly static Type TYPE_ByteNull = typeof(byte?);
        public readonly static Type TYPE_FloatNull = typeof(float?);
        public readonly static Type TYPE_DoubleNull = typeof(double?);
        public readonly static Type TYPE_DecimalNull = typeof(decimal?);
        public readonly static Type TYPE_CharNull = typeof(char?);
        public static object ChangeType(this object obj, Type targetType)
        {
            if (obj == null || targetType == null)
                return obj;
            else if (targetType == TYPE_DateTime)
                return ToDateTime(obj, DateTime.MinValue);
            else if (targetType == TYPE_DateTimeNull)
                return ToNullDateTime(obj);
            else if (targetType == TYPE_GuidNull)
                return ToNullGuid(obj);
            else if (targetType == TYPE_IntNull)
                return ToNullInt(obj);
            else if (targetType == TYPE_ShortNull)
                return ToNullShort(obj);
            else if (targetType == TYPE_BoolNull)
                return ToNullBool(obj);
            else if (targetType == TYPE_LongNull)
                return ToNullLong(obj);
            else if (targetType == TYPE_ByteNull)
                return ToNullByte(obj);
            else if (targetType == TYPE_FloatNull)
                return ToNullFloat(obj);
            else if (targetType == TYPE_DoubleNull)
                return ToNullDouble(obj);
            else if (targetType == TYPE_DecimalNull)
                return ToNullDecimal(obj);
            else if (targetType == TYPE_CharNull)
                return ToNullChar(obj);
            else
                return Convert.ChangeType(obj, targetType);
        }

        /// <summary>
        /// 二进制方式序列化对象
        /// </summary>
        public static byte[] ObjectSerialize<T>(this T obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 二进制方式反序列化对象
        /// </summary>
        /// <returns></returns>
        public static T ObjectDeserialize<T>(this byte[] data) where T : class
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                ms.Position = 0;
                BinaryFormatter formatter = new BinaryFormatter();
                T obj = formatter.Deserialize(ms) as T;
                return obj;
            }
        }
    }
}
