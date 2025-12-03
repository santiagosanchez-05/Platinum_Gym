using System.Collections.Generic;
using Platinum_Gym_System.Models;

namespace Platinum_Gym_System.ViewModels
{
    public class SaleCreateVM
    {
        public Sale Sale { get; set; }
        public List<SaleDetail> Items { get; set; }
    }
}
