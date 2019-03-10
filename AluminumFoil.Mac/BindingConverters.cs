using System;
using System.Globalization;
using System.Linq;
using System.Collections.ObjectModel;
using Avalonia.Data.Converters;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace BindingConverters
{
    public class NCAFilter : IValueConverter
    {
        // Filters NSP contents to only *.nca files
        public static NCAFilter Instance = new NCAFilter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var contents = value as ObservableCollection<AluminumFoil.NSP.PFS0File>;
            return contents.Where(x => x.Name.EndsWith("nca"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class Equals : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BitmapValueConverter : IValueConverter
    // Converts resm:// URIs into bitmap for image source binding
    {
        public static BitmapValueConverter Instance = new BitmapValueConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string && targetType == typeof(IBitmap))
            {
                var uri = new Uri((string)value, UriKind.RelativeOrAbsolute);
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                var r = new Bitmap(assets.Open(uri));
                return r;
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}

