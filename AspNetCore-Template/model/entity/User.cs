using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetCore_Template.model.entity;

public enum AvailableLocales
{
    En,
    Sk,
    Cz
}

public class User : IdentityUser<long>
{
    [Required] public AvailableLocales CurrentLocale { get; set; } = AvailableLocales.Sk;
    [Required] public string Name { get; set; }
    [Required] public string Surname { get; set; }
    [Required] public TimeZoneInfo Timezone { get; set; }
    [Required] public bool IsOAuth2Only { get; set; }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Primary key
        builder.HasKey(u => u.Id);

        // Indexes
        builder.HasIndex(u => u.Email).IsUnique(); // Unique constraint on Email

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256); // Adjust length as needed

        builder.Property(u => u.PasswordHash)
            .IsRequired();
        builder.Property(u => u.CurrentLocale)
            .HasConversion(
                v => v.ToString(),
                v => (AvailableLocales)Enum.Parse(typeof(AvailableLocales), v))
            .IsRequired();
        // Time zone conversion
        builder.Property(u => u.Timezone)
            .HasConversion(tz => tz.Id,
                id => TimeZoneInfo.FindSystemTimeZoneById(id));

 
    }
}