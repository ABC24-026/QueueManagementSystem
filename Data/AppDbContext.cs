using Microsoft.EntityFrameworkCore;
using QueueManagementSystem.Models;

namespace QueueManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<SmsMessage> SmsMessages { get; set; }
    }
}