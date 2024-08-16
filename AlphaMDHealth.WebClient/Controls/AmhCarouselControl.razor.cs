using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Runtime.Serialization;

namespace AlphaMDHealth.WebClient;

public partial class AmhCarouselControl<DataSourceType> where DataSourceType : class
{
    private MudCarousel<DataSourceType> _carousel = new MudCarousel<DataSourceType>();

    /// <summary>
    /// Autocycle carausel 
    /// </summary>
    [Parameter]
    [DataMember]
    public string Heading { get; set; }
    
    /// <summary>
    /// Autocycle carausel 
    /// </summary>
    [Parameter]
    [DataMember]
    public bool Autocycle { get; set; } = false;

    /// <summary>
    /// Autocycle carausel 
    /// </summary>
    [Parameter]
    [DataMember]
    public bool ShowArrows { get; set; } = true;

    /// <summary>
    /// Autocycle carausel 
    /// </summary>
    [Parameter]
    [DataMember]
    public bool ShowBullets { get; set; } = true;

    /// <summary>
    /// Autocycle carausel 
    /// </summary>
    [Parameter]
    [DataMember]
    public bool EnableSwipeGesture { get; set; } = true;

    /// <summary>
    /// Datasource of generic type <DataSourceType> 
    /// </summary>
    [Parameter]
    [DataMember]
    public IEnumerable<DataSourceType> DataSource { get; set; }
    private AmhViewCellModel _sourceFields;

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

    private int _value;
    /// <summary>
    /// Control value represents text of button control
    /// </summary>
    [Parameter]
    public int Value
    {
        get
        {
            return _value;
        }
        set
        {
            if (value != _value)
            {
                _value = value;
                ValueChanged.InvokeAsync(_value);
                OnValueChangedAction(_value);
            }
        }
    }

    /// <summary>
    /// Bindable property of Value
    /// </summary>
    [Parameter]
    public EventCallback<int> ValueChanged { get; set; }

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
}