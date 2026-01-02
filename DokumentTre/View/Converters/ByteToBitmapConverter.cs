using DocumentRepository;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DokumentTre.View.Converters;

[ValueConversion(typeof((byte[], DocumentTypes)), typeof(BitmapSource))]
public sealed class ByteToBitmapConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is (byte[] data, DocumentTypes documentType))
        {
            using MemoryStream memoryStream = new(data);

            return documentType switch
            {
                DocumentTypes.PngImage or DocumentTypes.PngImageScaleBoth => new PngBitmapDecoder(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0],
                DocumentTypes.JpgImage or DocumentTypes.JpgImageScaleBoth => new JpegBitmapDecoder(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0],
                _ => null,
            };
        }
        else return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}