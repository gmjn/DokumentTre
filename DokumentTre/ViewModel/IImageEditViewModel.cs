using DocumentRepository;

namespace DokumentTre.ViewModel;

public interface IImageEditViewModel
{
    public string Name { get; set; }
    public (byte[] data, DocumentTypes type)? Image { get; set; }
    public bool ImageIsChanged { get; }

    public bool? ShowDialog();
}