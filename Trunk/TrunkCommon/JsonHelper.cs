using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TrunkCommon
{
    public static class JsonHelper
    {
        /// <summary>
        /// 将对象序列化成Json字符串
        /// </summary>
        /// <returns></returns>
        public static string ToJson(this object obj)
        {
            if (obj == null)
                return string.Empty;

            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 将Json反序列化成对象(对象类型必须是引用类型)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jSon"></param>
        /// <returns></returns>
        public static T ParseJson<T>(this string jSon) where T : class
        {
            return ParseJson<T>(jSon, null);
        }

        /// <summary>
        /// 将Json反序列化成对象(对象类型可以是应用类型，也可以是非引用类型)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jSon"></param>
        /// <param name="defaultVal">反序列化失败时返回的默认值</param>
        /// <returns></returns>
        public static T ParseJson<T>(this string jSon, T defaultVal)
        {
            if (string.IsNullOrEmpty(jSon))
                return defaultVal;
            T obj = defaultVal;
            try
            {
                obj = JsonConvert.DeserializeObject<T>(jSon, JsonDateTimeConverter.Entity);
            }
            catch(Exception e)
            {
                return defaultVal;
            }
            return obj;
        }
    }

    public class JsonDateTimeConverter : IsoDateTimeConverter
    {
        public static JsonDateTimeConverter Entity = new JsonDateTimeConverter();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return DateTime.MinValue;

            if (reader.ValueType == typeof(long))
                return DateTimeHelper.ParseUnixTimestamp(reader.Value.ToLong());

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
