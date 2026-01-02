using DocumentRepository;
using System.ComponentModel;

namespace DokumentTre.Model;

public sealed class ImageElement : BaseElement
{
    private static PropertyChangedEventArgs? s_imageChangedEventArgs;

    public ImageElement(IRepositoryItem databaseElement, FolderElement parent) : base(databaseElement, parent)
    {
        databaseElement.ContentChanged += NotifyImageChanged;
    }

    public (byte[], DocumentTypes) Image
    {
        get
        {
            return (DatabaseElement.GetContent(), DatabaseElement.DocumentType);
        }
    }

    private void NotifyImageChanged(object? sender, System.EventArgs e)
    {
        OnPropertyChanged(s_imageChangedEventArgs ??= new PropertyChangedEventArgs(nameof(Image)));
    }
}