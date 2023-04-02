using DocumentRepository;
using DokumentTre.View;
using DokumentTre.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace DokumentTre;

public partial class App : Application
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

        services.AddTransient<IFolderEditViewModel, FolderEditView>();
        services.AddTransient<ITextEditViewModel, TextEditView>();
        services.AddTransient<IRichTextEditViewModel, RichTextEditView>();
        services.AddTransient<IImageEditViewModel, ImageEditView>();
        services.AddTransient<IHtmlEditViewModel, HtmlEditView>();

        // Windows factory
        services.AddSingleton<Func<IFolderEditViewModel>>(x => () => x.GetRequiredService<IFolderEditViewModel>());
        services.AddSingleton<Func<ITextEditViewModel>>(x => () => x.GetRequiredService<ITextEditViewModel>());
        services.AddSingleton<Func<IRichTextEditViewModel>>(x => () => x.GetRequiredService<IRichTextEditViewModel>());
        services.AddSingleton<Func<IImageEditViewModel>>(x => () => x.GetRequiredService<IImageEditViewModel>());
        services.AddSingleton<Func<IHtmlEditViewModel>>(x => () => x.GetRequiredService<IHtmlEditViewModel>());
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