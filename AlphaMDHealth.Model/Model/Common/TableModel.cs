namespace AlphaMDHealth.Model;

public class TableModel
{
    public string Item { get; set; }
    public string Amount { get; set; }
}

public class ListModel
{
    public string ImageName { get; set; }
    public List<string> MiddleSection { get; set; }
    public string EndSection { get; set; }
}
