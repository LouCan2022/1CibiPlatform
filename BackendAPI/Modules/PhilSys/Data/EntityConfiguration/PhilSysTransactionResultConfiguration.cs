namespace PhilSys.Data.EntityConfiguration;

public class PhilSysTransactionResultConfiguration : IEntityTypeConfiguration<PhilSysTransactionResult>
{
	public void Configure(EntityTypeBuilder<PhilSysTransactionResult> builder)
	{
		builder.HasKey(ptr => ptr.Trid);

		builder.Property(ptr => ptr.Trid)
			   .IsRequired();

		builder.HasOne<PhilSysTransaction>()
			   .WithOne()
			   .HasForeignKey<PhilSysTransactionResult>(ptr => ptr.idv_session_id)
			   .OnDelete(DeleteBehavior.Cascade)
			   .IsRequired();

		builder.Property(ptr => ptr.verified)
			   .IsRequired();

		builder.OwnsOne(ptr => ptr.data_subject, ds =>
		{
			ds.OwnsOne(a => a.address);
			ds.Navigation(d => d.address).IsRequired(false);

			ds.OwnsOne(pb => pb.place_of_birth);
			ds.Navigation(d => d.place_of_birth).IsRequired(false);
		});
	}
}
