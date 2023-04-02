namespace DokumentTre.ViewModel;

public interface IPdfEditViewModel
{
    public string Name { get; set; }
    public byte[]? PdfData { get; set; }
    public bool PdfIsChanged { get; }

    public bool? ShowDialog();
}