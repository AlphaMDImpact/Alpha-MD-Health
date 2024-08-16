//using AlphaMDHealth.Utility;

//namespace AlphaMDHealth.MobileClient;

///// <summary>
///// More options popup page
///// </summary>
//public class MoreOptionsPopupPage : PopupPage
//{
//    private readonly MenuCollectionView _menuGroupCollection;
//    private readonly double _screenWidth = App._essentials.GetPreferenceValue(LibStorageConstants.PR_SCREEN_WIDTH_KEY, (double)0);
//    private readonly double _screenHeight = App._essentials.GetPreferenceValue(LibStorageConstants.PR_SCREEN_HEIGHT_KEY, (double)0);

//    /// <summary>
//    /// More options popup
//    /// </summary>
//    public MoreOptionsPopupPage()
//    {
//        _menuGroupCollection = new MenuCollectionView();
//        BackgroundColor = Color.Transparent;
//        Padding = new Thickness(0, 0, (double)Application.Current.Resources[LibStyleConstants.ST_APP_PADDING], 0.1 * _screenHeight);
//        Content = new PancakeView
//        {
//            Style = (Style)Application.Current.Resources[LibStyleConstants.ST_PANCAKE_STYLE],
//            HorizontalOptions = LayoutOptions.End,
//            VerticalOptions = LayoutOptions.End,
//            HeightRequest = 0.6 * _screenHeight,
//            WidthRequest = 0.3 * _screenWidth,
//            BackgroundColor = (Color)Application.Current.Resources[LibStyleConstants.ST_CONTROL_BACKGROUND_COLOR],
//            Content = _menuGroupCollection
//        };
//    }

//    /// <summary>
//    /// Invoked when page appears on screen
//    /// </summary>
//    protected override async void OnAppearing()
//    {
//        base.OnAppearing();
//        await _menuGroupCollection.LoadUIDataAsync().ConfigureAwait(true);
//        AppHelper.ShowBusyIndicator = false;
//    }

//    /// <summary>
//    /// Invoked when page is removed from screen
//    /// </summary>
//    protected override void OnDisappearing()
//    {
//        base.OnDisappearing();
//        _menuGroupCollection.UnLoadUIData();
//    }
//}