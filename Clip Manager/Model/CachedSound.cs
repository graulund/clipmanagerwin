using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Clip_Manager.Model
{
	public class CachedSound
	{
		const int CLIP_MAX_MINUTES = 7;
		const int DESIRED_SAMPLE_RATE = 44100;

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

				IWaveProvider source = audioFileReader;

				if (audioFileReader.WaveFormat.SampleRate != DESIRED_SAMPLE_RATE)
				{
					// Resample to desired sample rate
					source = new SampleToWaveProvider16(new WdlResamplingSampleProvider(audioFileReader, DESIRED_SAMPLE_RATE));
				}

				WaveFormat = source.WaveFormat;
				FileName = audioFileName;
				TotalTime = audioFileReader.TotalTime;
				var wholeFile = new List<byte>((int)(audioFileReader.Length / 4));
				var readBuffer = new byte[DESIRED_SAMPLE_RATE * source.WaveFormat.Channels];
				int samplesRead;

				while ((samplesRead = source.Read(readBuffer, 0, readBuffer.Length)) > 0)
				{
					wholeFile.AddRange(readBuffer.Take(samplesRead));
				}

				AudioData = wholeFile.ToArray();
			}
		}
	}
}
