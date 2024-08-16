using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class AssignEducationPage : BasePage
{
    private ContentPageDTO _educationData = new ContentPageDTO { PatientEducation = new PatientEducationModel(), RecordCount = -1 };
    private List<OptionModel> _educations = new List<OptionModel>();
    private List<ButtonActionModel> _actionData;
    private bool _showPreview;
    private long _pageID;
    private bool _hideConfirmationPopup = true;
    private bool _isEditable;

    /// <summary>
    /// Patient Education ID
    /// </summary>
    [Parameter]
    public long PatientEducationID { get; set; }

    /// <summary>
    /// Program Education ID
    /// </summary>
    [Parameter]
    public long ProgramEducationID { get; set; }

    /// <summary>
    /// Page ID
    /// </summary>
    [Parameter]
    public long PageID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _educationData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        await GetDataAsync();
    }

    private async Task GetDataAsync()
    {
        _educationData.Page = new ContentPageModel
        {
            PageID = PatientEducationID
        };
        await SendServiceRequestAsync(new ContentPageService(AppState.webEssentials).GetContentPagesAsync(_educationData), _educationData).ConfigureAwait(true);
        if (_educationData.ErrCode == ErrorCode.OK)
        {
            if (PatientEducationID > 0)
            {
                _isEditable = (LibPermissions.HasPermission(_educationData.FeaturePermissions, AppPermissions.PatientEducationAddEdit.ToString())
                    && _educationData.Page.ProgramEducationID < 1 && _educationData.Page.ToDate > DateTime.Now.Date
                    && _educationData.Page.Status != PatientEducationStatus.Completed.ToString());
            }
            else
            {
                _isEditable = LibPermissions.HasPermission(_educationData.FeaturePermissions, AppPermissions.PatientEducationAddEdit.ToString());
            }
            if (PageID > 0)
            {
                long selectedCategory = _educationData.Page.EducationCategoryID;
                _educationData.EducationTypes.ForEach(item =>
                {
                    item.IsSelected = item.OptionID == selectedCategory;
                });
                _educations = _educationData.Educations.Where(x => x.ParentOptionID == selectedCategory || x.OptionID == -1).ToList();
                _pageID = _educations?.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0;
            }
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_educationData.ErrCode.ToString());
        }
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnPreviewClick()
    {
        _showPreview = true;
    }

    private void OnPreviewClosed(string errorMessage)
    {
        _showPreview = false;
        //required for reRegistering the controls
        ClearControls();
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO,  ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task OnAssignButtonClickedAsync()
    {
        if (IsValid())
        {
            _educationData.PatientEducation = new PatientEducationModel
            {
                PatientEducationID = PatientEducationID,
                PageID = _pageID,
                EducationTypeID = Convert.ToInt16(_educationData.EducationTypes.FirstOrDefault(x => x.IsSelected)?.OptionID),
                UserID = _educationData.SelectedUserID,
                Status = PatientEducationStatus.Open,
                FromDate = _educationData?.Page.FromDate.Value.ToUniversalTime(),
                ToDate = _educationData?.Page.ToDate.Value.ToUniversalTime(),
                IsActive = true,
            };
            if (_educationData.PatientEducation.FromDate.Value.Date > _educationData.PatientEducation.ToDate.Value.Date)
            {
                Error = string.Format(CultureInfo.InvariantCulture,
                    LibResources.GetResourceValueByKey(_educationData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                    LibResources.GetResourceValueByKey(_educationData.Resources, ResourceConstants.R_START_DATE_KEY),
                    LibResources.GetResourceValueByKey(_educationData.Resources, ResourceConstants.R_END_DATE_KEY));
                return;
            }
            await SaveEducationDataAsync(_educationData);
        }
    }

    private void OnCategoryChange(object selectedCategoryId)
    {
        if (selectedCategoryId != null && !string.IsNullOrEmpty(selectedCategoryId.ToString()))
        {
            _educations = _educationData.Educations.Where(x => x.ParentOptionID == Convert.ToInt64(selectedCategoryId) || x.OptionID == -1).ToList();
        }
    }

    private void OnEducationChange(object selectedEducationID)
    {
        if (selectedEducationID != null && !string.IsNullOrEmpty(selectedEducationID.ToString()))
        {
            _pageID = Convert.ToInt64(selectedEducationID);
        }
    }

    private async Task SaveEducationDataAsync(ContentPageDTO contentPageData)
    {
        contentPageData.PatientEducation.AddedOn = GenericMethods.GetUtcDateTime;
        contentPageData.PatientEducation.LastModifiedON = GenericMethods.GetUtcDateTime;
        await SendServiceRequestAsync(new ContentPageService(AppState.webEssentials).SyncPatientEducationToServerAsync(contentPageData, new CancellationToken()), contentPageData).ConfigureAwait(true);
        if (contentPageData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(contentPageData.ErrCode.ToString());
        }
        else
        {
            Success = string.Empty;
            Error = contentPageData.ErrCode.ToString();
        }
    }

    private async Task DeletePopUpCallbackAsync(object e)
    {
        _hideConfirmationPopup = true;
        Success = Error = string.Empty;
        if (e != null)
        {
            if (Convert.ToInt64(e) == 1)
            {
                _educationData.PatientEducation.IsActive = false;
                _educationData.PatientEducation = new PatientEducationModel
                {
                    PatientEducationID = PatientEducationID,
                    PageID = _pageID,
                    UserID = _educationData.SelectedUserID,
                    Status = PatientEducationStatus.Open,
                    FromDate = _educationData?.Page.FromDate.Value.ToUniversalTime(),
                    ToDate = _educationData?.Page.ToDate.Value.ToUniversalTime(),
                    IsActive = false
                };
                await SaveEducationDataAsync(_educationData);
            }
        }
    }
}