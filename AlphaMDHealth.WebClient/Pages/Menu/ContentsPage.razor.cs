using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ContentsPage : BasePage
{
    private readonly ContentPageDTO _contentData = new ContentPageDTO();
    private bool _isEducationPage;
    private bool _showPreview;
    private long _pageId;
    private string _educationIDs;
    private string _pageType;
    private string _educationStatus;
    private bool _isDashboardView;

    /// <summary>
    /// Education page parameter
    /// </summary>
    [Parameter]
    public bool IsEducationPage { get; set; }

    /// <summary>
    /// Provider education page parameter
    /// </summary>
    [Parameter]
    public bool IsProviderEducations { get; set; }

    /// <summary>
    /// Page ID of clicked record from Dashboard
    /// </summary>
    [Parameter]
    public long PageID 
    { 
        get { return _pageId; } 
        set
        {
            if (_pageId != value)
            {
                if (_contentData.RecordCount > 0 || _pageId == 0)
                {
                    _contentData.RecordCount = default;
                    ShowDetailPage = true;
                    if (AppState.RouterData.SelectedRoute.Page == AppPermissions.MyEducationsView.ToString() || IsProviderEducations)
                    {
                        _showPreview = true;
                    }
                }
                _pageId = value;
            }
        } 
    }

    protected override async Task OnInitializedAsync()
    {
        _isEducationPage = AppState.RouterData.SelectedRoute.Page == AppPermissions.EducationsView.ToString() || AppState.RouterData.SelectedRoute.Page == AppPermissions.MyEducationsView.ToString() || IsEducationPage;
        await GetDataAsync().ConfigureAwait(true);
    }

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrWhiteSpace(_pageType))
        {
            _pageType = AppState.RouterData.SelectedRoute.Page;
        }
        else
        {
            if (_pageType != AppState.RouterData.SelectedRoute.Page)
            {
                _pageType = AppState.RouterData.SelectedRoute.Page;
                _isEducationPage = (AppState.RouterData.SelectedRoute.Page == AppPermissions.EducationsView.ToString()
                                    || AppState.RouterData.SelectedRoute.Page == AppPermissions.MyEducationsView.ToString())
                                    || IsEducationPage;
                if (PageID > 0)
                {
                    ShowDetailPage = true;
                    _pageId = PageID;
                }
                await GetDataAsync().ConfigureAwait(true);
            }
        }
    }

    private async Task GetDataAsync()
    {
        if (Parameters?.Count > 0)
        {
            _contentData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(UserDTO.RecordCount)));
        }
        _contentData.IsActive = (AppState.RouterData.SelectedRoute.Page == AppPermissions.MyEducationsView.ToString() ? true : false) || IsProviderEducations;
        _contentData.Page = new ContentPageModel { IsEducation = _isEducationPage };
        await SendServiceRequestAsync(new ContentPageService(AppState.webEssentials).GetContentPagesAsync(_contentData), _contentData).ConfigureAwait(true);
        _isDashboardView = _contentData.RecordCount > 0;
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        var list = new List<TableDataStructureModel>{new TableDataStructureModel{DataField=nameof(ContentPageModel.PageID), IsKey = true, IsSearchable=false, IsHidden=true, IsSortable=false}};
        if(AppState.RouterData.SelectedRoute.Page == AppPermissions.StaticContentsView.ToString())
        {
            list.Add(new TableDataStructureModel { DataField = nameof(ContentPageModel.Title), DataHeader = ResourceConstants.R_TITLE_KEY });
        }
        else
        {
            list.Add(new TableDataStructureModel { ImageSrc = nameof(ContentPageModel.ImageName), HasImage = true, ImageFieldType = FieldTypes.SquareWithBackgroundImageControl, ImageHeight = AppImageSize.ImageSizeM, ImageWidth = AppImageSize.ImageSizeM });
            list.Add(new TableDataStructureModel { DataField = nameof(ContentPageModel.Title), DataHeader = ResourceConstants.R_TITLE_KEY });
        }

        if (_isDashboardView)
        {
            list.Add(new TableDataStructureModel { DataField = nameof(ContentPageModel.Status), DataHeader = ResourceConstants.R_STATUS_KEY, IsBadge = true, BadgeFieldType = nameof(ContentPageModel.StatusColor) });
        }
        else
        {
            list.Add(new TableDataStructureModel { DataField = nameof(ContentPageModel.PageType), DataHeader = ResourceConstants.R_PAGE_TYPE_KEY });
            list.Add(new TableDataStructureModel { DataField = nameof(ContentPageModel.Status), DataHeader = ResourceConstants.R_STATUS_KEY, IsBadge = true, BadgeFieldType = nameof(ContentPageModel.StatusColor) });
        }
        return list;
    }

    private async Task OnAddEditClickAsync(ContentPageModel contentPageData)
    {
        Success = Error = string.Empty;
        if(_isDashboardView)
        {
            await NavigateToAsync(GetNavigationRoute(), (contentPageData == null ? 0 : contentPageData.PageID).ToString()).ConfigureAwait(false);
        }
        else
        {
            _pageId = contentPageData == null ? 0 : contentPageData.PageID;
            ShowDetailPage = true;
            if (AppState.RouterData.SelectedRoute.Page == AppPermissions.MyEducationsView.ToString() || IsProviderEducations)
            {
                var education = _contentData.Pages.FirstOrDefault(x => x.PageID == _pageId);
                _educationStatus = education.Status;
                _educationIDs = education.EducationIDs;
                _showPreview = true;
            }
        }
    }

    private async Task OnViewAllClickedAsync(object e)
    {
        await NavigateToAsync(GetNavigationRoute());
    }

    private string GetNavigationRoute()
    {
        return _isEducationPage
            ? (AppState.RouterData.SelectedRoute.Page == AppPermissions.MyEducationsView.ToString() || IsProviderEducations
                ? AppPermissions.MyEducationsView.ToString()
                : AppPermissions.EducationsView.ToString())
            : AppPermissions.StaticContentsView.ToString();
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = false;
        Success = Error = string.Empty;
        if (errorMessage == ErrorCode.OK.ToString() || string.IsNullOrWhiteSpace(errorMessage))
        {
            _isDataFetched = false;
            Success = errorMessage;
            _pageId = 0;
            await NavigateToAsync(GetNavigationRoute()).ConfigureAwait(true);
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = errorMessage;
        }
    }

    private void OnPreviewClosedAsync(string educationStatus)
    {
        if (string.IsNullOrWhiteSpace(educationStatus) && _educationStatus == PatientEducationStatus.Open.ToString())
        {
            educationStatus = PatientEducationStatus.InProgress.ToString();
        }
        if (educationStatus == PatientEducationStatus.InProgress.ToString() || educationStatus == PatientEducationStatus.Completed.ToString())
        {
            _contentData.Pages.FirstOrDefault(x => x.PageID == _pageId).Status = educationStatus;
        }
        _showPreview = false;
        ShowDetailPage = false;
    }

    private string GetTableHeader()
    {
        return _isEducationPage
            ? (AppState.RouterData.SelectedRoute.Page == AppPermissions.MyEducationsView.ToString() || IsProviderEducations
                ? LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.MyEducationsView.ToString())
                : LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.EducationsView.ToString()))
            : LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.StaticContentsView.ToString());
    }

    private bool GetAddEditPermission()
    {
        return _isEducationPage
            ? (AppState.RouterData.SelectedRoute.Page == AppPermissions.MyEducationsView.ToString()
                ? false
                : !_isDashboardView && LibPermissions.HasPermission(_contentData.FeaturePermissions, AppPermissions.EducationAddEdit.ToString()))
            : !_isDashboardView && LibPermissions.HasPermission(_contentData.FeaturePermissions, AppPermissions.StaticContentAddEdit.ToString());
    }
}