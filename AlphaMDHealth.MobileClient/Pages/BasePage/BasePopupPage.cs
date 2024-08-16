using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Maui.PancakeView;
using Mopups.Pages;
using Mopups.Services;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class BasePopupPage : PopupPage, IDisposable
{
    private AmhButtonControl _bottomButton;
    private BoxView _separatorView;
    //private readonly SvgImageButtonView _closeButton;//todo:

    /// <summary>
    /// Parent Page Instance
    /// </summary>
    protected BasePage _parentPage;

    /// <summary>
    /// Header label
    /// </summary>
    private readonly AmhLabelControl _popupHeaderLabel;

    /// <summary>
    /// Left menu action
    /// </summary>
    private readonly AmhButtonControl _leftHeaderButton;

    /// <summary>
    /// Right Menu action
    /// </summary>
    private readonly AmhButtonControl _rightHeaderButton;

    /// <summary>
    /// on click event of LeftHeader Button
    /// </summary>
    public event EventHandler<EventArgs> OnLeftHeaderClickedEvent;

    /// <summary>
    /// on click event of RightHeader Button
    /// </summary>
    public event EventHandler<EventArgs> OnRightHeaderClickedEvent;

    /// <summary>
    /// on click event of Bottom Button
    /// </summary>
    public event EventHandler<EventArgs> OnBottomButtonClickedEvent;

    /// <summary>
    /// on click event of Bottom Button
    /// </summary>
    public event EventHandler<EventArgs> OnCloseButtonClickedEvent;

    public BasePopupPage(BasePage page)
    {
        _parentPage = page;
        var padding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
        Grid headerGrid = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = GridLength.Auto  },
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition{Width = GridLength.Auto },
            }
        };
        _popupHeaderLabel = DeviceInfo.Idiom == DeviceIdiom.Phone ? new AmhLabelControl(FieldTypes.PrimarySmallHVCenterBoldLabelControl) : new AmhLabelControl(FieldTypes.PrimaryAppSmallHVCenterBoldLabelControl);
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            _leftHeaderButton = new AmhButtonControl(FieldTypes.TransparentButtonControl) { WidthRequest = Constants.LARGE_WIDTH_REQUEST_CONSTANT, HorizontalOptions = LayoutOptions.StartAndExpand, /*HorizontalContentAlignment = TextAlignment.Start*/ };
            _rightHeaderButton = new AmhButtonControl(FieldTypes.TransparentButtonControl) { WidthRequest = Constants.LARGE_WIDTH_REQUEST_CONSTANT, HorizontalOptions = LayoutOptions.EndAndExpand, /*HorizontalContentAlignment = TextAlignment.End*/ };

            headerGrid.Add(_leftHeaderButton, 0, 0);
            headerGrid.Add(_popupHeaderLabel, 1, 0);
            headerGrid.Add(_rightHeaderButton, 2, 0);
        }
        else
        {
            //_closeButton = new SvgImageButtonView(ImageConstants.I_CLOSE_PNG, AppImageSize.ImageSizeS, AppImageSize.ImageSizeS)
            //{
            //    // Margin added to close button avoid adding a separate header grid
            //    Margin = new OnIdiom<Thickness>
            //    {
            //        Phone = new Thickness(0, padding * 1.5, 0, 0),
            //    }
            //};
            headerGrid.Add(_popupHeaderLabel, 1, 0);
            //headerGrid.Add(_closeButton, 2, 0);
            // Margin added to close button avoid adding a separate header grid
            _popupHeaderLabel.Margin = new Thickness(0, padding * 1.5, 0, 0);
        }
        headerGrid.Padding = new Thickness(padding, 0);
        _parentPage.MasterGrid.Add(headerGrid, 0, 0);
        CloseWhenBackgroundIsClicked = false;
        Content = new PancakeView
        {
            Margin = new Thickness(padding * 9, padding * 3),
            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR],
            CornerRadius = new OnIdiom<CornerRadius>
            {
                Phone = new CornerRadius(padding * 2, padding * 2, 0, 0),
                Tablet = new CornerRadius(10)
            },
            Content = _parentPage.MasterGrid,
        };
    }

    protected BasePopupPage() : this(new BasePage())
    {

    }

    /// <summary>
    /// On Appearing event
    /// </summary>
    protected override void OnDisappearing()
    {
        if (_leftHeaderButton != null)
        {
            _leftHeaderButton.OnValueChanged -= OnLeftHeaderButtonClicked;
        }
        if (_rightHeaderButton != null)
        {
            _rightHeaderButton.OnValueChanged -= OnRightHeaderButtonClicked;
        }
        if (_bottomButton != null)
        {
            _bottomButton.OnValueChanged -= OnBottomButtonClicked;
        }
        //if (_closeButton != null)
        //{
        //    _closeButton.Clicked -= OnCloseButtonClicked;
        //}
        base.OnDisappearing();
    }

    /// <summary>
    /// Show close button action
    /// </summary>
    /// <param name="isShow"></param>
    protected void ShowCloseButton(bool isShow)
    {
        //if (isShow)
        //{
        //    _closeButton.IsVisible = true;
        //    _closeButton.Clicked += OnCloseButtonClicked;
        //}
        //else
        //{
        //    if (_closeButton != null)
        //    {
        //        _closeButton.IsVisible = false;
        //        _closeButton.Clicked -= OnCloseButtonClicked;
        //    }
        //}
    }

    private void OnCloseButtonClicked(object sender, EventArgs e)
    {
        OnCloseButtonClickedEvent?.Invoke(sender, e);
    }

    /// <summary>
    /// Sets title 
    /// </summary>
    /// <param name="title"></param>
    protected void SetTitle(string title)
    {
        _popupHeaderLabel.Value = title;
    }

    /// <summary>
    /// Close popup page action
    /// </summary>
    protected async Task ClosePopupAsync()
    {
        await MopupService.Instance.PopAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Shows left header menu
    /// </summary>
    /// <param name="resourceKey"></param>
    protected void DisplayLeftHeader(string resourceKey)
    {
        _leftHeaderButton.Value = LibResources.GetResourceValueByKey(_parentPage.PageData.Resources, resourceKey);
        _leftHeaderButton.OnValueChanged += OnLeftHeaderButtonClicked;
    }

    /// <summary>
    /// Left header menu action click event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnLeftHeaderButtonClicked(object sender, EventArgs e)
    {
        OnLeftHeaderClickedEvent?.Invoke(sender, e);
    }

    /// <summary>
    /// Shows right header menu
    /// </summary>
    /// <param name="resourceKey"></param>
    protected void DisplayRightHeader(string resourceKey)
    {
        _rightHeaderButton.Value = LibResources.GetResourceValueByKey(_parentPage.PageData.Resources, resourceKey);
        _rightHeaderButton.OnValueChanged += OnRightHeaderButtonClicked;
    }

    /// <summary>
    /// Right header menu action click event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnRightHeaderButtonClicked(object sender, EventArgs e)
    {
        OnRightHeaderClickedEvent?.Invoke(sender, e);
    }

    /// <summary>
    /// Bottom action button click event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnBottomButtonClicked(object sender, EventArgs e)
    {
        OnBottomButtonClickedEvent?.Invoke(sender, e);
    }

    /// <summary>
    /// Shows bottom button
    /// </summary>
    /// <param name="resourceKey"></param>
    /// <param name="controlType"></param>
    protected void DisplayBottomButton(string resourceKey, FieldTypes controlType)
    {
        _bottomButton = new AmhButtonControl(controlType);
        _parentPage.MasterGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _parentPage.MasterGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        //_separatorView = CreateLinkSeparator(true);
        _separatorView.Margin = new Thickness(0);
        _parentPage.MasterGrid.Add(_separatorView, 0, 2);
        _parentPage.MasterGrid.Add(_bottomButton, 0, 3);
        _bottomButton.Value = LibResources.GetResourceValueByKey(_parentPage.PageData.Resources, resourceKey);
        _bottomButton.OnValueChanged += OnBottomButtonClicked;
    }

    /// <summary>
    /// Show/Hide bottom button
    /// </summary>
    /// <param name="isShow"></param>
    protected void ShowHideBottomButton(bool isShow)
    {
        if (_bottomButton != null && _parentPage.MasterGrid.Children.Contains(_bottomButton))
        {
            _bottomButton.IsVisible = _separatorView.IsVisible = isShow;
        }
    }

    /// <summary>
    /// Dispose popup action
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        // Cleanup
    }

    /// <summary>
    /// Dispose popup
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}