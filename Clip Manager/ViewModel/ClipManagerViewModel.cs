using Clip_Manager.Model;
using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Timers;

namespace Clip_Manager.ViewModel
{
	public class ClipManagerViewModel : ViewModelBase, IDisposable
	{
		public const int MAX_CLIPS = 8;
		public const double WARNING_SECONDS = 10.0;
		public ICommand ToggleClipCommand { get; }

		public bool IsDirty { get; set; }

		private ClipManagerEngine engine;
		readonly CachedSound clip = new CachedSound("C:\\Users\\augr\\Desktop\\alting-ved-det-er-cool.wav");
		readonly CachedSound clip2 = new CachedSound("C:\\Users\\augr\\Desktop\\esport-dreng-kort.mp3");
		readonly CachedSound clip3 = new CachedSound("C:\\Users\\augr\\Desktop\\creme-sprøjtes-ud-start-slut.wav");
		readonly CachedSound clip4 = new CachedSound("C:\\Users\\augr\\Desktop\\go-morgen-clip.mp3");

		private Timer timer;

		private List<ClipViewModel> _clips;
		public List<ClipViewModel> Clips
		{
			get => _clips;
			set => SetProperty(ref _clips, value);
		}

		public List<ClipViewModel> UpperClips
		{
			get => Clips.Skip(MAX_CLIPS / 2).Take(MAX_CLIPS / 2).ToList();
		}

		public List<ClipViewModel> LowerClips
		{
			get => Clips.Take(MAX_CLIPS / 2).ToList();
		}

		public ClipManagerViewModel()
		{
			ToggleClipCommand = new DelegateCommand(ToggleClip);
			IsDirty = false;
			timer = new Timer(20);
			timer.Elapsed += Timer_Elapsed;
			engine = new ClipManagerEngine();
			engine.ClipsChanged += Engine_ClipsChanged;
			engine.ClipStartedPlaying += Engine_ClipStartedPlaying;
			engine.ClipStoppedPlaying += Engine_ClipStoppedPlaying;
			engine.SetClip(0, clip);
			engine.SetClip(1, clip2);
			engine.SetClip(2, clip3);
			engine.SetClip(3, clip4);
		}

		public ClipViewModel TransformClip(ClipViewModel existingClip, CachedSound clip, int index)
		{
			var isPlaying = engine.CurrentlyPlayingIndex != null && engine.CurrentlyPlayingIndex.Value == index;

			if (existingClip != null) {
				existingClip.Number = 1 + index;
				existingClip.HasValue = clip != null;
				existingClip.FileName = clip != null ? Path.GetFileName(clip.FileName) : null;
				existingClip.IsPlaying = isPlaying;
				existingClip.IsNotPlaying = !isPlaying;

				if (!isPlaying)
				{
					existingClip.TimeString = GetClipDurationString(index);
				}

				return existingClip;
			}
			else
			{
				return new ClipViewModel
				{
					Number = 1 + index,
					HasValue = clip != null,
					FileName = clip != null ? Path.GetFileName(clip.FileName) : null,
					IsPlaying = isPlaying,
					IsNotPlaying = !isPlaying,
					TimeString = GetClipDurationString(index)
				};
			}
		}

		public void TransformClips()
		{
			var clips = Clips != null ? Clips : new List<ClipViewModel>();

			for (var i = 0; i < MAX_CLIPS; i++)
			{
				if (clips.Count > i + 1)
				{
					clips[i] = TransformClip(clips[i], engine.Clips.ContainsKey(i) ? engine.Clips[i] : null, i);
				}

				else
				{
					clips.Add(TransformClip(null, engine.Clips.ContainsKey(i) ? engine.Clips[i] : null, i));
				}
			}

			Clips = clips;
		}

		public void SetClip(int number, string fileName)
		{
			engine.SetClip(number - 1, new CachedSound(fileName));
		}

		public void ToggleClip(object data)
		{
			try
			{
				var index = Convert.ToInt32(data);
				engine.ToggleSound(index);
			}

			catch (Exception) { }
		}

		public string GetClipDurationString(int index)
		{
			var duration = TimeSpan.Zero;

			if (engine.Clips.ContainsKey(index))
			{
				var clip = engine.Clips[index];
				if (clip != null)
				{
					duration = GetRoundedTimeSpan(clip.TotalTime);
				}
			}

			if (duration != TimeSpan.Zero)
			{
				return duration.ToString("m\\:ss");
			}

			return "";
		}

		private TimeSpan GetRoundedTimeSpan(TimeSpan timeSpan)
		{
			return new RoundedTimeSpan(timeSpan.Ticks, 0).TimeSpan;
		}

		private void UpdatePlayingClipView()
		{
			var currentTime = engine.CurrentlyPlayingSampleProvider?.CurrentTime;
			var duration = engine.CurrentlyPlayingClip?.TotalTime;

			if (currentTime != null && duration != null && engine.CurrentlyPlayingIndex != null)
			{
				var rest = GetRoundedTimeSpan(duration.Value - currentTime.Value);
				var warning = rest.TotalSeconds <= WARNING_SECONDS;
				var ratio = currentTime.Value.TotalMilliseconds / duration.Value.TotalMilliseconds;
				var index = engine.CurrentlyPlayingIndex.Value;
				var restString = rest.ToString("\\-m\\:ss");

				var clipView = Clips[index];

				clipView.IsPlaying = true;
				clipView.IsNotPlaying = false;
				clipView.TimeString = restString;
				clipView.PlayRatio = ratio;
				clipView.IsWarning = warning;

				/*Console.WriteLine(
					DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") +
					": Timer elapsed. Current index: {0}, Remaining: {1}, Ratio: {2}, Warning: {3}",
					index,
					restString,
					ratio,
					warning
				);*/
			}
		}

		private void Engine_ClipsChanged(object sender, EventArgs e)
		{
			TransformClips();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			UpdatePlayingClipView();
		}

		private void Engine_ClipStartedPlaying(object sender, ClipEventArgs e)
		{
			Console.WriteLine(
				DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") +
				": Clip started playing: {0}",
				e.Index
			);
			timer.Start();
			UpdatePlayingClipView();
		}

		private void Engine_ClipStoppedPlaying(object sender, ClipEventArgs e)
		{
			var index = e.Index;
			Console.WriteLine("Clip stopped playing: {0}", index);
			timer.Stop();
			var clipView = Clips[index];
			clipView.IsPlaying = false;
			clipView.IsNotPlaying = true;
			clipView.IsWarning = false;
			clipView.PlayRatio = 0.0;
			clipView.TimeString = GetClipDurationString(index);
		}

		public void Dispose()
		{
			engine?.Dispose();
			engine = null;
		}
	}
}
