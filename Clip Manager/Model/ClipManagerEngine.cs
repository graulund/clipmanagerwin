using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAudio.Midi;
using NAudio.Wave;

namespace Clip_Manager.Model
{
	class ClipManagerEngine : IDisposable
	{
		private AsioOut outputDevice;
		private bool outputDeviceDirty;
		private readonly SynchronizationContext mainThread;
		private System.Timers.Timer timer;

		public const int NUM_CLIPS = 8;
		public const int NUM_RECENTLY_USEDS = 11; // One more than is displayed in the interface

		public Dictionary<int, CachedSound> Clips { get; set; }

		public int? CurrentlyPlayingIndex = null;
		public CachedSound CurrentlyPlayingClip = null;
		public CachedSoundSampleProvider CurrentlyPlayingSampleProvider = null;
		private int? startIndexAfterStopping = null;

		public string OutputDeviceProductGuid { get; set; }
		public int OutputDeviceChannelOffset { get; set; }
		public List<MidiIn> MidiIns { get; set; }
		public List<MidiOut> MidiOuts { get; set; }

		public List<string> RecentlyUsedListFileNames { get; set; }
		public string ClipListFileName { get; set; }
		public bool ClipListIsDirty { get; set; }

		public event EventHandler ClipsChanged;
		public event EventHandler<ClipEventArgs> ClipStartedPlaying;
		public event EventHandler<ClipEventArgs> ClipStoppedPlaying;
		public event EventHandler ClipListChanged;
		public event EventHandler RecentlyUsedsChanged;

		public ClipManagerEngine()
		{
			mainThread = SynchronizationContext.Current;

			Clips = new Dictionary<int, CachedSound>(NUM_CLIPS);
			ClipListFileName = null;
			ClipListIsDirty = false;

			timer = new System.Timers.Timer(20);
			timer.Elapsed += Timer_Elapsed;
			outputDeviceDirty = false;
			LoadOutputDeviceProductGuidSetting();
			LoadOutputDeviceChannelOffsetSetting();
			SetOutputDevice();

			LoadMidiDevices();
			LoadRecentlyUsedsFromSettings();
		}

		private void SetOutputDevice() {
			if (OutputDeviceProductGuid == null || OutputDeviceProductGuid == string.Empty) {
				Console.WriteLine("Wanted to set output device, but it has no id");
				return;
			}

			Console.WriteLine("Setting output device");


			if (outputDevice != null) {
				if (!outputDeviceDirty && outputDevice.DriverName == OutputDeviceProductGuid) {
					Console.WriteLine("No need to reset output device");
					return;
				}

				if (outputDevice.PlaybackState != PlaybackState.Stopped)
				{
					outputDevice.Stop();
				}

				outputDevice.Dispose();
				outputDevice = null;
			}

			//outputDevice = new WaveOutEvent { DeviceNumber = GetOutputDeviceIndex() };
			try
			{
				outputDevice = new AsioOut(OutputDeviceProductGuid);
				outputDevice.ChannelOffset = OutputDeviceChannelOffset;
				outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
				outputDeviceDirty = false;
			}
			catch (Exception e) {
				Console.WriteLine("Exception occurred creating output device: {0}", e.Message);
				return;
			}

		}

		public void SetClip(int index, string fileName)
		{
			CachedSound clip;

			try {
				clip = new CachedSound(fileName);
			}

			catch (Exception e) {
				Console.WriteLine(
					"Exception occurred creating cached sound from file name {0}: {1}",
					fileName, e.Message
				);
				return;
			}

			if (Clips == null)
			{
				Clips = new Dictionary<int, CachedSound>(NUM_CLIPS);
			}

			if (Clips.ContainsKey(index)) {
				Clips.Remove(index);
			}

			Clips.Add(index, clip);
			ClipListIsDirty = true;
			OnClipsChanged();
			OnClipListChanged();
		}

		public void ClearClips() {
			Clips = new Dictionary<int, CachedSound>(NUM_CLIPS);
			ClipListFileName = null;
			ClipListIsDirty = false;
			OnClipsChanged();
			OnClipListChanged();
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
				SetOutputDevice();

				if (outputDevice == null) {
					Console.WriteLine("Tried to play index {0}, but output device is null", index);
					return;
				}

				outputDeviceDirty = true;

				try {
					outputDevice.Init(sampleProvider);
					outputDevice.Play();
					Console.WriteLine("Setting currently playing to be index {0}", index);
					if (outputDevice.PlaybackState == PlaybackState.Playing)
					{
						timer.Start();
						CurrentlyPlayingIndex = index;
						CurrentlyPlayingClip = clip;
						CurrentlyPlayingSampleProvider = sampleProvider;
						UpdateActivityIndicator(index);
						OnClipStartedPlaying(index);
					}
				}
				catch (Exception e) {
					Console.WriteLine(
						"Tried to play index {0}, but an exception occurred: {1}",
						index, e.Message
					);
				}
			}
			else
			{
				Console.WriteLine("Tried to play index {0}, but no such clip exists", index);
			}
		}

