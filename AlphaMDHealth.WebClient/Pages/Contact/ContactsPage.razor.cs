using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ContactsPage : BasePage
{
    private readonly ContactDTO _contactData = new ContactDTO();
    private Guid _contactID;
    private long _permissionAtLevel;

    /// <summary>
    /// Type of Contact Organization, Branch, User, Patient
    /// </summary>
    [Parameter]
    public ContactType ContactType { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _permissionAtLevel = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, 0);
        GetSelectedContactLevel();
        await RetrieveDataAndDisplayAsync().ConfigureAwait(true);
    }

    protected override async Task OnParametersSetAsync()
    {
        long currentPermissionAtLevel = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, 0);
        if (_permissionAtLevel != currentPermissionAtLevel)
        {
            GetSelectedContactLevel();
            await RetrieveDataAndDisplayAsync().ConfigureAwait(true);
        }
        _permissionAtLevel = currentPermissionAtLevel;
    }

    private async Task RetrieveDataAndDisplayAsync()
    {
        _contactData.SelectedUserID = _contactData.ContactType == ContactType.Patient || _contactData.ContactType == ContactType.User ? AppState.UserDetails.User?.UserID ?? 0 : 0;
        _contactData.Contact = new ContactModel { ContactID = Guid.Empty };
        await SendServiceRequestAsync(new ContactsService(AppState.webEssentials).GetContactsAsync(_contactData), _contactData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private void OnAddEditClick(ContactModel contactModel)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        _contactID = contactModel == null ? Guid.Empty : contactModel.ContactID;
    }

    private async Task OnAddEditClosedAsync(string isDataUpdated)
    {
        ShowDetailPage = false;
        _contactID = Guid.Empty;
        Success = Error = string.Empty;
        if (isDataUpdated == ResourceConstants.R_SAVE_ACTION_KEY)
        {
            _isDataFetched = false;
            Success = ErrorCode.OK.ToString();
            await RetrieveDataAndDisplayAsync().ConfigureAwait(true);
            StateHasChanged();
        }
        else if (isDataUpdated == ResourceConstants.R_CANCEL_ACTION_KEY)
        {
            _contactID = Guid.Empty;
        }
        else
        {
            Error = isDataUpdated;
        }
    }

    private async Task OnViewAllClickedAsync()
    {
        switch (ContactType)
        {
            case ContactType.Organisation:
                await NavigateToAsync(AppPermissions.OrganisationContactsView.ToString()).ConfigureAwait(true);
                break;
            case ContactType.Branch:
                await NavigateToAsync(AppPermissions.BranchContactsView.ToString()).ConfigureAwait(true);
                break;
            case ContactType.Patient:
                await NavigateToAsync(AppPermissions.PatientContactsView.ToString()).ConfigureAwait(true);
                break;
            case ContactType.User:
            default:
                await NavigateToAsync(AppPermissions.UserContactsView.ToString()).ConfigureAwait(true);
                break;
        }
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(ContactModel.ContactID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(ContactModel.ContactType),DataHeader=ResourceConstants.R_CONTACT_KEY},
            new TableDataStructureModel{DataField=nameof(ContactModel.ContactTypeIs),DataHeader=ResourceConstants.R_CONTACT_TYPE_KEY},
            new TableDataStructureModel{DataField=nameof(ContactModel.ContactValue),DataHeader=ResourceConstants.R_CONTACT_DETAIL_KEY},
        };
    }

    private void GetSelectedContactLevel()
    {
        if (ContactType == default)
        {
            if (AppState.SelectedTab == AppPermissions.PatientContactsView.ToString())
            {
                _contactData.ContactType = ContactType.Patient;
            }
            else if (AppState.SelectedTab == AppPermissions.UserContactsView.ToString())
            {
                _contactData.ContactType = ContactType.User;
            }
            else if (AppState.RouterData.SelectedRoute.FeatureText.Contains(LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.OrganisationContactsView.ToString()),StringComparison.OrdinalIgnoreCase))
            {
                _contactData.ContactType = ContactType.Organisation;
            }
            else if (AppState.RouterData.SelectedRoute.FeatureText.Contains(LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.BranchContactsView.ToString()), StringComparison.OrdinalIgnoreCase))
            {
                _contactData.ContactType = ContactType.Branch;
            }
            else if (AppState.RouterData.SelectedRoute.FeatureText.Contains(LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.PatientContactsView.ToString()), StringComparison.OrdinalIgnoreCase))
            {
                _contactData.ContactType = ContactType.Patient;
            }
            else
            {
                _contactData.ContactType = ContactType.User;
            }
        }
        else
        {
            _contactData.ContactType = ContactType;
        }
    }

    private string CheckAddEditPermission()
    {
        return _contactData.ContactType switch
        {
            ContactType.Organisation => AppPermissions.OrganisationContactAddEdit.ToString(),
            ContactType.Branch => AppPermissions.BranchContactAddEdit.ToString(),
            ContactType.Patient => AppPermissions.PatientContactAddEdit.ToString(),
            _ => AppPermissions.UserContactAddEdit.ToString(),
        };
    }

    private string GetTableHeader()
    {
        return _contactData.ContactType switch
        {
            ContactType.Organisation => AppPermissions.OrganisationContactsView.ToString(),
            ContactType.Branch => AppPermissions.BranchContactsView.ToString(),
            ContactType.Patient => AppPermissions.PatientContactsView.ToString(),
            _ => AppPermissions.UserContactsView.ToString(),
        };
    }
}
