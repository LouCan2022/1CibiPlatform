using System.Text.Json;
using ATS.Services.AuditTrail;

namespace Test.BackendAPI.Modules.ATS.UnitTests;

public class AtsAuditRedactorTests
{
	private const string Mask = "***";

	private sealed record SubjectCommand(
		string FirstName,
		string? SSS,
		string? TIN,
		DateOnly? DOB);

	private sealed record NestedCommand(string Label, SubjectCommand Subject);

	private sealed record CollectionCommand(IReadOnlyCollection<SubjectCommand> Subjects);

	private sealed record TokenCommand(string HashToken, string Password, string Site);

	private static JsonElement Parse(string json) =>
		JsonDocument.Parse(json).RootElement;

	[Fact]
	public void Redact_ShouldMaskGovernmentIdsAndBirthdate()
	{
		var payload = AtsAuditRedactor.Redact(
			new SubjectCommand("Juan", "1111111110", "123456789234", new DateOnly(1990, 5, 17)));

		var root = Parse(payload);

		Assert.Equal(Mask, root.GetProperty("SSS").GetString());
		Assert.Equal(Mask, root.GetProperty("TIN").GetString());
		Assert.Equal(Mask, root.GetProperty("DOB").GetString());

		// The whole point is that the rest still reads normally.
		Assert.Equal("Juan", root.GetProperty("FirstName").GetString());
	}

	[Fact]
	public void Redact_ShouldNotLeaveTheSensitiveValueAnywhereInTheDocument()
	{
		var payload = AtsAuditRedactor.Redact(
			new SubjectCommand("Juan", "1111111110", "123456789234", null));

		// The masked values must be gone from the serialized text, not merely replaced on
		// the property the assertions above happen to check.
		Assert.DoesNotContain("1111111110", payload, StringComparison.Ordinal);
		Assert.DoesNotContain("123456789234", payload, StringComparison.Ordinal);
	}

	[Fact]
	public void Redact_ShouldMaskTokensAndPasswords()
	{
		var payload = AtsAuditRedactor.Redact(
			new TokenCommand("abc123", "hunter2", "24 - 7 INTOUCH- CEBU"));

		var root = Parse(payload);

		Assert.Equal(Mask, root.GetProperty("HashToken").GetString());
		Assert.Equal(Mask, root.GetProperty("Password").GetString());
		Assert.Equal("24 - 7 INTOUCH- CEBU", root.GetProperty("Site").GetString());
	}

	[Fact]
	public void Redact_ShouldRecurseIntoNestedObjects()
	{
		var payload = AtsAuditRedactor.Redact(
			new NestedCommand("Order", new SubjectCommand("Juan", "1111111110", null, null)));

		var subject = Parse(payload).GetProperty("Subject");

		Assert.Equal(Mask, subject.GetProperty("SSS").GetString());
		Assert.Equal("Juan", subject.GetProperty("FirstName").GetString());
	}

	[Fact]
	public void Redact_ShouldRecurseIntoArrays()
	{
		// AddUserCommand and AddClientCommand both take collections, so an array element
		// is the common case rather than an edge one.
		var payload = AtsAuditRedactor.Redact(
			new CollectionCommand(
			[
				new SubjectCommand("Juan", "1111111110", null, null),
				new SubjectCommand("Maria", "2222222220", null, null)
			]));

		var subjects = Parse(payload).GetProperty("Subjects");

		Assert.Collection(
			subjects.EnumerateArray(),
			first => Assert.Equal(Mask, first.GetProperty("SSS").GetString()),
			second => Assert.Equal(Mask, second.GetProperty("SSS").GetString()));
	}

	[Theory]
	[InlineData("sss")]
	[InlineData("Sss")]
	[InlineData("SSS")]
	public void Redact_ShouldMatchPropertyNamesCaseInsensitively(string propertyName)
	{
		var payload = AtsAuditRedactor.Redact(
			new Dictionary<string, string> { [propertyName] = "1111111110" });

		Assert.DoesNotContain("1111111110", payload, StringComparison.Ordinal);
	}

	[Fact]
	public void Redact_ShouldLeaveNullsAndNumbersAsTheyAre()
	{
		var payload = AtsAuditRedactor.Redact(new { Count = 3, Name = (string?)null });

		var root = Parse(payload);

		Assert.Equal(3, root.GetProperty("Count").GetInt32());
		Assert.Equal(JsonValueKind.Null, root.GetProperty("Name").ValueKind);
	}

	[Fact]
	public void Redact_ShouldDropAPayloadTooLargeToStore()
	{
		// A bulk upload command carries every row of a spreadsheet; without the cap one
		// action could write a megabyte row.
		var oversized = new CollectionCommand(
			Enumerable.Range(0, 500)
				.Select(index => new SubjectCommand(new string('x', 100), null, null, null))
				.ToArray());

		var payload = AtsAuditRedactor.Redact(oversized);

		Assert.Equal(AtsAuditRedactor.OversizedPayload, payload);

		// Still valid JSON, so the jsonb column accepts it and the viewer can render it.
		Assert.Equal(JsonValueKind.Object, Parse(payload).ValueKind);
	}

	[Fact]
	public void Redact_ShouldReturnAMarkerRatherThanThrow_WhenThePayloadCannotBeSerialized()
	{
		// Recording an action must never fail because of the shape of its payload.
		var payload = AtsAuditRedactor.Redact(new SelfReferencing());

		Assert.Equal(JsonValueKind.Object, Parse(payload).ValueKind);
	}

	private sealed class SelfReferencing
	{
		public SelfReferencing() => Self = this;

		public SelfReferencing Self { get; }
	}
}
