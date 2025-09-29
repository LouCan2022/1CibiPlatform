namespace Auth.Data.EntityConfiguration;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.RoleId);
        builder.Property(r => r.RoleName).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Description).HasMaxLength(255);
        builder.HasIndex(r => r.RoleName).IsUnique();
    }
}
