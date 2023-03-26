using DocumentRepository;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DokumentTre.View.Converters;

[ValueConversion(typeof((byte[], DocumentTypes)), typeof(BitmapSource))]
public class ByteToBitmapConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is (byte[] data, DocumentTypes documentType))
        {
            using MemoryStream memoryStream = new(data);

            switch (documentType)
            {
                case DocumentTypes.PngImage:
                    return new PngBitmapDecoder(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
                case DocumentTypes.JpgImage:
                    return new JpegBitmapDecoder(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
                default:
                    return null;
            }
        }
        else return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}