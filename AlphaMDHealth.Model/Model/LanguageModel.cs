using SQLite;

namespace AlphaMDHealth.Model;

/// <summary>
/// Model used to store language details
/// </summary>
public class LanguageModel //: INotifyPropertyChanged
{
    /// <summary>
    /// Id used to identify uniquely
    /// </summary>
    [PrimaryKey]
    public byte LanguageID { get; set; }

    /// <summary>
    /// Code of language
    /// </summary>
    public string LanguageCode { get; set; }

    /// <summary>
    /// Name of Language
    /// </summary>
    public string LanguageName { get; set; }

    /// <summary>
    /// Flag which indicates 
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// flag based on which content flow direction decided
    /// </summary>
    public bool IsRightToLeft { get; set; }

    /// <summary>
    /// Flag which indicates language is active or not
    /// </summary>
    public bool IsActive { get; set; } = true;

    //private string _fontColor;

    //private string _bgColor;

    ///// <summary>
    ///// PropertyChanged event
    ///// </summary>
    //public event PropertyChangedEventHandler PropertyChanged;

    ///// <summary>
    ///// lnaguage Text Color
    ///// </summary>
    //[Ignore]
    //public string TextColor
    //{
    //    get { return _fontColor; }
    //    set
    //    {
    //        _fontColor = value;
    //        if (PropertyChanged != null)
    //        {
    //            PropertyChanged(this, new PropertyChangedEventArgs("TextColor"));
    //        }
    //    }
    //}

    ///// <summary>
    ///// BGColor
    ///// </summary>
    //[Ignore]
    //public string BGColor
    //{
    //    get { return _bgColor; }
    //    set
    //    {
    //        _bgColor = value;
    //        if (PropertyChanged != null)
    //        {
    //            PropertyChanged(this, new PropertyChangedEventArgs("BGColor"));
    //        }
    //    }
    //}

    ///// <summary>
    ///// SetColors for Langugae selection
    ///// </summary>
    ///// <param name="isSelected">which language is selected</param>
    //public void SetColors(bool isSelected)
    //{
    //    BGColor = GenericMethods.GetBackgroundColor(isSelected);
    //    TextColor = GenericMethods.GetTextColor(isSelected);
    //}
}