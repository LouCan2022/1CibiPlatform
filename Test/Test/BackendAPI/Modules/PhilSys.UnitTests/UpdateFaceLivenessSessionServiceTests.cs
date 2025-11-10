using BuildingBlocks.Exceptions;
using FluentAssertions;
using Moq;
using PhilSys.Data.Entities;
using PhilSys.DTO;
using PhilSys.Services;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Test.BackendAPI.Modules.PhilSys.UnitTests.Fixture;

namespace Test.BackendAPI.Modules.PhilSys.UnitTests
{
	public class UpdateFaceLivenessSessionServiceTests : IClassFixture<PhilSysServiceFixture>
	{
		private readonly PhilSysServiceFixture _fixture;

		public UpdateFaceLivenessSessionServiceTests(PhilSysServiceFixture fixture)
		{
			_fixture = fixture;
		}

		[Fact]
		public async Task UpdateFaceLivenessSessionAsync_ShouldThrow_WhenFailedToUpdateTransactionStatus()
		{
			// Arrange
			var service = _fixture.UpdateFaceLivenessSessionService;
			var hash_token = "hash-token";
			var face_liveness_session_id = "valid-session-id";
			_fixture.MockPhilSysRepository.Setup(x => x.GetTransactionDataByHashTokenAsync(
				It.IsAny<string>()
			)).ReturnsAsync((PhilSysTransaction?)null);
		
			// Act
			Func<Task> act = async () => await service.UpdateFaceLivenessSessionAsync(
				hash_token,
				face_liveness_session_id
			);

			// Assert
			await act.Should().ThrowAsync<InternalServerException>().WithMessage("No transaction record found for your Token. Face Liveness Session update aborted.");
		}

		[Fact]
		public async Task UpdateFaceLivenessSessionAsync_ShouldThrow_WhenFailedToUpdateTransactionData()
		{
			// Arrange
			var service = _fixture.UpdateFaceLivenessSessionService;
			var hash_token = "hash-token";
			var face_liveness_session_id = "valid-session-id";
			var philsysTransaction = new PhilSysTransaction
			{
				Tid = Guid.NewGuid(),
				InquiryType = "pcn",
				PCN = "6786785465456459",
				HashToken = "hash-token",
				WebHookUrl = "/",
				CreatedAt = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow.AddMinutes(5)
			};
			_fixture.MockPhilSysRepository.Setup(x => x.GetTransactionDataByHashTokenAsync(
				It.IsAny<string>()
			)).ReturnsAsync(philsysTransaction);
			_fixture.MockPhilSysRepository.Setup(x => x.UpdateTransactionDataAsync(
				philsysTransaction
			)).ReturnsAsync((PhilSysTransaction?)null);

			// Act
			Func<Task> act = async () => await service.UpdateFaceLivenessSessionAsync(
				hash_token,
				face_liveness_session_id
			);

			// Assert
			await act.Should().ThrowAsync<InternalServerException>().WithMessage("No transaction record found for your Token. Face Liveness Session update aborted.");
		}

		[Fact]
		public async Task UpdateFaceLivenessSessionAsync_ShouldThrow_WhenUpdateFaceLivenessSessionFails()
		{
			// Arrange
			var service = _fixture.UpdateFaceLivenessSessionService;
			var hash_token = "vhash-token";
			var face_liveness_session_id = "valid-session-id";
			_fixture.MockPhilSysRepository.Setup(x => x.UpdateFaceLivenessSessionAsync(
				hash_token,
				face_liveness_session_id
			)).ReturnsAsync((PhilSysTransaction?)null); 

			// Act
			Func<Task> act = async () => await service.UpdateFaceLivenessSessionAsync(
				hash_token,
				face_liveness_session_id
			);
			// Assert
			await act.Should().ThrowAsync<InternalServerException>().WithMessage("No transaction record found for your Token. Face Liveness Session update aborted.");
		}

		[Fact]
		public async Task UpdateFaceLivenessSessionAsync_ShouldReturnData_WhenSuccessful()
		{
			// Arrange
			var hash_token = "hash-token";
			var face_liveness_session_id = "valid-session-id";
			var philsysTransaction = new PhilSysTransaction
			{
				Tid = Guid.NewGuid(),
				InquiryType = "pcn",
				PCN = "6786785465456459",
				HashToken = "hash-token",
				WebHookUrl = "/",
				CreatedAt = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow.AddMinutes(5)
			};
			var philsysTransactionResult = new VerificationResponseDTO
			{
				idv_session_id = Guid.NewGuid().ToString(),
				verified = true,
				data_subject = new DataSubject
				{
					digital_id = "DIG123456789",
					national_id_number = "1234-5678-9012",
					face_image_url = "https://example.com/images/face123.jpg",
					full_name = "Juan Dela Cruz",
					first_name = "Juan",
					middle_name = "Santos",
					last_name = "Dela Cruz",
					suffix = "Jr.",
					gender = "Male",
					marital_status = "Single",
					birth_date = "1990-05-15",
					email = "juan.delacruz@example.com",
					mobile_number = "+639171234567",
					blood_type = "O+",
					address = new Address
					{
						permanent = "123 Barangay Mabini, Calamba City, Laguna",
						present = "Unit 5, Tower B, Makati City, Metro Manila"
					},
					place_of_birth = new PlaceOfBirth
					{
						full = "Calamba City, Laguna, Philippines",
						municipality = "Calamba City",
						province = "Laguna",
						country = "Philippines"
					}
				}
			};
			_fixture.MockPhilSysRepository.Setup(x => x.GetTransactionDataByHashTokenAsync(
				hash_token
			)).ReturnsAsync(philsysTransaction);
			_fixture.MockPhilSysRepository.Setup(x => x.UpdateTransactionDataAsync(
				philsysTransaction
			)).ReturnsAsync(philsysTransaction);
			_fixture.MockPhilSysRepository
			.Setup(x => x.UpdateFaceLivenessSessionAsync(hash_token, face_liveness_session_id))
			.ReturnsAsync(philsysTransaction);
			_fixture.GetTokenService
			.Setup(x => x.GetPhilsysTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
			.ReturnsAsync("fake-access-token");
			var service =  new UpdateFaceLivenessSessionService(
				_fixture.MockHttpClientFactory.Object,
				_fixture.MockPhilSysRepository.Object,
				_fixture.MockPhilSysResultRepository.Object,
				_fixture.MockUpdateFaceLivenessSessionLogger.Object,
				_fixture.PostBasicInformationService,
				_fixture.PostPCNService,
				_fixture.GetTokenService,
				_fixture.Configuration
			);

			// Act
			var result = await service.UpdateFaceLivenessSessionAsync(
				hash_token,
				face_liveness_session_id
			);

			// Assert
			result!.verified.Should().NotBeNull();
		}
	}
}
