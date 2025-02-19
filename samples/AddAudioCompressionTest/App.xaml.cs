using Plugin.Maui.Audio;
using System.Diagnostics;

namespace AddAudioCompressionTest;

public partial class App : Application {
	public App() {
	}

	protected override Window CreateWindow(IActivationState? activationState) {
		return new Window(new MyTestPage());
	}
}
public partial class MyTestPage : ContentPage {
	IAudioManager audioManager;
	IAudioRecorder audioRecorder;
	IAudioPlayer audioPlayer;
	IAudioSource recordedAudio;

	AbsoluteLayout absButton;

	public MyTestPage() {

		AbsoluteLayout abs = new();
		this.Content = abs;

		absButton = new();
		absButton.BackgroundColor = Colors.AliceBlue;
		abs.Add(absButton);

		this.SizeChanged += delegate {
			if (this.Width > 0) {
				absButton.WidthRequest = this.Width;
				absButton.HeightRequest = this.Height;
			}
		};

		//initialize audio components:
		audioManager = AudioManager.Current;
		audioRecorder = audioManager.CreateRecorder();

		//main recorder click function:
		TapGestureRecognizer recordTap = new();
		recordTap.Tapped += delegate {
			//pause, play if audio exists
			if (audioRecorder.IsRecording) {
				_ = stopRecording(); //stop recording if recording
			}
			else {
				if (recordedAudio == null) {
					_ = startRecording(); //start recording if no recorded audio
				}
				else {
					if (audioPlayer?.IsPlaying ?? false) {
						_ = pausePlayback(); //pause if player exists and is playing
					}
					else {
						_ = startPlaying(); //create player if needed and start playing
					}
				}
			}
		};
		absButton.GestureRecognizers.Add(recordTap);

		//add clear button:
		Label clearLabel = new();
		clearLabel.FontSize = 50;
		clearLabel.Text = "CLEAR AUDIO";
		clearLabel.BackgroundColor = Colors.Magenta;
		abs.Add(clearLabel);

		//clear tap function:
		TapGestureRecognizer clearTap = new();
		clearTap.Tapped += delegate {
			_ = clearRecording();
		};
		clearLabel.GestureRecognizers.Add(clearTap);

	}

	async Task startRecording() {

		if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted) {
			//popup to instruct how to add permissions
			Debug.WriteLine("NO MIC PERMISSIONS");
			return;
		}

		if (!audioRecorder.IsRecording) {
			await audioRecorder.StartAsync();
			absButton.BackgroundColor = Colors.DarkRed;
			Debug.WriteLine("STARTED RECORDING");
		}
	}
	async Task stopRecording() {
		if (audioRecorder.IsRecording) {
			absButton.BackgroundColor = Colors.DarkGoldenrod;
			Debug.WriteLine("STOP RECORDING");
			recordedAudio = await audioRecorder.StopAsync();

#if ANDROID
			var stream = recordedAudio.GetAudioStream();
			//string cacheFolder = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).AbsoluteFile.Path.ToString(); // gives app package in data structure
			string cacheFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsoluteFile.Path.ToString(); //gives general downloads folder
			cacheFolder = cacheFolder + System.IO.Path.DirectorySeparatorChar;

			string fileNameToSave = cacheFolder + "audiotemp.wav";
			if (stream != null) {

				Directory.CreateDirectory(Path.GetDirectoryName(fileNameToSave));
				if (System.IO.File.Exists(fileNameToSave)) {
					System.IO.File.Delete(fileNameToSave); //must delete first or length not working properly
				}
				FileStream fileStream = System.IO.File.Create(fileNameToSave);
				fileStream.Position = 0;
				stream.Position = 0;
				stream.CopyTo(fileStream);
				fileStream.Close();

				Debug.WriteLine("AUDIO FILE SAVED DONE: " + fileNameToSave);
			}
#endif
		}
	}

	async Task startPlaying() {
		if (recordedAudio != null) {
			absButton.BackgroundColor = Colors.DarkGreen;

			Debug.WriteLine("START PLAYBACK");

			var audioStream = recordedAudio.GetAudioStream();

			if (audioPlayer == null) { //should certainly be if cleared properly

				audioPlayer = audioManager.CreatePlayer(audioStream);
				audioPlayer.PlaybackEnded += delegate {
					absButton.BackgroundColor = Colors.DarkGoldenrod;
				};

			}
			if (!audioPlayer.IsPlaying) {
				audioPlayer.Play();
			}
		}
		else {
			Debug.WriteLine("TRIED TO START BUT NO AUDIO");
		}
	}
	async Task pausePlayback() {
		if (audioPlayer != null && audioPlayer.IsPlaying) {
			absButton.BackgroundColor = Colors.DarkGoldenrod;
			Debug.WriteLine("PAUSE PLAYBACK");
			audioPlayer.Pause();
		}
	}

	async Task clearRecording() {
		if (audioRecorder.IsRecording) {
			await stopRecording();
		}
		recordedAudio = null;
		audioPlayer?.Dispose();
		audioPlayer = null;
		absButton.BackgroundColor = Colors.AliceBlue;

	}
}