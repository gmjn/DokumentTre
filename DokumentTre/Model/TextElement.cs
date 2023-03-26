using DocumentRepository;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DokumentTre.Model;

public class TextElement : BaseElement
{
    private static PropertyChangedEventArgs? s_textChangedEventArgs;

    public TextElement(IRepositoryItem databaseElement, FolderElement parent) : base(databaseElement, parent)
    {
        databaseElement.ContentChanged += NotifyTextChanged;
    }

    public string Text
    {
        get
        {
            return DecodeString(DatabaseElement.GetContent());
        }
    }

    public async Task<string> GetTextAsync()
    {
        return DecodeString(await DatabaseElement.GetContentAsync());
    }

    public Task SetTextAsync(string text)
    {
        return DatabaseElement.SetContentAsync(EncodeString(text));
    }

    private void NotifyTextChanged(object? sender, System.EventArgs e)
    {
        OnPropertyChanged(s_textChangedEventArgs ??= new PropertyChangedEventArgs(nameof(Text)));
    }
}