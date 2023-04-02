using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DokumentTre.View.Converters;

// Hjelper for å kunne legge til FlowDocument som attatched property.
// Dette er et property som det kan bindes til. Noe som ikke er mulig
// i property Document i RichTextBox (er ikke dependecy property).
// Hvis flow som er bindable attatched property. 
public class RichTextBoxHelper : DependencyObject
{
    public static byte[] GetFlowDocument(UIElement target)
    {
        return (byte[])target.GetValue(FlowDocumentProperty);
    }

    // Feil type på value, burde vært byte[], men får da feilmelding:
    // MC4102 Tags of type 'PropertyArrayStart' are not supported in template sections.
    // når jeg binder til property. Bruker derfor object.
    public static void SetFlowDocument(UIElement target, object value)
    {
        target.SetValue(FlowDocumentProperty, value);
    }

    public static readonly DependencyProperty FlowDocumentProperty = DependencyProperty.RegisterAttached(
            "FlowDocument",
            typeof(byte[]),
            typeof(RichTextBoxHelper),
            new FrameworkPropertyMetadata { PropertyChangedCallback = FlowDocumentPropertyChanged });


    private static void FlowDocumentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is RichTextBox richTextBox)
        {
            byte[]? data = GetFlowDocument(richTextBox);

            if (data is not null)
            {
                using MemoryStream memoryStream = new(data);
                FlowDocument flowDocument = new();
                TextRange textRange = new(flowDocument.ContentStart, flowDocument.ContentEnd);
                textRange.Load(memoryStream, DataFormats.XamlPackage);

                // Set the document
                richTextBox.Document = flowDocument;
            }
            else
            {
                richTextBox.Document.Blocks.Clear();
            }
        }
    }
}