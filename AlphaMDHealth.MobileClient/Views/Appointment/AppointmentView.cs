using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class AppointmentView : BaseLibCollectionView
{
    private readonly AppointmentDTO _appointmentData = new AppointmentDTO { Appointment = new AppointmentModel() };
    private readonly CustomButton _joinButton;
    private readonly CustomButton _declineButton;
    private readonly CustomButton _acceptButton;
    private readonly CustomLabelControl _listheader;
    private readonly CustomLabelControl _subjectLabel;
    private readonly CustomLabelControl _informationLabel;
    private readonly CustomLabelControl _dateTimeLabel;
    private readonly Grid _bodyLayout;
    private readonly Grid _mainLayout;
    private readonly bool _isPatient;
    internal string _appointmentHeader;
    private long _accountID;
    private int _roleID;

    /// <summary>
    /// Used to check whether user clicked on edit button
    /// </summary>
    public bool IsEditClicked { get; set; }

    /// <summary>
    /// OnVideoCallJoin
    /// </summary>
    public event EventHandler<EventArgs> OnVideoCallJoin;

    public AppointmentView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new AppointmentService(App._essentials);
        _roleID = App._essentials.GetPreferenceValue(StorageConstants.PR_ROLE_ID_KEY, 0);
        _isPatient = _roleID == (int)RoleName.Patient || _roleID == (int)RoleName.CareTaker;
        _accountID = App._essentials.GetPreferenceValue(StorageConstants.PR_ACCOUNT_ID_KEY, (long)0);
        double appPadding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
        _subjectLabel = new CustomLabelControl(LabelType.LeftSubHeader);
        _informationLabel = new CustomLabelControl(LabelType.PrimarySmallLeft)
        {
            Margin = new Thickness(0, appPadding, 0, 0),
            LineBreakMode = LineBreakMode.WordWrap
        };
        _dateTimeLabel = new CustomLabelControl(LabelType.SecondrySmallLeft) { Margin = new Thickness(0, appPadding) };
        _listheader = new CustomLabelControl(LabelType.PrimaryMediumLeft);
        _joinButton = new CustomButtonControl(_isPatient ? ButtonType.PrimaryWithMargin : ButtonType.PrimaryWithoutMargin);
        _declineButton = new CustomButtonControl(_isPatient ? ButtonType.DeleteWithMargin : ButtonType.DeleteWithoutMargin)
        {
            IsVisible = false,
        };
        _acceptButton = new CustomButtonControl(_isPatient ? ButtonType.PrimaryWithMargin : ButtonType.PrimaryWithoutMargin)
        {
            IsVisible = false,
        };
        CustomCellModel customCellModel = new CustomCellModel
        {
            CellHeader = nameof(ParticipantsModel.FullName),
            CellDescription = nameof(ParticipantsModel.Profession),
            CellLeftDefaultIcon = nameof(ParticipantsModel.NameInitials),
            //todo:CellLeftSourceIcon = nameof(ParticipantsModel.ImageSource),
            CellRightContentDescription = nameof(ParticipantsModel.AppointmentStatusName),
            CellDescriptionColor = nameof(ParticipantsModel.AppointmentStatusColor),
        };
        _bodyLayout = new Grid
        {
            Style = (Style)App.Current.Resources[!_isPatient ? StyleConstants.ST_DEFAULT_GRID_STYLE : StyleConstants.ST_END_TO_END_GRID_STYLE],
            BackgroundColor = !_isPatient && MobileConstants.IsTablet ? (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR] :default, //todo: Color.Transparent,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star)},
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            }
        };

        _bodyLayout.Add(_subjectLabel, 0, 0);
        _bodyLayout.Add(_dateTimeLabel, 0, 2);
        _bodyLayout.Add(_listheader, 0, 3);
        AddCollectionView(_bodyLayout, customCellModel, 0, 4);
        _mainLayout = new Grid
        {
            Style = (Style)App.Current.Resources[_isPatient && MobileConstants.IsDevicePhone ? StyleConstants.ST_DEFAULT_GRID_STYLE : StyleConstants.ST_END_TO_END_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star)},
                new RowDefinition { Height = new GridLength(1, GridUnitType.Absolute) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition{ Width = new GridLength(1, GridUnitType.Star)},
                new ColumnDefinition{ Width = new GridLength(1, GridUnitType.Star)},
            }
        };
        _mainLayout.Add(_bodyLayout, 0, 0);
        Grid.SetColumnSpan(_bodyLayout, 2);
        Content = _mainLayout;
    }

    internal async Task LoadUIDataAsync(bool ShowHeader, string AppointmentID)
    {
        _appointmentData.Appointment.AppointmentID = Convert.ToInt64(AppointmentID, CultureInfo.InvariantCulture);
        await (ParentPage.PageService as AppointmentService).GetAppointmentsAsync(_appointmentData).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        _emptyListView.PageResources = ParentPage.PageData;
        if (_appointmentData.ErrCode == ErrorCode.OK)
        {
            if (_appointmentData.Appointment.AppointmentID > 0)
            {
                if (!ShowHeader)
                {
                    MenuView titleView = new MenuView(MenuLocation.Header, _appointmentData.Appointment.AppointmentTypeName, ShellMasterPage.CurrentShell.CurrentPage.IsAddEditPage);
                    if (MobileConstants.IsTablet)
                    {
                        await ParentPage.OverrideTitleViewAsync(titleView).ConfigureAwait(true);
                    }
                    else
                    {
                        await ParentPage.OverrideTitleViewAsync(titleView).ConfigureAwait(true);
                    }
                }
                bool showRejectedAcceptButton = (App._essentials.ConvertToLocalTime(_appointmentData.Appointment.FromDateTime.Value) - DateTime.Now).TotalMinutes >= Convert.ToInt32(ParentPage.GetSettingsValueByKey(SettingsConstants.S_APPOINTMENT_DEFAULT_TIMEOUT_KEY), CultureInfo.InvariantCulture);
                CollectionViewField.ItemsSource = _appointmentData.AppointmentParticipants;
                CollectionViewField.HeightRequest = _appointmentData.AppointmentParticipants.Count * CellRowHeight;
                _subjectLabel.Text = _appointmentData.Appointment.PageHeading;
                _informationLabel.Text = _appointmentData.Appointment.PageData;
                if (!string.IsNullOrWhiteSpace(_appointmentData.Appointment.PageData))
                {
                    _bodyLayout.Add(_informationLabel, 0, 1);
                }
                _dateTimeLabel.Text = string.Format(CultureInfo.InvariantCulture, Constants.APPOINTMENT_VIEW_DATE_TIME_STRING_SEQUENCE,
                    GetFormattedDate(App._essentials.ConvertToLocalTime(_appointmentData.Appointment.FromDateTime.Value), DateTimeType.ExtendedDateTime),
                    GetFormattedDate(App._essentials.ConvertToLocalTime(_appointmentData.Appointment.ToDateTime.Value), DateTimeType.Time));
                _listheader.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_PARTICIPANTS_TEXT_KEY) + " (" + _appointmentData.AppointmentParticipants.Count + ")";
                _joinButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_JOIN_TEXT_KEY);
                _acceptButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ACCEPT_KEY);
                _declineButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_DECLINE_TEXT_KEY);
                _joinButton.Clicked -= JoinButton_Clicked;
                _joinButton.Clicked += JoinButton_Clicked;
                _acceptButton.Clicked += AcceptButton_Clicked;
                _declineButton.Clicked += DeclineButton_Clicked;
                RenderJoinButton(showRejectedAcceptButton);
                RenderAcceptDeclineButton(showRejectedAcceptButton, ShowHeader);
                _appointmentHeader = _appointmentData.Appointment.AppointmentTypeName;
            }
            else
            {
                RenderErrorView(_bodyLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, false, true);
            }
        }
        else
        {
            RenderErrorView(_bodyLayout, _appointmentData.ErrCode.ToString(), false, 0, false, true);
        }
    }

    public bool CheckAppointmentPermission(string permission)
    {
        return (_appointmentData.Appointment.AccountID == _accountID 
            && ParentPage.CheckFeaturePermissionByCode(permission));
    }

    private void RenderJoinButton(bool showRejectedAcceptButton)
    {
        if (DateTime.Now <= App._essentials.ConvertToLocalTime(_appointmentData.Appointment.ToDateTime.Value)
            && !showRejectedAcceptButton
            && _appointmentData.Appointment.AppointmentTypeName == ParentPage.GetResourceValueByKey(ResourceConstants.R_VIDEO_APPOINTMENT_TYPE_KEY)
            && CheckAppointmentStatus()
            && ParentPage.CheckFeaturePermissionByCode(AppPermissions.AppointmentJoin.ToString())
            && _appointmentData.AppointmentParticipants.Exists(x =>
                (x.AccountID == _accountID || (_roleID == (int)RoleName.CareTaker && x.ParticipantID == _appointmentData.SelectedUserID))
                && x.AppointmentStatusID != ResourceConstants.R_REJECTED_STATUS_KEY
            ))
        {
            _mainLayout.Add(_joinButton, 0, 3);
            Grid.SetColumnSpan(_joinButton, 2);
        }
    }

    private void RenderAcceptDeclineButton(bool showRejectedAcceptButton, bool showHeader)
    {
        ShowHideAcceptAppointmentButton(showRejectedAcceptButton);
        ShowHideDeclineAppointmentButton(showRejectedAcceptButton);
        if (_acceptButton.IsVisible)
        {
            _mainLayout.Add(_acceptButton, 0, 2);
            if (_declineButton.IsVisible)
            {
                _mainLayout.Add(_declineButton, showHeader ? 1 : 0, showHeader ? 2 : 3);
                Grid.SetColumnSpan(_acceptButton, showHeader ? 1 : 2);
                Grid.SetColumnSpan(_declineButton, showHeader ? 1 : 2);
            }
            else
            {
                Grid.SetColumnSpan(_acceptButton, 2);
            }
        }
        else
        {
            _mainLayout.Add(_declineButton, 0, 3);
            Grid.SetColumnSpan(_declineButton, 2);
        }
        if (showHeader && _declineButton.IsVisible)
        {
            AddSeparatorView(_mainLayout, 0, 1);
            Grid.SetColumnSpan(Separator, 2);
        }
        if (showHeader)
        {
            _acceptButton.Margin = new Thickness(0);
            _declineButton.Margin = new Thickness(0);
        }
    }

    private void ShowHideDeclineAppointmentButton(bool showRejectedAcceptButton)
    {
        _declineButton.IsVisible = showRejectedAcceptButton && CheckAppointmentStatus()
            && ParentPage.CheckFeaturePermissionByCode(AppPermissions.AppointmentDecline.ToString())
            && _appointmentData.AppointmentParticipants.Exists(x =>
                (x.AccountID == _accountID || (_roleID == (int)RoleName.CareTaker && x.ParticipantID == _appointmentData.SelectedUserID))
                && x.AppointmentStatusID != ResourceConstants.R_REJECTED_STATUS_KEY
            );
    }

    private void ShowHideAcceptAppointmentButton(bool showRejectedAcceptButton)
    {
        _acceptButton.IsVisible = showRejectedAcceptButton && CheckAppointmentStatus()
            && _appointmentData.AppointmentParticipants.Exists(x =>
                (x.AccountID == _accountID || (_roleID == (int)RoleName.CareTaker && x.ParticipantID == _appointmentData.SelectedUserID))
                && x.AppointmentStatusID != ResourceConstants.R_ACCEPTED_STATUS_KEY
                && x.AppointmentStatusID != ResourceConstants.R_REJECTED_STATUS_KEY
            );
    }

    private bool CheckAppointmentStatus()
    {
        return _appointmentData.Appointment.AppointmentStatusID != ResourceConstants.R_REJECTED_STATUS_KEY
            && _appointmentData.Appointment.AppointmentStatusID != ResourceConstants.R_CANCELLED_STATUS_KEY
            && _appointmentData.Appointment.AppointmentStatusID != ResourceConstants.R_MISSED_STATUS_KEY;
    }

    private async void AcceptButton_Clicked(object sender, EventArgs e)
    {
        await SaveAppointmentAsync(true).ConfigureAwait(true);
    }

    private async void DeclineButton_Clicked(object sender, EventArgs e)
    {
        await ParentPage.DisplayMessagePopupAsync(ResourceConstants.R_DECLINE_ERROR_APPOINTMENT_KEY, OnMessgeViewActionClicked, true, true, false).ConfigureAwait(true);
    }

    private async void OnMessgeViewActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 0: break;
            case 1:
                ParentPage.OnClosePupupAction(sender, e);
                await SaveAppointmentAsync(false).ConfigureAwait(true);
                break;
            case 2:
                ParentPage.OnClosePupupAction(sender, e);
                break;
            default:// to do
                break;
        }
    }

    private async Task SaveAppointmentAsync(bool isAccepted)
    {
        if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            AppointmentDTO appointmentDTO = new AppointmentDTO
            {
                Appointment = new AppointmentModel
                {
                    AppointmentID = _appointmentData.Appointment.AppointmentID,
                    FromDateTime = _appointmentData.Appointment.FromDateTime,
                    AppointmentStatusID = isAccepted ? ResourceConstants.R_ACCEPTED_STATUS_KEY : ResourceConstants.R_REJECTED_STATUS_KEY,
                    AccountID = _accountID
                }
            };
            AppHelper.ShowBusyIndicator = true;
            await (ParentPage.PageService as AppointmentService).UpdateAppointmentAsync(appointmentDTO, new CancellationToken()).ConfigureAwait(true);
            ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(appointmentDTO.ErrCode.ToString()), appointmentDTO.ErrCode == ErrorCode.OK);
            AppHelper.ShowBusyIndicator = false;
            if (MobileConstants.IsDevicePhone)
            {
                await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
            }
            if (appointmentDTO.ErrCode == ErrorCode.OK)
            {
                await NavigateBackAsync().ConfigureAwait(true);
            }
        }
    }

    private async Task NavigateBackAsync()
    {
        if (!_isPatient)
        {
            //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
        }
        else
        {
            if (MobileConstants.IsTablet)
            {
                UnLoadUIData();
                await LoadUIDataAsync(false, _appointmentData.Appointment.AppointmentID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
            }
            else
            {
                await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
            }
        }
    }

    private async void JoinButton_Clicked(object sender, EventArgs e)
    {
        ////_joinButton.Clicked -= JoinButton_Clicked;
        if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY) && await PermissionHelper.CheckPermissionStatusAsync(Permission.Camera, ParentPage).ConfigureAwait(true) &&
                await PermissionHelper.CheckPermissionStatusAsync(Permission.Microphone, ParentPage).ConfigureAwait(true) &&
                await PermissionHelper.CheckPermissionStatusAsync(Permission.Storage, ParentPage).ConfigureAwait(true))
        {
            AppHelper.ShowBusyIndicator = true;
            ParticipantsModel userDetails = _appointmentData.AppointmentParticipants.FirstOrDefault(X =>
                X.AccountID == _accountID || (_roleID == (int)RoleName.CareTaker && X.ParticipantID == _appointmentData.SelectedUserID));
            VideoDTO videoData = new VideoDTO { Video = new VideoModel(), AddedBy = userDetails.AccountID.ToString(CultureInfo.InvariantCulture), LastModifiedBy = userDetails.FirstName };
            videoData.Video.VideoRoomID = _appointmentData.Appointment.AppointmentAddress;
            await new VideoService(App._essentials).SyncVideoSessionFromServerAsync(videoData).ConfigureAwait(true);
            if (videoData.ErrCode == ErrorCode.OK)
            {
                if (!string.IsNullOrWhiteSpace(videoData.Video.VideoToken))
                {
                    AppointmentModel appointmentData = new AppointmentModel
                    {
                        RoomID = _appointmentData.Appointment.AppointmentAddress,
                        PageHeading = _appointmentData.Appointment.PageHeading,
                        AppointmentTypeName = userDetails.FullName,
                        AppointmentStatusID = userDetails.ImageBase64,
                        VideoToken = videoData.Video.VideoToken,
                        ClientID = string.IsNullOrWhiteSpace(videoData.Video.ApplicationID) ? string.Empty : videoData.Video.ApplicationID,
                        AppointmentStatusName = string.IsNullOrWhiteSpace(videoData.Video.VideoLink) ? string.Empty : videoData.Video.VideoLink,
                        AccountID = userDetails.AccountID,
                        ServiceType = videoData.Video.ServiceType,
                        ErrCode = ErrorCode.OK
                    };
                    //todo:Microsoft.Maui.Controls.Application.Current.Properties[StorageConstants.PR_SELECTED_VIDEO_SERVICE_KEY] = appointmentData;
                    AppointmentDTO appointmentDTO = new AppointmentDTO
                    {
                        Appointment = new AppointmentModel
                        {
                            AppointmentID = _appointmentData.Appointment.AppointmentID,
                            AppointmentStatusID = ResourceConstants.R_INPROGRESS_STATUS_KEY,
                            AccountID = _accountID
                        }
                    };
                    await (ParentPage.PageService as AppointmentService).UpdateAppointmentAsync(appointmentDTO, new CancellationToken()).ConfigureAwait(true);
                    OnVideoCallJoin?.Invoke(new object(), new EventArgs());
                    await ShellMasterPage.CurrentShell.Navigation.PushAsync(new VideoCallingPage(_appointmentData.Appointment.AppointmentID.ToString(CultureInfo.InvariantCulture)), false).ConfigureAwait(true);
                }
                else
                {
                    AppHelper.ShowBusyIndicator = false;
                    ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(ErrorCode.TokenExpired.ToString()));
                }
            }
            else
            {
                AppHelper.ShowBusyIndicator = false;
                ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(videoData.ErrCode.ToString()));
            }
        }
        ////_joinButton.Clicked += JoinButton_Clicked;
    }

    private string GetFormattedDate(DateTimeOffset dateTime, DateTimeType dateTimeType)
    {
        return dateTimeType == DateTimeType.ExtendedDateTime
            ? GenericMethods.GetDateTimeBasedOnCulture(dateTime, dateTimeType, "dd", ParentPage.GetSettingsValueByKey(SettingsConstants.S_MONTH_FORMAT_KEY), string.Empty)
            : GenericMethods.GetDateTimeBasedOnCulture(dateTime, dateTimeType, string.Empty, string.Empty, string.Empty);
    }

    internal void UnLoadUIData()
    {
        _joinButton.Clicked -= JoinButton_Clicked;
        _declineButton.Clicked -= DeclineButton_Clicked;
    }
}