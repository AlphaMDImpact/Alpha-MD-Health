using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Mopups.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using SwipeItem = DevExpress.Maui.CollectionView.SwipeItem;

namespace AlphaMDHealth.MobileClient;

internal class AmhUploadControl : AmhBaseControl
{
    private Grid _container;
    private AmhLabelControl _headerLabel;
    private AmhLabelControl _placeHolderLabel;
    private AmhLabelControl _infoLabel;
    private AmhLabelControl _errorLabel;
    private Border _uploadFrame;
    private SwipeItem _endSwipeItem;
    private AmhListViewControl<AttachmentModel> _attachmentList;

    private ObservableCollection<AttachmentModel> _value;
    /// <summary>
    /// Control value as bool
    /// </summary>
    internal ObservableCollection<AttachmentModel> Value
    {
        get
        {
            return GetControlValue();
        }
        set
        {
            if (_value != value)
            {
                _value = value;
                SetControlValue();
            }
        }
    }

    /// <summary>
    /// value property
    /// </summary>
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(ObservableCollection<AttachmentModel>), typeof(AmhUploadControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhUploadControl control = (AmhUploadControl)bindable;
        if (newValue != null)
        {
            control.Value = (ObservableCollection<AttachmentModel>)newValue;
        }
    }

    /// <summary>
    /// provide supported file type
    /// </summary>
    //public string SupportedFileTypes { get; set; }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhUploadControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    public AmhUploadControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    /// <summary>
    /// enable/disable control
    /// </summary>
    /// <param name="value"></param>
    protected override void EnabledDisableField(bool value)
    {
        EnabledDisableControl();
    }

    /// <summary>
    /// get control value
    /// </summary>
    /// <returns></returns>
    private ObservableCollection<AttachmentModel>? GetControlValue()
    {
        return _value;
    }

    /// <summary>
    /// value as List of AttachmentModel
    /// </summary>
    private void SetControlValue()
    {
        if (_value != null)
        {
            AddFilesInList();
        }
    }

