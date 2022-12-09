using AVFoundation;
using Foundation;

namespace Plugin.Maui.Audio;

partial class AudioPlayer : IAudioPlayer
{
   readonly AVAudioPlayer player;
   bool isDisposed;

   public double CurrentPosition => player.CurrentTime;

   IDispatcherTimer? myTimer;
   DateTime startTime;
   public TimeSpan Ts = TimeSpan.Zero;

   public double Duration {
      get {
         double duration = Ts.TotalMilliseconds/1000;
         if (duration == 0)
         {
            duration = player.Duration / 1000.0;
         }
         return duration;
      }
   }

   void t_Tick(object? sender, EventArgs e)
   {
      Ts = DateTime.Now - startTime;
   }

   public double Volume
   {
      get => player.Volume;
      set => player.Volume = (float)Math.Clamp(value, 0, 1);
   }

   public double Balance
   {
      get => player.Pan;
      set => player.Pan = (float)Math.Clamp(value, -1, 1);
   }

   public bool IsPlaying => player.Playing;

   public bool Loop
   {
      get => player.NumberOfLoops != 0;
      set => player.NumberOfLoops = value ? -1 : 0;
   }

   public bool CanSeek => true;

   internal AudioPlayer(Stream audioStream)
   {
      var data = NSData.FromStream(audioStream)
         ?? throw new FailedToLoadAudioException("Unable to convert audioStream to NSData.");
      player = AVAudioPlayer.FromData(data)
         ?? throw new FailedToLoadAudioException("Unable to create AVAudioPlayer from data.");

      PreparePlayer();
   }

   internal AudioPlayer(string fileName)
   {
      player = AVAudioPlayer.FromUrl(NSUrl.FromFilename(fileName))
         ?? throw new FailedToLoadAudioException("Unable to create AVAudioPlayer from url.");

      PreparePlayer();
   }

   protected virtual void Dispose(bool disposing)
   {
      if (isDisposed)
      {
         return;
      }

      if (disposing)
      {
         Stop();

         player.FinishedPlaying -= OnPlayerFinishedPlaying;
         player.Dispose();
      }

      isDisposed = true;
   }

   public void Pause() => player.Pause();

   public void Play()
   {
      if (player.Playing)
      {
         player.CurrentTime = 0;
      }
      else
      {
         myTimer = Application.Current?.Dispatcher.CreateTimer();
         if (myTimer != null)
         {
            myTimer.Interval = TimeSpan.FromMilliseconds(100);
            myTimer.Tick += t_Tick;
            startTime = DateTime.Now;
            myTimer.Start();
         }
         player.Play();
      }
   }

    public void Seek(double position) => player.CurrentTime = position;

   public void Stop()
   {
      myTimer?.Stop();

      player.Stop();
      Seek(0);
      PlaybackEnded?.Invoke(this, EventArgs.Empty);
   }

   bool PreparePlayer()
   {
      player.FinishedPlaying += OnPlayerFinishedPlaying;
      player.PrepareToPlay();

      return true;
   }

   void OnPlayerFinishedPlaying(object? sender, AVStatusEventArgs e)
   {
      myTimer?.Stop();

      PlaybackEnded?.Invoke(this, e);
   }
}
