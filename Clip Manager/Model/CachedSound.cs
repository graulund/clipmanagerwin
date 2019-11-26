using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace Clip_Manager.Model
{
	public class CachedSound
	{
		const int CLIP_MAX_MINUTES = 7;

		public byte[] AudioData { get; }
		public WaveFormat WaveFormat { get; }
		public string FileName { get; }
		public TimeSpan TotalTime { get; }

		public CachedSound(string audioFileName)
		{
			using (var audioFileReader = new AudioFileReader(audioFileName))
			{
				if (audioFileReader.TotalTime > new TimeSpan(0, CLIP_MAX_MINUTES, 0)) {
					throw new OutOfMemoryException("Audio file too long");
				}

				WaveFormat = audioFileReader.WaveFormat;
				FileName = audioFileName;
				TotalTime = audioFileReader.TotalTime;
				var wholeFile = new List<byte>((int)(audioFileReader.Length / 4));
				var readBuffer = new byte[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
				int samplesRead;

				while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
				{
					wholeFile.AddRange(readBuffer.Take(samplesRead));
				}

				AudioData = wholeFile.ToArray();
			}
		}
	}
}
