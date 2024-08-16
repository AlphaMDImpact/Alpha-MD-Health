using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SwipeItem = DevExpress.Maui.CollectionView.SwipeItem;

namespace AlphaMDHealth.MobileClient;

public class ListViewDemoPage : BasePage
{
    private readonly AmhButtonControl _backButton;

    private readonly AmhRadioButtonControl _listViewOptions;
    private AmhListViewControl<DemoModel> _myListViewControl;
    List<DemoModel> appointments;

    public ListViewDemoPage() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.FillAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);
        AddRowColumnDefinition(new GridLength(1, GridUnitType.Auto), 3, true);

        //AddView(CreateAppLogo());

        _listViewOptions = new AmhRadioButtonControl(FieldTypes.VerticalRadioButtonControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY,
        };
        AddView(_listViewOptions);

        _backButton = new AmhButtonControl(FieldTypes.DeleteButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_backButton);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await new AuthService(App._essentials).GetAccountDataAsync(PageData).ConfigureAwait(true);

        var demoService = new ControlDemoService(App._essentials);
        demoService.GetControlDemoPageResources(PageData);

        _backButton.PageResources = PageData;
        _backButton.OnValueChanged += OnBackButtonClicked;

        _listViewOptions.PageResources = PageData;
        _listViewOptions.Options = demoService.GetListOptions();
        _listViewOptions.OnValueChanged += OnListTypeOptionChange;

        AppHelper.ShowBusyIndicator = false;
    }

    private async void EndSwapItemTapped(object sender, DevExpress.Maui.CollectionView.SwipeItemTapEventArgs e)
    {
        var resource = GetResourceByKey("ConfirmDeleteActionKey");
        var action = await ShellMasterPage.CurrentShell.CurrentPage.DisplayAlert(resource.ResourceValue, resource.PlaceHolderValue, LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_OK_ACTION_KEY), LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_CANCEL_ACTION_KEY));
        if (action)
        {
            appointments.Remove(e.Item as DemoModel);
            _myListViewControl.DataSource = null;
            _myListViewControl.DataSource = appointments;
        }
    }

    private void StartSwapItemTapped(object sender, DevExpress.Maui.CollectionView.SwipeItemTapEventArgs e)
    {
        var item = e.Item as DemoModel;
        _myListViewControl.Value = item;
    }

    protected override void OnDisappearing()
    {
        _backButton.OnValueChanged -= OnBackButtonClicked;
        _listViewOptions.OnValueChanged -= OnListTypeOptionChange;
        UnloadEvents();
        base.OnDisappearing();
    }

    private void UnloadEvents()
    {
        if (_myListViewControl != null)
        {
            _myListViewControl.OnRightViewClicked -= OnListRightViewClicked;
            _myListViewControl.OnValueChanged -= OnListItemClick;
            _myListViewControl.OnPullToRefresh -= OnListItemPullToRefresh;
            _myListViewControl.OnLoadMore -= OnListItemLoadMore;
            _myListViewControl.OnGroupShowMoreClicked -= OnGroupShowMoreClicked;
        }
    }

    private async void OnSignInButtonClicked(object sender, EventArgs e)
    {
        var sff = _myListViewControl.Value;
        if (IsFormValid())
        {
            AppHelper.ShowBusyIndicator = true;
            await ShellMasterPage.CurrentShell.PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
        }
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new ControlDemoPage()).ConfigureAwait(false);
    }

    private void AddView(View view)
    {
        int index = PageLayout.Children?.Count() ?? 0;
        PageLayout.Add(view, 0, index);
    }

    private void OnListTypeOptionChange(object sender, EventArgs e)
    {
        MapListItemSourceProps();
    }

    private async void OnListItemClick(object sender, EventArgs e)
    {
        if (_myListViewControl.Value != null)
        {
            DemoModel selectedappointment = _myListViewControl.Value as DemoModel;
            var action = await ShellMasterPage.CurrentShell.CurrentPage.DisplayActionSheet(selectedappointment.Name, LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_CANCEL_ACTION_KEY), LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_OK_ACTION_KEY));
        }
    }

    private void OnListRightViewClicked(object sender, EventArgs e)
    {
    }

    private void OnListItemLoadMore(object sender, EventArgs e)
    {
        appointments.AddRange(GenerateLargeDataset(appointments.Count));
        _myListViewControl.DataSource = appointments;
    }

    private void OnListItemPullToRefresh(object sender, EventArgs e)
    {
        List<DemoModel> shuffledData = Shuffle(appointments);
        appointments.Clear();
        appointments = shuffledData;
        _myListViewControl.DataSource = appointments;
    }

    private void MapListItemSourceProps()
    {
        if (_myListViewControl != null)
        {
            UnloadEvents();
            _myListViewControl.DataSource = null;
            PageLayout.Remove(_myListViewControl);
        }

        long val = 1;
        if (_listViewOptions.Value != null)
        {
            long.TryParse((string)_listViewOptions.Value, out val);
        }
        switch (val)
        {
            case 11://TwoRowFieldWithBadgeControl
                _myListViewControl = new AmhListViewControl<DemoModel>(FieldTypes.TwoRowListViewControl)
                {
                    IsGroupedData = false,
                    SourceFields = new AmhViewCellModel
                    {
                        ID = nameof(DemoModel.ID),
                        LeftHeader = nameof(DemoModel.Name),
                        LeftDescription = nameof(DemoModel.Description),
                        RightHeader = nameof(DemoModel.SubHeader),
                        RightDescription = nameof(DemoModel.Status),
                        RightDescriptionField = nameof(DemoModel.StatusType),
                        LeftFieldType = FieldTypes.SquareImageControl,
                        LeftImage = nameof(DemoModel.Image),
                        LeftIcon = nameof(DemoModel.MainIcon),
                        BandColor = nameof(DemoModel.BandColor),
                    }
                };
                break;
            case 8: //TwoRowGroupedListViewControl
            case 4: //TwoRowGroupedListViewControl
                _myListViewControl = new AmhListViewControl<DemoModel>(FieldTypes.TwoRowGroupedListViewControl)
                {
                    IsGroupedData = true,
                    SourceFields = new AmhViewCellModel
                    {
                        ID = nameof(DemoModel.ID),
                        LeftHeader = nameof(DemoModel.Name),
                        LeftDescription = nameof(DemoModel.Description),
                        RightHeader = nameof(DemoModel.SubHeader),
                        RightDescription = nameof(DemoModel.Status),
                        LeftFieldType = FieldTypes.CircleImageControl,
                        LeftImage = nameof(DemoModel.Image),
                        LeftIcon = nameof(DemoModel.MainIcon),
                        //RightImageByte = nameof(DemoModel.ImageBytes),
                        //RightHTMLLabelField = nameof(DemoModel.AppointmentStatusName),
                        //RightImageIcon = nameof(DemoModel.LanguageName),
                        GroupName = nameof(DemoModel.GroupName),
                        BandColor = nameof(DemoModel.BandColor),
                    }
                };
                break;
            case 10: //ShowMoreIconOneRowListViewControl
            case 9: //ShowMoreLabelOneRowListViewControl
            case 7: //OneRowGroupedListViewControl
            case 3: //OneRowGroupedListViewControl
                _myListViewControl = new AmhListViewControl<DemoModel>(FieldTypes.OneRowGroupedListViewControl)
                {
                    IsGroupedData = true,
                    SourceFields = new AmhViewCellModel
                    {
                        ID = nameof(DemoModel.ID),
                        LeftHeader = nameof(DemoModel.Name),
                        RightHeader = nameof(DemoModel.SubHeader),
                        LeftFieldType = FieldTypes.SquareImageControl,
                        LeftImage = nameof(DemoModel.Image),
                        LeftIcon = nameof(DemoModel.MainIcon),
                        GroupName = nameof(DemoModel.GroupName),
                        BandColor = nameof(DemoModel.BandColor),
                    }
                };
                break;
            case 6: //TwoRowListViewControl
            case 2: //TwoRowListViewControl
                _myListViewControl = new AmhListViewControl<DemoModel>(FieldTypes.TwoRowListViewControl)
                {
                    IsGroupedData = false,
                    SourceFields = new AmhViewCellModel
                    {
                        ID = nameof(DemoModel.ID),
                        LeftHeader = nameof(DemoModel.Name),
                        LeftDescription = nameof(DemoModel.Description),
                        RightHeader = nameof(DemoModel.SubHeader),
                        RightDescription = nameof(DemoModel.Status),
                        LeftFieldType = FieldTypes.SquareImageControl,
                        LeftImage = nameof(DemoModel.Image),
                        LeftIcon = nameof(DemoModel.MainIcon),
                        //RightImageByte = nameof(DemoModel.ImageBytes),
                        //RightHTMLLabelField = nameof(DemoModel.AppointmentStatusName),
                        RightIcon = nameof(DemoModel.NavIcon),
                        BandColor = nameof(DemoModel.BandColor),
                    }
                };
                break;
            case 5: //OneRowListViewControl
            case 1: //OneRowListViewControl
            default:
                _myListViewControl = new AmhListViewControl<DemoModel>(FieldTypes.OneRowListViewControl)
                {
                    IsGroupedData = false,
                    SourceFields = new AmhViewCellModel
                    {
                        ID = nameof(DemoModel.ID),
                        LeftFieldType = FieldTypes.CircleImageControl,
                        LeftImage = nameof(DemoModel.Image),
                        LeftIcon = nameof(DemoModel.MainIcon),
                        LeftHeader = nameof(DemoModel.Name),
                        RightHeader = nameof(DemoModel.SubHeader),
                    }
                };
                break;
        }

        _myListViewControl.ShowSearchBar = val == 5 || val == 6 || val == 7 || val == 8;
        _myListViewControl.GroupShowMoreIcon = val == 10 ? "arrowmenuiconltr.png" : "";
        _myListViewControl.GroupShowMoreText = val == 9 ? "Show More > " : "";

        appointments = GenerateLargeDataset(0);

        var startSwapItem = new SwipeItem
        {
            Caption = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_ITEM_KEY),
            Image = ImageSource.FromFile("arrowmenuiconrtl.png"),
            FontColor = (Color)App.Current.Resources[StyleConstants.ST_SUCCESS_COLOR],
            BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_TERTIARY_APP_COLOR],
            FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
        };
        startSwapItem.Tap += StartSwapItemTapped;
        _myListViewControl.StartSwapItems = new List<SwipeItem> { startSwapItem };

        var endSwapItem = new SwipeItem
        {
            Caption = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DELETE_ACTION_KEY),
            Image = ImageSource.FromFile("delete.png"),
            FontColor = (Color)App.Current.Resources[StyleConstants.ST_PRIMARY_TEXT_COLOR],
            BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_ERROR_COLOR],
            FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
        };
        endSwapItem.Tap += EndSwapItemTapped;
        _myListViewControl.EndSwapItems = new List<SwipeItem> { endSwapItem };

        _myListViewControl.PageResources = PageData;
        _myListViewControl.DataSource = appointments;

        AddView(_myListViewControl);
        _myListViewControl.OnRightViewClicked += OnListRightViewClicked;
        _myListViewControl.OnValueChanged += OnListItemClick;
        _myListViewControl.OnLoadMore += OnListItemLoadMore;
        _myListViewControl.OnPullToRefresh += OnListItemPullToRefresh;
        _myListViewControl.OnGroupShowMoreClicked += OnGroupShowMoreClicked;

        _myListViewControl.IsPullToRefreshEnabled = val == 1 || val == 2 || val == 3 || val == 4;
        _myListViewControl.IsLoadMoreEnabled = val == 5 || val == 6 || val == 7 || val == 8;
        if (val == 9 || val == 10)
        {
            _myListViewControl.IsPullToRefreshEnabled = true;
            _myListViewControl.IsLoadMoreEnabled = true;
        }
    }

    private async void OnGroupShowMoreClicked(object sender, EventArgs e)
    {
        if (sender != null)
        {
            var group = (string)sender;
            var action = await ShellMasterPage.CurrentShell.CurrentPage.DisplayActionSheet(group, LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_CANCEL_ACTION_KEY), LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_OK_ACTION_KEY));
        }
    }

    private List<DemoModel> Shuffle(List<DemoModel> collection)
    {
        // shuffle it
        List<DemoModel> list = collection.ToList();
        Random rng = new Random();

        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            DemoModel value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return new List<DemoModel>(list);
    }

    private Random rnd = new Random();

    private List<DemoModel> GenerateLargeDataset(int lastIndex)
    {
        lastIndex += 1;
        List<DemoModel> dataset = new List<DemoModel>();
        for (int i = lastIndex; i <= lastIndex + 10; i++)
        {
            var appointment = new DemoModel
            {
                ID = i,
                MainIcon = "bill.png",
                Image = i % 7 == 0
                    ? "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"
                    : $"T-{i}",
                Name = $"Test-{i}",
                Description = $"sstest data description {i} ddn",
                Status = $"Active{i}",
                StatusType = GetStatusType(i),
                SubHeader = "1990",
                HtmlData = $"<p><h1 style=\"background-color:red;\">HtmlLabel</h1><nbsp><i>HtmlLabel</i> <nbsp> <b>HtmlLabel</b> <nbsp> <u>HtmlLabel</u></p>",
                NavIcon = "delete.png",
            };
            if (i % 6 == 0)
            {
                appointment.BandColor = "#DAF7A6";
            }
            if (i % 5 == 0)
            {
                appointment.BandColor = "#7FFFD4";
            }
            else if (i % 4 == 0)
            {
                appointment.BandColor = "#0000FF";
            }
            else if (i % 3 == 0)
            {
                appointment.BandColor = "#9932CC";
            }

            if (_myListViewControl.IsGroupedData)
            {
                if (i % 5 == 0)
                {
                    appointment.GroupName = "Group-1";
                }
                else if (i % 4 == 0)
                {
                    appointment.GroupName = "Group-2";
                }
                else if (i % 3 == 0)
                {
                    appointment.GroupName = "Group-3";
                }
                else
                {
                    appointment.GroupName = "Group-0";
                }
            }
            dataset.Add(appointment);
        }
        return dataset;
    }

    private FieldTypes GetStatusType(int i)
    {
        return i % 8 == 0 ? FieldTypes.PrimaryBadgeControl
            : i % 7 == 0 ? FieldTypes.SecondaryBadgeControl
            : i % 6 == 0 ? FieldTypes.SuccessBadgeControl
            : i % 5 == 0 ? FieldTypes.DangerBadgeControl
            : i % 4 == 0 ? FieldTypes.WarningBadgeControl
            : i % 3 == 0 ? FieldTypes.InfoBadgeControl
            : i % 2 == 0 ? FieldTypes.DarkBadgeControl
            : i % 1 == 0 ? FieldTypes.LightBadgeControl
            : FieldTypes.BadgeControl;
    }
}