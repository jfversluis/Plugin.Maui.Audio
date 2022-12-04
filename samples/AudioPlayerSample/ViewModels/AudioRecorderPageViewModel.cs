using Plugin.Maui.Audio;

namespace AudioPlayerSample.ViewModels;

public class AudioRecorderPageViewModel : BaseViewModel
{
   ContentPage page;

   readonly IAudioManager audioManager;
   IAudioRecorder? audioRecorder;
   IAudioPlayer audioPlayer;
   IAudioSource audioSource = null;

   string _RecordButtonText = "Record";
   public string RecordButtonText
   {
      get => _RecordButtonText;
      set
      {
         _RecordButtonText = value;
         NotifyPropertyChanged();
      }
   }

   Color _RecordButtonColor = Colors.Blue;
   public Color RecordButtonColor
   {
      get => _RecordButtonColor;
      set
      {
         _RecordButtonColor = value;
         NotifyPropertyChanged();
      }
   }

   double audioTime = 0;
   public double AudioTime
   {
      get => audioTime;
      set
      {
         audioTime = value;
         NotifyPropertyChanged();
      }
   }

   public AudioRecorderPageViewModel(IAudioManager audioManager)
   {
      StartCommand = new Command(Start);
      PlayCommand = new Command(playAudio);
      this.audioManager = audioManager;
   }

   public void setPage(ContentPage page)
   {
      try
      {
         this.page = page;
      }
      catch (Exception ex)
      {
         page.DisplayAlert("Alert", $"setPage Exception: {ex.Message}", "OK");
      }
   }

   void DonePlaying(object sender, EventArgs e)
   {
      AudioTime = audioPlayer.Duration;
   }
   
   public Command PlayCommand { get; set; }
   public async void playAudio()
   {
      if (audioSource != null)
      {
         audioPlayer = this.audioManager.CreatePlayer(((FileAudioSource)audioSource).GetAudioStream());
         audioPlayer.PlaybackEnded += DonePlaying;
         await Task.Run(() =>
               {
                  audioPlayer.Play();
               });
      }
   }

   public Command StartCommand { get; set; }
   async void Start()
   {
      // This must be done for Android to avoid an exception but I don't think it would hurt in any case
      if (await HavePermissionMicrophoneAsync())
      {

         if (RecordButtonText == "Record")
         {
            audioRecorder = audioManager.CreateRecorder();
            try
            {
               RecordButtonColor = Colors.Red;
               RecordButtonText = "Stop and play";
               await audioRecorder.StartAsync();
            }
            catch (Exception ex)
            {
               await page.DisplayAlert("Alert", $"Start() recording exception: {ex.Message}", "OK");
            }
         }
         else
         {
            audioSource = await audioRecorder.StopAsync();
            AudioTime = audioRecorder.Duration();
            RecordButtonColor = Colors.Blue;
            RecordButtonText = "Record";
            // playAudio();
         }
      }
      else
      {
         await page.DisplayAlert("Alert", $"It is necessary to go to the settings for this app and give permission for the microphone.", "OK");
      }
   }
}
