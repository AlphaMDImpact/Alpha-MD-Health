using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient Medication View to add, edit and view medication data
/// </summary>
public class PatientMedicationView : ViewManager, IDisposable
{
    private readonly CustomLabelControl _medicationDetailHeaderLabel;
    private readonly CustomEntryControl _medicineNameEntry;
    private readonly CustomEntryControl _medicationDoseUnitEntry;
    private readonly CustomBindablePickerControl _medicationDoseUnitPicker;
    private CustomCheckBoxListControl _medicationFrequencyEntryMultipleCheckBox = null;
    private readonly CustomCheckBoxListControl _medicationAdditionalNotesMultipleCheckBox;
    private readonly CustomCheckBox _medicationIsCritical;
    private readonly CustomBindablePickerControl _medicationIntakePicker;
    private readonly CustomEntryControl _medicationAlternateDaysEntry;
    private readonly CustomDateTimeControl _medicationFromDatePicker;
    private readonly CustomDateTimeControl _medicationToDatePicker;
    private readonly CustomMultiLineEntryControl _medicationNoteEntry;
    private readonly CustomEntryControl _medicationAddedByEntry;
    private readonly CustomLabelControl _medicationReminderHeaderLabel;
    private readonly CustomButtonControl _viewPrescription;
    private readonly Switch _medicationSetReminderSwitch;
    private readonly Grid _reminderGrid;
    private readonly PatientMedicationDTO _medicationData;
    private readonly MedicinesSearchPopupPage _medicineSearchPage;
    private readonly bool _isNotPatientPage;
    private bool _isCriticalChecked;
    private PrescriptionPage _prescriptionView;

    /// <summary>
    /// Is Read Only property
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// DisplayStatus
    /// </summary>
    public event EventHandler<EventArgs> OnDisplayStatus;

