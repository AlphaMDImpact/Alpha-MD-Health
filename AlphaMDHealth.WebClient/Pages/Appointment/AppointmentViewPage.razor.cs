using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class AppointmentViewPage : BasePage
{
    private readonly AppointmentDTO _appointmentData = new() { RecordCount = -2 };
    private List<ButtonActionModel> _actionData;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _showAcceptRejectButton;

    /// <summary>
    /// Appointment ID parameter
    /// </summary>
    [Parameter]
    public long AppointmentID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _appointmentData.AccountID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
        _appointmentData.Appointment = new AppointmentModel
        {
            AppointmentID = AppointmentID
        };
        await SendServiceRequestAsync(new AppointmentService(AppState.webEssentials).GetAppointmentsAsync(_appointmentData), _appointmentData).ConfigureAwait(true);
        if (_appointmentData.ErrCode == ErrorCode.OK)
        {
            _showAcceptRejectButton = (_appointmentData.Appointment.FromDateTime.Value - DateTime.Now).TotalMinutes >= Convert.ToInt32(LibSettings.GetSettingValueByKey(_appointmentData.Settings, SettingsConstants.S_APPOINTMENT_DEFAULT_TIMEOUT_KEY), CultureInfo.InvariantCulture);
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_appointmentData.ErrCode.ToString());
        }
    }

    private void OnDeclineButtonClicked()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel{ ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
            new ButtonActionModel{ ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY }
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnJoinButtonClicked()
    {
        await NavigateToAsync(AppPermissions.VideoCallingView.ToString(), Convert.ToString(_appointmentData.Appointment.AppointmentID, CultureInfo.InvariantCulture)).ConfigureAwait(false);
    }

    private bool CheckAcceptAppointmentStatus()
    {
        var participantId = GetParticipantID();
        if (participantId != null)
        {
            return !(participantId.AppointmentStatusID == ResourceConstants.R_ACCEPTED_STATUS_KEY
                || participantId.AppointmentStatusID == ResourceConstants.R_REJECTED_STATUS_KEY);
        }
        return false;
    }

    private bool CheckDeclineAppointmentStatus()
    {
        var participantId = GetParticipantID();
        if (participantId != null)
        {
            if (participantId.AppointmentStatusID == ResourceConstants.R_ACCEPTED_STATUS_KEY
                || participantId.AppointmentStatusID == ResourceConstants.R_INPROGRESS_STATUS_KEY
                || participantId.AppointmentStatusID == ResourceConstants.R_NEW_STATUS_KEY)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckJoinAppointmentStatus()
    {
        var participantId = GetParticipantID();
        if (participantId != null)
        {
            return CheckAppointmentStatus(participantId.AppointmentStatusID);
        }
        return false;
    }

    private bool CheckAppointmentStatus(string statusID)
    {
        return statusID != ResourceConstants.R_MISSED_STATUS_KEY
            && statusID != ResourceConstants.R_REJECTED_STATUS_KEY
            && statusID != ResourceConstants.R_CANCELLED_STATUS_KEY;
    }

    private ParticipantsModel GetParticipantID()
    {
        //Check if logged in user is participant
        return _appointmentData.AppointmentParticipants?.FirstOrDefault(x => x.AccountID == _appointmentData.AccountID);
    }

    private async Task OnDeclineActionClickAsync(object buttonID)
    {
        _hideDeletedConfirmationPopup = true;
        if (buttonID?.ToString() == Constants.NUMBER_ONE)
        {
            await UpdateAppointmentStatusAsync(true);
        }
    }

    private async Task UpdateAppointmentStatusAsync(bool isDeclineRequest)
    {
        AppointmentDTO appointmentData = new()
        {
            Appointment = new AppointmentModel
            {
                AppointmentID = _appointmentData.Appointment.AppointmentID,
                AppointmentStatusID = isDeclineRequest ? ResourceConstants.R_REJECTED_STATUS_KEY : ResourceConstants.R_ACCEPTED_STATUS_KEY,
                AppointmentTypeImage = AppState.RouterData.SelectedRoute.Page
            }
        };
        await SendServiceRequestAsync(new AppointmentService(AppState.webEssentials).UpdateAppointmentAsync(appointmentData, new CancellationToken()), appointmentData).ConfigureAwait(true);
        if (appointmentData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(appointmentData.ErrCode.ToString());
        }
        else
        {
            Error = appointmentData.ErrCode.ToString();
        }
    }

    private async Task OnEditClickAsync()
    {
        await OnClose.InvokeAsync(_appointmentData.ErrCode.ToString() + Constants.SYMBOL_PIPE_SEPERATOR + _appointmentData.Appointment.AppointmentID);
    }

    private async Task OnCancelButtonClickAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{ DataField=nameof(ParticipantsModel.AppointmentStatusID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false },
            new TableDataStructureModel{ ImageSrc=nameof(ParticipantsModel.ImageName) ,HasImage=true, ImageFieldType=FieldTypes.SquareWithBackgroundImageControl, ImageHeight=AppImageSize.ImageSizeM, ImageWidth=AppImageSize.ImageSizeM },
            new TableDataStructureModel{ DataField=nameof(ParticipantsModel.FullName), DataHeader=ResourceConstants.R_NAME_KEY },
            new TableDataStructureModel { DataField=nameof(ParticipantsModel.CellFirstMiddleSatusContentHeader), DataHeader = ResourceConstants.R_STATUS_KEY, IsBadge = true, BadgeFieldType = nameof(ParticipantsModel.CellFirstMiddleContentHeaderColor) },
        };
    }

    private bool IsAppointmentAddEditAllowed()
    {
        return (LibPermissions.HasPermission(_appointmentData.FeaturePermissions, AppPermissions.AppointmentAddEdit.ToString()) && _appointmentData.Appointment.IsInitiator)
            || (_showAcceptRejectButton
                && CheckAppointmentStatus(_appointmentData.Appointment.AppointmentStatusID)
                && ResourceConstants.R_ACCEPTED_STATUS_KEY != _appointmentData.Appointment.AppointmentStatusID
                && hasStatus(false));
    }

    private bool IsAppointmentJoinAllowed()
    {
        return LibPermissions.HasPermission(_appointmentData.FeaturePermissions, AppPermissions.AppointmentJoin.ToString())
            && DateTime.Now <= AppState.webEssentials.ConvertToLocalTime(_appointmentData.Appointment.ToDateTime.Value)
            && !_showAcceptRejectButton
            && CheckJoinAppointmentStatus()
            && _appointmentData.Appointment.AppointmentTypeID == ResourceConstants.R_VIDEO_APPOINTMENT_TYPE_KEY_ID;
        //&& hasStatus(true);
    }

    private bool IsAppointmentDeclineAllowed()
    {
        return LibPermissions.HasPermission(_appointmentData.FeaturePermissions, AppPermissions.AppointmentDecline.ToString())
            && _showAcceptRejectButton
            && CheckDeclineAppointmentStatus();
        //&& hasStatus(true);
    }

    private bool hasStatus(bool checkRejected)
    {
        var accID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
        return _appointmentData.AppointmentParticipants.Any(x =>
            x.AccountID == accID
            && x.AppointmentStatusID != ResourceConstants.R_REJECTED_STATUS_KEY
            && (checkRejected || x.AppointmentStatusID != ResourceConstants.R_ACCEPTED_STATUS_KEY)
        );
    }

    private bool IsAppointmentAcceptAllowed()
    {
        return LibPermissions.HasPermission(_appointmentData.FeaturePermissions, AppPermissions.AppointmentDecline.ToString())
            && _showAcceptRejectButton
            && CheckAcceptAppointmentStatus()
            //&& CheckAppointmentStatus()
            && ResourceConstants.R_ACCEPTED_STATUS_KEY != _appointmentData.Appointment.AppointmentStatusID;
    }

    private async Task OnAcceptButtonClickedAsync()
    {
        await UpdateAppointmentStatusAsync(false);
    }
}