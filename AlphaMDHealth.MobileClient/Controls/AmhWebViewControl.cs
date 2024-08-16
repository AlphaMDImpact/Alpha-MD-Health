using AlphaMDHealth.Utility;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Web view control
/// </summary>
internal class AmhWebViewControls : AmhBaseControl
{
    private Uri? _mediaSource;
    private MediaElement _mediaElement;
    private DeviceOrientationService _deviceOrientationService;
    private Grid _videoGrid;
    private AmhImageControl _image;
    private WebView _webView;

    private string _value;
    /// <summary>
    /// Control value as bool
    /// </summary>
    internal string Value
    {
        get
        {
            return GetControlValue();
        }
        set
        {
            if (_value != value)
            {
                _value = value;
                SetControlValue();
            }
        }
    }

    /// <summary>
    /// value property
    /// </summary>
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(AmhWebViewControls), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhWebViewControls control = (AmhWebViewControls)bindable;
        if (newValue != null)
        {
            control.Value = (string)newValue;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhWebViewControls() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhWebViewControls(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    private string GetControlValue()
    {
        return _value;
    }

    private void SetControlValue()
    {
        switch (_fieldType)
        {
            case FieldTypes.ImageWebviewControl:
                _image.Icon = _value;
                break;
            case FieldTypes.UrlPageWebViewControl:
                SetUrlPageWebViewControlValue();
                break;
            case FieldTypes.UrlWebviewControl:
                SetUrlWebviewControlValue();
                break;
            case FieldTypes.YoutubeUrlWebviewControl:
                SetYoutubeUrlWebviewControlValue();
                break;
            case FieldTypes.TextWebviewControl:
                SetTextWebviewControlValue();
                break;
            case FieldTypes.HtmlWebviewControl:
            default:
                SetHtmlWebviewControlValue();
                break;
        }
    }

    internal void UnloadUIAsync()
    {
        if (_mediaElement != null)
        {
            _mediaElement.Stop();
        }
    }

    protected override void ApplyResourceValue()
    {
    }

    protected override void EnabledDisableField(bool value)
    {
    }

    protected override void RenderControl()
    {
        switch (_fieldType)
        {
            case FieldTypes.ImageWebviewControl:
                CreateImageWebViewControl();
                break;
            case FieldTypes.YoutubeUrlWebviewControl:
            case FieldTypes.UrlWebviewControl:
                CreateVideoWebViewControl();
                break;
            case FieldTypes.UrlPageWebViewControl:
            case FieldTypes.TextWebviewControl:
            case FieldTypes.HtmlWebviewControl:
            default:
                CreateWebViewControl();
                break;
        }
    }

    // Render Videos UI
    private void CreateVideoWebViewControl()
    {
        _videoGrid = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_VIDEO_GRID]
        };

        _videoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _videoGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        _mediaElement = new MediaElement();
        _mediaElement.Style = (Style)Application.Current.Resources[StyleConstants.ST_MEDIA_STYLE];

        _mediaElement.SetBinding(MediaElement.SourceProperty, new Binding("MediaSource"));

        var btnFullScreen = new ImageButton();
        btnFullScreen.Style = (Style)Application.Current.Resources[StyleConstants.ST_IMAGE_BUTTON];

        btnFullScreen.Clicked += OnFullScreen_Clicked;
        btnFullScreen.Behaviors.Add(new IconTintColorBehavior { TintColor = Color.Parse(StyleConstants.GENERIC_BACKGROUND_COLOR) });

        _videoGrid.Children.Add(_mediaElement);
        _videoGrid.Children.Add(btnFullScreen);

        Grid.SetRow(_mediaElement, 1);
        Grid.SetRow(btnFullScreen, 1);
        Grid.SetColumn(btnFullScreen, 1);

        Content = _videoGrid;

        _deviceOrientationService = new DeviceOrientationService();
        //todo: WeakReferenceMessenger.Default.Register<NotifyFullScreenClosed>(this, HandleOrientation);

    }

    private void CreateImageWebViewControl()
    {
        _image = new AmhImageControl(FieldTypes.ImageControl);
        Content = _image;
    }

    // Render UI Of case UrlPageWebViewControl, TextWebviewControl, HtmlWebviewControl
    private void CreateWebViewControl()
    {
        _webView = new WebView();
        Content = _webView;
    }

    // Custom Renderer for Videos
    private async void SetYoutubeUrlWebviewControlValue()
    {
        _mediaSource = await CreateMediaStreamForYoutube(_value);
        if (_mediaSource != null)
        {
            _mediaElement.Source = _mediaSource;
            _videoGrid.IsVisible = true; // Show the video grid
            Content = _videoGrid;
        }
    }

    //todo: 
    //private void HandleOrientation(object recipient, NotifyFullScreenClosed message)
    //{
    //    _deviceOrientationService = new DeviceOrientationService();
    //    _deviceOrientationService.SetDeviceOrientation(DisplayOrientation.Portrait);
    //}

    private async Task<Uri?> CreateMediaStreamForYoutube(string videoId)
    {
        var client = new YoutubeClient();

        var streamManifest = await client.Videos.Streams.GetManifestAsync(videoId);
        var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

        if (streamInfo != null)
            return new Uri(streamInfo.Url);
        else
            return null;
    }

    private async void OnFullScreen_Clicked(object sender, EventArgs e)
    {
        if (_mediaSource != null)
        {
            _mediaElement.Pause();
            //todo: 
            //FullScreen page = new FullScreen(new CurrentVideoState
            //{
            //    Position = _mediaElement.Position,
            //    VideoUri = _mediaSource,
            //});
            //await MopupService.Instance.PushAsync(page);
        }

        //mediaElement.Play();
    }

    // Seeting the Values of Controls
    private void SetHtmlWebviewControlValue()
    {
        _webView.Source = new HtmlWebViewSource { Html = _value };
    }

    private void SetUrlPageWebViewControlValue()
    {
        _webView.Source = new UrlWebViewSource { Url = _value };
    }

    private void SetUrlWebviewControlValue()
    {
        _mediaSource = new Uri(_value);
        _mediaElement.Source = _mediaSource;
        _videoGrid.IsVisible = true;
        Content = _videoGrid;
    }

    private void SetTextWebviewControlValue()
    {
        _webView = new WebView
        {
            Source = new HtmlWebViewSource
            {
                Html = _value
            }
        };
        Content = _webView;
    }
}