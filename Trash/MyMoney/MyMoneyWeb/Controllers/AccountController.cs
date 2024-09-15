using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Common;
using Common.Core;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using MyMoneyWeb.Filters;
using MyMoneyWeb.Helpers;
using MyMoneyWeb.Models;
using MyMoneyWeb.Structure;
using ServiceRequest.Account;
using ServiceResponse;
using ServiceWorker;

namespace MyMoneyWeb.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        public ActionResult LoginDemo(string returnUrl)
        {
            return Login(new LoginModel {Password = "123123", RememberMe = true, UserName = "demo"}, returnUrl);
        }

        [AllowAnonymous]
        public ActionResult ConfirmEmail(string login, string code)
        {
            var model = new ConfirmEmailResultModel();
            model.Result = new OperationResult();
            if (String.IsNullOrEmpty(login) || String.IsNullOrEmpty(code))
            {
                model.Result.Type = OperationResultTypes.Error;
                model.Result.Message = "Ошибочка :(";
                return PartialView("ConfirmEmailResult", model);
            }
            var request = new ConfirmEmailRequest();
            request.Login = login;
            request.Code = code;
            var loginResponse = AccountWorker.ConfirmEmail(request, Request);
            if (loginResponse.Type != ResponseType.Success)
            {
                model.Result.Type = OperationResultTypes.Warning;
                model.Result.Message = loginResponse.ResponseMessage;
                return PartialView("ConfirmEmailResult", model);
            }
            model.Result.Type = OperationResultTypes.Success;
            model.Result.Message = "Почта успешно подтверждена";
            return PartialView("ConfirmEmailResult", model);
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var loginRequest = new LoginRequest();
                loginRequest.Login = model.UserName;
                loginRequest.Password = model.Password;
                var loginResponse = AccountWorker.Login(loginRequest, Request);
                if (loginResponse.Type != ResponseType.Success)
                {
                    ModelState.AddModelError("", loginResponse.ResponseMessage);
                    return View(model);
                }
                var token = loginResponse.Body.Token;
                var userId = loginResponse.Body.UserId;
                var serModel = new AuthCookieSerializeModel();
                serModel.Token = token;
                serModel.UserId = userId;
                Response.SetAuthCookie(serModel);

                var password = "217razgnevannihObez'yan";
                var login = model.UserName;//.ToLower();
                //if(!WebSecurity.UserExists(login))
                //{
                //    WebSecurity.CreateUserAndAccount(login, password);
                //}
                if (WebSecurity.Login(login, password, persistCookie: model.RememberMe))
                {

                }
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Response.ClearAuthCookie();
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    var registrationRequest = new RegistrationRequest();
                    registrationRequest.Login = model.UserName;
                    registrationRequest.Password = model.Password;
                    registrationRequest.Email = model.Email;
                    var registrationResponse = AccountWorker.Registration(registrationRequest, Request);
                    if (registrationResponse.Type != ResponseType.Success)
                    {
                        ModelState.AddModelError("", registrationResponse.ResponseMessage);
                        return View(model);
                    }
                    var token = registrationResponse.Body.Token;
                    var userId = registrationResponse.Body.UserId;
                    var serModel = new AuthCookieSerializeModel();
                    serModel.Token = token;
                    serModel.UserId = userId;
                    Response.SetAuthCookie(serModel);
                    var login = model.UserName;//.ToLower();
                    var password = "217razgnevannihObez'yan";
                    WebSecurity.CreateUserAndAccount(login, password);
                    WebSecurity.Login(login, password);
                    return RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Пароль успешно сменён."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        var changePasswordRequest = new СhangePasswordRequest();
                        changePasswordRequest.Token = Request.GetAuthToken();
                        changePasswordRequest.OldPassword = model.OldPassword;
                        changePasswordRequest.NewPassword = model.NewPassword;
                        var registrationResponse = AccountWorker.СhangePassword(changePasswordRequest, Request);
                        if (registrationResponse.Type != ResponseType.Success)
                        {
                            ModelState.AddModelError("", registrationResponse.ResponseMessage);
                            return View(model);
                        }

                        var password = "217razgnevannihObez'yan";
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, password, password);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetComments()
        {
            var token = Request.GetAuthToken();
            var request = new GetCommentsRequest();
            request.Token = token;
            var response = AccountWorker.GetComments(request, Request);
            if (response.Type != ResponseType.Success)
            {
                return PartialView("MyError", new ErrorModel { Message = response.ResponseMessage });
            }
            var model = new CommentsModel();
            model.Comments = new List<CommentModel>();
            foreach(var comment in response.Body.Comments)
            {
                var c = new CommentModel();
                c.Author = comment.Author;
                c.CreateDate = comment.CreateDate;
                c.Text = comment.Text;
                c.Title = comment.Title;
                model.Comments.Add(c);
            }
            return PartialView("Comments", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetCommentForm()
        {
            var model = new CommentFormModel();
            return PartialView("CommentForm", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SaveComment(CommentFormModel debtForm)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("CommentForm", debtForm);
            }
            var token = Request.GetAuthToken();
            var comment = debtForm.Comment;
            var request = new CreateCommentRequest();
            request.Token = token;
            request.Author = comment.Author;
            request.Text = comment.Text;
            request.Title = comment.Title;
            var getCategoriesResponse = AccountWorker.CreateComment(request, Request);
            if (getCategoriesResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = getCategoriesResponse.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }

    }
}