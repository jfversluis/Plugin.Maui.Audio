using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plugin.Maui.Audio;

/// <summary>
/// Represents an audio mixer managing multiple audio channels and handling spatial audio calculations.
/// </summary>
public class AudioMixer : IDisposable
{
	/// <summary>
	/// Gets the total number of channels managed by the AudioMixer.
	/// </summary>
	public int ChannelCount { get; }

	/// <summary>
	/// Gets a read-only list of audio channels.
	/// </summary>
	public IReadOnlyList<IAudioPlayer> Channels => channels.AsReadOnly();

	readonly List<IAudioPlayer> channels;
	readonly IAudioManager audioManager;
	readonly float fastDecay;
	readonly float slowDecay;
	readonly float clipDist;
	readonly float closeDist;
	readonly float attenuator;
	float mapBalance;

	/// <summary>
	/// Gets the fast decay factor.
	/// </summary>
	public float FastDecay => fastDecay;

	/// <summary>
	/// Gets the slow decay factor.
	/// </summary>
	public float SlowDecay => slowDecay;

	/// <summary>
	/// Initializes a new instance of the <see cref="AudioMixer"/> class with the specified number of channels.
	/// </summary>
	/// <param name="audioManager">The audio manager to create audio players.</param>
	/// <param name="numberOfChannels">The number of audio channels to manage.</param>
	public AudioMixer(IAudioManager audioManager, int numberOfChannels)
	{
		if (audioManager is null)
		{
			throw new ArgumentNullException(nameof(audioManager));
		}

		if (numberOfChannels <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(numberOfChannels), "Number of channels must be positive.");
		}

		this.audioManager = audioManager;
		ChannelCount = numberOfChannels;
		channels = new List<IAudioPlayer>(numberOfChannels);

		// Initialize spatial audio variables
		fastDecay = (float)Math.Pow(0.5, 1.0 / (35.0 / 5.0)); // ≈ 0.9037
		slowDecay = (float)Math.Pow(0.5, 1.0 / 35.0);         // ≈ 0.9802

		clipDist = 1200f;
		closeDist = 160f;
		attenuator = clipDist - closeDist;

