using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class WelcomeScreensPage : BasePage
{
    private readonly AppIntroDTO _appIntroData = new() { RecordCount = 0, AppIntro = new AppIntroModel() };
    private long _appIntroID;

    /// <summary>
    /// App Intro ID parameter
    /// </summary>
    [Parameter]
    public long AppIntroID
    {
        get { return _appIntroID; }
        set
        {
            if (_appIntroID != value)
            {
                if (_appIntroData.RecordCount > 0 || _appIntroID == 0)
                {
                    _appIntroData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _appIntroID = value;
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (Parameters?.Count > 0)
        {
            _appIntroData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(AppIntroDTO.RecordCount)));
        }
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        await SendServiceRequestAsync(new AppIntroService(AppState.webEssentials).GetAppIntrosAsync(_appIntroData), _appIntroData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{ DataField=nameof(AppIntroModel.IntroSlideID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false },
            new TableDataStructureModel{ DataField=nameof(AppIntroModel.HeaderText), DataHeader = ResourceConstants.R_HEADER_TEXT_KEY },
            new TableDataStructureModel{ DataField=nameof(AppIntroModel.SequenceNo), DataHeader = ResourceConstants.R_SEQUENCE_NO_KEY },
        };
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.WelcomeScreensView.ToString()).ConfigureAwait(true);
    }

    private async Task OnAddEditClick(AppIntroModel introData)
    {
        Success = Error = string.Empty;
        if (_appIntroData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.WelcomeScreensView.ToString(), (introData == null ? 0 : introData.IntroSlideID).ToString()).ConfigureAwait(false);
        }
        else
        {
            _appIntroID = introData == null ? 0 : introData.IntroSlideID;
            ShowDetailPage = true;
        }
    }

    private async Task OnAddEditClosedAsync(string message)
    {
        Success = Error = string.Empty;
        ShowDetailPage = false;
        _appIntroID = 0;
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
}