		public void ToggleSound(int index)
		{
			if (CurrentlyPlayingIndex != null && CurrentlyPlayingIndex.Value == index)
			{
				Stop();
				return;
			}

			PlaySound(index);
		}

		public void Stop()
		{
			Console.WriteLine("Stopping clip");
			timer.Stop();

			if (outputDevice != null && !outputDeviceDirty) {
				return;
			}

			outputDevice?.Stop();
		}

		public void LoadClipsFromFile(string fileName) {
			try {
				Clips = ClipFileHandler.ReadClipsFile(fileName);
			}
			catch (Exception) {
				return;
			}

			ClipListFileName = fileName;
			ClipListIsDirty = false;
			OnClipsChanged();
			OnClipListChanged();
			AddToRecentlyUsedListFiles(fileName);
		}

		public void SaveClipsToFile(string fileName) {
			try {
				ClipFileHandler.WriteClipsFile(Clips, fileName);
			}
			catch (Exception) {
				return;
			}

			ClipListFileName = fileName;
			ClipListIsDirty = false;
			OnClipListChanged();
			AddToRecentlyUsedListFiles(fileName);
		}

		public void LoadClipsFromDirectory(string directoryName) {
			List<string> files;

			try {
				files = new List<string>(ClipFileHandler.LoadDirectory(directoryName));
			}
			catch (Exception) {
				return;
			}

			var clips = new Dictionary<int, CachedSound>(NUM_CLIPS);

			for (var i = 0; i < NUM_CLIPS; i++) {
				if (files.Count > i) {
					try {
						clips.Add(i, new CachedSound(files[i]));
					} catch (Exception e) {
						Console.WriteLine(
							"Exception occurred creating cached sound from file name {0}: {1}",
							files[i], e.Message
						);
						return;
					}
				}
			}

			Clips = clips;
			ClipListFileName = null;
			ClipListIsDirty = true;
			OnClipsChanged();
			OnClipListChanged();
		}

		public int GetOutputDeviceIndex() {
			for (int n = -1; n < WaveOut.DeviceCount; n++) {
				var caps = WaveOut.GetCapabilities(n);
				if (OutputDeviceProductGuid == caps.ProductGuid.ToString()) {
					return n;
				}
			}

			return -1;
		}

		public void LoadOutputDeviceProductGuidSetting() {
			OutputDeviceProductGuid = Properties.Settings.Default.OutputDeviceProductGuid;

			if (outputDevice != null && outputDevice.DriverName != OutputDeviceProductGuid)
			{
				SetOutputDevice();
			}
		}

		public void SaveOutputDeviceProductGuidSetting(string productGuid) {
			OutputDeviceProductGuid = productGuid;
			Properties.Settings.Default.OutputDeviceProductGuid = productGuid;
			Properties.Settings.Default.Save();

			if (outputDevice != null && outputDevice.DriverName != OutputDeviceProductGuid)
			{
				SetOutputDevice();
			}
		}

		public void LoadOutputDeviceChannelOffsetSetting()
		{
			OutputDeviceChannelOffset = Properties.Settings.Default.OutputDeviceChannelOffset;
		}

		public void SaveOutputDeviceChannelOffsetSetting(int channelOffset)
		{
			OutputDeviceChannelOffset = channelOffset;
			Properties.Settings.Default.OutputDeviceChannelOffset = channelOffset;
			Properties.Settings.Default.Save();
		}

		public void LoadRecentlyUsedsFromSettings() {
			var recentlyUseds = new List<string>();

			for (var i = 0; i < NUM_RECENTLY_USEDS; i++) {
				var path = Properties.Settings.Default["RecentlyUsedPath" + i];

				if (path is string && (string)path != "") {
					recentlyUseds.Add((string)path);
				}

				else {
					break;
				}
			}

			Console.WriteLine("Loaded {0} recently useds from settings", recentlyUseds.Count);

			RecentlyUsedListFileNames = recentlyUseds;
			OnRecentlyUsedsChanged();
		}

		public void StoreRecentlyUsedIntoSettings() {
			for (var i = 0; i < NUM_RECENTLY_USEDS; i++) {
				Properties.Settings.Default["RecentlyUsedPath" + i] =
					RecentlyUsedListFileNames.Count > i ? RecentlyUsedListFileNames[i] : null;
			}

			Properties.Settings.Default.Save();
		}

