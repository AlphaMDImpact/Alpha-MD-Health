using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(ChatsPage))]
[QueryProperty(nameof(ToID), "id")]
[QueryProperty(nameof(RecordCount), "recordCount")]
public class ChatsPage : BasePage
{
    private readonly ChatsView _chatsView;
    private int _recordCount;
    private long _toID;
    private bool _pageLoaded;

    /// <summary>
    /// No of chat conversation to be displayed
    /// </summary>
    public string RecordCount
    {
        get { return _recordCount.ToString(CultureInfo.InvariantCulture); }
        set
        {
            _recordCount = string.IsNullOrWhiteSpace(value) ? 0 : Convert.ToInt32(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// User Id of the selected conversation
    /// </summary>
    public string ToID
    {
        get { return _toID.ToString(CultureInfo.InvariantCulture); }
        set
        {
            _toID = string.IsNullOrWhiteSpace(value) ? 0 : Convert.ToInt64(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
            if (_pageLoaded && _chatsView != null)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await _chatsView.UpdateChatDetailsViewAsync(_toID, false).ConfigureAwait(true);
                });
            }
        }
    }

    public ChatsPage() : base(PageLayoutType.EndToEndPageLayout, false)
    {
        //IsSplitView = MobileConstants.IsTablet;
        _chatsView = new ChatsView(this, null) { Margin = 0 };
        PageLayout.Add(_chatsView);
        PageLayout.Padding = 0;
        SetPageContent(false);
    }

    protected override async void OnAppearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
            base.OnAppearing();
            _chatsView.Parameters = AddParameters(
                CreateParameter(nameof(RecordCount), RecordCount),
                CreateParameter(SPFieldConstants.FIELD_IS_PROVIDER_LIST, false.ToString(CultureInfo.InvariantCulture)),
                CreateParameter(nameof(BaseDTO.SelectedUserID), _toID.ToString(CultureInfo.InvariantCulture)));
            await _chatsView.LoadUIAsync(false).ConfigureAwait(true);
            _pageLoaded = true;
            AppHelper.ShowBusyIndicator = false;
        }
    }

    protected override async void OnDisappearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            await _chatsView.UnloadUIAsync().ConfigureAwait(true);
            _toID = 0;
            base.OnDisappearing();
        }
    }

    /// <summary>
    /// Method to override to handle header item clicks
    /// </summary>
    /// <param name="headerLocation">Header location whether Left, Right (in case of tablet)</param>
    /// <param name="menuType">Menu position whether Left, Right</param>
    /// <param name="menuAction">Action type</param>
    public async override Task OnMenuActionClick(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    {
        if (menuAction == MenuAction.MenuActionAddKey)
        {
            await PushPageByTargetAsync(Pages.NewChatPage.ToString(), GenericMethods.GenerateParamsWithPlaceholder(AlphaMDHealth.Utility.Param.id), string.Empty).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Refresh view/page based on current service sync
    /// </summary>
    /// <param name="syncFrom">From which page sync from is called</param>
    protected override async Task RefreshUIAsync(Pages syncFrom)
    {
        await Task.Run(async () =>
        {
            await _chatsView.LoadUIAsync(true).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }
}