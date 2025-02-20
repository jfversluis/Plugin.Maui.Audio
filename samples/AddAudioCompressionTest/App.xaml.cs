using Microsoft.Maui.Controls;
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
	Label statusLabel;


	//========================================
	//=== SET CODEC OR OTHER OPTIONS HERE
	//========================================
	string fileName = "audiotemp.m4a"; // saved to cache folder
	AudioRecordingOptions options = new() {
		Encoding = Encoding.Aac
	};
	//========================================
	//=== SET PATH TO SAVE FILES TO
	//========================================
#if ANDROID
	string cacheFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsoluteFile.Path.ToString(); //gives general documents folder
#elif IOS
	string cacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif WINDOWS
	string cacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#else
	string cacheFolder = "";
#endif
	//========================================


	// Page build
	public MyTestPage() {

		AbsoluteLayout abs = new();
		this.Content = abs;

		absButton = new();
		absButton.BackgroundColor = Colors.AliceBlue;
		abs.Add(absButton);

		statusLabel = new();
		statusLabel.Text = "STATUS: CLEARED";
		statusLabel.BackgroundColor = Colors.WhiteSmoke;
		statusLabel.HorizontalTextAlignment = TextAlignment.Center;
		statusLabel.FontSize = 30;
		abs.Add(statusLabel);

		this.SizeChanged += delegate {
			if (this.Width > 0) {
				absButton.WidthRequest = this.Width;
				absButton.HeightRequest = this.Height;
				statusLabel.WidthRequest = this.Width;
				statusLabel.TranslationY = this.Height * 0.5;
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
	//=========================
	// CLEAR RECORDING
	//=========================
	async Task clearRecording() {
		if (audioRecorder.IsRecording) {
			await stopRecording();
		}
		statusLabel.Text = "STATUS: CLEARED";
		recordedAudio = null;
		audioPlayer?.Dispose();
		audioPlayer = null;
		absButton.BackgroundColor = Colors.AliceBlue;

	}
	//=========================
	// START PLAYBACK
	//=========================
	async Task startPlaying() {
		if (recordedAudio != null) {
			absButton.BackgroundColor = Colors.DarkGreen;

			Debug.WriteLine("START PLAYBACK");
			statusLabel.Text = "STATUS: PLAYING";

			var audioStream = recordedAudio.GetAudioStream();

			if (audioPlayer == null) { //should certainly be if cleared properly

				audioPlayer = audioManager.CreatePlayer(audioStream);
				audioPlayer.PlaybackEnded += delegate {
					statusLabel.Text = "STATUS: PLAY FINISHED";
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
	//=========================
	// PAUSE PLAYBACK
	//=========================
	async Task pausePlayback() {
		if (audioPlayer != null && audioPlayer.IsPlaying) {
			absButton.BackgroundColor = Colors.DarkGoldenrod;
			statusLabel.Text = "STATUS: PAUSED";
			Debug.WriteLine("PAUSE PLAYBACK");
			audioPlayer.Pause();
		}
	}

	//=========================
	// START RECORDING
	//=========================
	async Task startRecording() {
		try {

			if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted) {
				//popup to instruct how to add permissions
				Debug.WriteLine("NO MIC PERMISSIONS");
				return;
			}

			if (!audioRecorder.IsRecording) {
				
				await audioRecorder.StartAsync(options);
				absButton.BackgroundColor = Colors.DarkRed;
				statusLabel.Text = "STATUS: RECORDING";
				Debug.WriteLine("STARTED RECORDING");
			}
		}
		catch (Exception ex) {
			//Windows gives me:
			//Exception thrown: 'System.InvalidOperationException' in WinRT.Runtime.dll
			//Exception thrown: 'System.InvalidOperationException' in System.Private.CoreLib.dll
			//EXCEPTION ON START RECORD: Operation is not valid due to the current state of the object.
			Debug.WriteLine("EXCEPTION ON START RECORD: " + ex.Message);
		}
	}

	//=========================
	// STOP RECORDING
	//=========================
	async Task stopRecording() {
		if (audioRecorder.IsRecording) {
			absButton.BackgroundColor = Colors.DarkGoldenrod;
			statusLabel.Text = "STATUS: STOPPED";
			Debug.WriteLine("STOP RECORDING");
			recordedAudio = await audioRecorder.StopAsync();

			var stream = recordedAudio.GetAudioStream();
			//string cacheFolder = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).AbsoluteFile.Path.ToString(); // gives app package in data structure
			Debug.WriteLine($"SAVE TO FOLDER: {cacheFolder}");

			//save to file name (change extension as needed for encoding)
			string fileNameToSave = Path.Combine(cacheFolder, fileName);
			
			if (stream != null) {

				Directory.CreateDirectory(Path.GetDirectoryName(fileNameToSave));
				if (File.Exists(fileNameToSave)) {
					File.Delete(fileNameToSave); //must delete first or length not working properly
				}
				FileStream fileStream = File.Create(fileNameToSave);
				fileStream.Position = 0;
				stream.Position = 0;
				stream.CopyTo(fileStream);
				fileStream.Close();

				Debug.WriteLine("AUDIO FILE SAVED DONE: " + fileNameToSave);
			}
		}
	}

	


}