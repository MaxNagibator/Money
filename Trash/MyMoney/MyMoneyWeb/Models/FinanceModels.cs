using Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MyMoneyWeb.Models
{
    public static class PaymentCategoryHelper
    {
        public static List<CategoryModel> SortByParent(this List<CategoryModel> categories, int? parentId = null)
        {
            var c = new List<CategoryModel>();
            foreach (var cat in categories.Where(x => x.ParentId == parentId))
            {
                c.Add(cat);
                c.AddRange(categories.SortByParent(cat.Id));
            }
            return c;
        }

        public static List<int> MvcArrayToList(this int[] array)
        {
            if (array != null && array.Length == 1 && array[0] == 0)
            {
                return null;
            }
            return array.ToList();
        }
    }

    public class PaymentFormModel
    {
        public List<CategoryModel> Categories { get; set; }

        [Required]
        public PaymentModel Payment { get; set; }

        public string DayId { get; set; }

        public List<CategoryModel> SortedCategories
        {
            get
            {
                return Categories.SortByParent();
            }
        }
    }

    public class PaymentModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "Категория")]
        public int? CategoryId { get; set; }

        [Required]
        [Display(Name = "Сумма")]
        public string Sum { get; set; }

        [Display(Name = "Комментарий")]
        public string Comment { get; set; }

        [Display(Name = "Место")]
        public string Place { get; set; }

        [Required]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        public PaymentTypes PaymentType { get; set; }
        public int? CreatedTaskId { get; internal set; }
    }

    //public class PaymentDayModel
    //{
    //    public List<PaymentRowModel> Payments { get; set; }
    //    public List<CategoryModel> Categories { get; set; }
    //    public string PaymentDate { get; set; }
    //    public string PaymentDayId { get; set; }
    //}

    public class PaymentRowModel
    {
        public PaymentModel Payment { get; set; }
        public string CategoryName { get; set; }
    }

    public class PaymentsModel
    {
        public string MinDate { get; set; }
        public string MaxDate { get; set; }
        public List<PaymentModel> Payments { get; set; }
        public List<CategoryModel> Categories { get; set; }
    }

    public class PaymentsStatsModel
    {
        public string MinDate { get; set; }
        public string MaxDate { get; set; }
        public List<PaymentModel> Payments { get; set; }
        public List<CategoryModel> Categories { get; set; }
    }

    public class PaymentsDayModel
    {
        public DateTime Date { get; set; }
        public List<PaymentModel> Payments { get; set; }
        public List<CategoryModel> Categories { get; set; }
    }

    public class PaymentsModelCategorySum
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public decimal MainSum { get; set; }
        public decimal TotalSum { get; set; }
        public int? ParentId { get; set; }
        public List<PaymentsModelCategorySum> Chields { get; set; }
    }

    public class SearchHelpModel
    {
        public List<HelpKeyWord> Words { get; set; }

        public class HelpKeyWord
        {
            public string Value { get; set; }
            public string Text { get; set; }
        }
    }

    public class DebtFormModel
    {
        public List<DebtTypes> Types { get; set; }

        [Required]
        public DebtModel Debt { get; set; }
    }
    public class DebtModel
    {
        public Guid Guid { get; set; }

        [Required]
        [Display(Name = "Тип")]
        public DebtTypes Type { get; set; }

        [Required]
        [Display(Name = "Сумма")]
        public decimal Sum { get; set; }

        [Required]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Комментарий")]
        public string Comment { get; set; }

        [Required]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        public int Id { get; set; }
        public DebtStatus Status { get; set; }
        public decimal PaySum { get; set; }
        public string PayComment { get; set; }
    }

    public class PayDebtFormModel
    {
        [Required]
        public int DebtId { get; set; }

        [Required]
        [Display(Name = "Сумма")]
        public decimal Sum { get; set; }

        [Display(Name = "Комментарий")]
        public string Comment { get; set; }

        [Required]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        public PayDebtInfoModel Debt { get; set; }
    }

    public class PayDebtInfoModel
    {
        [Display(Name = "Сумма")]
        public decimal Sum { get; set; }

        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Комментарий")]
        public string Comment { get; set; }
    }

    public class DebtsModel
    {
        public List<DebtModel> Debts { get; set; }
    }

    public class CategoriesModel
    {
        public List<CategoryModel> Categories { get; set; }

        public List<CategoryModel> SortedCategories
        {
            get
            {
                return Categories.SortByParent();
            }
        }
    }

    public class CategoryFormModel
    {
        public List<CategoryModel> Categories { get; set; }

        [Required]
        public CategoryModel Category { get; set; }
    }

    public class CategoryModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name="Значение для сортировки")]
        public int? Order { get; set; }

        [Display(Name = "Родитель")]
        public int? ParentId { get; set; }

        [Display(Name = "Цвет")]
        public string Color { get; set; }

        public int PaymentTypeId { get; set; }
    }


    public class RegularTaskFormModel
    {
        public List<RegularTaskTypes> Types { get; set; }

        public List<RegularTaskTimeTypes> TimeTypes { get; set; }

        public List<CategoryModel> PaymentCategories { get; set; }

        [Required]
        public RegularTaskModel RegularTask { get; set; }
    }

    public class RegularTasksModel
    {
        public List<RegularTaskModel> RegularTasks { get; set; }
    }

    public class RegularTaskModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "Тип")]
        public RegularTaskTypes Type { get; set; }

        [Required]
        [Display(Name = "Тип запуска")]
        public RegularTaskTimeTypes TimeType { get; set; }

        public int? TimeValue { get; set; }

        [Display(Name = "День недели(от 1 до 7)")]
        public int? TimeValue2 { get; set; }

        [Display(Name = "Число месяца")]
        public int? TimeValue3 { get; set; }

        [Required]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Дата с")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "Дата по")]
        public DateTime? DateTo { get; set; }

        public PaymentModel Payment { get; set; }

        public DateTime? RunTime { get; set; }

        public class PaymentModel
        {
            [Display(Name = "Категория")]
            public int? CategoryId { get; set; }

            [Display(Name = "Сумма")]
            public decimal? Sum { get; set; }

            public PaymentTypes PaymentType { get; set; }

            [Display(Name = "Место")]
            public string Place { get; set; }

            [Display(Name = "Комментарий")]
            public string Comment { get; set; }
        }
    }
}
