using AlphaMDHealth.Utility;
using SwipeItem = DevExpress.Maui.CollectionView.SwipeItem;
namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateOtherStyle()
    {
        //var gradientBackgroundStyle = new Style(typeof(CustomGradient))
        //{
        //    Setters =
        //    {
        //        new Setter{Property = CustomGradient.StartColorProperty, Value = _genericBackgroundColor },//GradientColor},
        //        new Setter{Property = CustomGradient.EndColorProperty, Value = _genericBackgroundColor }, //QuaternaryBackgroundColor
        //        new Setter{Property = CustomGradient.HorizontalOptionsProperty,Value=LayoutOptions.FillAndExpand},
        //        new Setter{Property = CustomGradient.FlowDirectionProperty , Value = _appFlowDirection}
        //    }
        //};
        //var defaultToggleStyle = new Style(typeof(Switch))
        //{
        //    Setters =
        //    {
        //        new Setter { Property = Switch.ThumbColorProperty, Value = _genericBackgroundColor},
        //        new Setter { Property = SwitchColorEffect.ThumbColorProperty, Value = _genericBackgroundColor},
        //        new Setter { Property = SwitchColorEffect.TrueColorProperty, Value =  _successColor},
        //        new Setter { Property = SwitchColorEffect.FalseColorProperty, Value = _errorColor},
        //        new Setter { Property = Switch.HorizontalOptionsProperty, Value = LayoutOptions.End },
        //        new Setter { Property = Switch.VerticalOptionsProperty, Value = LayoutOptions.Center },
        //        new Setter { Property = Switch.FlowDirectionProperty , Value = _appFlowDirection}
        //    }
        //};
        //Style searchBarStyle = new(typeof(CustomSearchBar))
        //{
        //    Setters =
        //    {
        //        new Setter { Property = CustomSearchBar.TextColorProperty, Value = _primaryTextColor },
        //        new Setter { Property = CustomSearchBar.PlaceholderColorProperty, Value = _tertiaryTextColor },
        //        new Setter { Property = CustomSearchBar.BackgroundColorProperty, Value = _genericBackgroundColor },
        //        new Setter { Property = CustomSearchBar.HeightRequestProperty, Value = GenericMethods.GetPlatformSpecificValue(40,37,0) },
        //        new Setter { Property = CustomSearchBar.FlowDirectionProperty , Value = _appFlowDirection },
        //        new Setter { Property = CustomSearchBar.BorderTypeProperty, Value = string.Empty },
        //        new Setter { Property = CustomSearchBar.BorderFocusColorProperty, Value = _primaryAppColor },
        //        new Setter { Property = CustomSearchBar.BorderUnFocusColorProperty, Value = _separatorAndDisableColor },
        //        new Setter { Property = CustomSearchBar.HorizontalOptionsProperty,Value=LayoutOptions.FillAndExpand },
        //        new Setter { Property = CustomSearchBar.HorizontalTextAlignmentProperty,Value=TextAlignment.Start },
        //        new Setter { Property = CustomSearchBar.FontSizeProperty,Value=Device.GetNamedSize(NamedSize.Small, typeof(CustomSearchBar)) }
        //    }
        //};

        Style endSwipeStyle = new(typeof(SwipeItem))
        {
            Setters=
            {
                new Setter {Property = SwipeItem.BackgroundColorProperty , Value = _errorColor },
                new Setter {Property = SwipeItem.ImageProperty , Value = ImageSource.FromFile(ImageConstants.I_CHAT_DELETE_ICON_PNG) },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_END_SWIPE_KEY, endSwipeStyle);

        //todo
        //Style yearCalendarStyle = new(typeof(Calendar))
        //{
        //    Setters =
        //    {
        //        new Setter { Property = Calendar.ShowYearPickerProperty,  Value = false },
        //        new Setter { Property = Calendar.DaysTitleColorProperty,  Value = _primaryTextColor },
        //        new Setter { Property = Calendar.ShowMonthPickerProperty,  Value = false },
        //        new Setter { Property = Calendar.DisableSwipeDetectionProperty,  Value = false },
        //        new Setter { Property = Calendar.SwipeToChangeMonthEnabledProperty,  Value = false },
        //        new Setter { Property = Calendar.FooterSectionVisibleProperty,  Value = false },
        //        new Setter { Property = Calendar.DayViewSizeProperty,  Value = LibGenericMethods.GetPlatformSpecificValue(18, 15, 0) },
        //        new Setter { Property = Calendar.DeselectedDayTextColorProperty,  Value = _primaryTextColor },
        //        new Setter { Property = Calendar.FlowDirectionProperty , Value = _AppFlowDirection }
        //    }
        //};
        //todo
        //Style monthCalendarStyle = new(typeof(Calendar))
        //{
        //    Setters =
        //    {
        //        new Setter { Property = Calendar.ShowYearPickerProperty,  Value = false },
        //        new Setter { Property = Calendar.DaysTitleColorProperty,  Value = _primaryTextColor },
        //        new Setter { Property = Calendar.ShowMonthPickerProperty,  Value = false },
        //        new Setter { Property = Calendar.SwipeToChangeMonthEnabledProperty,  Value = true },
        //        new Setter { Property = Calendar.FooterSectionVisibleProperty,  Value = false },
        //        new Setter { Property = Calendar.DayViewSizeProperty,  Value = App._essentials.GetPreferenceValue(LibStorageConstants.PR_SCREEN_HEIGHT_KEY, 0.0) / 9 },
        //        new Setter { Property = Calendar.DeselectedDayTextColorProperty,  Value = _primaryTextColor },
        //        new Setter { Property = Calendar.PaddingProperty,  Value = new Thickness(15, 0, 15, 15) },
        //        new Setter { Property = Calendar.FlowDirectionProperty , Value = _AppFlowDirection }
        //    }
        //};

        Style customPopupGridStyle = new(typeof(Grid))
        {
            Setters =
            {
                new Setter { Property = Grid.HorizontalOptionsProperty, Value = LayoutOptions.Center },
                new Setter { Property = Grid.VerticalOptionsProperty,Value = LayoutOptions.Center },
                new Setter { Property = Grid.BackgroundColorProperty, Value = _genericBackgroundColor },
                new Setter { Property = Grid.WidthRequestProperty, Value = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, 0.0) * 0.8 },
                new Setter { Property = Grid.PaddingProperty, Value = _appComponentPadding },
                new Setter { Property = Grid.ColumnSpacingProperty, Value  = _appComponentPadding / 4 },
                new Setter { Property = Grid.RowSpacingProperty, Value  = Constants.ZERO_PADDING },
                new Setter { Property = Grid.FlowDirectionProperty , Value = _appFlowDirection }
            }
        };
        CreateMauiLayoutStyles();

        Style shellMasterPageStyle = new(typeof(Shell))
        {
            Setters =
            {
                new Setter { Property = Shell.BackgroundColorProperty, Value = _genericBackgroundColor },
                new Setter { Property = Shell.TitleColorProperty, Value = _primaryTextColor },
                new Setter { Property = Shell.TabBarBackgroundColorProperty, Value = _genericBackgroundColor },
                new Setter { Property = Shell.TabBarForegroundColorProperty, Value = _primaryTextColor },
                new Setter { Property = Shell.TabBarTitleColorProperty, Value =_primaryAppColor },
                new Setter { Property = Shell.TabBarUnselectedColorProperty, Value = _tertiaryTextColor },
                new Setter { Property = Shell.DisabledColorProperty, Value = _separatorAndDisableColor },
                new Setter { Property = Shell.TabBarDisabledColorProperty, Value = _separatorAndDisableColor },
                new Setter { Property = Shell.FlowDirectionProperty, Value = _appFlowDirection },
                new Setter { Property = Shell.NavBarHasShadowProperty, Value = true },
            }
        };
        //Style defaultCustomListViewStyle = new(typeof(CustomListView))
        //{
        //    Setters =
        //    {
        //        new Setter { Property = CustomListView.HasUnevenRowsProperty, Value =  true },
        //        new Setter { Property = CustomListView.SeparatorColorProperty, Value =  _separatorAndDisableColor },
        //        new Setter { Property = CustomListView.SeparatorVisibilityProperty, Value = SeparatorVisibility.None},
        //        new Setter { Property = CustomListView.FlowDirectionProperty , Value = DefaultFlowDirection},
        //        new Setter { Property = CustomListView.IsPullToRefreshEnabledProperty , Value = false},
        //        new Setter { Property = CustomListView.BackgroundColorProperty, Value = _genericBackgroundColor },
        //        new Setter { Property = CustomListView.SelectionModeProperty , Value = ListViewSelectionMode.Single}
        //    }
        //};
        //Style defaultTransparentListViewStyle = new(typeof(CustomListView))
        //{
        //    Setters =
        //    {
        //        new Setter { Property = CustomListView.HasUnevenRowsProperty, Value =  true },
        //        new Setter { Property = CustomListView.SeparatorColorProperty, Value =  _separatorAndDisableColor },
        //        new Setter { Property = CustomListView.SeparatorVisibilityProperty, Value = SeparatorVisibility.None},
        //        new Setter { Property = CustomListView.FlowDirectionProperty , Value = DefaultFlowDirection},
        //        new Setter { Property = CustomListView.IsPullToRefreshEnabledProperty , Value = false},
        //        new Setter { Property = CustomListView.BackgroundColorProperty, Value = Colors.Transparent },
        //        new Setter { Property = CustomListView.SelectionModeProperty , Value = ListViewSelectionMode.Single}
        //    }
        //};
        //Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_LIST_KEY, defaultCustomListViewStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_TRANSPARENT_LIST_KEY, defaultTransparentListViewStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_TOGGLE_KEY, defaultToggleStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_GRADIENT_BACKGROUND_STYLE, gradientBackgroundStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_SEARCHBAR_KEY, searchBarStyle);

        //Application.Current.Resources.Add(LibStyleConstants.ST_YEAR_CALENDAR_STYLE, yearCalendarStyle);
        //Application.Current.Resources.Add(LibStyleConstants.ST_MONTH_CALENDAR_STYLE, monthCalendarStyle);
        Application.Current.Resources.Add(StyleConstants.ST_CUSTOM_POPUP_GRID_STYLE, customPopupGridStyle);
        Application.Current.Resources.Add(StyleConstants.ST_SHELL_MASTER_PAGE_STYLE, shellMasterPageStyle);
    }

    private void CreateViewsStyle()
    {
        CreateBoxViewStyle();
        //Style appLogoStyle = new Style(typeof(ContentView))
        //{
        //    Setters =
        //    {
        //        new Setter {Property = ContentView.FlowDirectionProperty , Value = DefaultFlowDirection},
        //        new Setter {Property=ContentView.HorizontalOptionsProperty,Value=LayoutOptions.CenterAndExpand},
        //        new Setter {Property=ContentView.VerticalOptionsProperty,Value=LayoutOptions.Center},
        //        new Setter {Property=ContentView.PaddingProperty, Value =  new Thickness(0, _defaultControlHeight, 0, 30) }
        //    }
        //};
        //Application.Current.Resources.Add(StyleConstants.ST_APP_LOGO_KEY, appLogoStyle);
    }

    ////private void CreateLayoutStyle()
    ////{
    ////    Style defaultEndToEndLayoutStyle = new Style(typeof(StackLayout))
    ////    {
    ////        Setters =
    ////                {
    ////                    new Setter {Property = StackLayout.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
    ////                    new Setter {Property = StackLayout.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
    ////                    new Setter {Property = StackLayout.OrientationProperty, Value = StackOrientation.Vertical},
    ////                    new Setter {Property = StackLayout.SpacingProperty, Value = Constants.ZERO_PADDING },
    ////                    new Setter {Property = StackLayout.PaddingProperty, Value =Constants.ZERO_PADDING },
    ////                    new Setter {Property = StackLayout.FlowDirectionProperty , Value = DefaultFlowDirection},
    ////                }
    ////    };

    ////    Style defaultLayoutStyle = new Style(typeof(StackLayout))
    ////    {
    ////        BasedOn = defaultEndToEndLayoutStyle,
    ////        Setters =
    ////                {
    ////                    new Setter {Property = StackLayout.SpacingProperty, Value = _controlPaddingMargin},
    ////                    new Setter {Property = StackLayout.PaddingProperty, Value = new Thickness(_controlPaddingMargin, 0, _controlPaddingMargin, _controlPaddingMargin)},
    ////                }
    ////    };
    ////    Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_END_TO_END_LAYOUT_STYLE, defaultEndToEndLayoutStyle);
    ////    Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_LAYOUT_STYLE, defaultLayoutStyle);
    ////}

    private void CreatePancakeStyle()
    {
        //todo:
        //Style badgeStyle = new Style(typeof(PancakeView))
        //{
        //    Setters =
        //                {
        //                    new Setter { Property = PancakeView.FlowDirectionProperty , Value = DefaultFlowDirection},
        //                    new Setter {Property = PancakeView.HasShadowProperty, Value = false},
        //                    new Setter {Property = PancakeView.HorizontalOptionsProperty, Value = LayoutOptions.End },
        //                    new Setter {Property = PancakeView.VerticalOptionsProperty, Value = LayoutOptions.Center },
        //                    new Setter {Property = PancakeView.MarginProperty , Value = Constants.ZERO_PADDING},
        //                    new Setter {Property = PancakeView.PaddingProperty , Value = new Thickness(0)},
        //                    new Setter {Property = PancakeView.HeightRequestProperty, Value = ImageSizeS},
        //                    new Setter {Property = PancakeView.WidthRequestProperty, Value = ImageSizeS},
        //                    new Setter {Property = PancakeView.CornerRadiusProperty,Value = ImageSizeS/2},
        //                    new Setter {Property = PancakeView.BackgroundColorProperty, Value = _accentColorKey }
        //                }
        //};

        //todo:
        //Style statusPancakeStyle = new Style(typeof(PancakeView))
        //{
        //    Setters =
        //                {
        //                    new Setter {Property = PancakeView.FlowDirectionProperty,Value= DefaultFlowDirection },
        //                    new Setter {Property = PancakeView.HasShadowProperty, Value = false},
        //                    new Setter {Property = PancakeView.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand },
        //                    new Setter {Property = PancakeView.VerticalOptionsProperty, Value = LayoutOptions.End },
        //                    new Setter {Property = PancakeView.PaddingProperty, Value = _controlPadding },
        //                    new Setter {Property = PancakeView.BorderColorProperty, Value = _accentColorKey},
        //                    new Setter {Property = PancakeView.BorderThicknessProperty,Value=1},
        //                    new Setter {Property = PancakeView.CornerRadiusProperty,Value=4},
        //                    new Setter {Property = PancakeView.BackgroundColorProperty, Value = Color.FromArgb(StyleConstants.PERCENT20_ALPHA_COLOR + (StyleConstants.ACCENT_COLOR).Replace(Constants.SYMBOL_HASH, ' ').Trim()) }
        //                }
        //};

        //todo:
        //Application.Current.Resources.Add(StyleConstants.ST_BADGE_STYLE, badgeStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_INSTRUCTION_PANCAKE_STYLE, statusPancakeStyle);

    }
}