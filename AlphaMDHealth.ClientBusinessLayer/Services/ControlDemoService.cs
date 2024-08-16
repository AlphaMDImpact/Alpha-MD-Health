using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientBusinessLayer;

public class ControlDemoService : BaseService
{
    public ControlDemoService(IEssentials essentials):base(essentials) { }
    public void GetControlDemoPageResources(BaseDTO pageData)
    {
        pageData.Resources.Add(new ResourceModel { ResourceID = -1, ResourceKey = FieldTypes.TextEntryControl.ToString(), ResourceValue = "Text Entry", PlaceHolderValue = "Enter Text", InfoValue = "<p><strong>Text Field:</strong> This tutorial will not teach you how servers are processing input.\r\nProcessing input is explained in our <a href=\"/php/default.asp\" target=\"_blank\">PHP tutorial</a>.</p>", IsRequired = true, MinLength = 2, MaxLength = 30 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -2, FieldType = FieldTypes.MultiLineEntryControl.ToString(), ResourceKey = FieldTypes.MultiLineEntryControl.ToString(), ResourceValue = "Multiline Entry", PlaceHolderValue = "Enter Text", InfoValue = "This field is for Text type of data", IsRequired = true, MinLength = 2, MaxLength = 50 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -3, ResourceKey = FieldTypes.EmailEntryControl.ToString(), ResourceValue = "Email Entry", PlaceHolderValue = "Enter Email", InfoValue = "This field is for Email type of entry data", IsRequired = false, MinLength = 8, MaxLength = 30 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -4, ResourceKey = FieldTypes.PasswordEntryControl.ToString(), ResourceValue = "Password Entry", PlaceHolderValue = "Enter Password", InfoValue = "This field is for Password type of entry data", IsRequired = true, MinLength = 5, MaxLength = 30 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -5, FieldType = FieldTypes.NumericEntryControl.ToString(), ResourceKey = FieldTypes.NumericEntryControl.ToString(), ResourceValue = "Numeric Entry", PlaceHolderValue = "Enter Numeric", InfoValue = "This field is for Numeric type of entry data", IsRequired = true, MinLength = -8, MaxLength = 999 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -6, ResourceKey = "NumericEntryControlwithoutIcon", ResourceValue = "Numeric Entry", PlaceHolderValue = "Enter Numeric", InfoValue = "This field is for Numeric type of entry data", IsRequired = true, MinLength = -2, MaxLength = 10 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -7, ResourceKey = "NumericEntryControlDisabled", ResourceValue = "My Disabled Numeric Field", PlaceHolderValue = "Enter Numeric", InfoValue = "This field is for Numeric type of entry data", IsRequired = true, MinLength = -2, MaxLength = 10 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -8, ResourceKey = FieldTypes.CounterEntryControl.ToString(), ResourceValue = "Counter Control", PlaceHolderValue = "Counter", InfoValue = "This field is for Counter type of entry data", IsRequired = true, MinLength = -4, MaxLength = 30 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -9, ResourceKey = FieldTypes.DecimalEntryControl.ToString(), ResourceValue = "Decimal Entry", PlaceHolderValue = "Enter Decimal", InfoValue = "This field is for Decimal type of entry data", IsRequired = true, MinLength = -3, MaxLength = 9999 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -10, ResourceKey = FieldTypes.ColorPickerControl.ToString(), ResourceValue = "Color Picker", PlaceHolderValue = "Select Color", InfoValue = "<p><strong>Lorem Ipsum</strong> is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.</p>", IsRequired = true });
        pageData.Resources.Add(new ResourceModel { ResourceID = -11, ResourceKey = "DisableColorPickerControl", ResourceValue = "Disable Color Picker", PlaceHolderValue = "Select Color", InfoValue = "<p><strong>Lorem Ipsum</strong> is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.</p>", IsRequired = true });
        pageData.Resources.Add(new ResourceModel { ResourceID = -12, ResourceKey = FieldTypes.SwitchControl.ToString(), ResourceValue = "Switch Control", PlaceHolderValue = "Toggle the switch", InfoValue = "This field is for toggle switch", IsRequired = true });
        pageData.Resources.Add(new ResourceModel { ResourceID = -13, ResourceKey = "SwitchDisableControl", ResourceValue = "Disable Switch Control", PlaceHolderValue = "Toggle the switch", InfoValue = "This field is for toggle switch", IsRequired = true });
        pageData.Resources.Add(new ResourceModel { ResourceID = -14, ResourceKey = "SingleSelectKey", GroupDesc = "The Given Option not found", ResourceValue = "Single Select without Edit", PlaceHolderValue = "Single Select without Edit", InfoValue = "Single select info", IsRequired = true, MinLength = 0, MaxLength = 0 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -15, ResourceKey = "MultiSelectKey", GroupDesc = "The Given Option not found", ResourceValue = "Multi Select without Edit", PlaceHolderValue = "Multi Select without Edit", InfoValue = "Multi select info", IsRequired = true, MinLength = 2, MaxLength = 5 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -16, ResourceKey = "EditableSingleSelectKey", GroupDesc = "The Given Option not found", ResourceValue = "Single Select with Edit", PlaceHolderValue = "Single Select with Edit", InfoValue = "", IsRequired = true, MinLength = 0, MaxLength = 0 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -17, ResourceKey = "EditableMultiSelectKey", GroupDesc = "The Given Option not found", ResourceValue = "Multi Select with Edit", PlaceHolderValue = "Multi Select with Edit", InfoValue = "", IsRequired = true, MinLength = 1, MaxLength = 3 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -18, ResourceKey = "SingleUploadControl", ResourceValue = "Single Document Upload", PlaceHolderValue = "Tap on box to Upload document", KeyDescription= "jpeg,png,jpg,jfif,svg", IsRequired = true, MinLength = 1, MaxLength = 1 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -19, ResourceKey = "MultipleUploadControl", ResourceValue = "Documents", PlaceHolderValue = "Tap on box to Upload document", KeyDescription = "pdf,doc,docx,xlsx,xls,png,jpeg,jpg,svg", IsRequired = true, MinLength = 1, MaxLength = 7 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -20, ResourceKey = "ConfirmDeleteActionKey", ResourceValue = "Delete List Item", PlaceHolderValue = "Are You Sure you want to delete this item?" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -21, ResourceKey = "SearchTextKey", ResourceValue = "Search", PlaceHolderValue = "Search..." });
        pageData.Resources.Add(new ResourceModel { ResourceID = -22, ResourceKey = FieldTypes.SecondaryButtonControl.ToString(), ResourceValue = "Secondary Button", IsRequired = true });
        pageData.Resources.Add(new ResourceModel { ResourceID = -23, ResourceKey = FieldTypes.PrimaryButtonControl.ToString(), ResourceValue = "Primary Button", IsRequired = true });
        pageData.Resources.Add(new ResourceModel { ResourceID = -24, ResourceKey = FieldTypes.TertiaryButtonControl.ToString(), ResourceValue = "Tertiary Button", IsRequired = true });
        pageData.Resources.Add(new ResourceModel { ResourceID = -25, ResourceKey = FieldTypes.TransparentButtonControl.ToString(), ResourceValue = "Transparent Button", IsRequired = true });
        pageData.Resources.Add(new ResourceModel { ResourceID = -26, ResourceKey = FieldTypes.DeleteButtonControl.ToString(), ResourceValue = "Delete Button", IsRequired = true });
        pageData.Resources.Add(new ResourceModel { ResourceID = -27, ResourceKey = FieldTypes.HorizontalCheckBoxControl.ToString(), ResourceValue = "Check Boxes", InfoValue = "This field is for CheckBox data", IsRequired = true, MinLength = 2 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -28, ResourceKey = FieldTypes.VerticalCheckBoxControl.ToString(), ResourceValue = "Check Boxes", InfoValue = "This field is for CheckBox data", IsRequired = true, MinLength = 2 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -29, ResourceKey = FieldTypes.VerticalRadioButtonControl.ToString(), ResourceValue = "Radio Buttons", InfoValue = "This field is for Radio List data", IsRequired = true, MinLength = 2 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -31, ResourceKey = FieldTypes.PrimaryButtonControl.ToString(), ResourceValue = "Primary button" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -32, ResourceKey = FieldTypes.SecondaryButtonControl.ToString(), ResourceValue = "Secondary button" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -33, ResourceKey = FieldTypes.TertiaryButtonControl.ToString(), ResourceValue = "Tertiary button" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -34, ResourceKey = FieldTypes.TransparentButtonControl.ToString(), ResourceValue = "Transparent button" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -35, ResourceKey = FieldTypes.DeleteButtonControl.ToString(), ResourceValue = "Delete Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -36, ResourceKey = FieldTypes.PrimaryTransparentButtonControl.ToString(), ResourceValue = "Primary Transparent Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -37, ResourceKey = FieldTypes.SecondaryTransparentButtonControl.ToString(), ResourceValue = "Secondary Transparent Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -38, ResourceKey = FieldTypes.TertiaryTransparentButtonControl.ToString(), ResourceValue = "Tertiary Transparent Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -39, ResourceKey = FieldTypes.DeleteTransparentButtonControl.ToString(), ResourceValue = "Delete Transparent Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -40, ResourceKey = FieldTypes.PrimaryBorderTransparentButtonControl.ToString(), ResourceValue = "Primary Border Transparent Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -41, ResourceKey = FieldTypes.SecondaryBorderTransparentButtonControl.ToString(), ResourceValue = "Secondary Border Transparent Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -42, ResourceKey = FieldTypes.TertiaryBorderTransparentButtonControl.ToString(), ResourceValue = "Tertiary Border Transparent Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -43, ResourceKey = FieldTypes.DeleteBorderTransparentButtonControl.ToString(), ResourceValue = "Delete Border Transparent Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -44, ResourceKey = FieldTypes.PrimaryExButtonControl.ToString(), ResourceValue = "primary Ex Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -45, ResourceKey = FieldTypes.SecondaryExButtonControl.ToString(), ResourceValue = "Secondary Ex Btn  kjdsghdfigkdufigub" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -46, ResourceKey = FieldTypes.TertiaryExButtonControl.ToString(), ResourceValue = "Tertiary Ex Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -47, ResourceKey = FieldTypes.DeleteExButtonControl.ToString(), ResourceValue = "Delete Ex Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -48, ResourceKey = FieldTypes.PrimaryTransparentExButtonControl.ToString(), ResourceValue = "Primary Transparent Ex Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -49, ResourceKey = FieldTypes.SecondaryTransparentExButtonControl.ToString(), ResourceValue = "Secondary Transparent Ex Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -50, ResourceKey = FieldTypes.TertiaryTransparentExButtonControl.ToString(), ResourceValue = "Tertiary Transparent Ex Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -51, ResourceKey = FieldTypes.DeleteTransparentExButtonControl.ToString(), ResourceValue = "Delete Transparent Ex Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -52, ResourceKey = FieldTypes.PrimaryBorderTransparentExButtonControl.ToString(), ResourceValue = "Primary Border Transparent Ex Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -53, ResourceKey = FieldTypes.SecondaryBorderTransparentExButtonControl.ToString(), ResourceValue = "Secondary Border Transparent Ex Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -54, ResourceKey = FieldTypes.TertiaryBorderTransparentExButtonControl.ToString(), ResourceValue = "Tertiary Border Transparent Ex Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -55, ResourceKey = FieldTypes.DeleteBorderTransparentExButtonControl.ToString(), ResourceValue = "Delete Border Transparent Ex Btn" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -56, ResourceKey = FieldTypes.RichTextControl.ToString(), ResourceValue = "Rich Text Entry", PlaceHolderValue = "Enter Text to edit", InfoValue = "This field is for Rich Text type of data", IsRequired = true, MinLength = 20, MaxLength = 1000 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -57, ResourceKey = ResourceConstants.R_TODAY_TEXT_KEY, ResourceValue = "Today", });
        pageData.Resources.Add(new ResourceModel { ResourceID = -58, ResourceKey = FieldTypes.ProgressBarControl.ToString(), ResourceValue = " Progress Bar Control", InfoValue = "This field is for Progress bar Control", MinLength = 0, MaxLength = 100 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -59, ResourceKey = ResourceConstants.R_SAVE_ACTION_KEY, ResourceValue = "Save" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -60, ResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY, ResourceValue = "Cancel" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -61, ResourceKey = FieldTypes.ProgressIndicatorControl.ToString(), ResourceValue = " Progress Indicator Control", InfoValue = "This field is for Progress Indicator Control", MinLength = 0, MaxLength = 100 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -62, ResourceKey = FieldTypes.DateControl.ToString(), ResourceValue = "Date Picker", InfoValue = "This field is for Date Picker", IsRequired = true, PlaceHolderValue = "Select Date", MinLength = -10, MaxLength = 10 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -63, ResourceKey = FieldTypes.DateTimeControl.ToString(), ResourceValue = "DateTime Picker", InfoValue = "This field is for DateTime Picker", IsRequired = true, PlaceHolderValue = "Select Date & Time" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -64, ResourceKey = FieldTypes.TimeControl.ToString(), ResourceValue = "Time Picker", InfoValue = "This field is for Time Picker", IsRequired = true, PlaceHolderValue = "Select Time", MinLength = 3, MaxLength = 10 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -65, ResourceKey = "DisabledDateControl", ResourceValue = "Disabled Date Picker", PlaceHolderValue = "Select Date", InfoValue = "This field is for Date Picker", IsRequired = true, MinLength = 3, MaxLength = 10 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -66, ResourceKey = "DisabledDateTimeControl", ResourceValue = "Disabled DateTime Picker", PlaceHolderValue = "Select Date & Time", InfoValue = "This field is for DateTime Picker", IsRequired = true, MinLength = 3, MaxLength = 10 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -67, ResourceKey = "DisabledTimeControl", ResourceValue = "Disabled Time Picker", PlaceHolderValue = "Select Time", InfoValue = "This field is for Time Picker", IsRequired = true, MinLength = 3, MaxLength = 10 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -68, ResourceKey = "DisabledPhonecontrol", ResourceValue = "Disabled Phone Number Control", InfoValue = "This field is for Phone Number Control", IsRequired = true, PlaceHolderValue = "Enter Phone Number", MaxLength = 10, MinLength = 8 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -69, FieldType = FieldTypes.MobileNumberControl.ToString(), ResourceKey = FieldTypes.MobileNumberControl.ToString(), ResourceValue = "Phone Number Control", InfoValue = "This field is for Phone Number Control", IsRequired = true, PlaceHolderValue = "Enter Phone Number", MaxLength = 10, MinLength = 8 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -70, ResourceKey = "GraphDataNotFoundKey", ResourceValue = "The Graph Does Not Nontain Any Data" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -71, ResourceKey = FieldTypes.UploadControl.ToString(), ResourceValue = "Single Upload", PlaceHolderValue = "Select a File", InfoValue = "info", IsRequired = true, MinLength = 1, MaxLength = 1 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -72, ResourceKey = FieldTypes.UploadControl.ToString(), ResourceValue = "Multiple Upload", PlaceHolderValue = "Select FIles", InfoValue = "info", IsRequired = true, MinLength = 2, MaxLength = 5 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -73, ResourceKey = FieldTypes.HorizontalSliderControl.ToString(), ResourceValue = "Horizontal Slider", InfoValue = "This field is for Horozontal Slider", IsRequired = true, MinLength = 0, MaxLength = 990 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -74, ResourceKey = FieldTypes.VerticalSliderControl.ToString(), ResourceValue = "Vertical Slider", InfoValue = "This field is for Vertical Slider", IsRequired = true, MinLength = 0, MaxLength = 990 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -75, ResourceKey = FieldTypes.SingleSelectEditableDropdownControl.ToString(), ResourceValue = "Single Select editable", PlaceHolderValue = "Select Name", InfoValue = "Single select info", IsRequired = true, GroupDesc = "The Given Option not found" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -76, ResourceKey = FieldTypes.SingleSelectDropdownControl.ToString(), ResourceValue = "Single Select", PlaceHolderValue = "Select Name", InfoValue = "Single select info", IsRequired = true });
        pageData.Resources.Add(new ResourceModel { ResourceID = -77, ResourceKey = FieldTypes.MultiSelectDropdownControl.ToString(), ResourceValue = "Multi Select", PlaceHolderValue = "Select Names", InfoValue = "Multi select info", IsRequired = true, MinLength = 3 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -78, ResourceKey = FieldTypes.MultiSelectEditableDropdownControl.ToString(), ResourceValue = "Multi Select editable", PlaceHolderValue = "Select Names", InfoValue = "Multi select info", IsRequired = true, MinLength = 3, });
        pageData.Resources.Add(new ResourceModel { ResourceID = -79, ResourceKey = FieldTypes.SingleSelectEditableDropdownControl.ToString() + "Disabled", ResourceValue = "Disabled Single Select editable", PlaceHolderValue = "Select Name", InfoValue = "Single select info", IsRequired = true, GroupDesc = "The Given Option not found" });
        pageData.Resources.Add(new ResourceModel { ResourceID = -80, ResourceKey = FieldTypes.SingleSelectDropdownControl.ToString() + "Disabled", ResourceValue = "Disabled Single Select", PlaceHolderValue = "Select Name", InfoValue = "Single select info", IsRequired = true });
        pageData.Resources.Add(new ResourceModel { ResourceID = -81, ResourceKey = FieldTypes.MultiSelectDropdownControl.ToString() + "Disabled", ResourceValue = "Disabled Multi Select", PlaceHolderValue = "Select Names", InfoValue = "Multi select info", IsRequired = true, MinLength = 3 });
        pageData.Resources.Add(new ResourceModel { ResourceID = -82, ResourceKey = FieldTypes.MultiSelectEditableDropdownControl.ToString() + "Disabled", ResourceValue = "Disabled Multi Select editable", PlaceHolderValue = "Select Names", InfoValue = "Multi select info", IsRequired = true, MinLength = 3, });

        pageData.Resources.Add(new ResourceModel { ResourceID = -83, ResourceKey = FieldTypes.MessageControl.ToString(), KeyDescription = ImageConstants.I_BLUETOOTH_PNG, ResourceValue = "Message Control", PlaceHolderValue = "Message Control Placeholder Value", InfoValue = "<p>find my <b>Message Control Info</b> here.</p>", FieldType = FieldTypes.MessageControl.ToString() });
        pageData.Resources.Add(new ResourceModel { ResourceID = -84, ResourceKey = FieldTypes.TopHeadingMessageControl.ToString(), KeyDescription = "Medication-list-icon.svg", ResourceValue = "TopHeading Message Control", PlaceHolderValue = "TopHeading Message Control Placeholder Value", InfoValue = "<p>find my <b>TopHeading Message Control Info</b> here.</p>", FieldType = FieldTypes.TopHeadingMessageControl.ToString() });
        pageData.Resources.Add(new ResourceModel { ResourceID = -85, ResourceKey = FieldTypes.CloseButtonMessageControl.ToString(), KeyDescription = "Medication-list-icon.svg", ResourceValue = "CloseButton Message Control", PlaceHolderValue = "CloseButton Message Control Placeholder Value", InfoValue = "<p>find my <b>CloseButton Message Control Info</b> here.</p>", FieldType = FieldTypes.CloseButtonMessageControl.ToString() });
        pageData.Resources.Add(new ResourceModel { ResourceID = -86, ResourceKey = FieldTypes.TopHeadingWithCloseButtonMessageControl.ToString(), KeyDescription = "Medication-list-icon.svg", ResourceValue = "TopHeadingWithCloseButton Message Control", PlaceHolderValue = "TopHeadingWithCloseButton Message Control Placeholder Value", InfoValue = "<p>find my <b>TopHeadingWithCloseButton Message Control Info</b> here.</p>", FieldType = FieldTypes.TopHeadingWithCloseButtonMessageControl.ToString() });
        pageData.Resources.Add(new ResourceModel { ResourceID = -87, ResourceKey = FieldTypes.PopupMessageControl.ToString(), KeyDescription = null, ResourceValue = "Popup Message Control", PlaceHolderValue = "Popup Message Control Placeholder Value", InfoValue = "<p>find my <b>Popup Message Control Info</b> here.</p>", FieldType = FieldTypes.PopupMessageControl.ToString() });
        pageData.Resources.Add(new ResourceModel { ResourceID = -88, ResourceKey = FieldTypes.TopHeadingPopupMessageControl.ToString(), KeyDescription = null, ResourceValue = "TopHeadingPopup Message Control", PlaceHolderValue = "TopHeadingPopup Message Control Placeholder Value", InfoValue = "<p>find my <b>TopHeadingPopup Message Control Info</b> here.</p>", FieldType = FieldTypes.TopHeadingPopupMessageControl.ToString() });
        pageData.Resources.Add(new ResourceModel { ResourceID = -89, ResourceKey = FieldTypes.CloseButtonPopupMessageControl.ToString(), KeyDescription = null, ResourceValue = "CloseButtonPopup Message Control", PlaceHolderValue = "CloseButtonPopup Message Control Placeholder Value", InfoValue = "<p>find my <b>CloseButtonPopup Message Control Info</b> here.</p>", FieldType = FieldTypes.CloseButtonPopupMessageControl.ToString() });
        pageData.Resources.Add(new ResourceModel { ResourceID = -90, ResourceKey = FieldTypes.TopHeadingWithCloseButtonPopupMessageControl.ToString(), KeyDescription = null, ResourceValue = "TopHeadingWithCloseButtonPopup Message Control", PlaceHolderValue = "TopHeadingWithCloseButtonPopup Message Control Placeholder Value", InfoValue = "<p>find my <b>TopHeadingWithCloseButtonPopup Message Control Info</b> here.</p>", FieldType = FieldTypes.TopHeadingWithCloseButtonPopupMessageControl.ToString() });
        pageData.Resources.Add(new ResourceModel { ResourceID = -91, ResourceKey = FieldTypes.VerticalTimelineControl.ToString(), ResourceValue = "VerticalTimelineControl", InfoValue = " This is timeline Vertical Control", FieldType = FieldTypes.VerticalTimelineControl.ToString() });
        pageData.Resources.Add(new ResourceModel { ResourceID = -92, ResourceKey = FieldTypes.HorizontalTimelineControl.ToString(), ResourceValue = "HorizontalTimelineControl", InfoValue = " This is timeline Horizontal Control", FieldType = FieldTypes.HorizontalTimelineControl.ToString() });
    }

    public List<OptionModel> GetListOptions()
    {
        return new List<OptionModel> {
            new OptionModel{ OptionID = 1, OptionText= FieldTypes.OneRowListViewControl.ToString() },
            new OptionModel{ OptionID = 2, OptionText= FieldTypes.TwoRowListViewControl.ToString() },
            new OptionModel{ OptionID = 3, OptionText= FieldTypes.OneRowGroupedListViewControl.ToString() },
            new OptionModel{ OptionID = 4, OptionText= FieldTypes.TwoRowGroupedListViewControl.ToString() },
            new OptionModel{ OptionID = 5, OptionText= "Search"+FieldTypes.OneRowListViewControl.ToString() },
            new OptionModel{ OptionID = 6, OptionText= "Search"+FieldTypes.TwoRowListViewControl.ToString() },
            new OptionModel{ OptionID = 7, OptionText= "Search"+FieldTypes.OneRowGroupedListViewControl.ToString() },
            new OptionModel{ OptionID = 8, OptionText= "Search"+FieldTypes.TwoRowGroupedListViewControl.ToString() },
            new OptionModel{ OptionID = 9, OptionText= "ShowMoreLabel"+FieldTypes.OneRowGroupedListViewControl.ToString() },
            new OptionModel{ OptionID = 10, OptionText= "ShowMoreIcon"+FieldTypes.OneRowGroupedListViewControl.ToString() },
            new OptionModel{ OptionID = 11, OptionText= "TwoRowFieldWithBadgeControl" },
        };
    }

    public List<OptionModel> GetCarauseloptionList()
    {
        return new List<OptionModel> {
            new OptionModel { OptionID = 1, GroupName = "https://www.w3schools.com/bootstrap/la.jpg", ParentOptionText="Lorem Ipsum 1", OptionText =  "<p>\"Neque porro quisquam est qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit...\"</p>"},
            new OptionModel { OptionID = 2, GroupName = "https://www.w3schools.com/bootstrap/chicago.jpg", ParentOptionText="Lorem Ipsum 2", OptionText =  "<p>\"Neque porro quisquam est qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit...\"</p>"},
            new OptionModel { OptionID = 3, GroupName = "https://www.w3schools.com/bootstrap/ny.jpg", ParentOptionText="Lorem Ipsum 3", OptionText = "<p>\"Nature is an important and integral part of mankind. It is one of the greatest blessings for human life; however, nowadays humans fail to recognize it as one....\"</p>"},
        };
    }

    public List<OptionModel> GetOptionsList()
    {
        return new List<OptionModel> {
            new OptionModel { OptionID = 1, GroupName = "SS", OptionText = "option 1", IsSelected = false },
            new OptionModel { OptionID = 2, GroupName = "SS", OptionText = "option 2", IsSelected = true },
            new OptionModel { OptionID = 3, GroupName = "SS", OptionText = "option 3 ", IsSelected = false },
            //new OptionModel { OptionID = 3, GroupName = "SS", OptionText = "SS option 3 If you select an item, which has text that is wider than the width of the field, in a multi-value select2 field the item extends past the edge of the field. It makes more sense for the item to either wrap or get cut short.", IsSelected = false },
            new OptionModel { OptionID = 4, GroupName = "SS", OptionText = "option 4", IsSelected = false },
            new OptionModel { OptionID = 5, GroupName = "SS", OptionText = "option 5", IsSelected = false, IsDisabled = true },
            new OptionModel { OptionID = 6, GroupName = "SS", OptionText = "option 6", IsSelected = false },
            new OptionModel { OptionID = 7, GroupName = "SS", OptionText = "option 7", IsSelected = false },
            new OptionModel { OptionID = 8, GroupName = "SS", OptionText = "option 8", IsSelected = false },
            new OptionModel { OptionID = 9, GroupName = "SS", OptionText = "option 9", IsSelected = false, IsDisabled = true },
            new OptionModel { OptionID = 10, GroupName = "SS", OptionText = "option 10", IsSelected = false },
            new OptionModel { OptionID = 11, GroupName = "SS", OptionText = "option 11", IsSelected = false },
            //new OptionModel { OptionID = 6, GroupName = "SS", OptionText = "SS option 6 You can use max-width + text-emphasis css properties. For more information you can check here:\r\n\r\n", IsSelected = false }
        };
    }

    public List<OptionModel> GetTimelineList()
    {
        return new List<OptionModel>
       {
           new OptionModel { OptionID = 1, SequenceNo=1, GroupName="NOV 2022" ,OptionText="Celebrating the official release of Radzen Blazor Studio." , ParentOptionText = AppPermissions.ForgotPasswordView.ToString()},
           new OptionModel { OptionID = 2, SequenceNo=2,GroupName="SEP 2022" ,OptionText="Celebrating the official release of Radzen Blazor Studio." , ParentOptionText = AppPermissions.RegistrationView.ToString()},
           new OptionModel { OptionID = 3, SequenceNo=3,GroupName="OCT 2022" ,OptionText="Celebrating the official release of Radzen Blazor Studio." , ParentOptionText = AppPermissions.LoginView.ToString()},
       };
    }

    public List<OptionModel> GetCountryCode()
    {
        return new List<OptionModel>
        {
            new OptionModel{OptionID = 1 , OptionText="+91" , GroupName = "India" , ParentOptionID = 10 , SequenceNo = 10 },
            new OptionModel{OptionID = 1 , OptionText="+1" , GroupName = "US" , ParentOptionID = 9 , SequenceNo = 9},
            new OptionModel{OptionID = 1 , OptionText="+888" , GroupName = "UK" , ParentOptionID = 12 , SequenceNo = 12},
            new OptionModel{OptionID = 1 , OptionText="+31" , GroupName = "Netherlands" , ParentOptionID = 10 , SequenceNo = 10},

        };
    }

    public List<OptionModel> GetOptionsListForCharts()
    {
        return new List<OptionModel> {
            new OptionModel { OptionID = -1, GroupName = "none", OptionText = "Select Graphs", IsSelected = true },
            new OptionModel { OptionID = 1, GroupName = FieldTypes.LineGraphControl.ToString(), OptionText = "Line Graph", IsSelected = false },
            new OptionModel { OptionID = 2, GroupName = FieldTypes.BarGraphControl.ToString(), OptionText = "Bar Graph", IsSelected = false }
        };
    }

    public List<OptionModel> GetOptionsListForTypeOfCharts()
    {
        return new List<OptionModel> {
            new OptionModel { OptionID = 475, GroupName = "AllFilterKey", OptionText="All", SequenceNo = -1, IsSelected = true },
            new OptionModel { OptionID = 476, GroupName = "YearlyFilterKey", OptionText="Yearly", SequenceNo = 365 },
            new OptionModel { OptionID = 477, GroupName = "QuarterlyFilterKey", OptionText="Quarterly", SequenceNo = 90 },
            new OptionModel { OptionID = 478, GroupName = "MonthFilterKey", OptionText="Month", SequenceNo = 30 },
            new OptionModel { OptionID = 479, GroupName = "WeekFilterKey", OptionText="Week", SequenceNo = 7 },
            new OptionModel { OptionID = 480, GroupName = "DayFilterKey", OptionText="Day", SequenceNo = 1 },
        };
        //return new List<OptionModel> {
        //    new OptionModel { OptionID = 4, GroupName = "S1", OptionText = "Year", IsSelected = true },
        //    new OptionModel { OptionID = 5, GroupName = "S1", OptionText = "Quarter", IsSelected = false },
        //    new OptionModel { OptionID = 6, GroupName = "S1", OptionText = "Month", IsSelected = false },
        //    new OptionModel { OptionID = 7, GroupName = "S1", OptionText = "Week", IsSelected = false },
        //    new OptionModel { OptionID = 8, GroupName = "S1", OptionText = "Daily", IsSelected = false }
        //};
    }

    public List<DemoModel> GenerateLargeDataset(int lastIndex)
    {
        lastIndex += 1;
        List<DemoModel> dataset = new List<DemoModel>();
        for (int i = lastIndex; i <= lastIndex + 10; i++)
        {
            var cardData = new DemoModel
            {
                ID = i,// Id common
                Name = $"Heart Rate-{i}", // header Name like Heart-Rate
                Image =@ImageConstants.I_DOC_UPLOAD_ICON,  // Icon
                NavIcon = @ImageConstants.I_DOC_UPLOAD_ICON,
                Description = "bpm",    // Value
                SubHeader = (80 + i).ToString(),
                Status = "Ranges 72 - 150",  // Range
            };
            dataset.Add(cardData);
        }
        return dataset;
    }
   

    public void GetGraphData1(ChartUIDTO chartData)
    {
        chartData.Lines = new List<ChartLineModel>();
        chartData.Bands = new List<ChartBandModel>();

        var days = chartData.SelectedDuration.SequenceNo;
        if (days == -1 && chartData.SelectedDuration.OptionID == ResourceConstants.R_ALL_FILTER_KEY_ID)
        {
            chartData.EndDate = null;
            chartData.StartDate = null;
        }
        else
        {
            if (chartData.EndDate == null)
            {
                chartData.EndDate = DateTime.Now;
                chartData.StartDate = DateTime.Now.AddDays(-days);
            }
            else
            {
                chartData.EndDate = chartData.EndDate.Value.AddDays(-days);
                chartData.StartDate = chartData.StartDate.Value.AddDays(-days);
            }
        }

        //chartData.Lines.Add(GetLines("First Line", "#40F723", 2, chartData.StartDate, chartData.EndDate));
        //chartData.Bands.Add(new ChartBandModel { BandName = "Normal", Color = "#D4F723", MinValue = 2.0, MaxValue = 50 });
        //chartData.Bands.Add(new ChartBandModel { BandName = "Target", Color = "#A0FAEB", MinValue = 20.0, MaxValue = 40.0 });

        //chartData.Lines.Add(GetLines("First Line", "#40F723", 15, chartData.StartDate, chartData.EndDate));
        //chartData.Bands.Add(new ChartBandModel { BandName = "Normal", Color = "#D4F723", MinValue = 15.0, MaxValue = 65 });
        //chartData.Bands.Add(new ChartBandModel { BandName = "Target", Color = "#A0FAEB", MinValue = 25.0, MaxValue = 35.0 });




        chartData.Lines = new List<ChartLineModel>
        {
            new ChartLineModel
            {
                LineColor ="#651CF9",
                LineName = "Systolic Blood Pressure",
                ChartData = new List<DataPointModel>
                {
                    new DataPointModel { DateTime = new DateTime(2023, 10, 11), Value = 40 },
                    new DataPointModel { DateTime = new DateTime(2023, 9, 13), Value = 60 },
                    new DataPointModel { DateTime = new DateTime(2023, 8, 17), Value = 40 },
                    new DataPointModel { DateTime = new DateTime(2023, 7, 19), Value = 115 },
                    new DataPointModel { DateTime = new DateTime(2023, 11, 21), Value = 100 },
                    new DataPointModel { DateTime = new DateTime(2023, 11, 23), Value = 90 },
                    new DataPointModel { DateTime = new DateTime(2023, 11, 24), Value = 130 },
                    new DataPointModel { DateTime = new DateTime(2023, 11, 26), Value = 160 },
                    new DataPointModel { DateTime = new DateTime(2023, 11, 28), Value = 15 },
                    new DataPointModel { DateTime = new DateTime(2022, 12, 30), Value = 120 },
                    new DataPointModel { DateTime = new DateTime(2023, 11, 15), Value = 140 },
                    new DataPointModel { DateTime = new DateTime(2021, 12, 1), Value = 80 },
                    new DataPointModel { DateTime = new DateTime(2023, 12, 2), Value = 70 },
                    new DataPointModel { DateTime = new DateTime(2023, 12, 9,1,0,0), Value = 140 },
                    new DataPointModel { DateTime = new DateTime(2023, 12, 3), Value = 180 },
                    new DataPointModel { DateTime = new DateTime(2023, 12, 9,4,0,0), Value = 100 },
                    new DataPointModel { DateTime = new DateTime(2023, 12, 9,8, 0, 0), Value = 140 },
                    new DataPointModel { DateTime = new DateTime(2023, 12, 9,12,0,0), Value = 140 },
                    new DataPointModel { DateTime = new DateTime(2023, 12, 9,16, 0, 0), Value = 140 },
                }
            },
            new ChartLineModel
            {
                LineColor ="#408CFA",
                LineName = "Diastolic Blood Pressure",
                ChartData = new List<DataPointModel>
                {
                    new DataPointModel { DateTime = new DateTime(2023, 10, 11), Value = 40 },
                    new DataPointModel { DateTime = new DateTime(2023, 9, 13), Value = 60 },
                    new DataPointModel { DateTime = new DateTime(2023, 8, 17), Value = 40 },
                    new DataPointModel { DateTime = new DateTime(2023, 7, 19), Value = 155 },
                    new DataPointModel { DateTime = new DateTime(2023, 11, 21), Value = 100 },
                    new DataPointModel { DateTime = new DateTime(2023, 11, 23), Value = 90 },
                    new DataPointModel { DateTime = new DateTime(2023, 11, 24), Value = 130 },
                    new DataPointModel { DateTime = new DateTime(2023, 11, 26), Value = 160 },
                    new DataPointModel { DateTime = new DateTime(2023, 11, 28), Value = 15 },
                    new DataPointModel { DateTime = new DateTime(2022, 12, 30), Value = 160 },
                    new DataPointModel { DateTime = new DateTime(2023, 11, 15), Value = 140 },
                    new DataPointModel { DateTime = new DateTime(2021, 12, 1), Value = 40 },
                    new DataPointModel { DateTime = new DateTime(2023, 12, 2), Value = 70 },
                    new DataPointModel { DateTime = new DateTime(2023, 12, 9,1,0,0), Value = 140 },
                    new DataPointModel { DateTime = new DateTime(2023, 12, 3), Value = 180 },
                    new DataPointModel { DateTime = new DateTime(2023, 12, 9,4,0,0), Value = 140 },
                    new DataPointModel { DateTime = new DateTime(2023, 12, 9,8, 0, 0), Value = 140 },
                    new DataPointModel { DateTime = new DateTime(2023, 12, 9,12,0,0), Value = 140 },
                    new DataPointModel { DateTime = new DateTime(2023, 12, 9,16, 0, 0), Value = 140 },
                }
            }
        };
        chartData.Bands = new List<ChartBandModel>
        {
            new ChartBandModel { BandName = "Systolic Blood Pressure Target", Color = "#D5F5E3", MinValue = 100, MaxValue = 120 },
            new ChartBandModel { BandName = "Diastolic Blood Pressure Target", Color = "#E8DAEF", MinValue = 60, MaxValue = 80 },
        };

        //};

        //      new ChartUIDTO
        //        {
        //           Lines = new List<ChartLineModel>
        //            {
        //                new ChartLineModel
        //                {
        //                    LineName = "India",
        //                    ChartData = new List<DataPointModel>
        //                    {
        //                        new DataPointModel { DateTime = new DateTime(2023, 1, 1), Value = 40 },
        //                        new DataPointModel { DateTime = new DateTime(2023, 2, 1), Value = 60 },
        //                        new DataPointModel { DateTime = new DateTime(2023, 3, 1), Value = 40 },
        //                        new DataPointModel { DateTime = new DateTime(2023, 4, 1), Value = 155 },
        //                        new DataPointModel { DateTime = new DateTime(2023, 5, 1), Value = 100 },
        //                        new DataPointModel { DateTime = new DateTime(2023, 6, 1), Value = 90 },
        //                        new DataPointModel { DateTime = new DateTime(2023, 7, 1), Value = 130 },
        //                        new DataPointModel { DateTime = new DateTime(2023, 8, 1), Value = 160 },
        //                        new DataPointModel { DateTime = new DateTime(2023, 9, 1), Value = 15 },
        //                        new DataPointModel { DateTime = new DateTime(2023, 10, 1), Value = 160 },
        //                        new DataPointModel { DateTime = new DateTime(2023, 11, 15), Value = 140 },
        //                        new DataPointModel { DateTime = new DateTime(2023, 12, 1), Value = 40 },
        //                    }
        //                }
        //            },
        //            Bands = new List<ChartBandModel>
        //            {
        //                new ChartBandModel { BandName = "Average", Color = "#87CEEB", MinValue = 50.0, MaxValue = 80.0 },
        //            }
        //        },

        //        //new ChartUIDTO
        //        //{
        //        //   Lines = new List<ChartLineModel>
        //        //    {
        //        //        new ChartLineModel
        //        //        {
        //        //            LineName = "USA",
        //        //            ChartData = new List<DataPointModel>
        //        //            {

        //        //                new DataPointModel { DateTime = new DateTime(2014, 1, 1), Value = 80 },
        //        //                new DataPointModel { DateTime = new DateTime(2013, 1, 1), Value = 60 },
        //        //                new DataPointModel { DateTime = new DateTime(2012, 1, 1), Value = 130 },
        //        //                new DataPointModel { DateTime = new DateTime(2011, 1, 1), Value = 15 },
        //        //                new DataPointModel { DateTime = new DateTime(2010, 1, 1), Value = 60 },
        //        //            }
        //        //        }
        //        //    },
        //        //   Bands = new List<ChartBandModel>
        //        //    {
        //        //        new ChartBandModel { BandName = "High", Color = "#FFA500", MinValue = 120.0, MaxValue = 150.0 },
        //        //    }
        //        //},

        //        // new ChartUIDTO
        //        //{
        //        //   Lines = new List<ChartLineModel>
        //        //    {
        //        //        new ChartLineModel
        //        //        {
        //        //            LineName = "Japan",
        //        //            ChartData = new List<DataPointModel>
        //        //            {

        //        //                new DataPointModel { DateTime = new DateTime(2014, 1, 1), Value = 130 },
        //        //                new DataPointModel { DateTime = new DateTime(2013, 1, 1), Value = 160 },
        //        //                new DataPointModel { DateTime = new DateTime(2012, 1, 1), Value = 30 },
        //        //                new DataPointModel { DateTime = new DateTime(2011, 1, 1), Value = 150 },
        //        //                new DataPointModel { DateTime = new DateTime(2010, 1, 1), Value = 70 },
        //        //            }
        //        //        }
        //        //    },
        //        //    Bands = null
        //        //},

        //};
        // return chartList;
    }

    private ChartLineModel GetLines(string name, string color, int start, DateTime? fromDate, DateTime? toDate)
    {
        ChartLineModel line = new() { LineName = name, LineColor = color, ChartData = new List<DataPointModel>() };
        for (int i = 0; i < 100; i++)
        {
            var datetime = fromDate.HasValue ? fromDate.Value.AddDays(i) : DateTime.Now;
            if (datetime <= toDate && datetime >= fromDate)
            {
                line.ChartData.Add(new DataPointModel { DateTime = datetime, Value = i + start });
            }
        }
        return line;
    }

    public List<OptionModel> GetOptionsListWithColor()
    {
        return new List<OptionModel> {
            new OptionModel { OptionID = 1, ParentOptionText = "#FFA07A", OptionText = "SS option 1", IsSelected = true },
            new OptionModel { OptionID = 2, ParentOptionText = "#0000FF", OptionText = "SS option 2", IsSelected = false },
            new OptionModel { OptionID = 3, ParentOptionText = "#FF1493", OptionText = "SS option 3 er9e4o ro4eoiesoirtuio4pet4jpw4u apr0i0w3i r930i W0E -3I0R04E AYHTHAIENnoifBIERGT8 HW9AU09IQei893wu9U89R34WER4 ", IsSelected = false },
            new OptionModel { OptionID = 4, ParentOptionText = "#7FFFD4", OptionText = "SS option 4", IsSelected = false },
            new OptionModel { OptionID = 5, ParentOptionText = "#9932CC", OptionText = "SS option 5", IsSelected = false },
            new OptionModel { OptionID = 6, ParentOptionText = "#DAF7A6", OptionText = "SS option 6 sjdk ifh sijfoish rosej or eosih f9ozrfohe Rgw8qieq3wo e awaoehowAJDOe dhaHWFEGHESO HFEES OHFHOEHSDOFESHFe", IsSelected = false }
        };
    }
    public List<OptionModel> GetTabList()
    {
        return new List<OptionModel>
        {
            new OptionModel { OptionID = 1,GroupName="TB" ,OptionText="Tab1" , ParentOptionText =ImageConstants.I_USER_ID_PNG},
            new OptionModel { OptionID = 2,GroupName="TB" ,OptionText="Tab2" },
            new OptionModel { OptionID = 3,GroupName="TB" ,OptionText="Tab3" , ParentOptionText = ImageConstants.I_IMAGE_ICON_PNG},
            new OptionModel { OptionID = 4,GroupName="TB" ,OptionText="Tab4" , ParentOptionText = ImageConstants.I_INFO_ICON_PNG},
            new OptionModel { OptionID = 5,GroupName="TB" ,OptionText="Tab5" , ParentOptionText = ImageConstants.I_PASSWORD_ICON_PNG},
            new OptionModel { OptionID = 6,GroupName="TB" ,OptionText="Tab6" },
            new OptionModel { OptionID = 7,GroupName="TB" ,OptionText="Tab7"},
            new OptionModel { OptionID = 8,GroupName="TB" ,OptionText="Tab8" , ParentOptionText = ImageConstants.I_USER_ID_PNG},
        };
    }

    public List<OptionModel> GetCalanderData()
    {
        var currentDate = DateTime.Now;
        var CustomeEvents = new List<OptionModel>
        {
        new OptionModel
        {
        OptionID = 1,
        From = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 13, 0, 0),
        To = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 15, 0, 0),
        OptionText = "Google Meeting",
        ParentOptionText = "#ff0000",
        },
        new OptionModel
        {
        OptionID = 2,
        From = new DateTime(DateTime.Now.AddDays(2).Year,DateTime.Now.AddDays(2).Month,DateTime.Now.AddDays(2).Day,14,0,0),
        To = new DateTime(DateTime.Now.AddDays(2).Year,DateTime.Now.AddDays(2).Month,DateTime.Now.AddDays(2).Day,16,0,0),
        OptionText = "Project Updates",
      //Background = Colors.Red,
       },
       new OptionModel
       {
      OptionID = 3,
      From = new DateTime(DateTime.Now.AddDays(3).Year,DateTime.Now.AddDays(3).Month,DateTime.Now.AddDays(3).Day,16,0,0),
      To = new DateTime(DateTime.Now.AddDays(3).Year,DateTime.Now.AddDays(3).Month,DateTime.Now.AddDays(3).Day,17,0,0),
      OptionText = "Building The Future",
     // Background = Colors.Purple,
      },
      new OptionModel
      {
      OptionID = 4,
      From = new DateTime(DateTime.Now.AddDays(4).Year,DateTime.Now.AddDays(4).Month,DateTime.Now.AddDays(4).Day,17,0,0),
      To = new DateTime(DateTime.Now.AddDays(4).Year,DateTime.Now.AddDays(4).Month,DateTime.Now.AddDays(4).Day,18,0,0),
      OptionText = "Unity Update",
     },
       new OptionModel
        {
        OptionID = 5,
        From = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 10, 0, 0),
        To = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 11, 0, 0),
        OptionText = "zoom Meeting",
        },
         new OptionModel
        {
        OptionID = 6,
        From = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 4, 0, 0),
        To = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 5, 0, 0),
        OptionText = "Project kickoffs",
        },
         new OptionModel
       {
       OptionID = 7,
       From = new DateTime(DateTime.Now.AddDays(4).Year,DateTime.Now.AddDays(4).Month,DateTime.Now.AddDays(4).Day,17,0,0),
       To = new DateTime(DateTime.Now.AddDays(4).Year,DateTime.Now.AddDays(4).Month,DateTime.Now.AddDays(4).Day,18,0,0),
       OptionText = "Unity Update",
       },
       new OptionModel
       {
       OptionID = 8,
       From = new DateTime(2024, 1, 22, 17, 0, 0),
       To = new DateTime(2024, 1, 22, 18, 0, 0),
       OptionText = "xiomy Update",
        },
        new OptionModel
       {
       OptionID = 9,
       From = new DateTime(2024, 2, 22, 15, 0, 0),
       To = new DateTime(2024, 2, 22, 16, 0, 0),
       OptionText = "Restructuring Project Goals",
        },
        new OptionModel
       {
       OptionID = 10,
       From = new DateTime(2024, 2, 23, 16, 0, 0),
       To = new DateTime(2024, 2, 23, 17, 0, 0),
       OptionText = "Backroom Thinkers",
        },
        new OptionModel
       {
       OptionID = 11,
       From = new DateTime(2024, 1, 25, 17, 0, 0),
       To = new DateTime(2024, 1, 25, 18, 0, 0),
       OptionText = "Friends Talking",
        },
        new OptionModel
       {
       OptionID = 12,
       From = new DateTime(2024, 2, 22, 17, 0, 0),
       To = new DateTime(2024, 2, 22, 18, 0, 0),
       OptionText = "Backroom Thinkers",
        },
        new OptionModel
       {
       OptionID = 13,
       From = new DateTime(2024, 1, 12, 17, 0, 0),
       To = new DateTime(2024, 1, 12, 18, 0, 0),
       OptionText = "Restructuring Project Goals",
        },
        new OptionModel
       {
       OptionID = 14,
       From = new DateTime(2024, 2, 2, 17, 0, 0),
       To = new DateTime(2024, 2, 2, 18, 0, 0),
       OptionText = "one-on-ones",
        },
        new OptionModel
       {
       OptionID = 15,
       From = new DateTime(2024, 3, 22, 17, 0, 0),
       To = new DateTime(2024, 3, 22, 18, 0, 0),
       OptionText = "Backroom Thinkers",
        },
        new OptionModel
       {
       OptionID = 16,
       From = new DateTime(2025, 1, 22, 17, 0, 0),
       To = new DateTime(2025, 1, 22, 18, 0, 0),
       OptionText = "Weekly Strategy Planning for Client Project",

        },
         new OptionModel
       {
       OptionID = 17,
       From = new DateTime(2023, 11, 23, 17, 0, 0),
       To = new DateTime(2023, 11, 23, 18, 0, 0),
       OptionText = " Client Project",

        },
           new OptionModel
       {
       OptionID = 18,
       From = new DateTime(2023, 11, 24, 17, 0, 0),
       To = new DateTime(2023, 11, 24, 18, 0, 0),
       OptionText = " Strategy Planning for Client Project",
        },
         new OptionModel
       {
       OptionID = 18,
       From = new DateTime(2023, 11, 23, 15, 0, 0),
       To = new DateTime(2023, 11, 23, 16, 0, 0),
       OptionText = "Project",
        },
        new OptionModel
       {
       OptionID = 19,
       From = new DateTime(2023, 11, 22, 15, 0, 0),
       To = new DateTime(2023, 11, 22, 16, 0, 0),
       OptionText = "Planning for Client Project",
        },
       new OptionModel
       {
       OptionID = 20,
       From = new DateTime(2023, 11, 22, 17, 0, 0),
       To = new DateTime(2023, 11, 22, 18, 0, 0),
       OptionText = " Client Project",
        },
       new OptionModel
       {
       OptionID = 21,
       From = new DateTime(2023, 11, 28, 17, 0, 0),
       To = new DateTime(2023, 11, 28, 18, 0, 0),
       OptionText = "Planning for Client Project",
        },
       new OptionModel
       {
       OptionID = 22,
       From = new DateTime(2023, 10, 2, 17, 0, 0),
       To = new DateTime(2023, 10, 2, 18, 0, 0),
       OptionText = "weekly Catch-Up",
        },
         new OptionModel
       {
       OptionID = 23,
       From = new DateTime(2024, 1, 1, 17, 0, 0),
       To = new DateTime(2024, 1, 1, 18, 0, 0),
       OptionText = "xiomy Update",
        },
           new OptionModel
       {
       OptionID = 24,
       From = new DateTime(2024, 1, 1, 15, 0, 0),
       To = new DateTime(2024, 1, 1, 16, 0, 0),
       OptionText = "zoom Update",
        },
        };
        return CustomeEvents;
    }

    public List<LanguageModel> GetLanguageTabList()
    {
        return new List<LanguageModel> {
        new LanguageModel { LanguageID = 1, LanguageName = "English" },
        new LanguageModel { LanguageID = 2, LanguageName = "Hindi" },
        new LanguageModel { LanguageID = 3,LanguageName = "Marathi"},
    };
    }

}