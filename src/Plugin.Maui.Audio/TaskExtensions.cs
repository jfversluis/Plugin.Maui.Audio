namespace Plugin.Maui.Audio;

/// <summary>
/// Provides extension methods for Task-related operations.
/// </summary>
public static class TaskExtensions
{
	/// <summary>
	/// Provides a mechanism to await until the supplied <paramref name="cancellationToken"/> has been cancelled.
	/// </summary>
	/// <param name="cancellationToken">The <see cref="CancellationToken"/> to await.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public static Task WhenCanceled(this CancellationToken cancellationToken)
	{
		var completionSource = new TaskCompletionSource<bool>();

		cancellationToken.Register(
			input =>
			{
				if (input is TaskCompletionSource<bool> taskCompletionSource)
				{
					taskCompletionSource.SetResult(true);
				}
			},
			completionSource);

		return completionSource.Task;
	}
}
