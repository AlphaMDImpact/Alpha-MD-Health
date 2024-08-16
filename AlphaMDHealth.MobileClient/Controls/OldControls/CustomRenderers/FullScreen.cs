using AlphaMDHealth.Utility;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Mopups.Pages;
using Mopups.Services;

namespace AlphaMDHealth.MobileClient;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class FullScreen : PopupPage
{
    private readonly DeviceOrientationService deviceOrientationService;
    public CurrentVideoState Video { get; set; }
    private readonly MediaElement mediaElement;
    public FullScreen(CurrentVideoState currentVideo)
    {
        mediaElement = new MediaElement()
        {
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromRgba(0, 0, 0, 255),
            HorizontalOptions = LayoutOptions.FillAndExpand,
            //ShouldAutoPlay = true,
            ShouldShowPlaybackControls = true,
            VerticalOptions = LayoutOptions.FillAndExpand
        };
        //mediaElement.Style = (Style)Application.Current.Resources[StyleConstants.ST_MEDIA_STYLE];


        var backButton = new ImageButton();
       
        backButton.Style = (Style)Application.Current.Resources[StyleConstants.ST_BACK_IMAGE_BUTTON];

        backButton.Clicked += Button_Clicked;
        backButton.Behaviors.Add(new IconTintColorBehavior { TintColor = Color.FromRgba(255, 255, 255, 255) });

        var aspectButton = new ImageButton();
       
        aspectButton.Style = (Style)Application.Current.Resources[StyleConstants.ST_ASPECT_IMAGE_BUTTON];

        aspectButton.Clicked += btnChangeAspect_Clicked;
        aspectButton.Behaviors.Add(new IconTintColorBehavior { TintColor = Color.FromRgba(255, 255, 255, 255) });

        var orientationButton = new ImageButton();
      
        orientationButton.Style = (Style)Application.Current.Resources[StyleConstants.ST_ORIENTATION_IMAGE_BUTTON];

        orientationButton.Clicked += btnChangeOrientation_Clicked;
        orientationButton.Behaviors.Add(new IconTintColorBehavior { TintColor = Color.FromRgba(255, 255, 255, 255) });

        var buttonStackLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Padding = new Thickness(5),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.StartAndExpand,
            Children = { backButton, aspectButton, orientationButton }
        };

        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                },
            Children = { mediaElement, buttonStackLayout }
        };

        Content = mainGrid;

        Video = currentVideo;

        deviceOrientationService = new DeviceOrientationService();
        deviceOrientationService.SetDeviceOrientation(displayOrientation: DisplayOrientation.Landscape);

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        mediaElement.Source = Video.VideoUri;
        mediaElement.SeekTo(Video.Position);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        mediaElement.Source = null;
        WeakReferenceMessenger.Default.Send(new NotifyFullScreenClosed(true));
        await MopupService.Instance.PopAsync();
    }

    private void btnChangeAspect_Clicked(object sender, EventArgs e)
    {
        if (mediaElement.Aspect == Aspect.AspectFit) MainThread.InvokeOnMainThreadAsync(() => mediaElement.Aspect = Aspect.Fill);
        else if (mediaElement.Aspect == Aspect.Fill) MainThread.InvokeOnMainThreadAsync(() => mediaElement.Aspect = Aspect.Center);
        else if (mediaElement.Aspect == Aspect.Center) MainThread.InvokeOnMainThreadAsync(() => mediaElement.Aspect = Aspect.AspectFit);

    }

    private void btnChangeOrientation_Clicked(object sender, EventArgs e)
    {
        switch (DeviceDisplay.Current.MainDisplayInfo.Orientation)
        {
            case DisplayOrientation.Landscape: deviceOrientationService.SetDeviceOrientation(DisplayOrientation.Portrait); break;
            case DisplayOrientation.Portrait: deviceOrientationService.SetDeviceOrientation(DisplayOrientation.Landscape); break;
        }
    }
}


public class CurrentVideoState
{
    public Uri? VideoUri { get; set; }
    public TimeSpan Position { get; set; }
}

public class NotifyFullScreenClosed : ValueChangedMessage<bool>
{
    public NotifyFullScreenClosed(bool value) : base(value) { }
}
