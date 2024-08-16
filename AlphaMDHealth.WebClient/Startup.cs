using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Blazor.Analytics;
using Blazored.LocalStorage;
using Microsoft.Fast.Components.FluentUI;
using MudBlazor.Services;
using Radzen;
using System.Globalization;

//[assembly: System.Runtime.InteropServices.ComVisible(true)]
namespace AlphaMDHealth.WebClient;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddRazorPages();
        services.AddScoped<AppState>();
        services.AddScoped<StorageState>();
        services.AddSingleton<NavRefreshService>();
        services.AddLocalization();
        // LocalStorage and Session Storage
        services.AddBlazoredLocalStorage();
        // Add detection services container and device resolver service.
        services.AddDetection();
        // Add the SyncfusionBlazor service
        //services.AddSyncfusionBlazor();

        services.AddServerSideBlazor()
        .AddHubOptions(o =>
        {
            o.MaximumReceiveMessageSize = Constants.MAX_BUFFER_SIZE;
            o.StreamBufferCapacity = int.MaxValue;
        });
        services.AddGoogleAnalytics("G-FR7CQPDS9Z");
        // used to get browser size ( scoped service)P56KTPSETD"
        services.AddScoped<BrowserSizeService>();
        services.AddFluentUIComponents(options =>
        {
            //options.HostingModel = BlazorHostingModel.Hybrid; //todo:
        });
        //services.AddBlazorBootstrap(); // Add Blazor-bootstrap services https://docs.blazorbootstrap.com/getting-started/blazor-server
        services.AddMudServices(); //MudBlazor https://mudblazor.com/getting-started/installation#manual-install-add-components
        services.AddRadzenComponents();
        services.AddBlazorBootstrap();
        services.AddScoped<Radzen.DialogService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<TooltipService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        string[] supportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures)
                .Select(cul => cul.Name)
                .Where(cul => !string.IsNullOrEmpty(cul))
                .ToArray();
        var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0])
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);
        app.UseRequestLocalization(localizationOptions);
        app.UseHttpsRedirection();

        const string cacheMaxAge = "604800";
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                //// using Microsoft.AspNetCore.Http;
                ctx.Context.Response.Headers.Append(
                     "Cache-Control", $"public, max-age={cacheMaxAge}");
            }
        });

        app.UseStaticFiles();

        app.UseDetection();

        // Syncfusion licensing
        //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NHaF5cWWFCf1JpR2ZGfV5yd0VPalxUTnJZUiweQnxTdEZiWX1ZcHZRQGRaV0xxWQ==");

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}