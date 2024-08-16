using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(DevicesPage))]
public class DevicesPage : BasePage
{
    private readonly DevicesView _devicesView;
    private DevicesView _addDeviceView;
    protected bool _isAdd;

    /// <summary>
    /// Property used for add edit device page
    /// </summary>
    public string IsAdd
    {
        get { return _isAdd.ToString(CultureInfo.InvariantCulture); }
        set
        {
            _isAdd = Convert.ToBoolean(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
        }
    }

    public DevicesPage() : base(PageLayoutType.EndToEndPageLayout, false)
    {
        //IsSplitView = MobileConstants.IsTablet;
        _devicesView = new DevicesView(this, null);
        PageLayout.Add(_devicesView, 0, 0);
    }

    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _devicesView.Parameters = AddParameters(CreateParameter(nameof(Param.isAdd), _isAdd.ToString(CultureInfo.InvariantCulture)));
        if (!Convert.ToBoolean(IsAdd))
        {
            PageLayout.Padding = 0;
        }
        //if (IsSplitView)
        //{
        //    _devicesView.OnListRefresh += OnListRefresh;
        //}
        await _devicesView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        _devicesView.OnListRefresh -= OnListRefresh;
        await _devicesView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }

    /// <summary>
    /// Method to override to handle header item clicks
    /// </summary>
    /// <param name="headerLocation">Header location whether Left, Right (in case of tablet)</param>
    /// <param name="menuType">Menu position whether Left, Right</param>
    /// <param name="menuAction">Action type</param>
    public async override Task OnMenuActionClick(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    {
        if (menuAction == MenuAction.MenuActionAddKey)
        {
            //if (IsSplitView)
            //{
            //    _addDeviceView = await _devicesView.AddButtonClickedAsync().ConfigureAwait(true);
            //    _addDeviceView.OnListRefresh += OnListRefresh;
            //}
            //else
            //{
                await PushPageByTargetAsync(Pages.SupportedDevicesPage.ToString(), GenericMethods.GenerateParamsWithPlaceholder(Param.isAdd), true.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
            //}
        }
    }

    private async void OnListRefresh(object sender, EventArgs e)
    {
        if (_addDeviceView != null)
        {
            _addDeviceView.OnListRefresh -= OnListRefresh;
        }
        _devicesView.SearchField.Value = string.Empty;
        await _devicesView.LoadUIAsync(true).ConfigureAwait(true);
    }
}
