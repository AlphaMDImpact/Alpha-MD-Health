using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class AmhMenuItem
{
    private string _menuClass = "menu-item";
    private string _imageClass = "image-icon";

    [Parameter]
    public string MenuID { get; set; }
    [Parameter]
    public long BranchID { get; set; }

    [Parameter]
    public bool IsTitle { get; set; }

    /// <summary>
    /// Type and Location of Menu Placement
    /// </summary>
    [Parameter]
    public MenuLocation Location { get; set; }

    [Parameter]
    public string MenuTitle { get; set; }

    [Parameter]
    public bool IsSelected { get; set; }
    [Parameter]
    public EventCallback<MenuModel> OnMenuItemSelected { get; set; }
    [Parameter]
    public EventCallback<long> OnDropdownIconSelected { get; set; }

    [Parameter]
    public EventCallback OnBackButtonClicked { get; set; }

    [Parameter]
    public string MenuIcon { get; set; }
    [Parameter]
    public string SubMenuIconName { get; set; }

    [Parameter]
    public bool IsDropdown { get; set; }

    [Parameter]
    public MenuModel MenuItem { get; set; }

    [Parameter]
    public bool IsSubMenu { get; set; }

    [Parameter]
    public bool ShowBack { get; set; }

    [Parameter]
    public string AppendClass { get; set; }

    [Parameter]
    public string BackColor { get; set; }

    protected override void OnParametersSet()
    {
        DecideClass();
        base.OnParametersSet();
    }

    protected override void OnInitialized()
    {
        DecideClass();
        base.OnInitialized();
    }

    private void DecideClass()
    {
        switch (Location)
        {
            case MenuLocation.Header:
                _menuClass = "menu-item-header";
                if (IsSelected)
                {
                    _menuClass += " activate-menu-header";
                }
                break;
            case MenuLocation.Footer:
                _menuClass = IsTitle ? "menu-item-footer-content padding-bottom-sm" : "menu-item-footer padding-bottom-xxs cursor-pointer";
                if (IsSelected)
                {
                    _menuClass += " activate-menu-footer";
                }
                break;
            case MenuLocation.Left:
                _menuClass = string.Concat("menu-item", string.Empty);
                if (IsSelected)
                {
                    _menuClass += " activate";
                    _imageClass = "image-icon-active";
                }
                else
                {
                    _imageClass = "image-icon image-icon-active";
                }
                break;
            default:
                //Default Case
                break;
        }

        StateHasChanged();
    }

    private void OnMenuItemClicked()
    {
        OnMenuItemSelected.InvokeAsync(MenuItem);
        StateHasChanged();
    }

    private string TailTruncateString(string input, int threshold)
    {
        if (input?.Length > threshold)
        {
            return input.Substring(0, threshold) + " ...";
        }
        return input;
    }
}