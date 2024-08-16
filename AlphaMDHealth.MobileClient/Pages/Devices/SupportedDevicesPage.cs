namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(SupportedDevicesPage))]
[QueryProperty(nameof(IsAdd), "isAdd")]
public class SupportedDevicesPage : DevicesPage
{
}
