using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(ChatPage))]
[QueryProperty(nameof(ToID), "id")]
public class ChatPage : BasePage
{
    private readonly ChatView _chatView;
    private string _toID;

    /// <summary>
    /// User Id of the selected conversation
    /// </summary>
    public string ToID
    {
        get
        {
            return _toID;
        }
        set => _toID = Uri.UnescapeDataString(value);
    }

    public ChatPage() : base(PageLayoutType.EndToEndPageLayout, false)
    {
        //IsSplitView = MobileConstants.IsTablet;
        HideFooter(true);
        _chatView = new ChatView(this, null);
        PageLayout.Add(_chatView);
        SetPageContent(false);
    }

    protected override async void OnAppearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
            base.OnAppearing();
            _chatView.Parameters = AddParameters(
                CreateParameter(nameof(BaseDTO.IsActive), true.ToString(CultureInfo.InvariantCulture)),
                CreateParameter(nameof(BaseDTO.SelectedUserID), _toID));
            await _chatView.LoadUIAsync(false).ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
        }
    }

    protected override async void OnDisappearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            await _chatView.UnloadUIAsync().ConfigureAwait(true);
            base.OnDisappearing();
        }
    }
}