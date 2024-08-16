using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model;

public class DemoModel
{
    [PrimaryKey]
    public long ID { get; set; }

    public string Name { get; set; }

    public string Image { get; set; } 

    public string MainIcon { get; set; }

    public string NavIcon { get; set; }

    public string SubHeader { get; set; }

    public string Description { get; set; }

    public string Status { get; set; }

    public FieldTypes StatusType { get; set; }

    public IEnumerable<DemoModel> Items { get; set; }

    public string HtmlData { get; set; }

    public long GroupID { get; set; }

    public string GroupName { get; set; }

    public string BandColor { get; set; }

}