		public void AddToRecentlyUsedListFiles(string filePath) {
			RecentlyUsedListFileNames = RecentlyUsedListFileNames.Where(f => f != filePath).ToList();
			RecentlyUsedListFileNames.Insert(0, filePath);
			RecentlyUsedListFileNames = RecentlyUsedListFileNames.Take(NUM_RECENTLY_USEDS).ToList();
			OnRecentlyUsedsChanged();
			StoreRecentlyUsedIntoSettings();
		}

		public void LoadRecentlyUsedListFile(int index) {
			if (RecentlyUsedListFileNames.Count <= index) {
				return;
			}

			LoadClipsFromFile(RecentlyUsedListFileNames[index]);
		}

		private void OutputDevice_PlaybackStopped(object sender, StoppedEventArgs e)
		{
			Console.WriteLine("Playback stopped");
			var playedIndex = CurrentlyPlayingIndex;

			CurrentlyPlayingIndex = null;
			CurrentlyPlayingClip = null;

			if (playedIndex != null)
			{
				UpdateActivityIndicator(playedIndex.Value);
				OnClipStoppedPlaying(playedIndex.Value);
			}

			if (startIndexAfterStopping != null)
			{
				var index = startIndexAfterStopping.Value;
				startIndexAfterStopping = null;
				PlaySound(index);
			}
		}

		private void OnNoteOn(int note) {
			var index = ClipMidiTools.IndexForNote(note);

			if (index >= 0) {
				ToggleSound(index);
			}
		}

		private void OnNoteOff(int note) {
			var index = ClipMidiTools.IndexForNote(note);

			if (index >= 0) {
				UpdateActivityIndicator(index);
			}
		}

		private void MidiIn_MessageReceived(object sender, MidiInMessageEventArgs e) {
			var evt = e.MidiEvent;

			if (MidiEvent.IsNoteOn(evt)) {
				mainThread.Post(o => OnNoteOn(((NoteEvent)evt).NoteNumber), null);
			}

			else if (MidiEvent.IsNoteOff(evt)) {
				mainThread.Post(o => OnNoteOff(((NoteEvent)evt).NoteNumber), null);
			}
		}

		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (timer.Enabled && outputDevice != null && outputDevice.HasReachedEnd)
			{
				timer.Stop();
				outputDevice.Stop();
			}
		}

		private void SetActivityIndicator(int index, bool on) {
			if (MidiOuts != null) {
				foreach (var device in MidiOuts) {
					ClipMidiTools.SetIndexActivityIndicator(device, index, on);
				}
			}
		}

		private void UpdateActivityIndicator(int index) {
			SetActivityIndicator(index, CurrentlyPlayingIndex != null && CurrentlyPlayingIndex == index);
		}

		public void ClearActivityIndicators() {
			for (var i = 0; i < NUM_CLIPS; i++) {
				SetActivityIndicator(i, false);
			}
		}

		private void DisposeMidiDevices() {
			if (MidiIns != null) {
				foreach (var device in MidiIns) {
					device.Stop();
					device.Dispose();
				}
			}

			if (MidiOuts != null) {
				foreach (var device in MidiOuts) {
					device.Dispose();
				}
			}
		}

		public void LoadMidiDevices() {
			DisposeMidiDevices();

			MidiIns = new List<MidiIn>();
			MidiOuts = new List<MidiOut>();

			for (int device = 0; device < MidiIn.NumberOfDevices; device++) {
				var caps = MidiIn.DeviceInfo(device);

				if (ClipMidiTools.IsCompatibleDeviceName(caps.ProductName)) {
					var midiIn = new MidiIn(device);
					midiIn.MessageReceived += MidiIn_MessageReceived;
					midiIn.Start();
					MidiIns.Add(midiIn);
				}
			}

			for (int device = 0; device < MidiOut.NumberOfDevices; device++) {
				var caps = MidiOut.DeviceInfo(device);

				if (ClipMidiTools.IsCompatibleDeviceName(caps.ProductName)) {
					MidiOuts.Add(new MidiOut(device));
				}
			}

			for (var i = 0; i < NUM_CLIPS; i++) {
				UpdateActivityIndicator(i);
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

		protected virtual void OnClipListChanged()
		{
			ClipListChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnRecentlyUsedsChanged()
		{
			RecentlyUsedsChanged?.Invoke(this, EventArgs.Empty);
		}

		public void Dispose()
		{
			outputDeviceDirty = true;
			outputDevice.Dispose();
			DisposeMidiDevices();
		}
	}

	public class ClipEventArgs : EventArgs
	{
		public int Index { get; set; }
	}
}
