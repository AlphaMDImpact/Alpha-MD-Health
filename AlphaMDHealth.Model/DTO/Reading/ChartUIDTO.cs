namespace AlphaMDHealth.Model;

public class ChartUIDTO
{
    public bool ShowGraph { get; set; }
    public OptionModel SelectedDuration { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<ChartLineModel> Lines { get; set; }
    public List<ChartBandModel> Bands { get; set; }
}

public class ChartLineModel
{
    public string LineColor { get; set; }
    public string LineName { get; set; }
    public List<DataPointModel> ChartData { get; set; }
}

public class ChartBandModel
{
    public List<BandModel> Ranges { get; set; }
    public string BandName { get; set; }
    public string Color { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
}

public class BandModel
{
    public string BandName { get; set; }
    public DateTime BandDate { get; set; } = DateTime.Now;
    public string Color { get; set; }
    public double Value { get; set; }
}

public class DataPointModel
{
    public DateTime DateTime { get; set; }
    public double Value { get; set; }
}