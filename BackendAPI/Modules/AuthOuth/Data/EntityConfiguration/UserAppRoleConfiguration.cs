namespace Auth.Data.EntityConfiguration;

public class UserAppRoleConfiguration : IEntityTypeConfiguration<UserAppRole>
{
    public void Configure(EntityTypeBuilder<UserAppRole> builder)
    {
        builder.HasKey(uar => new { uar.UserId, uar.AppId, uar.RoleId });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(uar => uar.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Application>()
            .WithMany()
            .HasForeignKey(uar => uar.AppId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(uar => uar.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(uar => uar.AssignedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
