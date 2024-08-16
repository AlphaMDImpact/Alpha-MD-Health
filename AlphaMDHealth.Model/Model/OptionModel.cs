using System.ComponentModel;

namespace AlphaMDHealth.Model;

/// <summary>
/// Option Model
/// </summary>
public class OptionModel : INotifyPropertyChanged
{
    /// <summary>
    /// Image value to render as icon
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Group Name  
    /// </summary>
    public string GroupName { get; set; }

    /// <summary>
    /// Flag to Store IsDefault
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Stores Option Id 
    /// </summary>
    public long OptionID { get; set; }

    /// <summary>
    /// Stores Option Text
    /// </summary>
    public string OptionText { get; set; }

    /// <summary>
    /// Stores Sequence Number 
    /// </summary>
    public long SequenceNo { get; set; }

    /// <summary>
    /// stores parent Option Id 
    /// </summary>
    public long ParentOptionID { get; set; }

    /// <summary>
    /// Stores Parent Option Text
    /// </summary>
    public string ParentOptionText { get; set; }

    public DateTime From { get; set; }
    public DateTime To { get; set; }

    /// <summary>
    /// Flag to Store IsDisabled
    /// </summary>
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Flag to Store IsActive
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Flag to Store IsSelected
    /// </summary>
    private bool _isSelected;

    /// <summary>
    /// Flag to Store IsSelected
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSelected"));
        }
    }

    /// <summary>
    /// Stores Property Changed Event 
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
}