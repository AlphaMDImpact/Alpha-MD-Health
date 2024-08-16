using System.Windows.Input;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Controls;
using WebView = Microsoft.Maui.Controls.WebView;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents custom video cell
    /// </summary>
    public  class CustomWebView : WebView
    {

        private static  DXPopup _popupPage ;
        /// <summary>
        /// file name
        /// </summary>
        public static readonly BindableProperty FileNameProperty = BindableProperty.Create(nameof(FileName), typeof(string), typeof(CustomWebView), string.Empty);

        /// <summary>
        /// file name
        /// </summary>
        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set => SetValue(FileNameProperty, value);
        }
        /// <summary>
        /// file name
        /// </summary>
        public static readonly BindableProperty PDFLinkNameProperty = BindableProperty.Create(nameof(PDFLinkName), typeof(string), typeof(CustomWebView), string.Empty);

        /// <summary>
        /// file name
        /// </summary>
        public string PDFLinkName
        {
            get => (string)GetValue(PDFLinkNameProperty);
            set => SetValue(PDFLinkNameProperty, value);
        }
        /// <summary>
        /// URL click failer event
        /// </summary>
        public event EventHandler URLClickedFailer;

        /// <summary>
        /// Web view load complete event
        /// </summary>
        public event EventHandler WebviewLoadedCompleted;
        /// <summary>
        /// Is Under Line Property with default value as false
        /// </summary>
        public static readonly BindableProperty IsAutoIncreaseHeightProperty = BindableProperty.Create(nameof(IsAutoIncreaseHeight), typeof(bool), typeof(CustomWebView), false);
        /// <summary>
        /// show busy indicator Property with default value as false
        /// </summary>
        public static readonly BindableProperty ShowBusyIndicatorProperty = BindableProperty.Create(nameof(ShowBusyIndicator), typeof(bool), typeof(CustomWebView), false);

        /// <summary>
        /// Property to allow opening of links in browser instead of within webview
        /// </summary>
        public static readonly BindableProperty IsShouldOpenLinksInBrowserProperty = BindableProperty.Create(nameof(ShouldOpenLinksInBrowser), typeof(bool), typeof(CustomWebView), false);

        /// <summary>
        /// Format from settings is assigned to this html source property in the formatted webview
        /// </summary>
        public static readonly BindableProperty HtmlDataProperty = BindableProperty.Create(nameof(HtmlDataSource), typeof(string), typeof(CustomWebView), string.Empty);

        /// <summary>
        /// Format from settings is assigned to this html source property in the formatted webview
        /// </summary>
        public string HtmlDataSource
        {
            get => (string)GetValue(HtmlDataProperty);
            set => SetValue(HtmlDataProperty, value);
        }


        //full screen kumkum
        /// <summary>
        /// Bindable property for <see cref="EnterFullScreenCommand"/>.
        /// </summary>
        public static readonly BindableProperty EnterFullScreenCommandProperty =
            BindableProperty.Create(
                nameof(EnterFullScreenCommand),
                typeof(ICommand),
                typeof(CustomWebView),
                defaultValue: new Command( (view) =>  DefaultEnterAsync((View)view)));

        /// <summary>
        /// Bindable property for <see cref="ExitFullScreenCommand"/>.
        /// </summary>
        public static readonly BindableProperty ExitFullScreenCommandProperty =
            BindableProperty.Create(
                nameof(ExitFullScreenCommand),
                typeof(ICommand),
                typeof(CustomWebView),
                defaultValue: new Command( (view) =>DefaultExitAsync()));

        /// <summary>
        /// Gets or sets the command executed when the web view content requests entering full-screen.
        /// The command is passed a <see cref="View"/> containing the content to display.
        /// The default command displays the content as a modal page.
        /// </summary>
        public ICommand EnterFullScreenCommand
        {
            get => (ICommand)GetValue(EnterFullScreenCommandProperty);
            set => SetValue(EnterFullScreenCommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the command executed when the web view content requests exiting full-screen.
        /// The command is passed no parameters.
        /// The default command pops a modal page off the navigation stack.
        /// </summary>
        public ICommand ExitFullScreenCommand
        {
            get => (ICommand)GetValue(ExitFullScreenCommandProperty);
            set => SetValue(ExitFullScreenCommandProperty, value);
        }

        private static void DefaultEnterAsync(View view)
        {
            //await PopupNavigation.Instance.PopAsync();
            _popupPage = new DXPopup()
            {
                Content = view,
                IsOpen = false
            };

            _popupPage.IsOpen = true;
            //if (PopupNavigation.Instance.PopupStack?.Count > 0)
            //{
            //   foreach(var item in PopupNavigation.Instance.PopupStack)
            //    {

            //        item.IsVisible = false;
            //    }
            //}
            //await PopupNavigation.Instance.PushAsync(page);
            //  await Application.Current.MainPage.Navigation.PushModalAsync(page).ConfigureAwait(true);
        }

        private static void DefaultExitAsync()
        {
            //if (PopupNavigation.Instance.PopupStack?.Count > 0)
            //{
            //    foreach (var item in PopupNavigation.Instance.PopupStack)
            //    {
            //        item.IsVisible = true;
            //    }
            //}

            //await PopupNavigation.Instance.PopAsync();

            _popupPage.IsOpen = false;
            //await MauiPopup.PopupAction.ClosePopup(_nameOfPage);
        }

        /// <summary>
        /// Is auto increase height property
        /// </summary>
        public bool IsAutoIncreaseHeight
        {
            get => (bool)GetValue(IsAutoIncreaseHeightProperty);
            set => SetValue(IsAutoIncreaseHeightProperty, value);
        }

        /// <summary>
        /// show busy indicator property
        /// </summary>
        public bool ShowBusyIndicator
        {
            get => (bool)GetValue(ShowBusyIndicatorProperty);
            set => SetValue(ShowBusyIndicatorProperty, value);
        }

        /// <summary>
        /// Should allow opening of links in browser
        /// </summary>
        public bool ShouldOpenLinksInBrowser
        {
            get => (bool)GetValue(IsShouldOpenLinksInBrowserProperty);
            set => SetValue(IsShouldOpenLinksInBrowserProperty, value);
        }

        /// <summary>
        /// Invoke url click failer method
        /// </summary>
        /// <param name="errprReason">error reason data</param>
        /// <returns></returns>
        public void InvokeUrlClickedFailer(object errprReason)
        {
            URLClickedFailer?.Invoke(errprReason, new EventArgs());
        }

        /// <summary>
        /// Invoke web view load complete
        /// </summary>
        /// <param name="webviewLoadedHeight">web view height data</param>
        /// <returns></returns>
        public void InvokeWebviewLoadedCompleted(object webviewLoadedHeight)
        {
            WebviewLoadedCompleted?.Invoke(webviewLoadedHeight, new EventArgs());
        }

        /// <summary>
        /// Adds default font style to HTML content
        /// </summary>
        /// <param name="htmlString">HTML string</param>
        /// <returns>HTML string with default font style</returns>
        public string AddFontStyleToHtmlString(string htmlString)
        {
            if (htmlString.Contains("font-family"))
            {
                return htmlString;
            }
            return Constants.DEFAULT_FONT_HTML.Replace("{0}", htmlString);
        }

    }

    /// <summary>
    /// Represents pdf web view data
    /// </summary>
    public class PdfWebView : WebView
    {
        /// <summary>
        /// file name
        /// </summary>
        public static readonly BindableProperty FileNameProperty = BindableProperty.Create(nameof(FileName), typeof(string), typeof(PdfWebView), string.Empty);

        /// <summary>
        /// file name
        /// </summary>
        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set => SetValue(FileNameProperty, value);
        }
    }
}