using Avalonia;
using Avalonia.Controls;

namespace AluminumFoil.Mac.Views
{
    public class AutoHeightWindow : Window
    {
        // This is a bit of a hacky workaround for autosizing not working in
        //      linux/osx. MainPanel has to be re-measured with an infinite
        //      height so that it measures outside the window bounds.
        // In order to use this define the sizing element as MainPanel in 
        //      the codebehind xaml.cs

        public StackPanel MainPanel;

        public AutoHeightWindow()
        {
            this.LayoutUpdated += AutoSizeHeight;
        }

        public void AutoSizeHeight(object sender, System.EventArgs e)
        {
            MainPanel.Measure(new Size(this.Width, double.PositiveInfinity));
            this.Height = MainPanel.DesiredSize.Height;
            this.InvalidateMeasure();
        }
    }
}
