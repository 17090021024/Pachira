using System;

namespace TrunkCommon
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// 闰年每月包含的天数
        /// </summary>
        public static readonly int[] DayInMonth = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        /// <summary>
        /// 根据月日计算在闰年中的第几天
        /// </summary>
        public static int DayOfLeepYear(int month, int day)
        {
            int count = 31;
            for (int i = 1; i < month - 1; i++)
            {
                count += DayInMonth[i];
            }
            return count + day;
        }

        public static long Diff(this DateTime t1, DateTime t2, DateTimePart part)
        {
            switch (part)
            {
                case DateTimePart.Year:
                    return t1.Year - t2.Year;
                case DateTimePart.Month:
                    return (t1.Year - t2.Year) * 12 + (t1.Month - t2.Month);
                case DateTimePart.Day:
                    return t1.Ticks / TimeSpan.TicksPerDay - t2.Ticks / TimeSpan.TicksPerDay;
                case DateTimePart.Hour:
                    return t1.Ticks / TimeSpan.TicksPerHour - t2.Ticks / TimeSpan.TicksPerHour;
                case DateTimePart.Minute:
                    return t1.Ticks / TimeSpan.TicksPerMinute - t2.Ticks / TimeSpan.TicksPerMinute;
                case DateTimePart.Millisecond:
                    return t1.Ticks / TimeSpan.TicksPerMillisecond - t2.Ticks / TimeSpan.TicksPerMillisecond;
                default:
                case DateTimePart.Tick:
                    return t1.Ticks - t2.Ticks;
            }
        }

        public enum DateTimePart
        {
            Year,
            Month,
            Day,
            Hour,
            Minute,
            Millisecond,
            Tick,
        }

        /// <summary>
        /// 判断日期是否是润年
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool IsLeepYear(this DateTime date)
        {
            return 0 == date.Year % 4 && (date.Year % 100 != 0) || (date.Year % 400 == 0);
        }

        /// <summary>
        /// 根据在闰年中的第几天计算月日
        /// </summary>
        public static int[] LeepYearFromDay(int dayCount)
        {
            int[] monthAndDay = { 0, 0 };
            for (int i = 0; i < 12; i++)
            {
                dayCount -= DayInMonth[i];
                if (dayCount <= 0)
                {
                    monthAndDay[0] = i + 1;
                    monthAndDay[1] = DayInMonth[i] + dayCount;
                }
            }
            return monthAndDay;
        }

        /// <summary>
        /// 将yyyyMMdd[HHmmss][fff]格式日期转换为DataTime类型
        /// </summary>
        /// <param name="dateString">yyyyMMddHHmmssfff、yyyyMMddHHmmss、yyyyMMdd或yyMMdd格式日期字符串</param>
        /// <returns></returns>
        public static DateTime FromyyyyMMddHHmmss(string dateString)
        {
            if (dateString == null)
                throw new ArgumentException("dateString参数不能是null。");
            int length = dateString.Length;
            if (dateString.Length != 17
                && dateString.Length != 14
                && dateString.Length != 8
                && dateString.Length != 6)
                throw new ArgumentException("dateString参数不是yyyyMMddHHmmssfff、yyyyMMddHHmmss、yyyyMMdd或yyMMdd格式日期字符串。");
            char[] cs = new char[length == 17 ? 23 : (length == 14 ? 19 : 10)];
            if (dateString.Length == 6)
            {
                if (int.Parse(dateString.Substring(0, 2)) >= 50)
                {
                    dateString = (DateTime.Now.Year / 100 - 1) + dateString;
                }
                else
                {
                    dateString = DateTime.Now.Year / 100 + dateString;
                }
            }
            cs[0] = dateString[0]; cs[1] = dateString[1]; cs[2] = dateString[2]; cs[3] = dateString[3];
            cs[4] = '-';
            cs[5] = dateString[4]; cs[6] = dateString[5];
            cs[7] = '-';
            cs[8] = dateString[6]; cs[9] = dateString[7];
            if (length >= 14)
            {
                cs[10] = ' ';
                cs[11] = dateString[8]; cs[12] = dateString[9];
                cs[13] = ':';
                cs[14] = dateString[10]; cs[15] = dateString[11];
                cs[16] = ':';
                cs[17] = dateString[12]; cs[18] = dateString[13];
                if (length == 17)
                {
                    cs[19] = '.';
                    cs[20] = dateString[14]; cs[21] = dateString[15]; cs[22] = dateString[16];
                }
            }
            return DateTime.Parse(new string(cs));
        }

        /// <summary>
        /// 返回标准通用格式时间yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ToGeneralTimeString(this DateTime t)
        {
            return t.ToString("yyyy-MM-dd HH:mm:ss");
        }
        /// <summary>
        /// 返回标准通用格式日期yyyy-MM-dd
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ToGeneralDateString(this DateTime t)
        {
            return t.ToString("yyyy-MM-dd");
        }
        /// <summary>
        /// 返回javascript可用格式时间yyyy/MM/dd HH:mm:ss
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ToJSTimeString(this DateTime t)
        {
            return t.ToString("yyyy-MM-dd HH:mm:ss").Replace('-', '/');
        }
        /// <summary>
        /// 返回javascript可用格式日期yyyy/MM/dd
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ToJSDateString(this DateTime t)
        {
            return t.ToString("yyyy-MM-dd").Replace('-', '/');
        }
        /// <summary>
        /// yyyyMMddHHmmss
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ToZipTimeString(this DateTime t)
        {
            return t.ToString("yyyyMMddHHmmss");
        }
        /// <summary>
        /// yyyyMMdd
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ToZipDateString(this DateTime t)
        {
            return t.ToString("yyyyMMdd");
        }

        /// <summary>
        /// 将时间转换为Unix时间戳(Unix时间戳是1970年到现在的毫秒数)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static long ToUnixTimestamp(this DateTime t)
        {
            return t.ToUniversalTime().Ticks / 10000 - MILLISECONDFROM0TO1970;
        }

        /// <summary>
        /// 将Unix时间戳转换为时间(Unix时间戳是1970年到现在的毫秒数)
        /// </summary>
        /// <param name="unixTimestamp"></param>
        /// <returns></returns>
        public static DateTime ParseUnixTimestamp(long unixTimestamp)
        {
            return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime((MILLISECONDFROM0TO1970 + unixTimestamp) * 10000));
        }

        /// <summary>
        /// 1970 年 1 月 1 日距离0年已过的毫秒数
        /// </summary>
        public const long MILLISECONDFROM0TO1970 = 62135596800000L;
        /// <summary>
        /// 将日期类型转换为1970 年 1 月 1 日至t的毫秒数（与javascript中的getTime()一致）
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static long DateTimeToMillisecondFrom1970(DateTime t)
        {
            return t.Ticks / 10000 - MILLISECONDFROM0TO1970;
        }
        /// <summary>
        /// 将已过1970 年 1 月 1 日的毫秒数转换为日期
        /// </summary>
        /// <param name="millisecondFrom1970"></param>
        /// <returns></returns>
        public static DateTime MillisecondFrom1970ToDateTime(long millisecondFrom1970)
        {
            return new DateTime((MILLISECONDFROM0TO1970 + millisecondFrom1970) * 10000);
        }

        /// <summary>
        /// 返回两个日期中的较大日期
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static DateTime Max(DateTime t1, DateTime t2)
        {
            return t1 > t2 ? t1 : t2;
        }
        /// <summary>
        /// 返回两个日其中的较小日期
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static DateTime Min(DateTime t1, DateTime t2)
        {
            return t1 < t2 ? t1 : t2;
        }


    }//end class
}