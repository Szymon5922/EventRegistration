using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class RegistrationDbContext : DbContext
    {
        public DbSet<RegistrationData> Registrations { get; set; }
        public RegistrationDbContext(DbContextOptions<RegistrationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new RegistrationDataConfiguration());
        }
    }
}
