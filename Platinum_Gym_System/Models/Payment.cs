using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platinum_Gym_System.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        [Range(0, 999999)]
        [DataType(DataType.Currency)]
        public double Amount { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        public int SubscriptionId { get; set; } 

        [ForeignKey("SubscriptionId")]
        public Subscription? Subscription { get; set; }
        [Required]
        public byte State { get; set; }
    }
}
