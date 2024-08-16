using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Scheduler;
using Maui.ColorPicker;

namespace AlphaMDHealth.MobileClient;

public class ButtonDemoPage : BasePage
{
    // Regular Buttons
    private readonly AmhButtonControl _primaryButton;
    private readonly AmhButtonControl _secondaryButton;
    private readonly AmhButtonControl _tertiaryButton;
    private readonly AmhButtonControl _deleteButton;

    // Transparent buttons
    private readonly AmhButtonControl _primaryTransparentButton;
    private readonly AmhButtonControl _secondaryTransparentButton;
    private readonly AmhButtonControl _tertiaryTransparentButton;
    private readonly AmhButtonControl _deleteTransparentButton;

    // Border Transparent buttons
    private readonly AmhButtonControl _primaryBorderTransparentButton;
    private readonly AmhButtonControl _secondaryBorderTransparentButton;
    private readonly AmhButtonControl _tertiaryBorderTransparentButton;
    private readonly AmhButtonControl _deleteBorderTransparentButton;

    // Expandable Buttons
    private readonly AmhButtonControl _primaryExButton;
    private readonly AmhButtonControl _secondaryExButton;
    private readonly AmhButtonControl _tertiaryExButton;
    private readonly AmhButtonControl _deleteExButton;

    // Transparent buttons
    private readonly AmhButtonControl _primaryTransparentExButton;
    private readonly AmhButtonControl _secondaryTransparentExButton;
    private readonly AmhButtonControl _tertiaryTransparentExButton;
    private readonly AmhButtonControl _deleteTransparentExButton;

    // Border Transparent buttons
    private readonly AmhButtonControl _primaryBorderTransparentExButton;
    private readonly AmhButtonControl _secondaryBorderTransparentExButton;
    private readonly AmhButtonControl _tertiaryBorderTransparentExButton;
    private readonly AmhButtonControl _deleteBorderTransparentExButton;

    private readonly AmhCardsControl _amhCardsControl;
    List<OptionModel> cardsData;

