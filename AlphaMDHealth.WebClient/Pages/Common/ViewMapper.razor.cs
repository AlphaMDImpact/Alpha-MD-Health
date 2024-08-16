using AlphaMDHealth.Model;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ViewMapper
{
    /// <summary>
    /// Feature Code to display
    /// </summary>
    [Parameter]
    public bool IsDashboardPage { get; set; }

    /// <summary>
    /// Feature Code to display
    /// </summary>
    [Parameter]
    public string FeatureCode { get; set; }

    /// <summary>
    /// Feature Text to display
    /// </summary>
    [Parameter]
    public string FeatureText { get; set; }

    /// <summary>
    /// List of Dashboard Configuration parameters
    /// </summary>
    [Parameter]
    public List<SystemFeatureParameterModel> Parameters { get; set; }

    /// <summary>
    /// Data to load in the view
    /// </summary>
    [Parameter]
    public object PageData { get; set; }
}