using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Runtime.Serialization;
using static MudBlazor.CategoryTypes;

namespace AlphaMDHealth.WebClient;

public partial class AmhTableControl<DataSourceType> where DataSourceType : class
{
    private string _searchString;
    private List<DataSourceType> _filteredDataSource;
    private readonly int[] pageSizeOptions = new[] { 10, 20, 25 };
    private bool isSelected = false;

    /// <summary>
    /// Structure of the Table and Configuration Parameters
    /// </summary>
    [Parameter]
    [DataMember]
    public IList<TableDataStructureModel> TableStructure { get; set; }

    [Parameter]
    [DataMember]
    public AmhViewCellModel SourceFieldStructure { get; set; }

    [Parameter]
    public bool IsHorizontal { get; set; } = false;

    /// <summary>
    /// Is Add Allowed
    /// </summary>
    [Parameter]
    public bool? ShowAddButton { get; set; } = false;

    /// <summary>
    /// Is Add Allowed
    /// </summary>
    [Parameter]
    public bool? IsEdit { get; set; } = true;

    [Parameter]
    public bool IsPatientMobileView { get; set; } = false;

    /// <summary>
    /// Is Search Enabled
    /// </summary>
    [Parameter]
    public bool ShowSearch { get; set; } = true;

    [Parameter]
    public string BorderColorDataField { get; set; } = string.Empty;

    [Parameter]
    public string SelectedId { get; set; } = null;

    [Parameter]
    public int rowIndex { get; set; }

    /// <summary>
    /// show View All
    /// </summary>
    [Parameter]
    public bool ShowViewAll { get; set; }

    /// <summary>
    /// show Save btn
    /// </summary>
    [Parameter]
    public bool ShowSaveButton { get; set; }

    /// <summary>
    /// show Pagination btn
    /// </summary>
    [Parameter]
    public bool ShowPagination { get; set; } = true;

    [Parameter]
    public bool NoShowViewAllText { get; set; } = false;

    /// <summary>
    /// show Pagination btn
    /// </summary>
    [Parameter]
    public int RowsPerPage { get; set; }

    [Parameter]
    public string Height { get; set; }

    /// <summary>
    /// show NORecordFoundError btn
    /// </summary>
    [Parameter]
    public bool ShouldShowNoRecordFound { get; set; } = true;

    /// <summary>
    /// IsGroupData
    /// </summary>
    [Parameter]
    public bool IsGroupedData { get; set; }

    [Parameter]
    public bool ShowMoreGroupedData { get; set; }

    /// <summary>
    /// Datasource of generic type <DataSourceType> to fill the table
    /// </summary>
    [Parameter]
    [DataMember]
    public IList<DataSourceType> DataSource { get; set; }

    [Parameter]
    public EventCallback<DataSourceType> OnViewClicked { get; set; }

    [Parameter]
    public EventCallback<DataSourceType> OnSaveClicked { get; set; }

    [Parameter]
    public EventCallback<Tuple<string, DataSourceType>> OnActionClicked { get; set; }

    [Parameter]
    public string TableHeader { get; set; }

    /// <summary>
    /// It is used for style apply
    /// </summary>
    [Parameter]
    public string CssClass { get; set; } = string.Empty;

    /// <summary>
    /// List of Actions Buttons on the header
    /// </summary>
    /// 
    [DataMember]
    [Parameter]
    public IList<ButtonActionModel> ActionButtons { get; set; }

    /// <summary>
    /// After render method
    /// </summary>
    /// <param name="firstRender">first time</param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            isSelected = false;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private static List<OptionModel> GetOptions()
    {
        return new List<OptionModel> {
            new OptionModel { OptionID = 1, OptionText = "" }
        };
    }

