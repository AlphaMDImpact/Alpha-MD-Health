using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientBillDetailsView : BaseLibCollectionView
{
    //todo: private PDFToHtml PDFToHtml { get; set; }
    private readonly CustomImageControl _organisationLogo;
    private readonly CustomLabelControl _organisationName;
    private readonly CustomLabelControl _organisationDetails;
    private readonly CustomEntryControl _programName;
    private readonly CustomEntryControl _doctorName;
    private readonly CustomEntryControl _patientName;
    private readonly CustomEntryControl _billDate;
    private readonly CustomLabelControl _total;
    private readonly CustomLabelControl _discount;
    private readonly CustomLabelControl _amountPaid;
    private readonly CustomLabelControl _paymentMode;
    private readonly CustomLabelControl _totalValue;
    private readonly CustomLabelControl _discountValue;
    private readonly CustomLabelControl _amountPaidValue;
    private readonly CustomLabelControl _paymentModeValue;
    private readonly CustomLabelControl _itemHeader;
    private readonly CustomLabelControl _amountHeader;
    private readonly Grid _mainLayout;
    private readonly Grid _orgLayout;
    private readonly BillingItemDTO _billingsData = new BillingItemDTO { PatientBillItem = new PatientBillModel(), PatientBillItems = new List<PatientBillItemModel>() };
    private readonly string _listColor;

    /// <summary>
    /// Onsuccess save Event
    /// </summary>
    public event EventHandler<EventArgs> OnSaveSuccess;

    public PatientBillDetailsView(BasePage page, object parameters) : base(page, parameters)
    {
        if (parameters != null)
        {
            _listColor = GenericMethods.MapValueType<string>(GetParameterValue(nameof(PatientBillModel.ProgramColor)));
        }
        ParentPage.PageService = new PatientBillService(App._essentials);
        _organisationLogo = new CustomImageControl(AppImageSize.ImageSizeXL, AppImageSize.ImageSizeXL, string.Empty, ImageConstants.I_AVTAR_NEW_PNG, false)
        {
            VerticalOptions = LayoutOptions.StartAndExpand,
        };
        _organisationName = new CustomLabelControl(LabelType.PrimaryMediumLeft);
        _organisationDetails = new CustomLabelControl(LabelType.SecondrySmallLeft) { LineBreakMode = LineBreakMode.WordWrap };
        _programName = CreateDisabledEntry(ResourceConstants.R_PROGRAM_TITLE_KEY);
        _doctorName = CreateDisabledEntry(ResourceConstants.R_DOCTOR_NAME_KEY);
        _patientName = CreateDisabledEntry(ResourceConstants.R_PATIENT_KEY);
        _billDate = CreateDisabledEntry(ResourceConstants.R_ENTER_DATE_KEY);
        CustomCellModel customCellModel = new CustomCellModel
        {
            CellHeader = nameof(PatientBillItemModel.Name),
            CellRightContentHeader = nameof(PatientBillItemModel.Amount),
            NoMarginNoSeprator = true,
            RemoveLeftMargin = true,
            IsList = true,
            IsCellHeaderInBold = true,
            IsCellRightContentHeaderInBold = true,
            IsEnabled = false,
        };
        _itemHeader = new CustomLabelControl(LabelType.SecondrySmallLeft);
        _amountHeader = new CustomLabelControl(LabelType.SecondrySmallLeft) { HorizontalOptions = LayoutOptions.End, HorizontalTextAlignment = TextAlignment.End };
        _total = new CustomLabelControl(LabelType.SecondrySmallLeft);
        _discount = new CustomLabelControl(LabelType.SecondrySmallLeft);
        _amountPaid = new CustomLabelControl(LabelType.SecondrySmallLeft);
        _paymentMode = new CustomLabelControl(LabelType.SecondrySmallLeft);
        _totalValue = new CustomLabelControl(LabelType.PrimarySmallRight) { FontAttributes = FontAttributes.Bold };
        _discountValue = new CustomLabelControl(LabelType.PrimarySmallRight) { FontAttributes = FontAttributes.Bold };
        _amountPaidValue = new CustomLabelControl(LabelType.PrimarySmallRight) { FontAttributes = FontAttributes.Bold };
        _paymentModeValue = new CustomLabelControl(LabelType.PrimarySmallRight) { FontAttributes = FontAttributes.Bold };
        _mainLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            ColumnSpacing = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition {Width=new GridLength(1, GridUnitType.Star)},
                new ColumnDefinition {Width=new GridLength(1, GridUnitType.Star)}
            },
        };
        _orgLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            ColumnSpacing = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition {Width = GridLength.Auto},
                new ColumnDefinition {Width = GridLength.Star},
            }
        };

        _orgLayout.Add(_organisationLogo, 0, 0);
        Grid.SetRowSpan(_organisationLogo, 2);
        _orgLayout.Add(_organisationName, 1, 0);
        _orgLayout.Add(_organisationDetails, 1, 1);
        _mainLayout.Add(_orgLayout, 0, 0);
        Grid.SetColumnSpan(_orgLayout, 2);
        _mainLayout.Add(_programName, 0, 1);
        _mainLayout.Add(_doctorName, 1, 1);
        _mainLayout.Add(_patientName, 0, 2);
        _mainLayout.Add(_billDate, 1, 2);
        _mainLayout.Add(_itemHeader, 0, 3);
        _mainLayout.Add(_amountHeader, 1, 3);
        AddListView(_mainLayout, customCellModel, 0, 4, null, ListViewCachingStrategy.RetainElement, string.IsNullOrWhiteSpace(_listColor) ? StyleConstants.DEFAULT_BACKGROUND_COLOR : _listColor);
        Grid.SetColumnSpan(ListViewField, 2);
        ListViewField.IsEnabled = false;
        if (_listColor == StyleConstants.TRANSPARENT_COLOR_STRING)
        {
            ListViewField.BackgroundColor = Color.FromArgb(_listColor);
        }
        else
        {
            ListViewField.BackgroundColor = Color.FromArgb(StyleConstants.DEFAULT_BACKGROUND_COLOR);
        }
        ListViewField.SeparatorVisibility = SeparatorVisibility.Default;
        ListViewField.Margin = new Thickness(-15, 0);
        _mainLayout.Add(_total, 0, 5);
        _mainLayout.Add(_totalValue, 1, 5);
        _mainLayout.Add(_discount, 0, 6);
        _mainLayout.Add(_discountValue, 1, 6);
        _mainLayout.Add(_amountPaid, 0, 7);
        _mainLayout.Add(_amountPaidValue, 1, 7);
        _mainLayout.Add(_paymentMode, 0, 8);
        _mainLayout.Add(_paymentModeValue, 1, 8);
        SetPageContent(_mainLayout);
    }

    private CustomEntryControl CreateDisabledEntry(string key)
    {
        return new CustomEntryControl
        {
            ControlType = FieldTypes.TextEntryControl,
            ControlResourceKey = key,
            IsBackGroundTransparent = true,
            IsEnabled = false,
            IsBoldText = true,
        };
    }

    public async override Task LoadUIAsync(bool isRefreshRequest)
    {
        // isActive is For details Page
        _billingsData.IsBillDetailViews = true;
        _billingsData.PatientBillItem.PatientBillID = new Guid(GetParameterValue(nameof(PatientBillModel.PatientBillID)));

        _billingsData.SelectedUserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));
        await (ParentPage.PageService as PatientBillService).GetPatientBillingDataAsync(_billingsData).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_billingsData.ErrCode == ErrorCode.OK)
        {
            AssignControlValue();
        }
        else
        {
            RenderErrorView(_mainLayout, ErrorCode.NoInternetConnection.ToString(), false, (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
        }
    }

    private void AssignControlValue()
    {
        if (_billingsData.PatientBillItem.PatientBillID != Guid.Empty)
        {
            _organisationName.Text = _billingsData.PatientBillItem.OrganisationName;
            _organisationDetails.Text = _billingsData.PatientBillItem.OrganisationDetail;
            if (string.IsNullOrWhiteSpace(_billingsData.AddedBy))
            {
                _organisationLogo.DefaultValue = _billingsData.LastModifiedBy;
            }
            else
            {
                _organisationLogo.ImagePathSource = _billingsData.AddedBy;
            }
            _programName.PageResources = ParentPage.PageData;
            _doctorName.PageResources = ParentPage.PageData;
            _patientName.PageResources = ParentPage.PageData;
            _billDate.PageResources = ParentPage.PageData;
            _programName.Value = _billingsData.PatientBillItem.ProgramName;
            _doctorName.Value = _billingsData.PatientBillItem.ProviderName;
            _patientName.Value = _billingsData.PatientBillItem.PatientName;
            _billDate.Value = _billingsData.PatientBillItem.BillDateTimeString;
            _itemHeader.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ITEM_KEY);
            _amountHeader.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_AMOUNT_KEY);
            _total.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_TOTAL_AMOUNT_KEY);
            _totalValue.Text = Convert.ToString(_billingsData.PatientBillItem.GrossTotal, CultureInfo.InvariantCulture);
            _discount.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_DISCOUNT_KEY);
            _discountValue.Text = Convert.ToString(_billingsData.PatientBillItem.Discount, CultureInfo.InvariantCulture);
            _amountPaid.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_AMOUNT_PAID_KEY);
            _amountPaidValue.Text = Convert.ToString(_billingsData.PatientBillItem.TotalPaid, CultureInfo.InvariantCulture);
            _paymentMode.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_PAYMENT_MODE_NAME_KEY);
            _paymentModeValue.Text = _billingsData.PatientBillItem.PaymentMode;
            ListViewField.ItemsSource = _billingsData.PatientBillItems;
            ListViewField.HeightRequest = (double)AppImageSize.ImageSizeL * _billingsData.PatientBillItems.Count + 15;
        }
    }

    public override Task UnloadUIAsync()
    {
        return Task.CompletedTask;
    }

    private void CreatePDF(string html, string filename)
    {
        //todo: 
        //PDFToHtml = new PDFToHtml(filename);
        //this.BindingContext = PDFToHtml;
        //PDFToHtml.HTMLString = html;
    }
    internal async Task ShareButtonClicked()
    {
        var data = new PatientBillService(App._essentials).GetPrintData(_billingsData, _billingsData.AddedBy, _billingsData.PatientBillItem.OrganisationName);
        if (data.Item2 == ErrorCode.OK)
        {
            this.CreatePDF(data.Item1, data.Item3);
            //todo: PDFToHtml.GeneratePDF();
        }
        else
        {
            //Need to confirm when reviewing
            RenderErrorView(_mainLayout, ErrorCode.InvalidData.ToString(), false, (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
        }
    }
}

