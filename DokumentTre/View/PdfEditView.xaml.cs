using DokumentTre.ViewModel;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DokumentTre.View;


public sealed partial class PdfEditView : Window, IPdfEditViewModel, INotifyPropertyChanged
{
    byte[]? _pdfData;
    private bool _pdfIsChanged = false;

    public PdfEditView(MainView owner)
    {
        InitializeComponent();
        Owner = owner;
        Icon = owner.Icon;
        WindowState = owner.WindowState;
        Width = owner.Width;
        Height = owner.Height;

        DataContext = this;
    }

    private void TextBoxName_TextChanged(object sender, TextChangedEventArgs e)
    {
        ButtonOk.IsEnabled = TextBoxName.Text.Length > 0 && _pdfData != null;
    }

    private async void ButtonOpen_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Åpne pdf",
                Filter = "Pdf filer (*.pdf)|*.pdf"
            };

            if (openFileDialog.ShowDialog(this) == true)
            {
                PdfData = await File.ReadAllBytesAsync(openFileDialog.FileName);
                _pdfIsChanged = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PdfData)));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Åpning av pdf feilet. Feilmelding:\n\n{ex.Message}", "Feilmelding", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
        if (TextBoxName.Text.Length > 0 && _pdfData != null)
        {
            DialogResult = true;
        }
    }

    string IPdfEditViewModel.Name
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

    public byte[]? PdfData
    {
        get => _pdfData;
        set
        {
            _pdfData = value;
            ButtonOk.IsEnabled = TextBoxName.Text.Length > 0 && _pdfData != null;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PdfData)));
        }
    }

    bool IPdfEditViewModel.PdfIsChanged => _pdfIsChanged;

    public event PropertyChangedEventHandler? PropertyChanged;
}