using DocumentRepository;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DokumentTre.Model;

public sealed class FolderElement : BaseElement
{
    private static PropertyChangedEventArgs? s_textChangedEventArgs;

    public FolderElement(IRepositoryItem databaseElement, FolderElement? parent) : base(databaseElement, parent)
    {
        Children = [];
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

    public async Task LoadChildrenAsync(bool loadChildFoldersOfChilds, PropertyChangedEventHandler folderExpandChanged)
    {
        if (Children.Count > 0) return;

        await foreach (IRepositoryItem Child in DatabaseElement.GetChildrenAsync())
        {
            switch (Child.DocumentType)
            {
                case DocumentTypes.Folder:
                    {
                        FolderElement newFolder = new(Child, this);

                        newFolder.PropertyChanged += folderExpandChanged;
                        Children.Add(newFolder);

                        if (loadChildFoldersOfChilds)
                        {
                            await newFolder.LoadChildrenAsync(false, folderExpandChanged);
                        }
                    }
                    break;
                case DocumentTypes.PlainText:
                    Children.Add(new TextElement(Child, this));
                    break;
                case DocumentTypes.JpgImage:
                case DocumentTypes.PngImage:
                case DocumentTypes.JpgImageScaleBoth:
                case DocumentTypes.PngImageScaleBoth:
                    Children.Add(new ImageElement(Child, this));
                    break;
                case DocumentTypes.RichTextDocument:
                    Children.Add(new RichTextElement(Child, this));
                    break;
                case DocumentTypes.HtmlDocument:
                    Children.Add(new HtmlElement(Child, this));
                    break;
                case DocumentTypes.PdfDocument:
                    Children.Add(new PdfElement(Child, this));
                    break;
                default:
                    throw new Exception("Database error.");
            }
        }
    }

    public async Task<FolderElement> AddFolderAsync(string name, string text)
    {
        FolderElement folderElement = new(await DatabaseElement.CreateChildAsync(name, DocumentTypes.Folder, EncodeString(text)), this);
        Children.Add(folderElement);
        IsExpanded = true;
        folderElement.IsSelected = true;

        return folderElement;
    }

    public async Task<TextElement> AddTextAsync(string name, string text)
    {
        TextElement textElement = new(await DatabaseElement.CreateChildAsync(name, DocumentTypes.PlainText, EncodeString(text)), this);
        Children.Add(textElement);
        IsExpanded = true;
        textElement.IsSelected = true;

        return textElement;
    }

    public async Task<RichTextElement> AddRichTextAsync(string name, byte[] richTextData)
    {
        RichTextElement richTextElement = new(await DatabaseElement.CreateChildAsync(name, DocumentTypes.RichTextDocument, richTextData), this);
        Children.Add(richTextElement);
        IsExpanded = true;
        richTextElement.IsSelected = true;

        return richTextElement;
    }

    public async Task<ImageElement> AddImageAsync(string name, DocumentTypes documentType, byte[] image)
    {
        ImageElement imageElement = new(await DatabaseElement.CreateChildAsync(name, documentType, image), this);
        Children.Add(imageElement);
        IsExpanded = true;
        imageElement.IsSelected = true;

        return imageElement;
    }

    public async Task<HtmlElement> AddHtmlAsync(string name, string html)
    {
        HtmlElement htmlElement = new(await DatabaseElement.CreateChildAsync(name, DocumentTypes.HtmlDocument, EncodeString(html)), this);
        Children.Add(htmlElement);
        IsExpanded = true;
        htmlElement.IsSelected = true;

        return htmlElement;
    }

    public async Task<PdfElement> AddPdfAsync(string name, byte[] pdfData)
    {
        PdfElement pdfElement = new(await DatabaseElement.CreateChildAsync(name, DocumentTypes.PdfDocument, pdfData), this);
        Children.Add(pdfElement);
        IsExpanded = true;
        pdfElement.IsSelected = true;

        return pdfElement;
    }

    public async Task DeleteChildAsync(BaseElement child)
    {
        await DatabaseElement.DeleteChildAsync(child.DatabaseElement);

        Children.Remove(child);
    }

    public ObservableCollection<BaseElement> Children { get; }

    private void NotifyTextChanged(object? sender, System.EventArgs e)
    {
        OnPropertyChanged(s_textChangedEventArgs ??= new PropertyChangedEventArgs(nameof(Text)));
    }
}