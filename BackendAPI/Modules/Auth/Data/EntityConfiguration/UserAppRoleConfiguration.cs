namespace Auth.Data.EntityConfiguration;

public class UserAppRoleConfiguration : IEntityTypeConfiguration<AuthUserAppRole>
{
    public void Configure(EntityTypeBuilder<AuthUserAppRole> builder)
    {
        builder.HasKey(uar => new { uar.UserId, uar.AppId, uar.RoleId });

        builder.HasOne<Authusers>()
            .WithMany()
            .HasForeignKey(uar => uar.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AuthApplication>()
            .WithMany()
            .HasForeignKey(uar => uar.AppId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AuthRole>()
            .WithMany()
            .HasForeignKey(uar => uar.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Authusers>()
            .WithMany()
            .HasForeignKey(uar => uar.AssignedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
