using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientProgramView : ViewManager
{
    private readonly CustomLabelControl _headerLabel;
    private readonly CustomCheckBoxListControl _programOptions;
    private readonly CustomButtonControl _actionButton;
    internal PatientProgramDTO _programData;
    private int _roleID;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PatientProgramView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new PatientProgramService(App._essentials);
        _roleID = App._essentials.GetPreferenceValue(StorageConstants.PR_ROLE_ID_KEY, 0);
        var padding = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
        _headerLabel = new CustomLabelControl(LabelType.BeforeLoginHeaderWithNoTopMargin)
        {
            IsVisible = false
        };
        _programOptions = new CustomCheckBoxListControl
        {
            CheckBoxType = new OnIdiom<ListStyleType>
            {
                Phone = ListStyleType.SeperatorView,
                Tablet = ListStyleType.BoxView
            },
            ApplyMargin = _roleID != (int)RoleName.Patient,
            Margin = new Thickness(0, 0, 0, padding),
        };
        _actionButton = new CustomButtonControl(ButtonType.PrimaryWithMargin)
        {
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, padding),
        };
        _actionButton.SetIsDisableButton(true);
        ParentPage.AddRowColumnDefinition(GridLength.Auto, 1, true);
        ParentPage.AddRowColumnDefinition(GridLength.Star, 1, true);
        ParentPage.AddRowColumnDefinition(GridLength.Auto, 1, true);
        if (_roleID == (int)RoleName.Patient)
        {
            SetPageContent(ParentPage.PageLayout);
        }
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns view with loaded data</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        ParentPage.PageLayout.Children?.Clear();
        _programData = new PatientProgramDTO
        {
            Programs = new List<ProgramModel>(),
            // Note : IsActive is used to store IsBeforeLogin flag data
            IsActive = GenericMethods.MapValueType<bool>(GetParameterValue(nameof(ProgramDTO.IsActive))),
            RecordCount = _roleID == (int)RoleName.Patient ? -2 : -1
        };
        _programData.SelectedUserID = _programData.RecordCount == -2
            ? App._essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0)
            : App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        await (ParentPage.PageService as PatientProgramService).GetPatientProgramsAsync(_programData).ConfigureAwait(true);
        ParentPage.PageData = (ParentPage.PageService as PatientProgramService).PageData;
        await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            //this indicate the the page is before login
            if (_programData.IsActive)
            {
                SetBeforeLoginContent();
            }
            else
            {
                if (MobileConstants.IsTablet)
                {
                    _programOptions.Margin = new Thickness(0, (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING]);
                }
            }
            if (GenericMethods.IsListNotEmpty(_programData.Programs))
            {
                _programOptions.RightViewes.Clear();
                foreach (var a in from program in _programData.Programs
                                  select new
                                  {
                                      Key = program.ProgramID,
                                      Value = CreateColorView(program) as View
                                  })
                {
                    if (_programOptions.RightViewes.ContainsKey(a.Key))
                    {
                        _programOptions.RightViewes[a.Key] = a.Value;
                    }
                    else
                    {
                        _programOptions.RightViewes.Add(a.Key, a.Value);
                    }
                }

                _programOptions.SetOptions((from program in _programData.Programs
                                            select new OptionSelectModel
                                            {
                                                OptionID = program.ProgramID,
                                                Value = program.ProgramID.ToString(CultureInfo.InvariantCulture),
                                                DisplayText = program.Name,
                                                IsSelected = program.IsActive,
                                                IsEnabled = (_programData.IsPatientAllowedForProgramSelection && program.AllowSelfRegistration) || IsPatientPage()
                                            }).ToList());
                ParentPage.PageLayout.Add(_programOptions, 0, 1);
                return;
            }
        }
        ParentPage.PageLayout.Add(new CustomMessageControl(false)
        {
            PageResources = ParentPage.PageData,
            VerticalOptions = LayoutOptions.CenterAndExpand,
            ControlResourceKey = _programData.ErrCode == ErrorCode.OK
                ? ResourceConstants.R_NO_DATA_FOUND_KEY
                : _programData.ErrCode.ToString()
        }, 0, 1);
    }

    /// <summary>
    /// unregister event of views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        _actionButton.Clicked -= OnNextButtonClicked;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Save and handle navigation
    /// </summary>
    /// <returns>Result of operations</returns>
    public async Task<bool> SaveButtonClickedAsync()
    {
        if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            AppHelper.ShowBusyIndicator = true;
            _programData.PatientPrograms = (from x in _programData.Programs
                                            let isActive = _programOptions.SelectedIndexValues.Contains(x.ProgramID.ToString(CultureInfo.InvariantCulture))
                                            select new PatientProgramModel
                                            {
                                                PatientProgramID = x.PatientProgramID,
                                                ProgramID = x.ProgramID,
                                                PatientID = _programData.SelectedUserID,
                                                ProgramGroupIdentifier = x.ProgramGroupIdentifier,
                                                ProgramEntryPoint = string.IsNullOrWhiteSpace(x.ProgramEntryPoint)
                                                    ? Constants.CONSTANT_ZERO
                                                    : x.ProgramEntryPoint,
                                                IsActive = isActive,
                                                IsSynced = (isActive == x.IsActive) && !x.IsColorChanged,
                                                AddedON = GenericMethods.GetUtcDateTime,
                                                LastModifiedON = GenericMethods.GetUtcDateTime,
                                            }).ToList();
            if (_programData.PatientPrograms.Any(x => x.IsActive))
            {
                await (ParentPage.PageService as PatientProgramService).SavePatientProgramsAsync(_programData).ConfigureAwait(true);
                if (_programData.ErrCode == ErrorCode.OK)
                {
                    await SyncAndNavigateAsync();
                    return true;
                }
                else
                {
                    DisplayError(_programData.ErrCode.ToString());
                }
            }
            else
            {
                DisplayError(ResourceConstants.R_PROGRAM_SELECTION_MANDATORY_KEY);
            }
        }
        return false;
    }

    private async Task SyncAndNavigateAsync()
    {
        await ParentPage.NavigateOnNextPageAsync(false, _programData.IsActive, LoginFlow.PatientProgramPage).ConfigureAwait(true);
    }

    private void SetBeforeLoginContent()
    {
        ParentPage.SetPageLayoutOption(new OnIdiom<LayoutOptions>
        {
            Phone = LayoutOptions.FillAndExpand,
            Tablet = _programData?.RecordCount == -2
                ? LayoutOptions.CenterAndExpand
                : LayoutOptions.FillAndExpand
        }, false);
        ParentPage.PageLayout.Add(_headerLabel, 0, 0);
        ParentPage.PageLayout.Add(_actionButton, 0, 2);
        _headerLabel.Text = ParentPage.GetFeatureValueByCode(AppPermissions.PatientProgramAddEdit.ToString());
        _headerLabel.IsVisible = true;
        _actionButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_NEXT_ACTION_KEY);
        _actionButton.SetIsDisableButton(false);
        _actionButton.Clicked += OnNextButtonClicked;
    }

    private async void OnNextButtonClicked(object sender, EventArgs e)
    {
        _actionButton.Clicked -= OnNextButtonClicked;
        await SaveButtonClickedAsync().ConfigureAwait(true);
        _actionButton.Clicked += OnNextButtonClicked;
    }

    private BoxView CreateColorView(ProgramModel program)
    {
        const double size = (double)AppImageSize.ImageSizeD;
        var colorView = new BoxView
        {
            StyleId = program.ProgramID.ToString(CultureInfo.InvariantCulture),
            BackgroundColor = program.ProgramGroupIdentifier == null ? (default) : Color.FromArgb(program.ProgramGroupIdentifier),
            HeightRequest = size,
            WidthRequest = size,
            CornerRadius = size / 2,
            VerticalOptions = LayoutOptions.Center,
            FlowDirection = (FlowDirection)App.Current.Resources[StyleConstants.ST_FLOW_DIRECTION]
        };
        if (_programData.RecordCount == -2 && !_programData.IsActive)
        {
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnColorTapped;
            colorView.GestureRecognizers.Add(tapGestureRecognizer);
        }
        return colorView;
    }

    private async void OnColorTapped(object sender, EventArgs e)
    {
        var selectedId = Convert.ToInt64((sender as BoxView).StyleId, CultureInfo.InvariantCulture);
        var selectedProgram = _programData.Programs.First(x => x.ProgramID == selectedId);
        if (_programOptions.SelectedIndexValues.Contains(selectedProgram.ProgramID.ToString()))
        {
            CustomColorPickerPopupPage colorPickerPage = new CustomColorPickerPopupPage(selectedProgram, ParentPage);
            colorPickerPage.OnSaveButtonClicked += ColorPickerPage_OnSaveButtonClicked;
            //todo:await Navigation.PushPopupAsync(colorPickerPage).ConfigureAwait(true);
        }
    }

    private void ColorPickerPage_OnSaveButtonClicked(object sender, EventArgs e)
    {
        ProgramModel program = (sender as ProgramModel);
        _programData.Programs.First(x => x.ProgramID == program.ProgramID).ProgramGroupIdentifier = program.ProgramGroupIdentifier;
        _programData.Programs.First(x => x.ProgramID == program.ProgramID).IsColorChanged = true;
        _programOptions.RightViewes[program.ProgramID].BackgroundColor = Color.FromArgb(program.ProgramGroupIdentifier);
    }

    private void DisplayError(string key)
    {
        AppHelper.ShowBusyIndicator = false;
        ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(key));
    }
}