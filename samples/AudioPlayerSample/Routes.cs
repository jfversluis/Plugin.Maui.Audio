namespace AudioPlayerSample;

public static class Routes
{
	public static class AudioRecorder
	{
		public const string RouteName = "audio-recorder";
	}

	public static class MusicPlayer
	{
		public const string RouteName = "music-player";

		public static class Arguments
		{
			public const string Music = "music";
		}
	}
}