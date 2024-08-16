using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model;

public class ButtonActionModel
{
    public string ButtonID { get; set; }
    public string ButtonResourceKey { get; set; }
    public FieldTypes FieldType { get; set; }
    public string Icon { get; set; }
    public string ButtonClass { get; set; }
    public Action ButtonAction { get; set; }

    public string Style { get; set; }

    public string Value { get; set; }
    //public List<OptionModel> Options { get; set; }
    //public Func<OptionModel, Task> SelectedValueChanged;
}