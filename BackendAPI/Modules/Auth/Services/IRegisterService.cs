﻿namespace Auth.Services;

public interface IRegisterService
{
	Task<OtpVerificationResponse> RegisterAsync(RegisterRequestDTO registerRequestDTO);

	Task<bool> VerifyOtpAsync(string email, string otp);

	Task<bool> ResendOtpAsync(OtpVerification otpVerification);
}
