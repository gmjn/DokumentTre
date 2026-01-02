using DocumentRepository;
using DokumentTre.ViewModel;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DokumentTre.View;

public sealed partial class ImageEditView : Window, IImageEditViewModel, INotifyPropertyChanged
{
    private (byte[] data, DocumentTypes type)? _image;
    private bool _imageIsChanged = false;

    public ImageEditView(MainView owner)
    {
        InitializeComponent();
        Owner = owner;
        Icon = owner.Icon;
        DataContext = this;
    }

    private void TextBoxName_TextChanged(object sender, TextChangedEventArgs e)
    {
        ButtonOk.IsEnabled = TextBoxName.Text.Length > 0 && _image != null;
    }

    private async void ButtonOpen_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Åpne bilde",
                Filter = "Bilde filer (*.jpg;*.png)|*.jpg;*.png"
            };

            if (openFileDialog.ShowDialog(this) == true)
            {
                DocumentTypes documentType = Path.GetExtension(openFileDialog.FileName).ToLower() switch
                {
                    ".jpg" => DocumentTypes.JpgImage,
                    ".png" => DocumentTypes.PngImage,
                    _ => throw new InvalidOperationException("Error file extension.")
                };

                Image = (await File.ReadAllBytesAsync(openFileDialog.FileName), documentType);
                ButtonSave.IsEnabled = true;
                _imageIsChanged = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
            }
        }
        catch(Exception ex)
        {
            MessageBox.Show(this, $"Åpning av bilde feilet. Feilmelding:\n\n{ex.Message}", "Feilmelding", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        if (_image.HasValue)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Title = "Lagre bilde",
                Filter = _image.Value.type switch
                {
                    DocumentTypes.JpgImage => "Bilde fil (*.jpg)|*.jpg",
                    DocumentTypes.PngImage => "Bilde fil (*.png)|*.png",
                    _ => throw new InvalidOperationException("Internal error. Not image file."),
                }
            };

            try
            {
                if (saveFileDialog.ShowDialog(this) == true)
                {
                    await File.WriteAllBytesAsync(saveFileDialog.FileName, _image.Value.data);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Lagring av bilde feilet. Feilmelding:\n\n{ex.Message}", "Feilmelding", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
        if (TextBoxName.Text.Length > 0)
        {
            DialogResult = true;
        }
    }

    string IImageEditViewModel.Name
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

    public (byte[] data, DocumentTypes type)? Image
    {
        get
        {
            if (ChecBoxSkalerOpp.IsChecked == true && _image is not null)
            {
                var eksternType = _image.Value.type switch
                {
                    DocumentTypes.JpgImage => DocumentTypes.JpgImageScaleBoth,
                    DocumentTypes.PngImage => DocumentTypes.PngImageScaleBoth,
                    _ => _image.Value.type
                };

                return (_image.Value.data, eksternType);
            }
            else
            {
                return _image;
            }
        }
        set
        {
            _image = value;

            if (_image.HasValue && (_image.Value.type == DocumentTypes.PngImageScaleBoth || _image.Value.type == DocumentTypes.JpgImageScaleBoth))
            {
                ChecBoxSkalerOpp.IsChecked = true;
                var internType = _image.Value.type switch
                {
                    DocumentTypes.JpgImageScaleBoth => DocumentTypes.JpgImage,
                    DocumentTypes.PngImageScaleBoth => DocumentTypes.PngImage,
                    _ => DocumentTypes.JpgImage
                };

                _image = (_image.Value.data, internType);
            }
            else
            {
                _image = value;
            }

            ButtonOk.IsEnabled = TextBoxName.Text.Length > 0 && _image != null;
            ButtonSave.IsEnabled = _image != null;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
        }
    }

    bool IImageEditViewModel.ImageIsChanged => _imageIsChanged;

    public event PropertyChangedEventHandler? PropertyChanged;
}