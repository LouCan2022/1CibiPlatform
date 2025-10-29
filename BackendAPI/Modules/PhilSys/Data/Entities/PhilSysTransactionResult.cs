﻿namespace PhilSys.Data.Entities;
public class PhilSysTransactionResult
{
	public int Trid { get; init; }
	public Guid idv_session_id { get; init; }
	public bool verified { get; init; }
	public DataSubject data_subject { get; init; }
	public string? error { get; init; }
	public string? message { get; init; }
	public string? error_description { get; init; }
}

public class DataSubjectEntity
{
	public string? digital_id { get; init; }
	public string? national_id_number { get; init; }
	public string? face_image_url { get; init; }
	public string? full_name { get; init; }
	public string? first_name { get; init; }
	public string? middle_name { get; init; }
	public string? last_name { get; init; }
	public string? suffix { get; init; }
	public string? gender { get; init; }
	public string? marital_status { get; init; }
	public string? birth_date { get; init; }
	public string? email { get; init; }
	public string? mobile_number { get; init; }
	public string? blood_type { get; init; }
	public AddressEntity address { get; init; }
	public PlaceOfBirthEntity place_of_birth { get; init; }
}

public class AddressEntity
{
	public string? permanent { get; init; }
	public string? present { get; init; }
}

public class PlaceOfBirthEntity
{
	public string? full { get; init; }
	public string? municipality { get; init; }
	public string? province { get; init; }
	public string? country { get; init; }
}


