using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

internal class TabsViewControlDemo : BasePage
{
    private readonly AmhButtonControl _primaryButton;
    private readonly AmhTabControl _tabControl;
    public TabsViewControlDemo() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.FillAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);
        //AddRowColumnDefination(new GridLength(1, GridUnitType.Auto), 2, true);
        
        _tabControl = new AmhTabControl(FieldTypes.TabControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY,
        };
        AddView(_tabControl);

        //_primaryButton = new AmhButtonControl(FieldTypes.PrimaryButtonControl)
        //{
        //    ControlIcon = ImageConstants.I_USER_ID_PNG,
        //    Key = ResourceConstants.R_LOGIN_ACTION_KEY,
        //};
        //AddView(_primaryButton);
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await new AuthService(App._essentials).GetAccountDataAsync(PageData).ConfigureAwait(true);

        var demoService = new ControlDemoService(App._essentials);
        demoService.GetControlDemoPageResources(PageData);

        _tabControl.Options = demoService.GetTabList();
        _tabControl.Value = 6;

        _tabControl.OnValueChanged += _tabControl_OnValueChanged; ;
        //_primaryButton.PageResources = PageData;
        //_primaryButton.OnValueChanged += OnBackButtonClicked;

        AppHelper.ShowBusyIndicator = false;
    }
    private void _tabControl_OnValueChanged(object sender, EventArgs e)
    {
        var value = _tabControl.Value;      
    }
    private void AddView(View view)
    {
        int index = PageLayout.Children?.Count() ?? 0;
        PageLayout.Add(view, 0, index);
    }
}
