using Clip_Manager.ViewModel;
using NAudio.Wave;
using System.Windows;

namespace Clip_Manager
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			InitializeComponent();
		}

		private void saveButton_Click(object sender, RoutedEventArgs e)
		{
			var manager = (ClipManagerViewModel)DataContext;
			var deviceName = soundDeviceComboBox.SelectedItem != null ? soundDeviceComboBox.SelectedItem.ToString() : string.Empty;
			var channelIndex = channelComboBox.SelectedIndex;
			var channelOffsetValue = channelIndex > 0 ? 2 * channelIndex : 0;

			manager.SaveOutputDeviceProductGuidSetting(deviceName);
			manager.SaveOutputDeviceChannelOffsetSetting(channelOffsetValue);
			Close();
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var manager = (ClipManagerViewModel)DataContext;
			var selectedIndex = 0;
			var i = 0;
			var asioDriverNames = AsioOut.GetDriverNames();

			foreach (string driverName in asioDriverNames)
			{
				soundDeviceComboBox.Items.Add(driverName);

				if (manager.OutputDeviceProductGuid == driverName)
				{
					selectedIndex = i;
				}

				i++;
			}

			soundDeviceComboBox.SelectedIndex = selectedIndex;
		}

		private void SoundDeviceComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			var manager = (ClipManagerViewModel)DataContext;
			var selectedName = soundDeviceComboBox.SelectedItem?.ToString();
			var selectedIndex = 0;

			if (selectedName != null)
			{
				var outputChannelCount = 0;
				try
				{
					var driver = new AsioOut(selectedName);
					outputChannelCount = driver.DriverOutputChannelCount;
				}
				catch (System.Exception) { }

				channelComboBox.Items.Clear();

				for (var i = 0; i < outputChannelCount / 2; i++)
				{
					var first = 2 * i + 1;
					var second = 2 * i + 2;

					if (second <= outputChannelCount)
					{
						channelComboBox.Items.Add(string.Format("Channels {0}–{1}", first, second));
					}

					if (manager.OutputDeviceChannelOffset == 2 * i)
					{
						selectedIndex = i;
					}
				}
			}

			channelComboBox.SelectedIndex = selectedIndex;
		}
	}
}
