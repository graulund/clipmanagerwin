using System;

namespace Clip_Manager.ViewModel
{
	public class ClipViewModel : ViewModelBase
	{
		private int _number;
		public int Number
		{
			get => _number;
			set => SetProperty(ref _number, value);
		}

		private bool _hasValue;
		public bool HasValue
		{
			get => _hasValue;
			set => SetProperty(ref _hasValue, value);
		}

		private string _fileName;
		public string FileName
		{
			get => _fileName;
			set => SetProperty(ref _fileName, value);
		}

		private string _timeString;
		public string TimeString
		{
			get => _timeString;
			set => SetProperty(ref _timeString, value);
		}

		private bool _isPlaying;
		public bool IsPlaying
		{
			get => _isPlaying;
			set => SetProperty(ref _isPlaying, value);
		}

		private double _playRatio;
		public double PlayRatio
		{
			get => _playRatio;
			set => SetProperty(ref _playRatio, value);
		}

		private bool _isWarning;
		public bool IsWarning
		{
			get => _isWarning;
			set => SetProperty(ref _isWarning, value);
		}
	}
}
