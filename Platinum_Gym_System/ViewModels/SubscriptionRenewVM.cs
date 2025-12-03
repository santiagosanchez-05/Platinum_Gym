using Platinum_Gym_System.Models;
namespace Platinum_Gym_System.ViewModels
{
    public class SubscriptionRenewVM
    {
        public int UserId { get; set; }
        public int PlanId { get; set; }
        public string PaymentMethod { get; set; }
        public List<Plan>? Plans { get; set; }
    }
}
