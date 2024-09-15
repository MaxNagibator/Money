using System.Collections.Generic;
using Common.Enums;

namespace DataWorker.Classes
{
    /// <summary>
    /// Шаблон документа.
    /// </summary>
    public class DocumentTemplate
    {
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
