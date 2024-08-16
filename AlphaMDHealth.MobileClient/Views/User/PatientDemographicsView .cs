using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Demographics View
/// </summary>
public class PatientDemographicsView : ViewManager
{
    private readonly CustomLabelControl _pageHeader;
    private readonly CustomLabelControl _firstName;
    private readonly CustomLabelControl _patientFirstName;
    private readonly CustomLabelControl _middleName;
    private readonly CustomLabelControl _patientMiddleName;
    private readonly CustomLabelControl _lastName;
    private readonly CustomLabelControl _patientLastName;
    private readonly CustomLabelControl _age;
    private readonly CustomLabelControl _patientAge;
    private readonly CustomLabelControl _dob;
    private readonly CustomLabelControl _patientDOB;
    private readonly CustomLabelControl _bloodGroup;
    private readonly CustomLabelControl _patientBloodGroup;
    private readonly CustomLabelControl _gender;
    private readonly CustomLabelControl _patientGender;
    private readonly CustomLabelControl _phoneNumber;
    private readonly CustomLabelControl _patientPhoneNumber;
    private readonly CustomLabelControl _socialSecurityNumber;
    private readonly CustomLabelControl _patientSocialSecurityNumber;
    private readonly CustomLabelControl _internalCode;
    private readonly CustomLabelControl _patientInternalCode;
    private readonly CustomLabelControl _patientExternalCode;
    private readonly CustomLabelControl _externalCode;
    private readonly BasePage _parentPage;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PatientDemographicsView(BasePage page, object parameters) : base(page, parameters)
    {
        _parentPage = page;
        double layoutPadding = Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.InvariantCulture);
        _pageHeader = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft)
        {
            Margin = new Thickness(0, layoutPadding / 3, 0, layoutPadding * 0.5),
        };
        _firstName = new CustomLabelControl(LabelType.SecondrySmallLeft)
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
        };
        _patientFirstName = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _middleName = new CustomLabelControl(LabelType.SecondrySmallLeft)
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
        };
        _patientMiddleName = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _lastName = new CustomLabelControl(LabelType.SecondrySmallLeft)
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
        };
        _patientLastName = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _age = new CustomLabelControl(LabelType.SecondrySmallLeft)
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
        };
        _patientAge = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _dob = new CustomLabelControl(LabelType.SecondrySmallLeft)
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
        };
        _patientDOB = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _bloodGroup = new CustomLabelControl(LabelType.SecondrySmallLeft)
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
        };
        _patientBloodGroup = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _gender = new CustomLabelControl(LabelType.SecondrySmallLeft)
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
        };
        _patientGender = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _phoneNumber = new CustomLabelControl(LabelType.SecondrySmallLeft)
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
        };
        _patientPhoneNumber = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _socialSecurityNumber = new CustomLabelControl(LabelType.SecondrySmallLeft)
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
        };
        _patientSocialSecurityNumber = new CustomLabelControl(LabelType.PrimarySmallLeft);

        _internalCode = new CustomLabelControl(LabelType.SecondrySmallLeft)
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
        };
        _patientInternalCode = new CustomLabelControl(LabelType.PrimarySmallLeft);

        _externalCode = new CustomLabelControl(LabelType.SecondrySmallLeft)
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
        };
        _patientExternalCode = new CustomLabelControl(LabelType.PrimarySmallLeft);
       

        var mainLayout = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto  },
                new RowDefinition { Height = (double)App.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT] },
                new RowDefinition { Height = GridLength.Auto  },
                new RowDefinition { Height = (double)App.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT] },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = (double)App.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT] },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = (double)App.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT] },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = (double)App.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT] },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = (double)App.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT] },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = (double)App.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT] },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = (double)App.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT] },
                new RowDefinition { Height = GridLength.Auto },
                 new RowDefinition { Height = (double)App.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT] },
                new RowDefinition { Height = GridLength.Auto },
                 new RowDefinition { Height = (double)App.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT] },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
            }
        };
        mainLayout.Add(_pageHeader, 0, 0);
        mainLayout.Add(_firstName, 0, 1);
        mainLayout.Add(_patientFirstName, 1, 1);
        CreateSepratorView(mainLayout, 0, 2);

        mainLayout.Add(_middleName, 0, 3);
        mainLayout.Add(_patientMiddleName, 1, 3);
        CreateSepratorView(mainLayout, 0, 4);

        mainLayout.Add(_lastName, 0, 5);
        mainLayout.Add(_patientLastName, 1, 5);
        CreateSepratorView(mainLayout, 0, 6);

        mainLayout.Add(_age, 0, 7);
        mainLayout.Add(_patientAge, 1, 7);
        CreateSepratorView(mainLayout, 0, 8);

        mainLayout.Add(_dob, 0, 9);
        mainLayout.Add(_patientDOB, 1, 9);
        CreateSepratorView(mainLayout, 0, 10);

        mainLayout.Add(_bloodGroup, 0, 11);
        mainLayout.Add(_patientBloodGroup, 1, 11);
        CreateSepratorView(mainLayout, 0, 12);

        mainLayout.Add(_gender, 0, 13);
        mainLayout.Add(_patientGender, 1, 13);
        CreateSepratorView(mainLayout, 0, 14);

        mainLayout.Add(_phoneNumber, 0, 15);
        mainLayout.Add(_patientPhoneNumber, 1, 15);
        CreateSepratorView(mainLayout, 0, 16);

        mainLayout.Add(_socialSecurityNumber, 0, 17);
        mainLayout.Add(_patientSocialSecurityNumber, 1, 17);
        CreateSepratorView(mainLayout, 0, 18);

        mainLayout.Add(_internalCode, 0, 19);
        mainLayout.Add(_patientInternalCode, 1, 19);
        CreateSepratorView(mainLayout, 0, 20);

        mainLayout.Add(_externalCode, 0, 21);
        mainLayout.Add(_patientExternalCode, 1, 21);
        CreateSepratorView(mainLayout, 0, 22);
        Content = new ScrollView { Content = mainLayout };
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        UserDTO patientData = new UserDTO
        {
            User = new UserModel
            {
                UserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID))),
                RoleID = (int)RoleName.Patient
            }
        };
        var userService = new UserService(App._essentials);
        //await userService.GetUserAsync(patientData, false, false).ConfigureAwait(true);//todo:
        ParentPage.PageData = userService.PageData;
        if (patientData.ErrCode == ErrorCode.OK && patientData.User != null)
        {
            _pageHeader.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_PROFILE_HEADER_KEY);
            _firstName.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_FIRST_NAME_KEY);
            _patientFirstName.Text = patientData.User.FirstName;
            _middleName.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_MIDDLE_NAME_KEY);
            _patientMiddleName.Text = patientData.User.MiddleName;
            _lastName.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_LAST_NAME_KEY);
            _patientLastName.Text = patientData.User.LastName;
            _age.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_AGE_KEY);
            _patientAge.Text = patientData.User.UserAge.ToString(CultureInfo.InvariantCulture);
            _dob.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_DATE_OF_BIRTH_KEY);
            _patientDOB.Text = patientData.User.DateOfBirth;
            _bloodGroup.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_BLOOD_GROUP_KEY);
            _patientBloodGroup.Text = _parentPage.GetResourceByKeyID(patientData.User.BloodGroupID)?.ResourceValue;


            _gender.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_GENDER_KEY);
            _patientGender.Text = _parentPage.GetResourceByKey(patientData.User.GenderID)?.ResourceValue;
            _phoneNumber.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_MOBILE_NUMBER_KEY);
            _patientPhoneNumber.Text = patientData.User.PhoneNo.ToString(CultureInfo.InvariantCulture);
            _socialSecurityNumber.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_SOCIAL_SECURITY_NUMBER_KEY);
            _patientSocialSecurityNumber.Text = patientData.User.SocialSecurityNo;

            _internalCode.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_INTERNAL_CODE_KEY);
            _patientInternalCode.Text = patientData.User.GeneralMedicalIdenfier;

            _externalCode.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_EXTERNAL_CODE_KEY);
            _patientExternalCode.Text = patientData.User.HospitalIdenfier;
        }
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        await Task.CompletedTask;
    }

    private void CreateSepratorView(Grid mainLayout, int left, int top)
    {
        var seprator = new BoxView
        {
            HeightRequest = 1,
            BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE]
        };
        mainLayout?.Add(seprator, left, top);
        Grid.SetColumnSpan(seprator, 2);
    }
}