namespace library
{
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;

	/// <summary>
	/// Interaction logic for LedIndicator.xaml
	/// </summary>
	public partial class LedIndicator : UserControl
	{
		public LedIndicator()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Sets a built-in gradient color that mimics a real glow.
		/// </summary>
		/// <param name="lensColor"></param>
		public void setLensColor(LensColor lensColor)
		{
			if (lensColor == LensColor.BLANK)
			{
				this.ellipseDiode.Fill = this.FindResource("blankDiode") as Brush;
			}
			else if (lensColor == LensColor.BLUE)
			{
				this.ellipseDiode.Fill = this.FindResource("blueDiode") as Brush;
			}
			else if (lensColor == LensColor.GREEN)
			{
				this.ellipseDiode.Fill = this.FindResource("greenDiode") as Brush;
			}
			else if (lensColor == LensColor.ORANGE)
			{
				this.ellipseDiode.Fill = this.FindResource("orangeDiode") as Brush;
			}
			else if (lensColor == LensColor.RED)
			{
				this.ellipseDiode.Fill = this.FindResource("redDiode") as Brush;
			}
			else if (lensColor == LensColor.WHITE)
			{
				this.ellipseDiode.Fill = this.FindResource("whiteDiode") as Brush;
			}
		}

		/// <summary>
		/// Basically sets the opacity of the inner ellipse.
		/// </summary>
		/// <param name="intensity"></param>
		public void setIntensity(double intensity)
		{
			if (intensity >= 0.0 && intensity <= 1.0)
			{
				this.ellipseDiode.Opacity = intensity;
			}
		}

		/// <summary>
		/// This makes a neat transition from one color to another by changing it then pulsing.
		/// </summary>
		/// <param name="color"></param>
		public void setLensColorPulse(LensColor color)
		{
			setLensColor(color);
			pulse();
		}

		/// <summary>
		/// Makes the LED dim and then go back to full intensity.
		/// </summary>
		public void pulse()
		{
			if (PulseEvent != null)
			{
				RoutedEventArgs e = new RoutedEventArgs(PulseEvent);
				RaiseEvent(e);
			}
		}

		/// <summary>
		/// Use this if want to set the led color as a basic, uniform color
		/// with no glow effects.
		/// </summary>
		/// <param name="brush"></param>
		public void setLensColor(Brush brush)
		{
			this.ellipseDiode.Fill = brush;
		}

		/// <summary>
		/// These are mapped to some resources in the User Control.
		/// </summary>
		public enum LensColor
		{
			RED,GREEN,BLUE,ORANGE,BLANK,WHITE
		}

		//
		// Control Events

		/// <summary>
		/// Set the initial lens color.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			setLensColor(LensColor.BLANK);
		}

		//
		// Events

		/// <summary>
		/// This isn't really meant to use publicly.  This is used for pulse events.
		/// </summary>
		public event RoutedEventHandler Pulse
		{
			remove { RemoveHandler(PulseEvent, value); }
			add { AddHandler(PulseEvent, value); }
		}
		public static readonly RoutedEvent PulseEvent =
			EventManager.RegisterRoutedEvent("Pulse", RoutingStrategy.Direct, 
			typeof(RoutedEventHandler), typeof(LedIndicator));
	}
}
