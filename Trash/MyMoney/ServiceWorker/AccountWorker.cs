using Common.Exceptions;
using Common.Service;
using DataWorker;
using Extentions;
using ServiceRequest.Account;
using ServiceRespone.Account;
using ServiceResponse;
using ServiceWorker.Executor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Web;

namespace ServiceWorker
{
    public class AccountWorker
    {
        public static Response<RegistrationResponse> Registration(RegistrationRequest request, HttpRequestBase httpRequestBase)
        {
            Func<RegistrationResponse> x =
                () =>
                {
                    var result = new RegistrationResponse();
                    var login = request.Login.TrimValue().ToLowerOrNull();
                    var email = request.Email.TrimValue().ToLowerOrNull();
                    var password = request.Password;
                    if (String.IsNullOrEmpty(login))
                    {
                        throw new Exception("Логин обязательное поле");
                    }
                    if (login.Contains(" "))
                    {
                        throw new Exception("Логин содержит недопустимые символы");
                    }
                    if (String.IsNullOrEmpty(password))
                    {
                        throw new Exception("Пароль обязательное поле");
                    }
                    var user = DbAccountWorker.Registration(login, password, email);
                    result.Token = user.Token;
                    result.UserId = user.Id;
                    return result;
                };
            var parameters = new List<ServiceParam>();
            parameters.Add(new ServiceParam("Login", request.Login));
            parameters.Add(new ServiceParam("Email", request.Email));
            return ServiceExecutor.Execute(x, httpRequestBase, null, request.Client, ServiceNames.Account.Registration, parameters);
        }

        public static Response<LoginResponse> Login(LoginRequest request, HttpRequestBase httpRequestBase)
        {
            Func<LoginResponse> x =
                () =>
                {
                    var result = new LoginResponse();
                    var login = request.Login.TrimValue().ToLowerOrNull();
                    var password = request.Password;
                    var user = DbAccountWorker.Login(login, password);
                    result.Token = user.Token;
                    result.UserId = user.Id;
                    return result;
                };
            var parameters = new List<ServiceParam>();
            parameters.Add(new ServiceParam("Login", request.Login));
            return ServiceExecutor.Execute(x, httpRequestBase, "", request.Client, ServiceNames.Account.Login, parameters);
        }

        public static Response<ConfirmEmailResponse> ConfirmEmail(ConfirmEmailRequest request, HttpRequestBase httpRequestBase)
        {
            Func<ConfirmEmailResponse> x =
                () =>
                {
                    var result = new ConfirmEmailResponse();
                    var login = request.Login;
                    var code = request.Code;
                    DbAccountWorker.ConfirmEmail(login, code);
                    return result;
                };
            var parameters = new List<ServiceParam>();
            parameters.Add(new ServiceParam("Login", request.Login));
            return ServiceExecutor.Execute(x, httpRequestBase, "", request.Client, ServiceNames.Account.ConfirmEmail, parameters);
        }

        public static Response<СhangePasswordResponse> СhangePassword(СhangePasswordRequest request, HttpRequestBase httpRequestBase)
        {
            Func<СhangePasswordResponse> x =
                () =>
                {
                    var result = new СhangePasswordResponse();
                    var userId = request.GetUserIdByToken();
                    DbAccountWorker.СhangePassword(userId, request.OldPassword, request.NewPassword);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.Account.ConfirmEmail, null);
        }

        public static Response<GetCommentsResponse> GetComments(GetCommentsRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetCommentsResponse> x =
                 () =>
                 {
                     var result = new GetCommentsResponse();
                     var comments = DbCommentWorker.GetComments();
                     result.Comments = new List<GetCommentsResponse.CommentValue>();
                     foreach (var comment in comments)
                     {
                         var c = new GetCommentsResponse.CommentValue();
                         c.Author = comment.Author;
                         c.CreateDate = comment.CreateDate;
                         c.Text = comment.Text;
                         c.Title = comment.Title;
                         result.Comments.Add(c);
                     }
                     return result;
                 };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.Account.GetComments, null);
        }

        public static Response<CreateCommentResponse> CreateComment(CreateCommentRequest request, HttpRequestBase httpRequestBase)
        {
            Func<CreateCommentResponse> x =
                 () =>
                 {
                     if (String.IsNullOrEmpty(request.Title) || String.IsNullOrEmpty(request.Text) || String.IsNullOrEmpty(request.Author))
                     {
                         throw new MessageException("Заполните обязательный поля");
                     }
                     var result = new CreateCommentResponse();
                     DbCommentWorker.CreateComment(request.Title, request.Text, request.Author);
                     return result;
                 };
            var parameters = new List<ServiceParam>();
            parameters.Add(new ServiceParam("Author", request.Author));
            parameters.Add(new ServiceParam("Text", request.Text));
            parameters.Add(new ServiceParam("Title", request.Title));
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.Account.CreateComment, parameters);
        }

        public CreateCommentResponse CreateComment2(CreateCommentRequest request, HttpRequestBase httpRequestBase)
        {
            if (String.IsNullOrEmpty(request.Title) || String.IsNullOrEmpty(request.Text) || String.IsNullOrEmpty(request.Author))
            {
                throw new MessageException("Заполните обязательный поля");
            }
            var result = new CreateCommentResponse();
            DbCommentWorker.CreateComment(request.Title, request.Text, request.Author);
            return result;
        }
    }

    public class H
    {
        public static void H2()
        {
            var a = LoggingProxy<AccountWorker>.Init().CreateComment2(new CreateCommentRequest(), null);
        }
    }

    public class W
    {
        public static Action A2(Action a)
        {
            return a;
        }
    }

    public class LoggingProxy<T> : RealProxy where T : new()
    {
        private readonly T _instance;

        private LoggingProxy() 
            : base(typeof(T))
        {
            _instance = new T();
        }

        private LoggingProxy(T instance)
            : base(typeof(T))
        {
            _instance = instance;
        }

        public static T Init()
        {
            return (T)new LoggingProxy<T>().GetTransparentProxy();
        }

        public static T Init(T instance)
        {
            return (T)new LoggingProxy<T>(instance).GetTransparentProxy();
        }

        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = (IMethodCallMessage)msg;
            var method = (MethodInfo)methodCall.MethodBase;

            try
            {
                Console.WriteLine("Before invoke: " + method.Name);
                var result = method.Invoke(_instance, methodCall.InArgs);
                Console.WriteLine("After invoke: " + method.Name);
                return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
                if (e is TargetInvocationException && e.InnerException != null)
                {
                    return new ReturnMessage(e.InnerException, msg as IMethodCallMessage);
                }

                return new ReturnMessage(e, msg as IMethodCallMessage);
            }
        }
    }
}
