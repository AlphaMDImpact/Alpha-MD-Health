namespace AlphaMDHealth.Utility;

/// <summary>
/// Type of Control 
/// </summary>
public enum FieldTypes
{
    /// <summary>
    /// if no control type is specified
    /// </summary>
    Default = 0,

    #region Entry (index from 1 to 14)

    /// <summary>
    /// ControlType is "TextEntryControl"
    /// </summary>
    TextEntryControl = 1,

    /// <summary>
    /// ControlType is "NumericTextEntryControl"
    /// </summary>
    NumericTextEntryControl = 2,

    /// <summary>
    /// ControlType is "AlphaEntryControl"
    /// </summary>
    AlphaEntryControl = 3,

    /// <summary>
    /// ControlType is "AlphaNumericEntryControl"
    /// </summary>
    AlphaNumericEntryControl = 4,

    /// <summary>
    /// ControlType is "EmailEntryControl"
    /// </summary>
    EmailEntryControl = 5,

    /// <summary>
    /// ControlType is "PasswordControl"
    /// </summary>
    PasswordEntryControl = 6,

    /// <summary>
    /// ControlType is "NumericControl"
    /// </summary>
    NumericEntryControl = 7,

    /// <summary>
    /// ControlType is "DecimalControl"
    /// </summary>
    DecimalEntryControl = 8,

    /// <summary>
    /// ControlType is "CounterEntryControl"
    /// </summary>
    CounterEntryControl = 9,

    /// <summary>
    /// Mobile Number Control
    /// </summary>
    MobileNumberControl = 10,

    #endregion

    #region DateTime control (index from 15 to 20)

    /// <summary>
    /// ControlType is "DateControl"
    /// </summary>
    DateControl = 15,

    /// <summary>
    /// ControlType is "TimeControl"
    /// </summary>
    TimeControl = 16,

    /// <summary>
    /// ControlType is "DateTimeControl"
    /// </summary>
    DateTimeControl = 17,

    #endregion

    #region Checkbox control (index from 21 to 30)

    /// <summary>
    /// ControlType is "VerticalCheckBoxControl"
    /// </summary>
    VerticalCheckBoxControl = 21,

    /// <summary>
    /// ControlType is "ColorBoxCheckBoxControl"
    /// </summary>
    ColorBoxCheckBoxControl = 22,

    /// <summary>
    /// InRow CheckBox
    /// </summary>
    HorizontalCheckBoxControl = 23,

    /// <summary>
    /// single CheckBox
    /// </summary>
    CheckBoxControl = 24,

    #endregion

    #region RadioButton control (index from 31 to 40)

    /// <summary>
    /// ControlType is "VerticalRadioButtonControl"
    /// </summary>
    VerticalRadioButtonControl = 31,

    /// <summary>
    /// ControlType is "HorizontalRadioButtonControl"
    /// </summary>
    HorizontalRadioButtonControl = 32,

    #endregion

    #region DropdownControl (index from 41 to 49)

    /// <summary>
    /// ControlType is "SingleSelectDropdownControl"
    /// </summary>
    SingleSelectDropdownControl = 41,

    /// <summary>
    /// ControlType is "MultiSelectDropdownControl"
    /// </summary>
    MultiSelectDropdownControl = 42,

    /// <summary>
    /// ControlType is "SingleSelectEditableDropdownControl"
    /// </summary>
    SingleSelectEditableDropdownControl = 43,

    /// <summary>
    /// ControlType is "MultiSelectEditableDropdownControl"
    /// </summary>
    MultiSelectEditableDropdownControl = 44,

    /// <summary>
    /// ControlType is "SingleSelectWithoutBorderDropDownControl"
    /// </summary>
    SingleSelectWithoutBorderDropDownControl = 45,

    #endregion

    #region Button (index from 51 to 79)

    /// <summary>
    /// Transparent Button 
    /// </summary>
    TransparentButtonControl = 53,
    LeftIconButtonControl = 55,
    RightIconButtonControl = 56,
    DefaultSlotIconButtonControl = 57,


    /// <summary>
    /// with full length set as per page
    /// </summary>
    PrimaryButtonControl = 50,
    SecondaryButtonControl = 51,
    TertiaryButtonControl = 52,
    DeleteButtonControl = 54,

    /// <summary>
    /// transparent Button , above are regular buttons same as mobile
    /// </summary>
    PrimaryTransparentButtonControl = 58,
    SecondaryTransparentButtonControl = 59,
    TertiaryTransparentButtonControl = 60,
    DeleteTransparentButtonControl = 61,

