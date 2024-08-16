using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient Medication View to add, edit and view medication data
/// </summary>
public class PatientContactView : ViewManager, IDisposable
{
    private readonly CustomBindablePickerControl _contactTypeIsPicker;
    private readonly CustomBindablePickerControl _contactTypePicker;
    private readonly StackLayout _languageStack;
    private readonly CustomTabsControl _customTabs;
    private readonly CustomLabelControl _languageErrorLabel;
    private readonly CustomButtonControl _deleteButton;
    private readonly ContactDTO _contactData;
    private readonly bool _isNotPatientPage;

    /// <summary>
    /// Parameterized constructor containing page inance
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PatientContactView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new ContactsService(App._essentials);
        _isNotPatientPage = !IsPatientPage();
        _contactData = new ContactDTO
        {
            RecordCount = -1,
            Contact = new ContactModel()
        };
        var padding = (double)App.Current.Resources[StyleConstants.ST_APP_PADDING];
        ParentPage.AddRowColumnDefinition(GridLength.Auto, 6, true);
        ParentPage.AddRowColumnDefinition(GridLength.Star, 1, true);
        _contactTypeIsPicker = new CustomBindablePickerControl
        {
            ControlResourceKey = ResourceConstants.R_CONTACT_TYPE_KEY,
            IsBackGroundTransparent = _isNotPatientPage
        };
        ParentPage.PageLayout.Add(_contactTypeIsPicker, 0, 1);
       Grid.SetColumnSpan(_contactTypeIsPicker, 1);

        _contactTypePicker = new CustomBindablePickerControl
        {
            ControlResourceKey = ResourceConstants.R_CONTACT_KEY,
            IsBackGroundTransparent = _isNotPatientPage
        };
        ParentPage.PageLayout.Add(_contactTypePicker, 0, 2);

        _customTabs = new CustomTabsControl
        {
            Margin = new Thickness(padding, 0, padding, padding),
            HorizontalOptions = LayoutOptions.Start,
            IsVisible = false
        };
        _languageStack = new StackLayout
        {
            Spacing = 0,
            FlowDirection = (FlowDirection)App.Current.Resources[StyleConstants.ST_FLOW_DIRECTION]
        };
        ParentPage.PageLayout.Add(_languageStack, 0, 4);

        _languageErrorLabel = new CustomLabelControl(LabelType.ClientErrorLabel)
        {
            IsVisible = false,
            Margin = new Thickness(0, 0, 0, 10)
        };
        ParentPage.PageLayout.Add(_languageErrorLabel, 0, 5);

