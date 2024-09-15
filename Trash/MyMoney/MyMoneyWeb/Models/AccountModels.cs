using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;
using Common.Core;

namespace MyMoneyWeb.Models
{
    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} не менее {2} символов.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить тебя?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} не менее {2} символов.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Почта(Необязательна, но вдруг забудите пароль)")]
        public string Email { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }

    public class ConfirmEmailResultModel
    {
        public OperationResult Result { get; set; }
    }

    public class CommentsModel
    {
        public List<CommentModel> Comments { get; set; }
    }

    public class CommentFormModel
    {
        [Required]
        public CommentModel Comment { get; set; }
    }

    public class CommentModel
    {
        [Required]
        [Display(Name = "Заголовок")]
        [StringLength(1000)]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Текст")]
        public string Text { get; set; }

        [Required]
        [Display(Name = "Представьтесь")]
        [StringLength(1000)]
        public string Author { get; set; }

        public string CreateDate { get; set; }
    }
}
