using System.ComponentModel;
using Extentions;

namespace ServiceNames
{
    [Description("Финансы")]
    [ServiceName("Money")]
    public enum MoneyService
    {
        [Description("Создать категорию")]
        [ServiceName("Money/CreateCategory")]
        CreateCategory,

        [Description("Изменить категорию")]
        [ServiceName("Money/UpdateCategory")]
        UpdateCategory,

        [Description("Удалить категорию")]
        [ServiceName("Money/DeleteCategory")]
        DeleteCategory,

        [Description("Получить категорию")]
        [ServiceName("Money/GetCategory")]
        GetCategory,

        [Description("Получение списка категорий")]
        [ServiceName("Money/GetCategories")]
        GetCategories,



        [Description("Создать платёж")]
        [ServiceName("Money/CreatePayment")]
        CreatePayment,

        [Description("Изменить платёж")]
        [ServiceName("Money/UpdatePayment")]
        UpdatePayment,

        [Description("Изменить пачку платежей")]
        [ServiceName("Money/UpdatePaymentsBatch")]
        UpdatePaymentsBatch,

        [Description("Удалить платёж")]
        [ServiceName("Money/DeletePayment")]
        DeletePayment,

        [Description("Получить платеж")]
        [ServiceName("Money/GetPayment")]
        GetPayment,

        [Description("Получить платежи")]
        [ServiceName("Money/GetPayments")]
        GetPayments,

        [Description("Получить статистику платежей")]
        [ServiceName("Money/GetPaymentStatistics")]
        GetPaymentStatistics,

        [Description("Получить места")]
        [ServiceName("Money/GetPlaces")]
        GetPlaces,

        [Description("Получить долги")]
        [ServiceName("Money/GetDebts")]
        GetDebts,

        [Description("Получить долг")]
        [ServiceName("Money/GetDebt")]
        GetDebt,

        [Description("Создать долг")]
        [ServiceName("Money/CreateDebt")]
        CreateDebt,

        [Description("Оплатить долг")]
        [ServiceName("Money/PayDebt")]
        PayDebt,

        [Description("Изменить долг")]
        [ServiceName("Money/UpdateDebt")]
        UpdateDebt,

        [Description("Удалить долг")]
        [ServiceName("Money/DeleteDebt")]
        DeleteDebt,

        [Description("Перенести долг в расходы")]
        [ServiceName("Money/MoveDebtToOperations")]
        MoveDebtToOperations,

        [Description("Получение держателей долгов")]
        [ServiceName("Money/GetDebtUsers")]
        GetDebtUsers,

        [Description("Слияние держателей долгов")]
        [ServiceName("Money/MergeDebtUsers")]
        MergeDebtUsers,

        [Description("Получить регулярные задачи")]
        [ServiceName("Money/GetRegularTasks")]
        GetRegularTasks,

        [Description("Получить регулярную задачу")]
        [ServiceName("Money/GetRegularTask")]
        GetRegularTask,

        [Description("Создать регулярную задачу")]
        [ServiceName("Money/CreateRegularTask")]
        CreateRegularTask,

        [Description("Обновить регулярную задачу")]
        [ServiceName("Money/UpdateRegularTask")]
        UpdateRegularTask,

        [Description("Удалить регулярную задачу")]
        [ServiceName("Money/DeleteRegularTask")]
        DeleteRegularTask,

        [Description("Запустить регулярную задачу")]
        [ServiceName("Money/RunRegularTask")]
        RunRegularTask,

        [Description("Получить быстрые операции")]
        [ServiceName("Money/GetFastOperations")]
        GetFastOperations,

        [Description("Получить быструю операцию")]
        [ServiceName("Money/GetFastOperation")]
        GetFastOperation,

        [Description("Создать быструю операцию")]
        [ServiceName("Money/CreateFastOperation")]
        CreateFastOperation,

        [Description("Обновить быструю операцию")]
        [ServiceName("Money/UpdateFastOperation")]
        UpdateFastOperation,

        [Description("Удалить быструю операцию")]
        [ServiceName("Money/DeleteFastOperation")]
        DeleteFastOperation,
    }
}