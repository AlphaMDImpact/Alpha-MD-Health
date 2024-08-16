using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ScanVitalsDetailsPage : BasePage
{
    [Parameter]
    public bool ShowNextScanPage { get; set; } = false;

    [Parameter]
    public bool ShowScanVitalsPage { get; set; } = false;

    /*private void OnDetailPageClosedAsync()
    {
        ShowScanVitalsPage = true;
        ShowNextScanPage = false;
    }*/

    protected override async Task OnInitializedAsync()
    {
        /* _scanVitalData.SelectedUserID = UserID;
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
         }*/
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }
    private void OnNextlClickedAsync()
    {
        ShowScanVitalsPage = false;
        ShowNextScanPage = false;
        ShowDetailPage = false;
    }
}