    public ButtonDemoPage() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.FillAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);
        //AddRowColumnDefinition(new GridLength(1, GridUnitType.Auto), 2, true);

        // Regular Buttons
        _primaryButton = new AmhButtonControl(FieldTypes.PrimaryButtonControl)
        {
           
        };
        AddView(_primaryButton);
        _secondaryButton = new AmhButtonControl(FieldTypes.SecondaryButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_secondaryButton);
        _tertiaryButton = new AmhButtonControl(FieldTypes.TertiaryButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_tertiaryButton);      
        _deleteButton = new AmhButtonControl(FieldTypes.DeleteButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_deleteButton);

        // Transparent buttons
        _primaryTransparentButton = new AmhButtonControl(FieldTypes.PrimaryTransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_primaryTransparentButton);
        _secondaryTransparentButton = new AmhButtonControl(FieldTypes.SecondaryTransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_secondaryTransparentButton);
        _tertiaryTransparentButton = new AmhButtonControl(FieldTypes.TertiaryTransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_tertiaryTransparentButton);
        _deleteTransparentButton = new AmhButtonControl(FieldTypes.DeleteTransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_deleteTransparentButton);

        // Border Transparent buttons
        _primaryBorderTransparentButton = new AmhButtonControl(FieldTypes.PrimaryBorderTransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_primaryBorderTransparentButton);
        _secondaryBorderTransparentButton = new AmhButtonControl(FieldTypes.SecondaryBorderTransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_secondaryBorderTransparentButton);
        _tertiaryBorderTransparentButton = new AmhButtonControl(FieldTypes.TertiaryBorderTransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_tertiaryBorderTransparentButton);
        _deleteBorderTransparentButton = new AmhButtonControl(FieldTypes.DeleteBorderTransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_deleteBorderTransparentButton);

        // Expandable Buttons
        _primaryExButton = new AmhButtonControl(FieldTypes.PrimaryExButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_primaryExButton);
        _secondaryExButton = new AmhButtonControl(FieldTypes.SecondaryExButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_secondaryExButton);
        _tertiaryExButton = new AmhButtonControl(FieldTypes.TertiaryExButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_tertiaryExButton);
        _deleteExButton = new AmhButtonControl(FieldTypes.DeleteExButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_deleteExButton);

        // Transparent buttons
        _primaryTransparentExButton = new AmhButtonControl(FieldTypes.PrimaryTransparentExButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_primaryTransparentExButton);
        _secondaryTransparentExButton = new AmhButtonControl(FieldTypes.SecondaryTransparentExButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_secondaryTransparentExButton);
        _tertiaryTransparentExButton = new AmhButtonControl(FieldTypes.TertiaryTransparentExButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_tertiaryTransparentExButton);
        _deleteTransparentExButton = new AmhButtonControl(FieldTypes.DeleteTransparentExButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_deleteTransparentExButton);

        // Border Transparent buttons
        _primaryBorderTransparentExButton = new AmhButtonControl(FieldTypes.PrimaryBorderTransparentExButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_primaryBorderTransparentExButton);
        _secondaryBorderTransparentExButton = new AmhButtonControl(FieldTypes.SecondaryBorderTransparentExButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_secondaryBorderTransparentExButton);
        _tertiaryBorderTransparentExButton = new AmhButtonControl(FieldTypes.TertiaryBorderTransparentExButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_tertiaryBorderTransparentExButton);
        _deleteBorderTransparentExButton = new AmhButtonControl(FieldTypes.DeleteBorderTransparentExButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_deleteBorderTransparentExButton);

        _amhCardsControl = new AmhCardsControl();
        AddView(_amhCardsControl);
        _primaryButton.Icon = ImageConstants.I_USER_ID_PNG;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await new AuthService(App._essentials).GetAccountDataAsync(PageData).ConfigureAwait(false);

        var demoService = new ControlDemoService(App._essentials);
        demoService.GetControlDemoPageResources(PageData);

        _primaryButton.Value = "primary button";
        _secondaryButton.Value = "Secondary button";
        _tertiaryButton.Value = "tertiary button";
        _deleteButton.Value = "Delete button";

        _primaryTransparentButton.Value = "primary tr button";
        _secondaryTransparentButton.Value = "Secondary tr button";
        _tertiaryTransparentButton.Value = "tertiary tr button";
        _deleteTransparentButton.Value = "Delete tr button";

        _primaryBorderTransparentButton.Value = "primary border tr button";
        _secondaryBorderTransparentButton.Value = "Secondary border tr button";
        _tertiaryBorderTransparentButton.Value = "tertiary border tr button";
        _deleteBorderTransparentButton.Value = "Delete border tr button";

        _primaryExButton.Value = "primary Ex button";
        _secondaryExButton.Value = "Secondary Ex button";
        _tertiaryExButton.Value = "tertiary Ex button";
        _deleteExButton.Value = "Delete Ex button";

        _primaryTransparentExButton.Value = "primary tr Ex button";
        _secondaryTransparentExButton.Value = "Secondary tr Ex button";
        _tertiaryTransparentExButton.Value = "tertiary tr Ex button";
        _deleteTransparentExButton.Value = "Delete tr Ex button";

        _primaryBorderTransparentExButton.Value = "primary border tr Ex button";
        _secondaryBorderTransparentExButton.Value = "Secondary border tr Ex button";
        _tertiaryBorderTransparentExButton.Value = "tertiary border tr Ex button";
        _deleteBorderTransparentExButton.Value = "Delete border tr Ex button";

        _primaryButton.StyleName= StyleConstants.ST_DELETE_BORDER_TRANSPARENT_EX_BUTTON_STYLE;

        _primaryButton.PageResources = PageData;
     

        cardsData = new List<OptionModel>();
        //cardsData = demoService.GenerateLargeDataset(0);
        _amhCardsControl.Options = cardsData;
        _primaryButton.OnValueChanged += OnSignInButtonClicked;
        _secondaryButton.OnValueChanged += OnSignInButtonClicked;
        //_tertiaryButton.OnValueChanged += OnViewChartsDemoClicked;
        //_transparentButton.OnValueChanged += OnViewListViewDemoClicked;
        _deleteButton.OnValueChanged += OnBackButtonClicked;

        AppHelper.ShowBusyIndicator = false;
    }

    protected override void OnDisappearing()
    {
        _primaryButton.OnValueChanged -= OnSignInButtonClicked;
        _secondaryButton.OnValueChanged -= OnSignInButtonClicked;
        //_tertiaryButton.OnValueChanged -= OnViewChartsDemoClicked;
        //_transparentButton.OnValueChanged -= OnViewListViewDemoClicked;
        _deleteButton.OnValueChanged -= OnBackButtonClicked;
        base.OnDisappearing();
    }

    private async void OnSignInButtonClicked(object sender, EventArgs e)
    {
      
        //if (IsFormValid())
        //{
        //    AppHelper.ShowBusyIndicator = true;
        //    await ShellMasterPage.CurrentShell.PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
        //}
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        //await ShellMasterPage.CurrentShell.PushMainPageAsync(new ControlDemoPage()).ConfigureAwait(false);
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
    }

    private void AddView(View view)
    {
        int index = PageLayout.Children?.Count() ?? 0;
        PageLayout.Add(view, 0, index);
    }
}