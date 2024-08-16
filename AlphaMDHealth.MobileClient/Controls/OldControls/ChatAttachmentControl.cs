using System.Runtime.CompilerServices;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Device = Microsoft.Maui.Controls.Device;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represemnts Chat Attachment Control
    /// </summary>
    public class ChatAttachmentControl : BaseContentView
    {
        private readonly Grid _mainLayout;
        private readonly CustomLabelControl _messageLabel;
        private readonly CustomLabelControl _dateLabel;
        private readonly CustomButtonControl _sendbutton;
        private readonly CustomMultiLineEntryControl _captionEntry;
        private readonly CustomImageControl _attachmentImage;
        private readonly Frame _imageHolder;
        private readonly double _padding = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];

        /// <summary>
        /// Value Property 
        /// </summary>
        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create(nameof(Value), typeof(CustomAttachmentModel), typeof(ChatAttachmentControl), defaultBindingMode: BindingMode.OneWayToSource, propertyChanged: ChatAttachmentControl.ValueChangeProperty);

        /// <summary>
        /// Is Preview property
        /// </summary>
        public bool IsPreview { get; set; }

        /// <summary>
        /// Value property
        /// </summary>
        public CustomAttachmentModel Value
        {
            get
            {
                CustomAttachmentModel data = (CustomAttachmentModel)GetValue(ValueProperty);
                data.Text = _captionEntry?.Value;
                return data;
            }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// OnSendClick event handler
        /// </summary>
        public event EventHandler<EventArgs> OnSendClick;

        /// <summary>
        /// UpdateImagesInCell
        /// </summary>
        /// <param name="itemToBeUpdated"></param>
        public void UpdateImagesInCell(CustomAttachmentModel itemToBeUpdated)
        {
            AssignData(itemToBeUpdated);
        }


        /// <summary>
        /// Chat Attachment Control class renderer
        /// </summary>
        public ChatAttachmentControl()
        {
            _attachmentImage = new CustomImageControl(Constants.STRING_SPACE, Constants.STRING_SPACE);
            AutomationProperties.SetIsInAccessibleTree(_attachmentImage, true);
            _messageLabel = new CustomLabelControl(LabelType.PrimarySmallLeft) { LineBreakMode = LineBreakMode.WordWrap, WidthRequest = 200 };
            AutomationProperties.SetIsInAccessibleTree(_messageLabel, true);
            _dateLabel = new CustomLabelControl(LabelType.SecondryExtraSmallRight) { Margin = new Thickness(0, _padding / 2, 0, 0), FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(CustomLabel)), };
            AutomationProperties.SetIsInAccessibleTree(_dateLabel, true);
            _sendbutton = new CustomButtonControl(ButtonType.PrimaryWithoutMargin)
            {
                VerticalOptions = LayoutOptions.End,

            };
            AutomationProperties.SetIsInAccessibleTree(_sendbutton, true);

            _captionEntry = new CustomMultiLineEntryControl
            {
                EditorHeightRequest = EditorHeight.Chat,
                Margin = new Thickness(0, _padding, 0, 0)
            };
            AutomationProperties.SetIsInAccessibleTree(_captionEntry, true);

            _mainLayout = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)}
                },
                ColumnDefinitions =
                {
                   new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                },
            };
            _imageHolder = new Frame
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_FRAME_STYLE],
                BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR],
                VerticalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = Constants.ATTACHMENT_MAX_HEIGHT,
                HorizontalOptions = new OnIdiom<LayoutOptions> { Phone = LayoutOptions.FillAndExpand, Tablet = LayoutOptions.CenterAndExpand },
                Content = _attachmentImage,
            };
            AutomationProperties.SetIsInAccessibleTree(_imageHolder, true);

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
                _mainLayout.Children?.Clear();
                if (IsPreview)
                {
                    _mainLayout.Margin = 0;
                    _imageHolder.HeightRequest = Constants.ATTACHMENT_MIN_HEIGHT;
                    _imageHolder.WidthRequest = Constants.ATTACHMENT_MIN_WIDTH;
                    _mainLayout.Add(_messageLabel, 0, 0);
                    _mainLayout.Add(_imageHolder, 0, 1);
                    _mainLayout.Add(_dateLabel, 0, 2);
                    Content = _mainLayout;
                }
                else
                {
                    _mainLayout.Margin = new Thickness((double)Application.Current.Resources[StyleConstants.ST_APP_PADDING]);
                    _imageHolder.WidthRequest = new OnIdiom<double> { Phone = -1, Tablet = Constants.ATTACHMENT_MAX_WIDTH };
                    _imageHolder.HeightRequest = Constants.ATTACHMENT_MAX_HEIGHT;
                    _mainLayout.Add(_imageHolder, 0, 0);
                    _mainLayout.Add(_captionEntry, 0, 1);
                    _mainLayout.Add(_sendbutton, 0, 4);
                    _sendbutton.Clicked += Sendbutton_Clicked;

                    TapGestureRecognizer imageTapped = new TapGestureRecognizer();
                    imageTapped.Tapped += ImageTapped_Tapped;
                    _attachmentImage.GestureRecognizers.Add(imageTapped);
                    Content = new ScrollView { Content = _mainLayout };
                }
            }
        }

        private void ImageTapped_Tapped(object sender, EventArgs e)
        {
            if (sender is CustomImageControl && !IsPreview && !string.IsNullOrWhiteSpace(Value.AttachmentBase64))
            {
                string filename;
                if (Uri.IsWellFormedUriString(Value.FileName, UriKind.Absolute))
                {
                    Uri uri = new Uri(Value.FileName);
                    filename = System.IO.Path.GetFileName(uri.Segments.Last());
                }
                else
                {
                    filename = Value.FileName;
                }
                ShowAttachment(Value.AttachmentBase64, filename);
            }
        }

        /// <summary>
        /// Converts base64 string to file and displays it
        /// </summary>
        /// <param name="base64String">Base64 string of a file</param>
        /// <param name="fileName">Name of the file</param>
        public void ShowAttachment(string base64String, string fileName)
        {
            string fileBase64;
            if (base64String.Contains(Constants.SYMBOL_COMMA_SEPERATOR_STRING))
            {
                fileBase64 = base64String.Split(Constants.SYMBOL_COMMA)[1];
            }
            else if (base64String.Contains(Constants.SYMBOL_SEMI_COLAN_STRING))
            {
                fileBase64 = base64String.Split(Constants.SYMBOL_SEMI_COLAN)[1];
            }
            else if (base64String.Contains(Constants.SYMBOL_COLAN_STRING))
            {
                fileBase64 = base64String.Split(Constants.SYMBOL_COLAN)[1];
            }
            else
            {
                fileBase64 = base64String;
            }
            App._essentials.SetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, true);
            //todo
            //DependencyService.Get<ClientBusinessLayer.IDataViewer>().ShowAttachment(new AttachmentModel { OriginalFile = Convert.FromBase64String(fileBase64), FileName = fileName.Replace(LibConstants.STRING_SPACE, LibConstants.SYMBOL_UNDERSCORE_STRING) });
        }

        private void Sendbutton_Clicked(object sender, EventArgs e)
        {
            Value.Text = _captionEntry?.Value;
            ValidateControl(sender, e);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ValidateControl(object sender, EventArgs e)
        {
            var isValid = false;
            if (RunValidation())
            {
                isValid = true;
                OnSendClick?.Invoke(sender, e);
            }
            return isValid;
        }

        /// <summary>
        /// ValidateControl
        /// </summary>
        /// <returns></returns>
        public bool RunValidation()
        {
            _captionEntry.ValidateControl(true);
            return _captionEntry.IsValid;
        }

        /// <summary>
        /// Invoked when binding context is changed
        /// </summary>
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (BindingContext is CustomAttachmentModel item)
            {
                //todo
                //  _attachmentImage.Source = item.AttachmentSource;
                _attachmentImage.DefaultValue = item.DefaultIcon;
                _messageLabel.Text = item.Text;
                _messageLabel.IsVisible = !string.IsNullOrWhiteSpace(item.Text?.Trim());
                _dateLabel.Text = item.AddedOnDate;
                if (!string.IsNullOrWhiteSpace(item.TextColor) && !string.IsNullOrWhiteSpace(item.DateColor))
                {
                    _messageLabel.TextColor = Color.FromArgb(item.TextColor);
                    _dateLabel.TextColor = Color.FromArgb(item.DateColor);
                }
                _messageLabel.Margin = string.IsNullOrWhiteSpace(_messageLabel.Text) ? new Thickness(0) : new Thickness(0, 0, 0, _padding / 2);
            }
        }

        private static void ValueChangeProperty(BindableObject bindable, object oldValue, object newValue)
        {
            ChatAttachmentControl control = (ChatAttachmentControl)bindable;
            if (control != null && newValue != null)
            {
                control.AssignData((CustomAttachmentModel)newValue);
            }
        }

        private void AssignData(CustomAttachmentModel value)
        {
            if (IsPreview)
            {
                //todo
                //  _attachmentImage.SetBinding(CustomImageControl.SourceProperty, nameof(CustomAttachmentModel.AttachmentSource));
                _attachmentImage.SetBinding(CustomImageControl.DefaultValueProperty, nameof(CustomAttachmentModel.DefaultIcon));
                if (string.IsNullOrWhiteSpace(value.Text?.Trim()))
                {
                    _messageLabel.IsVisible = false;
                }
                else
                {
                    _messageLabel.Text = value.Text;
                    _messageLabel.IsVisible = true;
                }
                _dateLabel.Text = value.AddedOnDate;
                _messageLabel.HeightRequest = -1;
            }
            else
            {
                _captionEntry.Value = value.Text;
                GetFileIcon(value.FileType, value.AttachmentBase64);
                _sendbutton.IsEnabled = !value.IsDisabledSaveButton;
            }
            _messageLabel.SetBinding(CustomLabelControl.TextColorProperty, nameof(CustomAttachmentModel.TextColor));
            _dateLabel.SetBinding(CustomLabelControl.TextColorProperty, nameof(CustomAttachmentModel.DateColor));
        }

        private void GetFileIcon(AppFileExtensions fileType, string base64)
        {
            switch (fileType)
            {
                case AppFileExtensions.pdf:
                    _attachmentImage.DefaultValue = ImageConstants.I_FILES_PDF_PNG;
                    break;
                case AppFileExtensions.doc:
                case AppFileExtensions.docx:
                    _attachmentImage.DefaultValue = ImageConstants.I_FILES_DOC_PNG;
                    break;
                case AppFileExtensions.xls:
                case AppFileExtensions.xlsx:
                    _attachmentImage.DefaultValue = ImageConstants.I_FILES_XLSX_PNG;
                    break;
                case AppFileExtensions.jpeg:
                case AppFileExtensions.jpg:
                case AppFileExtensions.png:
                    if (!string.IsNullOrWhiteSpace(base64) && base64.Length > 100)
                    {
                        _attachmentImage.Source = ImageSource.FromStream(() => GenericMethods.GetMemoryStreamFromBase64(base64));
                    }
                    break;
                default:
                    _attachmentImage.DefaultValue = ImageConstants.I_UPLOAD_ICON_PNG;
                    break;
            }
        }

        /// <summary>
        /// validate Entry Control 
        /// </summary>
        /// <param name="isButtonClick">validation fired after button is clicked</param>
        public override void ValidateControl(bool isButtonClick)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// method to Apply Resource Value
        /// </summary>
        protected override void ApplyResourceValue()
        {
            _captionEntry.ControlResourceKey = ControlResourceKey;
            _captionEntry.PageResources = PageResources;
            _sendbutton.Text = GetResourceValueByKey(ResourceConstants.R_SEND_ACTION_KEY);

        }

        /// <summary>
        /// Method to Enable value
        /// </summary>
        /// <param name="value"></param>
        protected override void EnabledValue(bool value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Method to set rendere value
        /// </summary>
        /// <param name="value">border thikness</param>
        protected override void RenderBorder(bool value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Method to render header
        /// </summary>
        protected override void RenderHeader()
        {
            throw new NotSupportedException();
        }
    }
}