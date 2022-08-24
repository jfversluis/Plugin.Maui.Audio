namespace Plugin.Maui.Audio;

public class PlayAudioBehavior : Behavior<View>
{
	IAudioPlayer? audioPlayer;
	readonly TapGestureRecognizer tapGestureRecognizer;

	public static readonly BindableProperty AudioFileProperty =
		BindableProperty.Create(
			nameof(AudioFile),
			typeof(string),
			typeof(PlayAudioBehavior),
			propertyChanged: OnAudioFilePropertyChanged);

	public string? AudioFile // TODO: AudioSource???
	{
		get => (string?)GetValue(AudioFileProperty);
		set => SetValue(AudioFileProperty, value);
	}

	public PlayAudioBehavior()
	{
		tapGestureRecognizer = new TapGestureRecognizer();
	}

	private void OnTapGestureRecognizerTapped(object? sender, EventArgs e)
	{
		audioPlayer?.Play();
	}

	protected override void OnAttachedTo(View bindable)
	{
		base.OnAttachedTo(bindable);

		TryCreateAudioPlayer();

		tapGestureRecognizer.Tapped += OnTapGestureRecognizerTapped;
		bindable.GestureRecognizers.Add(tapGestureRecognizer);
	}

	private static void OnAudioFilePropertyChanged(BindableObject sender, object oldValue, object newValue)
	{
		((PlayAudioBehavior)sender).TryCreateAudioPlayer();
	}

	protected override void OnBindingContextChanged()
	{
		base.OnBindingContextChanged();

		TryCreateAudioPlayer();
	}

	protected override void OnDetachingFrom(View bindable)
	{
		base.OnDetachingFrom(bindable);

		tapGestureRecognizer.Tapped -= OnTapGestureRecognizerTapped;
		bindable.GestureRecognizers.Remove(tapGestureRecognizer);

		audioPlayer?.Dispose();
		audioPlayer = null;
	}

	private async void TryCreateAudioPlayer()
	{
		if (AudioFile is null || audioPlayer is not null)
		{
			return;
		}

		// TODO: how best to load files?
		var fileStream = await FileSystem.OpenAppPackageFileAsync(AudioFile);

		audioPlayer = AudioManager.Current.CreatePlayer(fileStream);
	}
}
