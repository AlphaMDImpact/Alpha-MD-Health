using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.ComponentModel;

namespace AlphaMDHealth.WebClient;

public partial class PatientMobileMenuPage : BasePage
{

    List<GroupModelMenu> groupModelMenus = null;

    private List<GroupModelMenu> getData()
    {
        groupModelMenus = null;
        if (groupModelMenus == null)
        {
            groupModelMenus = new List<GroupModelMenu>();
        }
        var menudata = GetTabOptions();
        GroupModelMenu Groupmenu = null;
        if (menudata != null && menudata.Count > 0)
        {
            Groupmenu = new GroupModelMenu();
            Groupmenu.GroupID = 1;
            Groupmenu.GroupName = LibResources.GetResourceValueByKey(PageResources, ResourceConstants.R_MENU_HEADER_KEY);
            Groupmenu.Items = menudata;
            groupModelMenus.Add(Groupmenu);
        }
        var settingData = GetRouteOptions();
        if (settingData != null && settingData.Count > 0)
        {
            Groupmenu = new GroupModelMenu();
            Groupmenu.GroupID = 2;
            Groupmenu.GroupName = LibResources.GetResourceValueByKey(PageResources, ResourceConstants.R_SETTING_HEADER_KEY);
            Groupmenu.Items = settingData;
            groupModelMenus.Add(Groupmenu);
        }
        return groupModelMenus;
    }
    private AmhViewCellModel getViewCellModel()
    {
        return new AmhViewCellModel
        {
            ID = nameof(OptionModel.OptionID),
            LeftHeader = nameof(OptionModel.OptionText),
            LeftHeaderFieldType = FieldTypes.PrimarySmallHStartVCenterBoldLabelControl,
            LeftIcon = nameof(OptionModel.Icon),
            RightIcon = nameof(OptionModel.GroupName),
            GroupIDField = nameof(GroupModelMenu.GroupID),
            GroupName = nameof(GroupModelMenu.GroupName),
            ChildItems = nameof(GroupModelMenu.Items),
        };
    }

    private List<GroupModelMenu> GetTabOptions()
    {
        var options = new List<GroupModelMenu>();
        AddOptionIfAvailable(AppPermissions.PatientFilesView.ToString(), options);
        AddOptionIfAvailable(AppPermissions.PatientScanHistoryView.ToString(), options);
        return options;
    }

    private void AddOptionIfAvailable(string featureCode, List<GroupModelMenu> options)
    {
        var tabData = AppState.Tabs?.FirstOrDefault(x => x.FeatureCode == featureCode);
        if(tabData == null)
        {
            var data = AppState.RouterData.Routes.Where(X => X.Page == AppPermissions.ConsentsView.ToString()).FirstOrDefault();
            options.Add(new GroupModelMenu
            {
                ParentOptionID = data.FeatureId,
                OptionID = data.FeatureId,
                OptionText = data.FeatureText,
                ParentOptionText = data.Page,
                Icon = $"{data.Page}.svg",
                GroupName = ImageConstants.PATIENT_MOBILE_FORWARD_SVG
            });
        }
        else
        {
            options.Add(new GroupModelMenu
            {
                ParentOptionID = tabData.FeatureParentID,
                OptionID = tabData.FeatureID,
                OptionText = tabData.FeatureText,
                ParentOptionText = tabData.FeatureCode,
                Icon = $"{tabData.FeatureCode}.svg",
                GroupName = ImageConstants.PATIENT_MOBILE_FORWARD_SVG
            });
        }
    }

    private List<GroupModelMenu> GetRouteOptions()
    {
        var options = new List<GroupModelMenu>();
        AddOptionIfAvailable(AppPermissions.UserConsentsView.ToString(), options);
        return options;
    }

    private async Task OnMoreTabChangeAsync(object value)
    {
        if (value != null && value is GroupModelMenu groupoptionModel)
        {
            value = (long)groupoptionModel.OptionID;
        }
        if (value != null && value is long)
        {
            var selectedFeatureID = (long)value;
            var selectedTab = AppState.RouterData.Routes.Where(X => X.FeatureId == selectedFeatureID).FirstOrDefault();
            await NavigateToAsync(selectedTab.Page);
        }
    }
}

public class GroupModelMenu
{
    public long GroupID { get; set; }

    public string GroupName { get; set; }

    public List<GroupModelMenu> Items { get; set; }
    /// <summary>
    /// Image value to render as icon
    /// </summary>
    public string Icon { get; set; }


    /// <summary>
    /// Flag to Store IsDefault
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Stores Option Id 
    /// </summary>
    public long OptionID { get; set; }

    /// <summary>
    /// Stores Option Text
    /// </summary>
    public string OptionText { get; set; }

    /// <summary>
    /// Stores Sequence Number 
    /// </summary>
    public long SequenceNo { get; set; }

    /// <summary>
    /// stores parent Option Id 
    /// </summary>
    public long ParentOptionID { get; set; }

    /// <summary>
    /// Stores Parent Option Text
    /// </summary>
    public string ParentOptionText { get; set; }

}
