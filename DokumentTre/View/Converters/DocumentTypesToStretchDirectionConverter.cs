using DocumentRepository;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace DokumentTre.View.Converters;

[ValueConversion(typeof(DocumentTypes), typeof(StretchDirection))]
public sealed class DocumentTypesToStretchDirectionConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DocumentTypes documentType)
        {
            return documentType switch
            {
                DocumentTypes.PngImageScaleBoth or DocumentTypes.JpgImageScaleBoth => StretchDirection.Both,
                _ => StretchDirection.DownOnly,
            };
        }
        else return StretchDirection.DownOnly;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}