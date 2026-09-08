namespace ATS.DTO;

public record ReportResultDTO
{
	public string? SubjectName { get; set; }
	public string? OrderStatus { get; set; }
	public string? HitStatus { get; set; }
	public string? SelectedPackage { get; set; }
	public string? ResumeFileName { get; set; }
	public string? ResumeFileKey { get; set; }
	public string? IdUploadedFileName { get; set; }
	public string? IdUploadedFileKey { get; set; }
	public string? NbiClearanceFileName { get; set; }
	public string? NbiClearanceFileKey { get; set; }

	// CoeFileName/Key keep the first-available coalesce for existing consumers;
	// the per-employer fields let callers address each COE individually.
	public string? CoeFileName { get; set; }
	public string? CoeFileKey { get; set; }
	public string? Coe1FileName { get; set; }
	public string? Coe1FileKey { get; set; }
	public string? Coe2FileName { get; set; }
	public string? Coe2FileKey { get; set; }
	public string? Coe3FileName { get; set; }
	public string? Coe3FileKey { get; set; }
	public string? DiplomaFileName { get; set; }
	public string? DiplomaFileKey { get; set; }
	public string? UploadDiplomaAt { get; set; }
	public string? BiometricPhotoFileName { get; set; }
	public string? BiometricPhotoFileKey { get; set; }
	public string? UploadBiometricPhotoAt { get; set; }
	public string? ConsentFormFileName { get; set; }
	public string? ConsentFormFileKey { get; set; }
	public string? UploadedReportFileName { get; set; }
	public string? UploadedReportFileKey { get; set; }
	public string? FilledFormAt { get; set; }
	public string? ReportUploadedAt { get; set; }
	public string? ReportStatus { get; set; }
}
