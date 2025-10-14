
namespace Auth.Data.EntityConfiguration;

public class UserConfiguration : IEntityTypeConfiguration<Authusers>
{
	public void Configure(EntityTypeBuilder<Authusers> builder)
	{
		builder.HasKey(u => u.Id);

		builder.Property(u => u.Username).IsRequired().HasMaxLength(50);

		builder.Property(u => u.Email).IsRequired().HasMaxLength(100);

		builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);

		builder.Property(u => u.FirstName).HasMaxLength(50);

		builder.Property(u => u.LastName).HasMaxLength(50);

		builder.Property(u => u.MiddleName).HasMaxLength(50);

		builder.HasIndex(u => u.Username).IsUnique();

		builder.HasIndex(u => u.Email).IsUnique();

		builder.Property(u => u.CreatedAt).HasDefaultValue(DateTime.UtcNow);

		builder.Property(u => u.IsActive).HasDefaultValue(true);
	}

}