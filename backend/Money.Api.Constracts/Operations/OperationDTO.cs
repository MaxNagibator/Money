namespace Money.Api.Constracts.Operations
{
    public class OperationDTO : OperationDTODetails
    {
        public int Id { get; set; }

        /// <summary>
        ///     Идентификатор родительской регулярной задачи.
        /// </summary>
        /// <remarks>
        ///     Не null, если операция создана регулярной задачей.
        /// </remarks>
        public int? CreatedTaskId { get; set; }
    }
}