namespace Auth.Data.EntityConfiguration;

public class RoleConfiguration : IEntityTypeConfiguration<AuthRole>
{
	public void Configure(EntityTypeBuilder<AuthRole> builder)
	{
		builder.HasKey(r => r.RoleId);

		builder.Property(r => r.RoleName).IsRequired().HasMaxLength(50);

		builder.Property(r => r.Description).HasMaxLength(255);

		builder.HasIndex(r => r.RoleName).IsUnique();

		builder.Property(r => r.CreatedAt).HasDefaultValueSql("timezone('utc', now())");

	}
}