    /// <summary>
    /// transparent with border Button 
    /// </summary>
    PrimaryBorderTransparentButtonControl = 62,
    SecondaryBorderTransparentButtonControl = 63,
    TertiaryBorderTransparentButtonControl = 64,
    DeleteBorderTransparentButtonControl = 65,


    /// <summary>
    /// same as above only only required to Expand as per text
    /// </summary>
    PrimaryExButtonControl = 66,
    SecondaryExButtonControl = 67,
    TertiaryExButtonControl = 68,
    DeleteExButtonControl = 69,
    PrimaryTransparentExButtonControl = 70,
    SecondaryTransparentExButtonControl = 71,
    TertiaryTransparentExButtonControl = 72,
    DeleteTransparentExButtonControl = 73,
    PrimaryBorderTransparentExButtonControl = 74,
    SecondaryBorderTransparentExButtonControl = 75,
    TertiaryBorderTransparentExButtonControl = 77,
    DeleteBorderTransparentExButtonControl = 78,

    MenuButtonControl = 79,

    #endregion

    #region Image (index from 80 to 89)

    /// <summary>
    /// Image Control
    /// </summary>
    ImageControl = 80,

    /// <summary>
    /// In circle image Control
    /// </summary>
    CircleImageControl = 81,

    /// <summary>
    /// In square image control
    /// </summary>
    SquareImageControl = 82,

    /// <summary>
    /// In circle image with border Control
    /// </summary>
    CircleWithBorderImageControl = 83,

    /// <summary>
    /// In square image with border Control
    /// </summary>
    SquareWithBorderImageControl = 84,

    /// <summary>
    /// In circle image with backgroud Control
    /// </summary>
    CircleWithBackgroundImageControl = 85,

    /// <summary>
    /// In square image with backgroud Control
    /// </summary>
    SquareWithBackgroundImageControl = 86,

    /// <summary>
    /// In circle image with border and backgroud Control
    /// </summary>
    CircleWithBorderAndBackgroundImageControl = 87,

    /// <summary>
    /// In square image with border and backgroud Control
    /// </summary>
    SquareWithBorderAndBackgroundImageControl = 88,

    #endregion

    #region UploadControl(index from 91 to 99)

    /// <summary>
    /// ControlType is "UploadControl"
    /// </summary>
    UploadControl = 91,

    #endregion

    #region Slider control (index from 100 to 110)

    /// <summary>
    /// ControlType is "VerticalSliderControl"
    /// </summary>
    VerticalSliderControl = 100,

    /// <summary>
    /// ControlType is "HorizontalSliderControl"
    /// </summary>
    HorizontalSliderControl = 101,

    #endregion

    #region ProgressBar control(index from 111 to 120) 

    /// <summary>
    /// 
    /// </summary>
    ProgressBarControl = 111,

    /// <summary>
    /// 
    /// </summary>
    ProgressIndicatorControl = 112,

    #endregion

    #region GraphControl (index from 121 to 130)

    LineGraphControl = 482,
    BarGraphControl = 483,

    #endregion

    #region ListViewControl (index from 131 to 150)

    /// <summary>
    /// OneRowListViewControl
    /// </summary>
    OneRowListViewControl = 131,

    /// <summary>
    /// TwoRowListViewControl
    /// </summary>
    TwoRowListViewControl = 132,

    /// <summary>
    /// OneRowGroupedListViewControl
    /// </summary>
    OneRowGroupedListViewControl = 133,

    /// <summary>
    /// TwoRowGroupedListViewControl
    /// </summary>
    TwoRowGroupedListViewControl = 134,

    #endregion

    #region CarauselControl (index from 151 to 160)

    CarouselControl = 151,

    FullOverlayCarouselControl = 152,

    HalfOverlayCarouselControl = 153,

    CardsControl = 156,

    #endregion

    #region Webview control (index from 161 to 170)

    //text, pdf, word, image, html, excel, video, youtube video

    /// <summary>
    /// ControlType is "MultiSelectEditableDropdownControl"
    /// </summary>
    HtmlWebviewControl = 161,

    /// <summary>
    /// ControlType is "MultiSelectEditableDropdownControl"
    /// </summary>
    TextWebviewControl = 162,

    /// <summary>
    /// ControlType is "MultiSelectEditableDropdownControl"
    /// </summary>
    PdfWebviewControl = 163,

    /// <summary>
    /// ControlType is "MultiSelectEditableDropdownControl"
    /// </summary>
    UrlWebviewControl = 164,

    /// <summary>
    /// ControlType is "MultiSelectEditableDropdownControl"
    /// </summary>
    YoutubeUrlWebviewControl = 165,

