using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class DashboardConfigurationPage : BasePage
{
    private readonly DashboardDTO _dashboardData = new DashboardDTO { RecordCount = -1 };
    private List<SystemFeatureParameterModel> _configurationRecordParameters = new List<SystemFeatureParameterModel>();
    private List<ButtonActionModel> _actionData;
    private bool _hideDeleteConfirmation = true;
    private OptionModel _selectedFeature;
    private double? _sequenceNo;
    private bool _isEditable;

    /// <summary>
    /// DashboardSetting ID parameter
    /// </summary>
    [Parameter]
    public long DashboardSettingID { get; set; }

    /// <summary>
    /// Role ID parameter
    /// </summary>
    [Parameter]
    public byte RoleID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync();
    }

    private async Task GetDataAsync()
    {
        _dashboardData.ConfigurationRecord = new ConfigureDashboardModel
        {
            DashboardSettingID = DashboardSettingID,
            RoleID = RoleID
        };
        await SendServiceRequestAsync(new DashboardService(AppState.webEssentials).GetDashboardDataAsync(_dashboardData), _dashboardData).ConfigureAwait(true);
        if (_dashboardData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_dashboardData.FeaturePermissions, AppPermissions.DashboardConfigurationAddEdit.ToString());
            _sequenceNo = Convert.ToDouble(_dashboardData.ConfigurationRecord.SequenceNo);
            SetSelectedFeature();
            GetFeatureParameters();
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_dashboardData.ErrCode.ToString() + Constants.SYMBOL_PIPE_SEPERATOR + _dashboardData.ConfigurationRecord.RoleID);
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        if (IsValid())
        {
            await SaveAndDeleteDataAsync(true);
        }
    }

    private void OnDeleteClicked()
    {
        _actionData = new List<ButtonActionModel> {
             new ButtonActionModel{ ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
             new ButtonActionModel{ ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY }
        };
        _hideDeleteConfirmation = false;
    }

    private async Task OnDeleteConfirmationActionClickedAsync(object sequenceNo)
    {
        if (sequenceNo != null)
        {
            _hideDeleteConfirmation = true;
            if (Convert.ToInt32(sequenceNo) == 1)
            {
                await SaveAndDeleteDataAsync(false);
            }
        }
    }

    private async Task SaveAndDeleteDataAsync(bool isSaveRequest)
    {
        DashboardDTO dashboardData = new DashboardDTO
        {
            ConfigurationRecord = _dashboardData.ConfigurationRecord
        };
        dashboardData.ConfigurationRecord.SequenceNo = Convert.ToByte(_sequenceNo, CultureInfo.InvariantCulture);
        dashboardData.IsActive = isSaveRequest;
        dashboardData.ConfigurationRecordParameters = _configurationRecordParameters;
        await SendServiceRequestAsync(new DashboardService(AppState.webEssentials).SyncDashboardConfigurationToServerAsync(dashboardData, CancellationToken.None), dashboardData).ConfigureAwait(true);
        _dashboardData.ErrCode = dashboardData.ErrCode;
        if (dashboardData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(dashboardData.ErrCode.ToString() + Constants.SYMBOL_PIPE_SEPERATOR + dashboardData.ConfigurationRecord.RoleID);
        }
        else
        {
            Error = dashboardData.ErrCode.ToString();
        }
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnFeatureSelectionChanged(object e)
    {
        _dashboardData.ErrCode = ErrorCode.OK;
        SetSelectedFeature();
        RemoveControlContainsKey(nameof(SystemFeatureParameterModel.ParameterValue));
        _dashboardData.ConfigurationRecord.FeatureID = Convert.ToInt32(_selectedFeature?.OptionID ?? 0);
        GetFeatureParameters();
    }

    public void OnSequenceChanged(object sequenceNo)
    {
        _dashboardData.ErrCode = ErrorCode.OK;
    }

    private void GetFeatureParameters()
    {
        _configurationRecordParameters = _dashboardData.ConfigurationRecordParameters.FindAll(x => x.FeatureID == _dashboardData.ConfigurationRecord.FeatureID);
    }

    private void SetSelectedFeature()
    {
        _selectedFeature = _dashboardData.FeaturesOptions.FirstOrDefault(x => x.IsSelected);
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(SystemFeatureParameterModel.ParameterID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(SystemFeatureParameterModel.ParameterName),DataHeader=ResourceConstants.R_PARAMETER_NAME_KEY},
            new TableDataStructureModel{DataField=nameof(SystemFeatureParameterModel.ParameterDescription),DataHeader=ResourceConstants.R_PARAMETER_TYPE_NAME_KEY},
            new TableDataStructureModel{DataField=nameof(SystemFeatureParameterModel.ParameterValue),DataHeader=ResourceConstants.R_PARAMETER_VALUE_NAME_KEY,IsEditable=true, RegexPattern=LibSettings.GetSettingValueByKey(_dashboardData.Settings,SettingsConstants.S_ALPHA_NUMERIC_REGEX_KEY)},
        };
    }
}