using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(DevicePage))]
[QueryProperty(nameof(ReadingSourceID), "id")]
[QueryProperty(nameof(DeviceType), "type")]
public class DevicePage : BasePage
{
    private readonly DeviceView _deviceView;
    private string _deviceType;
    private string _readingSourceID;

    /// <summary>
    /// ID of device 
    /// </summary>
    public string ReadingSourceID
    {
        get
        {
            return _readingSourceID;
        }
        set => _readingSourceID = Uri.UnescapeDataString(value);
    }

    /// <summary>
    /// Type of device
    /// </summary>
    public string DeviceType
    {
        get
        {
            return _deviceType;
        }
        set => _deviceType = Uri.UnescapeDataString(value);
    }

    /// <summary>
    /// Device page
    /// </summary>
    public DevicePage() : base(PageLayoutType.MastersContentPageLayout, false)
    {
        _deviceView = new DeviceView(this, null);
        SetPageContent(true);
    }

    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _deviceView.Parameters = AddParameters(CreateParameter(nameof(ReadingSourceID), ReadingSourceID), CreateParameter(nameof(DeviceType), DeviceType));
        await _deviceView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override void OnDisappearing()
    {
        _deviceView.UnLoadUIData();
        base.OnDisappearing();
    }
}