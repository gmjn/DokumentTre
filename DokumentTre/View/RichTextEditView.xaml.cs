using DokumentTre.ViewModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        if (_textIsChanged)
        {
            foreach (Block block in RichTextBoxContent.Document.Blocks)
            {
                if(block is Paragraph paragraph)
                {
                    foreach (Inline inline in paragraph.Inlines)
                    {
                        if (inline is InlineUIContainer uIContainer)
                        {
                            if (uIContainer.Child is Image image)
                            {
                                CheckImage(image);
                            }
                        }
                    }
                }
                else if (block is BlockUIContainer uIContainer)
                {
                    if (uIContainer.Child is Image image)
                    {
                        CheckImage(image);
                    }
                }
            }
        }

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

    private void CheckImage(Image image)
    {
        try
        {
            if (image.Source.ToString().Contains("bmp"))
            {
                int width = (int)image.Width;
                int height = (int)image.Height;

                RenderTargetBitmap rtBmp = new(width, height, 96.0, 96.0, PixelFormats.Pbgra32);

                image.Measure(new Size(width, height));
                image.Arrange(new Rect(new Size(width, height)));

                rtBmp.Render(image);

                PngBitmapEncoder encoder = new();
                using MemoryStream memoryStream = new();
                encoder.Frames.Add(BitmapFrame.Create(rtBmp));

                encoder.Save(memoryStream);

                image.Source = new PngBitmapDecoder(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];

                _textIsChanged = true;
            }
        }
        catch { }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_textIsChanged && DialogResult != true)
        {
            MessageBoxResult answer = MessageBox.Show(this, "Er du sikker på at du vil lukke uten å lagre endring?", "Bekreft lukking", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);

            if (answer != MessageBoxResult.OK)
            {
                e.Cancel = true;
            } 
        }
    }

    private void ButtonStrikeThrough_Click(object sender, RoutedEventArgs e)
    {
        if (RichTextBoxContent.Selection is not null)
        {
            TextRange textRange = RichTextBoxContent.Selection;
            var currentTextDecoration = textRange.GetPropertyValue(Inline.TextDecorationsProperty);

            if (currentTextDecoration != DependencyProperty.UnsetValue)
            {
                textRange.ApplyPropertyValue(Inline.TextDecorationsProperty, ((TextDecorationCollection)currentTextDecoration == TextDecorations.Strikethrough) ? new TextDecorationCollection() : TextDecorations.Strikethrough);
            }
            else
            {
                textRange.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
            }
        }
    }

    private void Button_Font_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.Tag is string tag)
            {
                if (RichTextBoxContent.Selection is not null)
                {
                    switch (tag)
                    {
                        case "N":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, "Calibri");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontSizeProperty, 14.666666666666666);
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontWeightProperty, "Normal");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStretchProperty, "Normal");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStyleProperty, "Normal");
                            break;
                        case "H1":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, "Calibri");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontSizeProperty, 34.0);
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontWeightProperty, "Normal");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStretchProperty, "Normal");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStyleProperty, "Normal");
                            break;
                        case "H2":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, "Calibri");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontSizeProperty, 28.0);
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontWeightProperty, "Normal");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStretchProperty, "Normal");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStyleProperty, "Normal");
                            break;
                        case "H3":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, "Calibri");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontSizeProperty, 24.0);
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontWeightProperty, "Normal");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStretchProperty, "Normal");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStyleProperty, "Normal");
                            break;
                        case "H4":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, "Calibri");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontSizeProperty, 20.0);
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontWeightProperty, "Normal");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStretchProperty, "Normal");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStyleProperty, "Normal");
                            break;
                        case "C":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, "Cascadia Mono");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontSizeProperty, 12.6666666);
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontWeightProperty, "Normal");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStretchProperty, "Normal");
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStyleProperty, "Normal");
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    private void Button_Color_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.Tag is string tag)
            {
                if (RichTextBoxContent.Selection is not null)
                {
                    switch (tag)
                    {
                        case "C1":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.ForegroundProperty, "Black");
                            break;
                        case "C2":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.ForegroundProperty, "Red");
                            break;
                        case "C3":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.ForegroundProperty, "#FFB62515");
                            break;
                        case "C4":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.ForegroundProperty, "#FF008000");
                            break;
                        case "C5":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.ForegroundProperty, "Blue");
                            break;
                        case "C6":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.ForegroundProperty, "#FF2B91AF");
                            break;
                        case "C7":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.ForegroundProperty, "#FFFFC000");
                            break;
                        case "C8":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.ForegroundProperty, "#FF74531F");
                            break;
                        case "C9":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.ForegroundProperty, "#FF8F08C4");
                            break;
                        case "B1":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.BackgroundProperty, "Yellow");
                            break;
                        case "B2":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.BackgroundProperty, "#FF12EC12");
                            break;
                        case "B3":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.BackgroundProperty, "#FF34BDE6");
                            break;
                        case "B4":
                            RichTextBoxContent.Selection.ApplyPropertyValue(Inline.BackgroundProperty, "White");
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}