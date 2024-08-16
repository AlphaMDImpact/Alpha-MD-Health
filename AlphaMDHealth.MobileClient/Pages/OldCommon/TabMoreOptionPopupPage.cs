//using AlphaMDHealth.Model;
//using AlphaMDHealth.Utility;

//namespace AlphaMDHealth.MobileClient;

///// <summary>
///// More options popup page for tabs
///// </summary>
//public class TabMoreOptionPopupPage : PopupPage
//{
//    private readonly TabMoreOptionView _tabMoreOptionView = new TabMoreOptionView();
//    private readonly List<OptionModel> _tabOptions;
//    private readonly PancakeView _optionContent;

//    /// <summary>
//    /// on click event of Send Button
//    /// </summary>
//    public event EventHandler<EventArgs> OnTabChanged;

//    /// <summary>
//    /// More options popup for tabs
//    /// </summary>
//    public TabMoreOptionPopupPage(List<OptionModel> tabOptions)
//    {
//        _tabOptions = tabOptions;
//        //todo: CloseWhenBackgroundIsClicked = true;
//        BackgroundColor = Color.Transparent;
//        _optionContent = new PancakeView
//        {
//            Style = (Style)Application.Current.Resources[LibStyleConstants.ST_PANCAKE_STYLE],
//            HorizontalOptions = LayoutOptions.End,
//            VerticalOptions = LayoutOptions.StartAndExpand,
//            WidthRequest = 0.2 * App._essentials.GetPreferenceValue(LibStorageConstants.PR_SCREEN_WIDTH_KEY, (double)0),
//            BackgroundColor = (Color)Application.Current.Resources[LibStyleConstants.ST_CONTROL_BACKGROUND_COLOR],
//            Content = _tabMoreOptionView
//        };
//        Content = _optionContent;
//    }

//    /// <summary>
//    /// Invoked when page appears on screen
//    /// </summary>
//    protected override async void OnAppearing()
//    {
//        base.OnAppearing();
//        _tabMoreOptionView.OnListRefresh += OnTabSelectionChanged;
//        await _tabMoreOptionView.LoadUIDataAsync(_tabOptions).ConfigureAwait(true);
//        if (_tabOptions.Count > 0)
//        {
//            _optionContent.HeightRequest = _tabOptions.Count * ((double)AppImageSize.ImageSizeL + 1);
//        }
//        AppHelper.ShowBusyIndicator = false;
//    }

//    /// <summary>
//    /// Event of tab switch
//    /// </summary>
//    /// <param name="sender">Sender object</param>
//    /// <param name="e">Event argument</param>
//    public async void OnTabSelectionChanged(object sender, EventArgs e)
//    {
//        OnTabChanged?.Invoke(sender, e);
//        //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
//    }

//    /// <summary>
//    /// Invoked when page is removed from screen
//    /// </summary>
//    protected override void OnDisappearing()
//    {
//        _tabMoreOptionView.UnLoadUIData();
//        _tabMoreOptionView.OnListRefresh -= OnTabSelectionChanged;
//        base.OnDisappearing();
//    }
//}