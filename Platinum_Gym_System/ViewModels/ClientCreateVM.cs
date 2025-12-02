using Platinum_Gym_System.Models;

namespace Platinum_Gym_System.ViewModels
{
    public class ClientCreateVM
    {
        // USER
        public string BillingName { get; set; }
        public string CI { get; set; }

        // SUSCRIPCIÓN
        public int PlanId { get; set; }

        // PAGO
        public string PaymentMethod { get; set; }

        // COMBOS
        public List<Plan>? Plans { get; set; }
    }

}
