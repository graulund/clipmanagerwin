using System.Windows;
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

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			//var viewModel = (ClipViewModel)DataContext;
		}
	}
}
