using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwilioConference.DataServices.Entities
{
    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual Guid Id { get; set; }

        [Column(TypeName = "NVARCHAR")]
        [StringLength(40)]
        public virtual string AccountsID { get; set; }

        [Column(TypeName = "NVARCHAR")]
        [StringLength(15)]
        public virtual string Service_User_Conference_With_Number { get; set; }

        [Column(TypeName = "NVARCHAR")]
        [StringLength(15)]
        public virtual string Service_User_Twilio_Phone_Number { get; set; }

        [Column(TypeName = "BIT")]
        public virtual Boolean AvailableStatus { get; set; }

        [Column(TypeName = "NVARCHAR")]
        [StringLength(50)]
        public virtual string UserFullName { get; set; }

        [Column(TypeName = "NVARCHAR")]
        [StringLength(30)]
        public virtual string IANATimeZone { get; set; }

    }
}
