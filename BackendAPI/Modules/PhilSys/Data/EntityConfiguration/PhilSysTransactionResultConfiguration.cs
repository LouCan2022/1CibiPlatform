namespace PhilSys.Data.EntityConfiguration;

public class PhilSysTransactionResultConfiguration : IEntityTypeConfiguration<PhilSysTransactionResult>
{
	public void Configure(EntityTypeBuilder<PhilSysTransactionResult> builder)
	{
		builder.ToTable("PhilSysTransactionResult", "philsys");

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

		builder.OwnsOne(
		   ptr => ptr.data_subject,
		   nav =>
		   {
			   nav.Property(p => p.digital_id);
			   nav.Property(p => p.national_id_number);
			   nav.Property(p => p.face_url);
			   nav.Property(p => p.full_name);
			   nav.Property(p => p.first_name);
			   nav.Property(p => p.middle_name);
			   nav.Property(p => p.last_name);
			   nav.Property(p => p.suffix);
			   nav.Property(p => p.gender);
			   nav.Property(p => p.marital_status);
			   nav.Property(p => p.blood_type);
			   nav.Property(p => p.email);
			   nav.Property(p => p.mobile_number);
			   nav.Property(p => p.birth_date);
			   nav.Property(p => p.full_address);
			   nav.Property(p => p.address_line_1);
			   nav.Property(p => p.address_line_2);
			   nav.Property(p => p.barangay);
			   nav.Property(p => p.municipality);
			   nav.Property(p => p.province);
			   nav.Property(p => p.country);
			   nav.Property(p => p.postal_code);
			   nav.Property(p => p.present_full_address);
			   nav.Property(p => p.present_address_line_1);
			   nav.Property(p => p.present_address_line_2);
			   nav.Property(p => p.present_barangay);
			   nav.Property(p => p.present_municipality);
			   nav.Property(p => p.present_province);
			   nav.Property(p => p.present_country);
			   nav.Property(p => p.present_postal_code);
			   nav.Property(p => p.residency_status);
			   nav.Property(p => p.place_of_birth);
			   nav.Property(p => p.pob_municipality);
			   nav.Property(p => p.pob_province);
			   nav.Property(p => p.pob_country);
		   });
	}
}