    protected override Task OnInitializedAsync()
    {
        if (RowsPerPage < 1)
        {
            RowsPerPage = pageSizeOptions.FirstOrDefault();
        }
        UpdateFilteredDataSource();
        if (ShowAddButton == true || ShowSaveButton || ShowViewAll)
        {
            //if (ActionButtons == null)
            //{
            //    ActionButtons = new List<ButtonActionModel>();
            //}
            ActionButtons ??= new List<ButtonActionModel>();
        }
        if (ShowAddButton == true)
        {
            var hasAddButton = ActionButtons.FirstOrDefault(x => x.ButtonResourceKey == ResourceConstants.R_ADD_ACTION_KEY);
            if (hasAddButton == null)
            {
                ActionButtons.Add(new ButtonActionModel
                {
                    ButtonResourceKey = ResourceConstants.R_ADD_ACTION_KEY,
                    ButtonAction = () => { AddEditButtonClicked(null); },
                    Icon = ImageConstants.I_ADD_ICON_RESPONSIVE
                });
            }
        }
        if (ShowSaveButton)
        {
            var saveButton = new ButtonActionModel
            {
                ButtonResourceKey = ResourceConstants.R_SAVE_ACTION_KEY,
                ButtonAction = ShowSaveButtonClickEvent
            };
            ActionButtons.Add(saveButton);
        }
        if (ShowViewAll)
        {
            var viewButton = new ButtonActionModel
            {
                ButtonResourceKey = ResourceConstants.R_SHOW_MORE_KEY,
                ButtonAction = ViewAllButtonClickEvent
            };
            ActionButtons.Add(viewButton);
        }
        return base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        //_searchString = string.Empty;
        UpdateFilteredDataSource();
        if (ShowAddButton == true)
        {
            //if (ActionButtons == null)
            //{
            //    ActionButtons = new List<ButtonActionModel>();
            //}
            ActionButtons ??= new List<ButtonActionModel>();

            var hasAddButton = ActionButtons.FirstOrDefault(x => x.ButtonResourceKey == ResourceConstants.R_ADD_ACTION_KEY);
            if (hasAddButton == null)
            {
                //var addButton = new ButtonActionModel
                //{
                //    ButtonResourceKey = ResourceConstants.R_ADD_ACTION_KEY,
                //    ButtonAction = () => { AddEditButtonClicked(null); }
                //};
                ActionButtons.Add(new ButtonActionModel
                {
                    ButtonResourceKey = ResourceConstants.R_ADD_ACTION_KEY,
                    ButtonAction = () => { AddEditButtonClicked(null); }
                });
            }
        }
        base.OnParametersSet();
    }

    private void UpdateFilteredDataSource()
    {
        _filteredDataSource = DataSource?.ToList() ?? new List<DataSourceType>();
        StateHasChanged();
    }

    private void AddEditButtonClicked(DataSourceType dataItem)
    {
        _searchString = string.Empty;
        OnValueChanged.InvokeAsync(dataItem);
    }

    private void OnGroupShowAllClicked(DataSourceType dataItem)
    {
        OnViewClicked.InvokeAsync(dataItem);
    }

    private void ViewAllButtonClickEvent()
    {
        if (ShowViewAll)
        {
            OnViewClicked.InvokeAsync(null);
        }
    }

    private void ShowSaveButtonClickEvent()
    {
        if (ShowSaveButton)
        {
            OnSaveClicked.InvokeAsync(null);
        }
    }

    private void LabelClickEvent(DataSourceType dataItem, string value)
    {
        OnActionClicked.InvokeAsync(new Tuple<string, DataSourceType>(value, dataItem));
    }



