using System;
using System.Configuration;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace MyMoneyWeb.Structure
{
    public static class Cookie
    {
        public const string TimeZoneCookieName = "ClientTimeToUtcShiftInMinutes";
        public static readonly string AuthCookieName = ConfigurationManager.AppSettings["ProjectName"] ?? "MyMoney";

        public static double GetClientTimeToUtcShiftInMinutes(this HttpRequestBase request)
        {
            // Offset из браузера присылается как количество минут, которые необходимо прибавить к локальному
            // времени, чтобы получить время в UTC. В TimeZoneInfo.Local.BaseUtcOffset наоборот хранится TimeSpan,
            // который необходимо прибавить ко времени UTС, чтобы получить локальное время. Потому
            // локальный BaseUtcOffset умножим на -1, чтобы было в едином стиле всё.
            return request.GetCookie<int?>(TimeZoneCookieName) ?? -1 * TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes;
        }

        public static void SetAuthCookie(this HttpResponseBase response, AuthCookieSerializeModel serializeModel)
        {
            var serializer = new JavaScriptSerializer();
            var userData = serializer.Serialize(serializeModel);
            var ticket = new FormsAuthenticationTicket(1, serializeModel.UserId.ToString(), DateTime.Now, DateTime.Now.AddDays(2), true, userData);
            var encryptedTicket = FormsAuthentication.Encrypt(ticket);

            response.SetCookie(AuthCookieName, encryptedTicket);
        }

        public static void ClearAuthCookie(this HttpResponseBase response)
        {
            SetAuthCookie(response, new AuthCookieSerializeModel());
        }

        public static string GetAuthToken(this HttpRequestBase request)
        {
            var ticket = GetAuthCookie(request);
            if (!String.IsNullOrEmpty(ticket.UserData))
            {
                var serializer = new JavaScriptSerializer();
                var deSerializeModel = serializer.Deserialize<AuthCookieSerializeModel>(ticket.UserData);
                return deSerializeModel.Token;
            }
            return "";
        }

        private static FormsAuthenticationTicket GetAuthCookie(this HttpRequestBase request)
        {
            var cookieValue = request.GetCookie<string>(AuthCookieName);;
            if (cookieValue != null && !String.IsNullOrEmpty(cookieValue))
            {
                return FormsAuthentication.Decrypt(cookieValue);
            }

            return new FormsAuthenticationTicket(AuthCookieName, false, 0);
        }

        public static void SetCookie<T>(this HttpResponseBase response, string cookieName, T cookieValue)
        {
            var cookieToSet = new HttpCookie(cookieName);
            if (cookieValue is string cookieStringValue)
            {
                cookieToSet.Value = cookieStringValue;
            }
            else
            {
                var serializer = new JavaScriptSerializer();
                cookieToSet.Value = serializer.Serialize(cookieValue);
            }

            response.Cookies.Set(cookieToSet);
        }

        public static T GetCookie<T>(this HttpRequestBase request, string cookieName)
        {
            var cookie = request.Cookies.Get(cookieName);
            if (cookie == null)
            {
                return default;
            }

            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(cookie.Value, typeof(T));
            }

            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(cookie.Value);
        }
    }

    public class AuthCookieSerializeModel
    {
        public string Token { get; set; }
        public int UserId { get; set; }
    }
}