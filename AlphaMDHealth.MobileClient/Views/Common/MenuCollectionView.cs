using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Menu collection view
/// </summary>
public class MenuCollectionView : ViewManager
{
    private readonly MenuDTO _moreMenus;
    private readonly AmhListViewControl<MenuModel> _menuList;

    /// <summary>
    /// Parameterized constructor of more menus containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public MenuCollectionView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new MenuService(App._essentials);
        _moreMenus = new MenuDTO();
        if (Parameters?.Count > 0)
        {
            _moreMenus.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
        }
        _menuList = new AmhListViewControl<MenuModel>(FieldTypes.OneRowListViewControl)
        {
            ResourceKey = AppPermissions.MobileMenusView.ToString(),
            SourceFields = new AmhViewCellModel
            {
                ID = nameof(MenuModel.MenuID),
                GroupName = nameof(MenuModel.GroupHeading),
                LeftIcon = nameof(MenuModel.GroupIcon),
                LeftImage = nameof(MenuModel.Image),
                LeftFieldType = FieldTypes.SquareImageControl,
                LeftHeader = nameof(MenuModel.PageHeading),
                RightIcon = nameof(MenuModel.NavIcon),
                RightFieldType = FieldTypes.CircleImageControl
            },
            ShowSearchBar = false
        };
        ParentPage.PageLayout.Add(_menuList, 0, 0);
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        await (ParentPage.PageService as MenuService).GetMoreMenusAsync(_moreMenus).ConfigureAwait(false);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _menuList.PageResources = ParentPage.PageData = _moreMenus;
            ParentPage.ApplyPageResources();
            _menuList.ErrorCode = _moreMenus.ErrCode;
            _menuList.DataSource = _moreMenus.Menus ?? new List<MenuModel>();
            _menuList.OnValueChanged += OnSelectionChanged;
        });
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        _menuList.OnValueChanged -= OnSelectionChanged;
        await Task.CompletedTask;
    }

    private async void OnSelectionChanged(object sender, EventArgs e)
    {
        if (_menuList.Value != null)
        {
            //IsEnabled = false;
            var item = _menuList.Value as MenuModel;
            if (item.MenuID > 0)
            {
                AppHelper.ShowBusyIndicator = true;
                if (!await ShellMasterPage.CurrentShell.OnMoreOptionSelectionChangeAsync(item).ConfigureAwait(false))
                {
                    await ShellMasterPage.CurrentShell.PageInstance.PushPageByNodeIDAsync(item.TargetID, true, string.Empty).ConfigureAwait(false);
                }
                //todo:
                //if (PopupNavigation.Instance.PopupStack?.Count > 0)
                //{
                //    await PopupNavigation.Instance.PopAsync().ConfigureAwait(true);
                //}
            }
            //MainThread.BeginInvokeOnMainThread(() =>
            //{
            //    _menuList.Value = null;
            //    IsEnabled = true;
            //});
        }
    }
}

//using AlphaMDHealth.ClientBusinessLayer;
//using AlphaMDHealth.Model;
//using AlphaMDHealth.Utility;

//namespace AlphaMDHealth.MobileClient;

///// <summary>
///// Menu collection view
///// </summary>
//public class MenuCollectionView : BaseLibCollectionView
//{
//    private readonly CustomMessageControl _messageView;

//    /// <summary>
//    /// Creates menu collection view
//    /// </summary>
//    public MenuCollectionView()
//    {
//        _messageView = new CustomMessageControl(false);
//        CustomCellModel customCellModel = new CustomCellModel
//        {
//            CellHeader = nameof(MenuModel.PageHeading),
//            //todo:
//            //CellLeftSourceIcon = nameof(MenuModel.ImageSource),
//            CellRightIcon = nameof(MenuModel.ImageBase64),
//            CellLeftDefaultIcon = nameof(MenuModel.GroupIcon),
//            IconSize = AppImageSize.ImageSizeS,
//            NoMarginNoSeprator = true,
//            IsCircle = (bool)Application.Current.Resources[StyleConstants.ST_IS_PROFILE_CIRCULAR_STYLE],
//            IsList = (bool)Application.Current.Resources[StyleConstants.ST_IS_PROFILE_CIRCULAR_STYLE],
//            CellSecondMiddleSatusContentHeader = nameof(MenuModel.BadgeCount),
//            //todo: //CellMiddleSatusHeaderStyle = (Style)Application.Current.Resources[LibStyleConstants.ST_BADGE_STYLE]
//        };
//        Grid mainLayout = new Grid
//        {
//            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
//            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR],
//            RowDefinitions =
//            {
//                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
//            },
//            ColumnDefinitions =
//            {
//                new ColumnDefinition { Width = GridLength.Auto }
//            }
//        };
//        AddListView(mainLayout, customCellModel, 0, 0, null, ListViewCachingStrategy.RetainElement, StyleConstants.TRANSPARENT_COLOR_STRING);
//        Content = mainLayout;
//    }

//    /// <summary>
//    /// Loads menus to be shown in list
//    /// </summary>
//    /// <returns>Returns ContentView with menu list</returns>
//    internal async Task LoadUIDataAsync()
//    {
//        ListViewField.ItemTapped += OnSelectionChanged;
//        IEnumerable<IGrouping<(byte SequenceNo, long MenuGroupID, string Content), MenuModel>> moreMenus = null;
//        moreMenus = await ShellMasterPage.CurrentShell.GetMoreMenusAsync().ConfigureAwait(true);
//        MainThread.BeginInvokeOnMainThread(async () =>
//        {
//            if (moreMenus?.Count() > 0)
//            {
//                ListViewField.HasUnevenRows = true;
//                ListViewField.ItemsSource = moreMenus;
//                ListViewField.IsGroupingEnabled = true;
//                ListViewField.GroupHeaderTemplate = new DataTemplate(typeof(GroupViewCell));
//            }
//            else
//            {
//                var service = new ResourceService(App._essentials);
//                await service.GetResourcesAsync(GroupConstants.RS_COMMON_GROUP).ConfigureAwait(true);
//                _messageView.PageResources = service.PageData;
//                _messageView.ControlResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY;
//                ListViewField.Header = _messageView;
//            }
//        });
//    }

//    private async void OnSelectionChanged(object sender, ItemTappedEventArgs e)
//    {
//        if (ListViewField.SelectedItem != null)
//        {
//            IsEnabled = false;
//            var item = ListViewField.SelectedItem as MenuModel;
//            if (item.MenuID > 0)
//            {
//                AppHelper.ShowBusyIndicator = true;
//                if (!await ShellMasterPage.CurrentShell.OnMoreOptionSelectionChangeAsync(item).ConfigureAwait(false))
//                {
//                    await ShellMasterPage.CurrentShell.BaseContentPageInstance.PushPageByNodeIDAsync(item.TargetID, true, string.Empty).ConfigureAwait(false);
//                }
//                //todo:
//                //if (PopupNavigation.Instance.PopupStack?.Count > 0)
//                //{
//                //    await PopupNavigation.Instance.PopAsync().ConfigureAwait(true);
//                //}
//            }
//            MainThread.BeginInvokeOnMainThread(() =>
//            {
//                ListViewField.SelectedItem = null;
//                IsEnabled = true;
//            });
//        }
//    }

//    /// <summary>
//    /// Unregister events attached in view
//    /// </summary>
//    internal void UnLoadUIData()
//    {
//        ListViewField.ItemTapped -= OnSelectionChanged;
//    }
//}