        _deleteButton = new CustomButtonControl(ButtonType.DeleteWithMargin)
        {
            IsVisible = false,
            VerticalOptions = LayoutOptions.End
        };
        ParentPage.PageLayout.Add(_deleteButton, 0, 6);

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
            string contactId = GenericMethods.MapValueType<string>(GetParameterValue(nameof(ContactModel.ContactID)));
            _contactData.Contact.ContactID = string.IsNullOrWhiteSpace(contactId)
                ? Guid.Empty
                : new Guid(contactId);
        }
        await (ParentPage.PageService as ContactsService).GetContactsAsync(_contactData).ConfigureAwait(true);
        ParentPage.PageData = _contactTypeIsPicker.PageResources = _contactTypePicker.PageResources = ParentPage.PageService.PageData;
        if (_contactData.ErrCode == ErrorCode.OK)
        {
            _contactTypeIsPicker.ItemSource = _contactData.ContactTypeIsOptions;
            _contactTypePicker.ItemSource = _contactData.ContactTypeOptions;
            if (_contactData.Contact.ContactID != Guid.Empty)
            {
                _contactTypeIsPicker.SelectedValue = _contactData.Contact.ContactTypeIsID;
                _contactTypePicker.SelectedValue = _contactData.Contact.ContactTypeID;
                if (ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientContactDelete.ToString()))
                {
                    _deleteButton.IsVisible = true;
                    _deleteButton.Clicked += OnDeleteActionClicked;
                }
                CreateLanguageTabs();
            }
            _contactTypePicker.SelectedValuesChanged += OnContactPicker_ValuesChanged;
        }
        else
        {
            await RenderErrorAsync();
        }
    }

    /// <summary>
    /// Unregister events of View
    /// </summary>
    public async override Task UnloadUIAsync()
    {
        _contactTypePicker.SelectedValuesChanged -= OnContactPicker_ValuesChanged;
        await Task.CompletedTask;
    }

    internal async Task<bool> OnSaveActionClicked()
    {
        var isFormValid = ParentPage.IsFormValid();
        if (!AreLanguageSpecificFieldsValid())
        {
            isFormValid = false;
        }
        if (isFormValid)
        {
            AppHelper.ShowBusyIndicator = true;
            _contactData.Contact.ContactTypeIsID = (int)_contactTypeIsPicker.SelectedValue;
            _contactData.Contact.ContactTypeID = (int)_contactTypePicker.SelectedValue;
            _contactData.Contact.IsActive = true;
            return await SaveContactAsync();
        }
        else
        {
            return false;
        }
    }

    private async void OnDeleteActionClicked(object sender, EventArgs e)
    {
        await ParentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnDeleteConfirmationActionClicked, true, true, false).ConfigureAwait(true);
    }

    private async void OnDeleteConfirmationActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 1:
                ParentPage.OnClosePupupAction(sender, e);
                AppHelper.ShowBusyIndicator = true;
                _contactData.Contact.IsActive = false;
                await SaveContactAsync().ConfigureAwait(true);
                break;
            case 2:
                ParentPage.OnClosePupupAction(sender, e);
                break;
            default:// to do
                break;
        }
    }

   

    private void OnContactPicker_ValuesChanged(object sender, EventArgs e)
    {
        if (GenericMethods.IsListNotEmpty(_contactData.PatientContactDetails))
        {
            foreach (var detail in _contactData.PatientContactDetails)
            {
                detail.ContactValue = string.Empty;
            }
            if (_customTabs.IsVisible)
            {
                RenderLanguageSpecificFields();
            }
            else
            {
                CreateLanguageTabs();
            }
        }
    }

    private void Language_TabClicked(object sender, EventArgs e)
    {
        ShowHideLanguageSpecificFields(Convert.ToByte(sender, CultureInfo.InvariantCulture));
    }

    private async Task<bool> SaveContactAsync()
    {
        _contactData.ErrCode = ErrorCode.OK;
        await (ParentPage.PageService as ContactsService).SaveContactsAsync(_contactData).ConfigureAwait(true);
        if (_contactData.ErrCode == ErrorCode.OK)
        {
            await ParentPage.SyncDataWithServerAsync(Pages.PatientContactsView, false, App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)).ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false; 
            if (MobileConstants.IsDevicePhone)
            {
                await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
            }
            else
            {
                InvokeListRefresh(Guid.Empty, new EventArgs());
                //todo:
                //if (PopupNavigation.Instance.PopupStack?.Count > 0)
                //{
                //    //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
                //}
            }
            return true;
        }
        else
        {
            AppHelper.ShowBusyIndicator = false;
            ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(_contactData.ErrCode.ToString()));
            return false;
        }
    }

    private bool AreLanguageSpecificFieldsValid()
    {
        _languageErrorLabel.IsVisible = false;
        foreach (var detail in _contactData.PatientContactDetails)
        {
            detail.IsActive = true;
            var viewData = _languageStack.Children.Where(x => x.AutomationId == detail.LanguageID.ToString(CultureInfo.InvariantCulture)).ToList();
            if(_contactTypePicker.SelectedValue<=0|| _contactTypePicker.SelectedValue<=0)
            {
                return false;
            }
            else if (_contactTypePicker.SelectedValue == ParentPage.GetResourceByKey(ResourceConstants.R_ADDRESS_CONTACT_KEY)?.ResourceKeyID)
            {
                detail.ContactValue = (viewData[0] as CustomMultiLineEntryControl).Value;
            }
            else if (_contactTypePicker.SelectedValue == ParentPage.GetResourceByKey(ResourceConstants.R_PHONE_CONTACT_KEY)?.ResourceKeyID)
            {
                detail.ContactValue = (viewData[0] as CustomMobileControl).Value;
            }
            else
            {
                detail.ContactValue = (viewData[0] as CustomEntryControl).Value;
            }
            if (string.IsNullOrWhiteSpace(detail.ContactValue))
            {
                _languageErrorLabel.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_REQUIRED_ALL_FIELD_VALIDATION_KEY);
                _languageErrorLabel.IsVisible = true;
            }
        }
        return !_languageErrorLabel.IsVisible;
    }

    private void CreateLanguageTabs()
    {
        if (GenericMethods.IsListNotEmpty(_contactData.PatientContactDetails) && _contactTypePicker.SelectedValue > 0)
        {
            _customTabs.IsVisible = true;
            _customTabs.LoadUIData((from language in _contactData.PatientContactDetails
                                    select new OptionModel
                                    {
                                        OptionID = Convert.ToInt64(language.LanguageID, CultureInfo.InvariantCulture),
                                        OptionText = language.LanguageName,
                                        GroupName = Convert.ToString(language.LanguageID, CultureInfo.InvariantCulture)
                                    }).ToList(), false);
            _customTabs.TabClicked += Language_TabClicked;
            ParentPage.PageLayout.Add(_customTabs, 0, 3);
            RenderLanguageSpecificFields();
        }
    }

    private void RenderLanguageSpecificFields()
    {
        CreateLanguageSpecificFields();
        ShowHideLanguageSpecificFields(_contactData.PatientContactDetails[0].LanguageID);
    }

    private void CreateLanguageSpecificFields()
    {
        _languageStack.Children.Clear();
        foreach (var detail in _contactData.PatientContactDetails)
        {
            CreateLanguageSpecificField(detail);
        }
    }

    private void CreateLanguageSpecificField(ContactDetailModel detail)
    {
        var selectedType = _contactData.ContactTypeOptions.FirstOrDefault(x => x.OptionID == _contactTypePicker.SelectedValue);
        if (selectedType != null && selectedType.OptionID > 0)
        {
            BaseContentView valueField;
            if (selectedType.OptionID == ParentPage.GetResourceByKey(ResourceConstants.R_ADDRESS_CONTACT_KEY)?.ResourceKeyID)
            {
                valueField = new CustomMultiLineEntryControl
                {
                    ControlResourceKey = ResourceConstants.R_ADDRESS_CONTACT_KEY,
                    EditorHeightRequest = EditorHeight.Default,
                    PageResources = ParentPage.PageData,
                    AutomationId = detail.LanguageID.ToString(CultureInfo.InvariantCulture),
                    Value = detail.ContactValue ?? string.Empty
                };
            }
            else if (selectedType.OptionID == ParentPage.GetResourceByKey(ResourceConstants.R_PHONE_CONTACT_KEY)?.ResourceKeyID)
            {
                valueField = new CustomMobileControl
                {
                    ControlResourceKey = ResourceConstants.R_PHONE_CONTACT_KEY,
                    CountrySource = _contactData,
                    PageResources = ParentPage.PageData,
                    AutomationId = detail.LanguageID.ToString(CultureInfo.InvariantCulture),
                    Value = detail.ContactValue ?? string.Empty
                };
                if (string.IsNullOrWhiteSpace(detail.ContactValue))
                {
                    (valueField as CustomMobileControl).SetSelectedCountryId(0);
                }
            }
            else
            {
                valueField = new CustomEntryControl
                {
                    ControlResourceKey = ResourceConstants.R_EMAIL_CONTACT_KEY,
                    ControlType = FieldTypes.EmailEntryControl,
                    ControlIcon = ImageConstants.I_EMAIL_ICON_PNG,
                    RegexPattern = ParentPage.GetSettingsValueByKey(SettingsConstants.S_EMAIL_REGEX_KEY),
                    PageResources = ParentPage.PageData,
                    AutomationId = detail.LanguageID.ToString(CultureInfo.InvariantCulture),
                    Value = detail.ContactValue ?? string.Empty
                };
            }
            _languageStack.Add(valueField);
        }
    }

    private void ShowHideLanguageSpecificFields(byte languageID)
    {
        foreach (var view in _languageStack.Children)
        {
            //todo:view.IsVisible = view.AutomationId == languageID;
        }
    }

    private async Task RenderErrorAsync()
    {
        if (IsPatientPage() || MobileConstants.IsDevicePhone)
        {
            if (MobileConstants.IsDevicePhone)
            {
                await ParentPage.DisplayMessagePopupAsync(_contactData.ErrCode.ToString(), OnPopupActionClicked);
            }
            else
            {
                InvokeListRefresh(_contactData.ErrCode.ToString(), new EventArgs());
                //todo:await Navigation.PopAllPopupAsync();
            }
        }
        else
        {
            InvokeListRefresh(_contactData.ErrCode.ToString(), new EventArgs());
        }
    }

    private async void OnPopupActionClicked(object sender, int e)
    {
        ParentPage.OnClosePupupAction(sender, e);
        await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
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