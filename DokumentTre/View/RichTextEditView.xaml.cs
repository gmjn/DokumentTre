using DokumentTre.ViewModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DokumentTre.View;

public partial class RichTextEditView : Window, IRichTextEditViewModel
{
    private bool _textIsChanged = false;

    public RichTextEditView(MainView owner)
    {
        InitializeComponent();
        Owner = owner;
        Icon = owner.Icon;
        WindowState = owner.WindowState;
        Width = owner.Width;
        Height = owner.Height;
    }

    private void TextBoxName_TextChanged(object sender, TextChangedEventArgs e)
    {
        ButtonOk.IsEnabled = TextBoxName.Text.Length > 0;
    }

    private void RichTextBoxContent_TextChanged(object sender, TextChangedEventArgs e)
    {
        _textIsChanged = true;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
        if (TextBoxName.Text.Length > 0)
        {
            DialogResult = true;
        }
    }

    string IRichTextEditViewModel.Name
    {
        get
        {
            return TextBoxName.Text;
        }
        set
        {
            TextBoxName.Text = value;
        }
    }

    byte[] IRichTextEditViewModel.Text
    {
        get
        {
            using MemoryStream memoryStream = new();
            TextRange textRange = new(RichTextBoxContent.Document.ContentStart, RichTextBoxContent.Document.ContentEnd);
            textRange.Save(memoryStream, DataFormats.XamlPackage);

            return memoryStream.ToArray();
        }
        set
        {
            using MemoryStream memoryStream = new(value);
            TextRange textRange = new(RichTextBoxContent.Document.ContentStart, RichTextBoxContent.Document.ContentEnd);
            textRange.Load(memoryStream, DataFormats.XamlPackage);

            _textIsChanged = false;
        }
    }

    bool IRichTextEditViewModel.TextIsChanged => _textIsChanged;

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_textIsChanged && DialogResult != true)
        {
            MessageBoxResult answer = MessageBox.Show(this, "Er du sikker på at du vil lukke uten å lagre endring?", "Bekreft lukking", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (answer != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            } 
        }
    }
}