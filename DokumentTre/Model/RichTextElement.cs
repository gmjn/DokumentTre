using DocumentRepository;
using System.ComponentModel;

namespace DokumentTre.Model;

public sealed class RichTextElement : BaseElement
{
    private static PropertyChangedEventArgs? s_richTextDataChangedEventArgs;

    public RichTextElement(IRepositoryItem databaseElement, FolderElement parent) : base(databaseElement, parent)
    {
        databaseElement.ContentChanged += NotifyRichTextDataChanged;
    }

    public byte[] RichTextData
    {
        get
        {
            return DatabaseElement.GetContent();
        }
    }

    public void NotifyRichTextDataChanged(object? sender, System.EventArgs e)
    {
        OnPropertyChanged(s_richTextDataChangedEventArgs ??= new PropertyChangedEventArgs(nameof(RichTextData)));
    }
}