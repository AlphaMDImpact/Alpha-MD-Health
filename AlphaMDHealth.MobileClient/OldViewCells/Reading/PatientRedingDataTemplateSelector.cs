using AlphaMDHealth.Model;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Template to display readings list either in one row or in 2 row
/// </summary>
public class PatientRedingDataTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Two row data Template to display readings list 
    /// </summary>
    public DataTemplate TwoRowList { get; set; }

    /// <summary>
    /// Single row data Template to display readings list 
    /// </summary>
    public DataTemplate SingleRowList { get; set; }

    /// <summary>
    /// Action on template selection
    /// </summary>
    /// <param name="item">data item</param>
    /// <param name="container">bindable container</param>
    /// <returns>data template</returns>
    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        return ((PatientReadingUIModel)item).IsTwoRowList ? TwoRowList : SingleRowList;
    }
}