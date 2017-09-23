using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace hoReverse.DataBinding
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new BitmapImage(new Uri("Resources/Images/Activity.bmp", UriKind.Relative)); 
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("The method or operation is not implemented.");
        }
    }
}
