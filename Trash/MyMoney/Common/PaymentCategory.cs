using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;

namespace Common
{
    public class PaymentCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public int? Order { get; set; }
        public string Color { get; set; }
        public PaymentTypes PaymentType { get; set; }
    }

    public static class PaymentCategoryHelper
    {
        public static List<PaymentCategory> SortByParent(this List<PaymentCategory> categories, int? parentId = null)
        {
            var c = new List<PaymentCategory>();
            foreach (var cat in categories.Where(x => x.ParentId == parentId))
            {
                c.Add(cat);
                c.AddRange(categories.SortByParent(cat.Id));
            }
            return c;
        }
    }
}
