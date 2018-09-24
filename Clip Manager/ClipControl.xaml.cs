using Clip_Manager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Clip_Manager
{
	/// <summary>
	/// Interaction logic for ClipControl.xaml
	/// </summary>
	public partial class ClipControl : UserControl
	{
		public CachedSound AudioClip
		{
			get { return (CachedSound)GetValue(AudioClipProperty); }
			set { SetValue(AudioClipProperty, value); }
		}

		// Using a DependencyProperty as the backing store for AudioClip.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AudioClipProperty =
			DependencyProperty.Register("AudioClip", typeof(CachedSound), typeof(ClipControl), new PropertyMetadata(null));



		public int ClipIndex
		{
			get { return (int)GetValue(ClipIndexProperty); }
			set { SetValue(ClipIndexProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ClipIndex.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ClipIndexProperty =
			DependencyProperty.Register("ClipIndex", typeof(int), typeof(ClipControl), new PropertyMetadata(0));



		public ClipControl()
		{
			InitializeComponent();

			Console.WriteLine("ClipControl initialized with clip index {0}", ClipIndex);
		}

		private void clipControl_Loaded(object sender, RoutedEventArgs e)
		{
			Console.WriteLine("ClipControl loaded with clip index {0}", ClipIndex);
		}
	}
}
