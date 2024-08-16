using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace AlphaMDHealth.WebClient;

public partial class AmhChartControl : AmhBaseControl
{
    private string _dateFormat;
    private string _timeFormat;
    private string _datetimeFormat;
    private List<ChartBandModel> bands;

    private ChartUIDTO? _value;
    /// <summary>
    /// Control value represents text of Label control
    /// </summary>
    [Parameter]
    public ChartUIDTO Value
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
            }
            Refresh();
        }
    }
    /// <summary>
    /// Bindable property of Value
    /// </summary>
    [Parameter]
    public EventCallback<ChartUIDTO> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<string> OnButtonClicked { get; set; }


    protected override Task OnInitializedAsync()
    {
        GetDateTimeFormat();
        // GetData();
        CreateListForBand();
        return base.OnInitializedAsync();
    }

    //private void GetData()
    //{
    //    if (Value != null)
    //    {
    //        Value.StartDate = Value.Lines?.Min(x => x.ChartData?.Min(y => y.DateTime)).Value;
    //        Value.EndDate = Value.Lines?.Max(x => x.ChartData?.Max(y => y.DateTime)).Value;
    //    }
    //}

    private string GetDateTimeLabelText()
    {
        if (Value.StartDate.Value.Date == Value.EndDate.Value.Date)
        {
            return GenericMethods.GetDateTimeBasedOnFormatString(Value.StartDate.Value, _dateFormat);
        }
        else
        {
            return string.Format("{0} - {1}"
                , GenericMethods.GetDateTimeBasedOnFormatString(Value.StartDate.Value, _dateFormat)
                , GenericMethods.GetDateTimeBasedOnFormatString(Value.EndDate.Value, _dateFormat));
        }
    }

    private void GetDateTimeFormat()
    {
        LibSettings.TryGetDateFormatSettings(AppState.MasterData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
        _dateFormat = GenericMethods.GetDateTimeFormat(DateTimeType.Date, dayFormat, monthFormat, yearFormat);
        _timeFormat = GenericMethods.GetDateTimeFormat(DateTimeType.Time, dayFormat, monthFormat, yearFormat);
        _datetimeFormat = GenericMethods.GetDateTimeFormat(DateTimeType.DateTime, dayFormat, monthFormat, yearFormat);
        if (AppState.AppDirection == Constants.RTL_DIRECTION_CONSTANT)
        {
            _timeFormat = string.Concat(Enumerable.Reverse(_timeFormat));
        }
    }

    private void CreateListForBand()
    {
        if (Value?.Bands?.Count > 0)
        {
            bands = Value.Bands.OrderBy(x => x.MaxValue).ToList();
            List<ChartBandModel> gaps = new List<ChartBandModel>();
            for (int i = 0; i < bands.Count; i++)
            {
                var band = bands[i];
                if (i == 0)
                {
                    if (band.MinValue > 0)
                    {
                        gaps.Add(new ChartBandModel { Color = StyleConstants.GENERIC_BACKGROUND_COLOR, MinValue = 0, MaxValue = band.MinValue });
                    }
                }
                else
                {
                    var previousBand = bands[i - 1];
                    if (previousBand.MaxValue < band.MinValue)
                    {
                        gaps.Add(new ChartBandModel { Color = StyleConstants.GENERIC_BACKGROUND_COLOR, MinValue = previousBand.MaxValue, MaxValue = band.MinValue });
                    }
                }
            }
            if (gaps?.Count > 0)
            {
                bands.AddRange(gaps);
                bands = bands.OrderByDescending(x => x.MaxValue).ToList();
            }
            foreach (ChartBandModel band in bands)
            {
                band.Ranges = new List<BandModel>();
                if (Value.StartDate.HasValue)
                {
                    band.Ranges.Add(new BandModel { BandName = $"Max {band.BandName}", Color = band.Color, Value = band.MaxValue, BandDate = Value.StartDate.Value });
                    // band.Ranges.Add(new BandModel { BandName = $"Min {band.BandName}", Color = band.Color, Value = band.MinValue, BandDate = Value.StartDate.Value });
                }
                if (Value.EndDate.HasValue)
                {
                    // band.Ranges.Add(new BandModel { BandName = $"Min {band.BandName}", Color = band.Color, Value = band.MinValue, BandDate = Value.EndDate.Value });
                    band.Ranges.Add(new BandModel { BandName = $"Max {band.BandName}", Color = band.Color, Value = band.MaxValue, BandDate = Value.EndDate.Value });
                }
            }
        }
    }

    private string FormatCategoryAxis(object value)
    {
        if (value != null)
        {
            DateTime dateValue = (DateTime)value;
            return (Value.SelectedDuration?.OptionID) switch
            {
                ResourceConstants.R_ALL_FILTER_KEY_ID => FormatCategoryAxisForAllFilterKeyCase(dateValue),
                ResourceConstants.R_YEAR_FILTER_KEY_ID => dateValue.ToString(Constants.DEFAULT_MONTH_FORMAT),
                ResourceConstants.R_QUARTER_FILTER_KEY_ID => dateValue.ToString(Constants.MONTH_DAY_FORMAT),
                ResourceConstants.R_MONTH_FILTER_KEY_ID => dateValue.ToString(Constants.MONTH_DAY_FORMAT),
                ResourceConstants.R_WEEK_FILTER_KEY_ID => dateValue.ToString(Constants.WEEKLY_FORMAT),
                ResourceConstants.R_DAY_FILTER_KEY_ID => dateValue.ToString(Constants.ISO_TIME_STRING_FORMAT),
                _ => dateValue.ToString(Constants.EXCEL_UPLOAD_DATE_TIME_STRING_FORMAT),
            };
        }
        return string.Empty;
    }

    private string FormatCategoryAxisForAllFilterKeyCase(DateTime dateValue)
    {
        if ((Value.EndDate - Value.StartDate)?.TotalDays <= 1)
        {
            return dateValue.ToString(Constants.ISO_TIME_STRING_FORMAT);
        }
        else if ((Value.EndDate - Value.StartDate)?.TotalDays <= 7)
        {
            return dateValue.ToString(Constants.WEEKLY_FORMAT);
        }
        else if ((Value.EndDate - Value.StartDate)?.TotalDays <= 30)
        {
            return dateValue.ToString(Constants.MONTH_DAY_FORMAT);
        }
        else if ((Value.EndDate - Value.StartDate)?.TotalDays <= 90)
        {
            return dateValue.ToString(Constants.MONTH_DAY_FORMAT);
        }
        else if ((Value.EndDate - Value.StartDate)?.TotalDays <= 365)
        {
            return dateValue.ToString(Constants.DEFAULT_MONTH_FORMAT);
        }
        else if ((Value.EndDate - Value.StartDate)?.TotalDays > 365)
        {
            return dateValue.ToString(Constants.DEFAULT_YEAR_FORMAT);
        }
        return string.Empty;
    }

    public void Refresh()
    {
        CreateListForBand();
    }

    private async Task OnPrevClick()
    {
        if (IsPrevEnabled())
        {
            await OnButtonClicked.InvokeAsync(ResourceConstants.R_PREVIOUS_ACTION_KEY);
        }
    }

    private async Task OnNextClick()
    {
        if (IsNextEnabled())
        {
            await OnButtonClicked.InvokeAsync(ResourceConstants.R_NEXT_ACTION_KEY);
        }
    }
    private bool IsPrevEnabled()
    {
        return Value.SelectedDuration.OptionID != ResourceConstants.R_ALL_FILTER_KEY_ID;
    }

    private bool IsNextEnabled()
    {
        return Value.SelectedDuration.OptionID != ResourceConstants.R_ALL_FILTER_KEY_ID && Value.EndDate != null && Value.EndDate.Value.Date != DateTime.Now.Date;
    }

    //private bool IsPrevEnabled()
    //{
    //    return Value?.SelectedDuration != null && Value.SelectedDuration.OptionID != ResourceConstants.R_ALL_FILTER_KEY_ID;
    //}

    //private bool IsNextEnabled()
    //{
    //    return Value?.SelectedDuration != null && Value.SelectedDuration.OptionID != ResourceConstants.R_ALL_FILTER_KEY_ID && Value.EndDate != null && Value.EndDate.Value.Date != DateTime.Now.Date;
    //}

    private void ApplyTooltip(RenderTreeBuilder builder, ChartBandModel band)
    {
        if (!string.IsNullOrWhiteSpace(band.BandName))
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, $"{band.BandName}: from {band.MinValue} to {band.MaxValue}");
            builder.CloseElement();
        }
    }
}