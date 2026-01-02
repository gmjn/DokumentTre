using DocumentRepository;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DokumentTre.Model;

public sealed class HtmlElement : BaseElement
{
    private static PropertyChangedEventArgs? s_htmlChangedEventArgs;

    public HtmlElement(IRepositoryItem databaseElement, FolderElement parent) : base(databaseElement, parent)
    {
        databaseElement.ContentChanged += NotifyHtmlChanged;
    }

    public string Html
    {
        get
        {
            return DecodeString(DatabaseElement.GetContent());
        }
    }

    public async Task<string> GetHtmlAsync()
    {
        return DecodeString(await DatabaseElement.GetContentAsync());
    }

    public Task SetHtmlAsync(string html)
    {
        return DatabaseElement.SetContentAsync(EncodeString(html));
    }

    private void NotifyHtmlChanged(object? sender, System.EventArgs e)
    {
        OnPropertyChanged(s_htmlChangedEventArgs ??= new PropertyChangedEventArgs(nameof(Html)));
    }
}