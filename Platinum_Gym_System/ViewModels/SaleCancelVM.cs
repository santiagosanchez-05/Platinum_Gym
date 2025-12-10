using System.ComponentModel.DataAnnotations;

namespace Platinum_Gym_System.ViewModels
{
    public class SaleCancelVM
    {
        public int SaleId { get; set; }

        [Required(ErrorMessage = "You must enter a reason")]
        [StringLength(250)]
        public string Reason { get; set; }
    }
}
