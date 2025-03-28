using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace worker_service
{
    public class AppDbContext: DbContext
    {
        public DbSet<Coin> Coins { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        // Override OnModelCreating if you need to configure the model further
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional configurations can go here (e.g., indexes, constraints)
        }
    }
}
