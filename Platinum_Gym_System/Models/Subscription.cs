using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platinum_Gym_System.Models
{
    public class Subscription
    {
        [Key]
        public int SubscriptionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PlanId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public byte State { get; set; }

        // Navegación
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("PlanId")]
        public Plan? Plan { get; set; }

        public ICollection<Payment>? Payments { get; set; }
    }
}
