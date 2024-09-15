using System;

namespace Common.Service
{
    public static class ServiceParamSystemNames
    {
        public const string Ip = "system_ip";
        public const string IpFromWeb = "system_web_ip";
        public const string Client = "client";
    }

    public class ServiceParam
    {
        public ServiceParam(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public ServiceParam(string name, int value)
        {
            Name = name;
            Value = value.ToString();
        }

        public ServiceParam(string name, int? value)
        {
            Name = name;
            Value = value.ToString();
        }

        public ServiceParam(string name, decimal? value)
        {
            Name = name;
            Value = value.ToString();
        }

        public ServiceParam(string name, Guid value)
        {
            Name = name;
            Value = value.ToString();
        }

        public ServiceParam(string name, Guid? value)
        {
            Name = name;
            Value = value.ToString();
        }

        public ServiceParam(string name, bool value)
        {
            Name = name;
            Value = value.ToString();
        }

        public ServiceParam(string name, bool? value)
        {
            Name = name;
            Value = value.ToString();
        }

        public ServiceParam(string name, DateTime value)
        {
            Name = name;
            Value = value.ToString("s");
        }

        public ServiceParam(string name, DateTime? value)
        {
            Name = name;
            Value = value == null ? "null" : ((DateTime) value).ToString("s");
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}
