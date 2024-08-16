using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ContactPage : BasePage
{
    private readonly ContactDTO _contactData = new ContactDTO { RecordCount = -1 };
    private List<TabDataStructureModel> _dataFormatter = new List<TabDataStructureModel>();
    private bool _hideConfirmationPopup = true;
    private bool _isEditable;
    private bool _isChangedFromControl = false;
    private List<ButtonActionModel> _actionData;

    /// <summary>
    /// Type of Contact Organization, Branch, User, Patient
    /// </summary>
    [Parameter]
    public ContactType ContactType { get; set; }

    /// <summary>
    /// User ID of user whose contact needs to be retrieved
    /// </summary>
    [Parameter]
    public long UserID { get; set; }

    private bool UseSameValues { get; set;} 

    /// <summary>
    /// Contact ID parameter
    /// </summary>
    [Parameter]
    public Guid ContactID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _contactData.ContactType = ContactType;
        _contactData.SelectedUserID = UserID;
        _contactData.Contact = new ContactModel { ContactID = ContactID };
        await SendServiceRequestAsync(new ContactsService(AppState.webEssentials).GetContactsAsync(_contactData), _contactData).ConfigureAwait(true);
        if (_contactData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_contactData.FeaturePermissions, CheckAddEditPermission());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_contactData.ErrCode.ToString());
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Error = Success = string.Empty;
        if (IsValid())
        {
            if (_contactData.Contact.ContactID == Guid.Empty)
            {
                _contactData.Contact.ContactID = GenericMethods.GenerateGuid();
                _contactData.ContactDetails.ForEach(x => x.ContactID = _contactData.Contact.ContactID);
            }
            _contactData.Contact.ContactTypeIsID = Convert.ToInt32(_contactData.ContactTypeIsOptions.Find(x => x.IsSelected).OptionID);
            _contactData.Contact.ContactTypeID = Convert.ToInt32(_contactData.ContactTypeOptions.Find(x => x.IsSelected).OptionID);
            _contactData.Contact.IsActive = true;
            _contactData.Contacts = new List<ContactModel>
            {
                _contactData.Contact
            };
            _contactData.IsActive = true;
            await SaveContactDataAsync();
        }
    }

    private async Task OnDeleteConfirmationPopUpClickedAsync(object sequenceNo)
    {
        Error = Success = string.Empty;
        if (sequenceNo != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _contactData.Contact.IsActive = false;
                _contactData.Contacts = new List<ContactModel> { _contactData.Contact };
                _contactData.IsActive = false;
                await SaveContactDataAsync();
            }
        }
    }

    private async Task SaveContactDataAsync()
    {
        await SendServiceRequestAsync(new ContactsService(AppState.webEssentials).SaveContactsAsync(_contactData), _contactData).ConfigureAwait(true);
        if (_contactData.SaveResults?.Count > 0)
        {
            _contactData.ErrCode = _contactData.SaveResults.FirstOrDefault().ErrCode;
        }
        if (_contactData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(ResourceConstants.R_SAVE_ACTION_KEY);
        }
        else
        {
            Error = _contactData.ErrCode.ToString();
        }
    }

    private async Task OnCanceledClickAsync()
    {
        await OnClose.InvokeAsync(ResourceConstants.R_CANCEL_ACTION_KEY);
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE,ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO,ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private void OnValueChanged(object value, bool isChangedFromControl)
    {
        if (value != null)
        {            
            if (long.TryParse(value.ToString(), out long optionId) && optionId != -1)
            {
                var optionText = _contactData.ContactTypeOptions.Find(x => x.OptionID == optionId).OptionText;
                var resourceKey = _contactData.Resources.Find(x => x.ResourceValue == optionText).ResourceKey;
                UseSameValues = resourceKey == ResourceConstants.R_PHONE_CONTACT_KEY || resourceKey == ResourceConstants.R_EMAIL_CONTACT_KEY;
                if (!string.IsNullOrEmpty(resourceKey))
                {
                    _dataFormatter.Clear();
                    _dataFormatter.Add(new TabDataStructureModel { DataField = nameof(ContactModel.ContactValue), CountryCodes = _contactData.CountryCodes, ResourceKey = resourceKey, RegexPattern = LibSettings.GetSettingValueByKey(_contactData.Settings, SettingsConstants.S_EMAIL_REGEX_KEY) });
                    if(isChangedFromControl &= _isChangedFromControl)
                    {
                        ResetPageDetails(ResourceConstants.R_ADDRESS_CONTACT_KEY);
                        ResetPageDetails(ResourceConstants.R_EMAIL_CONTACT_KEY);
                        ResetPageDetails(ResourceConstants.R_PHONE_CONTACT_KEY);
                    }
                    _isChangedFromControl = true;
                }
            }
            else
            {
                _dataFormatter.Clear();
            }
        }
    }

    private string CheckAddEditPermission()
    {
        return ContactType switch
        {
            ContactType.Organisation => AppPermissions.OrganisationContactAddEdit.ToString(),
            ContactType.Branch => AppPermissions.BranchContactAddEdit.ToString(),
            ContactType.Patient => AppPermissions.PatientContactAddEdit.ToString(),
            _ => AppPermissions.UserContactAddEdit.ToString(),
        };
    }

    private string CheckDeletePermission()
    {
        return _contactData.ContactType switch
        {
            ContactType.Organisation => AppPermissions.OrganisationContactDelete.ToString(),
            ContactType.Branch => AppPermissions.BranchContactDelete.ToString(),
            ContactType.Patient => AppPermissions.PatientContactDelete.ToString(),
            _ => AppPermissions.UserContactDelete.ToString(),
        };
    }

    private void ResetPageDetails(string resourceKey)
    {
        foreach (var contactDetail in _contactData.ContactDetails)
        {
            RemoveControlContainsKey(resourceKey);
            contactDetail.ContactValue = default;
        }
    }
}