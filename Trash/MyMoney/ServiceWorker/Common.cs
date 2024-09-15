using System;
using System.Configuration;

namespace ServiceWorker
{
    public static class Global
    {
        private static string _serviceLink;

        public static string ServiceLink
        {
            get
            {
                if (String.IsNullOrEmpty(_serviceLink))
                {
                    _serviceLink = ConfigurationManager.AppSettings["ServiceUrl"];
                }
                return _serviceLink;
            }
            set { _serviceLink = value; }
        }

        private static string _serviceToken;

        public static string ServiceToken
        {
            get
            {
                if (String.IsNullOrEmpty(_serviceToken))
                {
                    _serviceToken = ConfigurationManager.AppSettings["ServiceToken"];
                }
                return _serviceToken;
            }
            set { _serviceLink = value; }
        }
    }

    public enum RequestType
    {
        PUT = 1,
        GET = 2,
        POST = 3,
        DELETE = 4,
    }
}
