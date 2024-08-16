using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Mopups.Services;
using System.Globalization;

namespace AlphaMDHealth.MobileClient
{
    public class AmhAttachmentPreviewPage : BasePopupPage
    {
        private AttachmentModel _fileDetail;
        private AmhImageControl _closeButton;
        private readonly AmhLabelControl _title;
        private readonly BasePage _parent;
        private AmhImageControl _imageControl;
        private AmhButtonControl _deleteButton;

        /// <summary>
        /// show delete button
        /// </summary>
        public bool ShowDeleteButton { get; set; }

        /// <summary>
        /// on click event of Delete Button
        /// </summary>
        public event EventHandler<EventArgs> OnDeleteButtonClicked;

        /// <summary>
        /// upload popup page
        /// </summary>
        /// <param name="fileDetail">attachment info</param>
        public AmhAttachmentPreviewPage(AttachmentModel fileDetail, BasePage parent)
        {
            _parent = parent;
            _fileDetail = fileDetail;
            var padding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
            BackgroundColor = Color.FromArgb(StyleConstants.OVERLAY_COLOR);

            _title = new AmhLabelControl(FieldTypes.PrimarySmallHStartVCenterLabelControl);
            _title.Margin = new Thickness(padding, padding * 1.5, padding, padding);
            _closeButton = new AmhImageControl(FieldTypes.ImageControl)
            {
                Icon = ImageConstants.I_CLOSE_PNG,
                ImageWidth = AppImageSize.ImageSizeS,
                ImageHeight = AppImageSize.ImageSizeS,
                Margin = new Thickness(padding, padding * 1.5, padding, padding)
            };
            _imageControl = new AmhImageControl(FieldTypes.ImageControl);
            Border imageFrame = new Border()
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_UPLOAD_FRAME_STYLE],
                Margin = new Thickness(padding),
                Padding = new Thickness(padding),
                Content = _imageControl,
                HeightRequest = padding * 15
            };

            _deleteButton = new AmhButtonControl(FieldTypes.DeleteButtonControl)
            {
                Value = LibResources.GetResourceValueByKey(_parent.PageData.Resources, ResourceConstants.R_DELETE_ACTION_KEY),
            };

            var mainLayout = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                BackgroundColor = Color.FromArgb(StyleConstants.DEFAULT_BACKGROUND_COLOR),
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            mainLayout.Add(_title, 0, 0);
            mainLayout.Add(_closeButton, 1, 0);
            mainLayout.Add(imageFrame, 0, 2);
            Grid.SetColumnSpan(imageFrame, 2);
            mainLayout.Add(_deleteButton, 0, 3);
            Grid.SetColumnSpan(_deleteButton, 3);


            Border layoutBorder = new Border
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_STYLE],
                Content = mainLayout
            };

            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                _title.HorizontalOptions = LayoutOptions.CenterAndExpand;
                layoutBorder.Margin = new Thickness(padding * 9, padding * 3);
                imageFrame.Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_FRAME_STYLE];
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
                layoutBorder.Margin = new Thickness(0, padding * 20, 0, 0);
            }
            Content = layoutBorder;
        }

        /// <summary>
        /// On Appearing
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _title.Value = LibResources.GetResourceValueByKey(_parent.PageData.Resources, ResourceConstants.R_ATTACH_FILE_KEY);
            //todo:_imageControl.Source = _fileDetail.ImageBytes;
            _imageControl.Icon = _fileDetail.FileIcon;
            if (!GenericMethods.IsImageFile(_fileDetail.FileExtension))
            {
                _imageControl.ImageWidth = AppImageSize.ImageSizeXL;
                _imageControl.ImageHeight = AppImageSize.ImageSizeXL;
            }
            if (string.IsNullOrWhiteSpace(_fileDetail.FileIcon))//todo:&& _fileDetail.ImageBytes == null)
            {
                
                //_imageControl.ImageWidth = AppImageSize.ImageSizeXL;
                //_imageControl.ImageHeight = AppImageSize.ImageSizeXL;
                _imageControl.Value = _fileDetail.FileValue;
            }

            var fileTapGestureRecognizer = new TapGestureRecognizer();
            fileTapGestureRecognizer.Tapped += OnImageTap;
            _imageControl.GestureRecognizers.Add(fileTapGestureRecognizer);
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnPopupClose;
            _closeButton.GestureRecognizers.Add(tapGestureRecognizer);
            if (ShowDeleteButton)
            {
                _deleteButton.OnValueChanged += OnDeleteButtonClick;
            }
            else
            {
                _deleteButton.IsVisible = false;
                _deleteButton.OnValueChanged -= OnDeleteButtonClick;
            }
        }

        private async void OnImageTap(object sender, TappedEventArgs e)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            //todo:if(_fileDetail.FileBytes != null  && !GenericMethods.IsImageFile(_fileDetail.FileExtension))
            {
                //todo:using var stream = new MemoryStream(_fileDetail.FileBytes);
                //var path = await FileSaver.SaveAsync(_fileDetail.FileName, stream, cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// On Disappearing
        /// </summary>
        protected override void OnDisappearing()
        {
            _closeButton.GestureRecognizers.Clear();
            _imageControl.GestureRecognizers.Clear();
            base.OnDisappearing();
        }

        private async void OnPopupClose(object sender, EventArgs e)
        {
            await MopupService.Instance.PopAsync();
        }

        private void OnDeleteButtonClick(object sender, EventArgs e)
        {
            OnDeleteButtonClicked.Invoke(_fileDetail?.FileName, new EventArgs());
        }
    }
}