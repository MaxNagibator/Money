using System;
using System.Configuration;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Common.Core;
using Common.Enums;
using Common.Exceptions;
using Extentions;

namespace DataWorker
{
    public class DbAccountWorker
    {
        public static User Registration(string login, string password, string email)
        {
            using (var context = new Data.DataContext())
            {
                var isExists = context.Users.Any(x => x.Login == login);
                if (isExists)
                {
                    throw new MessageException("Логин занят");
                }
                if (!String.IsNullOrEmpty(email))
                {
                    isExists = context.Users.Any(x => x.EmailConfirm && x.Email == email);
                    if (isExists)
                    {
                        throw new MessageException("Почта занята");
                    }
                }
                using (MD5 md5Hash = MD5.Create())
                {
                    string emailCode = "";
                    if (!String.IsNullOrEmpty(email))
                    {
                        var registerSendEmailSiteUrl = ConfigurationManager.AppSettings["RegisterSendEmailSiteUrl"];
                        emailCode = GetMd5Hash(md5Hash, email + DateTime.Now.AddDays(-217).ToUnixDate());
                        var link = String.Format(registerSendEmailSiteUrl + "/Account/ConfirmEmail?login={0}&code={1}", login, emailCode);
                        var emailMessage = "Для подтверждения почты перейдите по " + String.Format("<a href='{0}'>ссылке</a>", link) + " или скопируйте: " + link + " в адресную строку браузера. <br>";
                        var sendResult = Sender.SendEmail(email, "Подтверждение регистрации", emailMessage);
                        if (sendResult.Type != OperationResultTypes.Success)
                        {
                            throw new MessageException("Не удалось отправить письмо на почту, приносим извинения");
                        }
                    }

                    string hash = GetMd5Hash(md5Hash, password + login);
                    string token = GetMd5Hash(md5Hash, password + DateTime.Now.AddDays(-217).ToUnixDate());
                    var dbUser = new Data.User();
                    dbUser.Login = login;
                    dbUser.Password = hash;
                    dbUser.Token = token;
                    dbUser.CreateDate = DateTime.Now;
                    if (!String.IsNullOrEmpty(email))
                    {
                        dbUser.Email = email;
                        dbUser.EmailSendCode = emailCode;
                        dbUser.EmailSendCodeDate = dbUser.CreateDate;
                    }
                    context.Users.Add(dbUser);
                    context.SaveChanges();
                    DbFinanceWorker.CreateCategory(dbUser.Id, "Базовая", "", null, null, PaymentTypes.Costs, 1);
                    DbFinanceWorker.CreateCategory(dbUser.Id, "Базовая", "", null, null, PaymentTypes.Income, 1);
                    var user = new User();
                    user.Token = dbUser.Token;
                    user.Id = dbUser.Id;
                    return user;
                }
            }
        }

        public static User Login(string login, string password)
        {
            using (var context = new Data.DataContext())
            {
                using (MD5 md5Hash = MD5.Create())
                {
                    string hash = GetMd5Hash(md5Hash, password + login);
                    var dbUser = context.Users.SingleOrDefault(x => x.Login == login && x.Password == hash);
                    if (dbUser == null)
                    {
                        throw new MessageException("Неверный логин или пароль");
                    }

                    var user = new User();
                    user.Token = dbUser.Token;
                    user.Id = dbUser.Id;
                    return user;
                }
            }
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static int GetUserIdByToken(string token)
        {
            using (var context = new Data.DataContext())
            {
                var userId = context.Users.Single(x => x.Token == token).Id;
                return userId;
            }
        }

        public static void ConfirmEmail(string login, string code)
        {
            using (var context = new Data.DataContext())
            {
                var dbUser = context.Users.SingleOrDefault(x => x.Login == login);
                if (dbUser == null)
                {
                    throw new MessageException("ошибка подтверждения");
                }
                if (dbUser.EmailSendCode != code)
                {
                    throw new MessageException("ошибка подтверждения");
                }
                if(context.Users.Any(x=>x.EmailConfirm && x.Email == dbUser.Email && x.Id != dbUser.Id))
                {
                    throw new MessageException("данная почта уже занята");
                }
                dbUser.EmailConfirm = true;
                dbUser.EmailSendCode = null;
                dbUser.EmailSendCodeDate = null;
                context.SaveChanges();
            }
        }

        public static void СhangePassword(int userId, string oldPassword, string newPassword)
        {
            using (var context = new Data.DataContext())
            {
                var dbUser = context.Users.Single(x => x.Id == userId);
                if (dbUser.Login == "demo")
                {
                    throw new MessageException("Демо пользователю пароль менять запрещено ^_^");
                }
                using (MD5 md5Hash = MD5.Create())
                {
                    string hash = GetMd5Hash(md5Hash, oldPassword + dbUser.Login);
                    if (hash != dbUser.Password)
                    {
                        throw new MessageException("Неверный старый пароль");
                    }
                    string newHash = GetMd5Hash(md5Hash, newPassword + dbUser.Login);
                    dbUser.Password = newHash;
                    context.SaveChanges();
                }
            }
        }

        public static string GetConnectionString()
        {
            using (var context = new Data.DataContext())
            {
                var str = context.Database.Connection.ConnectionString;
                return str;
            }
        }
    }
}