		for (int i = 0; i < numberOfChannels; i++)
		{
			var player = this.audioManager.CreatePlayer();
			channels.Add(player);
		}
	}

	/// <summary>
	/// Play an already prepared channel.
	/// </summary>
	/// <param name="channelIndex">The index of the channel to play the sound on.</param>
	/// <param name="loop">Indicates whether the audio should loop.</param>
	public void Play(int channelIndex, bool loop = false)
	{
		ValidateChannelIndex(channelIndex);
		var player = channels[channelIndex];
		player.Loop = loop;
		player.Play();
	}
 
	/// <summary>
	/// Plays the specified audio clip on the given channel.
	/// </summary>
	/// <param name="channelIndex">The index of the channel to play the sound on.</param>
	/// <param name="audioClip">The audio clip to play.</param>
	/// <param name="loop">Indicates whether the audio should loop.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the channel index is invalid.</exception>
	/// <exception cref="ArgumentNullException">Thrown if the audio clip is null.</exception>
	public void Play(int channelIndex, IAudioSource audioClip, bool loop = false)
	{
		ValidateChannelIndex(channelIndex);
		if (audioClip is null)
		{
			throw new ArgumentNullException(nameof(audioClip));
		}

		var player = channels[channelIndex];
		player.Stop(); 
		player.SetSource(audioClip.GetAudioStream());
		player.Loop = loop;
		player.Play();
	}

	/// <summary>
	/// Stops playback on the specified channel.
	/// </summary>
	/// <param name="channelIndex">The index of the channel to stop.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the channel index is invalid.</exception>
	public void Stop(int channelIndex)
	{
		ValidateChannelIndex(channelIndex);
		var player = channels[channelIndex];
		player.Stop();
	}

	/// <summary>
	/// Pauses playback on all channels.
	/// </summary>
	public void StopAll()
	{
		foreach (var player in channels)
		{
			player.Stop();
		}
	}

	/// <summary>
	/// Pauses playback on all channels.
	/// </summary>
	public void PauseAll()
	{
		foreach (var player in channels)
		{
			if (player.IsPlaying)
			{
				player.Pause();
			}
		}
	}

	/// <summary>
	/// Resumes playback on all channels from unchanged positions.
	/// </summary>
	public void ResumeAll()
	{
		foreach (var player in channels)
		{
			if (!player.IsPlaying)
			{
				player.Play();
			}
		}
	}

	/// <summary>
	/// Starts  playback on all channels from zero.
	/// </summary>
	public void PlayAll()
	{
		foreach (var player in channels)
		{
			player.Seek(0);
			player.Play();
		}
	}

	/// <summary>
	/// Sets the audio source for the specified channel without playing it.
	/// </summary>
	/// <param name="channelIndex">The index of the channel.</param>
	/// <param name="audioClip">The audio clip to set as the source.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the channel index is invalid.</exception>
	/// <exception cref="ArgumentNullException">Thrown if the audio clip is null.</exception>
	public void SetSource(int channelIndex, IAudioSource audioClip)
	{
		ValidateChannelIndex(channelIndex);
		if (audioClip is null)
		{
			throw new ArgumentNullException(nameof(audioClip));
		}

		var player = channels[channelIndex];
		player.Stop(); 
		player.SetSource(audioClip.GetAudioStream());
	}

	/// <summary>
	/// Gets the audio player for the specified channel.
	/// </summary>
	/// <param name="channelIndex">The index of the channel to retrieve.</param>
	/// <returns>The audio player associated with the specified channel.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the channel index is invalid.</exception>
	public IAudioPlayer GetChannel(int channelIndex)
	{
		ValidateChannelIndex(channelIndex);
		var player = channels[channelIndex];
		return player;
	}

	/// <summary>
	/// Sets the stereo balance for the specified channel.
	/// </summary>
	/// <param name="channelIndex">The index of the channel.</param>
	/// <param name="balance">The balance value ranging from -1 (full left) to +1 (full right).</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the channel index is invalid.</exception>
	public void SetBalance(int channelIndex, float balance)
	{
		balance *= mapBalance;
		ValidateChannelIndex(channelIndex);
		balance = Math.Clamp(balance, -1f, 1f);
		var player = channels[channelIndex];
		player.Balance = balance;
	}

	/// <summary>
	/// Changes the stereo balance for all channels, might need when device is rotated. 1 means not change. Range -1 to +1.
	/// </summary>
	/// <param name="value"></param>
	public void MapBalance(float value)
	{
		mapBalance = value;
	}

	/// <summary>
	/// Sets the volume for the specified channel.
	/// </summary>
	/// <param name="channelIndex">The index of the channel.</param>
	/// <param name="volume">The volume level (typically between 0.0 and 1.0).</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the channel index is invalid.</exception>
	public void SetVolume(int channelIndex, float volume)
	{
		ValidateChannelIndex(channelIndex);
		volume = Math.Clamp(volume, 0f, 1f);
		var player = channels[channelIndex];
		player.Volume = volume;
	}

	/// <summary>
	/// Validates that the provided channel index is within the valid range.
	/// </summary>
	/// <param name="channelIndex">The channel index to validate.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the channel index is out of range.</exception>
	void ValidateChannelIndex(int channelIndex)
	{
		if (channelIndex < 0 || channelIndex >= ChannelCount)
		{
			throw new ArgumentOutOfRangeException(nameof(channelIndex), $"Channel index must be between 0 and {ChannelCount - 1}.");
		}
	}

	/// <summary>
	/// Calculates the adjusted Balance and Volume based on the 3D position relative to the listener.
	/// </summary>
	/// <param name="position">The 3D position vector of the sound source.</param>
	/// <param name="baseVolume">The base volume before spatial adjustments.</param>
	/// <param name="baseBalance">The base balance before spatial adjustments.</param>
	/// <returns>A tuple containing the adjusted Balance and Volume.</returns>
	public (float Balance, float Volume) PositionInSpace(Vector3 position, float baseVolume, float baseBalance)
	{
		Vector3 forward = new Vector3(0, 0, -1);

		if (Vector3.Distance(position, forward) < 0.001f)
		{
			return (baseBalance, baseVolume);
		}

		float x = position.X;
		float z = position.Z;
		float angle = MathF.Atan2(x, z);
		float balance = MathF.Sin(angle);
		float distance = MathF.Sqrt(x * x + z * z);
		float attenuation = GetDistanceDecay(distance);
		float volume = baseVolume * attenuation;
		float panningEffect = Math.Clamp(1f - (distance / clipDist), 0f, 1f);

		balance *= panningEffect;
		balance = Math.Clamp(balance, -1f, 1f);

		return (balance, volume);
	}

	/// <summary>
	/// Calculates distance-based attenuation.
	/// </summary>
	/// <param name="dist">Distance from the listener.</param>
	/// <returns>Attenuation factor.</returns>
	public float GetDistanceDecay(float dist)
	{
		if (dist < closeDist)
		{
			return 1f;
		}
		else
		{
			return Math.Max((clipDist - dist) / attenuator, 0f);
		}
	}

	/// <summary>
	/// Gets a value indicating whether this instance has been disposed.
	/// </summary>
	public bool IsDisposed { get; protected set; }

	/// <summary>
	/// Releases the unmanaged resources used by the <see cref="AudioMixer"/> and optionally releases the managed resources.
	/// </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (IsDisposed)
		{
			return;
		}

		foreach (var player in channels)
		{
			try
			{
				player.Stop();
				player.Dispose();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}

		channels.Clear();
		IsDisposed = true;
	}

	/// <summary>
	/// Disposes all managed audio players.
	/// </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}

