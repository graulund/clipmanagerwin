using NAudio.Wave;
using System;

namespace Clip_Manager.Model
{
	class CachedSoundSampleProvider : IWaveProvider
	{
		private readonly CachedSound cachedSound;
		private long position;

		public virtual TimeSpan CurrentTime
		{
			get
			{
				return TimeSpan.FromSeconds((double)position / WaveFormat.AverageBytesPerSecond);
			}
		}

		public CachedSoundSampleProvider(CachedSound cachedSound)
		{
			this.cachedSound = cachedSound;
		}

		public int Read(byte[] buffer, int offset, int count)
		{
			var availableSamples = cachedSound.AudioData.Length - position;
			var samplesToCopy = Math.Min(availableSamples, count);
			Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesToCopy);
			position += samplesToCopy;
			return (int)samplesToCopy;
		}

		public WaveFormat WaveFormat => cachedSound.WaveFormat;
	}
}
