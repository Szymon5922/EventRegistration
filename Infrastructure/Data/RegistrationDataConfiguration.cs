using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using EmailVO = Domain.ValueObjects.Email;

namespace Infrastructure.Data
{
    public class RegistrationDataConfiguration : IEntityTypeConfiguration<RegistrationData>
    {
        public void Configure(EntityTypeBuilder<RegistrationData> builder)
        {
            builder.ToTable("registrations");
            builder.HasKey(r => r.Id);

            builder.Property(x => x.Email)
                .HasConversion(
                    email => email.ToString(),
                    address => FromDbEmail(address))
                    .HasColumnName("email")
                    .IsRequired();

            builder.HasIndex(r => r.Email).IsUnique();

            builder.HasIndex(r => r.ResumeToken).IsUnique();

            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        }
        private static EmailVO FromDbEmail(string address)
        {
            var result = EmailVO.Create(address);
            if (result.IsFailure)
                throw new InvalidOperationException(
                    $"Invalid email value in DB: '{address}'. {result.Error}");

            return result.Value;
        }
    }
}
