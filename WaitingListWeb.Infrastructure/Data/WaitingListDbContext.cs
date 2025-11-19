using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaitingListWeb.Domain.Entities;

namespace WaitingListWeb.Infrastructure.Data
{
    public class WaitingListDbContext : DbContext
    {
        public WaitingListDbContext(DbContextOptions<WaitingListDbContext> options) : base(options)
        {
        }
        public DbSet<WaitingListEntry> WaitingListEntries => Set<WaitingListEntry>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
