
namespace Auth.Data.EntityConfiguration;

public class ApplicationConfiguration : IEntityTypeConfiguration<AuthApplication>
{
    public void Configure(EntityTypeBuilder<AuthApplication> builder)
    {
        builder.HasKey(a => a.AppId);
        builder.Property(a => a.AppName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.AppCode).IsRequired().HasMaxLength(20);
        builder.HasIndex(a => a.AppName).IsUnique();
        builder.HasIndex(a => a.AppCode).IsUnique();
    }
}