    /// <summary>
    /// ControlType is "MultiSelectEditableDropdownControl"
    /// </summary>
    ImageWebviewControl = 166,

    /// <summary>
    /// ControlType is "UrlPageWebViewControl"
    /// </summary>
    UrlPageWebViewControl = 167,

    #endregion

    #region Other Controls(index from 181 to 199)

    /// <summary>
    /// ControlType is "TabControl"
    /// </summary>
    TabControl = 181,

    /// <summary>
    /// ControlType is "RichTextControl"
    /// </summary>
    RichTextControl = 182,

    /// <summary>
    /// ControlType is "PinCodeControl"
    /// </summary>
    PinCodeControl = 183,

    /// <summary>
    /// Color Picker 
    /// </summary>
    ColorPickerControl = 184,

    /// <summary>
    /// Badge Count Control
    /// </summary>
    BadgeCountControl = 185,

    SwitchControl = 186,

    CalendarControl = 187,



    MultiLineEntryControl = 189,

    /// <summary>
    /// default badge control
    /// </summary>
    BadgeControl = 190,

    /// <summary>
    /// ControlType for PrimaryBadgeControl
    /// </summary>
    PrimaryBadgeControl = 191,

    /// <summary>
    /// ControlType for SecondaryBadgeControl
    /// </summary>
    SecondaryBadgeControl = 192,

    /// <summary>
    /// ControlType for LightBadgeControl
    /// </summary>
    LightBadgeControl = 193,

    /// <summary>
    /// ControlType for DarkBadgeControl
    /// </summary>
    DarkBadgeControl = 194,

    /// <summary>
    /// ControlType for SuccessBadgeControl
    /// </summary>
    SuccessBadgeControl = 195,

    /// <summary>
    /// ControlType for DangerBadgeControl
    /// </summary>
    DangerBadgeControl = 196,

    /// <summary>
    /// ControlType for WarningBadgeControl
    /// </summary>
    WarningBadgeControl = 197,

    /// <summary>
    /// ControlType for InfoBadgeControl
    /// </summary>
    InfoBadgeControl = 198,

    /// <summary>
    /// ControlType for MessageControl with heading below icon without close button
    /// </summary>
    MessageControl = 199,

    /// <summary>
    /// ControlType for MessageControl with heading on top above icon without close button
    /// </summary>
    TopHeadingMessageControl = 200,

    /// <summary>
    /// ControlType for MessageControl with heading below icon with close button on top
    /// </summary>
    CloseButtonMessageControl = 201,

    /// <summary>
    /// ControlType for MessageControl with heading on top above icon with close button on top
    /// </summary>
    TopHeadingWithCloseButtonMessageControl = 202,

    /// <summary>
    /// ControlType for PopupMessageControl with heading below icon without close button
    /// </summary>
    PopupMessageControl = 203,

    /// <summary>
    /// ControlType for PopupMessageControl with heading on top above icon without close button
    /// </summary>
    TopHeadingPopupMessageControl = 204,

    /// <summary>
    /// ControlType for PopupMessageControl with heading below icon with close button on top
    /// </summary>
    CloseButtonPopupMessageControl = 205,

    /// <summary>
    /// ControlType for PopupMessageControl with heading on top above icon with close button on top
    /// </summary>
    TopHeadingWithCloseButtonPopupMessageControl = 206,

    /// <summary>
    /// ControlType for TimelineControl 
    /// </summary>
    VerticalTimelineControl = 207,

    /// <summary>
    /// ControlType for TimelineControl 
    /// </summary>
    HorizontalTimelineControl = 208,



    #endregion

    #region Label (index from 211) 

    #region LightLabelControl

    LightLargeHVCenterLabelControl = 490,
    LightLargeHStartVCenterLabelControl = 491,
    LightLargeHEndVCenterLabelControl = 492,
    LightLargeHVCenterBoldLabelControl = 493,
    LightLargeHStartVCenterBoldLabelControl = 494,
    LightLargeHEndVCenterBoldLabelControl = 495,

    LightMediumHVCenterLabelControl = 681,
    LightMediumHStartVCenterLabelControl = 682,
    LightMediumHEndVCenterLabelControl = 683,
    LightMediumHVCenterBoldLabelControl = 684,
    LightMediumHStartVCenterBoldLabelControl = 685,
    LightMediumHEndVCenterBoldLabelControl = 686,

    LightSmallHVCenterLabelControl = 700,
    LightSmallHStartVCenterLabelControl = 701,
    LightSmallHEndVCenterLabelControl = 702,
    LightSmallHVCenterBoldLabelControl = 703,
    LightSmallHStartVCenterBoldLabelControl = 704,
    LightSmallHEndVCenterBoldLabelControl = 705,

