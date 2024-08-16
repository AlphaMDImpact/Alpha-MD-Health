using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using static MudBlazor.CategoryTypes;

namespace AlphaMDHealth.WebClient;

public partial class AmhLanguageTabControl<DataSourceType> where DataSourceType : class
{
    private int index = 0;
    private long _selectedLanguageID;
    private List<OptionModel> _languageOptions;

    private IList<TabDataStructureModel> _languageTabData { get; set; }
    /// <summary>
    /// Structure of the Table and Configuration Parameters
    /// </summary>
    [Parameter]
    [DataMember]
    public IList<TabDataStructureModel> LanguageTabData
    {
        get { return _languageTabData; }
        set
        {
            if (_languageTabData != value)
            {
                _languageTabData = value;
                LanguageTabDataChanged.InvokeAsync(value);
                index = 0;
                ResetError();
                InitializeData();
                OnAfterRender(false);
            }
        }
    }

    /// <summary>
    /// Language Data source
    /// </summary>
    [Parameter]
    [DataMember]
    public EventCallback<IList<TabDataStructureModel>> LanguageTabDataChanged { get; set; }

    /// <summary>
    /// Datasource of generic type <DataSourceType> to fill the table
    /// </summary>
    [Parameter]
    [DataMember]
    public IList<DataSourceType> DataSource { get; set; }

    /// <summary>
    /// Use Same Value for all the Language tabs so the user does not re eneter same value in all the tabs
    /// </summary>
    [Parameter]
    public bool UseSameValues { get; set; }

    /// <summary>
    /// Initialized Method
    /// </summary>
    /// <returns></returns>
    protected override Task OnInitializedAsync()
    {
        InitializeData();
        return base.OnInitializedAsync();
    }

    private void InitializeData()
    {
        var controlID = UniqueID + _resourceKey + "LanguagesTab";
        if (!string.IsNullOrWhiteSpace(controlID))
        {
            //register new key
            RegisterComponent.InvokeAsync(new KeyValuePair<string, object>(controlID, this));
        }
        MapLanguagesDataIntoTabOptions();
    }

    //protected override Task OnParametersSetAsync()
    //{
    //    InitializeData();
    //    return base.OnParametersSetAsync();
    //}

    protected override async void OnAfterRender(bool firstRender)
    {
        if (index < DataSource?.Count())
        {
            for (int item = 0; item < DataSource.Count(); item++)
            {
                if (index == item)
                {
                    index++;
                    var languageID = typeof(DataSourceType).GetProperty(nameof(LanguageModel.LanguageID)).GetValue(DataSource[item]);
                    _selectedLanguageID = Convert.ToInt64(languageID);
                    StateHasChanged();
                    await Task.Delay(1);
                }
            }
        }
        else if (index == DataSource?.Count())
        {
            index++;
            _selectedLanguageID = _languageOptions.FirstOrDefault()?.OptionID ?? default;
            StateHasChanged();
        }
    }

    public override bool ValidateControl(bool isButtonClick, Dictionary<string, object> controls)
    {
        ResetError();
        bool controlValid = true;
        if (DataSource?.Count() > 1)
        {
            int[] fieldLanguageValueCounts = new int[_languageTabData.Count()];
            int languageCount = 0;
            foreach (var item in DataSource)
            {
                languageCount++;
                var languageID = typeof(DataSourceType).GetProperty(nameof(LanguageModel.LanguageID)).GetValue(item);
                var id = $"LanguagesField-{languageID}-";
                var controlsInTab = controls.Where(x => x.Key.StartsWith(id)).ToList();
                if (controlsInTab.Count() > 0)
                {
                    foreach (KeyValuePair<string, object> control in controlsInTab)
                    {
                        TabDataStructureModel tabData = _languageTabData.FirstOrDefault(x => control.Key.Contains(x.ResourceKey));
                        int index = _languageTabData.IndexOf(tabData);
                        if (_languageTabData.Any(x => control.Key.Contains(x.ResourceKey))
                            && control.Value is AmhBaseControl)
                        {
                            var field = typeof(DataSourceType).GetProperty(tabData.DataField).GetValue(item);
                            controlValid = ((AmhBaseControl)control.Value).ValidateControl(true);
                            IsValid = IsValid && controlValid;
                            if (IsValid)
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(field)))
                                {
                                    fieldLanguageValueCounts[index] = fieldLanguageValueCounts[index] + 1;
                                    if(languageCount> 1 && fieldLanguageValueCounts[index] < languageCount)
                                    {
                                        controlValid = false;
                                        ((AmhBaseControl)control.Value).IsValid = false;
                                        IsValid = false;
                                    }
                                }
                                else if(fieldLanguageValueCounts[index] > 0)
                                {
                                    controlValid = false;
                                    ((AmhBaseControl)control.Value).IsValid = false;
                                    IsValid = false;
                                }
                                
                            }
                            if (!controlValid)
                            {
                                ErrorMessage = LibResources.GetResourceValueByKey(PageResources, ResourceConstants.R_REQUIRED_ALL_FIELD_VALIDATION_KEY);
                            }
                        }
                    }
                }
                else
                {
                    if (IsValid)
                    {
                        _selectedLanguageID = Convert.ToInt64(languageID);
                    }
                    ErrorMessage = LibResources.GetResourceValueByKey(PageResources, ResourceConstants.R_REQUIRED_ALL_FIELD_VALIDATION_KEY);
                    break;
                }
            }
        }
        SetValidationResult(isButtonClick);
        return IsValid;
    }

    

    private void ResetError()
    {
        ErrorMessage = string.Empty;
        IsValid = true;
    }

    /// <summary>
    /// Validation method
    /// </summary>
    /// <returns>flag representing tab is valid or not</returns>
    public override bool ValidateControl(bool isButtonClick)
    {
        ErrorMessage = string.Empty;
        SetValidationResult(isButtonClick);
        return IsValid;
    }

    private void OnFieldValueChanged(object text, string field)
    {
        SetControlValue(text, field);
        ValidateControl(IsButtonClick);
    }

    private void SetControlValue(object text, string field)
    {
        PropertyInfo languageProp = typeof(DataSourceType).GetProperty(nameof(LanguageModel.LanguageID));
        var data = DataSource?.FirstOrDefault(x => Convert.ToString(languageProp.GetValue(x), CultureInfo.InvariantCulture) == _selectedLanguageID.ToString());
        data.GetType().GetProperty(field).SetValue(data, text);
        if (UseSameValues)
        {
            foreach (var item in DataSource)
            {
                item.GetType().GetProperty(field).SetValue(item, text);
            }
        }
    }

    private void MapLanguagesDataIntoTabOptions()
    {
        _languageOptions = new List<OptionModel>();
        if (DataSource?.Count() >= 1)
        {
            foreach (var item in DataSource)
            {
                var id = typeof(DataSourceType).GetProperty(nameof(LanguageModel.LanguageID)).GetValue(item);
                var name = typeof(DataSourceType).GetProperty(nameof(LanguageModel.LanguageName)).GetValue(item);
                _languageOptions.Add(new OptionModel { OptionID = Convert.ToInt64(id), OptionText = name?.ToString() });
            }
        }
        _selectedLanguageID = _languageOptions.FirstOrDefault()?.OptionID ?? default;
    }
}