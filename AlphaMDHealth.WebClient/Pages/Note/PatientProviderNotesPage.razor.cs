using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class PatientProviderNotesPage : BasePage
{
    private PatientProviderNoteDTO _noteData = new();
    private readonly List<ButtonActionModel> _actionButtons = new List<ButtonActionModel>();
    private Guid _providerNoteID;
    private bool _isDashboardView;
    private object _pageData;

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
                _noteData = _pageData as PatientProviderNoteDTO;
            }
        }
    }

    /// <summary>
    /// Provider Note ID
    /// </summary>
    [Parameter]
    public Guid ProviderNoteID
    {
        get { return _providerNoteID; }
        set
        {
            if (_providerNoteID != value)
            {
                if (_noteData.RecordCount > 0 || _providerNoteID == Guid.Empty)
                {
                    _noteData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _providerNoteID = value;
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (PageData == null)
        {
            if (Parameters?.Count > 0)
            {
                _noteData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(ContentPageDTO.RecordCount)));
            }
            await GetDataAsync().ConfigureAwait(false);
        }
        else
        {
            _noteData = PageData as PatientProviderNoteDTO;
            _isDataFetched = true;
        }
    }

    private async Task GetDataAsync()
    {
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).GetProviderNotesAsync(_noteData), _noteData).ConfigureAwait(true);
        _isDashboardView = _noteData.RecordCount > 0;
        _isDataFetched = true;
    }

    private void OnAddEditClickAsync(OptionModel note)
    {
        Success = Error = string.Empty;
        if (_noteData.RecordCount > 0)
        {
            NavigateToAsync(AppPermissions.PatientProviderNotesView.ToString(), (note == null ? string.Empty : note.ParentOptionText).ToString()).ConfigureAwait(false);
        }
        else
        {
            _providerNoteID = (note == null ? Guid.Empty : Guid.Parse(note.ParentOptionText));
            ShowDetailPage = true;
        }
    }

    private async Task OnCloseEventCallbackAsync(string message)
    {
        ShowDetailPage = false;
        _providerNoteID = Guid.Empty;
        Success = Error = string.Empty;
        if (PageData == null)
        {
            await OnViewAllClickedAsync().ConfigureAwait(false);
        }
        if (message == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = message;
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = message;
        }
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.PatientProviderNotesView.ToString()).ConfigureAwait(true);
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        List<TableDataStructureModel> columns = new()
        {
            new TableDataStructureModel { DataField = nameof(OptionModel.ParentOptionText), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false },
            new TableDataStructureModel { DataField = nameof(OptionModel.OptionText), IsHtmlTag = true, ShowRowColumnHeader=false},
        };
        return columns;
    }
}