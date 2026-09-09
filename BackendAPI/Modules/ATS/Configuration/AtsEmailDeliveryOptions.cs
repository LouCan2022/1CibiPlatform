namespace ATS.Configuration;

/// <summary>
/// SMTP throughput and back-off tuning, mirroring AtsNotificationOptions. Every value has a
/// working default, so an absent section is valid; bind the "AtsEmailDelivery" section only
/// to override one.
///
/// These exist as configuration rather than constants because the safe ceiling belongs to
/// the provider, not to the code. Gmail throttled this sender at 14 messages, and finding
/// the sustainable rate for a new provider must not require a redeploy.
/// </summary>
public sealed class AtsEmailDeliveryOptions
{
	public const string SectionName = "AtsEmailDelivery";

	// Connections, not messages. Each one is authenticated once and then reused for many
	// sends, so this is the number of simultaneous SMTP sessions - which is what a provider
	// actually counts. Two is deliberately conservative for Gmail; a transactional provider
	// (SES, SendGrid) will happily take far more.
	public int MaxConcurrentConnections { get; set; } = 2;

	// Messages per second across ALL connections. The token bucket enforces this globally,
	// so raising MaxConcurrentConnections alone cannot outrun the provider.
	//
	// Gmail accepted 14 messages in roughly 8 seconds (~1.75/s) before refusing. Half that
	// is the sustainable rate, and it still clears 200 invitations in about three minutes.
	public double MaxSendsPerSecond { get; set; } = 0.9;

	// How many messages one authenticated session sends before it is torn down and rebuilt.
	// Providers cap the lifetime of a single session, and a very long-lived connection is
	// also more likely to have gone silently stale.
	public int MaxMessagesPerConnection { get; set; } = 50;

	// Generous on purpose. A 10s timeout was firing while the provider had ALREADY accepted
	// the message, so the send was recorded as failed and retried - which is how one
	// candidate received the same invitation several times.
	public int SendTimeoutSeconds { get; set; } = 60;

	// Attempts within one pass, before the row goes back to the queue for a later tick.
	public int MaxAttemptsPerPass { get; set; } = 3;

	// First retry waits this long, doubling per attempt (2s, 4s, 8s...). Fixed short delays
	// re-knock on a door that is deliberately closed.
	public int RetryBaseDelaySeconds { get; set; } = 2;

	// When the provider answers "slow down" (SMTP 4xx), the whole pass stops for this long.
	// Continuing to send into a rate limit is what turns a short throttle into a long one.
	public int ThrottleBackoffSeconds { get; set; } = 600;
}
