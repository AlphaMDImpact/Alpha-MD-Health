using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Default view used to display default content
/// </summary>
public class DefaultContentView : ViewManager
{
    readonly MessageViewType _viewType;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public DefaultContentView(BasePage page, object parameters) : base(page, parameters)
    {
        string viewype = GenericMethods.MapValueType<string>(GetParameterValue(nameof(MessageViewType)));
        _viewType = string.IsNullOrEmpty(viewype) ? MessageViewType.Default : (MessageViewType)(Convert.ToInt32(viewype));
        CreateDefaultContentView(_viewType);
        SetPageContent(_contentView);
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        string key = GenericMethods.MapValueType<string>(GetParameterValue(nameof(ResourceModel.ResourceKey)));
        if (!GenericMethods.IsListNotEmpty(ParentPage.PageData?.Resources))
        {
            await ParentPage.GetResourcesAsync(GroupConstants.RS_COMMON_GROUP).ConfigureAwait(false);
        }
        RenderMessageView(key);
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        await Task.CompletedTask.ConfigureAwait(true);
    }
}