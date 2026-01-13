using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data
{
    public class RegistrationDataConfiguration : IEntityTypeConfiguration<RegistrationData>
    {
        public void Configure(EntityTypeBuilder<RegistrationData> builder)
        {
            builder.ToTable("registrations");
            builder.HasKey(r => r.Id);

            builder.OwnsOne(r => r.Email, email =>
            {
                email.Property(e => e.Address)
                    .HasColumnName("email")
                    .IsRequired();
            });

            builder.HasIndex("email").IsUnique();
            builder.HasIndex(r => r.ResumeToken).IsUnique();

            builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        }
    }
}
