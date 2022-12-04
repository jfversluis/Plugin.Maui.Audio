using Windows.Media.Core;
using Windows.Media.Playback;

namespace Plugin.Maui.Audio;

partial class AudioPlayer : IAudioPlayer
{
   bool isDisposed = false;
   readonly MediaPlayer player;

   public double CurrentPosition => player.PlaybackSession.Position.TotalSeconds;

   IDispatcherTimer myTimer = null;
   DateTime startTime;
   public TimeSpan ts = TimeSpan.Zero;

   public double Duration {
      get {
         double duration = ts.TotalMilliseconds/1000;
         if (duration == 0)
            duration = player.PlaybackSession.NaturalDuration.TotalSeconds;
         return duration;
      }
   }

   void t_Tick(object sender, EventArgs e)
   {
      ts = DateTime.Now - startTime;
   }

   public double Volume
   {
      get => player.Volume;
      set => SetVolume(value, Balance);
   }

   public double Balance
   {
      get => player.AudioBalance;
      set => SetVolume(Volume, value);
   }

   public bool IsPlaying =>
           player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing; //might need to expand

   public bool Loop
   {
      get => player.IsLoopingEnabled;
      set => player.IsLoopingEnabled = value;
   }

   public bool CanSeek => player.PlaybackSession.CanSeek;

   public AudioPlayer(Stream audioStream)
   {
      player = CreatePlayer();

      if (player is null)
      {
         throw new FailedToLoadAudioException($"Failed to create {nameof(MediaPlayer)} instance. Reason unknown.");
      }

      player.Source = MediaSource.CreateFromStream(audioStream?.AsRandomAccessStream(), string.Empty);
      player.MediaEnded += OnPlaybackEnded;
   }

   public AudioPlayer(string fileName)
   {
      player = CreatePlayer();

      if (player is null)
      {
         throw new FailedToLoadAudioException($"Failed to create {nameof(MediaPlayer)} instance. Reason unknown.");
      }

      player.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/" + fileName));
      player.MediaEnded += OnPlaybackEnded;
   }

   void OnPlaybackEnded(MediaPlayer sender, object args)
   {
      myTimer?.Stop();

      PlaybackEnded?.Invoke(sender, EventArgs.Empty);
   }

   public void Play()
   {
      if (player.Source is null)
      {
         return;
      }

      if (player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
      {
         Pause();
         Seek(0);
      }

      myTimer = Microsoft.Maui.Controls.Application.Current.Dispatcher.CreateTimer();
      myTimer.Interval = TimeSpan.FromMilliseconds(100);
      myTimer.Tick += t_Tick;
      startTime = DateTime.Now;
      myTimer.Start();

      player.Play();
   }

   public void Pause()
   {
      player.Pause();
   }

   public void Stop()
   {
      myTimer?.Stop();

      Pause();
      Seek(0);
      PlaybackEnded?.Invoke(this, EventArgs.Empty);
   }

   public void Seek(double position)
   {
      if (player.PlaybackSession is null)
      {
         return;
      }

      if (player.PlaybackSession.CanSeek)
      {
         player.PlaybackSession.Position = TimeSpan.FromSeconds(position);
      }
   }

   void SetVolume(double volume, double balance)
   {
      if (isDisposed)
      {
         return;
      }

      player.Volume = Math.Clamp(volume, 0, 1);
      player.AudioBalance = Math.Clamp(balance, -1, 1);
   }

   MediaPlayer CreatePlayer()
   {
      return new MediaPlayer() { AutoPlay = false, IsLoopingEnabled = false };
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

         player.MediaEnded -= OnPlaybackEnded;
         player.Dispose();
      }

      isDisposed = true;
   }
}
