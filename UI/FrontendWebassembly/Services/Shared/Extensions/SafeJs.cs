namespace FrontendWebassembly.Services.Shared.Extensions;

/// <summary>
/// JS interop that is safe to call while a component is being torn down.
/// </summary>
/// <remarks>
/// Disposing a JS module or telling it to detach an observer happens in Dispose, which
/// runs during navigation - by which point the browser may already have discarded the
/// object, or the renderer may be gone. Both cases throw, and neither is a fault: the
/// thing being cleaned up no longer exists, which is the outcome that was wanted.
///
/// This is the client-side counterpart to BuildingBlocks' SideEffectGuard, and exists for
/// the same reason: so cleanup code does not sprout its own try/catch in every component.
/// Only the two exceptions that mean "already gone" are swallowed - anything else is a
/// real bug and still surfaces.
/// </remarks>
public static class SafeJs
{
	public static async ValueTask InvokeVoidAsync(
		IJSObjectReference module,
		string identifier,
		params object?[]? args)
	{
		try
		{
			await module.InvokeVoidAsync(identifier, args ?? []);
		}
		catch (JSDisconnectedException)
		{
			// Circuit gone; nothing to clean up on the other side.
		}
		catch (ObjectDisposedException)
		{
			// The reference was already disposed.
		}
	}

	public static async ValueTask DisposeAsync(IJSObjectReference module)
	{
		try
		{
			await module.DisposeAsync();
		}
		catch (JSDisconnectedException)
		{
		}
		catch (ObjectDisposedException)
		{
		}
	}
}
