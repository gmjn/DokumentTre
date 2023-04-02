using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.IO;
using System.Windows;

namespace DokumentTre.View.Converters;

// Hjelper for å kunne legge til Pdf som attatched property.
// Dette er et property som det kan bindes til. 
public class WebView2PdfHelper : DependencyObject
{
    private static int s_counter = 0;
    private static byte[] s_data = Array.Empty<byte>();

    public static byte[] GetPdfDocument(UIElement target)
    {
        return (byte[])target.GetValue(PdfDocumentProperty);
    }

    // Feil type på value, burde vært byte[], men får da feilmelding:
    // MC4102 Tags of type 'PropertyArrayStart' are not supported in template sections.
    // når jeg binder til property. Bruker derfor object.
    public static void SetPdfDocument(UIElement target, object value)
    {
        target.SetValue(PdfDocumentProperty, value);
    }

    public static readonly DependencyProperty PdfDocumentProperty = DependencyProperty.RegisterAttached(
            "PdfDocument",
            typeof(byte[]),
            typeof(WebView2PdfHelper),
            new FrameworkPropertyMetadata { PropertyChangedCallback = PdfDocumentPropertyChanged });


    private async static void PdfDocumentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is WebView2 webView)
        {
            byte[] data = GetPdfDocument(webView);

            if (data is not null)
            {
                if (webView.CoreWebView2 is null)
                {
                    await webView.EnsureCoreWebView2Async();

                    if (webView.CoreWebView2 is not null)
                    {
                        webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
                        webView.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
                    }
                }

                if (webView.CoreWebView2 is not null && webView.CoreWebView2.Settings.AreDevToolsEnabled)
                {
                    webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                }

                s_data = data;
                webView.Source = new System.Uri($"file://{++s_counter}.pdf");                
            }
            else
            {
                s_data = Array.Empty<byte>();
                webView.NavigateToString("");
            }
        }
    }

    private static void CoreWebView2_WebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
        if (sender is CoreWebView2 webView)
        {
            MemoryStream ms = new(s_data);

            CoreWebView2Environment environment = webView.Environment;
            e.Response = environment.CreateWebResourceResponse(ms, 200, "OK", "");
            e.Response.Headers.AppendHeader("Content-Type", "application/pdf");
        }
    }
}