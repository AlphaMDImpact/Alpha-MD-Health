using AlphaMDHealth.IntegrationServiceLayer;
using AlphaMDHealth.ServiceConfiguration;
using AlphaMDHealth.Utility;
using Azure.Identity;
using Microsoft.OpenApi.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();

        //var builder = WebApplication.CreateBuilder(args);

        ////var builtConfig = builder.Configuration;
        ////var vaultLocation = builtConfig.GetSection("Azure:KeyVault").Value;
        ////var tanantId = builtConfig.GetSection("Azure:tanantId").Value;
        ////var applicationID = builtConfig.GetSection("Azure:ApplicationId").Value;
        ////var thumbPrint = builtConfig.GetSection("Azure:Thumbprint").Value;

        ////var clientCertCred = new ClientCertificateCredential(tanantId, applicationID, LibGenericMethods.GetCertificate(thumbPrint));
        ////var secretClient = new SecretClient(new Uri(vaultLocation), clientCertCred);
        ////builder.Configuration.AddAzureKeyVault(secretClient, new PrefixKeyVaultSecretManager("PersonalHealth")); // new KeyVaultSecretManager());

        //Console.WriteLine($"Application Name: {builder.Environment.ApplicationName}");
        //Console.WriteLine($"Environment Name: {builder.Environment.EnvironmentName}");
        //Console.WriteLine($"ContentRoot Path: {builder.Environment.ContentRootPath}");
        //Console.WriteLine($"WebRootPath: {builder.Environment.WebRootPath}");

        //// Add services to the container.
        //builder.Services.AddControllers(options =>
        //{
        //    options.Filters.Add<HmacAuthenticationFilter>();
        //    //}).AddJsonOptions(options => //
        //}).AddNewtonsoftJson(options =>
        //{
        //    options.UseMemberCasing();
        //});

        //builder.Services.AddSignalR();

        //// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        //builder.Services.AddEndpointsApiExplorer();
        //builder.Services.AddSwaggerGen(swagger =>
        //{
        //    swagger.SwaggerDoc("v1", new OpenApiInfo
        //    {
        //        Title = "Alpha MD Health Api",
        //        Version = "v1.0",
        //        Description = ""
        //    });
        //    swagger.OperationFilter<AddRequiredHeaderParameter>();
        //});
        ////// Pascal casing
        ////builder.Services.AddControllersWithViews()
        ////    .AddJsonOptions(options =>
        ////{
        ////    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        ////    options.JsonSerializerOptions.PropertyNamingPolicy = null;
        ////});

        ////MyConfiguration myConfiguration = MyConfiguration.GetInstance;
        //var app = builder.Build();
        ////myConfiguration.SetConfiguration(app.Configuration);
        //app.UseHttpsRedirection();
        //app.UseRouting();
        //app.UseAuthorization();

        //app.MapControllers();
        ////app.MapHub<NotificationHub>(Constants.SE_API_PATH + Constants.SE_SIGNALR_ENDPOINT);

        //app.UseSwagger();
        //app.UseSwaggerUI(c =>
        //{
        //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Alpha MD Health Api");
        //});

        //app.MapGet("/", (IConfiguration config) =>

        //app.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                var builtConfig = configurationBuilder.Build();
                var vaultLocation = builtConfig.GetSection("Azure:KeyVault").Value;
                var tanantId = builtConfig.GetSection("Azure:tanantId").Value;
                var applicationID = builtConfig.GetSection("Azure:ApplicationId").Value;
                var thumbPrint = builtConfig.GetSection("Azure:Thumbprint").Value;
                var credential = new ClientCertificateCredential(tanantId, applicationID, GenericMethods.GetCertificate(thumbPrint));
                configurationBuilder.AddAzureKeyVault(new Uri(vaultLocation), credential, new PrefixKeyVaultSecretManager("AlphaMDDev"));
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}

public class Startup
{
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Startup class for web service
    /// </summary>
    /// <param name="configuration">Instance of configuration</param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        MyConfiguration myConfiguration = MyConfiguration.GetInstance;
        myConfiguration.SetConfiguration(configuration);
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services">Collection of services</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<HmacAuthenticationFilter>();
        }).AddNewtonsoftJson(options =>
        {
            options.UseMemberCasing();
        });
        services.AddSignalR();

        //add swagger documentation to apis
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(swagger =>
        {
            swagger.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Alpha MD Health Api",
                Version = "v1.0",
                Description = ""
            });
            swagger.OperationFilter<AddRequiredHeaderParameter>();
        });
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="env">Web environment</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        //app.UseEndpoints(endpoints =>
        //{
        //    endpoints.MapHub<NotificationHub>(Constants.SE_API_PATH + Constants.SE_SIGNALR_ENDPOINT);
        //});

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Alpha MD Health Api");
        });
    }
}