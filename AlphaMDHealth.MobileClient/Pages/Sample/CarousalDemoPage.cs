using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class CarousalDemoPage : BasePage
{
    private readonly AmhCarouselControl _carousalControl;
    private readonly AmhCarouselControl _fullOverlayCarousalControl;
    private readonly AmhCarouselControl _halfOverlayCarousalControl;
    private long lastIndex;

    public CarousalDemoPage() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.FillAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);

        AddRowColumnDefinition(GridLength.Star, 3, true);
        _carousalControl = new AmhCarouselControl(FieldTypes.CarouselControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY
        };
        AddView(_carousalControl);


        _fullOverlayCarousalControl = new AmhCarouselControl(FieldTypes.FullOverlayCarouselControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY
        };
        AddView(_fullOverlayCarousalControl);

        _halfOverlayCarousalControl = new AmhCarouselControl(FieldTypes.HalfOverlayCarouselControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY
        };
        AddView(_halfOverlayCarousalControl);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await new AuthService(App._essentials).GetAccountDataAsync(PageData).ConfigureAwait(true);

        var demoService = new ControlDemoService(App._essentials);
        demoService.GetControlDemoPageResources(PageData);
        List<OptionModel> optionsList = new List<OptionModel> {
            new OptionModel { OptionID = 1, OptionText = "Description", ParentOptionText = "slider 1",Icon=ImageConstants.I_Demo_PNG},
            new OptionModel { OptionID = 2, OptionText = "health is a state of complete physical, mental, and social well-being and not merely the absence of disease or infirmity", ParentOptionText = "Slider 2",Icon="demo1.jpg" },
            new OptionModel { OptionID = 3, OptionText = "good nutrition, regular exercise, avoiding harmful habits, making informed and responsible decisions about health, and seeking medical assistance when necessary", ParentOptionText = "Slider 3",Icon="demo2.jpg"},
        };
        _carousalControl.PageResources = _fullOverlayCarousalControl.PageResources = _halfOverlayCarousalControl.PageResources = PageData;
        _carousalControl.Options = _fullOverlayCarousalControl.Options = _halfOverlayCarousalControl.Options = optionsList;

        _carousalControl.OnValueChanged += OnValueChange;
        _fullOverlayCarousalControl.OnValueChanged += OnValueChange;
        _halfOverlayCarousalControl.OnValueChanged += OnValueChange;
        AppHelper.ShowBusyIndicator = false;
    }

    protected override void OnDisappearing()
    {
        _carousalControl.OnValueChanged -= OnValueChange;
        _fullOverlayCarousalControl.OnValueChanged -= OnValueChange;
        _halfOverlayCarousalControl.OnValueChanged -= OnValueChange;
        base.OnDisappearing();
    }

    private async void OnValueChange(object sender, EventArgs e)
    {
        if (_carousalControl.Value != null)
        {
            var option = _carousalControl.Value as OptionModel;
            if (lastIndex == 3 && option.OptionID == 1)
            {
                // Perform the action
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new ControlDemoPage()).ConfigureAwait(false);
            }
            lastIndex = option.OptionID;
        }
    }

    private void AddView(View view)
    {
        int index = PageLayout.Children?.Count() ?? 0;
        PageLayout.Add(view, 0, index);
    }
}