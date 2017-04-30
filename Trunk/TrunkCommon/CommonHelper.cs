using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Threading;

namespace TrunkCommon
{
    /// <summary>
    /// 公共方法
    /// </summary>
    public static class CommonHelper
    {
        /// <summary>
        /// 获取词典中的值，如果键不存在，则返回默认值
        /// </summary>
        public static V Get<K, V>(this Dictionary<K, V> dic, K key, V defValue)
        {
            if (dic == null)
                return defValue;
            V v;
            return dic.TryGetValue(key, out v) ? v : defValue;
        }
        /// <summary>
        /// 获取词典中的值，如果键不存在，则返回默认值
        /// </summary>
        public static V Get<K, V>(this Dictionary<K, V> dic, K key)
        {
            return dic.Get(key, default(V));
        }
        /// <summary>
        /// 对象为空判断，如果null，则返回默认值
        /// </summary>
        /// <returns></returns>
        public static T IsNull<T>(this object obj, T def)
        {
            return obj == null ? def : (T)obj;
        }
        /// <summary>
        /// 尝试获取数组元素
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <param name="arr">数组</param>
        /// <param name="index">索引</param>
        /// <param name="defVal">默认值</param>
        /// <returns></returns>
        public static T Get<T>(this Array arr, int index, T defVal)
        {
            if (arr == null || arr.Length <= index)
                return defVal;
            return (T)arr.GetValue(index);
        }
        /// <summary>
        /// 将nvc转化成字典
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDictionary(this NameValueCollection nvc)
        {
            if (nvc == null)
                return null;

            Dictionary<string, string> result = new Dictionary<string, string>();
            for (int i = 0; i < nvc.Count; i++)
                result.Add(nvc.Keys[i], nvc[i]);

            return result;
        }

        public static bool IsNullOrEmpty(this ICollection collection)
        {
            return collection == null || collection.Count == 0;
        }

        /// <summary>
        /// 深拷贝对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepClone<T>(this T obj) where T : class,ISerializable
        {
            return obj.ObjectSerialize().ObjectDeserialize<T>();
        }
    }

    public delegate ResponseType CommonDelegate<ResponseType, RequestType>(RequestType request);
    public delegate ResponseType CommonDelegate<ResponseType>();
    [Serializable]
    public struct Nvp { public string Name; public string Value; public Nvp(string name, string value) { Name = name; Value = value; } }

    public class Disposer : IDisposable
    {
        ThreadStart DisposeMethod;
        public Disposer(ThreadStart disposeMethod)
        {
            this.DisposeMethod = disposeMethod;
        }
        public void Dispose()
        {
            this.DisposeMethod();
        }
    }

    [Serializable]
    public class WrapClass<T> { public T Value { get; set; } }
    [Serializable]
    public class WrapClass<T1, T2> { public T1 Value1 { get; set; } public T2 Value2 { get; set; } }
    [Serializable]
    public class WrapClass<T1, T2, T3> { public T1 Value1 { get; set; } public T2 Value2 { get; set; } public T3 Value3 { get; set; } }
    [Serializable]
    public class WrapClass<T1, T2, T3, T4> { public T1 Value1 { get; set; } public T2 Value2 { get; set; } public T3 Value3 { get; set; } public T4 Value4 { get; set; } }
    [Serializable]
    public class WrapClass<T1, T2, T3, T4, T5> { public T1 Value1 { get; set; } public T2 Value2 { get; set; } public T3 Value3 { get; set; } public T4 Value4 { get; set; } public T5 Value5 { get; set; } }
    [Serializable]
    public class WrapClass<T1, T2, T3, T4, T5, T6> { public T1 Value1 { get; set; } public T2 Value2 { get; set; } public T3 Value3 { get; set; } public T4 Value4 { get; set; } public T5 Value5 { get; set; } public T6 Value6 { get; set; } }
    [Serializable]
    public class WrapClass<T1, T2, T3, T4, T5, T6, T7> { public T1 Value1 { get; set; } public T2 Value2 { get; set; } public T3 Value3 { get; set; } public T4 Value4 { get; set; } public T5 Value5 { get; set; } public T6 Value6 { get; set; } public T7 Value7 { get; set; } }
    [Serializable]
    public class WrapClass<T1, T2, T3, T4, T5, T6, T7, T8> { public T1 Value1 { get; set; } public T2 Value2 { get; set; } public T3 Value3 { get; set; } public T4 Value4 { get; set; } public T5 Value5 { get; set; } public T6 Value6 { get; set; } public T7 Value7 { get; set; } public T8 Value8 { get; set; } }
    [Serializable]
    public class WrapClass<T1, T2, T3, T4, T5, T6, T7, T8, T9> { public T1 Value1 { get; set; } public T2 Value2 { get; set; } public T3 Value3 { get; set; } public T4 Value4 { get; set; } public T5 Value5 { get; set; } public T6 Value6 { get; set; } public T7 Value7 { get; set; } public T8 Value8 { get; set; } public T9 Value9 { get; set; } }
    [Serializable]
    public class WrapClass<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> { public T1 Value1 { get; set; } public T2 Value2 { get; set; } public T3 Value3 { get; set; } public T4 Value4 { get; set; } public T5 Value5 { get; set; } public T6 Value6 { get; set; } public T7 Value7 { get; set; } public T8 Value8 { get; set; } public T9 Value9 { get; set; } public T10 Value10 { get; set; } }
    [Serializable]
    public class WrapClass<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNext> { public T1 Value1 { get; set; } public T2 Value2 { get; set; } public T3 Value3 { get; set; } public T4 Value4 { get; set; } public T5 Value5 { get; set; } public T6 Value6 { get; set; } public T7 Value7 { get; set; } public T8 Value8 { get; set; } public T9 Value9 { get; set; } public T10 Value10 { get; set; } public TNext Next { get; set; } }

    public class IgnoreCaseDictionary<T, V> : Dictionary<string, V>
    {
        public IgnoreCaseDictionary() : base(StringComparer.OrdinalIgnoreCase) { }
    }
}