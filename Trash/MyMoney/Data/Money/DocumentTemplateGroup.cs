namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Money.DocumentTemplateGroup")]
    public partial class DocumentTemplateGroup
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int DocumentTemplateGroupId { get; set; }

        [Required]
        public string Name { get; set; }
        
        [Required]
        public string ReplaceWords { get; set; }
    }
}
