using System;
using System.ComponentModel;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace TrunkCommon
{
    public static class EnumHelper
    {
        public static string GetDescription(Enum e)
        {
            return GetDescription(e, "(未知)");
        }

        public static string GetDescription(Enum e, string defaultDescription)
        {
            FieldInfo fi = e.GetType().GetField(e.ToString());
            try
            {
                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                return (attributes != null && attributes.Length > 0) ? attributes[0].Description : e.ToString();
            }
            catch
            {
                return defaultDescription;
            }
        }

        public static Dictionary<int,string> GetAllDescriptions(Type enumtype)
        {
            if (enumtype == null) throw new ArgumentNullException("Enumtype");
            if (!enumtype.IsEnum) throw new Exception("参数类型不正确");

            Dictionary<int, string> dics = new Dictionary<int, string>();
            FieldInfo[] fieldinfo = enumtype.GetFields();
            foreach (FieldInfo item in fieldinfo)
            {
                Object[] obj = item.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (obj != null && obj.Length != 0)
                {
                    DescriptionAttribute des = (DescriptionAttribute)obj[0];
                    dics.Add(int.Parse(item.GetRawConstantValue().ToString()), des.Description);
                }
            }
            return dics;
        }

        public static string GetValue(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            try
            {
                Type _enumType = obj.GetType();
                return Enum.Format(_enumType, Enum.Parse(_enumType, obj.ToString()), "d");
            }
            catch
            {
            }
            return obj.ToString();
        }

        public static string GetDescription(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            try
            {
                Type _enumType = obj.GetType();
                DescriptionAttribute dna = null;
                FieldInfo fi = _enumType.GetField(Enum.GetName(_enumType, obj));
                dna = (DescriptionAttribute)Attribute.GetCustomAttribute(
                    fi, typeof(DescriptionAttribute));
                if (dna != null && string.IsNullOrEmpty(dna.Description) == false)
                    return dna.Description;
            }
            catch
            {
            }
            return obj.ToString();
        }
    }
}
