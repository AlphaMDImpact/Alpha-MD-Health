using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class CaregiverView : ViewManager
{
    private readonly CaregiverDTO _caregiverData = new CaregiverDTO { Caregiver = new CaregiverModel(), RecordCount = -11 };
    private readonly CustomLabelControl _organisation;
    private readonly CustomLabelControl _organisationValue;
    private readonly CustomLabelControl _caregiver;
    private readonly CustomLabelControl _caregiverValue;
    private readonly CustomLabelControl _program;
    private readonly CustomLabelControl _programValue;
    private readonly CustomLabelControl _fromDate;
    private readonly CustomLabelControl _fromDateValue;
    private readonly CustomLabelControl _endDate;
    private readonly CustomLabelControl _endDateValue;
    private readonly CustomLabelControl _profession;
    private readonly CustomLabelControl _professionValue;

    public CaregiverView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new UserService(App._essentials);

        _organisation = CreateTitleLabel();
        _organisationValue = CreateValueLabel();
        _caregiver = CreateTitleLabel();
        _caregiverValue = CreateValueLabel();
        _professionValue = CreateValueLabel();
        _profession = CreateTitleLabel();
        _program = CreateTitleLabel();
        _programValue = CreateValueLabel();
        _fromDate = CreateTitleLabel();
        _fromDateValue = CreateValueLabel();
        _endDate = CreateTitleLabel();
        _endDateValue = CreateValueLabel();

        Grid bodyGrid = new Grid
        {
            Style = DeviceInfo.Idiom == DeviceIdiom.Tablet
                ? (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE]
                : (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
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
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
            }
        };
        bodyGrid.Add(_organisation, 0, 0);
        bodyGrid.Add(_organisationValue, 0, 1);
        bodyGrid.Add(_caregiver, 0, 2);
        bodyGrid.Add(_caregiverValue, 0, 3);
        bodyGrid.Add(_profession, 0, 4);
        bodyGrid.Add(_professionValue, 0, 5);
        bodyGrid.Add(_program, 0, 6);
        bodyGrid.Add(_programValue, 0, 7);
        bodyGrid.Add(_fromDate, 0, 8);
        bodyGrid.Add(_fromDateValue, 0, 9);
        bodyGrid.Add(_endDate, 0, 10);
        bodyGrid.Add(_endDateValue, 0, 11);
        ParentPage.PageLayout.Add(bodyGrid, 0, 0);
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        if (isPatientData)
        {
            SetPageContent(ParentPage.PageLayout);
        }
    }

    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        _caregiverData.Caregiver.PatientCareGiverID = Convert.ToInt64(GenericMethods.MapValueType<long>(GetParameterValue(nameof(CaregiverModel.PatientCareGiverID))));
        await (ParentPage.PageService as UserService).GetCaregiverDetailsAsync(_caregiverData).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_caregiverData.ErrCode == ErrorCode.OK)
        {
            AssignControlResources();
        }
    }

    public override async Task UnloadUIAsync()
    {
        await Task.CompletedTask;
    }

    private void AssignControlResources()
    {
        _organisation.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ORGANISATION_KEY);
        if (_caregiverData.Caregiver.Department != null)
        {
            _organisationValue.Text = _caregiverData.Caregiver.Department.ToString();
        }
        _caregiver.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_CAREGIVER_KEY);
        _caregiverValue.Text = _caregiverData.Caregiver.FirstName.ToString();
        _fromDate.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_START_DATE_KEY);
        _fromDateValue.Text = _caregiverData.Caregiver.FromDateValue;
        _endDate.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_END_DATE_KEY);
        _endDateValue.Text = _caregiverData.Caregiver.ToDateValue;
        _profession.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_PROFESSION_KEY);
        _professionValue.Text = _caregiverData.Caregiver.RoleName;
        if (_caregiverData.Caregiver.ProgramID > 0)
        {
            _program.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_PROGRAM_TITLE_KEY);
            _programValue.Text = _caregiverData.Caregiver.ProgramName?.ToString();
            _program.IsVisible = _programValue.IsVisible = true;
        }
        else
        {
            _program.IsVisible = _programValue.IsVisible = false;
        }
    }

    private CustomLabelControl CreateTitleLabel()
    {
        return new CustomLabelControl(LabelType.SecondrySmallLeft);
    }

    private CustomLabelControl CreateValueLabel()
    {
        return new CustomLabelControl(LabelType.PrimarySmallLeft)
        {
            Margin = new Thickness(0, 10, 0, Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture))
        };
    }
}