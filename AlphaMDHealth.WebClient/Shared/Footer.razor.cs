using AlphaMDHealth.Model;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class Footer
{
    /// <summary>
    /// menu click event
    /// </summary>
    [Parameter]
    public EventCallback<MenuModel> OnMenuClicked { get; set; }

    private async Task OnFeatureMenuClickAsync(MenuModel menuItem)
    {
        await OnMenuClicked.InvokeAsync(menuItem);
    }
}