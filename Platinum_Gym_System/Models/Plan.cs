using System.ComponentModel.DataAnnotations;

namespace Platinum_Gym_System.Models
{
    public class Plan
    {
        [Key]
        public int PlanId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        [Required]
        public int DurationMonths { get; set; }

        [Required]
        [Range(0, 9999)]
        [DataType(DataType.Currency)]
        public double Price { get; set; }

        [Required]
        public byte State { get; set; }

        // Navegación
        public ICollection<Subscription>? Subscriptions { get; set; }
    }
}
