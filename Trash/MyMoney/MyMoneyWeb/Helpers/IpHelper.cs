using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace MyMoneyWeb.Helpers
{
    public class IpHelper
    {
        public const string UnknownIP = "0.0.0.0";
        private static readonly Regex _ipAddress = new Regex(@"\b([0-9]{1,3}\.){3}[0-9]{1,3}$",
                                                         RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private static bool IsPrivateIP(string s)
        {
            return (s.StartsWith("192.168.") || s.StartsWith("10.") || s.StartsWith("127.0.0."));
        }
        public static string GetRemoteIP(NameValueCollection ServerVariables)
        {
            string ip = ServerVariables["REMOTE_ADDR"]; // could be a proxy -- beware
            string ipForwarded = ServerVariables["HTTP_X_FORWARDED_FOR"];

            // check if we were forwarded from a proxy
            if (ipForwarded.HasValue())
            {
                ipForwarded = _ipAddress.Match(ipForwarded).Value;
                if (ipForwarded.HasValue() && !IsPrivateIP(ipForwarded))
                    ip = ipForwarded;
            }

            return ip.HasValue() ? ip : UnknownIP;
        }
    }

    public static class Extensions
    {
        public static bool HasValue(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }
    }
}