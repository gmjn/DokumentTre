﻿using DokumentTre.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace DokumentTre.View;

public partial class FolderEditView : Window, IFolderEditViewModel
{
    private bool _textIsChanged = false;

    public FolderEditView(MainView owner)
    {
        InitializeComponent();
        Owner = owner;
        Icon = owner.Icon;
    }

    private void TextBoxName_TextChanged(object sender, TextChangedEventArgs e)
    {
        ButtonOk.IsEnabled = TextBoxName.Text.Length > 0;
    }

    private void TextBoxContent_TextChanged(object sender, TextChangedEventArgs e)
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

    string IFolderEditViewModel.Name
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

    string IFolderEditViewModel.Text
    {
        get
        {
            return TextBoxContent.Text;
        }
        set
        {
            TextBoxContent.Text = value;
            _textIsChanged = false;
        }
    }

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