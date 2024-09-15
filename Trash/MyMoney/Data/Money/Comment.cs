namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Money.Comment")]
    public partial class Comment
    {
        public int Id { get; set; }

        [StringLength(1000)]
        public string Title { get; set; }

        public string Text { get; set; }

        [StringLength(1000)]
        public string Author { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
