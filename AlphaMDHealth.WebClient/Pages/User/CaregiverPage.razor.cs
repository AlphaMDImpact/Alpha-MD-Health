using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class CaregiverPage : BasePage
{
    private readonly CaregiverDTO _caregiverData = new CaregiverDTO { RecordCount = -1, Caregiver = new CaregiverModel() };
    private List<ButtonActionModel> _actionData;
    private UserService _userService;
    private bool _hideConfirmationPopup = true;
    private bool _isEditable = false;
    private bool _isChangedFromControl = false;

    /// <summary>
    /// Patient Caregiver ID parameter
    /// </summary>
    [Parameter]
    public long PatientCaregiverID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _userService = new UserService(AppState.webEssentials);
        _caregiverData.Caregiver.PatientCareGiverID = PatientCaregiverID;
        await SendServiceRequestAsync(_userService.GetPatientCareGiversAsync(_caregiverData), _caregiverData).ConfigureAwait(true);
        if (_caregiverData.ErrCode == ErrorCode.OK)
        {
            _isEditable = !(PatientCaregiverID > 0 && (_caregiverData.Caregiver.ProgramCareGiverID > 0 || GenericMethods.GetUtcDateTime >= _caregiverData.Caregiver.ToDate));
            _isEditable = _isEditable && LibPermissions.HasPermission(_caregiverData.FeaturePermissions, AppPermissions.CaregiverAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_caregiverData.ErrCode.ToString());
        }
    }

    private void OnBranchChanged(object optionId, bool isChangedFromControl)
    {
        if (isChangedFromControl && !string.IsNullOrWhiteSpace(optionId as string))
        {
            AssignPickerValues(optionId, 1);
        }
        else
        {
            _caregiverData.CaregiverList = _caregiverData.CaregiverOptions;
        }
    }

    private void OnDepartmentChanged(object optionId, bool isChangedFromControl)
    {
        if (isChangedFromControl)
        {
            AssignPickerValues(optionId, 2);
        }
    }

    private void AssignPickerValues(object optionId, byte level)
    {
        if (optionId != null)
        {
            _userService.AssignPickerValues(_caregiverData, level, !string.IsNullOrWhiteSpace(optionId.ToString()) ? Convert.ToInt64(optionId) : 0, _isChangedFromControl);
            _isChangedFromControl = true;
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            if (_caregiverData.Caregiver.FromDate.Value.Date > _caregiverData.Caregiver.ToDate.Value.Date)
            {
                Error = string.Format(CultureInfo.InvariantCulture,
                        LibResources.GetResourceValueByKey(_caregiverData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                        LibResources.GetResourceValueByKey(_caregiverData.Resources, ResourceConstants.R_START_DATE_KEY),
                        LibResources.GetResourceValueByKey(_caregiverData.Resources, ResourceConstants.R_END_DATE_KEY));
                return;
            }
            if (_caregiverData.DepartmentList.Any(x => x.IsSelected) && _caregiverData.DepartmentList.Find(x => x.IsSelected).OptionID > 0)
            {
                _caregiverData.Caregiver.OrganisationID = _caregiverData.DepartmentList.Find(x => x.IsSelected).OptionID;
            }
            else if (_caregiverData.Branches.Any(x => x.IsSelected) && _caregiverData.Branches.Find(x => x.IsSelected).OptionID > 0)
            {
                _caregiverData.Caregiver.OrganisationID = _caregiverData.Branches.Find(x => x.IsSelected).OptionID;
            }
            else
            {
                _caregiverData.Caregiver.OrganisationID = _caregiverData.Organisations.Find(x => x.IsSelected).OptionID;
            }
            _caregiverData.Caregiver.CareGiverID = _caregiverData.CaregiverList.Find(x => x.IsSelected).OptionID;
            _caregiverData.IsActive = true;
            await SaveCaregiverDataAsync();
        }
    }

    private async Task SaveCaregiverDataAsync()
    {
        Success = Error = string.Empty;
        await SendServiceRequestAsync(_userService.SyncPatientCaregiverToServerAsync(_caregiverData
            , CancellationToken.None), _caregiverData).ConfigureAwait(true);
        if (_caregiverData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_caregiverData.ErrCode.ToString());
        }
        else
        {
            Error = _caregiverData.ErrCode.ToString();
        }
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE,ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO,ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task OnDeleteConfirmationPopUpClickedAsync(object value)
    {
        if (value != null)
        {
            _hideConfirmationPopup = true;
            Success = Error = string.Empty;
            if (Convert.ToInt64(value) == 1)
            {
                _caregiverData.IsActive = false;
                await SaveCaregiverDataAsync();
            }
        }
    }
}