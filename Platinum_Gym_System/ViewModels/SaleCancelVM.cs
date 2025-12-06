using System.ComponentModel.DataAnnotations;

namespace Platinum_Gym_System.ViewModels
{
    public class SaleCancelVM
    {
        public int SaleId { get; set; }

        [Required(ErrorMessage = "Debe ingresar un motivo")]
        [StringLength(250)]
        public string Reason { get; set; }
    }
}