    LightMicroHVCenterLabelControl = 719,
    LightMicroHStartVCenterLabelControl = 720,
    LightMicroHEndVCenterLabelControl = 721,
    LightMicroHVCenterBoldLabelControl = 722,
    LightMicroHStartVCenterBoldLabelControl = 723,
    LightMicroHEndVCenterBoldLabelControl = 724,

    #endregion

    #region PrimaryAppLabelControl

    PrimaryAppLargeHVCenterLabelControl = 382,
    PrimaryAppLargeHStartVCenterLabelControl = 383,
    PrimaryAppLargeHEndVCenterLabelControl = 384,
    PrimaryAppLargeHVCenterBoldLabelControl = 385,
    PrimaryAppLargeHStartVCenterBoldLabelControl = 386,
    PrimaryAppLargeHEndVCenterBoldLabelControl = 387,

    PrimaryAppMediumHVCenterLabelControl = 389,
    PrimaryAppMediumHStartVCenterLabelControl = 390,
    PrimaryAppMediumHEndVCenterLabelControl = 391,
    PrimaryAppMediumHVCenterBoldLabelControl = 392,
    PrimaryAppMediumHStartVCenterBoldLabelControl = 393,
    PrimaryAppMediumHEndVCenterBoldLabelControl = 394,

    PrimaryAppSmallHVCenterLabelControl = 396,
    PrimaryAppSmallHStartVCenterLabelControl = 397,
    PrimaryAppSmallHEndVCenterLabelControl = 398,
    PrimaryAppSmallHVCenterBoldLabelControl = 399,
    PrimaryAppSmallHStartVCenterBoldLabelControl = 400,
    PrimaryAppSmallHEndVCenterBoldLabelControl = 401,

    PrimaryAppMicroHVCenterLabelControl = 795,
    PrimaryAppMicroHStartVCenterLabelControl = 798,
    PrimaryAppMicroHEndVCenterLabelControl = 799,
    PrimaryAppMicroHVCenterBoldLabelControl = 805,
    PrimaryAppMicroHStartVCenterBoldLabelControl = 806,
    PrimaryAppMicroHEndVCenterBoldLabelControl = 808,

    #endregion

    #region PrimaryLabelControl

    PrimaryLargeHVCenterLabelControl = 214,
    PrimaryLargeHStartVCenterLabelControl = 215,
    PrimaryLargeHEndVCenterLabelControl = 216,
    PrimaryLargeHVCenterBoldLabelControl = 217,
    PrimaryLargeHStartVCenterBoldLabelControl = 218,
    PrimaryLargeHEndVCenterBoldLabelControl = 219,

    PrimaryMediumHVCenterLabelControl = 221,
    PrimaryMediumHStartVCenterLabelControl = 222,
    PrimaryMediumHEndVCenterLabelControl = 223,
    PrimaryMediumHVCenterBoldLabelControl = 224,
    PrimaryMediumHStartVCenterBoldLabelControl = 225,
    PrimaryMediumHEndVCenterBoldLabelControl = 226,

    PrimarySmallHVCenterLabelControl = 228,
    PrimarySmallHStartVCenterLabelControl = 229,
    PrimarySmallHEndVCenterLabelControl = 230,
    PrimarySmallHVCenterBoldLabelControl = 231,
    PrimarySmallHStartVCenterBoldLabelControl = 232,
    PrimarySmallHEndVCenterBoldLabelControl = 233,

    PrimaryMicroHVCenterLabelControl = 1017,
    PrimaryMicroHStartVCenterLabelControl = 1020,
    PrimaryMicroHEndVCenterLabelControl = 1021,
    PrimaryMicroHVCenterBoldLabelControl = 1027,
    PrimaryMicroHStartVCenterBoldLabelControl = 1028,
    PrimaryMicroHEndVCenterBoldLabelControl = 1030,

    #endregion

    #region SecondaryLabelControl

    SecondaryLargeHVCenterLabelControl = 270,
    SecondaryLargeHStartVCenterLabelControl = 271,
    SecondaryLargeHEndVCenterLabelControl = 272,
    SecondaryLargeHVCenterBoldLabelControl = 273,
    SecondaryLargeHStartVCenterBoldLabelControl = 274,
    SecondaryLargeHEndVCenterBoldLabelControl = 275,

    SecondaryMediumHVCenterLabelControl = 277,
    SecondaryMediumHStartVCenterLabelControl = 278,
    SecondaryMediumHEndVCenterLabelControl = 279,
    SecondaryMediumHVCenterBoldLabelControl = 280,
    SecondaryMediumHStartVCenterBoldLabelControl = 281,
    SecondaryMediumHEndVCenterBoldLabelControl = 282,

