using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using Common.Core;

namespace DataWorker
{
    public class Sender
    {
        private const string SMTP_SERVER = "http://schemas.microsoft.com/cdo/configuration/smtpserver";
        private const string SMTP_SERVER_PORT = "http://schemas.microsoft.com/cdo/configuration/smtpserverport";
        private const string SEND_USING = "http://schemas.microsoft.com/cdo/configuration/sendusing";
        private const string SMTP_USE_SSL = "http://schemas.microsoft.com/cdo/configuration/smtpusessl";
        private const string SMTP_AUTHENTICATE = "http://schemas.microsoft.com/cdo/configuration/smtpauthenticate";
        private const string SEND_USERNAME = "http://schemas.microsoft.com/cdo/configuration/sendusername";
        private const string SEND_PASSWORD = "http://schemas.microsoft.com/cdo/configuration/sendpassword";

        public static OperationResult SendEmail(string to, string title, string messageBody)
        {
            var result = new OperationResult();
            try
            {
                var client = ConfigurationManager.AppSettings["EmailServer"];
                var port = Convert.ToInt32(ConfigurationManager.AppSettings["EmailServerPort"]);
                var login = ConfigurationManager.AppSettings["EmailLogin"];
                var password = ConfigurationManager.AppSettings["EmailPassword"];
                //var smtp = new SmtpClient(client, port);
                //smtp.Credentials = new NetworkCredential(login, password);
                //smtp.EnableSsl = true;                


                var mail = new System.Web.Mail.MailMessage();
                mail.Fields[SMTP_SERVER] = client;
                mail.Fields[SMTP_SERVER_PORT] = port;
                mail.Fields[SEND_USING] = 2;
                mail.Fields[SMTP_USE_SSL] = true;
                mail.Fields[SMTP_AUTHENTICATE] = 1;
                mail.Fields[SEND_USERNAME] = login;
                mail.Fields[SEND_PASSWORD] = password;

                mail.From = login;
                mail.To = to;
                mail.Subject = title;
                mail.Body = messageBody;
                mail.BodyFormat = System.Web.Mail.MailFormat.Html;

                System.Web.Mail.SmtpMail.Send(mail);

                //var message = new MailMessage();
                //message.From = new MailAddress(login);
                //message.To.Add(new MailAddress(to));
                //message.Subject = title;
                //message.Body = messageBody;
                //message.IsBodyHtml = true;
                //smtp.Send(message);
                //smtp.Dispose();

                result.Type = OperationResultTypes.Success;
                return result;
            }
            catch (Exception ex)
            {
                result.Type = OperationResultTypes.Error;
                result.Message = ex.Message;
                DbLogWorker.WriteExecuteMessage(2, "Не удалось отправить письмо на почту " + to, LogTypes.Error, null, ex.Message);
                return result;
            }
        }
    }
}