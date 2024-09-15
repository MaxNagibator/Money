using System.ComponentModel;
using Extentions;

namespace ServiceNames
{
    [Description("Профиль")]
    [ServiceName("Account")]
    public enum Account
    {
        [Description("Регистрация")]
        [ServiceName("Account/Registration")]
        Registration,

        [Description("Авторизация")]
        [ServiceName("Account/Login")]
        Login,

        [Description("Подтверждение почты")]
        [ServiceName("Account/ConfirmEmail")]
        ConfirmEmail,

        [Description("Смена пароля")]
        [ServiceName("Account/СhangePassword")]
        СhangePassword,

        [Description("Получение отзывов")]
        [ServiceName("Account/GetComments")]
        GetComments,

        [Description("Создание отзыва")]
        [ServiceName("Account/CreateComment")]
        CreateComment,
    }
}