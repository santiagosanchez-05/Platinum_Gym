using Platinum_Gym_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Platinum_Gym_System.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }    
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }
    }
}
