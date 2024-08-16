using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class UserWelcomeScreensPage : BasePage
{
    private readonly AppIntroDTO _welcomeData = new() { RecordCount = -3, AppIntros = new() };
    private int _preIndex = 0;
    private int _selectedIndex = 0;
    private int _lastIndex;
    string _title;

    protected override async Task OnInitializedAsync()
    {
        base.OnInitialized();
        await SendServiceRequestAsync(new AppIntroService(AppState.webEssentials).GetAppIntrosAsync(_welcomeData), _welcomeData).ConfigureAwait(true);
        _lastIndex = _welcomeData.AppIntros?.IndexOf(_welcomeData.AppIntros.Last())??0;
        _isDataFetched = true;
    }

    private AmhViewCellModel MapSourceField()
    {
        return new AmhViewCellModel
        {
            ID = nameof(AppIntroModel.IntroSlideID),
            LeftFieldType = FieldTypes.ImageControl,
            LeftImage = nameof(AppIntroModel.ImageName),
            LeftHeaderFieldType = FieldTypes.PrimaryLargeHVCenterBoldLabelControl,
            LeftHeader = nameof(AppIntroModel.HeaderText),
            LeftDescriptionFieldType = FieldTypes.HtmlSecondaryCenterLabelControl,
            LeftDescription = nameof(AppIntroModel.SubHeaderText),
        };
    }

    private async void OnSlideChangedAsync(object? indexObj)
    {
        if (_selectedIndex == 0 && _preIndex == _lastIndex - 1)
        {
            await NavigateWelcomePageOnNextAsync().ConfigureAwait(false);
        }
        else
        {
            _preIndex = _selectedIndex - 1;
        }
    }

    private async void OnNextButtonClickedAsync(object sender)
    {
        if (_lastIndex > _selectedIndex)
        {
            _preIndex = _selectedIndex;
            _selectedIndex = _selectedIndex + 1;
        }
        else
        {
            await NavigateWelcomePageOnNextAsync().ConfigureAwait(false);
        }
    }

    private async Task NavigateWelcomePageOnNextAsync()
    {
        AppState.MasterData.HasWelcomeScreens = false;
        AppState.webEssentials.SetPreferenceValue(nameof(MasterDTO.HasWelcomeScreens), 0);
        await NavigateToAsync(AppPermissions.NavigationComponent.ToString(), true).ConfigureAwait(false);
    }
}