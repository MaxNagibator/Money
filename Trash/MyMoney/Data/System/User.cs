namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("System.User")]
    public partial class User
    {
        public User()
        {
            Car = new HashSet<Car>();
            CarEvent = new HashSet<CarEvent>();
            Category = new HashSet<Category>();
            Debt = new HashSet<Debt>();
            DebtUser = new HashSet<DebtUser>();
            FastOperation = new HashSet<FastOperation>();
            Payment = new HashSet<Payment>();
            Place = new HashSet<Place>();
            RegularTask = new HashSet<RegularTask>();
            Log = new HashSet<Log>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [StringLength(2048)]
        public string Token { get; set; }

        public DateTime CreateDate { get; set; }

        [StringLength(1024)]
        public string Email { get; set; }

        public bool EmailConfirm { get; set; }

        public string EmailSendCode { get; set; }

        public DateTime? EmailSendCodeDate { get; set; }

        public virtual ICollection<Car> Car { get; set; }

        public virtual ICollection<CarEvent> CarEvent { get; set; }

        public virtual ICollection<Category> Category { get; set; }

        public virtual ICollection<Debt> Debt { get; set; }

        public virtual ICollection<DebtUser> DebtUser { get; set; }

        public virtual ICollection<FastOperation> FastOperation { get; set; }

        public virtual ICollection<Payment> Payment { get; set; }

        public virtual ICollection<Place> Place { get; set; }

        public virtual ICollection<RegularTask> RegularTask { get; set; }

        public virtual ICollection<Log> Log { get; set; }
    }
}
