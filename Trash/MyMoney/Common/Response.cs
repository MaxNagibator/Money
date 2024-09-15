using System.ComponentModel;
using System.Runtime.Serialization;
using Extentions;

namespace ServiceResponse
{
    [DataContract]
    public class Response<T>
    {
        [DataMember(Order = 1)]
        public ResponseStatus Status { get; set; }

        [DataMember(Order = 2)]
        public string ApiDescription { get; set; }

        [DataMember(Order = 3)]
        public T Body { get; set; }

        [DataMember(Order = 4)]
        public string Message { get; set; }

        [DataMember(Order = 5)]
        public int? MessageCode { get; set; }

        public static Response<T> GeneratePackage(string apiDescripton, T data, ResponseStatus status, string message = null, int? messageCode = null)
        {
            var package = new Response<T>
            {
                ApiDescription = apiDescripton,
                Body = data,
                Status = status,
                Message = message,
                MessageCode = messageCode
            };

            return package;
        }

        public ResponseType Type
        {
            get
            {
                if (Status == ResponseStatus.OK)
                {
                    return ResponseType.Success;
                }
                return ResponseType.Warning;
            }
        }

        public string ResponseMessage
        {
            get
            {
                if (Status == ResponseStatus.OK)
                {
                    return Status.DescriptionAttr();
                }
                if (Status == ResponseStatus.IMPOSIBLE)
                {
                    return Message;
                }
                return Status.DescriptionAttr();
            }
        }
    }

    public enum ResponseStatus
    {
        [Description("Успех")]
        OK = 200,

        [Description("Недостаточно прав доступа")]
        ACCESS_DENIED = 201,

        [Description("Ваш IP адрес заблокирован")]
        ACCESS_DENIED_BY_IP = 202,

        [Description("Недопустимая операция")]
        IMPOSIBLE = 301,

        [Description("Ошибка на сервере")]
        INTERNAL_ERROR = 500,
    }

    public enum ResponseType
    {
        Success = 1,
        Warning = 2,
        Error = 3,
    }

    public static class ResponseHelper
    {
    }
}
