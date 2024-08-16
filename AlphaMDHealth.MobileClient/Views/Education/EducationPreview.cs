using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class EducationPreview : ViewManager
{
    internal readonly ContentPageDTO _educationDTO = new ContentPageDTO { RecordCount = -2, Page = new ContentPageModel() };
    internal readonly CustomLabelControl _headerLabel;
    private readonly CustomWebView _browser;
    private readonly CustomButtonControl _readButton;

    public EducationPreview(BasePage parentPage, object parameters) : base(parentPage, parameters)
    {
        ParentPage.PageService = new ContentPageService(App._essentials);
        _headerLabel = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft);
        _browser = new CustomWebView
        {
            HeightRequest = 1,
            IsAutoIncreaseHeight = true,
            ShowBusyIndicator = true
        };
        _readButton = new CustomButtonControl(ButtonType.PrimaryWithMargin)
        {
            IsVisible = false,
        };
        Grid mainLayout = new Grid
        {
            Style = (Style)App.Current.Resources[MobileConstants.IsTablet ? StyleConstants.ST_END_TO_END_GRID_STYLE : StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowSpacing = Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture),
            RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto },
                },
        };
        mainLayout.Add(_headerLabel, 0, 0);
        mainLayout.Add(_browser, 0, 1);
        mainLayout.Add(_readButton, 0, 2);
        Content = mainLayout;
    }

    public override async Task UnloadUIAsync()
    {
        _readButton.Clicked -= ReadButton_Clicked;
        await Task.CompletedTask;
    }

    internal async Task LoadUIDataAsync(string pageID, long patientTaskID)
    {
        _educationDTO.Page.PageID = Convert.ToInt64(pageID, CultureInfo.InvariantCulture);
        _educationDTO.PermissionAtLevelID = patientTaskID;
        await (ParentPage.PageService as ContentPageService).GetContentPagesAsync(_educationDTO).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_educationDTO.ErrCode == ErrorCode.OK && _educationDTO.Page != null)
        {
            _readButton.Clicked += ReadButton_Clicked;
            if (_educationDTO.PermissionAtLevelID > 0)
            {
                await SetTitleViewAsync().ConfigureAwait(true);
                _readButton.IsVisible = true;
                _readButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_READ_BUTTON_TEXT_KEY);
                if (_educationDTO.AddedBy == ResourceConstants.R_NEW_STATUS_KEY)
                {
                    await UpdateTaskStausAsync(ResourceConstants.R_INPROGRESS_STATUS_KEY).ConfigureAwait(true);
                }
            }
            _headerLabel.Text = _educationDTO.Page.Title;
            if (_educationDTO.Page.IsLink)
            {
                if (Device.RuntimePlatform == Device.Android && _educationDTO.Page.PageData.EndsWith(Constants.PDF_FILE_EXTENSION))
                {
                    _browser.PDFLinkName = _educationDTO.Page.PageData;
                }
                else
                {
                    _browser.Source = _educationDTO.Page.PageData;
                }
            }
            else
            {
                HtmlWebViewSource _educationContentView = new HtmlWebViewSource
                {
                    Html = ParentPage.GetSettingsValueByKey(SettingsConstants.S_HTML_WRAPPER_KEY).Replace(Constants.STRING_FROMAT, _educationDTO.Page.PageData)
                };
                _browser.Source = _educationContentView;
            }
        }
        else
        {
            await ParentPage.DisplayMessagePopupAsync(_educationDTO.ErrCode != ErrorCode.OK ? _educationDTO.ErrCode.ToString() : ResourceConstants.R_NO_DATA_FOUND_KEY, OnPopupActionClicked).ConfigureAwait(true);
        }
        AppHelper.ShowBusyIndicator = false;
    }

    private async Task SetTitleViewAsync()
    {
        MenuView titleView = new MenuView(MenuLocation.Header, _educationDTO.Page.Title, ShellMasterPage.CurrentShell.CurrentPage.IsAddEditPage);
        if (DeviceInfo.Idiom == DeviceIdiom.Phone)
        {
            await ParentPage.OverrideTitleViewAsync(titleView).ConfigureAwait(true);
        }
    }

    private async Task UpdateTaskStausAsync(string statusKey)
    {
        _educationDTO.ErrCode = await new PatientTaskService(App._essentials).UpdateTaskStatusAsync(_educationDTO.PermissionAtLevelID, statusKey).ConfigureAwait(true);
        if (_educationDTO.ErrCode == ErrorCode.OK)
        {
            await (ParentPage as BasePage).SyncDataWithServerAsync(Pages.EducationPreviewPage, false, default).ConfigureAwait(true);
        }
    }

    private async void ReadButton_Clicked(object sender, EventArgs e)
    {
        await UpdateTaskStausAsync(ResourceConstants.R_COMPLETED_STATUS_KEY).ConfigureAwait(true);
        if (_educationDTO.ErrCode == ErrorCode.OK)
        {
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                InvokeListRefresh(_educationDTO.ErrCode.ToString(), new EventArgs());
            }
            else
            {
                await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
            }
        }
        else
        {
            ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(_educationDTO.ErrCode.ToString()), false);
        }
    }

    private async void OnPopupActionClicked(object sender, int e)
    {
        //todo:
        //if (ShellMasterPage.CurrentShell.CurrentPage is PatientsPage)
        //{
        //    //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
        //    //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
        //}
        //else
        //{
        //    //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
        //}
    }

    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        await LoadUIDataAsync(Constants.CONSTANT_ZERO, 0).ConfigureAwait(true);
    }
}