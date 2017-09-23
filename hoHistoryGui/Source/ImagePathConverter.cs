using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

// ReSharper disable once CheckNamespace
namespace hoReverse.DataBinding
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string path = "/AddinControl;component/Resources/Images/" + value + "Diagram.bmp";
            Uri uri = new Uri(path, UriKind.RelativeOrAbsolute);
            BitmapImage image = new BitmapImage(uri);
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("The method or operation is not implemented.");
        }
    }
}
