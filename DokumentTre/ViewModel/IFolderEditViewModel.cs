namespace DokumentTre.ViewModel;

public interface IFolderEditViewModel
{
    public string Name { get; set; }
    public string Text { get; set; }

    public bool? ShowDialog();
}