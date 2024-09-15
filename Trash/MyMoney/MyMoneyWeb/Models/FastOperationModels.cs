using Common.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyMoneyWeb.Models
{
    public class FastOperationFormModel
    {
        public List<CategoryModel> PaymentCategories { get; set; }

        [Required]
        public FastOperationModel FastOperation { get; set; }
    }

    public class FastOperationsModel
    {
        public List<CategoryModel> PaymentCategories { get; set; }
        public List<FastOperationModel> FastOperations { get; set; }
    }

    public class FastOperationModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Категория")]
        public int? CategoryId { get; set; }

        [Display(Name = "Сумма")]
        public decimal? Sum { get; set; }

        public PaymentTypes PaymentType { get; set; }

        [Display(Name = "Место")]
        public string Place { get; set; }

        [Display(Name = "Комментарий")]
        public string Comment { get; set; }

        [Display(Name = "Параметр сортировки")]
        public int? Order { get; set; }
    }
}
