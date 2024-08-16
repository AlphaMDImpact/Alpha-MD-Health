using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.Fingerprint;

namespace AlphaMDHealth.MobileClient;

[Activity(
    //Label = "Alpha MD Health",
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    ScreenOrientation = ScreenOrientation.Portrait
)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        RequestedOrientation = DeviceInfo.Idiom == DeviceIdiom.Tablet ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
        CrossFingerprint.SetCurrentActivityResolver(() => this);
    }
}