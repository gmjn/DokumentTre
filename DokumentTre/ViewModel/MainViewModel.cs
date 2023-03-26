using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentRepository;
using DokumentTre.Model;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DokumentTre.ViewModel;

public class MainViewModel : ObservableObject
{
    public string? DatabaseName { get; set; }

    private readonly IRepository _documentDatabase;
    private BaseElement? _selectedElement;
    private bool _isEnabled = true;

    private readonly Func<IFolderEditViewModel> _getFolderEdit;
    private readonly Func<ITextEditViewModel> _getTextEdit;
    private readonly Func<IRichTextEditViewModel> _getRichTextEdit;
    private readonly Func<IImageEditViewModel> _getImageEdit;

    public IRelayCommand NewButtonCommand { get; }
    public ICommand NewFolderCommand { get; }
    public ICommand NewPlainTextCommand { get; }
    public ICommand NewImageCommand { get; }
    public ICommand NewRichTextCommand { get; }
    public IAsyncRelayCommand EditCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
    public IAsyncRelayCommand VacuumCommand { get; }

    public FolderElement[] DocumentRoot { get; private set; }

    public BaseElement? SelectedElement
    {
        get => _selectedElement;
        set
        {
            if (value != _selectedElement)
            {
                _selectedElement = value;

                NewButtonCommand.NotifyCanExecuteChanged();
                EditCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();

                OnPropertyChanged(nameof(SelectedElement));
            }
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled; 
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
    }

    public MainViewModel(IRepository documentDatabase, Func<IFolderEditViewModel> getFolderEdit, Func<ITextEditViewModel> getTextEdit, Func<IRichTextEditViewModel> getRichTextEdit, Func<IImageEditViewModel> getImageEdit)
    {
        _documentDatabase = documentDatabase;
        _getFolderEdit = getFolderEdit;
        _getTextEdit = getTextEdit;
        _getRichTextEdit = getRichTextEdit;
        _getImageEdit = getImageEdit;
        DocumentRoot = Array.Empty<FolderElement>();

        NewButtonCommand = new RelayCommand(() => { }, () => SelectedElement is FolderElement);
        NewFolderCommand = new AsyncRelayCommand(NewFolder);
        NewPlainTextCommand = new AsyncRelayCommand(NewPlainText);
        NewImageCommand = new AsyncRelayCommand(NewImage);
        NewRichTextCommand = new AsyncRelayCommand(NewRichText);
        EditCommand = new AsyncRelayCommand(Edit, () => SelectedElement is not null);
        DeleteCommand = new AsyncRelayCommand(Delete, () => SelectedElement is not null && SelectedElement.DatabaseElement.Id != 0);
        VacuumCommand = new AsyncRelayCommand(Vacuum);
    }

    public async Task OpenDatabaseAsync()
    {
        if (DatabaseName is null)
        {
            throw new InvalidOperationException("Internal error.\n\nDatabase name is null. Set DatabaseName before call this method.");
        }

        await _documentDatabase.OpenRepositoryAsync(DatabaseName);

        FolderElement rootElement = new(_documentDatabase.RootFolder, null);
        DocumentRoot = new[] { rootElement };
        OnPropertyChanged(nameof(DocumentRoot));
        await rootElement.LoadChildrenAsync(true, FolderIsExpanded_Changed);
        DocumentRoot[0].IsExpanded = true;
    }

    private async void FolderIsExpanded_Changed(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(BaseElement.IsExpanded) && sender is FolderElement folderElement)
            {
                folderElement.PropertyChanged -= FolderIsExpanded_Changed;

                foreach (BaseElement baseElement in folderElement.Children)
                {
                    if (baseElement is FolderElement folderElementSub)
                    {
                        await folderElementSub.LoadChildrenAsync(true, FolderIsExpanded_Changed);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            SendUiMessage?.Invoke(($"Lesing av dokumenter i undermappe feilet. Feilmelding:\n\n{ex.Message}", "Feilmelding", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
        }
    }

    private async Task NewFolder()
    {
        try
        {
            if (SelectedElement is FolderElement folder)
            {
                IFolderEditViewModel folderEdit = _getFolderEdit();
                if (folderEdit.ShowDialog() == true)
                {
                    await folder.AddFolderAsync(folderEdit.Name, folderEdit.Text);
                }
            }
        }
        catch (Exception ex)
        {
            SendUiMessage?.Invoke(($"Ny feilet. Feilmelding:\n\n{ex.Message}", "Feilmelding", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
        }
    }

    private async Task NewPlainText()
    {
        try
        {
            if (SelectedElement is FolderElement folder)
            {
                ITextEditViewModel textEdit = _getTextEdit();
                if (textEdit.ShowDialog() == true)
                {
                    await folder.AddTextAsync(textEdit.Name, textEdit.Text);
                }
            }
        }
        catch (Exception ex)
        {
            SendUiMessage?.Invoke(($"Ny feilet. Feilmelding:\n\n{ex.Message}", "Feilmelding", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
        }
    }

    private async Task NewImage()
    {
        try
        {
            if (SelectedElement is FolderElement folder)
            {
                IImageEditViewModel imageEdit = _getImageEdit();

                if (imageEdit.ShowDialog() == true)
                {
                    if (imageEdit.Image is not null)
                    {
                        await folder.AddImageAsync(imageEdit.Name, imageEdit.Image.Value.Item2, imageEdit.Image.Value.Item1);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            SendUiMessage?.Invoke(($"Ny feilet. Feilmelding:\n\n{ex.Message}", "Feilmelding", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
        }
    }

    private async Task NewRichText()
    {
        try
        {
            if (SelectedElement is FolderElement folder)
            {
                IRichTextEditViewModel richTextEdit = _getRichTextEdit();
                if (richTextEdit.ShowDialog() == true)
                {
                    await folder.AddRichTextAsync(richTextEdit.Name, richTextEdit.Text);
                }
            }
        }
        catch (Exception ex)
        {
            SendUiMessage?.Invoke(($"Ny feilet. Feilmelding:\n\n{ex.Message}", "Feilmelding", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
        }
    }

    private async Task Edit()
    {
        try
        {
            if (SelectedElement is FolderElement folder)
            {
                IFolderEditViewModel folderEdit = _getFolderEdit();
                folderEdit.Name = folder.DatabaseElement.Name;
                string oldText = await folder.GetTextAsync();
                folderEdit.Text = oldText;

                if (folderEdit.ShowDialog() == true)
                {
                    if (folder.DatabaseElement.Name != folderEdit.Name)
                    {
                        await folder.DatabaseElement.SetNameAsync(folderEdit.Name);
                    }

                    if (folderEdit.Text != oldText)
                    {
                        await folder.SetTextAsync(folderEdit.Text);
                    }
                }
            }
            else if (SelectedElement is TextElement text)
            {
                ITextEditViewModel textEdit = _getTextEdit();
                textEdit.Name = text.DatabaseElement.Name;
                string oldText = await text.GetTextAsync();
                textEdit.Text = oldText;

                if (textEdit.ShowDialog() == true)
                {
                    if (text.DatabaseElement.Name != textEdit.Name)
                    {
                        await text.DatabaseElement.SetNameAsync(textEdit.Name);
                    }

                    if (textEdit.Text != oldText)
                    {
                        await text.SetTextAsync(textEdit.Text);
                    }
                }
            }
            else if (SelectedElement is RichTextElement richText)
            {
                IRichTextEditViewModel richTextEdit = _getRichTextEdit();
                richTextEdit.Name = richText.DatabaseElement.Name;
                richTextEdit.Text = await richText.DatabaseElement.GetContentAsync();

                if (richTextEdit.ShowDialog() == true)
                {
                    if (richText.DatabaseElement.Name != richTextEdit.Name)
                    {
                        await richText.DatabaseElement.SetNameAsync(richTextEdit.Name);
                    }

                    if (richTextEdit.TextIsChanged)
                    {
                        await richText.DatabaseElement.SetContentAsync(richTextEdit.Text);
                    }
                }
            }
            else if (SelectedElement is ImageElement image)
            {
                IImageEditViewModel imageEdit = _getImageEdit();
                imageEdit.Name = image.DatabaseElement.Name;
                imageEdit.Image = await Task.Run(() => image.Image);
                DocumentTypes oldDocumentType = imageEdit.Image.Value.Item2;

                if (imageEdit.ShowDialog() == true)
                {
                    if (image.DatabaseElement.Name != imageEdit.Name)
                    {
                        await image.DatabaseElement.SetNameAsync(imageEdit.Name);
                    }

                    if (imageEdit.ImageIsChanged && imageEdit.Image.HasValue)
                    {
                        if (oldDocumentType != imageEdit.Image.Value.Item2)
                        {
                            await image.DatabaseElement.SetDocumentTypeAsync(imageEdit.Image.Value.Item2);
                        }

                        await image.DatabaseElement.SetContentAsync(imageEdit.Image.Value.Item1);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            SendUiMessage?.Invoke(($"Endring feilet. Feilmelding:\n\n{ex.Message}", "Feilmelding", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
        }
    }

    private async Task Delete()
    {
        const string text = "Er du sikker på at du fil slette valgt dokument.";
        const string caption = "Bekreft sletting";
        const MessageBoxButton button = MessageBoxButton.YesNo;
        const MessageBoxImage image = MessageBoxImage.Warning;
        const MessageBoxResult defaultResult = MessageBoxResult.No;

        if (SendUiMessage?.Invoke((text, caption, button, image, defaultResult)) == MessageBoxResult.Yes)
        {
            try
            {
                if (SelectedElement is not null && SelectedElement.Parent is not null)
                {
                    await SelectedElement.Parent.DeleteChildAsync(SelectedElement);
                }
            }
            catch(Exception ex)
            {
                SendUiMessage?.Invoke(($"Sletting feilet. Feilmelding:\n\n{ex.Message}", "Feilmelding", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
            }
        }
    }

    private async Task Vacuum()
    {
        try
        {
            IsEnabled = false;
            await _documentDatabase.VacuumAsync();
        }
        catch (Exception ex)
        {
            SendUiMessage?.Invoke(($"Database rens feilet. Feilmelding:\n\n{ex.Message}", "Feilmelding", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
        }
        finally
        {
            IsEnabled = true;
        }

    }

    public event Func<(string text, string caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult defaultButton), MessageBoxResult>? SendUiMessage;
}