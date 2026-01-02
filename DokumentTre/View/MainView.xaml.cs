using DokumentTre.Model;
using DokumentTre.ViewModel;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace DokumentTre.View;

public sealed partial class MainView : Window
{
    private readonly MainViewModel _viewModel;

    public MainView(MainViewModel viewModel)
    {
        InitializeComponent();

        Width = Math.Floor(SystemParameters.PrimaryScreenWidth * 90 / 100);
        Height = Math.Floor(SystemParameters.PrimaryScreenHeight * 90 / 100);

        _viewModel = viewModel;
        DataContext = _viewModel;
        viewModel.SendUiMessage += UiMessage;
    }

    private async void Window_LoadedAsync(object sender, RoutedEventArgs e)
    {
        if (_viewModel.DatabaseName is null)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Åpne eller opprett ny database",
                Filter = "Dokument tre database (*.dokt)|*.dokt",
                CheckFileExists = false,
                CheckPathExists = true
            };

            if (openFileDialog.ShowDialog(this) != true)
            {
                Close();
                return;
            }

            _viewModel.DatabaseName = openFileDialog.FileName;
        }

        try
        {
            Title = $"{Title}  –  {Path.GetFileName(_viewModel.DatabaseName)}";
            await _viewModel.OpenDatabaseAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Åpning av dokument \"{_viewModel.DatabaseName}\" feilet. Feil melding:\n\n{ex.Message}", "Feilmelding", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }
    }

    private void TreeViewDocumentTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _viewModel.SelectedElement = TreeViewDocumentTree.SelectedItem as BaseElement;
    }

    private MessageBoxResult UiMessage((string text, string caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult defaultButton) arg)
    {
        return MessageBox.Show(this, arg.text, arg.caption, arg.button, arg.image, arg.defaultButton);
    }
}