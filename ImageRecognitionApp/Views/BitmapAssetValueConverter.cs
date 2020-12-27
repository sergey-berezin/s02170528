using System;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace ImageRecognitionApp.Views
{
    public class BitmapAssetValueConverter: IValueConverter
    {
        public static BitmapAssetValueConverter Instance = new BitmapAssetValueConverter();
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value == null ? null : new Bitmap((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}