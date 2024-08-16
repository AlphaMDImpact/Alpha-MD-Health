using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class PatientEducationsPage : BasePage
{
    private ContentPageDTO _patientEducationData = new ContentPageDTO();
    private long _selectedEducationID;
    private long _patientEducationID;
    private long _programEducationID;
    private bool _isDashboardView;
    public object _pageData;
    private bool _showPreview;
    private AmhViewCellModel _sourceFields;
    private List<PatientEducationModel> _categorywiseEducations;
    private bool _isGoupedData;
    //private List<IGrouping<string, PatientEducationModel>> _categorywiseEducations;
    private IList<ButtonActionModel> _actionButtons = null;

    /// <summary>
    /// External data to load
    /// </summary>
    [Parameter]
    public object PageData
    {
        get
        {
            return _pageData;
        }
        set
        {
            _pageData = value;
            if (_pageData != null)
            {
                _patientEducationData = _pageData as ContentPageDTO;
            }
        }
    }

    /// <summary>
    /// PageID
    /// </summary>
    [Parameter]
    public long PageID
    {
        get { return _selectedEducationID; }
        set
        {
            if (_selectedEducationID != value)
            {
                _selectedEducationID = value;
                SetDetailPage();
            }
        }
    }

    /// <summary>
    /// PatientEducationID
    /// </summary>
    [Parameter]
    public long PatientEducationID
    {
        get { return _patientEducationID; }
        set
        {
            if (_patientEducationID != value)
            {
                _patientEducationID = value;
                SetDetailPage();
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (PageData == null)
        {
            if (Parameters?.Count > 0)
            {
                _patientEducationData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(ContentPageDTO.RecordCount)));
            }
            MapCommonProperties();
            await GetDataAsync();
        }
        else
        {
            _patientEducationData = PageData as ContentPageDTO;
            MapCommonProperties();
            RenderUIData();
        }
        if (IsPatientMobileView)
        {
            _actionButtons ??= new List<ButtonActionModel>();
            _actionButtons.Add(new ButtonActionModel
            {
                ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY,
                ButtonAction = () => { OnViewAllClickedAsync(null); },
                Icon = ImageConstants.PATIENT_MOBILE_BACK_SVG
            });
        }
    }

    private void MapCommonProperties()
    {
        _patientEducationData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        _patientEducationData.IsActive = false;
    }

    private async Task GetDataAsync()
    {
        await SendServiceRequestAsync(new ContentPageService(AppState.webEssentials).GetContentPagesAsync(_patientEducationData), _patientEducationData).ConfigureAwait(true);
        RenderUIData();
    }

    private void RenderUIData()
    {
        _isDashboardView = _patientEducationData.RecordCount > 0;
        if (GenericMethods.IsListNotEmpty(_patientEducationData.PatientEducations) && AppState.IsPatient)
        {
            _categorywiseEducations = new List<PatientEducationModel>();
            if (_isDashboardView)
            {
                _categorywiseEducations = _patientEducationData.PatientEducations;
            }
            else
            {
                var groupData = _patientEducationData.PatientEducations.GroupBy(x => x.CategoryName);
                foreach (var group in groupData)
                {
                    var items = group.ToList();
                    _patientEducationData.RecordCount = Constants.NUMBER_TWO_VALUE;
                    _categorywiseEducations.Add(new PatientEducationModel { CategoryName = group.Key, CategoryEducations = items.Take((int)_patientEducationData.RecordCount) });
                }
                _isGoupedData = true;
            }      
        }
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {

        var columns = new List<TableDataStructureModel>
        {
             new TableDataStructureModel{DataField=nameof(PatientEducationModel.PageID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
             new TableDataStructureModel { BorderColorDataField = nameof(PatientEducationModel.ProgramColor) },
             new TableDataStructureModel{DataField=nameof(PatientEducationModel.PageHeading),DataHeader=ResourceConstants.R_SELECT_EDUCATION_LABEL_KEY},
             new TableDataStructureModel{DataField=nameof(PatientEducationModel.CategoryName),DataHeader=ResourceConstants.R_EDUCATION_CATEGORY_NAME_KEY},
        };
        if (_patientEducationData.RecordCount < 1)
        {
            columns.Add(new TableDataStructureModel { DataField = nameof(PatientEducationModel.ProgramName), DataHeader = ResourceConstants.R_PROGRAM_TITLE_KEY });
            if (AppState.IsPatient)
            {
                columns.Add(new TableDataStructureModel { DataField = nameof(PatientEducationModel.ToDateString), DataHeader = ResourceConstants.R_END_DATE_KEY, Formatter = _patientEducationData.PatientEducation?.ToDateString, IsBadge = true, BadgeFieldType = nameof(PatientEducationModel.DateStyle) });
            }
            else
            {
                columns.Add(new TableDataStructureModel { DataField = nameof(PatientEducationModel.FromDateString), DataHeader = ResourceConstants.R_START_DATE_KEY, Formatter = _patientEducationData.PatientEducation?.FromDateString, IsBadge = true, BadgeFieldType = nameof(PatientEducationModel.DateStyle) });
                columns.Add(new TableDataStructureModel { DataField = nameof(PatientEducationModel.ToDateString), DataHeader = ResourceConstants.R_END_DATE_KEY, Formatter = _patientEducationData.PatientEducation?.ToDateString, IsBadge = true, BadgeFieldType = nameof(PatientEducationModel.DateStyle) });
                columns.Add(new TableDataStructureModel { DataField = nameof(PatientEducationModel.StatusValue), DataHeader = ResourceConstants.R_STATUS_KEY, IsBadge = true, BadgeFieldType = nameof(PatientEducationModel.ProgramColor) });
            }

        }
        return columns;
    }
    private AmhViewCellModel getViewCellModel()
    {
        _sourceFields = new AmhViewCellModel
        {
            ID = nameof(PatientEducationModel.PageID),
            LeftHeader = nameof(PatientEducationModel.PageHeading),
            BandColor= nameof(PatientEducationModel.ProgramColor),
            LeftImage = nameof(PatientEducationModel.ImageName),
            LeftIcon = nameof(PatientEducationModel.LeftDefaultIcon),
            GroupName = nameof(PatientEducationModel.CategoryName),
            ChildItems = nameof(PatientEducationModel.CategoryEducations)
        };
        return _sourceFields;
    }

    private async Task OnViewAllClickedAsync(PatientEducationModel educationData)
    {
        if(educationData == null) 
        {
            await NavigateToAsync(AppPermissions.PatientEducationsView.ToString()).ConfigureAwait(false);

            if (IsPatientMobileView)
            {
                _patientEducationData.RecordCount = 0;
                MapCommonProperties();
                await GetDataAsync();
            }
        }
        else
        {
            _isGoupedData = false;
            _categorywiseEducations = _patientEducationData.PatientEducations.Where(x => x.CategoryName == educationData.CategoryName).ToList();
        }
        StateHasChanged();
    }

    private async void OnAddEditClick(PatientEducationModel educationData)
    {
        Success = Error = string.Empty;
        if (_isDashboardView)
        {
            await NavigateToAsync(AppPermissions.PatientEducationsView.ToString(), (educationData?.PageID ?? 0).ToString(), (educationData?.PatientEducationID ?? 0).ToString()).ConfigureAwait(false);
        }
        else
        {
            _selectedEducationID = educationData == null ? 0 : educationData.PageID;
            _patientEducationID = educationData == null ? 0 : educationData.PatientEducationID;
            _programEducationID = educationData == null ? 0 : educationData.ProgramEducationID;
            ShowDetailPage = true;
        }
        if (AppState.IsPatient)
        {
            _showPreview = true;
        }
    }

    private void SetDetailPage()
    {
        if (_patientEducationData.RecordCount > 0 && _selectedEducationID != 0 && _patientEducationID != 0)
        {
            _patientEducationData.RecordCount = default;
            ShowDetailPage = true;
        }
        if (AppState.IsPatient)
        {
            ShowDetailPage = true;
            _showPreview = true;
        }
    }

    private void OnPreviewClosedAsync(string educationStatus)
    {
        _showPreview = false;
        ShowDetailPage = false;
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = false;
        Success = Error = string.Empty;
        _selectedEducationID = 0;
        _patientEducationID = 0;
        if (PageData == null) await OnViewAllClickedAsync(null);
        if (errorMessage == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = errorMessage;
            await GetDataAsync();
        }
        else
        {
            Error = errorMessage;
        }
    }
}
