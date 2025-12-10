using Platinum_Gym_System.Models;
using System.ComponentModel.DataAnnotations;

namespace Platinum_Gym_System.ViewModels
{
    public class ClientCreateVM
    {
        // USER
        [Required(ErrorMessage = "The name is required")] //Es obligatorio
        [StringLength(50, ErrorMessage = "{0} must be: minimum {2} and maxmum {1} character long ", MinimumLength = 3)]
        public string BillingName { get; set; }
        [Required(ErrorMessage = "CI is required.")]
        [RegularExpression(@"^\d{5,10}(-?[A-Za-z]{1,2})?$",
         ErrorMessage = "Invalid format. Valid examples: 1234567, 1234567LP, 1234567-LP, 1234567A")]
        public string CI { get; set; }

        // SUSCRIPCIÓN
        [Required(ErrorMessage = "You must select a plan.")]
        public int PlanId { get; set; }

        // PAGO
        [Required(ErrorMessage = "You must select a payment method")]
        public string PaymentMethod { get; set; }

        // COMBOS
      
        public List<Plan>? Plans { get; set; }
    }

}
