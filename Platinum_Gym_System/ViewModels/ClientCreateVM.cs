using Platinum_Gym_System.Models;
using System.ComponentModel.DataAnnotations;

namespace Platinum_Gym_System.ViewModels
{
    public class ClientCreateVM
    {
        // USER
        [Required(ErrorMessage = "El nombre es obligatorio")] //Es obligatorio
        [StringLength(50, ErrorMessage = "{0} must be: minimum {2} and maxmum {1} character long ", MinimumLength = 3)]
        public string BillingName { get; set; }
        [Required(ErrorMessage = "El CI es obligatorio.")]
        [RegularExpression(@"^\d{5,10}(-?[A-Za-z]{1,2})?$",
         ErrorMessage = "Formato inválido. Ejemplos válidos: 1234567, 1234567LP, 1234567-LP, 1234567A")]
        public string CI { get; set; }

        // SUSCRIPCIÓN
        [Required(ErrorMessage = "Debe seleccionar un plan")]
        public int PlanId { get; set; }

        // PAGO
        [Required(ErrorMessage = "Debe seleccionar un método de pago")]
        public string PaymentMethod { get; set; }

        // COMBOS
      
        public List<Plan>? Plans { get; set; }
    }

}
