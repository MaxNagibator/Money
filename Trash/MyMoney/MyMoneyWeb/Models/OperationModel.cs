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
    public class OperationModel
    {
        public List<CategoryModel> Categories { get; set; }

        public List<CategoryModel> SortedCategories
        {
            get
            {
                return Categories.SortByParent();
            }
        }

        public List<FastOperationModel> FastOperations { get; set; }
    }
}
