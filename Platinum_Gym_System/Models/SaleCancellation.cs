using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platinum_Gym_System.Models
{
    public class SaleCancellation
    {
        [Key]
        public int CancellationId { get; set; }

        [Required]
        public int SaleId { get; set; }

        [ForeignKey("SaleId")]
        public Sale Sale { get; set; }

        [Required]
        public DateTime CancellationDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(250)]
        public string Reason { get; set; }

        public string? CancelledBy { get; set; } // opcional si manejas usuarios
    }
}
