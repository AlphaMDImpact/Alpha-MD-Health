using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class WelcomeScreensPage : BasePage
{
    private readonly AppIntroDTO _appIntroData = new AppIntroDTO();
    private readonly AmhCarouselControl _carousalControl;
    private readonly AmhButtonControl _nextButton;
    private int _lastSequence;

    public WelcomeScreensPage() : base(PageLayoutType.EndToEndPageLayout, false)
    {
        PageService = new AppIntroService(App._essentials);
        AddRowColumnDefinition(GridLength.Star, 1, true);
        AddRowColumnDefinition(GridLength.Auto, 1, true);

        _carousalControl = new AmhCarouselControl(FieldTypes.HalfOverlayCarouselControl);
        PageLayout.Add(_carousalControl, 0, 0);

        _nextButton = new AmhButtonControl(FieldTypes.PrimaryButtonControl)
        {
            ResourceKey = ResourceConstants.R_NEXT_ACTION_KEY,
        };
        PageLayout.Add(_nextButton, 0, 1);
    }

    protected override async void OnAppearing()
    {
        await (PageService as AppIntroService).GetAppIntrosAsync(_appIntroData).ConfigureAwait(true);
        if (_appIntroData.AppIntros?.Count > 0)
        {
            _carousalControl.PageResources = _nextButton.PageResources = PageData = PageService.PageData;
            _lastSequence = _appIntroData.AppIntros.Max(x => x.SequenceNo);
            _carousalControl.Options = (from slide in _appIntroData.AppIntros
                                        select new OptionModel
                                        {
                                            OptionID = slide.IntroSlideID,
                                            SequenceNo = slide.SequenceNo,
                                            ParentOptionText = slide.HeaderText,
                                            OptionText = slide.SubHeaderText,
                                            GroupName = slide.ImageName
                                        }).ToList();
            _carousalControl.OnValueChanged += OnSlideChangedAsync;
            _nextButton.OnValueChanged += OnNextButtonClickedAsync;
            AppHelper.ShowBusyIndicator = false;
        }
        else
        {
            await NavigateWelcomePageOnNextAsync().ConfigureAwait(false);
        }
    }

    protected override void OnDisappearing()
    {
        _carousalControl.OnValueChanged -= OnSlideChangedAsync;
        _nextButton.OnValueChanged -= OnNextButtonClickedAsync;
        base.OnDisappearing();
    }

    private async void OnSlideChangedAsync(object sender, EventArgs e)
    {
        if (_carousalControl.Value != null)
        {
            var option = _carousalControl.Value as OptionModel;
            if (option.SequenceNo == _lastSequence)
            {
                await NavigateWelcomePageOnNextAsync().ConfigureAwait(false);
            }
        }
    }

    private async void OnNextButtonClickedAsync(object sender, EventArgs e)
    {
        var currentOption = _carousalControl.Value as OptionModel;
        var nextOption = _carousalControl.Options?.FirstOrDefault(x => x.SequenceNo > currentOption.SequenceNo);
        if (nextOption != null)
        {
            _carousalControl.Value = nextOption;
        }
        else
        {
            await NavigateWelcomePageOnNextAsync().ConfigureAwait(false);
        }
    }

    private async Task NavigateWelcomePageOnNextAsync()
    {
        AppHelper.ShowBusyIndicator = false;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new InitializationPage(Pages.WelcomeScreensPage)).ConfigureAwait(false);
    }
}