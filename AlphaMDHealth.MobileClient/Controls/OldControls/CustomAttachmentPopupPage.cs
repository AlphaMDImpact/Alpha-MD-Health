using System.Globalization;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Controls;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// custom attachment Popup Pa
    /// </summary>
    public class CustomAttachmentPopupPage : ContentPage
    {
        private readonly CustomAttachmentModel _chatDetail;
        private readonly ChatAttachmentControl _chatAttachment;
        private readonly CustomLabelControl _title;
        private readonly SvgImageButtonView _closeButton;
        private readonly BasePage _parent;
        private readonly DXPopup _popUpPage;
        /// <summary>
        /// on click event of Send Button
        /// </summary>
        public event EventHandler<EventArgs> OnSendButtonClicked;

        /// <summary>
        /// Attachment upload popup page
        /// </summary>
        /// <param name="chatDetail">Chat detail data used to update attachment info</param>
        /// <param name="parent">Instance of parent view</param>
        public CustomAttachmentPopupPage(CustomAttachmentModel chatDetail, BasePage parent)
        {
            _parent = parent;
            _chatDetail = chatDetail;
            var padding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
            _chatAttachment = new ChatAttachmentControl
            {
                PageResources = _parent.PageData,
                ControlResourceKey = chatDetail.ControlResourceKey
            };
            AutomationProperties.SetIsInAccessibleTree(_chatAttachment, true);
            _closeButton = new SvgImageButtonView(ImageConstants.I_CLOSE_PNG, AppImageSize.ImageSizeS, AppImageSize.ImageSizeS)
            {
                // Margin added to close button avoid adding a separate header grid
                Margin = new OnIdiom<Thickness>
                {
                    Phone = new Thickness(padding, padding * 1.5, padding, 0),
                    Tablet = new Thickness(padding)
                }
            };
            AutomationProperties.SetIsInAccessibleTree(_closeButton, true);
            _title = new CustomLabelControl(LabelType.PrimaryMediumLeft);

            AutomationProperties.SetIsInAccessibleTree(_title, true);
            // Padding added for popup view
            Padding = new OnIdiom<Thickness>
            {
                Phone = new Thickness(0, padding * 3.5, 0, 0),
                Tablet = new Thickness(0.15 * App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, 0.0), 0.05 * App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, 0.0))
            };

            var mainLayout = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            mainLayout.Add(_title, 0, 0);
            mainLayout.Add(_closeButton, 1, 0);
            if (MobileConstants.IsTablet)
            {
                _title.HorizontalOptions = LayoutOptions.CenterAndExpand;
                var seperator = new BoxView
                {
                    HeightRequest = 1,
                    BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE]
                };
                AutomationProperties.SetIsInAccessibleTree(seperator, true);
                mainLayout.Add(seperator, 0, 1);
                Grid.SetColumnSpan(seperator, 2);
            }
            else
            {
                // Margin added to close button avoid adding a separate header grid
                _title.Margin = new Thickness(padding, padding * 1.5, padding, 0);
            }
            AutomationProperties.SetIsInAccessibleTree(mainLayout, true);
            mainLayout.Add(_chatAttachment, 0, 2);
            Grid.SetColumnSpan(_chatAttachment, 2);

            _popUpPage = new DXPopup
            {
                CloseOnScrimTap = false,
                // Style = (Style)Application.Current.Resources[LibStyleConstants.ST_PANCAKE_STYLE],
                CornerRadius = new OnIdiom<CornerRadius> { Phone = new CornerRadius(padding * 2, padding * 2, 0, 0), Tablet = new CornerRadius(10) },
                BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR],
                Content = mainLayout
            };
            Content = _popUpPage;
        }

        /// <summary>
        /// On Appearing
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _chatAttachment.PageResources = _parent.PageData;
            _chatAttachment.Value = _chatDetail;
            _title.Text = _parent.GetResourceValueByKey(ResourceConstants.R_ATTACH_FILE_KEY);
            _chatAttachment.OnSendClick += OnSendClick;
            _closeButton.Clicked += OnPopupClose;
            if (string.IsNullOrWhiteSpace(_closeButton.AutomationId))
            {

                _closeButton.AutomationId = $"{ShellMasterPage.CurrentShell.CurrentPage.GetType().Name}{Constants.AUTOMATION_ID_SEPRATOR}{nameof(_closeButton)}";
                AutomationProperties.SetName(_closeButton, _closeButton.AutomationId);
            }
            MessagingCenter.Subscribe<object, string>(this, Constants.KEYBOARD_DIAPPEARS, (sender, eargs) =>
            {
                if (App._essentials.GetPreferenceValue(StorageConstants.PR_IS_DONE_CLICK_KEY, false))
                {
                    _chatAttachment.OnSendClick -= OnSendClick;
                    if (_chatAttachment.ValidateControl(sender, new EventArgs()))
                    {
                        OnSendClick(sender, new EventArgs());
                        App._essentials.SetPreferenceValue(StorageConstants.PR_IS_DONE_CLICK_KEY, false);
                    }
                }
            });
            AppHelper.ShowBusyIndicator = false;
        }

        /// <summary>
        /// On Disappearing
        /// </summary>
        protected override void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<object, string>(this, Constants.KEYBOARD_DIAPPEARS);
            _chatAttachment.OnSendClick -= OnSendClick;
            _closeButton.Clicked -= OnPopupClose;
            base.OnDisappearing();
        }

        private void OnPopupClose(object sender, EventArgs e)
        {
            _chatDetail.AttachmentBase64 = string.Empty;
            _chatDetail.FileType = AppFileExtensions.none;
            _chatDetail.FileName = string.Empty;
            _popUpPage.IsOpen = false;
            //await Navigation.PopPopupAsync().ConfigureAwait(true);
        }

        private void OnSendClick(object sender, EventArgs e)
        {
            AppHelper.ShowBusyIndicator = true;
            OnSendButtonClicked.Invoke(_chatAttachment.Value, new EventArgs());
            _popUpPage.IsOpen = true;
            //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
        }
    }
}