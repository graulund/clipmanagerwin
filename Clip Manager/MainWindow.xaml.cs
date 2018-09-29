using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Clip_Manager.ViewModel;

namespace Clip_Manager
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			var viewModel = new ClipManagerViewModel();
			DataContext = viewModel;
			InitializeComponent();

			KeyUp += MainWindow_KeyUp;
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
						Console.WriteLine("It's an audio file! Yay!");

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
}
