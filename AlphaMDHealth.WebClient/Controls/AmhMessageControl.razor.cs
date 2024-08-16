using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static MudBlazor.Colors;

namespace AlphaMDHealth.WebClient;

public partial class AmhMessageControl:AmhBaseControl
{
    private string _modalStyle = "modal";
    private bool _modalShow;
    private bool IsPopupMessage => FieldType == FieldTypes.TopHeadingWithCloseButtonPopupMessageControl ||
        FieldType == FieldTypes.CloseButtonPopupMessageControl ||
        FieldType == FieldTypes.PopupMessageControl ||
        FieldType == FieldTypes.TopHeadingPopupMessageControl;

    /// <summary>
    /// Display action buttons based on given input
    /// </summary>
    [Parameter]
    public List<ButtonActionModel> Actions
    {
        get; set;
    }

    [Parameter]
    public bool IsMobileView
    {
        get; set;
    }

    [Parameter]
    public string ContentClass { get; set; }

    /// <summary>
    /// Display or close popups
    /// </summary>
    [Parameter]
    public bool ShowHidePopup
    {
        get => _modalShow;
        set
        {
            ShowPopup(value);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        ContentClass = ResourceKey == ResourceConstants.R_DELETE_CONFIRMATION_KEY ? "top-center" : "";
        await base.OnInitializedAsync();
    }

    private void OnCloseActionButtonClicked(OptionModel option)
    {
        ShowPopup(true);
        OnValueChangedAction(option.SequenceNo);
    }

    private bool IsYouTubeLink(string url)
    {
        return url.Contains("youtube.com") && url.Contains("embed");
    }

    private bool IsWebPageLink(string url)
    {
        return url.StartsWith("http") || url.StartsWith("www");
    }

    private bool IsPdfLink(string url)
    {
        return url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    //private bool IsVideoLink(string url)
    //{
    //    return url.Contains("youtube.com") || url.Contains("vimeo.com");
    //}

    private void ShowPopup(bool show)
    {
        if (show)
        {
            _modalStyle = "modal";
            _modalShow = false;
        }
        else
        {
            _modalStyle = "modal show";
            _modalShow = true;
        }
        StateHasChanged();
    }
}