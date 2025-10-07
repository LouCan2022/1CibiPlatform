namespace Auth.Data.EntityConfiguration;

public class AuthSubMenuConfiguration : IEntityTypeConfiguration<AuthSubMenu>
{
	public void Configure(EntityTypeBuilder<AuthSubMenu> builder)
	{
		builder.HasKey(e => e.SubMenuId);
	}
}
