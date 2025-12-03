using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platinum_Gym_System.Models
{
    public class SaleDetail
    {
        [Key]
        public int DetailId { get; set; }

        [Required]
        public int SaleId { get; set; }

        [ForeignKey("SaleId")]
        public Sale? Sale { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mínimo 1")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, 999999)]
        [DataType(DataType.Currency)]
        public double Subtotal { get; set; }
    }
}
