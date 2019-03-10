using System;
using System.Linq;
using System.Globalization;
using System.Windows.Data;
using System.Collections.ObjectModel;

namespace AluminumFoil.Windows.BindingConverters 
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

    public class VisibleIfInstallationTarget : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value == (string)parameter ? "Visible" : "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisibleIfExists : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? "Collapsed" : "Visible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}