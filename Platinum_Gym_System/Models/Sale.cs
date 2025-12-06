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

        public ICollection<SalePayment>? SalePayments { get; set; }
        public ICollection<SaleDetail>? SaleDetails { get; set; }
        public bool IsCancelled { get; set; } = false;

        public ICollection<SaleCancellation>? Cancellations { get; set; }

    }
}
