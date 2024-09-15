using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Common.Enums;

namespace MyMoneyWeb.Models
{
    public class DocumentTemplateGroupsModel
    {
        public List<DocumentTemplateGroupModel> DocumentTemplateGroups { get; set; }
    }

    public class DocumentTemplateGroupModel
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public List<ReplaceWordModel> ReplaceWords { get; set; }

        public List<DocumentTemplateModel> DocumentTemplates { get; set; }
    }

    public class ReplaceWordModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class DocumentTemplateModel
    {
        /// <summary>
        /// Тип.
        /// </summary>
        public FileTypes Type { get; set; }

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Наименование скачиваемого фаила.
        /// </summary>
        /// <remarks>
        /// Шаблоны замены слов, работают и на это свойство.
        /// </remarks>
        public string DownloadFileName { get; set; }

        /// <summary>
        /// Название фаила на жёстком диске.
        /// </summary>
        public string FileName { get; set; }
    }
}
