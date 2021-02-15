using Clip_Manager.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Clip_Manager.ViewModel
{
	public class ClipManagerViewModel : ViewModelBase, IDisposable
	{
		public const int MAX_CLIPS = 8;
		public const int NUM_RECENT_MENU_ITEMS = 10;
		public const int RECENT_ITEM_MAX_LENGTH = 50;
		public const double WARNING_SECONDS = 10.0;
		public ICommand ToggleClipCommand { get; }

		private ClipManagerEngine engine;

		public Separator RecentMenuSeparator { get; set; }
		public List<MenuItem> RecentMenuItems { get; set; }

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

		private string _mainWindowTitle;
		public string MainWindowTitle
		{
			get => _mainWindowTitle;
			set => SetProperty(ref _mainWindowTitle, value);
		}

		public string FileName { get => engine.ClipListFileName; }
		public bool IsDirty { get => engine.ClipListIsDirty; }
		public string OutputDeviceProductGuid { get => engine.OutputDeviceProductGuid; }
		public int OutputDeviceChannelOffset { get => engine.OutputDeviceChannelOffset; }

		public ClipManagerViewModel()
		{
			ToggleClipCommand = new DelegateCommand(ToggleClip);
			RecentMenuItems = new List<MenuItem>(NUM_RECENT_MENU_ITEMS);
			timer = new Timer(20);
			timer.Elapsed += Timer_Elapsed;
			engine = new ClipManagerEngine();
			TransformClips();
			SetMainWindowTitle();
			engine.ClipsChanged += Engine_ClipsChanged;
			engine.ClipStartedPlaying += Engine_ClipStartedPlaying;
			engine.ClipStoppedPlaying += Engine_ClipStoppedPlaying;
			engine.ClipListChanged += Engine_ClipListChanged;
			engine.RecentlyUsedsChanged += Engine_RecentlyUsedsChanged;
		}

		public ClipViewModel TransformClip(ClipViewModel existingClipVM, CachedSound clip, int index)
		{
			var isPlaying = engine.CurrentlyPlayingIndex != null && engine.CurrentlyPlayingIndex.Value == index;

			if (existingClipVM != null)
			{
				existingClipVM.Number = 1 + index;
				existingClipVM.HasValue = clip != null;
				existingClipVM.FileName = clip != null ? Path.GetFileName(clip.FileName) : null;
				existingClipVM.IsPlaying = isPlaying;
				existingClipVM.IsNotPlaying = !isPlaying;

				if (!isPlaying)
				{
					existingClipVM.TimeString = GetClipDurationString(index);
				}

				return existingClipVM;
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
				if (clips.Count > i)
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

		public void SetMainWindowTitle()
		{
			if (engine.ClipListFileName != null)
			{
				MainWindowTitle = string.Format(
					"{0}{1} — Clips",
					Path.GetFileName(engine.ClipListFileName),
					engine.ClipListIsDirty ? "*" : ""
				);
			}

			else
			{
				MainWindowTitle = "Clips";
			}
		}

		public void SetMostRecentUsedItems()
		{
			try
			{
				RecentMenuSeparator.Visibility =
					engine.RecentlyUsedListFileNames.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

				for (var i = 0; i < NUM_RECENT_MENU_ITEMS; i++)
				{
					if (RecentMenuItems.Count > i)
					{
						var menuItem = RecentMenuItems[i];
						if (engine.RecentlyUsedListFileNames.Count > i)
						{
							var fileName = engine.RecentlyUsedListFileNames[i];
							var trimmedFileName = fileName.Substring(
								Math.Max(0, fileName.Length - RECENT_ITEM_MAX_LENGTH),
								Math.Min(RECENT_ITEM_MAX_LENGTH, fileName.Length)
							);

							if (trimmedFileName.Length != fileName.Length)
							{
								trimmedFileName = string.Format("…{0}", trimmedFileName);
							}

							menuItem.Header = trimmedFileName;
							menuItem.Visibility = Visibility.Visible;
						}

						else
						{
							menuItem.Visibility = Visibility.Collapsed;
						}
					}
				}
			}
			catch (Exception)
			{
				// Nothing. This is not critical functionality.
			}
		}

		public void SetClip(int number, string fileName)
		{
			engine.SetClip(number - 1, fileName);
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

		public void LoadClipsFromFile(string fileName)
		{
			using (new WaitCursor())
			{
				engine.LoadClipsFromFile(fileName);
			}
		}

		public void SaveClipsToFile(string fileName)
		{
			using (new WaitCursor())
			{
				engine.SaveClipsToFile(fileName);
			}
		}

		public void LoadClipsFromDirectory(string directoryName)
		{
			using (new WaitCursor())
			{
				engine.LoadClipsFromDirectory(directoryName);
			}
		}

		public void ClearClips()
		{
			engine.ClearClips();
		}

		public void LoadRecentlyUsedListFile(int index)
		{
			using (new WaitCursor())
			{
				engine.LoadRecentlyUsedListFile(index);
			}
		}

		public void SaveOutputDeviceProductGuidSetting(string productGuid)
		{
			engine.SaveOutputDeviceProductGuidSetting(productGuid);
		}

		public void SaveOutputDeviceChannelOffsetSetting(int channelOffset)
		{
			engine.SaveOutputDeviceChannelOffsetSetting(channelOffset);
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

				var clipVM = Clips[index];

				clipVM.IsPlaying = true;
				clipVM.IsNotPlaying = false;
				clipVM.TimeString = restString;
				clipVM.PlayRatio = ratio;
				clipVM.IsWarning = warning;
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
			var clipVM = Clips[index];
			clipVM.IsPlaying = false;
			clipVM.IsNotPlaying = true;
			clipVM.IsWarning = false;
			clipVM.PlayRatio = 0.0;
			clipVM.TimeString = GetClipDurationString(index);
		}

		private void Engine_ClipListChanged(object sender, EventArgs e)
		{
			SetMainWindowTitle();
		}

		private void Engine_RecentlyUsedsChanged(object sender, EventArgs e)
		{
			SetMostRecentUsedItems();
		}

		public void WindowClosing()
		{
			engine.ClearActivityIndicators();
		}

		public void Dispose()
		{
			engine?.Dispose();
			engine = null;
		}
	}

	public class WaitCursor : IDisposable
	{
		private Cursor _previousCursor;

		public WaitCursor()
		{
			_previousCursor = Mouse.OverrideCursor;

			Mouse.OverrideCursor = Cursors.Wait;
		}

		#region IDisposable Members

		public void Dispose()
		{
			Mouse.OverrideCursor = _previousCursor;
		}

		#endregion
	}
}
