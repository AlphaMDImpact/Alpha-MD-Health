using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using AlphaMDHealth.WebClient.Controls;
using Microsoft.AspNetCore.Components;
using Microsoft.Fast.Components.FluentUI;

namespace AlphaMDHealth.WebClient;

public partial class ContentPreviewPage : BasePage
{
    private readonly ContentPageDTO _contentPage = new ContentPageDTO();
    private string _pageTitle;
    private MarkupString _content;

    /// <summary>
    /// PageId parameter
    /// </summary>
    [Parameter]
    public long PageId { get; set; }

    /// <summary>
    /// Id of the user
    /// </summary>
    [Parameter]
    public long UserId { get; set; }

    /// <summary>
    /// Id of the education
    /// </summary>
    [Parameter]
    public long PatientEducationID { get; set; }

    /// <summary>
    /// Id of the education
    /// </summary>
    [Parameter]
    public string PatientEducationIDs { get; set; }

    /// <summary>
    /// isEducation parameter
    /// </summary>
    [Parameter]
    public bool IsEducationPreview { get; set; }

    /// <summary>
    /// Education status parameter
    /// </summary>
    [Parameter]
    public string EducationStatus { get; set; }

    private IList<ButtonActionModel> _actionButtons = null;

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    protected override async Task OnParametersSetAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
        await base.OnParametersSetAsync();
    }

    private async Task GetDataAsync()
    {
        _contentPage.Page = new ContentPageModel
        {
            PageID = PageId,
        };
        _contentPage.RecordCount = PatientEducationID > 0 || !string.IsNullOrWhiteSpace(PatientEducationIDs) ? -2 : -1;
        _contentPage.SelectedUserID = UserId;
        await SendServiceRequestAsync(new ContentPageService(AppState.webEssentials).GetContentPagesAsync(_contentPage), _contentPage).ConfigureAwait(true);
        if (_contentPage.ErrCode == ErrorCode.OK)
        {
            byte languageID = (byte)AppState.webEssentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 1);
            _pageTitle = _contentPage.PageDetails.FirstOrDefault(x => x.LanguageID == languageID).PageHeading;
            _content = (MarkupString)_contentPage.PageDetails.FirstOrDefault(x => x.LanguageID == languageID).PageData;
            _isDataFetched = true;
            if (AppState.RouterData.SelectedRoute.Page == AppPermissions.MyEducationsView.ToString()
                && EducationStatus == PatientEducationStatus.Open.ToString())
            {
                await UpdateEducationTask(PatientEducationStatus.InProgress);
            }
            if (IsPatientMobileView)
            {
                _actionButtons ??= new List<ButtonActionModel>();
                _actionButtons.Add(new ButtonActionModel
                {
                    ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY,
                    ButtonAction = () => { OnOkayClicked(); },
                    Icon = ImageConstants.PATIENT_MOBILE_BACK_SVG
                });
            }

        }
        else
        {
            await OnClose.InvokeAsync(_contentPage.ErrCode.ToString());
        }
    }

    private async Task UpdateEducationTask(PatientEducationStatus status)
    {
        if (EducationStatus != PatientEducationStatus.Completed.ToString())
        {
            if (PatientEducationID > 0
                && (string.IsNullOrWhiteSpace(PatientEducationIDs) || PatientEducationIDs == Constants.NUMBER_ZERO))
            {
                PatientEducationIDs = PatientEducationID.ToString();
            }
            if (!string.IsNullOrWhiteSpace(PatientEducationIDs))
            {
                ContentPageDTO contentPage = new ContentPageDTO
                {
                    PatientEducations = PatientEducationIDs.Split(Constants.COMMA_SEPARATOR).Select(x => new PatientEducationModel
                    {
                        PatientEducationID = Convert.ToInt64(x),
                        Status = status
                    }).ToList()
                };
                await SendServiceRequestAsync(new ContentPageService(AppState.webEssentials).SyncEducationStatusToServerAsync(contentPage, CancellationToken.None), contentPage);
                EducationStatus = status.ToString();
            }
        }
    }

    private async Task OnReadActionClicked()
    {
        await UpdateEducationTask(PatientEducationStatus.Completed);
        await OnClose.InvokeAsync(EducationStatus);
    }

    private async Task OnOkayClicked()
    {
        await OnClose.InvokeAsync(EducationStatus);
    }
}
