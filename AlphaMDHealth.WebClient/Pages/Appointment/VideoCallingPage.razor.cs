using AlphaMDHealth.Model;

using Microsoft.AspNetCore.Components;
using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AlphaMDHealth.WebClient
{
    public partial class VideoCallingPage : BasePage
    {
        private readonly AppointmentDTO _appointmentDTO = new AppointmentDTO { RecordCount = -2 };
        //private CustomVideoCallControl _videoCallingControl = new CustomVideoCallControl();

        private bool _noError;
        /// <summary>
        /// Appointment ID parameter
        /// </summary>
        [Parameter]
        public long AppointmentID { get; set; }

        [Parameter]
        public EventCallback<string> PopUpClosed { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await GetDataAsync().ConfigureAwait(true);
        }

        private async Task GetDataAsync()
        {
            _appointmentDTO.Appointment = new AppointmentModel
            {
                AppointmentID = AppointmentID,
            };
            await SendServiceRequestAsync(new AppointmentService(AppState.webEssentials).GetAppointmentsAsync(_appointmentDTO), _appointmentDTO).ConfigureAwait(true);
            if (_appointmentDTO.ErrCode == ErrorCode.OK)
            {
                ParticipantsModel userDetails = _appointmentDTO.AppointmentParticipants.FirstOrDefault(X => X.AccountID == AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0));
                VideoDTO videoData = new VideoDTO { Video = new VideoModel(), AddedBy = userDetails.AccountID.ToString(CultureInfo.InvariantCulture), LastModifiedBy = userDetails.FirstName };
                videoData.Video.VideoRoomID = _appointmentDTO.Appointment.AppointmentAddress;
                await SendServiceRequestAsync(new VideoService(AppState.webEssentials).SyncVideoSessionFromServerAsync(videoData), videoData).ConfigureAwait(true);
                if (videoData.ErrCode == ErrorCode.OK)
                {
                    if (!string.IsNullOrWhiteSpace(videoData.Video?.VideoToken))
                    {
                        //ButtonActionModel leaveMeetingBtn = new ButtonActionModel
                        //{
                        //    ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY,
                        //    ButtonClass = "btn-secondary",
                        //    ButtonAction = async () => { await OnLeaveMeetingClickedAsync(); }
                        //};
                        //_actionButtons.Add(leaveMeetingBtn);
                        _appointmentDTO.Appointment.RoomID = _appointmentDTO.Appointment.AppointmentAddress;
                        _appointmentDTO.Appointment.AppointmentTypeName = userDetails.FullName;
                        _appointmentDTO.Appointment.AppointmentStatusID = userDetails.ImageBase64;
                        _appointmentDTO.Appointment.VideoToken = videoData.Video.VideoToken;
                        _appointmentDTO.Appointment.ClientID = string.IsNullOrWhiteSpace(videoData.Video.ApplicationID) ? string.Empty : videoData.Video.ApplicationID;
                        _appointmentDTO.Appointment.AppointmentStatusName = string.IsNullOrWhiteSpace(videoData.Video.VideoLink) ? string.Empty : videoData.Video.VideoLink;
                        _appointmentDTO.Appointment.ClientSecret = videoData.Video.SecretKey;
                        _appointmentDTO.Appointment.AccountID = userDetails.AccountID;
                        _appointmentDTO.Appointment.ServiceType = videoData.Video.ServiceType;
                        _appointmentDTO.AddedBy = videoData.AddedBy;
                        _noError = true;

                        await UpdateAppointmentStatusAsync(ResourceConstants.R_INPROGRESS_STATUS_KEY);
                    }
                    else
                    {
                        await PopUpClosed.InvokeAsync(ErrorCode.TokenExpired.ToString());
                    }
                }
                else
                {
                    await PopUpClosed.InvokeAsync(videoData.ErrCode.ToString());
                }
                _isDataFetched = true;
            }
            StateHasChanged();
        }

        private async Task PopUpClosedEventCallbackAsync(string isDataUpdated)
        {
            //await _videoCallingControl.OnLeaveAsync();
        }

        private async Task OnLeaveMeetingClickedAsync()
        {
            ErrorCode errorCode = await UpdateAppointmentStatusAsync(ResourceConstants.R_COMPLETED_STATUS_KEY);
            if (errorCode == ErrorCode.OK)
            {
                await PopUpClosed.InvokeAsync(string.Empty);
                if (_appointmentDTO.Appointment.ServiceType == ServiceType.Vidyo_Io)
                {
                    await NavigateToAsync(AppPermissions.AppointmentsView.ToString(), true, OnMenuClicked.ToString()).ConfigureAwait(false);
                }
            }
            else
            {
                Error = errorCode.ToString();
            }
        }

        private async Task<ErrorCode> UpdateAppointmentStatusAsync(string appointmentStatus)
        {
            AppointmentDTO appointmentDTO = new AppointmentDTO
            {
                Appointment = new AppointmentModel
                {
                    AppointmentID = AppointmentID,
                    AppointmentStatusID = appointmentStatus
                }
            };
            await SendServiceRequestAsync(new AppointmentService(AppState.webEssentials).UpdateAppointmentAsync(appointmentDTO, new CancellationToken()), appointmentDTO).ConfigureAwait(true);
            return appointmentDTO.ErrCode;
        }

        private async Task OnCancelButtonClickAsync()
        {
            NavigateToAsync(AppPermissions.AppointmentsView.ToString()).ConfigureAwait(true);
        }
    }
}
