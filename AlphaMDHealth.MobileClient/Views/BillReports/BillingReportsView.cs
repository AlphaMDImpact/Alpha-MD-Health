using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class BillingReportsView : BaseLibCollectionView
{
    private readonly BillingItemDTO _billingData = new BillingItemDTO();
    private Grid _mainLayout;
    private BoxView _boxview;
    private CustomDateTimeControl _fromDate;
    private CustomDateTimeControl _toDate;
    private CustomLabelControl _billingReportLabel;
    private CustomButtonControl _searchButton;
    private CustomLabelControl _grossTotalValue;
    private CustomLabelControl _discountValue;
    private CustomLabelControl _totalAmountValue;
    private CustomLabelControl _grossTotal;
    private CustomLabelControl _discount;
    private CustomLabelControl _totalAmount;
    private readonly CustomCellModel _billListCell;
    private readonly BasePage _parentPage;
    private bool _isRepeatRequest;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public BillingReportsView(BasePage page, object parameters) : base(page, parameters)
    {
        _isRepeatRequest = false;
        _parentPage = page;
        var padding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
        _billListCell = new CustomCellModel
        {
            CellHeader = nameof(PatientBillModel.PatientName),
            CellDescription = nameof(PatientBillModel.DescriptionInMobile),
            CellRightContentHeader = nameof(PatientBillModel.BillDateTimeString),
            BandColor = nameof(PatientBillModel.ProgramColor),
            CellRightContentDescription = nameof(PatientBillModel.Status),
            CellDescriptionColor = nameof(PatientBillModel.CurrentStatus)
        };
        _mainLayout = new Grid()
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            Margin = new Thickness(new OnIdiom<double> { Phone = padding, Tablet = ParentPage.PageLayout.Padding.Right + padding }, padding),
            Padding = new Thickness(padding, padding, padding, padding),
            ColumnDefinitions = new ColumnDefinitionCollection()
            {
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = GridLength.Star },
                new ColumnDefinition{ Width = GridLength.Star },
            },
            RowDefinitions = new RowDefinitionCollection()
            {
                new RowDefinition{ Height = GridLength.Auto },
                new RowDefinition{ Height = GridLength.Auto },
                new RowDefinition{ Height = GridLength.Auto },
                new RowDefinition{ Height = GridLength.Auto },
                new RowDefinition{ Height = GridLength.Auto },
                new RowDefinition{ Height = GridLength.Star },
            }
        };
        _boxview = new BoxView() { HeightRequest = 10, VerticalOptions = LayoutOptions.Center };
        CardValueSetter();
        _fromDate = CreateDateControl(ResourceConstants.R_Report_From_Date_Key);
        _toDate = CreateDateControl(ResourceConstants.R_Report_TO_Date_Key);
        _searchButton = new CustomButtonControl(ButtonType.SecondryWithoutMargin)
        {
            WidthRequest = 200,
            HeightRequest = 35,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
        };
        _mainLayout.Add(_boxview, 0, 3);
        _mainLayout.Add(_billingReportLabel, 0, 0);
        _mainLayout.Add(_fromDate, 2, 0);
        _mainLayout.Add(_toDate, 3, 0);
        _mainLayout.Add(_searchButton, 4, 0);
        _mainLayout.Add(_grossTotalValue, 0, 1);
        Grid.SetColumnSpan(_grossTotalValue, 2);
        _mainLayout.Add(_discountValue, 2, 1);
        _mainLayout.Add(_totalAmountValue, 3, 1);
        Grid.SetColumnSpan(_totalAmountValue, 2);
        _mainLayout.Add(_grossTotal, 0, 2);
        Grid.SetColumnSpan(_grossTotal, 2);
        _mainLayout.Add(_discount, 2, 2);
        _mainLayout.Add(_totalAmount, 3, 2);
        Grid.SetColumnSpan(_totalAmount, 2);
        DefineTable(padding);
        _searchButton.Clicked += OnSearchButtonClicked;
        SetPageContent(_mainLayout);
    }

    private async void PatientBillingSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CollectionViewField.SelectionChanged -= PatientBillingSelectionChanged;
        var item = sender as CollectionView;
        if (item.SelectedItem != null)
        {
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                var patientBill = item.SelectedItem as PatientBillModel;
                var parameter = ParentPage.AddParameters(
                        ParentPage.CreateParameter(nameof(PatientBillModel.PatientBillID), patientBill.PatientBillID.ToString()),
                        ParentPage.CreateParameter(nameof(BaseDTO.SelectedUserID), patientBill.PatientID.ToString(CultureInfo.InvariantCulture))
                   );
                var patientAddEditPage = new PatientBillingAddEditPagePopupPage(ParentPage, parameter);
                //todo:await Navigation.PushPopupAsync(patientAddEditPage).ConfigureAwait(false);
            }
        }
        CollectionViewField.SelectionChanged += PatientBillingSelectionChanged;
    }

    private async void RefreshBillsList(object sender, EventArgs e)
    {
        await ListChange();
    }

    private void CardValueSetter()
    {
        _billingReportLabel = new CustomLabelControl(LabelType.HeaderPrimaryMediumBoldForDashboard) { WidthRequest = 250 };
        _grossTotalValue = new CustomLabelControl(LabelType.PrimaryMediumLeft) { Padding = 0, Text = string.Empty, HorizontalOptions = LayoutOptions.Center };
        _discountValue = new CustomLabelControl(LabelType.PrimaryMediumLeft) { Padding = 0, Text = string.Empty, HorizontalOptions = LayoutOptions.Center };
        _totalAmountValue = new CustomLabelControl(LabelType.PrimaryMediumLeft) { Padding = 0, Text = string.Empty, HorizontalOptions = LayoutOptions.Center };
        _grossTotal = new CustomLabelControl(LabelType.PrimarySmallLeft) { Text = string.Empty, HorizontalOptions = LayoutOptions.Center, HeightRequest = 20, VerticalOptions = LayoutOptions.Center };
        _discount = new CustomLabelControl(LabelType.PrimarySmallLeft) { Text = string.Empty, HorizontalOptions = LayoutOptions.Center, HeightRequest = 20, VerticalOptions = LayoutOptions.Center };
        _totalAmount = new CustomLabelControl(LabelType.PrimarySmallLeft) { Text = string.Empty, HorizontalOptions = LayoutOptions.Center, HeightRequest = 20, VerticalOptions = LayoutOptions.Center };
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        if (!_isRepeatRequest)
        {
            AppHelper.ShowBusyIndicator = true;
            _billingData.RecordCount = -1;
            _billingData.FromDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            _billingData.ToDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            _billingData.AccountID = App._essentials.GetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, (long)0);
            ReportsService reportsService = new ReportsService(App._essentials);
            await reportsService.SyncBillsFromServerAsync(_billingData, CancellationToken.None).ConfigureAwait(true);
            SearchField.OnSearchTextChanged += CustomSearch_OnSearchTextChanged;
            if (_billingData.ErrCode == ErrorCode.OK)
            {
                _isRepeatRequest = true;
                ParentPage.PageData.Resources = _billingData.Resources;
                ParentPage.PageData.Settings = _billingData.Settings;
                _grossTotal.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_GROSS_TOTAL_KEY);
                _discount.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_DISCOUNT_KEY);
                _totalAmount.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_TOTAL_AMOUNT_KEY);
                _billingReportLabel.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_BILLING_REPORTS_KEY);
                _fromDate.PageResources = ParentPage.PageData;
                _toDate.PageResources = ParentPage.PageData;
                await Task.Delay(Constants.DATE_RENDER_DELAY).ConfigureAwait(true);
                _fromDate.GetSetDate = DateTime.Now.Date;
                _toDate.GetSetDate = DateTime.Now.Date;
                ApplyPageData();
                AppHelper.ShowBusyIndicator = false;
                if (_billingData.PatientBills != null)
                {
                    _totalAmountValue.Text = TotalCalculator(_billingData.PatientBills);
                    _discountValue.Text = DiscountCalculator(_billingData.PatientBills);
                    _grossTotalValue.Text = GrossCalculator(_billingData.PatientBills);
                    DisplayDataInList(_billingData.PatientBills);
                }
                else
                {
                    RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
                    _totalAmountValue.Text = Constants.ZERO;
                    _discountValue.Text = Constants.ZERO;
                    _grossTotalValue.Text = Constants.ZERO;
                }
                ApplyTableHeaderText(_billingData.PatientBills != null ? _billingData.PatientBills.Count : 0);
                _searchButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_SEARCH_TEXT_KEY);
                OnListItemSelection(PatientBillingSelectionChanged, !IsPatientOverview(_billingData.RecordCount));
            }
        }
    }

    private void ApplyPageData()
    {
        if (SearchField != null)
        {
            SearchField.PageResources = ParentPage.PageData;
            SearchField.OnSearchTextChanged += CustomSearch_OnSearchTextChanged;
        }
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        if (SearchField != null)
        {
            SearchField.OnSearchTextChanged -= CustomSearch_OnSearchTextChanged;
        }
        await base.UnloadUIAsync().ConfigureAwait(true);
        if (SearchField != null)
        {
            SearchField.OnSearchTextChanged -= CustomSearch_OnSearchTextChanged;
        }
        await base.UnloadUIAsync().ConfigureAwait(true);
    }

    private void CustomSearch_OnSearchTextChanged(object sender, EventArgs e)
    {
        var serchBar = sender as CustomSearchBar;
        CollectionViewField.Footer = null;
        if (string.IsNullOrWhiteSpace(serchBar.Text))
        {
            CollectionViewField.ItemsSource = _billingData.PatientBills;
            ApplyTableHeaderText(_billingData.PatientBills.Count);
            _totalAmountValue.Text = TotalCalculator(_billingData.PatientBills);
            _discountValue.Text = DiscountCalculator(_billingData.PatientBills);
            _grossTotalValue.Text = GrossCalculator(_billingData.PatientBills);
        }
        else
        {
            var searchedUsers = _billingData.PatientBills.FindAll(y =>
            {
                return y.PatientName.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant())
                || y.ProviderName.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant())
                || y.ProgramName.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant())
                || y.BillDateTimeString.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant())
                || y.PaymentMode.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant())
                || y.TotalPaid.ToString().ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant())
                || y.Status.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant());
            });
            ApplyTableHeaderText(searchedUsers.Count);
            CollectionViewField.ItemsSource = searchedUsers;
            if (searchedUsers.Count <= 0)
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
            }
            _totalAmountValue.Text = TotalCalculator(searchedUsers);
            _discountValue.Text = DiscountCalculator(searchedUsers);
            _grossTotalValue.Text = GrossCalculator(searchedUsers);
        }
    }

    private void ApplyTableHeaderText(int count)
    {
        TabletHeader.Text = $"{ParentPage.GetResourceValueByKey(ResourceConstants.R_BILLS_KEY)} ({count})";
    }

    private async void OnSearchButtonClicked(object sender, EventArgs e)
    {
        await ListChange();
    }

    private async Task ListChange()
    {
        AppHelper.ShowBusyIndicator = true;
        if (_fromDate.GetSetDate != null && _toDate.GetSetDate != null)
        {
            DateTimeOffset from = _fromDate.GetSetDate.Value;
            DateTimeOffset to = _toDate.GetSetDate.Value;
            if (_fromDate.GetSetDate.Value > _toDate.GetSetDate.Value)
            {
                ParentPage.DisplayOperationStatus(string.Format(CultureInfo.InvariantCulture, ParentPage.GetResourceValueByKey(ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                ParentPage.GetResourceValueByKey(ResourceConstants.R_START_DATE_KEY), ParentPage.GetResourceValueByKey(ResourceConstants.R_END_DATE_KEY)));
            }
            else
            {
                _billingData.FromDate = new DateTimeOffset(from.DateTime, TimeSpan.Zero).ToString(CultureInfo.InvariantCulture);
                _billingData.ToDate = new DateTimeOffset(to.DateTime, TimeSpan.Zero).ToString(CultureInfo.InvariantCulture);
                await GetBillingData();
            }
        }
        AppHelper.ShowBusyIndicator = false;
    }

    private async Task GetBillingData()
    {
        ReportsService reportsService = new ReportsService(App._essentials);
        await reportsService.SyncBillsFromServerAsync(_billingData, CancellationToken.None).ConfigureAwait(true);
        DisplayDataInList(_billingData.PatientBills);
        if (_billingData.PatientBills == null)
        {
            _billingData.PatientBills = new List<PatientBillModel>();
            RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
        }
        ApplyTableHeaderText(_billingData.PatientBills.Count);
        _totalAmountValue.Text = TotalCalculator(_billingData.PatientBills);
        _discountValue.Text = DiscountCalculator(_billingData.PatientBills);
        _grossTotalValue.Text = GrossCalculator(_billingData.PatientBills);
    }

    private CustomDateTimeControl CreateDateControl(string resourceKey)
    {
        return new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateControl,
            ControlResourceKey = resourceKey,
            IsApplyHeightToError = false,
        };
    }

    private string GrossCalculator(List<PatientBillModel> patientBills)
    {
        double GrossTotal = 0;
        foreach (var x in patientBills)
        {
            GrossTotal = x.GrossTotal + GrossTotal;
        }
        return GrossTotal.ToString();
    }

    private string TotalCalculator(List<PatientBillModel> patientBills)
    {
        double TotalAmount = 0;
        foreach (var x in patientBills)
        {
            TotalAmount = x.TotalPaid + TotalAmount;
        }
        return TotalAmount.ToString();
    }

    private string DiscountCalculator(List<PatientBillModel> patientBills)
    {
        double Discount = 0;
        foreach (var x in patientBills)
        {
            Discount = x.Discount + Discount;
        }
        return Discount.ToString();
    }

    private void DisplayDataInList(List<PatientBillModel> patientBills)
    {
        CollectionViewField.ItemsSource = null;
        CollectionViewField.ItemsSource = new List<PatientBillModel>();
        if (_billingData.PatientBills != null)
        {
            CollectionViewField.ItemSizingStrategy = GenericMethods.GetPlatformSpecificValue(ItemSizingStrategy.MeasureFirstItem, ItemSizingStrategy.MeasureAllItems, default);
            CollectionViewField.ItemsSource = _billingData.PatientBills;
        }
    }

    private void DefineTable(double padding)
    {
        TabletHeader = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft);
        _mainLayout.Add(TabletHeader, 0, 4);
        SearchField = new CustomSearchControl
        {
            ControlResourceKey = ResourceConstants.R_SEARCH_TEXT_KEY,
            SearchedOn = SearchedOnType.Both,
            SearchOutSideViewHide = true,
            IsAppliedMargin = true
        };
        _mainLayout.Add(SearchField, 3, 4);
        Grid.SetColumnSpan(SearchField, 2);
        CollectionViewField = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
            ItemsLayout = ItemLayoutCreate(_billListCell.ArrangeHorizontal),
            Margin = new Thickness(0, _margin, 0, 0),
            ItemTemplate = new DataTemplate(() =>
            {
                var content = new ContentView
                {
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_SELECTED_CONTENT_STYLE],
                    Content = new ResponsiveView(_billListCell),
                };
                return content;
            }),
        };
        isHorizontal = _billListCell.ArrangeHorizontal;
        CellRowHeight = _billListCell.ArrangeHorizontal
        ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_DEFAULT_CARD_HEIGHT_STYLE], CultureInfo.InvariantCulture)
        : (double)_billListCell.IconSize + 2 * _margin + new OnIdiom<double> { Phone = _margin + 8, Tablet = 8 };
        _mainLayout?.Add(CollectionViewField, 0, 5);
        Grid.SetColumnSpan(CollectionViewField, 5);
    }
}