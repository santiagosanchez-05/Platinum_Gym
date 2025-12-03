using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platinum_Gym_System.Models
{
    public class Sale
    {
        [Key]
        public int SaleId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SaleDate { get; set; }

        [Required(ErrorMessage = "El total es obligatorio")]
        [Range(0.01, 999999)]
        [DataType(DataType.Currency)]
        public double Total { get; set; }

        // Relación con pago (opcional dependiendo del flujo)
        public int? PaymentId { get; set; }

        [ForeignKey("PaymentId")]
        public Payment? Payment { get; set; }

        // Relación 1..N con detalles
        public ICollection<SaleDetail>? SaleDetails { get; set; }
    }
}
