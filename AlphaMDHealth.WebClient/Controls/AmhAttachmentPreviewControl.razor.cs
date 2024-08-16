using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using AlphaMDHealth.WebClient.Controls;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class AmhAttachmentPreviewControl : AmhBaseControl
{
    private string _fileIcon;
    private string _modalStyle;
    private bool _showModal;
    private string _description;
    private AmhMultilineEntryControl _amhChartControl = new AmhMultilineEntryControl();
    [Parameter]
    public bool ShowPopup
    {
        get => _showModal;
        set
        {
            SetShowPopup(value);
        }
    }

    /// <summary>
    /// Attachment data source
    /// </summary>
    [Parameter]
    public AttachmentModel DataSource { get; set; }

    /// <summary>
    ///  call Back event on Send Button Click
    /// </summary>
    [Parameter]
    public EventCallback<AttachmentModel> OnActionClick { get; set; }

    /// <summary>
    /// Initialize method
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (string.IsNullOrWhiteSpace(DataSource?.FileIcon))
        {
            if (!string.IsNullOrWhiteSpace(DataSource?.FileExtension))
            {
                _fileIcon = String.Concat(GenericMethods.SetIconBasedOnFileType(DataSource.FileExtension),Constants.SVG_FILE_TYPE);
            }
        }
        _description = DataSource.FileDescription;
    }

    private void OnClick(AttachmentModel attachmentData)
    {
        if (_amhChartControl.ValidateControl(true))
        {
            if (attachmentData != null)
            {
                attachmentData.FileDescription = _description;
                OnActionClick.InvokeAsync(attachmentData);
            }
        }
    }

    private void OnCloseActionButtonClicked()
    {
        if (!string.IsNullOrWhiteSpace(ResourceKey) && string.IsNullOrWhiteSpace(DataSource.FileDescription) && DataSource.IsFirstPopup)
        {
            DataSource.IsActive = false;
            OnActionClick.InvokeAsync(DataSource);
        }
        OnActionClick.InvokeAsync(null);
    }

    private void SetShowPopup(bool show)
    {
        if (show)
        {
            _showModal = true;
            _modalStyle = "modal show";
        }
        else
        {
            _showModal = false;
        }
        StateHasChanged();
    }
}