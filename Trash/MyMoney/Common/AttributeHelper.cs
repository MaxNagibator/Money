using System;
using System.ComponentModel;
using System.Reflection;

namespace Extentions
{
    public static class AttributeHelper
    {
        public static string DescriptionAttr<T>(this T source)
        {
            if (source == null)
            {
                return null;
            }
            FieldInfo fi = source.GetType().GetField(source.ToString());
            if (fi == null)
            {
                return null;
            }
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            return source.ToString();
        }

        public static string DescriptionAttr(this Type source)
        {
            if (source == null)
            {
                return null;
            }
            var attributes = (DescriptionAttribute[])source.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            return source.ToString();
        }

        public static string ServiceNameAttr<T>(this T source)
        {
            if (source == null)
            {
                return null;
            }
            FieldInfo fi = source.GetType().GetField(source.ToString());
            if (fi == null)
            {
                return String.Empty;
            }
            var attributes = (ServiceNameAttribute[])fi.GetCustomAttributes(
                typeof(ServiceNameAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Name;
            }
            return source.ToString();
        }
    }

    public class ServiceNameAttribute : Attribute
    {
        public string Name;
        public ServiceNameAttribute(string name)
        {
            Name = name;
        }
    }

}
