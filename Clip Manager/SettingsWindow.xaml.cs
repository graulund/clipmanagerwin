using Clip_Manager.ViewModel;
using NAudio.Wave;
using System.Collections.Generic;
using System.Windows;

namespace Clip_Manager {
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window {
		private List<WaveOutCapabilities> currentCaps;

		public SettingsWindow() {
			InitializeComponent();
		}

		private void saveButton_Click(object sender, RoutedEventArgs e) {
			var manager = (ClipManagerViewModel)DataContext;
			var index = soundDeviceComboBox.SelectedIndex;
			var item = currentCaps[index];

			manager.SaveOutputDeviceProductGuidSetting(item.ProductGuid.ToString());
			Close();
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			var manager = (ClipManagerViewModel)DataContext;
			var selectedIndex = 0;
			var i = 0;
			currentCaps = new List<WaveOutCapabilities>(WaveOut.DeviceCount);

			for (int n = -1; n < WaveOut.DeviceCount; n++) {
				var caps = WaveOut.GetCapabilities(n);
				soundDeviceComboBox.Items.Add(caps.ProductName);
				currentCaps.Add(caps);

				if (manager.OutputDeviceProductGuid == caps.ProductGuid.ToString()) {
					selectedIndex = i;
				}

				i++;
			}

			soundDeviceComboBox.SelectedIndex = selectedIndex;
		}
	}
}
