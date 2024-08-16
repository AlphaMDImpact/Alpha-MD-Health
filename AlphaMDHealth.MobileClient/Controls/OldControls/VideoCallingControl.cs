using System.Globalization;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using FlowDirection = Microsoft.Maui.FlowDirection;
namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// video calling Control based on service type 
    /// </summary>
    public class VideoCallingControl : ContentView
    {
        private readonly Grid _buttonGrid;
        private readonly Grid _videoGrid;
        private readonly Grid _mainGrid;
        private readonly SvgImageButtonView _endButton;
        private SvgImageButtonView _videoButton;
        private SvgImageButtonView _audioButton;
        private readonly CustomImageControl _profileImage;
        private readonly CustomLabelControl _participentNameLabel;

        /// <summary>
        /// ShowSpeakerButton
        /// </summary>
        public bool ShowSpeakerButton { get; set; }

        //todo
        /// <summary>
        /// VideoView
        /// </summary>
        //public VideoView VideoView { get; }

        /// <summary>
        /// mute unmute video
        /// </summary>
        public bool IsVideoEnabled { get; set; } = true;

        /// <summary>
        /// mute unmute audio
        /// </summary>
        public bool IsAudioEnabled { get; set; } = true;

        /// <summary>
        /// On disconnect call
        /// </summary>
        public event EventHandler<EventArgs> OnDisconnectCall;

        /// <summary>
        /// video calling Control based on service type 
        /// </summary>
        /// <param name="selectedService">service type Vidyo.io/twilio/</param>
        public VideoCallingControl(ServiceType selectedService)
        {
            _endButton = CreateImageButton(ImageConstants.I_VIDEO_CALL_END_ICON);
            _videoButton = CreateImageButton(ImageConstants.I_VIDEO_CAMERA_DISABLE_ICON_PNG);
            _audioButton = CreateImageButton(ImageConstants.I_MICROPHONE_DISABLE_ICON);
            //todo
            //VideoView = VideoManager.GetVideoView(selectedService);
            _buttonGrid = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                ColumnSpacing = 20,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star)},
                },
                ColumnDefinitions =
                {
                        new ColumnDefinition { Width =  new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width =  GridLength.Auto },
                        new ColumnDefinition { Width =  GridLength.Auto },
                        new ColumnDefinition { Width =  GridLength.Auto },
                        new ColumnDefinition { Width =  GridLength.Auto },
                        new ColumnDefinition { Width =  new GridLength(1, GridUnitType.Star) },
                },
            };
            _buttonGrid.Add(_audioButton, 1, 0);
            _buttonGrid.Add(_endButton, 2, 0);
            _buttonGrid.Add(_videoButton, 3, 0);
            _videoGrid = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                // Video view is currently set to LTR even if Arabic is selected, this is due to a bug in RTL for Vidyo.io
                FlowDirection = FlowDirection.LeftToRight,
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height =  GridLength.Auto },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                },
            };

            _profileImage = new CustomImageControl(AppImageSize.ImageSizeXXXL, AppImageSize.ImageSizeXXXL, string.Empty, ImageConstants.I_VIDEO_APPOINTMENT_ICON_PNG, true) { Margin = new Thickness(0, 80, 0, 0) };
            _participentNameLabel = new CustomLabelControl(LabelType.PrimarySmallCenter);
            //todo
            //_videoGrid.Add(VideoView, 0, 0);
            _videoGrid.Add(_buttonGrid, 0, 1);
            _mainGrid = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                BackgroundColor = Color.FromArgb(Application.Current.Resources[StyleConstants.ST_SECONDARY_APP_COLOR].ToString()),
                RowDefinitions =
                {
                    new RowDefinition { Height =  GridLength.Auto },
                    new RowDefinition { Height =  GridLength.Auto },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                Children =
                {
                    ////{ _profileImage,0,0 },
                    {
                        _participentNameLabel
                      
                    },
                    {
                        _videoGrid

                    }
                }
            };
            Content = _mainGrid;
        }

        /// <summary>
        /// load UI 
        /// </summary>
        /// <param name="appointmentModel">appoitment data</param>
        public async Task LoadUI(AppointmentModel appointmentModel)
        {
            if (appointmentModel.AppointmentTypeImage != null)
            {
                //// _profileImage.Source = ImageSource.FromStream(() => new MemoryStream(appointmentModel.AppointmentStatusID));
            }
            else if (string.IsNullOrWhiteSpace(appointmentModel.AppointmentStatusID))
            {
                _profileImage.Source = appointmentModel.AppointmentStatusID;
            }
            else
            {
                _profileImage.DefaultValue = appointmentModel.AppointmentStatusID;
            }
            _participentNameLabel.Text = appointmentModel.PageHeading;
            //todo
            //VideoView.VideoCallStatusChanged += Videoview_OnVideoCallStatusChanged;
            await Task.Delay(200).ConfigureAwait(true);
            StartCall(appointmentModel);
        }

        /// <summary>
        /// UnloadUI
        /// </summary>
        public async Task UnloadUIAsync()
        {
            //todo
            //VideoView.VideoCallStatusChanged -= Videoview_OnVideoCallStatusChanged;
            _endButton.Clicked -= OnCancelCallButtonClicked;
            _videoButton.Clicked -= OnVideoClicked;
            _audioButton.Clicked -= OnAudioClicked;
            //todo
            //await VideoView.OnUnloadAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// disconnect call
        /// </summary>
        public async Task DisconnectCallAsync()
        {
            //todo
            //await MainThread.InvokeOnMainThreadAsync(async () =>
            //{
            //    AppHelper.ShowBusyIndicator = true;
            //    // disconnect call
            //    await VideoView.DisconnectCallAsync().ConfigureAwait(true);
            //    await Task.Delay(1000).ConfigureAwait(true);
            //});
        }

        private void StartCall(AppointmentModel appointmentModel)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                //_connect video
                //todo
                //await VideoView.ConnectCallAsync(appointmentModel.ClientID, appointmentModel.VideoToken, appointmentModel.RoomID, appointmentModel.AppointmentStatusName,
                //    appointmentModel.ServiceType == ServiceType.Vidyo_Io ? appointmentModel.AppointmentTypeName : appointmentModel.AccountID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
                _buttonGrid.Children.Remove(_audioButton);
                ChangeImage(_audioButton, ImageConstants.I_MICROPHONE_ICON_PNG);
                _buttonGrid.Add(_audioButton, 1, 0);
                _buttonGrid.Children.Remove(_videoButton);
                ChangeImage(_videoButton, ImageConstants.I_VIDEO_CAMERA_ICON_PNG);
                _buttonGrid.Add(_videoButton, 3, 0);
                _videoButton.Clicked += OnVideoClicked;
                _audioButton.Clicked += OnAudioClicked;
                _endButton.Clicked += OnCancelCallButtonClicked;
                _endButton.IsEnabled = _audioButton.IsEnabled = _videoButton.IsEnabled = false;
                AppHelper.ShowBusyIndicator = false;
            });
        }

        private void OnAudioClicked(object sender, EventArgs e)
        {
            _audioButton.Clicked -= OnAudioClicked;
            if (IsAudioEnabled)
            {
                //// _buttonGrid.Children.Remove(_audioButton);
                ChangeImage(_audioButton, ImageConstants.I_MICROPHONE_DISABLE_ICON_PNG);
                //// _audioButton = CreateImageButton(LibImageConstants.I_MICROPHONE_DISABLE_ICON_SVG);
                //// _buttonGrid.Add(_audioButton, 1, 0);
            }
            else
            {

                ChangeImage(_audioButton, ImageConstants.I_MICROPHONE_ICON_PNG);
                ////_buttonGrid.Children.Remove(_audioButton);
                //// _audioButton = CreateImageButton(LibImageConstants.I_MICROPHONE_ICON_SVG);
                //// _buttonGrid.Add(_audioButton, 1, 0);
            }
            IsAudioEnabled = !IsAudioEnabled;
            //todo
            //VideoView.TurnOnOffMicrophoneAsync(IsAudioEnabled);
            _audioButton.Clicked += OnAudioClicked;
        }

        private void OnVideoClicked(object sender, EventArgs e)
        {
            _videoButton.Clicked -= OnVideoClicked;
            if (IsVideoEnabled)
            {
                ChangeImage(_videoButton, ImageConstants.I_VIDEO_CAMERA_DISABLE_ICON_PNG);
                ////_buttonGrid.Children.Remove(_videoButton);
                //// _videoButton = CreateImageButton(LibImageConstants.I_VIDEO_CAMERA_DISABLE_ICON_SVG);
                //// _buttonGrid.Add(_videoButton, 3, 0);
            }
            else
            {
                ChangeImage(_videoButton, ImageConstants.I_VIDEO_CAMERA_ICON_PNG);
                //// _buttonGrid.Children.Remove(_videoButton);
                //// _videoButton = CreateImageButton(LibImageConstants.I_VIDEO_CAMERA_ICON_SVG);
                //// _buttonGrid.Add(_videoButton, 3, 0);
            }
            IsVideoEnabled = !IsVideoEnabled;
            //todo
            //VideoView.TurnOnOffVideoAsync(IsVideoEnabled);
            _videoButton.Clicked += OnVideoClicked;
        }

        private void Videoview_OnVideoCallStatusChanged(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                //todo
                //if (sender is VideoState view)
                //{
                //    switch (view)
                //    {
                //        case VideoState.StateConnected:
                //            await Task.Delay(10).ConfigureAwait(true);
                //            _mainGrid.Children.Remove(_profileImage);
                //            _mainGrid.Children.Remove(_participentNameLabel);
                //            _videoGrid.Children.Remove(_buttonGrid);
                //            _videoGrid.Add(_buttonGrid, 0, 0);
                //            _endButton.IsEnabled = _audioButton.IsEnabled = _videoButton.IsEnabled = true;
                //            Grid.SetRowSpan(VideoView, 2);
                //            break;
                //        case VideoState.StateConnectionFailure:
                //        case VideoState.StateDisconnected:
                //        case VideoState.StateDisconnectedUnexpected:
                //            OnDisconnectCall?.Invoke(new object(), new EventArgs());
                //            break;
                //        default:
                //            // for future implementation
                //            break;
                //    }
                //}
            });
        }

        private async void OnCancelCallButtonClicked(object sender, EventArgs e)
        {
            await DisconnectCallAsync().ConfigureAwait(true);
        }

        private void ChangeImage(SvgImageButtonView view, string svgPath)
        {
            //todo
            //view.ImageSource = SvgImageSource.FromSvgResource(AppStyles.NameSpaceImage + svgPath.Replace(AppStyles.NameSpaceImage, string.Empty), (double)AppImageSize.ImageSizeXL, (double)AppImageSize.ImageSizeXL, Color.Default);
        }
        private static SvgImageButtonView CreateImageButton(string svgPath)
        {
            return new SvgImageButtonView(svgPath, AppImageSize.ImageSizeXL, AppImageSize.ImageSizeXL)
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };
        }
    }
}

