using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class OrganisationSettingsPage : BasePage
{
    private readonly BaseDTO _settingData = new BaseDTO();

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_settingData.LastModifiedBy != GetGroupName())
        {
            await GetDataAsync().ConfigureAwait(true);
            await base.OnParametersSetAsync();
        }
    }

    private async Task GetDataAsync()
    {
        _settingData.LastModifiedBy = GetGroupName();
        await SendServiceRequestAsync(new OrganisationService(AppState.webEssentials).SyncOrganisationSettingsFromServerAsync(_settingData, CancellationToken.None), _settingData).ConfigureAwait(false);
        if(_settingData.ErrCode != ErrorCode.OK)
        {
            Error = _settingData.ErrCode.ToString();
        }
        _isDataFetched = true;
    }

    private List<OptionModel> GetCheckBoxOptions(SettingModel setting)
    {
        return new List<OptionModel>{
            new OptionModel {
                OptionID = 1,
                OptionText= LibResources.GetResourceValueByKey(_settingData.Resources, setting.SettingKey),
                IsSelected=Convert.ToBoolean(setting.SettingValue.ToLower())
            }
        };
    }

    private void OnValueChanges(object value, SettingModel setting)
    {
        switch (setting.SettingType?.ToEnum<SettingType>())
        {
            case SettingType.Bool:
                setting.SettingValue = value != null && (value as string) == Constants.NUMBER_ONE ? Constants.TRUE.ToUpper() : Constants.FALSE.ToUpper();
                break;
            case SettingType.Numeric:
                setting.SettingValue = value != null ? value.ToString() : string.Empty;
                break;
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            BaseDTO settings = new BaseDTO { Settings = new List<SettingModel>() };
            settings.LastModifiedBy = GetGroupName();
            settings.OrganisationID = AppState.MasterData.OrganisationID;
            settings.Settings = _settingData.Settings.Where(x => x.GroupName == _settingData.LastModifiedBy && x.SettingType != null).ToList();
            await SendServiceRequestAsync(new OrganisationService(AppState.webEssentials).SyncOrganisationSettingsToServerAsync(settings, CancellationToken.None), settings).ConfigureAwait(true);
            if (settings.ErrCode == ErrorCode.OK)
            {
                Success = settings.ErrCode.ToString();
                await NavigateToAsync(AppState.MasterData.DefaultRoute, true).ConfigureAwait(true);
            }
            else
            {
                Error = settings.ErrCode.ToString();
            }
        }
    }

    private string GetGroupName()
    {
        return RouterDataRoute.Page == AppPermissions.OrganisationSettingsView.ToString()
            ? GroupConstants.RS_ORGANISATION_SETTINGS_GROUP
            : GroupConstants.RS_ORGANISATION_THEMES_STYLES_GROUP;
    }

    private string GetPageTile()
    {
        return LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, GetFeaturePermission());
    }

    private string GetFeaturePermission()
    {
        return RouterDataRoute.Page == AppPermissions.OrganisationSettingsView.ToString()
            ? AppPermissions.OrganisationSettingsAddEdit.ToString()
            : AppPermissions.OrganisationThemeSettingsAddEdit.ToString();
    }
}