using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Page used to display list of languages and select language
/// </summary>
[RouteRegistration("LanguageSelectionPage")]
public class LanguageSelectionPage : BasePage
{
    private bool _isBeforeLogin;
    private LanguageDTO _languagesData;
    private readonly AmhListViewControl<LanguageModel> _languagesListView;
    private readonly AmhButtonControl _actionButton;

    /// <summary>
    /// Default constructor
    /// </summary>
    public LanguageSelectionPage() : base(PageLayoutType.LoginFlowPageLayout, false)
    {
        _languagesData = new LanguageDTO();
        _isBeforeLogin = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0) == 0;
        _languagesListView = new AmhListViewControl<LanguageModel>(FieldTypes.OneRowListViewControl)
        {
            IsGroupedData = false,
            SourceFields = new AmhViewCellModel
            {
                ID = nameof(LanguageModel.LanguageID),
                LeftHeader = nameof(LanguageModel.LanguageName),
                RightHeader = nameof(LanguageModel.LanguageCode)
            },
            ShowSearchBar = false
        };

        _actionButton = new AmhButtonControl(FieldTypes.PrimaryButtonControl)
        {
            ResourceKey = ResourceConstants.R_NEXT_ACTION_KEY,
            VerticalOptions = LayoutOptions.End,
            IsControlEnabled = false,
        };
        AddRowColumnDefinition(GridLength.Star, 1, true);
        AddRowColumnDefinition(GridLength.Auto, 1, true);
        PageLayout.Add(_languagesListView, 0, 0);
        PageLayout.Add(_actionButton, 0, 1);
    }

    /// <summary>
    /// Render ui data
    /// </summary>
    protected override async void OnAppearing()
    {
        await new LanguageService(App._essentials).GetSupportedLanguagesAsync(_languagesData).ConfigureAwait(true);
        if (_languagesData.ErrCode == ErrorCode.OK)
        {
            base.OnAppearing();
            var language = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
            _languagesListView.PageResources = _actionButton.PageResources = PageData = _languagesData;
            _languagesListView.DataSource = _languagesData.Languages;
            var selectedLanguage = _languagesData.Languages?.FirstOrDefault(x => x.LanguageID == language);
            if (selectedLanguage != null)
            {
                _languagesListView.Value = selectedLanguage;
            }

            if (_isBeforeLogin)
            {
                Heading.Value = LibResources.GetResourceValueByKey(_languagesData.Resources, ResourceConstants.R_SELECT_LANGUAGES_KEY);
                if (string.IsNullOrWhiteSpace(Heading.Value))
                {
                    Heading.Value = Constants.SELECT_LANGUAGE_TEXT;
                }
            }

            if (string.IsNullOrWhiteSpace(_actionButton.Value))
            {
                _actionButton.Value = Constants.NEXT_ACTION_TEXT;
            }
            _actionButton.OnValueChanged += OnSaveButtonClick;
            _languagesListView.OnValueChanged += OnLanguageSelected;
            AppHelper.ShowBusyIndicator = false;
        }
        else
        {
            await ShellMasterPage.CurrentShell.PushMainPageAsync(new StaticMessagePage(_languagesData.ErrCode.ToString())).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Dispose page events
    /// </summary>
    protected override void OnDisappearing()
    {
        _actionButton.OnValueChanged -= OnSaveButtonClick;
        _languagesListView.OnValueChanged -= OnLanguageSelected;
        base.OnDisappearing();
    }

    private void OnLanguageSelected(object sender, EventArgs e)
    {
        if (_languagesListView.Value != null)
        {
            _languagesData.Language = _languagesListView.Value as LanguageModel;
        }
        _actionButton.IsControlEnabled = !IsLanguageMatched();
    }

    private bool IsLanguageMatched()
    {
        return App._essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0) == _languagesData.Language.LanguageID;
    }

    private void OnSaveButtonClick(object sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _actionButton.IsControlEnabled = false;
            if (await CheckAndDisplayInternetErrorAsync(false, CheckAndFetchMessage(ResourceConstants.R_OFFLINE_OPERATION_KEY, Constants.NO_INTERNET_CONNECTION_RESOURCE_VALUE_TEXT)).ConfigureAwait(true)
             && !IsLanguageMatched())
            {
                AppHelper.ShowBusyIndicator = true;
                if (!_isBeforeLogin)
                {
                    // Sync data to server if there are unsynced data then switch language
                    _languagesData.ErrCode = (await SyncDataWithServerAsync(Pages.LanguagesPage, _isBeforeLogin, default).ConfigureAwait(true)).ErrCode;
                    if (_languagesData.ErrCode != ErrorCode.OK)
                    {
                        if (_languagesData.ErrCode != ErrorCode.HandledRedirection)
                        {
                            DisplayOperationStatus(LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_SYNC_DATA_TO_SERVER_ERROR_KEY));
                            AppHelper.ShowBusyIndicator = false;
                        }
                        return;
                    }
                }
                // Reset sync from dattime to null for all language dependent tables
                _languagesData.ErrCode = (await ResetSyncFromDatesAsync(Pages.LanguageSelectionPage, _isBeforeLogin, default).ConfigureAwait(true)).ErrCode;
                if (_languagesData.ErrCode != ErrorCode.OK)
                {
                    DisplayOperationStatus(CheckAndFetchMessage(_languagesData.ErrCode.ToString(), Constants.ERROR_WHILE_RETRIEVING_RECORDS));
                    AppHelper.ShowBusyIndicator = false;
                    return;
                }
                App._essentials.SetPreferenceValue(StorageConstants.PR_IS_LANGUAGE_CHANGED_KEY, true);
                App._essentials.SetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, _languagesData.Language.LanguageID);
                App._essentials.SetPreferenceValue(StorageConstants.PR_IS_RIGHT_ALIGNED_KEY, _languagesData.Language.IsRightToLeft);
                await SyncAndNavigateAfterLanguageSelectionAsync(_isBeforeLogin, true).ConfigureAwait(true);
            }
            _actionButton.IsControlEnabled = true;
        });
    }

    private string CheckAndFetchMessage(string key, string defaultValue)
    {
        var message = LibResources.GetResourceValueByKey(PageData?.Resources, key);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = defaultValue;
        }
        return message;
    }
}