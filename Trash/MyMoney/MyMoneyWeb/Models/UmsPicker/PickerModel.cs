using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMoneyWeb.Models.UmsPicker
{
    public class DropDownPickerModel
    {
        /// <summary>
        /// Id элемента в html. Без него может не работать!!
        /// </summary>
        public string Id { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Берёт значение из этого справочника у сервиса
        /// </summary>
        public string Type { get; set; }
        public string SearchTemplate { get; set; }
        public string OrderTemplate { get; set; }
        public string AdditionalValues { get; set; }
        public string ShowColumns { get; set; }

        /// <summary>
        /// default = 1
        /// </summary>
        public int? ShowSelectMax { get; set; }
        public string DefaultText { get; set; }
        public string IdColumnName { get; set; }
        public string Width { get; set; }
        /// <summary>
        /// Можно оставить пустым Type и заполнить значения пикера вручную
        /// </summary>
        public List<DropDownPickerValue> Values { get; set; }

        /// <summary>
        /// нужен рефакторинг к более человечному формату
        //      var docs = collection.Select(x => new {Guid = x.Guid, Name = x.Name});
        //      documentClassPickerModel.SelectedValues = String.Join(";", docs); 
        /// </summary>
        [Obsolete("")]
        public string SelectedValues { get; set; }
        public bool SelectedValuesRemoveIfFoundNotExists { get; set; }

        public string SelectedValue { get; set; }

        public bool IsSingle { get; set; }
        public bool IsSingleNotNullable { get; set; }
        public bool IsDisabled { get; set; }
        /// <summary>
        /// Не срабатывает на выделить всё/снять выделение со всего
        /// </summary>
        public string BeforeSelectFunction { get; set; }
        public string BeforeSelectFunctionParam { get; set; }
        public string AfterSelectFunction { get; set; }
        public string AfterSelectFunctionParam { get; set; }
        public string AfterInitFunction { get; set; }

        /// <summary>
        /// false - загрузка при инициализации
        /// true - загрузка при первом обращении
        /// </summary>
        public bool PreventLoading { get; set; }
        public bool SelectAllIsEnabled { get; set; }
        public string SelectAllText { get; set; }
        public bool CloseAfterSelect { get; set; }
        public string ContainerClass { get; set; }
        public string ContainerClassIfHasSelected { get; set; }
        public string ButtonAdditionalContent { get; set; }
        public string ButtonClass { get; set; }
        public string ButtonDefaultClass { get; set; }
        public string ButtonIconClass { get; set; }
        public bool DisableShowSelectedText { get; set; }
        public bool ShowSelectedTitle { get; set; }
        public string SelectedTitlePrefix { get; set; }
        public int TypeId { get; set; }
        public int Offset { get; set; }
        public int? DataCount { get; set; }
        public bool DisableLoadNextButton { get; set; }
        public bool IsHideSearch { get; set; }
        public int? PageCount { get; set; }
        public DropDownPickerModel()
        {
            SelectAllIsEnabled = true;
            PreventLoading = false;
        }
    }

    public class DropDownPickerValue
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public bool IsChecked { get; set; }
    }

    public class DropDownPickerValuesModel
    {
        public string Id { get; set; }
        public List<ReferenceValue> Values { get; set; }
        public int CurrentOffset { get; set; }
        public int PageCount { get; set; }
        public int SearchCount { get; set; }
        public int TotalCount { get; set; }
        public string SearchString { get; set; }
        public string ShowColumns { get; set; }
        public string IdColumnName { get; set; }
        public string AdditionalValues { get; set; }
        public string SelectAllText { get; set; }
        public bool DisableLoadNextButton { get; set; }
        public bool IsHideSearch { get; set; }
    }


    public class ReferenceValue
    {
        public ReferenceValue()
        {
            Values = new List<KeyValue>();
        }

        public ReferenceValue(List<string> columnNames)
        {
            Values = new List<KeyValue>();
            foreach (var columnName in columnNames)
            {
                Values.Add(new KeyValue(columnName, null));
            }
        }

        public List<KeyValue> Values { get; set; }

        /// <summary>
        /// for ums dropdownpicker
        /// </summary>
        public bool IsChecked { get; set; }

        public string this[string key]
        {
            get
            {
                var k = key.ToLower();
                if (k == "id" || k == "guid")
                {
                    if (Values.All(x => x.Key.ToLower() != k))
                    {
                        return Id;
                    }
                }
                return Values.First(v => v.Key.ToLower() == k).Value;
            }
            set { Values.First(v => v.Key.ToLower() == key.ToLower()).Value = value; }
        }

        [Obsolete("Используем Values ^^")]
        public string Name
        {
            get
            {
                if (Values.Count(v => v.Key.ToLower() == "name") != 0)
                {
                    return Values.First(v => v.Key.ToLower() == "name").Value;
                }
                return null;
            }
        }

        public string Id { get; set; }

        public bool IsVisible { get; set; }

        public bool IsDefault { get; set; }

        public int ChildCount { get; set; }

        public string GetValuesSum(List<string> showColumnNames)
        {
            var valSum = "";
            if (showColumnNames == null || showColumnNames.Count == 0)
            {
                showColumnNames = new List<string>();
                var name = "name";
                if (!Values.Select(v => v.Key).Contains(name))
                {
                    name = Values.First().Key;
                }
                showColumnNames.Add(name);
            }
            for (var i = 0; i < showColumnNames.Count; i++)
            {
                var val = Values.First(v => v.Key.ToLower() == showColumnNames[i].ToLower()).Value;
                if (i > 0)
                {
                    val = " " + val;
                }
                valSum += val;
            }
            return valSum;
        }
    }

    public class KeyValue
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public KeyValue()
        {
        }

        public KeyValue(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}