using System;
using System.Collections.Generic;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Clip_Manager.Model
{
	class ClipManagerEngine : IDisposable
	{
		private readonly IWavePlayer outputDevice;

		public const int NUM_CLIPS = 8;
		public Dictionary<int, CachedSound> Clips { get; set; }
		public int? CurrentlyPlayingIndex = null;
		public CachedSound CurrentlyPlayingClip = null;
		public CachedSoundSampleProvider CurrentlyPlayingSampleProvider = null;
		private int? startIndexAfterStopping = null;

		public event EventHandler ClipsChanged;
		public event EventHandler<ClipEventArgs> ClipStartedPlaying;
		public event EventHandler<ClipEventArgs> ClipStoppedPlaying;

		public ClipManagerEngine()
		{
			outputDevice = new WaveOutEvent();
			outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
		}

		public void SetClip(int index, CachedSound clip)
		{
			if (Clips == null)
			{
				Clips = new Dictionary<int, CachedSound>();
			}

			Clips.Add(index, clip);
			OnClipsChanged();
		}

		public void PlaySound(int index)
		{
			if (Clips.ContainsKey(index))
			{
				if (CurrentlyPlayingIndex != null)
				{
					Console.WriteLine("Stopping current clip");
					startIndexAfterStopping = index;
					Stop();
					return;
				}

				var clip = Clips[index];
				var sampleProvider = new CachedSoundSampleProvider(clip);
				outputDevice.Init(sampleProvider);
				outputDevice.Play();
				Console.WriteLine("Setting currently playing to be index {0}", index);
				if (outputDevice.PlaybackState == PlaybackState.Playing)
				{
					CurrentlyPlayingIndex = index;
					CurrentlyPlayingClip = clip;
					CurrentlyPlayingSampleProvider = sampleProvider;
					OnClipStartedPlaying(index);
				}
			}

			else
			{
				Console.WriteLine("Tried to play index {0}, but no such clip exists", index);
			}
		}

		public void ToggleSound(int index)
		{
			Console.WriteLine("Toggle: {0}", CurrentlyPlayingIndex);
			if (CurrentlyPlayingIndex != null && CurrentlyPlayingIndex.Value == index)
			{
				Console.WriteLine("Toggle: Already playing, stopping");
				Stop();
				return;
			}

			Console.WriteLine("Toggle: Starting");
			PlaySound(index);
		}

		public void Stop()
		{
			Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ": Stopping");
			outputDevice?.Stop();
		}

		private void OutputDevice_PlaybackStopped(object sender, StoppedEventArgs e)
		{
			Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ": Stopped");
			var playedIndex = CurrentlyPlayingIndex;

			CurrentlyPlayingIndex = null;
			CurrentlyPlayingClip = null;

			if (playedIndex != null)
			{
				OnClipStoppedPlaying(playedIndex.Value);
			}

			if (startIndexAfterStopping != null)
			{
				var index = startIndexAfterStopping.Value;
				startIndexAfterStopping = null;
				PlaySound(index);
			}
		}

		protected virtual void OnClipsChanged()
		{
			ClipsChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnClipStartedPlaying(int index)
		{
			var args = new ClipEventArgs { Index = index };
			ClipStartedPlaying?.Invoke(this, args);
		}

		protected virtual void OnClipStoppedPlaying(int index)
		{
			var args = new ClipEventArgs { Index = index };
			ClipStoppedPlaying?.Invoke(this, args);
		}

		public void Dispose()
		{
			outputDevice.Dispose();
		}
	}

	public class ClipEventArgs : EventArgs
	{
		public int Index { get; set; }
	}
}