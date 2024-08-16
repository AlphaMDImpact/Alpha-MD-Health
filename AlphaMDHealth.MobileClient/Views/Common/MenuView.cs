using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Menu View
/// </summary>
public class MenuView : Grid
{
    /// <summary>
    /// Creates header menu item for the given position
    /// </summary>
    /// <param name="menuPosition">Position of menu item left, right, header</param>
    /// <param name="title">Icon for left right header items and title incase of center</param>
    /// <param name="isAddEditPage">dd edit ppage flag</param>
    public MenuView(MenuLocation menuPosition, string title, bool isAddEditPage) : this(menuPosition, title, default, default, null, string.Empty, string.Empty, isAddEditPage)
    {

    }

    /// <summary>
    /// Creates header menu item for the given position
    /// </summary>
    /// <param name="menuPosition">Position of menu item left, right, header</param>
    /// <param name="iconName">Icon for left right header items and title incase of center</param>
    /// <param name="menuActionID">Type of action</param>
    /// <param name="nodeID">Menu node id</param>
    /// <param name="profileImage">profile image source</param>
    /// <param name="profileInitials">profile image initials</param>
    /// <param name="nodeText">Node text data</param>
    /// <param name="isAddEditPage">add edit page flag</param>
    public MenuView(MenuLocation menuPosition, string iconName, MenuAction menuActionID, long nodeID, string profileImage, string profileInitials, string nodeText, bool isAddEditPage)
    {
        Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE];
        Padding = Constants.ZERO_PADDING;
        RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
        };
        ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
        };
        AppImageSize AppImageSize = menuActionID == MenuAction.MenuActionProfileKey ? AppImageSize.ImageSizeM : AppImageSize.ImageSizeS;
        AmhImageControl tabIcon = new AmhImageControl(FieldTypes.SquareImageControl)
        {
            ImageHeight = AppImageSize,
            ImageWidth = AppImageSize
        };
        switch (menuPosition)
        {
            case MenuLocation.Header:
                CreateTitleLabel(iconName, profileImage, profileInitials, isAddEditPage, tabIcon);
                return;
            case MenuLocation.Left:
                tabIcon.HorizontalOptions = HorizontalOptions = LayoutOptions.Start;
                break;
            case MenuLocation.Right:
                tabIcon.HorizontalOptions = HorizontalOptions = LayoutOptions.EndAndExpand;
                break;
            default:
                //will use for future implementation
                break;
        }
        VerticalOptions = LayoutOptions.FillAndExpand;
        SetContentForHeaderItem(menuPosition, iconName, menuActionID, nodeID, profileImage, profileInitials, nodeText, isAddEditPage, tabIcon);

        HorizontalOptions = LayoutOptions.FillAndExpand;
        if (menuActionID != MenuAction.MenuActionDefaultKey || nodeID > 0)
        {
            TapGestureRecognizer gridTapGesture = new TapGestureRecognizer();

            gridTapGesture.Tapped += async (s, e) =>
            {
                ((View)s).IsEnabled = false;
                await OnMenuClickedAsync(nodeID, menuActionID, menuPosition, this).ConfigureAwait(true);
                ((View)s).IsEnabled = true;
            };
            GestureRecognizers.Add(gridTapGesture);
        }
    }

    private void SetContentForHeaderItem(MenuLocation menuPosition, string iconName, MenuAction menuActionID, long nodeID
        , string profileImage, string profileInitials, string nodeText, bool isAddEditPage, AmhImageControl tabIcon)
    {
        if (menuActionID == MenuAction.MenuActionProfileKey)
        {
            ApplyImageSourceToIcon(tabIcon, profileImage, profileInitials);
            this.Add(tabIcon, 0, 1);
        }
        else if (string.IsNullOrWhiteSpace(iconName))
        {
            if (menuActionID != MenuAction.MenuActionDefaultKey || nodeID > 0)
            {
                AmhLabelControl menuTitle = new AmhLabelControl(FieldTypes.PrimarySmallHStartVCenterLabelControl)//(LabelType.PrimaryMediumCenter)
                {
                    Value = nodeText,
                    Margin = new Thickness(-10, 0, 0, 0),
                    //todo: TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR]
                };
                //todo:
                //if (isAddEditPage)
                //{
                //    menuTitle.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
                //}
                //if (menuPosition == MenuLocation.Left)
                //{
                //    menuTitle.HorizontalOptions = HorizontalOptions = LayoutOptions.Start;
                //    menuTitle.HorizontalTextAlignment = TextAlignment.Start;
                //}
                //else if (menuPosition == MenuLocation.Right)
                //{
                //    menuTitle.HorizontalOptions = HorizontalOptions = LayoutOptions.EndAndExpand;
                //    menuTitle.HorizontalTextAlignment = TextAlignment.End;
                //}
                this.Add(menuTitle, 0, 1);
            }
        }
        else
        {
            tabIcon.Icon = iconName;
            // If language is RTL and is back menu then rotate
            HandleBackIcon(menuActionID, tabIcon);
            this.Add(tabIcon, 0, 1);
        }
    }

    private void CreateTitleLabel(string iconName, string profileImage, string profileInitials, bool isAddEditPage, AmhImageControl tabIcon)
    {
        AmhLabelControl titleLabel = new AmhLabelControl(FieldTypes.PrimarySmallHVCenterLabelControl)//(LabelType.PrimaryMediumBoldCenter)
        {
            //todo:
            //VerticalTextAlignment = TextAlignment.Center,
            //LineBreakMode = LineBreakMode.TailTruncation,
            //IsVisible = true,
            Value = iconName
        };
        HorizontalOptions = titleLabel.VerticalOptions = LayoutOptions.Center;
        if (profileImage == null && string.IsNullOrWhiteSpace(profileInitials))
        {
            tabIcon.IsVisible = false;
            this.Add(titleLabel, 0, 1);
            titleLabel.Margin = new Thickness(0, 0,
                GenericMethods.GetPlatformSpecificValue((double)Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING] / 2,
                (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], 0), 0);
        }
        else
        {
            Grid centerGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                RowSpacing = 5,
            };
            //// tabIcon.ImageWidth = AppImageSize.ImageSizeM;
            ApplyImageSourceToIcon(tabIcon, profileImage, profileInitials);
            centerGrid.Add(tabIcon, 0, 0);
            centerGrid.Add(titleLabel, 1, 0);
            this.Add(centerGrid, 0, 1);
            centerGrid.Margin = new Thickness(0, 0
                , GenericMethods.GetPlatformSpecificValue((double)Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING] / 2
                , (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], 0), 0);
        }
    }

    //private LabelType GetLabelTypeBasedOnAppName(bool isAddEditPage)
    //{
    //    //// isAddEditPage used in future
    //    return LabelType.PrimaryMediumBoldCenter;
    //}

    /// <summary>
    /// On click of any meny, load its content page
    /// </summary>
    /// <param name="nodeID">node id to which the app has to navigate to</param>
    /// <param name="menuAction">action resource key for handling</param>
    /// <param name="menuPosition">Position of the menu</param>
    /// <param name="grid">Currently tapped grid</param>
    private async Task OnMenuClickedAsync(long nodeID, MenuAction menuAction, MenuLocation menuPosition, View grid)
    {
        if (nodeID == 0 && menuAction != MenuAction.MenuActionProfileKey)
        {
            if (menuAction == MenuAction.MenuActionBackKey || menuAction == MenuAction.MenuActionCloseKey)
            {
                // Check if the current page handles back or close navigation
                if (ShellMasterPage.CurrentShell.CurrentPage != null
                    && await ShellMasterPage.CurrentShell.CurrentPage.OnBackCloseClickAsync(GetHeaderPosition(), menuPosition, menuAction).ConfigureAwait(true))
                {
                    // as navigation is handled by page
                    return;
                }
                // If back navigation
                if (Shell.Current.Navigation.ModalStack.Count > 0)
                {
                    await Shell.Current.Navigation.PopModalAsync().ConfigureAwait(false);
                }
                else
                {
                    if (ShellMasterPage.CurrentShell.CurrentPage != null)
                    {
                        await ShellMasterPage.CurrentShell.CurrentPage.PopPageAsync(false).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await ShellMasterPage.CurrentShell.CurrentPage.OnMenuActionClick(GetHeaderPosition(), menuPosition, menuAction);
            }
        }
        else
        {
            AppHelper.ShowBusyIndicator = true;
            grid.IsEnabled = false;
            if (menuAction == MenuAction.MenuActionProfileKey)
            {
                await ShellMasterPage.CurrentShell.PageInstance.PushProfilePageAsync(nodeID).ConfigureAwait(true);
            }
            else
            {
                await ShellMasterPage.CurrentShell.PageInstance.PushPageByNodeIDAsync(nodeID, false, string.Empty).ConfigureAwait(true);
            }
            grid.IsEnabled = true;
        }
    }

    private MenuLocation GetHeaderPosition()
    {
        return StyleId == MenuLocation.Right.ToString() ? MenuLocation.Right : MenuLocation.Left;
    }

    private void ApplyImageSourceToIcon(AmhImageControl tabIcon, string profileImage, string profileInitials)
    {
        if (profileImage != null)
        {
            tabIcon.Value = profileImage;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(profileInitials))
            {
                tabIcon.Value = profileInitials;
            }
        }
    }

    private void HandleBackIcon(MenuAction menuActionID, AmhImageControl tabIcon)
    {
        if (FlowDirection == FlowDirection.RightToLeft && menuActionID == MenuAction.MenuActionBackKey)
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                tabIcon.AnchorX = 0.45;
            }
            tabIcon.RotationY = 180;
        }
    }
}