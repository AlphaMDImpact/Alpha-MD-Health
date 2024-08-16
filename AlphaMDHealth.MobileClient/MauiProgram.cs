using banditoth.MAUI.JailbreakDetector;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Controls.UserDialogs.Maui;
using DevExpress.Maui;
using Mopups.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace AlphaMDHealth.MobileClient;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        _ = builder.UseMauiApp<App>()
        .UseMauiCommunityToolkit()
        .UseSkiaSharp()
        .UseDevExpress()
        .UseMauiCommunityToolkitMarkup()
        .UseMauiCommunityToolkitMediaElement()
        //.ConfigureSyncfusionCore()
        .ConfigureMopups()
        .UseUserDialogs()
        .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        })
        .ConfigureJailbreakProtection(configuration =>
        {
            configuration.MaximumPossibilityPercentage = 20;
            configuration.MaximumWarningCount = 1;
            configuration.CanThrowException = true;
        })
        .ConfigureEffects(effects =>
        {
            //effects.Add<StatusBarEffect, PlatformStatusBarEffect>();
        });
        //#if DEBUG
        //        builder.Logging.AddDebug();
        //#endif
        SQLitePCL.Batteries_V2.Init();
        //builder.Services.AddScoped(sp => new HttpClientHandler { });
        //builder.Services.AddScoped(sp => new HttpClient());
        return builder.Build();
	}
}

