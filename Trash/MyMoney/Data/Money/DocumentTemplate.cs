namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Runtime.Serialization;

    [Table("Money.DocumentTemplate")]
    public partial class DocumentTemplate
    {
        [Key]
        public int Id { get; set; }

        public int DocumentTemplateGroupId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string FileName { get; set; }
        
        [Required]
        public string DownloadFileName { get; set; }
    }
}
