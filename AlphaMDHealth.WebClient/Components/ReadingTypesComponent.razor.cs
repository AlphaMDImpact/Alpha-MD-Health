using AlphaMDHealth.Model;
using Microsoft.AspNetCore.Components;
using System.Runtime.Serialization;

namespace AlphaMDHealth.WebClient;

public partial class ReadingTypesComponent
{

    /// <summary>
    /// Reading Categories data
    /// </summary>
    [Parameter]
    [DataMember]
    public List<OptionModel> ReadingCategories { get; set; }

    /// <summary>
    /// On Click event
    /// </summary>
    [Parameter]
    public EventCallback<object> ClickEvent { get; set; }

    private void OnClick(object id)
    {
        if (ReadingCategories.FirstOrDefault(x => x.IsSelected)?.OptionID != Convert.ToInt64(id))
        {
            ClickEvent.InvokeAsync(id);
        }
    }
}
