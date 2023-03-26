namespace DokumentTre.ViewModel;

public interface IRichTextEditViewModel
{
    public string Name { get; set; }
    public byte[] Text { get; set; }
    public bool TextIsChanged { get; }

    public bool? ShowDialog();
}