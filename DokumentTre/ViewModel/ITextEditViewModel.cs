namespace DokumentTre.ViewModel;

public interface ITextEditViewModel
{
    public string Name { get; set; }
    public string Text { get; set; }

    public bool? ShowDialog();
}