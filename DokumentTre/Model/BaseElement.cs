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

    public bool IsExpanded
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, s_isExpandedChangedEventArgs ??= new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }
    }

    public bool IsSelected
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
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
            return [];
        }

        using MemoryStream compressedStream = new();
        using (BrotliStream brotliStream = new(compressedStream, CompressionLevel.Optimal, true))
        {
            using StreamWriter streamWriter = new(brotliStream, leaveOpen: true);
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