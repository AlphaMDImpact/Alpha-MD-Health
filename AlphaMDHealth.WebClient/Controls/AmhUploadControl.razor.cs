using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.Fast.Components.FluentUI;
using Org.BouncyCastle.Utilities;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class AmhUploadControl : AmhBaseControl
{
    private AttachmentModel _currentFile;
    private List<ButtonActionModel> _popupActions;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _showPreview;
    private string _value;

    /// <summary>
    /// Control value represents attachment value
    /// </summary>
    [Parameter]
    public string Value
    {
        get
        {
            return _value;
        }
        set
        {
            if (_value != value)
            {
                _value = value;
                SetValueInFiles();
                ValueChanged.InvokeAsync(_value);
            }
        }
    }

    /// <summary>
    /// Bindable property of Value
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    private List<AttachmentModel> _files = new List<AttachmentModel>();
    /// <summary>
    /// List Of AttachmentModel
    /// </summary>
    [Parameter]
    public List<AttachmentModel> Files
    {
        get => _files;
        set
        {
            _files = value;
            SetFiles();
            FilesChanged.InvokeAsync(value);
        }
    }

    /// <summary>
    /// Bindable property of Value
    /// </summary>
    [Parameter]
    public EventCallback<List<AttachmentModel>> FilesChanged { get; set; }

    /// <summary>
    /// resource key of Control
    /// </summary>
    [Parameter]
    public string DescriptionResourceKey { get; set; }

    [Parameter]
    public bool IsPatientMobileView { get; set; } = false;

    /// <summary>
    /// Max File Size in MB
    /// </summary>
    [Parameter]
    public int MaxFileSize { get; set; } = 2;

    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }

    public override bool ValidateControl(bool isButtonClick)
    {
        ErrorMessage = string.Empty;
        if (IsControlEnabled && _resource != null)
        {
            var fileCount = GetActiveFilesCount();
            if (Validator.HasRequiredValidationError(_resource, fileCount > 0))
            {
                ErrorMessage = GetRequiredResourceValue();
            }
            else if (fileCount > 0)
            {
                if (Validator.HasMinLengthValidationError(_resource, fileCount))
                {
                    ErrorMessage = string.Format(CultureInfo.CurrentCulture
                        , GetResourceValue(ResourceConstants.R_MINIMUM_LENGTH_VALIDATION_KEY)
                        , _resource.MinLength);
                }
                else if (Validator.HasRangeValidationError(_resource, fileCount))
                {
                    ErrorMessage = string.Format(CultureInfo.CurrentCulture
                        , LibResources.GetResourceValueByKey(PageResources, ResourceConstants.R_RANGE_LENGTH_VALIDATION_KEY)
                        , _resource.MinLength, _resource.MaxLength);
                }
            }
        }
        SetValidationResult(isButtonClick);
        return IsValid;
    }

    private async Task OnFileUploadedAsync(FluentInputFileEventArgs file)
    {
        ErrorMessage = string.Empty;
        string fileExtension = GenericMethods.GetFileExtensionFromName(file.Name);
        if (GenericMethods.IsExtensionSupported(_resource?.KeyDescription, fileExtension))
        {
            if (MaxFileSize >= file.Size / Math.Pow(2, 20))
            {
                try
                {
                    var fileData = await GenericMethods.CreateFileBase64StringAsync(file.Name, file.Stream);
                    _currentFile = new AttachmentModel();
                    _currentFile.FileName = file.Name;
                    _currentFile.FileExtension = fileExtension;
                    _currentFile.IsActive = true;
                    _currentFile.FileIcon = GetFileIcon(fileExtension);
                    _currentFile.FileValue = fileData;
                    _currentFile.ThumbnailValue = GetIconValue(fileData, fileExtension);
                    if (IsPatientMobileView)
                    {
                        _currentFile.AddedBy = LibResources.GetResourceValueByKey(PageResources, ResourceConstants.R_UPLOAD_NOW_KEY);
                        var statusStyle = Constants.ERROR_COLOR;
                        var value = LibResources.GetResourceValueByKey(PageResources, ResourceConstants.R_REMOVE_TEXT_KEY);
                        _currentFile.Text = $"<label style ='{statusStyle}'><b>{value}</b></label>";
                    }
                    _files.Add(_currentFile);
                    if (IsPatientMobileView)
                    {
                        _files?.Reverse();
                    }
                    GetValueFromFiles();
                    OnValueChangedAction(_currentFile);
                    if (!string.IsNullOrWhiteSpace(DescriptionResourceKey))
                    {
                        _currentFile.IsFirstPopup = true;
                        _showPreview = true;
                    }
                    StateHasChanged();
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
            }
            else
            {
                ErrorMessage = string.Format(CultureInfo.CurrentCulture
                , LibResources.GetResourceValueByKey(PageResources,ResourceConstants.R_MAXIMUM_IMAGE_UPLOAD_SIZE_KEY)
                , MaxFileSize);
            }
        }
        else
        {
            ErrorMessage = string.Format(CultureInfo.CurrentCulture
                , LibResources.GetResourceValueByKey(PageResources, ResourceConstants.R_SUPPORTED_UPLOAD_FILE_TYPE_KEY)
                , _resource?.KeyDescription);
        }
        await file.Stream.DisposeAsync();
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        var docList = new List<TableDataStructureModel>()
        {
            new TableDataStructureModel{DataField=nameof(AttachmentModel.FileID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false},
            new TableDataStructureModel{HasImage=true,ImageFieldType=FieldTypes.SquareImageControl,ImageHeight=AppImageSize.ImageSizeM,ImageWidth=AppImageSize.ImageSizeM,ImageIcon=nameof(AttachmentModel.FileIcon), ImageSrc= nameof(AttachmentModel.ThumbnailValue) , IsSortable= false,ShowRowColumnHeader=false,MaxColumnWidthSize="5vh"}
        };
        if (string.IsNullOrWhiteSpace(DescriptionResourceKey))
        {
            docList.Add(new TableDataStructureModel { DataField = nameof(AttachmentModel.FileName), ShowRowColumnHeader = false, IsHtmlTag = true });
        }
        else
        {
            docList.Add(new TableDataStructureModel { DataField = nameof(AttachmentModel.Text), ShowRowColumnHeader = false, IsHtmlTag = true });
        }

        if (IsControlEnabled)
        {
            docList.Add(new TableDataStructureModel { LinkText = LibResources.GetResourceValueByKey(PageResources, ResourceConstants.R_DELETE_ACTION_KEY), LinkFieldType = FieldTypes.LinkErrorHEndVCenterLabelControl, IsLinkField = nameof(AttachmentModel.IsDeleteAllowed), ShowRowColumnHeader = false, MaxColumnWidthSize = "10vh" });
        }
        return docList;
    }

    private AmhViewCellModel getViewCellModel()
    {
        return new AmhViewCellModel
        {
            ID = nameof(AttachmentModel.FileID),
            LeftHeader = nameof(AttachmentModel.FileDescription),
            LeftHeaderFieldType = FieldTypes.PrimarySmallHStartVCenterBoldLabelControl,
            LeftDescription = nameof(AttachmentModel.AddedBy),
            LeftDescriptionFieldType = FieldTypes.PrimarySmallHStartVCenterLabelControl,
            LeftImage = nameof(AttachmentModel.ThumbnailValue),
            LeftIcon = nameof(AttachmentModel.FileIcon),
            RightImageText = nameof(AttachmentModel.Text),
            RightFieldType = FieldTypes.HtmlLightCenterLabelControl,
        };
    }

    private void OnFileClicked(AttachmentModel file)
    {
        if (_hideDeletedConfirmationPopup)
        {
            _showPreview = true;
            _currentFile = file;
            _currentFile.IsFirstPopup = false;
        }
    }

    private void OnDeleteActionClicked(Tuple<string, AttachmentModel> currentFile)
    {
        _currentFile = currentFile.Item2;
        _popupActions = new List<ButtonActionModel> {
                new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
                new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
            };
        _hideDeletedConfirmationPopup = false;
    }

    private void OnDeleteConfirmationActionClicked(object sequenceNo)
    {
        ErrorMessage = string.Empty;
        _hideDeletedConfirmationPopup = true;
        if (Convert.ToInt64(sequenceNo) == 1)
        {
            if (_currentFile.FileID == Guid.Empty)
            {
                _files.Remove(_currentFile);
            }
            else
            {
                _files.FirstOrDefault(x => x == _currentFile).IsActive = false;
            }
            _currentFile.IsActive = false;
            GetValueFromFiles();
            OnValueChangedAction(_currentFile);
            StateHasChanged();
        }
    }

    private void OnPreviewActionClicked(AttachmentModel attachmentData)
    {
        _showPreview = false;
        if (attachmentData != null)
        {
            attachmentData.Text = attachmentData.AddedBy == null 
                ? attachmentData.FileDescription 
                : string.Format(attachmentData.AddedBy, attachmentData.FileDescription);
            _files.Insert(_files.IndexOf(_currentFile), attachmentData);
            _files.Remove(_currentFile);
            GetValueFromFiles();
            OnValueChangedAction(attachmentData);
        }
        StateHasChanged();
    }

    private int GetActiveFilesCount()
    {
        return _files?.Where(x => x.IsActive == true)?.Count() ?? 0;
    }

    private string GetOnUploadJavaScript()
    {
        string fileInputId = string.Concat(ResourceKey, FieldType, "input-file", UniqueID);
        return $@"
            var fileInput = document.getElementById('{fileInputId}');
            var isControlEnabled = {IsControlEnabled.ToString().ToLower()};
            if (fileInput && isControlEnabled) fileInput.click();
        ";
    }

    private void GetValueFromFiles()
    {
        if (_files?.Count > 0)
        {
            Value = _files.FirstOrDefault(x => x.IsActive)?.FileValue ?? string.Empty;
        }
        else
        {
            Value = string.Empty;
        }
    }

    private void SetValueInFiles()
    {
        if (_files?.Any(x => x.FileValue == _value && x.IsActive) ?? false)
        {
            return;
        }
        Files = !string.IsNullOrWhiteSpace(_value)
            ? new List<AttachmentModel> { new AttachmentModel { FileValue = _value, IsActive = true } }
            : new List<AttachmentModel>();
    }

    private void SetFiles()
    {
        if (_files?.Count > 0)
        {
            foreach (var file in _files)
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
                    string clearnUri = file.FileValue.Split('?')[0];
                    file.FileName = clearnUri?.Split(Constants.BACK_SLASH)?.Last();
                    if (!string.IsNullOrWhiteSpace(file.AddedBy))
                    {
                        file.FileName = string.Format(file.AddedBy, file.FileName);
                        file.Text = string.Format(file.AddedBy, file.FileDescription);
                    }
                }
                file.ThumbnailValue = GetIconValue(file.FileValue, file.FileExtension);
            }
        }
    }

    private string GetFileIcon(string fileExtension)
    {
        return GenericMethods.IsImageFile(fileExtension)
            ? string.Empty
            : string.Concat(GenericMethods.SetIconBasedOnFileType(fileExtension), Constants.SVG_FILE_TYPE);
    }

    private string GetIconValue(string fileValue, string fileExtension)
    {
        return GenericMethods.IsDocumentFile(fileExtension)
            ? string.Empty
            : fileValue ?? string.Empty;
    }
}