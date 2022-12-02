using Plugin.Maui.Audio;

namespace AudioPlayerSample.ViewModels;

public class AudioRecorderPageViewModel : BaseViewModel
{
   ContentPage page;

   readonly IAudioManager audioManager;
   IAudioRecorder? audioRecorder;

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

   public AudioRecorderPageViewModel(IAudioManager audioManager)
   {
      StartCommand = new Command(Start);
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
            var audioSource = await audioRecorder.StopAsync();
            RecordButtonColor = Colors.Blue;
            RecordButtonText = "Record";
            await Task.Run(() => this.audioManager.CreatePlayer(((FileAudioSource)audioSource).GetAudioStream()).Play());
         }
      }
      else
      {
         await page.DisplayAlert("Alert", $"It is necessary to go to the settings for this app and give permission for the microphone.", "OK");
      }
   }
}