    /// <summary>
    /// render control
    /// </summary>
    protected override void RenderControl()
    {
        CreateWrapperControls(out _container, out _headerLabel, out _placeHolderLabel, out _infoLabel, out _errorLabel);
        _placeHolderLabel.VerticalOptions = LayoutOptions.Center;
        _errorLabel.IsVisible = false;
        _container.ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Star }
        };
        var uploadIcon = new AmhImageControl(FieldTypes.SquareImageControl)
        {
            Icon = ImageConstants.I_UPLOAD_ICON_PNG,
            ImageWidth = AppImageSize.ImageSizeL
        };
        _uploadFrame = new Border()
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_UPLOAD_FRAME_STYLE],
            Content = new StackLayout
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_ATTACHMENT_LAYOUT_KEY],
                Children = { uploadIcon, _placeHolderLabel }
            }
        };
        var tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += OnUploadTapAsync;
        _uploadFrame.GestureRecognizers.Add(tapGestureRecognizer);
        _value = new ObservableCollection<AttachmentModel>();
        _attachmentList = new AmhListViewControl<AttachmentModel>(FieldTypes.OneRowListViewControl)
        {
            SourceFields = new AmhViewCellModel
            {
                LeftHeader = nameof(AttachmentModel.FileName),
                LeftIcon = nameof(AttachmentModel.FileIcon),
                LeftImage = nameof(AttachmentModel.FileValue),
            },
            ShowAddButton = false,
            ShowSearchBar = false,
            ShowHeader = false,
            IsVisible = false,
        };
        _attachmentList.OnValueChanged += OnFileItemTappedAsync;
        _endSwipeItem = new SwipeItem()
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_SWIPE_KEY],
        };
        _attachmentList.EndSwapItems = new List<SwipeItem> { _endSwipeItem };
        _endSwipeItem.Tap += OnDeleteItemSwiped;

        _container.Children?.Clear();
        AddView(_container, _headerLabel, 0, 0);
        AddView(_container, _uploadFrame, 0, 1);
        AddView(_container, _infoLabel, 0, 2);
        AddView(_container, _errorLabel, 0, 3);
        AddView(_container, _attachmentList, 0, 4);
        Content = _container;
    }

    /// <summary>
    /// apply resources to control
    /// </summary>
    protected override void ApplyResourceValue()
    {
        ApplyResource(_headerLabel, _placeHolderLabel, _infoLabel);
        if (string.IsNullOrWhiteSpace(_headerLabel.Value as string))
        {
            _headerLabel.IsVisible = false;
        }
        if (string.IsNullOrWhiteSpace(_infoLabel.Value as string))
        {
            _infoLabel.IsVisible = false;
        }
        _attachmentList.PageResources = PageResources;
    }

    /// <summary>
    /// validate control
    /// </summary>
    /// <param name="isButtonClick"></param>
    internal override void ValidateControl(bool isButtonClick)
    {
        _errorLabel.Value = string.Empty;
        if (IsControlEnabled)
        {
            var fileCount = GetActiveFilesCount();
            if (_resource.IsRequired)
            {
                if (fileCount == 0)
                {
                    _errorLabel.Value = GetRequiredResourceValue();
                }
                else if (_fieldType != FieldTypes.UploadControl
                    && _resource.MinLength != 0 && fileCount < _resource.MinLength)
                {
                    _errorLabel.Value = string.Format(CultureInfo.CurrentCulture
                        , LibResources.GetResourceValueByKey(PageResources.Resources, ResourceConstants.R_MINIMUM_LENGTH_VALIDATION_KEY)
                        , _resource.MinLength);

                }
            }
        }
        IsValid = _errorLabel.Value == null || string.IsNullOrWhiteSpace(_errorLabel.Value as string);
        _errorLabel.IsVisible = !IsValid;
        if (!_isButtonClick)
        {
            _isButtonClick = isButtonClick;
        }
    }

    private async void OnFileItemTappedAsync(object sender, EventArgs e)
    {
        if (_attachmentList?.Value is AttachmentModel selectedFile)
        {
            var popupPage = new AmhAttachmentPreviewPage(selectedFile, ShellMasterPage.CurrentShell.CurrentPage);
            if (IsControlEnabled) { popupPage.ShowDeleteButton = true; }
            await MopupService.Instance.PushAsync(popupPage).ConfigureAwait(false);
            popupPage.OnDeleteButtonClicked += OnDeleteOptionClicked;
        }
    }

    private void OnDeleteOptionClicked(object sender, EventArgs e)
    {
        DeleteButtonClickedAsync(_attachmentList?.Value);
    }

    private void OnDeleteItemSwiped(object sender, DevExpress.Maui.CollectionView.SwipeItemTapEventArgs e)
    {
        DeleteButtonClickedAsync(e.Item);
    }

    private async void DeleteButtonClickedAsync(object fileObj)
    {
        if (fileObj is AttachmentModel file)
        {
            var action = await ShellMasterPage.CurrentShell.CurrentPage.DisplayAlert(_resource.ResourceValue, _resource.KeyDescription
                , LibResources.GetResourceValueByKey(PageResources.Resources, ResourceConstants.R_OK_ACTION_KEY)
                , LibResources.GetResourceValueByKey(PageResources?.Resources, ResourceConstants.R_CANCEL_ACTION_KEY));
            if (action)
            {
                if (file.FileID == Guid.Empty)
                {
                    _value.Remove(file);
                }
                else
                {
                    file.IsActive = false;
                }
                var count = GetActiveFilesCount();
                if (count < _resource.MaxLength)
                {
                    _uploadFrame.IsVisible = true;
                }
                SetAttachmentListDataSource(count);
                if (MopupService.Instance.PopupStack.Count > 0)
                {
                    await MopupService.Instance.PopAsync();
                }
                ValidateControl(_isButtonClick);
            }
        }
    }

    private async void OnUploadTapAsync(object sender, EventArgs e)
    {
        _errorLabel.Value = string.Empty;
        _errorLabel.IsVisible = false;
        GenericMethods.GetFilePickerOption(_resource?.KeyDescription, out FilePickerOptions fileOption, out bool hasCamera, out bool hasImageFormats, out bool hasDocumentFormats);
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await FetchFileBasedOnActionAsync(hasCamera, hasImageFormats, hasDocumentFormats, fileOption).ConfigureAwait(true);
        });
    }

    private async Task FetchFileBasedOnActionAsync(bool hasCamera, bool hasImageFormats, bool hasDocumentFormats, FilePickerOptions fileOption)
    {
        switch (fileOption)
        {
            case FilePickerOptions.Camera:
                await UploadFromCameraAsync().ConfigureAwait(true);
                break;
            case FilePickerOptions.Image:
                await UploadFromGallaryAsync().ConfigureAwait(true);
                break;
            case FilePickerOptions.Document:
                await UploadFromDocumentsAsync().ConfigureAwait(true);
                break;
            default:
                var options = new Dictionary<string, FilePickerOptions>();
                if (hasCamera)
                {
                    AddOption(options, ResourceConstants.R_TAKE_PHOTO_FROM_CAMERA_TEXT_KEY, FilePickerOptions.Camera);
                }
                if (hasImageFormats)
                {
                    AddOption(options, ResourceConstants.R_CHOOSE_PHOTO_FROM_GALLERY_TEXT_KEY, FilePickerOptions.Image);
                }
                if (hasDocumentFormats)
                {
                    AddOption(options, ResourceConstants.R_UPLOAD_DOCUMENT_TEXT_KEY, FilePickerOptions.Document);
                }
                var action = await ShellMasterPage.CurrentShell.CurrentPage.DisplayActionSheet(_resource.ResourceValue
                    , LibResources.GetResourceValueByKey(PageResources?.Resources, ResourceConstants.R_CANCEL_ACTION_KEY)
                    , null, options.Keys.ToArray());
                if (action != null)
                {
                    if (options.ContainsKey(action))
                    {
                        await FetchFileBasedOnActionAsync(hasCamera, hasImageFormats, hasDocumentFormats, options[action]).ConfigureAwait(true);
                    }
                }
                break;
        }
    }

    private void AddOption(Dictionary<string, FilePickerOptions> options, string resourceKey, FilePickerOptions option)
    {
        options.Add(LibResources.GetResourceValueByKey(PageResources?.Resources, resourceKey), option);
    }

    private async Task UploadFromCameraAsync()
    {
        if (await CheckPermissionAsync<Permissions.Camera>())
        {
            FileResult file = await MediaPicker.CapturePhotoAsync();
            string supportedFileType = string.Join(Constants.COMMA_SEPARATOR, Constants.JPEG_FILE_TYPE, Constants.JPG_FILE_TYPE, Constants.PNG_FILE_TYPE, Constants.SVG_FILE_TYPE);
            await AddFileAsync(file, supportedFileType).ConfigureAwait(true);
        }
        else
        {
            await DisplayPermissionError(Permission.Camera);
        }
    }

    private async Task UploadFromGallaryAsync()
    {
        if (await CheckPermissionAsync<Permissions.StorageRead>())
        {
            FileResult file = await MediaPicker.PickPhotoAsync();
            await AddFileAsync(file, _resource?.KeyDescription).ConfigureAwait(true);
        }
        else
        {
            await DisplayPermissionError(Permission.Storage);
        }
    }

    private async Task UploadFromDocumentsAsync()
    {
        if (await CheckPermissionAsync<Permissions.StorageRead>())
        {
            FileResult file = await FilePicker.PickAsync();
            await AddFileAsync(file, _resource?.KeyDescription).ConfigureAwait(true);
        }
        else
        {
            await DisplayPermissionError(Permission.Storage);
        }
    }

    private async Task DisplayPermissionError(Permission permission)
    {
        await ShellMasterPage.CurrentShell.CurrentPage.DisplayAlert(_resource.ResourceValue,
           $"{permission}:{LibResources.GetResourceValueByKey(PageResources?.Resources, ErrorCode.Forbidden.ToString())}" //todo: need to use different error message here 
           , LibResources.GetResourceValueByKey(PageResources?.Resources, ResourceConstants.R_OK_ACTION_KEY));
    }

    private async Task<bool> CheckPermissionAsync<T>() where T : Permissions.BasePermission, new()
    {
        var status = await Permissions.CheckStatusAsync<T>();
        if (status != PermissionStatus.Granted)
        {
            var statusResult = await Permissions.RequestAsync<T>();
            if (statusResult != PermissionStatus.Granted)
            {
                return false;
            }
        }
        status = await Permissions.CheckStatusAsync<T>();
        return true;
    }

    private async Task AddFileInListAsync(FileResult file)
    {
        using (Stream stream = await file.OpenReadAsync())
        {
            if (stream != null)
            {
                var uploadedFileExtension = GenericMethods.GetFileExtensionFromName(file.FileName);
                var attechmentFile = new AttachmentModel
                {
                    FileName = file.FileName,
                    FileExtension = uploadedFileExtension,
                    IsActive = true,
                    FileIcon = GetFileIcon(uploadedFileExtension),
                    FileValue = await GenericMethods.CreateFileBase64StringAsync(file.FileName, stream).ConfigureAwait(true),
                };
                _value.Add(attechmentFile);
                var count = GetActiveFilesCount();
                SetAttachmentListDataSource(count);
                if (count >= _resource.MaxLength)
                {
                    _uploadFrame.IsVisible = false;
                }
            }
        }
    }

    private void AddFilesInList()
    {
        foreach (var file in _value)
        {
            if (string.IsNullOrWhiteSpace(file.FileExtension))
            {
                file.FileExtension = GenericMethods.GetFileExtensionFromName(file.FileValue);
            }
            if (string.IsNullOrWhiteSpace(file.FileIcon))
            {
                file.FileIcon = GetFileIcon(file.FileExtension);
            }
            if (string.IsNullOrWhiteSpace(file.FileName) && GenericMethods.IsPathString(file.FileValue))
            {
                file.FileName = file.FileValue?.Split(Constants.BACK_SLASH)?.Last();
            }
            var count = GetActiveFilesCount();
            SetAttachmentListDataSource(count);
            if (count >= _resource.MaxLength)
            {
                _uploadFrame.IsVisible = false;
            }
        }
    }

    private void SetAttachmentListDataSource(int count)
    {

        if (count != 0)
        {
            _attachmentList.DataSource = null;
            _attachmentList.DataSource = _value.Where(x => x.IsActive == true);
            _attachmentList.IsVisible = true;
        }
        else
        {
            _attachmentList.IsVisible = false;
        }
    }

    private int GetActiveFilesCount()
    {
        return _value.Where(x => x.IsActive == true).Count();
    }


    private void EnabledDisableControl()
    {
        if (IsControlEnabled)
        {
            _uploadFrame.Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_UPLOAD_FRAME_STYLE];
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnUploadTapAsync;
            _uploadFrame.GestureRecognizers.Add(tapGestureRecognizer);
            _endSwipeItem.Tap += OnDeleteItemSwiped;
            _attachmentList.EndSwapItems = new List<SwipeItem> { _endSwipeItem };
        }
        else
        {
            _uploadFrame.Style = (Style)Application.Current.Resources[StyleConstants.ST_DISABLED_UPLOAD_FRAME_STYLE];
            _uploadFrame.Opacity = 0.6;
            _uploadFrame.GestureRecognizers.Clear();
            _attachmentList.EndSwapItems.Clear();
            _endSwipeItem.Tap -= OnDeleteItemSwiped;
        }
    }

    private async Task AddFileAsync(FileResult file, string SupportedFileTypes)
    {
        if (file != null)
        {
            if (GenericMethods.IsExtensionSupported(SupportedFileTypes, GenericMethods.GetFileExtensionFromName(file.FileName)))
            {
                await AddFileInListAsync(file).ConfigureAwait(true);
            }
            else
            {
                _errorLabel.Value = string.Format(LibResources.GetResourceValueByKey(PageResources?.Resources, ResourceConstants.R_SUPPORTED_UPLOAD_FILE_TYPE_KEY), SupportedFileTypes);
                _errorLabel.IsVisible = true;
            }
        }
    }

    private string GetFileIcon(string fileExtension)
    {
        return GenericMethods.IsImageFile(fileExtension)
            ? string.Empty
            : string.Concat(GenericMethods.SetIconBasedOnFileType(fileExtension), Constants.PNG_FILE_TYPE);
    }
}