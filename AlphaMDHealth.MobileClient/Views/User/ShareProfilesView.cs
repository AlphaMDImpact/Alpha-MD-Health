using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class ShareProfilesView : BaseLibCollectionView
{
    private readonly CustomLabelControl _profileShareWithLabel;
    private readonly CustomLabelControl _shareLabel;
    private readonly Grid _bodyGrid;
    private UserDTO _userRelations;
    private long _patientCareGiverID;
    private TapGestureRecognizer _tapGestureRecognizer;

    public ShareProfilesView()
    {
        _bodyGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            BackgroundColor = Color.FromArgb(StyleConstants.GENERIC_BACKGROUND_COLOR),
            RowSpacing= Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
               
            },
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
                 new ColumnDefinition { Width = GridLength.Auto },
            }
        };

        CustomCellModel customCellModel = new CustomCellModel
        {
            CellID = nameof(UserRelationModel.PatientCareGiverID),
            CellHeader = nameof(UserRelationModel.CareGiverName),
            CellDescription = nameof(UserRelationModel.RoleName),
            //todo:CellLeftSourceIcon = nameof(UserRelationModel.ImageSource),
            CellLeftDefaultIcon = nameof(UserRelationModel.FromDateString),
            CellRightContentHeader = nameof(UserRelationModel.ShowRemoveButtonText),
            ShowRemoveButton = nameof(UserRelationModel.ShowRemoveButton),
            BandColor = nameof(UserRelationModel.ProgramColor),
        };
        _profileShareWithLabel = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft) ;
        _shareLabel = new CustomLabelControl(LabelType.LinkLabelSmallLeft);
        _bodyGrid.Add(_profileShareWithLabel, 0, 0);
      
        _bodyGrid.Add(_shareLabel, 1, 0);
        CollectionViewField = new CollectionView
        {
            ItemsLayout = ItemLayoutCreate(false),
            ItemTemplate = new DataTemplate(() =>
            {
                ResponsiveView view = new ResponsiveView(customCellModel);
                view.OnItemClicked += OnRemoveCareTakerClicked;
                return new ContentView { Content = view };
            })
        };
        _bodyGrid.Add(CollectionViewField, 0, 1);
        Grid.SetColumnSpan(CollectionViewField, 2);
        Content = _bodyGrid;
    }

    internal Task LoadUIAsync(UserDTO _userData, BasePage _parent)
    {
        ParentPage = _parent;
        _userRelations = _userData;
        _emptyListView.PageResources = _parent.PageData = _parent.PageData;
        _profileShareWithLabel.Text = _parent.GetResourceValueByKey(ResourceConstants.R_PROFILE_SHARE_WITH_KEY);
        _tapGestureRecognizer = new TapGestureRecognizer();
        _tapGestureRecognizer.Tapped += ShareProfileGestureRecognizer_Tapped;
        _shareLabel.GestureRecognizers.Add(_tapGestureRecognizer);
        _shareLabel.Text = _parent.GetResourceValueByKey(ResourceConstants.R_MENU_ACTION_SHARE_KEY);
        if (GenericMethods.IsListNotEmpty(_userData.UserRelations))
        {
            CollectionViewField.ItemsSource = _userData.UserRelations;
            OnListItemSelection(CareTakerProfileSelectionChanged, true);
            if (_userData.UserRelations.Count > 0)
            {
                CollectionViewField.HeightRequest = (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] * _userData.UserRelations.Count + new OnIdiom<int> { Phone = 10, Tablet = 0 };
            }
        }
        else
        {
            CollectionViewField.ItemsSource = new List<UserRelationModel>();
           RenderErrorView(_bodyGrid, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, false, false);
            CollectionViewField.HeightRequest = (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT];
        }

        return Task.CompletedTask;
    }

    public override Task UnloadUIAsync()
    {
        _tapGestureRecognizer.Tapped -= ShareProfileGestureRecognizer_Tapped;
        return Task.CompletedTask;
    }

    private async void OnRemoveCareTakerClicked(object sender, EventArgs e)
    {
        _patientCareGiverID = Convert.ToInt64((sender as CustomLabelControl).ClassId, CultureInfo.InvariantCulture);
        await ParentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnDeleteItemActionClicked, true, true, false).ConfigureAwait(true);
    }

    private async void OnDeleteItemActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 1:
                ParentPage.OnClosePupupAction(sender, e);
                if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
                {
                    var caregiver = _userRelations.UserRelations.FirstOrDefault(x => x.PatientCareGiverID == _patientCareGiverID);
                    CaregiverDTO caregiverData = new CaregiverDTO
                    {
                        Caregiver = new CaregiverModel(),
                    };
                    caregiverData.SelectedUserID = _patientCareGiverID;
                    caregiverData.Caregiver.PatientCareGiverID = Convert.ToInt64(_patientCareGiverID, CultureInfo.InvariantCulture);
                    caregiverData.Caregiver.OrganisationID = App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, 0); ;
                    caregiverData.Caregiver.CareGiverID = caregiver.CareGiverID;
                    caregiverData.Caregiver.FromDate = caregiver.FromDate;
                    caregiverData.Caregiver.ToDate = caregiver.ToDate;
                    caregiverData.IsActive = false;
                    await (ParentPage.PageService as UserService).SyncPatientCaregiverToServerAsync(caregiverData, CancellationToken.None).ConfigureAwait(true);
                    AppHelper.ShowBusyIndicator = false;
                    if (caregiverData.ErrCode == ErrorCode.OK)
                    {
                        AppHelper.ShowBusyIndicator = true;
                        await new BasePage().SyncDataWithServerAsync(Pages.ShareProfilePage, false, default).ConfigureAwait(true);
                        InvokeListRefresh(Guid.Empty, new EventArgs());
                        AppHelper.ShowBusyIndicator = false;
                    }
                    else
                    {
                        ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(caregiverData.ErrCode.ToString()));
                    }
                }
                break;
            case 2:
                ParentPage.OnClosePupupAction(sender, e);
                break;
            default:// to do
                break;
        }
    }

    private async void ShareProfileGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        _tapGestureRecognizer.Tapped -= ShareProfileGestureRecognizer_Tapped;
        await NavigateToPopupPage(Constants.ZERO).ConfigureAwait(false);
        _tapGestureRecognizer.Tapped += ShareProfileGestureRecognizer_Tapped;
    }

    private async Task NavigateToPopupPage(string patientCareGiverID)
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            var parameter = ParentPage.AddParameters(
             ParentPage.CreateParameter(nameof(UserRelationModel.PatientCareGiverID), patientCareGiverID));
            var shareProfilePopupPage = new ShareProfilePopupPage(ParentPage, parameter);
            shareProfilePopupPage.OnSaveButtonClicked += RefreshPatientShareList;
            //todo:await Navigation.PushPopupAsync(shareProfilePopupPage).ConfigureAwait(false);
        }
        else
        {
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(ShareProfilePage)
                      , GenericMethods.GenerateParamsWithPlaceholder(Param.id), patientCareGiverID
                  ).ConfigureAwait(true);

        }
    }

    private void RefreshPatientShareList(object sender, EventArgs e)
    {
        InvokeListRefresh(Guid.Empty, new EventArgs());
    }

    private async void CareTakerProfileSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            CollectionViewField.SelectionChanged -= CareTakerProfileSelectionChanged;
            var item = sender as CollectionView;
            if (item.SelectedItem != null)
            {
                var patientCareTaker = item.SelectedItem as UserRelationModel;
                await NavigateToPopupPage(patientCareTaker.PatientCareGiverID.ToString());
            }
            CollectionViewField.SelectionChanged += CareTakerProfileSelectionChanged;
        }
    }
}