    SecondarySmallHVCenterLabelControl = 284,
    SecondarySmallHStartVCenterLabelControl = 285,
    SecondarySmallHEndVCenterLabelControl = 286,
    SecondarySmallHVCenterBoldLabelControl = 287,
    SecondarySmallHStartVCenterBoldLabelControl = 288,
    SecondarySmallHEndVCenterBoldLabelControl = 289,

    SecondaryMicroHVCenterLabelControl = 943,
    SecondaryMicroHStartVCenterLabelControl = 946,
    SecondaryMicroHEndVCenterLabelControl = 947,
    SecondaryMicroHVCenterBoldLabelControl = 953,
    SecondaryMicroHStartVCenterBoldLabelControl = 954,
    SecondaryMicroHEndVCenterBoldLabelControl = 956,

    #endregion

    #region TertiaryLabelControl

    TertiaryLargeHVCenterLabelControl = 326,
    TertiaryLargeHStartVCenterLabelControl = 327,
    TertiaryLargeHEndVCenterLabelControl = 328,
    TertiaryLargeHVCenterBoldLabelControl = 329,
    TertiaryLargeHStartVCenterBoldLabelControl = 330,
    TertiaryLargeHEndVCenterBoldLabelControl = 331,

    TertiaryMediumHVCenterLabelControl = 333,
    TertiaryMediumHStartVCenterLabelControl = 334,
    TertiaryMediumHEndVCenterLabelControl = 335,
    TertiaryMediumHVCenterBoldLabelControl = 336,
    TertiaryMediumHStartVCenterBoldLabelControl = 337,
    TertiaryMediumHEndVCenterBoldLabelControl = 338,

    TertiarySmallHVCenterLabelControl = 340,
    TertiarySmallHStartVCenterLabelControl = 341,
    TertiarySmallHEndVCenterLabelControl = 342,
    TertiarySmallHVCenterBoldLabelControl = 343,
    TertiarySmallHStartVCenterBoldLabelControl = 344,
    TertiarySmallHEndVCenterBoldLabelControl = 345,

    TertiaryMicroHVCenterLabelControl = 869,
    TertiaryMicroHStartVCenterLabelControl = 872,
    TertiaryMicroHEndVCenterLabelControl = 873,
    TertiaryMicroHVCenterBoldLabelControl = 879,
    TertiaryMicroHStartVCenterBoldLabelControl = 880,
    TertiaryMicroHEndVCenterBoldLabelControl = 882,

    #endregion

    #region LinkLabelControl

    LinkHVCenterLabelControl = 457,
    LinkHStartVCenterLabelControl = 458,
    LinkHEndVCenterLabelControl = 459,
    LinkHVCenterBoldLabelControl = 460,
    LinkHStartVCenterBoldLabelControl = 461,
    LinkHEndVCenterBoldLabelControl = 462,
    LinkErrorHEndVCenterLabelControl = 469,
    LinkHStartVCenterBoldUnderlineLabelControl = 470,

    #endregion

    #region ErrorLabelControl

    ErrorHVCenterLabelControl = 463,
    ErrorHStartVCenterLabelControl = 464,
    ErrorHEndVCenterLabelControl = 465,
    ErrorHVCenterBoldLabelControl = 466,
    ErrorHStartVCenterBoldLabelControl = 467,
    ErrorHEndVCenterBoldLabelControl = 468,

    #endregion

    #region HtmlLabelControl

    /// <summary>
    /// Html Primary LabelControl
    /// </summary>
    HtmlPrimaryLabelControl = 451,

    /// <summary>
    /// Html Primary Center LabelControl
    /// </summary>
    HtmlPrimaryCenterLabelControl = 452,

    /// <summary>
    /// Html Secondary LabelControl
    /// </summary>
    HtmlSecondaryLabelControl = 453,

    /// <summary>
    /// Html Secondary Center LabelControl
    /// </summary>
    HtmlSecondaryCenterLabelControl = 454,

    /// <summary>
    /// Html Tertiary LabelControl
    /// </summary>
    HtmlTertiaryLabelControl = 455,

    /// <summary>
    /// Html Tertiary Center LabelControl
    /// </summary>
    HtmlTertiaryCenterLabelControl = 456,

    HtmlLightLabelControl = 1033,
    HtmlLightCenterLabelControl = 1034,

    #endregion

    #endregion

    #region Popup Page

    DialogPage = 1035,
    SideLeftDialog = 1036,
    SideRightDialog = 1037,
    SideBottomDialog = 1038,
    SideTopDialog = 1039

    #endregion
}