    // Search filter - filter globally across multiple columns with the same input
    private Func<DataSourceType, bool> _quickFilter => x =>
    {
        var minValue = LibResources.GetMinLengthValueByKey(PageResources, ResourceConstants.R_SEARCH_TEXT_KEY);
        if (_searchString?.ToString().Length > minValue)
        {
            if (string.IsNullOrWhiteSpace(_searchString?.ToString()))
            {
                return true;
            }
            foreach (var tableStruct in TableStructure)
            {
                if (tableStruct.IsSearchable && !string.IsNullOrEmpty(tableStruct.DataField))
                {
                    var property = x.GetType().GetProperty(tableStruct.DataField);
                    var propertyValue = property.GetValue(x);
                    var propertyType = property.PropertyType;

                    // Check for string type
                    if (propertyType == typeof(string))
                    {
                        if (Convert.ToString(propertyValue).Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    else if (propertyType == typeof(int) || propertyType == typeof(long) ||
                 propertyType == typeof(short) || propertyType == typeof(byte) ||
                 propertyType == typeof(sbyte) || propertyType == typeof(uint) ||
                 propertyType == typeof(ulong) || propertyType == typeof(ushort))
                    {
                        if (long.TryParse(_searchString, out long searchNumber) && Convert.ToInt64(propertyValue) == searchNumber)
                        {
                            return true;
                        }
                    }
                    else if (propertyType == typeof(double) || propertyType == typeof(float) ||
                             propertyType == typeof(decimal))
                    {
                        if (decimal.TryParse(_searchString, out decimal searchDecimal) && Convert.ToDecimal(propertyValue) == searchDecimal)
                        {
                            return true;
                        }
                    }
                    else if (propertyType == typeof(DateTime))
                    {
                        if (DateTime.TryParse(_searchString, out DateTime searchDate) && Convert.ToDateTime(propertyValue).Date == searchDate.Date)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        else
        {
            return true;
        }
    };
    private bool getSortableFlag(TableDataStructureModel cellItem)
    {
        if (cellItem.IsEditable || cellItem.IsCheckBox)
        {
            return false;
        }
        return cellItem.IsSortable;
    }

    private IEnumerable<DataSourceType> GetFilteredElements()
    {
        return _filteredDataSource.Where(_quickFilter).ToList();
    }

    private void OnSetValueChanged(object text, string field, string id)
    {
        UpdateDataSourceValue(text, field, id);
    }

    private void OnSetCheckBoxChanged(bool text, string field, string id)
    {
        UpdateDataSourceValue(text, field, id);
    }

    private void UpdateDataSourceValue(object value, string field, string id)
    {
        var keyFieldName = TableStructure.FirstOrDefault(x => x.IsKey)?.DataField;
        if (string.IsNullOrEmpty(keyFieldName))
        {
            return;
        }

        var keyPropInfo = typeof(DataSourceType).GetProperty(keyFieldName);
        if (keyPropInfo == null)
        {
            return;
        }

        var dataItem = DataSource?.FirstOrDefault(x => Convert.ToString(keyPropInfo.GetValue(x), CultureInfo.InvariantCulture) == id);

        if (dataItem == null)
        {
            return;
        }

        var fieldPropInfo = dataItem.GetType().GetProperty(field);
        if (fieldPropInfo == null)
        {
            return;
        }

        fieldPropInfo.SetValue(dataItem, value);
        StateHasChanged();
    }

    private string getImageValue(string imageValue, dynamic dataItem, string defaultVal)
    {
        string dataImageValue = "";
        if (!string.IsNullOrWhiteSpace(imageValue))
        {
            dataImageValue = GetDataItemPropertyValueFromContext(dataItem, imageValue) ?? "";
        }
        else
        {
            dataImageValue = defaultVal;
        }
        return dataImageValue;
    }

    protected string GetInitials(string names)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(names))
            {
                return string.Empty;
            }
            // Extract the first character out of each block of non-whitespace
            string[] nameArray = names.Trim().Split(' ');
            return (string.Format(CultureInfo.CurrentCulture, "{0}{1}", nameArray[0].Substring(0, 1), (nameArray.Length > 1)
                ? nameArray[nameArray.Length - 1].Substring(0, 1) : string.Empty))?.ToUpper(CultureInfo.CurrentCulture);
        }
        catch (Exception ex)
        {
            return string.Empty;
        }
    }

    private string GetPropertyValueFromContext(dynamic context, string fieldValue)
    {
        var property = context.DataItem.GetType().GetProperty(fieldValue);
        if (property == null)
        {
            return string.Empty; // Return an empty string or handle this case as needed
        }
        var value = property.GetValue(context.DataItem);
        return value?.ToString() ?? string.Empty;
    }

    private string GetDataItemPropertyValueFromContext(dynamic dataItem, string fieldValue)
    {
        var property = dataItem.GetType().GetProperty(fieldValue);
        if (property == null)
        {
            return string.Empty; // Return an empty string or handle this case as needed
        }
        var value = property.GetValue(dataItem);
        return value?.ToString() ?? string.Empty;
    }

    private object GetSortDataItemPropertyValueFromContext(dynamic dataItem, string fieldValue)
    {
        var property = dataItem.GetType().GetProperty(fieldValue);
        if (property == null)
        {
            return null; // Return an empty string or handle this case as needed
        }

        var value = property.GetValue(dataItem);

        return value switch
        {
            int intValue => intValue,
            short shortValue => shortValue,
            byte byteValue => byteValue,
            _ => value // For other types, return the original value
        };
    }



    private string getCheckMaxHeaderWidthSize(string maxHeaderWidth, bool ShowRowColumnHeader)
    {
        if (!string.IsNullOrEmpty(maxHeaderWidth))
        {
            if (ShowRowColumnHeader)
            {
                return $"max-width: {maxHeaderWidth}";
            }
            else
            {
                return $"width: {maxHeaderWidth}";
            }
        }
        return string.Empty;
    }


    private void RowClicked(MudBlazor.DataGridRowClickEventArgs<DataSourceType> args)
    {
        if (IsEdit == true)
        {
            isSelected = true;
            SelectedId = string.IsNullOrEmpty(TableStructure.FirstOrDefault(x => x.IsKey)?.DataField) || args.Item.GetType().GetProperty(TableStructure.FirstOrDefault(x => x.IsKey)?.DataField)?.GetValue(args.Item) is null ? string.Empty : Convert.ToString(args.Item.GetType().GetProperty(TableStructure.FirstOrDefault(x => x.IsKey)?.DataField)?.GetValue(args.Item));
            rowIndex = args.RowIndex;
            AddEditButtonClicked(args.Item);
        }
    }

    private Func<DataSourceType, int, string> _rowStyleFunc => (item, i) =>
    {
        return "cursor:pointer";
    };
    private Func<DataSourceType, int, string> _rowClassFunc => (item, i) =>
    {
        if (isSelected)
        {
            if (rowIndex == i)
            {
                return "table-selected-item";
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(SelectedId) && CheckSelectedId(item))
            {
                return "table-selected-item";
            }
        }
        return "";
    };

    private bool CheckSelectedId(DataSourceType item)
    {
        var primaryDataField = TableStructure.FirstOrDefault(x => x.IsKey)?.DataField;

        if (string.IsNullOrEmpty(primaryDataField))
            return false;

        var property = item.GetType().GetProperty(primaryDataField);

        if (property == null)
            return false;

        var propertyValue = property.GetValue(item);

        if (propertyValue == null)
            return false;

        return SelectedId == Convert.ToString(propertyValue);
    }

    private string getTitle(string DataHeader, string DataHeaderValue)
    {
        return string.IsNullOrWhiteSpace(DataHeaderValue)
            ? LibResources.GetResourceValueByKey(PageResources, DataHeader)
            : DataHeaderValue;
    }
    public FieldTypes GetBadgeFieldType(string fieldType)
    {
        return fieldType switch
        {
            nameof(FieldTypes.PrimaryBadgeControl) => FieldTypes.PrimaryBadgeControl,
            nameof(FieldTypes.SecondaryBadgeControl) => FieldTypes.SecondaryBadgeControl,
            nameof(FieldTypes.SuccessBadgeControl) => FieldTypes.SuccessBadgeControl,
            nameof(FieldTypes.DangerBadgeControl) => FieldTypes.DangerBadgeControl,
            nameof(FieldTypes.WarningBadgeControl) => FieldTypes.WarningBadgeControl,
            nameof(FieldTypes.InfoBadgeControl) => FieldTypes.InfoBadgeControl,
            nameof(FieldTypes.LightBadgeControl) => FieldTypes.LightBadgeControl,
            nameof(FieldTypes.DarkBadgeControl) => FieldTypes.DarkBadgeControl,
            _ => FieldTypes.PrimaryBadgeControl,
        };
    }


}