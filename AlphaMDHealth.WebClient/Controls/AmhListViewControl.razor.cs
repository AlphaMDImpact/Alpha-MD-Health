using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Runtime.Serialization;

namespace AlphaMDHealth.WebClient;

public partial class AmhListViewControl<DataSourceType> where DataSourceType : class
{
    /// <summary>
    /// Search String for cards
    /// </summary>
    private string _searchedString;

    /// <summary>
    /// _filteredDataSource of generic type <DataSourceType> of Filtered data
    /// </summary>
    private IEnumerable<DataSourceType> _filteredDataSource;

    /// <summary>
    /// IsHorizontal for control type
    /// </summary>
    [Parameter]
    public bool IsHorizontal { get; set; } = false;

    /// <summary>
    /// Datasource of generic type <DataSourceType> 
    /// </summary>
    [Parameter]
    [DataMember]
    public IEnumerable<DataSourceType> DataSource { get; set; }
    private AmhViewCellModel _sourceFields;

    /// <summary>
    /// Event Callback in dashboard case
    /// </summary>
    [Parameter]
    public EventCallback<DataSourceType> OnShowAllClicked { get; set; }

    /// <summary>
    /// Event Callback in cards
    /// </summary>
    [Parameter]
    public EventCallback<DataSourceType> OnDeleteActionClicked { get; set; }

    [Parameter]
    public bool ShowViewAll { get; set; }


    [Parameter]
    public string TableHeader { get; set; }
    [Parameter]
    public bool IsPatientMobileView { get; set; } = false;

    [Parameter]
    public bool ShowCounterFlag { get; set; } = true;

    [Parameter]
    public bool ShowGroupedData { get; set; } = false;

    [Parameter]
    public bool ShowMoreGroupedData { get; set; } = false;

    /// <summary>
    /// show NORecordFoundError btn
    /// </summary>
    [Parameter]
    public bool ShouldShowNoRecordFound { get; set; } = true;

    [Parameter]
    public IList<ButtonActionModel> ActionButtons { get; set; }
    /// <summary>
    /// It is used for style apply
    /// </summary>
    [Parameter]
    public string CssClass { get; set; } = string.Empty;
    /// <summary>
    /// Placeholder fields of data source
    /// </summary>
    [Parameter]
    [DataMember]
    public AmhViewCellModel SourceFields
    {
        get
        {
            return _sourceFields;
        }
        set
        {
            if (_sourceFields != value)
            {
                _sourceFields = value;
            }
        }
    }

    /// <summary>
    /// Is Search Enabled
    /// </summary>
    [Parameter]
    public bool ShowSearch { get; set; } = true;

    private bool _isGroupedData;
    /// <summary>
    /// Data source to load on list
    /// </summary>
    [Parameter]
    public bool IsGroupedData
    {
        get
        {
            return _isGroupedData;
        }
        set
        {
            if (_isGroupedData != value)
            {
                _isGroupedData = value;
            }
        }
    }

    protected override Task OnInitializedAsync()
    {
        FilterData();
        return base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        FilterData();
        base.OnParametersSet();
    }

    private FieldTypes GetFieldType<T>(T dataSourceType, string field, FieldTypes fieldTypes)
    {
        if (!string.IsNullOrWhiteSpace(field))
        {
            return GetFieldByName(dataSourceType, field)?.ToString().ToEnum<FieldTypes>() ?? fieldTypes;
        }
        else
        {
            return fieldTypes;
        }
    }

    private object GetFieldByName<T>(T dataSourceType, string field)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            return null;
        }
        var property = typeof(T).GetProperty(field);
        if (property == null) return null;
        return property.GetValue(dataSourceType);
    }

    private void OnShowAllClick(DataSourceType item)
    {
        OnShowAllClicked.InvokeAsync(item);
    }

    private string GetCardStyle(string bandColor)
    {
        return string.IsNullOrWhiteSpace(bandColor)
            ? "display: grid; grid-template-columns: auto 1fr auto; align-items: center; gap: 0px;"
            : $"display: grid; grid-template-columns: auto 1fr auto; align-items: center;border-left: 4px solid {bandColor};";
    }

    private string GetRadzenDataListStyle()
    {
        return IsHorizontal ? "display: flex; overflow-x: auto; white-space: nowrap;scrollbar-width:none;" : "";
    }

    private void OnSearchChanged(Object e)
    {
        _searchedString = e.ToString();
        FilterData();
    }

    private void FilterData()
    {
        if (string.IsNullOrWhiteSpace(_searchedString))
        {
            _filteredDataSource = DataSource;
        }
        else
        {
            _filteredDataSource = DataSource.Where(item => IsSearchStringFoundInItem(item));
        }
    }

    private IEnumerable<DataSourceType> FilteredDataSource => _filteredDataSource ?? DataSource;

    private bool IsSearchStringFoundInItem(DataSourceType item)
    {
        var properties = typeof(DataSourceType).GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(item)?.ToString();
            if (!string.IsNullOrWhiteSpace(value) && value.Contains(_searchedString, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private void HandleCardClick(object item)
    {
        OnValueChangedAction(item);
    }
    private void GroupHandleCardClick(DataSourceType item)
    {
        OnValueChangedAction(item);
    }

    private void OnClick(DataSourceType item)
    {
        OnDeleteActionClicked.InvokeAsync(item);
    }

    private string getShowMoreResourceKey()
    {
        var data = LibResources.GetResourceValueByKey(PageResources, ResourceConstants.R_SHOW_MORE_KEY);
        return string.Concat(data, " ", ">");
    }
}