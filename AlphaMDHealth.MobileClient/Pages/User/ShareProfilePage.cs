using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(ShareProfilePage))]
[QueryProperty(nameof(UserRelationModel.PatientCareGiverID), "id")]
public class ShareProfilePage : BasePage
{
    private int _patientCaregiverID;
    internal ShareProfileView _shareProfileView;
    /// <summary>
    /// PatientCaregiverid
    /// </summary>
    public string PatientCareGiverID
    {
        get { return _patientCaregiverID.ToString(CultureInfo.InvariantCulture); }
        set
        {
            _patientCaregiverID = Convert.ToInt32(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
        }
    }

    public ShareProfilePage()
    {
        _shareProfileView = new ShareProfileView(this, null);
       PageLayout.Add(_shareProfileView, 0, 0);
    }

    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _shareProfileView.Parameters = AddParameters(CreateParameter(nameof(UserRelationModel.PatientCareGiverID), PatientCareGiverID));
        await _shareProfileView.LoadUIAsync(false).ConfigureAwait(true);
        _shareProfileView.OnSaveSuccess += ProfileView_OnSaveSuccess;
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        _shareProfileView.OnSaveSuccess -= ProfileView_OnSaveSuccess;
        await _shareProfileView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }

    private async void ProfileView_OnSaveSuccess(object sender, EventArgs e)
    {
        var data = sender as UserDTO;
        if (data.ErrCode == ErrorCode.OK)
        {
            DisplayOperationStatus(data.ErrorDescription,true);
            await SyncDataWithServerAsync(Pages.ShareProfilePage,false,default).ConfigureAwait(true);
            await PopPageAsync(true).ConfigureAwait(true);
        }
        else
        {
            DisplayOperationStatus(data.ErrorDescription);
        }
    }

    public async override Task OnMenuActionClick(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    {
        if (menuAction == MenuAction.MenuActionSaveKey)
        {
            if (await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
            {
                AppHelper.ShowBusyIndicator = true;
                await _shareProfileView.OnSaveButtonClickedAsync().ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
            }
        }
    }
}