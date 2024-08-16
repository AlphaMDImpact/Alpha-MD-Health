using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class ChartDemoPage : BasePage
{
    private readonly AmhSingleSelectDropDownControl _chartTypes;
    private readonly AmhSingleSelectDropDownControl _duration;
    private readonly AmhLabelControl _chartPrimaryLabel;
    private AmhChartControl _amhChartControl;
    private ControlDemoService _demoService;
    private ChartUIDTO _chartData;
    private FieldTypes _type;

    public ChartDemoPage() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.FillAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);
        AddRowColumnDefinition(new GridLength(1, GridUnitType.Auto), 60, true);
        _chartPrimaryLabel = new AmhLabelControl(FieldTypes.HtmlPrimaryLabelControl);
        AddView(_chartPrimaryLabel);
        _chartTypes = new AmhSingleSelectDropDownControl(FieldTypes.SingleSelectDropdownControl)
        {
            ResourceKey = "SingleSelectKey",
        };
        AddView(_chartTypes);
        _duration = new AmhSingleSelectDropDownControl(FieldTypes.SingleSelectDropdownControl)
        {
            ResourceKey = "SingleSelectKey",
        };
        AddView(_duration);
        _amhChartControl = new AmhChartControl(FieldTypes.LineGraphControl)
        {
            ResourceKey = "GraphDataNotFoundKey",
        };
        AddView(_amhChartControl);
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await new AuthService(App._essentials).GetAccountDataAsync(PageData).ConfigureAwait(true);
        var demoService = new ControlDemoService(App._essentials);
        demoService.GetControlDemoPageResources(PageData);
        List<OptionModel> optionsForGraphs = demoService.GetOptionsListForCharts();
        _chartTypes.PageResources = PageData;
        _chartTypes.Options = optionsForGraphs;
        _chartPrimaryLabel.Value = "<b>Select Chart from Dropdown</b>";
        _chartTypes.OnValueChanged += _singleDropdownListForCharts_OnValueChanged;

        List<OptionModel> optionsForTypeGraphs = demoService.GetOptionsListForTypeOfCharts();
        _duration.PageResources = PageData;
        _duration.Options = optionsForTypeGraphs;
        //_chartPrimaryLabel.Value = "<b>Select Chart from Dropdown</b>";
        _duration.OnValueChanged += _singleDropdownListForCharts_OnValueChanged;
        _amhChartControl.ButtonClicked += HandleChartButtonClicked;
        AppHelper.ShowBusyIndicator = false;
    }

    private void HandleChartButtonClicked(object sender, string direction)
    {
        _demoService = new ControlDemoService(App._essentials);
        var selectedDuration = _duration.Options.FirstOrDefault(x => x.OptionID == Convert.ToInt64(_duration.Value));
        DateTime startDate = _chartData.StartDate.Value;
        DateTime endDate = _chartData.EndDate.Value;
        _chartData = new ChartUIDTO
        {
            SelectedDuration = selectedDuration,
            StartDate = startDate,
            EndDate = endDate
        };
        if (direction == ChartDataFor.Prev.ToString())
        {
            _demoService.GetGraphData1(_chartData);
        }
        else //if (direction == ChartDataFor.Next.ToString())
        {
            _chartData.SelectedDuration.SequenceNo = -_chartData.SelectedDuration.SequenceNo;
            _demoService.GetGraphData1(_chartData);
            _chartData.SelectedDuration.SequenceNo = -_chartData.SelectedDuration.SequenceNo;
        }
        _amhChartControl.Value = _chartData;
    }

    private void _singleDropdownListForCharts_OnValueChanged(object sender, EventArgs e)
    {
        _demoService = new ControlDemoService(App._essentials);
        if (_chartTypes.Value != default)
        {
            var id = Convert.ToInt64(_chartTypes.Value);
            _type = _chartTypes.Options.FirstOrDefault(x => x.OptionID == id)?.GroupName.ToEnum<FieldTypes>() ?? default;
            var selectedDuration = _duration.Options.FirstOrDefault(x => x.OptionID == Convert.ToInt64(_duration.Value));
            if (selectedDuration != null && (_type == FieldTypes.BarGraphControl || _type == FieldTypes.LineGraphControl))
            {
                _amhChartControl.PageResources = PageData;
                _amhChartControl.FieldType = _type;
                _chartData = new ChartUIDTO() { SelectedDuration = selectedDuration };
                _demoService.GetGraphData1(_chartData);
                //_amhChartControl.Value = _singleDropdownListForCharts.Value;
                _amhChartControl.Value = _chartData;
            }
        }
    }

    private void AddView(View view)
    {
        int index = PageLayout.Children?.Count() ?? 0;
        PageLayout.Add(view, 0, index);
    }
}