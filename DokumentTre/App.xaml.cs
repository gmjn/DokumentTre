using DocumentRepository;
using DokumentTre.Factory;
using DokumentTre.View;
using DokumentTre.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DokumentTre;

public sealed partial class App : Application
{
    public ServiceProvider ServiceProvider { get; private set; }

    public App()
    {
        ServiceCollection services = new();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<IRepository, Repository>();

        services.AddSingleton<MainView>();
        services.AddSingleton<MainViewModel>();

        services.AddDiFactory(); // Fører til at alle registrerte klasser kan opprettes gjennom IFactory og bruk av metode Create<T>()

        services.AddTransient<IFolderEditViewModel, FolderEditView>();
        services.AddTransient<ITextEditViewModel, TextEditView>();
        services.AddTransient<IRichTextEditViewModel, RichTextEditView>();
        services.AddTransient<IImageEditViewModel, ImageEditView>();
        services.AddTransient<IHtmlEditViewModel, HtmlEditView>();
        services.AddTransient<IPdfEditViewModel, PdfEditView>();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        if (e.Args.Length > 0)
        {
            ServiceProvider.GetRequiredService<MainViewModel>().DatabaseName = e.Args[0];
        }

        ServiceProvider.GetRequiredService<MainView>().Show();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        ServiceProvider.Dispose();
    }
}