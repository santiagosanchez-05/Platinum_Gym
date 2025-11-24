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
    }
}
