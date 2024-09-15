using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using ServiceResponse;

namespace ServiceWorker
{
    public class Responser : IDisposable
    {
        private Dictionary<string, object> _parameters;

        public Responser()
        {
            _parameters = new Dictionary<string, object>();
        }

        public void SetParameter(string paramName, object paramValue)
        {
            if (_parameters.ContainsKey(paramName))
            {
                _parameters.Remove(paramName);
            }
            _parameters.Add(paramName, paramValue);
        }

        public void Dispose()
        {
            _parameters.Clear();
        }

        public Response<T> Execute<T>(RequestType typeRequest, string responseString)
        {
            try
            {
                Response<T> package;
                switch (typeRequest)
                {
                    case RequestType.GET:
                        package = ExecuteGetMethod<T>(responseString);
                        break;
                    case RequestType.POST:
                        package = ExecutePostMethod<T>(responseString);
                        break;
                    default:
                        throw new Exception("Ошибка!");
                }
                return package;
            }
            catch (Exception ex)
            {
                var package = new Response<T>();
                package.Status = ResponseStatus.INTERNAL_ERROR;
                return package;
            }
        }

        private Response<T> ExecuteGetMethod<T>(string responseText)
        {
            string paramString = "?";
            if (_parameters.Count > 0)
            {
                foreach (var param in _parameters)
                {
                    paramString += param.Key + "=" + param.Value + "&";
                }
            }
            var webResponse = GetWebResponse(responseText + paramString, "GET", "application/json");
            return CreateReturnPackage<T>(webResponse);
        }

        private Response<T> ExecutePostMethod<T>(string responseString)
        {
            string sd = JsonConvert.SerializeObject(_parameters.Single().Value);
            var webResponse = GetWebResponseWithSerializedData(responseString, "POST", "application/json", sd);
            return CreateReturnPackage<T>(webResponse);
        }

        private static Response<T> CreateReturnPackage<T>(HttpWebResponse webResponse)
        {
            using (var stream = webResponse.GetResponseStream())
            {
                var reader = new StreamReader(stream);
                var serializedResponse = reader.ReadToEnd();
                var package = JsonConvert.DeserializeObject<Response<T>>(serializedResponse);
                return package;
            }
        }

        public static HttpWebResponse GetWebResponse(string uri, string requestMethod, string contentType)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);
            webRequest.Method = requestMethod;
            webRequest.ContentType = contentType;
            webRequest.AllowWriteStreamBuffering = false;
            return (HttpWebResponse)webRequest.GetResponse();
        }

        public static HttpWebResponse GetWebResponseWithSerializedData(string uri, string requestMethod,
            string contentType, string serializedData)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);
            webRequest.Method = requestMethod;
            webRequest.AllowWriteStreamBuffering = false;
            byte[] byteArray = Encoding.UTF8.GetBytes(serializedData);
            webRequest.ContentLength = byteArray.Length;
            webRequest.ContentType = contentType;

            Stream dataStream = webRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            return (HttpWebResponse)webRequest.GetResponse();
        }
    }
}
