using System;
using Avalonia;
using Avalonia.Controls;

namespace AluminumFoil.Posix.Views
{
    public class AutoHeightWindow : Window
    {
        // This is a bit of a hacky workaround for autosizing not working in
        //      linux/osx. MainPanel has to be re-measured with an infinite
        //      height so that it measures outside the window bounds.
        // In order to use this define the sizing element as MainPanel in 
        //      the codebehind xaml.cs
        // This is far from perfect and can show some weird black areas but
        //      it is better than nothing.

        public StackPanel MainPanel;

        public AutoHeightWindow()
        {
            LayoutUpdated += AutoSizeHeight;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

        }

        public void AutoSizeHeight(object sender, EventArgs e)
        {
            MainPanel.Measure(new Size(Width, double.PositiveInfinity));
            Height = MainPanel.DesiredSize.Height;
            InvalidateMeasure();
        }
    }
}
