using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Clip_Manager.ViewModel;
using Microsoft.Win32;

namespace Clip_Manager
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		const string CloseWarning = "You have unsaved changes. Save them before continuing?";

		public MainWindow()
		{
			var viewModel = new ClipManagerViewModel();
			DataContext = viewModel;
			InitializeComponent();

			KeyUp += MainWindow_KeyUp;

			viewModel.RecentMenuSeparator = RecentMenuSeparator;
			viewModel.RecentMenuItems.Add(RecentMenuItem1);
			viewModel.RecentMenuItems.Add(RecentMenuItem2);
			viewModel.RecentMenuItems.Add(RecentMenuItem3);
			viewModel.RecentMenuItems.Add(RecentMenuItem4);
			viewModel.RecentMenuItems.Add(RecentMenuItem5);
			viewModel.RecentMenuItems.Add(RecentMenuItem6);
			viewModel.RecentMenuItems.Add(RecentMenuItem7);
			viewModel.RecentMenuItems.Add(RecentMenuItem8);
			viewModel.RecentMenuItems.Add(RecentMenuItem9);
			viewModel.RecentMenuItems.Add(RecentMenuItem10);
			viewModel.SetMostRecentUsedItems();
		}

		private void MainWindow_KeyUp(object sender, KeyEventArgs e)
		{
			var viewModel = (ClipManagerViewModel)DataContext;
			switch (e.Key)
			{
				case Key.D1:
				case Key.D2:
				case Key.D3:
				case Key.D4:
				case Key.D5:
				case Key.D6:
				case Key.D7:
				case Key.D8:
				case Key.D9:
					viewModel.ToggleClip((int)e.Key - 35);
					break;
			}
		}

		private void ProgressBar_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				var files = (string[])e.Data.GetData(DataFormats.FileDrop);

				if (files.Length == 1)
				{
					var file = files[0];
					Console.WriteLine("Dropped: {0}", file);
					var extension = Path.GetExtension(file).ToLowerInvariant();

					if (
						extension == ".wav" ||
						extension == ".mp3" ||
						extension == ".aiff" ||
						extension == ".aac"
					)
					{
						// Time to find the number you dragged on to
						// Is there really not a better way to do this?

						var parent = ((Control)sender).Parent;

						if (parent is Grid)
						{
							var grid = (Grid)parent;
							foreach (var item in grid.Children)
							{
								if (item is Label)
								{
									var label = (Label)item;
									if (label.Content is int)
									{
										var number = (int)label.Content;
										var manager = (ClipManagerViewModel)DataContext;
										manager.SetClip(number, file);
									}
								}
							}
						}
					}
				}
			}
		}

		private void NewClipList() {
			var manager = (ClipManagerViewModel)DataContext;

			if (!manager.IsDirty) {
				manager.ClearClips();
			} else {
				var result = MessageBox.Show(CloseWarning, "Clips", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

				switch (result) {
					case MessageBoxResult.Yes:
						SaveClipList();
						manager.ClearClips();
						break;
					case MessageBoxResult.No:
						manager.ClearClips();
						break;
				}
			}
		}

		private void OpenClipList() {
			var manager = (ClipManagerViewModel)DataContext;

			if (!manager.IsDirty) {
				_OpenClipList();
			} else {
				var result = MessageBox.Show(CloseWarning, "Clips", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

				switch (result) {
					case MessageBoxResult.Yes:
						SaveClipList();
						_OpenClipList();
						break;
					case MessageBoxResult.No:
						_OpenClipList();
						break;
				}
			}
		}

		private void _OpenClipList() {
			var dialog = new OpenFileDialog();
			dialog.DefaultExt = "clips";
			dialog.Filter = "Clip Lists (*.clips)|*.clips";

			if (dialog.ShowDialog() == true) {
				var manager = (ClipManagerViewModel)DataContext;
				manager.LoadClipsFromFile(dialog.FileName);
			}
		}

		private void OpenDirectory() {
			var manager = (ClipManagerViewModel)DataContext;

			if (!manager.IsDirty) {
				_OpenDirectory();
			} else {
				var result = MessageBox.Show(CloseWarning, "Clips", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

				switch (result) {
					case MessageBoxResult.Yes:
						SaveClipList();
						_OpenDirectory();
						break;
					case MessageBoxResult.No:
						_OpenDirectory();
						break;
				}
			}
		}

		private void _OpenDirectory() {
			var dialog = new System.Windows.Forms.FolderBrowserDialog();

			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				var manager = (ClipManagerViewModel)DataContext;
				manager.LoadClipsFromDirectory(dialog.SelectedPath);
			}
		}

		private void SaveClipList() {
			var manager = (ClipManagerViewModel)DataContext;
			
			if (manager.FileName != null) {
				manager.SaveClipsToFile(manager.FileName);
			}

			else {
				SaveClipListAs();
			}
		}

		private void SaveClipListAs() {
			var dialog = new SaveFileDialog();
			dialog.DefaultExt = "clips";
			dialog.Filter = "Clip Lists (*.clips)|*.clips";

			if (dialog.ShowDialog() == true) {
				var manager = (ClipManagerViewModel)DataContext;
				manager.SaveClipsToFile(dialog.FileName);
			}
		}

		private void exit_Click(object sender, RoutedEventArgs e) {
			Application.Current.Shutdown();
		}

		private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			NewClipList();
		}

		private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			OpenClipList();
		}

		private void OpenFolderCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			OpenDirectory();
		}

		private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			SaveClipList();
		}

		private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			SaveClipListAs();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			var manager = (ClipManagerViewModel)DataContext;

			if (manager.IsDirty) {
				var result = MessageBox.Show(CloseWarning, "Clips", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

				switch (result) {
					case MessageBoxResult.Yes:
						SaveClipList();
						break;
					case MessageBoxResult.Cancel:
						e.Cancel = true;
						break;
				}
			}
		}

		private void RecentMenuItem1_Click(object sender, RoutedEventArgs e) {
			var manager = (ClipManagerViewModel)DataContext;
			manager.LoadRecentlyUsedListFile(0);
		}

		private void RecentMenuItem2_Click(object sender, RoutedEventArgs e) {
			var manager = (ClipManagerViewModel)DataContext;
			manager.LoadRecentlyUsedListFile(1);
		}

		private void RecentMenuItem3_Click(object sender, RoutedEventArgs e) {
			var manager = (ClipManagerViewModel)DataContext;
			manager.LoadRecentlyUsedListFile(2);
		}

		private void RecentMenuItem4_Click(object sender, RoutedEventArgs e) {
			var manager = (ClipManagerViewModel)DataContext;
			manager.LoadRecentlyUsedListFile(3);
		}

		private void RecentMenuItem5_Click(object sender, RoutedEventArgs e) {
			var manager = (ClipManagerViewModel)DataContext;
			manager.LoadRecentlyUsedListFile(4);
		}

		private void RecentMenuItem6_Click(object sender, RoutedEventArgs e) {
			var manager = (ClipManagerViewModel)DataContext;
			manager.LoadRecentlyUsedListFile(5);
		}

		private void RecentMenuItem7_Click(object sender, RoutedEventArgs e) {
			var manager = (ClipManagerViewModel)DataContext;
			manager.LoadRecentlyUsedListFile(6);
		}

		private void RecentMenuItem8_Click(object sender, RoutedEventArgs e) {
			var manager = (ClipManagerViewModel)DataContext;
			manager.LoadRecentlyUsedListFile(7);
		}

		private void RecentMenuItem9_Click(object sender, RoutedEventArgs e) {
			var manager = (ClipManagerViewModel)DataContext;
			manager.LoadRecentlyUsedListFile(8);
		}

		private void RecentMenuItem10_Click(object sender, RoutedEventArgs e) {
			var manager = (ClipManagerViewModel)DataContext;
			manager.LoadRecentlyUsedListFile(9);
		}

		/*
        private void OnOpenFileClick(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            string allExtensions = "*.wav;*.aiff;*.mp3;*.aac";
            openFileDialog.Filter = String.Format("All Supported Files|{0}|All Files (*.*)|*.*", allExtensions);
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDialog.FileName;
            }
        }
		*/
	}

	public static class Commands {
		public static readonly RoutedCommand NewCommand =
			new RoutedCommand("New Clip List", typeof(MainWindow));
		public static readonly RoutedCommand OpenCommand =
			new RoutedCommand("Open Clip List…", typeof(MainWindow));
		public static readonly RoutedCommand OpenFolderCommand =
			new RoutedCommand("Open Clip List From Folder…", typeof(MainWindow));
		public static readonly RoutedCommand SaveCommand =
			new RoutedCommand("Save Clip List", typeof(MainWindow));
		public static readonly RoutedCommand SaveAsCommand =
			new RoutedCommand("Save Clip List As…", typeof(MainWindow));

		static Commands() {
			NewCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
			OpenCommand.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
			OpenFolderCommand.InputGestures.Add(new KeyGesture(Key.I, ModifierKeys.Control));
			SaveCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
			SaveAsCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift));
		}
	}
}
