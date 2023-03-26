using DocumentRepository;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;

namespace DokumentTre.Model;

public abstract class BaseElement : INotifyPropertyChanged
{
    private static PropertyChangedEventArgs? s_isExpandedChangedEventArgs;
    private static PropertyChangedEventArgs? s_isSelectedChangedEventArgs;

    public BaseElement(IRepositoryItem databaseElement, FolderElement? parent)
    {
        DatabaseElement = databaseElement;
        Parent = parent;
    }

    public IRepositoryItem DatabaseElement { get; }

    public FolderElement? Parent { get; }

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                PropertyChanged?.Invoke(this, s_isExpandedChangedEventArgs ??= new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, s_isSelectedChangedEventArgs ??= new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }

    protected void OnPropertyChanged(PropertyChangedEventArgs args)
    {
        PropertyChanged?.Invoke(this, args);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected static byte[] EncodeString(string text)
    {
        if (text.Length == 0)
        {
            return Array.Empty<byte>();
        }

        using MemoryStream compressedStream = new();
        using (BrotliStream brotliStream = new(compressedStream, CompressionLevel.Optimal, true))
        {
            using StreamWriter streamWriter = new StreamWriter(brotliStream, leaveOpen: true);
            streamWriter.Write(text);
        }

        return compressedStream.ToArray();
    }

    protected static string DecodeString(byte[] data)
    {
        if (data.Length == 0)
        {
            return string.Empty;
        }

        using MemoryStream compressedStream = new(data, false);
        using BrotliStream brotliStream = new(compressedStream, CompressionMode.Decompress, true);
        using StreamReader streamReader = new(brotliStream, detectEncodingFromByteOrderMarks: false, leaveOpen: true);

        return streamReader.ReadToEnd();
    }
}