    /// <summary>
    /// Parameterized constructor containing page inance
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PatientMedicationView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new MedicationSevice(App._essentials);
        _medicationData = new PatientMedicationDTO
        {
            Medication = new PatientMedicationModel(),
            RecordCount = -1
        };
        _isNotPatientPage = !IsPatientPage();
        var padding = (double)App.Current.Resources[StyleConstants.ST_APP_PADDING];
        _medicationDetailHeaderLabel = new CustomLabelControl(LabelType.HeaderPrimaryMediumBoldLeftWithoutPadding)
        {
            Margin = new Thickness(0, 0, 0, padding),
            BackgroundColor = _isNotPatientPage
                ? (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR]
                : Color.FromArgb(StyleConstants.GENERIC_BACKGROUND_COLOR),
        };
        _medicineNameEntry = new CustomEntryControl
        {
            ControlResourceKey = ResourceConstants.R_MEDICINE_NAME_KEY,
            ControlIcon = ImageConstants.I_SEARCH_ICON_PNG,
            ShowHeader = true,
            IsUnderLine = true,
            IsBackGroundTransparent = _isNotPatientPage
        };
        _medicineNameEntry.OnAdvanceEntryUnfocused += OnMedicineChanged;
        _medicationDoseUnitEntry = new CustomEntryControl
        {
            ControlType = FieldTypes.DecimalEntryControl,
            ControlResourceKey = ResourceConstants.R_DOSES_KEY,
            IsBackGroundTransparent = _isNotPatientPage
        };
        _medicationDoseUnitPicker = new CustomBindablePickerControl
        {
            ControlResourceKey = ResourceConstants.R_UNIT_KEY,
            IsBackGroundTransparent = _isNotPatientPage,
            IsEnabled = false
        };
        _medicationFrequencyEntryMultipleCheckBox = new CustomCheckBoxListControl(true)
        {
            ShowHeader = true,
            ControlResourceKey = ResourceConstants.R_FREQUENCY_KEY,
            CheckBoxType = ListStyleType.HorizontalView,
        };
        _medicationIntakePicker = new CustomBindablePickerControl
        {
            ControlResourceKey = ResourceConstants.R_HOW_OFTEN_KEY,
            IsBackGroundTransparent = _isNotPatientPage
        };
        _medicationAlternateDaysEntry = new CustomEntryControl
        {
            ControlType = FieldTypes.NumericEntryControl,
            ControlResourceKey = ResourceConstants.R_ALTERNATE_FOR_TEXT_KEY,
            IsBackGroundTransparent = _isNotPatientPage
        };
        _medicationFromDatePicker = new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateControl,
            IsApplyHeightToError = false,
            ControlResourceKey = ResourceConstants.R_MEDICATION_START_DATE_KEY,
            IsBackGroundTransparent = _isNotPatientPage
        };
        _medicationToDatePicker = new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateControl,
            IsApplyHeightToError = false,
            ControlResourceKey = ResourceConstants.R_MEDICATION_END_DATE_KEY,
            IsBackGroundTransparent = _isNotPatientPage
        };
        _medicationIsCritical = new CustomCheckBox()
        {
            Margin = new Thickness(Device.RuntimePlatform == Device.iOS ? (DeviceInfo.Idiom == DeviceIdiom.Phone ? 10 : (DeviceInfo.Idiom == DeviceIdiom.Tablet ? 30 : 0))
                        : 0, 0, 0, padding),
            Style = Device.RuntimePlatform == Device.iOS ? (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_CHECKBOX_KEY] : null,
        };
        _medicationAdditionalNotesMultipleCheckBox = new CustomCheckBoxListControl(true)
        {
            ShowHeader = true,
            ControlResourceKey = ResourceConstants.R_NOTE_TEXT_KEY,
            CheckBoxType = ListStyleType.HorizontalView,
        };
        _medicationNoteEntry = new CustomMultiLineEntryControl
        {
            ControlResourceKey = ResourceConstants.R_ADDITIONAL_NOTE_TEXT_KEY,
            EditorHeightRequest = Device.RuntimePlatform == Device.iOS ? (DeviceInfo.Idiom == DeviceIdiom.Phone ? EditorHeight.IOSMOBILE : EditorHeight.IOSTablet) : EditorHeight.Notes,
            IsBackGroundTransparent = true,
        };
        _medicationAddedByEntry = new CustomEntryControl
        {
            ControlType = FieldTypes.TextEntryControl,
            ControlResourceKey = ResourceConstants.R_ADDED_BY_KEY,
            IsEnabled = false,
            IsBackGroundTransparent = _isNotPatientPage
        };
        _medicationReminderHeaderLabel = new CustomLabelControl(LabelType.HeaderPrimaryMediumBoldLeftWithoutPadding)
        {
            Margin = new Thickness(0, 0, 0, padding),
        };
        _medicationSetReminderSwitch = new Switch
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_TOGGLE_KEY],

        };
        _reminderGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star}
            }
        };
        _viewPrescription = new CustomButtonControl(ButtonType.TabButton) { IsVisible = false, VerticalOptions = LayoutOptions.StartAndExpand, HeightRequest = 50 };
        _medicineSearchPage = new MedicinesSearchPopupPage(_medicationData);
        _medicineSearchPage.OnSelectMedicineItem += OnSelectMedicine;
        _medicineSearchPage.OnCloseSearchMedicine += OnCloseSearchMedicine;
        App._essentials.SetPreferenceValue(StorageConstants.PR_CONTROL_WIDTH_KEY, (App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, 0.0) / 2) - 30);
        ParentPage.AddRowColumnDefinition(GridLength.Auto, 24, true);
        ParentPage.AddRowColumnDefinition(GridLength.Star, 1, false);
        ParentPage.PageLayout.Add(_medicationDetailHeaderLabel, 0, 0);
        Grid.SetColumnSpan(_medicationDetailHeaderLabel, 2);
        ParentPage.PageLayout.Add(_medicineNameEntry, 0, 1);
        Grid.SetColumnSpan(_medicineNameEntry, 4);
        ParentPage.PageLayout.Add(_medicationDoseUnitEntry, 0, 2);
        Grid.SetColumnSpan(_medicationDoseUnitEntry, 3);
        ParentPage.PageLayout.Add(_medicationDoseUnitPicker, 3, 2);
        Grid.SetColumnSpan(_medicationDoseUnitPicker, 1);
        ParentPage.PageLayout.Add(_medicationFrequencyEntryMultipleCheckBox, 0, 3);
        Grid.SetColumnSpan(_medicationFrequencyEntryMultipleCheckBox, 4);
        ParentPage.PageLayout.Add(_medicationIntakePicker, 0, 5);
        Grid.SetColumnSpan(_medicationIntakePicker, 4);
        ParentPage.PageLayout.Add(_medicationFromDatePicker, 0, 7);
        Grid.SetColumnSpan(_medicationFromDatePicker, 2);
        ParentPage.PageLayout.Add(_medicationToDatePicker, 2, 7);
        Grid.SetColumnSpan(_medicationToDatePicker, 2);
        ParentPage.PageLayout.Add(_medicationIsCritical, 0, 8);
        Grid.SetColumnSpan(_medicationIsCritical, 4);
        ParentPage.PageLayout.Add(_medicationAdditionalNotesMultipleCheckBox, 0, 10);
        Grid.SetColumnSpan(_medicationAdditionalNotesMultipleCheckBox, 4);
        ParentPage.PageLayout.Add(_medicationNoteEntry, 0, 11);
        Grid.SetColumnSpan(_medicationNoteEntry, 4);
        ParentPage.PageLayout.Add(_medicationReminderHeaderLabel, 0, 13);
        Grid.SetColumnSpan(_medicationReminderHeaderLabel, 3);
        ParentPage.PageLayout.Add(_medicationSetReminderSwitch, 3, 13);
        ParentPage.PageLayout.Add(_viewPrescription, 0, 15);
        Grid.SetColumnSpan(_viewPrescription, 4);
        ParentPage.PageLayout.Margin = new Thickness(MobileConstants.IsTablet && _isNotPatientPage ? 0 : padding, 0);
        ParentPage.PageLayout.Padding = new Thickness(0);
        if (MobileConstants.IsTablet && _isNotPatientPage)
        {
            Content = new ScrollView { Content = ParentPage.PageLayout };
        }
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Operation status</returns>
    public async override Task LoadUIAsync(bool isRefreshRequest)
    {
        if (!isRefreshRequest)
        {
            string medicationId = GenericMethods.MapValueType<string>(GetParameterValue(nameof(PatientMedicationModel.PatientMedicationID)));
            _medicationData.Medication.PatientMedicationID = string.IsNullOrWhiteSpace(medicationId)
                ? Guid.Empty
                : new Guid(medicationId);
        }
        await (ParentPage.PageService as MedicationSevice).GetMedicationsAsync(_medicationData).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_medicationData.ErrCode == ErrorCode.OK)
        {
            var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
            bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
            if (_medicationData.Medication.PatientMedicationID != Guid.Empty)
            {
                ParentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_START_DATE_KEY).MinLength = GetMinDate(ResourceConstants.R_START_DATE_KEY);
                ParentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_END_DATE_KEY).MinLength = GetMinDate(ResourceConstants.R_END_DATE_KEY);
            }
            _medicationDetailHeaderLabel.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_MEDICATION_HEADER_KEY);
            _viewPrescription.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_VIEW_PRESCRIPTION_KEY);
            _medicationIsCritical.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_IS_CRITICAL_KEY);

            _medicationIsCritical.CheckedChanged += isCritical_CheckedChanged;
            _isCriticalChecked = _medicationData.Medication.IsCritical;
            _medicationIsCritical.IsChecked = _medicationData.Medication.IsCritical;
            _medicationIsCritical.Color = _medicationIsCritical.TextColor = _medicationData.Medication.IsCritical ? (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR] : (Color)Application.Current.Resources[StyleConstants.ST_TERTIARY_TEXT_COLOR];

            _medicationIsCritical.CheckBoxId = ParentPage.GetResourceValueByKey(ResourceConstants.R_IS_CRITICAL_KEY);
            AutomationId = $"{ShellMasterPage.CurrentShell.CurrentPage.GetType().Name}{Constants.AUTOMATION_ID_SEPRATOR}{_medicationData.Medication.IsCritical}";

            AutomationProperties.SetIsInAccessibleTree(_medicationIsCritical, true);
            AutomationProperties.SetName(_medicationIsCritical, _medicationIsCritical.AutomationId);

            _medicineNameEntry.PageResources = _medicationDoseUnitEntry.PageResources = _medicationDoseUnitPicker.PageResources
                    = _medicationIntakePicker.PageResources = _medicationFrequencyEntryMultipleCheckBox.PageResources = _medicationAdditionalNotesMultipleCheckBox.PageResources = _medicationAlternateDaysEntry.PageResources = _medicationFromDatePicker.PageResources
                    = _medicationToDatePicker.PageResources = _medicationNoteEntry.PageResources = _medicationAddedByEntry.PageResources = ParentPage.PageData;
            _medicationReminderHeaderLabel.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_SET_REMIDERS_KEY);
            _medicationFrequencyEntryMultipleCheckBox.RightViewes.Clear();
            _medicationFrequencyEntryMultipleCheckBox.SetOptions((from filter in _medicationData.FrequencyTypeOptions
                                                                  select new OptionSelectModel
                                                                  {
                                                                      OptionID = filter.OptionID,
                                                                      Value = filter.OptionID.ToString(),
                                                                      DisplayText = filter.OptionText,
                                                                      IsSelected = filter.IsSelected
                                                                  }).ToList());
            _medicationAdditionalNotesMultipleCheckBox.RightViewes.Clear();
            _medicationAdditionalNotesMultipleCheckBox.SetOptions((from filter in _medicationData.AdditionalNotesOptions
                                                                   select new OptionSelectModel
                                                                   {
                                                                       OptionID = filter.OptionID,
                                                                       Value = filter.OptionID.ToString(),
                                                                       DisplayText = filter.OptionText,
                                                                       IsSelected = filter.IsSelected
                                                                   }).ToList());
            _medicationIntakePicker.ItemSource = _medicationData.FrequencyOptions;
            _medicationDoseUnitPicker.ItemSource = _medicationData.UnitOptions;
            _medicationIntakePicker.SelectedValuesChanged += OnIntakePickerChnaged;
            _medicationSetReminderSwitch.Toggled += Reminder_Toggle;
            EnableDisableControl();
            await UpdateMedicationDataAsync().ConfigureAwait(true);
        }
    }

    private void isCritical_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        var selectedBox = ((CustomCheckBox)sender);
        _isCriticalChecked = selectedBox.IsChecked;
        if (!selectedBox.IsChecked)
        {
            //todo: selectedBox.BackgroundColor = Color.Transparent;
            selectedBox.IsChecked = false;
            selectedBox.Color = selectedBox.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_TERTIARY_TEXT_COLOR];
        }
        else
        {
            //todo: selectedBox.BackgroundColor = Color.Transparent;
            selectedBox.IsChecked = true;
            selectedBox.Color = selectedBox.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
        }
        if (_medicationData.Medication.PatientMedicationID == Guid.Empty)
        {
            if (_isCriticalChecked)
            {
                _medicationSetReminderSwitch.IsToggled = true;
            }
            else
            {
                _medicationSetReminderSwitch.IsToggled = false;
            }
        }
        RefreshReminderGrid();
    }

    private void RefreshReminderGrid()
    {
        if (ParentPage.PageLayout.Children.Contains(_reminderGrid))
        {
            int frequencyValue = _medicationFrequencyEntryMultipleCheckBox.SelectedIndexValues.Count;
            List<string> frequency = new List<string>();
            frequency = _medicationFrequencyEntryMultipleCheckBox.SelectedIndexValues;
            if (frequencyValue == 0 || !_medicationSetReminderSwitch.IsToggled)
            {
                _reminderGrid.Children.Clear();
            }
            else
            {
                if (frequencyValue < _reminderGrid.Children.Count)
                {
                    for (int i = _reminderGrid.Children.Count; i > frequencyValue; i--)
                    {
                        _reminderGrid.Children.RemoveAt(i - 1);
                        ParentPage.PageData.Resources.Remove(ParentPage.PageData.Resources.FirstOrDefault(x =>
                            x.ResourceKey == string.Concat(
                                ParentPage.GetResourceValueByKey(ResourceConstants.R_DOSE_REMINDER_KEY)
                                , (i).ToString(CultureInfo.InvariantCulture)
                            )
                        ));
                    }
                }
                else
                {
                    SetReminderTimePicker(frequencyValue, frequency);
                }
            }
        }
    }

    private void SetReminderTimePicker(int frequencyValue, List<string> frequency)
    {
        if (frequencyValue > _reminderGrid.Children.Count)
        {
            for (int i = _reminderGrid.Children.Count; i < frequencyValue; i++)
            {
                _reminderGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                string doseKey = string.Format(CultureInfo.InvariantCulture, ParentPage.GetResourceValueByKey(ResourceConstants.R_DOSE_REMINDER_KEY), (i + 1).ToString(CultureInfo.InvariantCulture));
                ResourceModel dose = ParentPage.GetResourceByKey(ResourceConstants.R_DOSE_REMINDER_KEY);
                ParentPage.PageData.Resources.Add(new ResourceModel
                {
                    ResourceKey = doseKey,
                    ResourceValue = doseKey,
                    MaxLength = dose.MaxLength,
                    MinLength = dose.MinLength,
                    LanguageID = dose.LanguageID
                });
                CustomDateTimeControl dosesTimePicker = new CustomDateTimeControl
                {
                    ControlResourceKey = doseKey,
                    IsBackGroundTransparent = _isNotPatientPage,
                    ControlType = FieldTypes.TimeControl,
                    TimeHorizontalOption = (Device.RuntimePlatform == Device.iOS && (FlowDirection)App.Current.Resources[StyleConstants.ST_FLOW_DIRECTION] == FlowDirection.RightToLeft)
                        ? TextAlignment.End
                        : TextAlignment.Start
                };
                dosesTimePicker.PageResources = ParentPage.PageData;
                string frequencyName = frequency[i];
                Enum.TryParse<FrequencyType>(frequencyName, out FrequencyType frequencyType);
                if (frequencyType != FrequencyType.MedicationInSOSKey)
                {
                    TimeSpan result = GetFrequencyTypeFromIdentifier(frequencyType, i);
                    dosesTimePicker.SetTime = result;
                    _reminderGrid.Add(dosesTimePicker, 0, _reminderGrid.Children.Count);
                }
                else
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// todo - not done yet
    /// </summary>
    private async void _viewPrescription_Clicked(object sender, EventArgs e)
    {
        _viewPrescription.Clicked -= _viewPrescription_Clicked;
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        if (isPatientData && MobileConstants.IsTablet)
        {
            //await ParentPage.SetRightHeaderItemsAsync(nameof(_prescriptionView)).ConfigureAwait(true);
            //PrescriptionViewPopUpPage trackerAddEditPage = new PrescriptionViewPopUpPage(_medicationData.Medication.PatientMedicationID);
            //todo:await Navigation.PushPopupAsync(trackerAddEditPage).ConfigureAwait(true);
        }
        else
        {
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.PrescriptionPage.ToString(),
                GenericMethods.GenerateParamsWithPlaceholder(Param.id), _medicationData.Medication.PatientMedicationID.ToString()).ConfigureAwait(true);
        }
        _viewPrescription.Clicked += _viewPrescription_Clicked;
    }

    /// <summary>
    /// Unregister events of View
    /// </summary>
    public async override Task UnloadUIAsync()
    {
        App._essentials.SetPreferenceValue(StorageConstants.PR_CONTROL_WIDTH_KEY, 0.0);
        _medicationSetReminderSwitch.Toggled -= Reminder_Toggle;
        _medicationIntakePicker.SelectedValuesChanged -= OnIntakePickerChnaged;
        await Task.CompletedTask;
    }

    internal async Task<bool> SaveMedicationAsync()
    {
        if (ParentPage.IsFormValid())
        {
            if (_medicationFromDatePicker.GetSetDate.Value > _medicationToDatePicker.GetSetDate.Value)
            {
                DisplayErrorMessage(string.Format(CultureInfo.InvariantCulture,
                    ParentPage.GetResourceValueByKey(ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                    ParentPage.GetResourceValueByKey(ResourceConstants.R_START_DATE_KEY),
                    ParentPage.GetResourceValueByKey(ResourceConstants.R_END_DATE_KEY)), false);
                return false;
            }
            if (!GetMedicationReminders())
            {
                DisplayErrorMessage(ParentPage.GetResourceValueByKey(ResourceConstants.R_REMINDER_TIME_ERROR_KEY), false);
                return false;
            }
            MapMedicationData();
            _medicationData.ErrCode = ErrorCode.OK;
            AppHelper.ShowBusyIndicator = true;
            await (ParentPage.PageService as MedicationSevice).SaveMedicationAsync(_medicationData).ConfigureAwait(true);
            if (_medicationData.ErrCode == ErrorCode.OK)
            {
                await ParentPage.SyncDataWithServerAsync(Pages.PatientMedicationPage, false
                     , App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)
                 ).ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
                if (MobileConstants.IsDevicePhone)
                {
                    await ParentPage.DisplayMessagePopupAsync(_medicationData.ErrCode.ToString(), false, true, false).ConfigureAwait(true);
                    await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
                }
                else
                {
                    DisplayErrorMessage(ParentPage.GetResourceValueByKey(_medicationData.ErrCode.ToString()), true);
                }
                return true;
            }
            else
            {
                AppHelper.ShowBusyIndicator = false;
                DisplayErrorMessage(ParentPage.GetResourceValueByKey(_medicationData.ErrCode.ToString()), false);
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private string GetAddedByText()
    {
        if (!string.IsNullOrWhiteSpace(_medicationData.Medication.AddedByName))
        {
            return _medicationData.Medication.AddedByName;
        }
        else
        {
            var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
            bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
            if (isPatientData)
            {
                return ParentPage.GetResourceValueByKey(ResourceConstants.R_PROVIDER_KEY);
            }
            else
            {
                return App._essentials.GetPreferenceValue(StorageConstants.PR_SELECTED_USER_ID_KEY, (long)0) == _medicationData.Medication.PatientID
                    ? ParentPage.GetResourceValueByKey(ResourceConstants.R_PATIENT_KEY)
                    : ParentPage.GetResourceValueByKey(ResourceConstants.R_PROVIDER_KEY);
            }
        }
    }

    private void EnableDisableControl()
    {
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        if (!isPatientData)
        {
            _medicationReminderHeaderLabel.IsVisible = false;
            _medicationSetReminderSwitch.IsVisible = false;
        }
        if (_medicationData.Medication.PatientMedicationID != Guid.Empty)
        {
            if (App._essentials.ConvertToLocalTime(_medicationData.Medication.EndDate.Value).Date < DateTime.Now.Date)
            {
                ParentPage.PageLayout.IsEnabled = false;
                IsReadOnly = true;
                _medicationDoseUnitPicker.IsEnabled = false;
                if (!IsPatientPage())
                {
                    ShellMasterPage.CurrentShell.CurrentPage.ShowHideLeftRightHeader(
                        ParentPage is PatientMedicationPage ? MenuLocation.Left : MenuLocation.Right,
                        !IsReadOnly
                    );
                }
            }
            else
            {
                if (_medicationData.Medication.ProgramID > 0
                    || (_medicationData.Medication.PatientMedicationID != Guid.Empty && isPatientData &&
                    _medicationData.AddedBy != _medicationData.Medication.AddedByID)
                )
                {
                    _medicineNameEntry.IsEnabled = false;
                    _medicationDoseUnitEntry.IsEnabled = false;
                    _medicationDoseUnitPicker.IsEnabled = false;
                    _medicationIntakePicker.IsEnabled = false;
                    _medicationAlternateDaysEntry.IsEnabled = false;
                    _medicationFrequencyEntryMultipleCheckBox.IsEnabled = false;
                    _medicationIsCritical.IsEnabled = false;
                    _medicationAdditionalNotesMultipleCheckBox.IsEnabled = false;
                    _medicationFromDatePicker.IsEnabled = false;
                    _medicationToDatePicker.IsEnabled = false;
                    _medicationNoteEntry.IsEditable = false;
                    if (!isPatientData)
                    {
                        IsReadOnly = true;
                    }
                }
            }
        }
    }

    private async Task UpdateMedicationDataAsync()
    {
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        if (_medicationData.Medication.PatientMedicationID != Guid.Empty)
        {
            _medicineNameEntry.Value = _medicationData.Medication.ShortName;
            _medicationDoseUnitEntry.Value = _medicationData.Medication.Doses.ToString(CultureInfo.InvariantCulture);
            _medicationDoseUnitPicker.SelectedValue = _medicationData.UnitOptions.FirstOrDefault(x => x.GroupName == _medicationData.Medication.UnitIdentifier).OptionID;
            _medicationDoseUnitPicker.IsEnabled = false;
            if (isPatientData)
            {
                if (ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PrescriptionView.ToString()) && (_medicationData.Medication.AddedByID != _medicationData.AddedBy || _medicationData.Medication.ProgramID > 0))
                {
                    _viewPrescription.IsVisible = true;
                    _viewPrescription.Clicked += _viewPrescription_Clicked;
                }
            }
            _medicationIntakePicker.SelectedValue = _medicationData.FrequencyOptions.Find(x => x.IsSelected).OptionID;
            _medicationAlternateDaysEntry.Value = _medicationData.Medication.AfterDays > 0
                ? _medicationData.Medication.AfterDays.ToString(CultureInfo.InvariantCulture)
                : string.Empty;
            await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
            _medicationFromDatePicker.GetSetDate = _medicationData.Medication.StartDate?.ToLocalTime().Date;
            _medicationToDatePicker.GetSetDate = App._essentials.ConvertToLocalTime(_medicationData.Medication.EndDate.Value).Date;
            _medicationNoteEntry.Value = _medicationData.Medication.Note;
            _medicationAddedByEntry.Value = GetAddedByText();
            ParentPage.PageLayout.Add(_medicationAddedByEntry, 0, 12);
            Grid.SetColumnSpan(_medicationAddedByEntry, 4);
            if (_medicationData.Medication.IsCritical && !_medicationData.Medication.Reminder && _medicationData.Medication.LastModifiedByID.ToString() != _medicationData.AddedBy)
            {
                _medicationData.Medication.Reminder = _medicationData.Medication.IsCritical;
            }
            _medicationSetReminderSwitch.IsToggled = _medicationData.Medication.Reminder;
            if (_medicationData.Medication.ErrCode != ErrorCode.OK)
            {
                DisplayErrorMessage(ParentPage.GetResourceValueByKey(_medicationData.Medication.ErrCode.ToString()), false);
            }
        }
    }

    private void OnIntakePickerChnaged(object sender, EventArgs e)
    {
        if (_medicationData.FrequencyOptions.FirstOrDefault(x => x.OptionID == _medicationIntakePicker.SelectedValue).GroupName == ResourceConstants.R_ALTERNATE_FOR_KEY)
        {
            if (!ParentPage.PageLayout.Children.Contains(_medicationAlternateDaysEntry))
            {
                ParentPage.PageLayout.Add(_medicationAlternateDaysEntry, 0, 6);
                Grid.SetColumnSpan(_medicationAlternateDaysEntry, 4);
            }
        }
        else
        {
            _medicationAlternateDaysEntry.Value = string.Empty;
            ParentPage.PageLayout.Children.Remove(_medicationAlternateDaysEntry);
        }
    }

    private TimeSpan GetFrequencyTypeFromIdentifier(FrequencyType frequencyType, int i)
    {
        string reminderkey = ParentPage.GetSettingsValueByKey(SettingsConstants.MEDICINE_REMINDER_TIME_KEY);
        string[] values = reminderkey.Split('|');

        TimeSpan timeSpan = DateTime.Now.TimeOfDay;

        switch (frequencyType)
        {
            case FrequencyType.MedicationInMorningKey:
                timeSpan = _medicationData.Reminders?.Count > i
                    ? App._essentials.ConvertToLocalTime(_medicationData.Reminders[i].ReminderDateTime).TimeOfDay
                    : TimeSpan.FromHours(double.Parse(values[0]));
                break;
            case FrequencyType.MedicationInAfternoonKey:
                timeSpan = _medicationData.Reminders?.Count > i
                    ? App._essentials.ConvertToLocalTime(_medicationData.Reminders[i].ReminderDateTime).TimeOfDay
                    : TimeSpan.FromHours(double.Parse(values[1]));
                break;
            case FrequencyType.MedicationInEveningKey:
                timeSpan = _medicationData.Reminders?.Count > i
                    ? App._essentials.ConvertToLocalTime(_medicationData.Reminders[i].ReminderDateTime).TimeOfDay
                    : TimeSpan.FromHours(double.Parse(values[2]));
                break;
            case FrequencyType.MedicationInNightKey:
                timeSpan = _medicationData.Reminders?.Count > i
                    ? App._essentials.ConvertToLocalTime(_medicationData.Reminders[i].ReminderDateTime).TimeOfDay
                    : TimeSpan.FromHours(double.Parse(values[3]));
                break;
            case FrequencyType.MedicationInSOSKey:
                break;
            default:
                timeSpan = DateTime.Now.TimeOfDay;
                break;
        }

        return timeSpan;
    }


    private void Reminder_Toggle(object sender, ToggledEventArgs e)
    {
        if (_medicationSetReminderSwitch.IsVisible && _medicationSetReminderSwitch.IsToggled)
        {
            if (!ParentPage.PageLayout.Children.Contains(_reminderGrid))
            {
                ParentPage.PageLayout.Add(_reminderGrid, 0, 14);
                Grid.SetColumnSpan(_reminderGrid, 4);
            }
            RefreshReminderGrid();
        }
        else
        {
            _reminderGrid.Children.Clear();
            ParentPage.PageLayout.Children.Remove(_reminderGrid);
        }

    }

    private double GetMinDate(string dateKey)
    {
        if (App._essentials.ConvertToLocalTime(_medicationData.Medication.StartDate.Value).Date < DateTime.Now.Date)
        {
            return -(DateTime.Now.Date - App._essentials.ConvertToLocalTime(_medicationData.Medication.StartDate.Value).Date).Days;
        }
        else
        {
            return ParentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == dateKey).MinLength;
        }
    }

    private void DisplayErrorMessage(string message, bool isSuccess)
    {
        if (MobileConstants.IsDevicePhone)
        {
            ParentPage.DisplayOperationStatus(message, isSuccess);
        }
        else
        {
            OnDisplayStatus?.Invoke(message, isSuccess ? new EventArgs() : EventArgs.Empty);
        }
    }

    private void MapMedicationData()
    {
        _medicationData.Medication.FullName = _medicineNameEntry.Value;
        _medicationData.Medication.ShortName = string.IsNullOrWhiteSpace(_medicationData.Medication.ShortName)
            ? _medicineNameEntry.Value
            : _medicationData.Medication.ShortName;
        _medicationData.Medication.UnitIdentifier = _medicationData.UnitOptions.FirstOrDefault(x => x.OptionID == _medicationDoseUnitPicker.SelectedValue).GroupName;
        _medicationData.Medication.Doses = string.IsNullOrWhiteSpace(_medicationDoseUnitEntry.Value)
            ? (decimal)0.0
            : Convert.ToDecimal(_medicationDoseUnitEntry.Value, CultureInfo.InvariantCulture);

        _medicationData.Medication.Frequency = string.Empty;
        foreach (var filter in _medicationFrequencyEntryMultipleCheckBox.SelectedIndexValues)
        {
            _medicationData.Medication.Frequency = string.IsNullOrWhiteSpace(_medicationData.Medication.Frequency)
                ? filter.ToString()
                : _medicationData.Medication.Frequency + Constants.SYMBOL_PIPE_SEPERATOR + filter.ToString();
        }
        _medicationData.Medication.IsCritical = _isCriticalChecked;
        _medicationData.Medication.AdditionalNotes = string.Empty;
        foreach (var filter in _medicationAdditionalNotesMultipleCheckBox.SelectedIndexValues)
        {
            _medicationData.Medication.AdditionalNotes = string.IsNullOrWhiteSpace(_medicationData.Medication.AdditionalNotes)
                ? filter.ToString()
                : _medicationData.Medication.AdditionalNotes + Constants.SYMBOL_PIPE_SEPERATOR + filter.ToString();
        }
        _medicationData.Medication.HowOften = Convert.ToInt32(_medicationIntakePicker.SelectedValue);
        _medicationData.Medication.AfterDays = string.IsNullOrWhiteSpace(_medicationAlternateDaysEntry.Value) || !_medicationAlternateDaysEntry.IsVisible
            ? (byte)0
            : Convert.ToByte(_medicationAlternateDaysEntry.Value, CultureInfo.InvariantCulture);
        _medicationData.Medication.StartDate = _medicationFromDatePicker.GetSetDate.Value.ToUniversalTime();
        _medicationData.Medication.EndDate = _medicationToDatePicker.GetSetDate.Value.ToUniversalTime();
        _medicationData.Medication.Note = _medicationNoteEntry.Value?.Trim();
        _medicationData.Medication.Reminder = _medicationSetReminderSwitch.IsToggled;
        _medicationData.Medication.IsActive = true;
        _medicationData.Medicines = new List<MedicineModel>();
    }

    private bool GetMedicationReminders()
    {
        if (ParentPage.PageLayout.Children.Contains(_reminderGrid) && _reminderGrid.Children.Count > 0)
        {
            _medicationData.Reminders = new List<MedicationReminderModel>();
            foreach (var item in _reminderGrid.Children)
            {
                CustomDateTimeControl reminderTimePicker = item as CustomDateTimeControl;
                if (_medicationData.Reminders.Any(r => r.ReminderDateTime.TimeOfDay == reminderTimePicker.GetSetDate.Value.ToUniversalTime().TimeOfDay))
                {
                    _medicationData.Reminders = new List<MedicationReminderModel>();
                    return false;
                }
                _medicationData.Reminders.Add(new MedicationReminderModel { ReminderDateTime = (item as CustomDateTimeControl).GetSetDate.Value.ToUniversalTime(), IsActive = true });
            }
        }
        else
        {
            _medicationData.Reminders = new List<MedicationReminderModel>();
        }
        return true;
    }

    private void OnSelectMedicine(object sender, EventArgs e)
    {
        if (sender is MedicineModel selectedMedicine)
        {
            SetSelectedMedicineData(selectedMedicine);
        }
    }

    private void OnCloseSearchMedicine(object sender, EventArgs e)
    {
        _medicineNameEntry.Value = String.Empty;
    }

    private async void OnMedicineChanged(object sender, EventArgs e)
    {
        if (_medicineNameEntry.Value?.Trim().Length > 2)
        {
            AppHelper.ShowBusyIndicator = true;
            await new MedicineService(App._essentials).SearchMedicineAsync(_medicationData, _medicineNameEntry.Value);
            AppHelper.ShowBusyIndicator = false;
            if (_medicationData.Medicines?.Count > 1)
            {
                _medicineSearchPage.SearchedMedicineList = _medicationData.Medicines;
                //todo:await Navigation.PushPopupAsync(_medicineSearchPage).ConfigureAwait(false);
            }
            else if (_medicationData.Medicines?.Count == 1)
            {
                SetSelectedMedicineData(_medicationData.Medicines.FirstOrDefault());
            }
            else
            {
                _medicationData.Medication.ShortName = string.Empty;
                _medicationDoseUnitPicker.IsEnabled = false;
                //todo:await Navigation.PushPopupAsync(_medicineSearchPage).ConfigureAwait(false);
            }
        }
        else
        {
            _medicationData.Medication.ShortName = string.Empty;
            _medicationDoseUnitPicker.IsEnabled = false;
        }
    }

    private void SetSelectedMedicineData(MedicineModel selectedMedicine)
    {
        _medicineNameEntry.Value = string.IsNullOrWhiteSpace(selectedMedicine.FullName) ? selectedMedicine.ShortName : selectedMedicine.FullName;
        _medicationData.Medication.ShortName = selectedMedicine.ShortName;
        var selectedOption = _medicationData.UnitOptions.FirstOrDefault(x => x.GroupName == selectedMedicine.UnitIdentifier);
        //todo need to change after updated units table 
        if (selectedOption != null)
        {
            _medicationDoseUnitPicker.SelectedValue = selectedOption.OptionID;
        }
        else
        {
            _medicationDoseUnitPicker.SelectedValue = -1;
        }
        _medicationDoseUnitPicker.IsEnabled = false;
    }

    /// <summary>
    /// Dispose method
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose method
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        // Cleanup
    }
}