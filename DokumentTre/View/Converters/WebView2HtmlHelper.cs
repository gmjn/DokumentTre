using Microsoft.Web.WebView2.Wpf;
using System.Windows;

namespace DokumentTre.View.Converters;

// Hjelper for å kunne legge til HTML som attatched property.
// Dette er et property som det kan bindes til.
public sealed class WebView2HtmlHelper : DependencyObject
{
    public static string GetHtmlDocument(UIElement target)
    {
        return (string)target.GetValue(HtmlDocumentProperty);
    }

    public static void SetHtmlDocument(UIElement target, string value)
    {
        target.SetValue(HtmlDocumentProperty, value);
    }

    public static readonly DependencyProperty HtmlDocumentProperty = DependencyProperty.RegisterAttached(
            "HtmlDocument",
            typeof(string),
            typeof(WebView2HtmlHelper),
            new FrameworkPropertyMetadata { PropertyChangedCallback = HtmlDocumentPropertyChanged });

    private async static void HtmlDocumentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is WebView2 webView2)
        {
            string data = GetHtmlDocument(webView2);

            if (data is not null)
            {
                if (webView2.CoreWebView2 is null)
                {
                    await webView2.EnsureCoreWebView2Async();
                }

                if (webView2.CoreWebView2 is not null && webView2.CoreWebView2.Settings.AreDevToolsEnabled)
                {
                    webView2.CoreWebView2.Settings.AreDevToolsEnabled = false;
                }

                webView2.NavigateToString(data);
            }
            else
            {
                webView2.NavigateToString("");
            }
        }
    }
}