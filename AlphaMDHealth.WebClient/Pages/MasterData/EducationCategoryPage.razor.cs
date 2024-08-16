using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class EducationCategoryPage : BasePage
{
    private readonly EducationCategoryDTO _educationCategoryData = new EducationCategoryDTO { RecordCount = -1, EductaionCatergory = new EductaionCatergoryModel(), CategoryDetails = new List<EducationCategoryDetailModel>() };
    private List<ButtonActionModel> _popupActions;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _isEditable = true;

    /// <summary>
    /// Profession ID parameter
    /// </summary>
    [Parameter]
    public long EducationCategoryID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _educationCategoryData.EductaionCatergory.EducationCategoryID = EducationCategoryID;
        await SendServiceRequestAsync(new EducationCategoryService(AppState.webEssentials).SyncEducationCategoriesFromServerAsync(_educationCategoryData, GenericMethods.GetDefaultDateTime, CancellationToken.None), _educationCategoryData).ConfigureAwait(true);
        if (_educationCategoryData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_educationCategoryData.FeaturePermissions, AppPermissions.EducationCategoryAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_educationCategoryData.ErrCode.ToString());
        }
    }

    private readonly List<TabDataStructureModel> DataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel{DataField = nameof(EducationCategoryDetailModel.PageHeading),ResourceKey = ResourceConstants.R_EDUCATION_CATEGORY_NAME_KEY,},
        new TabDataStructureModel{DataField = nameof(EducationCategoryDetailModel.PageData),ResourceKey = ResourceConstants.R_EDUCATION_CATEGORY_DESCRIPTION_KEY,}
    };

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _popupActions = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE,ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO,ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            _educationCategoryData.IsActive = true;
            EducationCategoryDTO categoryDTO = new EducationCategoryDTO
            {
                EductaionCatergory = new EductaionCatergoryModel { EducationCategoryID = EducationCategoryID, IsActive = _educationCategoryData.IsActive, ImageName = _educationCategoryData.EductaionCatergory.ImageName },
                CategoryDetails = _educationCategoryData.CategoryDetails
            };
            await SaveEducationCategoryAsync(categoryDTO).ConfigureAwait(false);
        }
    }

    private async Task OnDeleteConfirmationPopupClickedAsync(object sequenceNo)
    {
        _hideDeletedConfirmationPopup = true;
        Success = Error = string.Empty;
        if (sequenceNo != null)
        {
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _educationCategoryData.IsActive = _educationCategoryData.EductaionCatergory.IsActive = false;
                if (string.IsNullOrWhiteSpace(_educationCategoryData.EductaionCatergory.ImageName))
                {
                    _educationCategoryData.EductaionCatergory.ImageName = _educationCategoryData.EductaionCatergory.DeletedImageName;
                }
                await SaveEducationCategoryAsync(_educationCategoryData).ConfigureAwait(true);
            }
        }
    }

    private async Task SaveEducationCategoryAsync(EducationCategoryDTO categoryDTO)
    {
        await SendServiceRequestAsync(new EducationCategoryService(AppState.webEssentials).SyncEducationCategoryToServerAsync(categoryDTO, CancellationToken.None), categoryDTO).ConfigureAwait(true);
        if (categoryDTO.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_educationCategoryData.ErrCode.ToString());
        }
        else
        {
            Error = categoryDTO.ErrCode.ToString();
        }
    }
}
