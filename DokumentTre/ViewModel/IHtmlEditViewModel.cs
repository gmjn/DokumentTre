namespace DokumentTre.ViewModel;

public interface IHtmlEditViewModel
{
    public string Name { get; set; }
    public string Html { get; set; }

    public bool? ShowDialog();
}