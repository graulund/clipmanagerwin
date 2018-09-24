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

		private ClipManagerEngine engine;
		readonly CachedSound clip = new CachedSound("C:\\Users\\augr\\Desktop\\alting-ved-det-er-cool.wav");
		readonly CachedSound clip2 = new CachedSound("C:\\Users\\augr\\Desktop\\esport-dreng-kort.mp3");

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
			timer = new Timer(100);
			timer.Elapsed += Timer_Elapsed;
			engine = new ClipManagerEngine();
			engine.ClipsChanged += Engine_ClipsChanged;
			engine.ClipStartedPlaying += Engine_ClipStartedPlaying;
			engine.ClipStoppedPlaying += Engine_ClipStoppedPlaying;
			engine.SetClip(0, clip);
			engine.SetClip(1, clip2);
		}

		public ClipViewModel TransformClip(CachedSound clip, int index)
		{
			var duration = clip != null ? clip.TotalTime : TimeSpan.Zero;
			return new ClipViewModel
			{
				Number = 1 + index,
				HasValue = clip != null,
				FileName = clip != null ? Path.GetFileName(clip.FileName) : null,
				IsPlaying = engine.CurrentlyPlayingIndex != null && engine.CurrentlyPlayingIndex.Value == index,
				TimeString = duration.ToString("m\\:ss")
			};
		}

		public void TransformClips()
		{
			var clips = new List<ClipViewModel>();

			for (var i = 0; i < MAX_CLIPS; i++)
			{
				clips.Add(TransformClip(engine.Clips.ContainsKey(i) ? engine.Clips[i] : null, i));
			}

			Clips = clips;
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

		private void Engine_ClipsChanged(object sender, EventArgs e)
		{
			TransformClips();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			var currentTime = engine.CurrentlyPlayingSampleProvider?.CurrentTime;
			var duration = engine.CurrentlyPlayingClip?.TotalTime;

			if (currentTime != null && duration != null && engine.CurrentlyPlayingIndex != null)
			{
				var rest = currentTime.Value - duration.Value;
				var warning = rest.TotalSeconds <= WARNING_SECONDS;
				var ratio = currentTime.Value.TotalMilliseconds / duration.Value.TotalMilliseconds;
				var index = engine.CurrentlyPlayingIndex.Value;
				var restString = rest.ToString("\\-m\\:ss");

				var clipView = Clips[index];

				clipView.IsPlaying = true;
				clipView.TimeString = restString;
				clipView.PlayRatio = ratio;
				clipView.IsWarning = warning;

				Console.WriteLine(
					DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") +
					": Timer elapsed. Current index: {0}, Remaining: {1}, Ratio: {2}, Warning: {3}",
					index,
					restString,
					ratio,
					warning
				);
			}
		}

		private void Engine_ClipStartedPlaying(object sender, ClipEventArgs e)
		{
			Console.WriteLine("Clip started playing: {0}", e.Index);
			timer.Start();
		}

		private void Engine_ClipStoppedPlaying(object sender, ClipEventArgs e)
		{
			Console.WriteLine("Clip stopped playing: {0}", e.Index);
			timer.Stop();
		}

		public void Dispose()
		{
			engine?.Dispose();
			engine = null;
		}
	}
}
