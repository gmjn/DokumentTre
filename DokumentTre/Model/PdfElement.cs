using DocumentRepository;
using System.ComponentModel;

namespace DokumentTre.Model;

public sealed class PdfElement : BaseElement
{
    private static PropertyChangedEventArgs? s_pdfDataChangedEventArgs;

    public PdfElement(IRepositoryItem databaseElement, FolderElement parent) : base(databaseElement, parent)
    {
        databaseElement.ContentChanged += NotifyPdfDataChanged;
    }

    public byte[] PdfData
    {
        get
        {
            return DatabaseElement.GetContent();
        }
    }

    public void NotifyPdfDataChanged(object? sender, System.EventArgs e)
    {
        OnPropertyChanged(s_pdfDataChangedEventArgs ??= new PropertyChangedEventArgs(nameof(PdfData)));
    }
}