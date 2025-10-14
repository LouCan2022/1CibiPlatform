namespace Auth.Data.EntityConfiguration;

public class AuthSubMenuConfiguration : IEntityTypeConfiguration<AuthSubMenu>
{
	public void Configure(EntityTypeBuilder<AuthSubMenu> builder)
	{
		builder.HasKey(e => e.SubMenuId);

		builder.Property(e => e.SubMenuId)
			.IsRequired();

		builder.Property(e => e.CreatedAt).HasDefaultValue(DateTime.UtcNow);

		builder.Property(e => e.IsActive).HasDefaultValue(true);

	}
}
