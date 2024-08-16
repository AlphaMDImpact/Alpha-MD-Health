using System.Globalization;
using System.Runtime.CompilerServices;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represemnts Custom Upload Control
    /// </summary>
    public class CustomUploadControl : BaseAttachmentView
    {
        private const string _RENDERER = "Renderer";
        private readonly CustomImageControl _imageUpload;
        private readonly Grid _mainGrid;
        private readonly CustomLabel _cellHeader;
        private readonly CustomLabel _cellDescription;
        private readonly CustomButton _deleteUpload;
        private readonly double _controlMargin = (double)Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING];

        /// <summary>
        /// On File delete Event
        /// </summary>
        public event EventHandler<EventArgs> OnFileDelete;

        /// <summary>
        /// ControlType property
        /// </summary>
        public FieldTypes ControlType { get; set; }

        /// <summary>
        /// Value property
        /// </summary>
        public string Value
        {
            get
            {
                return _base64String;
            }
            set
            {
                _base64String = value;
                AssignSource();
            }
        }

        /// <summary>
        /// Patient readings list
        /// </summary>
        /// <param name="page">Instance of base page</param>
        /// <param name="parameters">View parameters</param>
        public CustomUploadControl(BasePage page, object parameters) : base(page, parameters)
        {
            _imageUpload = new CustomImageControl(
                GenericMethods.MapValueType<AppImageSize>(GetParameterValue(Constants.IMAGE_WIDTH_CONSTANT))
                , GenericMethods.MapValueType<AppImageSize>(GetParameterValue(Constants.IMAGE_HEIGHT_CONSTANT))
                , GenericMethods.MapValueType<string>(GetParameterValue(Constants.BASE64_STRING_CONSTANT))
                , GenericMethods.MapValueType<string>(GetParameterValue(Constants.DEFAULT_VALUE_CONSTANT))
                , GenericMethods.MapValueType<bool>(GetParameterValue(Constants.IS_CIRCLE_CONSTANT)));
            AutomationProperties.SetIsInAccessibleTree(_imageUpload, true);
            _cellHeader = new CustomLabelControl(LabelType.ListHeaderStyle);
            AutomationProperties.SetIsInAccessibleTree(_cellHeader, true);
            _cellDescription = new CustomLabelControl(LabelType.PrimarySmallLeft)
            {
                Padding = new Thickness(0, 0, 0, 4),
            };
            AutomationProperties.SetIsInAccessibleTree(_cellDescription, true);
            _deleteUpload = new CustomButtonControl(ButtonType.DeleteWithMargin);
            _mainGrid = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                ColumnSpacing = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING],
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto },
                }
            };
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
            _mainGrid.GestureRecognizers.Add(tapGestureRecognizer);
        }

        /////// <summary>
        /////// CustomUploadControl Class constructor
        /////// </summary>
        /////// <param name="ParentPage">name of page</param>
        /////// <param name="imageWidth">Iameg Width</param>
        /////// <param name="imageHeight">Image Height</param>
        /////// <param name="base64string">Image base 64 string</param>
        /////// <param name="defaultValue">default value</param>
        /////// <param name="isCircle">Is circle required</param>
        ////public CustomUploadControl(BasePage ParentPage, AppImageSize imageWidth, AppImageSize imageHeight, string base64string, string defaultValue, bool isCircle) : base(ParentPage, object parameters)
        ////{
        ////    _imageUpload = new CustomImageControl(imageWidth, imageHeight, base64string, defaultValue, isCircle);
        ////    AutomationProperties.SetIsInAccessibleTree(_imageUpload, true);
        ////    _cellHeader = new CustomLabelControl(LabelType.ListHeaderStyle);
        ////    AutomationProperties.SetIsInAccessibleTree(_cellHeader, true);
        ////    _cellDescription = new CustomLabelControl(LabelType.PrimarySmallLeft)
        ////    {
        ////        Padding = new Thickness(0, 0, 0, 4),
        ////    };
        ////    AutomationProperties.SetIsInAccessibleTree(_cellDescription, true);
        ////    _mainGrid = new Grid
        ////    {
        ////        Style = (Style)Application.Current.Resources[LibStyleConstants.ST_END_TO_END_GRID_STYLE],
        ////        ColumnSpacing = (double)Application.Current.Resources[LibStyleConstants.ST_APP_PADDING],
        ////        RowDefinitions =
        ////        {
        ////            new RowDefinition { Height = GridLength.Auto },
        ////            new RowDefinition { Height = GridLength.Auto },
        ////        },
        ////        ColumnDefinitions =
        ////        {
        ////            new ColumnDefinition { Width = GridLength.Auto },
        ////            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
        ////        }
        ////    };
        ////    var tapGestureRecognizer = new TapGestureRecognizer();
        ////    tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
        ////    _imageUpload.GestureRecognizers.Add(tapGestureRecognizer);
        ////}

        /// <summary>
        /// method to Set Empty View Text
        /// </summary>
        public void SetEmptyViewText()
        {
            if (ControlType == FieldTypes.UploadControl)
            {
                _cellHeader.Text = _parent.GetResourceValueByKey(ResourceConstants.R_IMAGE_UPLOAD_HEADER_TEXT_KEY);
            }
            else
            {
                if (ControlType == FieldTypes.UploadControl)
                {
                    _cellHeader.Text = _parent.GetResourceValueByKey(ResourceConstants.R_FILE_UPLOAD_HEADER_TEXT_KEY);
                }
            }
            _deleteUpload.Text = _parent.GetResourceValueByKey(ResourceConstants.R_DELETE_ACTION_KEY);
        }

        /// <summary>
        /// This method Invoked on property chnaged
        /// </summary>
        /// <param name="propertyName">Name Of the property changed</param>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == _RENDERER)
            {
                _deleteUpload.Clicked += DeleteUpload_Clicked;
                if (ControlType == FieldTypes.UploadControl)
                {
                    _mainGrid.Add(_cellHeader, 0, 0);
                    _mainGrid.Add(_imageUpload, 0, 1);
                    ImageEmptyView(_controlMargin);
                    Content = _mainGrid;
                }
                else
                {
                    if (_base64String == ImageConstants.I_UPLOAD_ICON_PNG)
                    {
                        _mainGrid.Add(_imageUpload, 0, 0);
                        _mainGrid.Add(_cellHeader, 1, 0);
                        _mainGrid.Add(_deleteUpload, 2, 0);

                        FileEmptyView();
                        Border pancakeView = new Border
                        {
                            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_PANCAKE_STYLE],
                            Margin = new Thickness(0, 0, 0, _controlMargin),
                            Content = _mainGrid
                        };
                        Content = pancakeView;
                    }
                }
            }
        }

        private void DeleteUpload_Clicked(object sender, EventArgs e)
        {
            DeleteUploads();
        }

        private void FileEmptyView()
        {
            _cellHeader.VerticalOptions = LayoutOptions.Center;
            Grid.SetRowSpan(_cellHeader, 2);
            Grid.SetRowSpan(_imageUpload, 2);
            _deleteUpload.IsVisible = false;
            Grid.SetRowSpan(_deleteUpload, 2);
        }

        private void ImageEmptyView(double controlMargin)
        {
            Grid.SetColumnSpan(_cellHeader, 2);
            Grid.SetColumnSpan(_imageUpload, 2);
            _cellHeader.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_TEXT_COLOR];
            _cellHeader.Margin = new Thickness(0, 0, 0, controlMargin);
            _mainGrid.Margin = new Thickness(0, 0, 0, controlMargin);
        }


        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            List<string> actions = Actionlist(ControlType);
            await ImageSourceSelectionAsync(actions).ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(_base64String) && _base64String.Length > 100)
            {
                _deleteUpload.IsVisible = true;
                if (GenericMethods.IsExtensionSupported(SupportedFileTypes, _uploadedFileExtension))
                {
                    AssignSource();
                }
                else
                {
                    await _parent.DisplayMessagePopupAsync(_parent.GetResourceValueByKey(ResourceConstants.R_SUPPORTED_UPLOAD_FILE_TYPE_KEY).Replace("{0}", SupportedFileTypes), false, true, false).ConfigureAwait(true);
                }
            }
        }

        private void AssignSource()
        {
            if (ControlType == FieldTypes.UploadControl)
            {
                if (_base64String.Length < 100)
                {
                    _imageUpload.DefaultValue = Value;
                }
                else
                {
                    _imageUpload.Source = ImageSource.FromStream(() => GenericMethods.GetMemoryStreamFromBase64(Value));
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_uploadedFileExtension) && !string.IsNullOrWhiteSpace(Value))
                {
                    _uploadedFileExtension = Value.Split(Constants.DOT_SEPARATOR).Last();
                }
                switch (_uploadedFileExtension)
                {
                    case "pdf":
                        _imageUpload.DefaultValue = ImageConstants.I_FILES_PDF_PNG;
                        break;
                    case "doc":
                    case "docx":
                        _imageUpload.DefaultValue = ImageConstants.I_FILES_DOC_PNG;
                        break;
                    case "xls":
                    case "xlsx":
                        _imageUpload.DefaultValue = ImageConstants.I_FILES_XLSX_PNG;
                        break;
                    default:
                        _imageUpload.DefaultValue = Value;
                        break;
                }
                _cellHeader.Text = FileNameWithExtention;
                Grid.SetRowSpan(_cellHeader, 1);
                _cellHeader.Padding = new Thickness(0, 4, 0, 0);
                _cellDescription.Text = string.Format(CultureInfo.CurrentCulture, _parent.GetResourceValueByKey(ResourceConstants.R_IMAGE_UPLOAD_SUBHEADER_TEXT_KEY),
                    GenericMethods.GetDateTimeBasedOnFormatString(App._essentials.ConvertToLocalTime(GenericMethods.GetUtcDateTime), Constants.DEFAULT_DATE_FORMAT));
                _mainGrid.Add(_cellDescription, 1, 1);

            }
        }

        /// <summary>
        /// To delete uploaded files 
        /// </summary>
        protected override void DeleteUploads()
        {
            _base64String = string.Empty;
            FileNameWithExtention = string.Empty;
            _uploadedFileExtension = string.Empty;
            MessageAttachment = null;
            if (ControlType == FieldTypes.UploadControl)
            {
                ImageEmptyView(_controlMargin);
            }
            else
            {
                if (ControlType == FieldTypes.UploadControl)
                {
                    _mainGrid.Children?.Remove(_cellDescription);
                    _imageUpload.DefaultValue = ImageConstants.I_UPLOAD_ICON_PNG;
                    _cellHeader.Padding = new Thickness(0);
                    FileEmptyView();
                }
            }
            SetEmptyViewText();
            OnFileDelete?.Invoke(ErrorCode.OK.ToString(), new EventArgs());
        }

        /// <summary>
        /// Load UI data of view
        /// </summary>
        /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
        /// <returns>Returns true if required view is found, else return false</returns>
        public override async Task LoadUIAsync(bool isRefreshRequest)
        {
            await Task.Run(() => { }).ConfigureAwait(true);
        }

        /// <summary>
        /// Unload UI data of view
        /// </summary>
        public override async Task UnloadUIAsync()
        {
            await Task.Run(() => { }).ConfigureAwait(true);
        }
    }
}
