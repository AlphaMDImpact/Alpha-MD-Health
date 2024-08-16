using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class UserAccountSettingsPage : BasePage
{
    private UserAccountSettingDTO _userSettings;

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        if (_userSettings == null)
        {
            _userSettings = new UserAccountSettingDTO { UserAccountSetting = new UserAccountSettingsModel(), RecordCount = -1 };
        }
        await SendServiceRequestAsync(new UserAccountSettingService(AppState.webEssentials).GetUserAccountSettingsAsync(_userSettings), _userSettings).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            foreach (UserAccountSettingsModel record in _userSettings.UserAccountSettings)
            {
                long selectedID = record.ReadingUnitOption.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0;
                record.SettingValue = selectedID.ToString();
            }
            await SendServiceRequestAsync(new UserAccountSettingService(AppState.webEssentials).SyncUserAccountSettingsToServerAsync(_userSettings, CancellationToken.None), _userSettings).ConfigureAwait(true);
            if (_userSettings.ErrCode == ErrorCode.OK)
            {
                Success = _userSettings.ErrCode.ToString();
            }
            else
            {
                Error = _userSettings.ErrCode.ToString();
            }
        }
    }
}