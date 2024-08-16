using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PrescriptionView : BaseLibCollectionView
{
    private readonly CustomImageControl _organisationLogo;
    private readonly CustomLabelControl _organisationName;
    private readonly CustomLabelControl _organisationDetails;
    private readonly CustomLabelControl _patientName;
    private readonly CustomLabelControl _doctorName;
    private readonly CustomLabelControl _doctorDegree;
    private readonly CustomLabelControl _authorisedLabel;
    private readonly Grid _mainLayout;
    private readonly Grid _orgLayout;
    private readonly CustomListView _medicineListView;
    private readonly PatientMedicationDTO _medicationData;
    //todo: private PDFToHtml _pdfToHtml { get; set; }

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PrescriptionView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage = page;
        ParentPage.PageService = new MedicationSevice(App._essentials);
        var padding = (double)App.Current.Resources[StyleConstants.ST_APP_PADDING];
        _medicationData = new PatientMedicationDTO
        {
            Medication = new PatientMedicationModel(),
            Medications = new List<PatientMedicationModel>(),
            RecordCount = -2
        };
        _organisationLogo = new CustomImageControl(AppImageSize.ImageSizeXL, AppImageSize.ImageSizeXL, string.Empty, ImageConstants.I_AVTAR_NEW_PNG, false)
        {
            VerticalOptions = LayoutOptions.StartAndExpand,
        };
        _organisationName = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft);
        _organisationDetails = new CustomLabelControl(LabelType.SecondrySmallLeft) { LineBreakMode = LineBreakMode.WordWrap };
        _patientName = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft) { Padding = new Thickness(0, 10, 0, 0) };
        _doctorName = new CustomLabelControl(LabelType.PrimaryMediumLeft);
        _doctorDegree = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _authorisedLabel = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft);
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
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition {Width=new GridLength(1, GridUnitType.Star)},
                new ColumnDefinition {Width= GridLength.Auto}
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
        _medicineListView = new CustomListView()
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_LIST_KEY],
            ItemTemplate = new DataTemplate(() =>
            {
                return new PrescriptionViewCell();
            }),
        };
        _orgLayout.Add(_organisationLogo, 0, 0);
        Grid.SetRowSpan(_organisationLogo, 2);
        _orgLayout.Add(_organisationName, 1, 0);
        _orgLayout.Add(_organisationDetails, 1, 1);
        _mainLayout.Add(_orgLayout, 0, 0);
        Grid.SetColumnSpan(_orgLayout, 2);
        _mainLayout.Add(_patientName, 0, 1);
        _mainLayout.Add(_doctorName, 0, 2);
        _mainLayout.Add(_doctorDegree, 0, 3);
        _mainLayout.Add(_medicineListView, 0, 4);
        _mainLayout.Add(_authorisedLabel, 0, 5);
        SetPageContent(_mainLayout);
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public async override Task LoadUIAsync(bool isRefreshRequest)
    {
        _medicationData.Medication.PatientMedicationID = new Guid(GetParameterValue(nameof(PatientMedicationModel.PatientMedicationID)));
        _medicationData.IsPrescriptionView = true;
        await (ParentPage.PageService as MedicationSevice).GetMedicationsAsync(_medicationData).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_medicationData.ErrCode == ErrorCode.OK)
        {
            if (string.IsNullOrWhiteSpace(_medicationData.AddedBy))
            {
                _organisationLogo.DefaultValue = _medicationData.LastModifiedBy;
            }
            else
            {
                _organisationLogo.ImagePathSource = _medicationData.AddedBy;
            }
            _organisationName.Text = _medicationData.Organisation.OrganisationName;
            _organisationDetails.Text = _medicationData.Medication.OrganisationDetail;
            _patientName.Text = _medicationData.Medication.PatientName;
            if (_medicationData.Caregiver != null)
            {
                _doctorName.Text = _medicationData.Caregiver.FirstName;
                _authorisedLabel.Text = _medicationData.Caregiver.FirstName + Environment.NewLine + _medicationData.Caregiver.DateStyle;
                _doctorDegree.Text = _medicationData.Medication.DoctorNameDisplayString;
            }
            _medicineListView.ItemsSource = _medicationData.Medications;
            _medicineListView.HeightRequest = _medicationData.Medications.Count * 300;
            _medicineListView.InputTransparent = true;
        }
    }

    /// <summary>
    /// Unregister events of View
    /// </summary>
    public async override Task UnloadUIAsync()
    {
        await Task.CompletedTask;
    }

    private void CreatePDF(string html, string filename)
    {
        //todo: 
        //_pdfToHtml = new PDFToHtml(filename);
        //this.BindingContext = _pdfToHtml;
        //_pdfToHtml.HTMLString = html;
    }

    /// <summary>
    /// Share medication Data
    /// </summary>
    /// <returns>Operation Status</returns>
    public async Task ShareButtonClicked()
    {
        var data = new MedicationSevice(App._essentials).GetPrintData(_medicationData, _medicationData.AddedBy, _medicationData.Organisation.OrganisationName);
        if (data.Item2 == ErrorCode.OK)
        {
            this.CreatePDF(data.Item1, data.Item3);
            //todo: _pdfToHtml.GeneratePDF();
        }
        else
        {
            RenderErrorView(_mainLayout, ErrorCode.InvalidData.ToString(), false, (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
        }
    }
}