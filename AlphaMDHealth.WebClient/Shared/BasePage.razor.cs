using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Blazor.Analytics;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Globalization;
using System.Runtime.Serialization;
using Wangkanai.Detection.Services;

namespace AlphaMDHealth.WebClient;

public partial class BasePage
{
    protected Dictionary<string, object> _controls = new Dictionary<string, object>();
    private string _pageTitle, _headerCompName, _headerIcon, _headerTitle, _headerSubTitle, _headerDescription;
    protected bool _isAfterLoginLayout, _isDataFetched = false, _modalHasBeenShown = false, _hideDeleteConfirmation = true;
    private IList<ButtonActionModel> _baseActions;
    private List<ButtonActionModel> _deleteConfirmationOptions;
    private string popupStyle = "modal show-flex ";
    public Modal _modal = default!;


    private bool isLoginView { get { return AppState.RouterData?.SelectedRoute?.Page == AppPermissions.LoginView.ToString(); } }

    [Inject]
    public IDetectionService DetectionService { get; set; }

    [Inject]
    private IAnalytics Analytics { get; set; }

    [Parameter]
    public RouterData RouterDataRoute { get; set; }

    [Parameter]
    [DataMember]
    public List<SystemFeatureParameterModel> Parameters { get; set; } = new List<SystemFeatureParameterModel>();

    /// <summary>
    /// Current page instance to perform operations
    /// </summary>
    [Parameter]
    public BasePage CurrentPage { get; set; }

    /// <summary>
    /// To render List of Action Buttons on the header
    /// </summary>
    [Parameter]
    public RenderFragment PageHeader { get; set; }

    /// <summary>
    /// Footer section to render action on page footer
    /// </summary>
    [Parameter]
    public RenderFragment PageFooter { get; set; }

    [Parameter]
    public RenderFragment ChildContent2 { get; set; }

    [Parameter]
    public string Content2Class { get; set; }

    /// <summary>
    /// Content to be wrapped in BasePage
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Apply Master Ready Made Page Structure
    /// </summary>
    [Parameter]
    public bool ApplyStructure { get; set; } = true;

    /// <summary>
    /// Is Page Add or Edit 
    /// </summary>
    [Parameter]
    public PageOperationType PageOperation { get; set; } = PageOperationType.View;

    /// <summary>
    /// list or Resources to get Control Label Values
    /// </summary>
    [Parameter]
    [DataMember]
    public IEnumerable<ResourceModel> PageResources { get; set; }

    /// <summary>
    /// Event Callback to Register a component for validation.
    /// </summary>
    [Parameter]
    public EventCallback<KeyValuePair<string, object>> RegisterComp { get; set; }

    [Parameter]
    public bool ApplyTab { get; set; } = true;

    /// <summary>
    /// Page Header
    /// </summary>
    [Parameter]
    public string PageTitle { get; set; }

    [Parameter]
    public bool ShowPageTitle { get; set; } = true;
    [Parameter]
    public bool SetDashboardFlag { get; set; } = false;

    /// <summary>
    /// RecordCount parameter
    /// </summary>
    [Parameter]
    public bool OnMenuClicked { get; set; }

    /// <summary>
    /// Set if Page has to be a popup page
    /// </summary>
    [Parameter]
    public bool IsPopup { get; set; }

    [Parameter]
    public bool ShowPopup { get; set; }

    [Parameter]
    public EventCallback<bool> ShowPopupChanged { get; set; }

    [Parameter]
    public bool ShowClose { get; set; } = true;

    /// <summary>
    /// Popup position
    /// </summary>
    [Parameter]
    public PopupPosition PopupPagePostion { get; set; } = PopupPosition.Center;

    /// <summary>
    /// Popup cancel callback action
    /// </summary>
    [Parameter]
    public EventCallback<string> OnClose { get; set; }

    /// <summary>
    /// Set if Page has to be opened as detail page in place of list page or not
    /// </summary>
    [Parameter]
    public bool ShowDetailPage { get { return AppState?.ShowDetailPage ?? false; } set { AppState.ShowDetailPage = value; } }

    /// <summary>
    /// Set when show IsDetailPage 
    /// </summary>
    [Parameter]
    public EventCallback<bool> ShowDetailPageChanged { get; set; }

    /// <summary>
    /// Set when show Success Banner is Needed
    /// </summary>
    [Parameter]
    public string Success { get; set; }

    /// <summary>
    /// Set when show Success Banner is Needed
    /// </summary>
    [Parameter]
    public EventCallback<string> SuccessChanged { get; set; }

    /// <summary>
    /// Set when show Error Banner is Needed
    /// </summary>
    [Parameter]
    public string Error { get; set; }

    /// <summary>
    /// Set when show Error Banner is Needed
    /// </summary>
    [Parameter]
    public EventCallback<string> ErrorChanged { get; set; }

    /// <summary>
    /// Apply Fixed Footer to footer content
    /// </summary>
    [Parameter]
    public bool ApplyFixedFooter { get; set; }

    [Parameter]
    public bool IsPatientMobileView { get { return AppState.GetTempToken(); } set { AppState.GetTempToken(); } }


    [Inject]
    public IJSRuntime JsRuntime { get; set; }

    [Parameter]
    public bool IsAccordion { get; set; }

    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public string FooterClass { get; set; }

    [Parameter]
    public bool ApplyCard { get; set; }

    [Parameter]
    public bool ApplyParentStructure { get; set; } = true;

    [Parameter]
    public bool ApplyMobileViewBeforLogin { get; set; } = false;


    [Parameter]
    public IList<ButtonActionModel> ActionButtons { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _isAfterLoginLayout = LocalStorage.IsUserAuthenticated && !AppState.IsBeforeLoginLayout;
        if (HasManadatoryWelcomeScreens())
        {
            await AppState.NavigateToAsync(AppPermissions.UserWelcomeScreensView.ToString(), true).ConfigureAwait(false);
        }
        else if (IsManadatoryConsentNotAccepted())
        {
            await AppState.NavigateToAsync(AppPermissions.UserConsentsView.ToString(), true).ConfigureAwait(false);
        }
        else if (IsSubscriptionRequired())
        {
            await AppState.NavigateToAsync(AppPermissions.SubscriptionPlansView.ToString(), true).ConfigureAwait(false);
        }
        else if (IsProfileNotCompleted())
        {
            await AppState.NavigateToAsync(AppPermissions.ProfileView.ToString(), true).ConfigureAwait(false);
        }
        else if (await RenderTabHeaderAsync().ConfigureAwait(true))
        {
            ApplyPopupStyle();
            SetPageTitle();
            StateHasChanged();
        }
        if (IsPatientMobileView)
        {
            ApplyFixedFooter = true;
        }
        await base.OnInitializedAsync();
    }

    private bool HasManadatoryWelcomeScreens()
    {
        return _isAfterLoginLayout
            && AppState.MasterData != null
            && AppState.MasterData.HasWelcomeScreens
            && AppState.RouterData?.SelectedRoute?.Page != AppPermissions.UserWelcomeScreensView.ToString()
            && AppState.RouterData?.SelectedRoute?.Page != AppPermissions.StaticMessageView.ToString();
    }

    private bool IsManadatoryConsentNotAccepted()
    {
        return _isAfterLoginLayout && AppState.MasterData != null
            && !AppState.MasterData.IsConsentAccepted
            && AppState.RouterData?.SelectedRoute?.Page != AppPermissions.UserConsentsView.ToString()
            && AppState.RouterData?.SelectedRoute?.Page != AppPermissions.StaticMessageView.ToString();
    }

    private bool IsSubscriptionRequired()
    {
        return _isAfterLoginLayout && AppState.MasterData != null
            && AppState.MasterData.IsSubscriptionRequired
            && AppState.RouterData?.SelectedRoute?.Page != AppPermissions.SubscriptionPlansView.ToString()
            && AppState.RouterData?.SelectedRoute?.Page != AppPermissions.StaticMessageView.ToString();
    }

    private bool IsProfileNotCompleted()
    {
        return _isAfterLoginLayout && AppState.MasterData != null
            && !AppState.MasterData.IsProfileCompleted
            && AppState.RouterData?.SelectedRoute?.Page != AppPermissions.ProfileView.ToString()
            && AppState.RouterData?.SelectedRoute?.Page != AppPermissions.StaticMessageView.ToString();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrWhiteSpace(Error) || !string.IsNullOrWhiteSpace(Success))
        {
            await ResetErrorAsync().ConfigureAwait(true);
        }

        if (await RenderTabHeaderAsync().ConfigureAwait(true))
        {
            SetPageTitle();
            if (IsPopup && ShowPopup)
            {
                ApplyPopupStyle();
                if (_modal != default && !_modalHasBeenShown)
                {
                    await _modal.ShowAsync().ConfigureAwait(true);
                    _modalHasBeenShown = true;
                }
            }
            StateHasChanged();
            await base.OnParametersSetAsync();
        }
    }

    private async Task<bool> RenderTabHeaderAsync()
    {
        if (AppState.Tabs?.Count > 0 && !OnMenuClicked)
        {
            _headerCompName = string.Empty;
            _baseActions = new List<ButtonActionModel>();
            if (AppState.RouterData?.Routes.Count() > 0)
            {
                if (HasTabHeader(AppPermissions.OrganisationTabView) || HasTabHeader(AppPermissions.BranchTabView))
                {
                    _headerCompName = AppPermissions.OrganisationTabView.ToString();
                    _headerIcon = LibSettings.GetSettingValueByKey(AppState.MasterData.Settings, SettingsConstants.S_LOGO_KEY);
                    if (string.IsNullOrWhiteSpace(_headerIcon))
                    {
                        if (string.IsNullOrWhiteSpace(AppState.OrganizationDetails?.OrganisationName))
                        {
                            _headerIcon = string.IsNullOrWhiteSpace(AppState.MasterData.OrganisationName)
                                ? string.Empty
                                : AppState.MasterData.OrganisationName.Substring(0, 1);
                        }
                        else
                        {
                            _headerIcon = AppState.OrganizationDetails.OrganisationName.Substring(0, 1);
                        }
                    }
                    _headerTitle = FetchOrganizationName();
                    _headerSubTitle = string.IsNullOrWhiteSpace(AppState.OrganizationDetails?.OrganisationDomain)
                        ? AppState.MasterData?.OrganisationDomain ?? string.Empty
                        : AppState.OrganizationDetails.OrganisationDomain;
                    _headerDescription = string.IsNullOrWhiteSpace(AppState.OrganizationDetails?.AddedON.ToString())
                     ? (AppState.MasterData?.AddedON?.ToString("MMMM dd, yyyy") ?? string.Empty)
                     : AppState.OrganizationDetails.AddedON.ToString("MMMM dd, yyyy");
                }
                else if (!AppState.IsPatient)
                {
                    if (HasTabHeader(AppPermissions.PatientTabView))
                    {
                        if (await HasUserDataAsync().ConfigureAwait(true))
                        {
                            _headerCompName = AppPermissions.PatientTabView.ToString();
                            GenerateUserHeaderView(new List<string>{
                            string.Concat(AppState.UserDetails.User?.FirstName, Constants.STRING_SPACE, AppState.UserDetails.User?.LastName)
                            , AppState.UserDetails.User?.UserAge.ToString(CultureInfo.InvariantCulture)
                            , !string.IsNullOrWhiteSpace(AppState.UserDetails.User?.GenderID)
                            ? LibResources.GetResourceValueByKey(AppState.UserDetails.Resources, AppState.UserDetails.User?.GenderID)
                            : string.Empty });
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (HasTabHeader(AppPermissions.UserTabView))
                    {
                        if (await HasUserDataAsync().ConfigureAwait(true))
                        {
                            _headerCompName = AppPermissions.UserTabView.ToString();
                            GenerateUserHeaderView(GetUsersTitleParts());
                            _headerDescription = AppState.UserDetails.User.RoleName;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            StateHasChanged();
            //await TabHeaderActionsChanged.InvokeAsync(TabHeaderActions).ConfigureAwait(true);
        }
        return true;
    }

    public string FetchOrganizationName()
    {
        return AppState.OrganizationDetails?.OrganisationName == null
            ? AppState.MasterData?.OrganisationName
            : string.Concat(AppState.OrganizationDetails?.OrganisationName);
    }

    private async Task<bool> HasUserDataAsync()
    {
        if (AppState.UserDetails.User == null || AppState.UserDetails.User?.UserID == 0)
        {
            if (CurrentPage == null)
            {
                await AppState.NavigateToAsync(NavigateToUsersPage(), true).ConfigureAwait(false);
            }
            else
            {
                //to do: Add user detail service call here
                await CurrentPage.NavigateToAsync(NavigateToUsersPage()).ConfigureAwait(true);
            }
            return false;
        }
        return true;
    }

    private void GenerateUserHeaderView(List<string> parts)
    {
        GeneratePatientActionButtons();

        _headerIcon = string.IsNullOrWhiteSpace(AppState.UserDetails.User?.ImageName)
            ? AppState.UserDetails.User.ImageName
            : AppState.UserDetails.User.ImageName;
        _headerTitle = string.Join(Constants.PIPE_SEPERATOR_WITH_SPACE, parts);
        _headerSubTitle = string.Join(Constants.PIPE_SEPERATOR_WITH_SPACE
            , AppState.UserDetails.User.EmailId
            , AppState.UserDetails.User.PhoneNo);
    }


    private void GeneratePatientActionButtons()
    {
        AppState.SelectedTabTitle = $"{AppState.UserDetails.User?.FirstName} {AppState.UserDetails.User?.LastName}";
        if ((_headerCompName == AppPermissions.UserTabView.ToString() && LibPermissions.HasActivePermission(AppState.MasterData.OrganisationFeatures, AppPermissions.UserDelete.ToString()))
            || (_headerCompName == AppPermissions.PatientTabView.ToString() && LibPermissions.HasActivePermission(AppState.MasterData.OrganisationFeatures, AppPermissions.PatientDelete.ToString())))
        {
            AddButton(ResourceConstants.R_DELETE_ACTION_KEY, FieldTypes.DeleteBorderTransparentExButtonControl, 5);
        }
        if (AppState.UserDetails.User?.IsTempPassword ?? false)
        {
            AddButton(ResourceConstants.R_SEND_ACTIVATION_AGAIN_KEY, FieldTypes.PrimaryBorderTransparentExButtonControl, 4);
        }
        AddButton(ResourceConstants.R_CANCEL_ACTION_KEY, FieldTypes.PrimaryBorderTransparentExButtonControl, 3);
    }

    private void AddButton(string resourceKey, FieldTypes type, int val)
    {
        _baseActions.Add(new ButtonActionModel
        {
            ButtonResourceKey = resourceKey,
            FieldType = type,
            ButtonAction = async () =>
            {
                await OnTabActionClickAsync(val).ConfigureAwait(true);
            }
        });
    }

    private List<string> GetUsersTitleParts()
    {
        List<string> parts = new List<string>() { string.Concat(AppState.UserDetails?.User?.FirstName, Constants.STRING_SPACE, AppState.UserDetails?.User?.LastName) };
        if (!string.IsNullOrWhiteSpace(AppState.UserDetails?.User?.GenderID))
        {
            parts.Add(LibResources.GetResourceValueByKey(AppState.UserDetails.Resources, AppState.UserDetails?.User?.GenderID));
        }
        if (AppState.UserDetails?.User?.ProffessionID > 0)
        {
            parts.Add(AppState.UserDetails?.Professions?.FirstOrDefault(x => x.OptionID == AppState.UserDetails?.User?.ProffessionID)?.OptionText ?? string.Empty);
        }

        return parts;
    }

    private bool HasTabHeader(AppPermissions permission)
    {
        return AppState.RouterData.Routes.Any(x => x.FeatureId == AppState.Tabs[0].FeatureGroupID && (x.Page == permission.ToString()));
    }

    private async Task OnTabActionClickAsync(int buttonID)
    {
        if (CurrentPage != null)
        {
            switch (buttonID)
            {
                case 1:
                    await CurrentPage.NavigateToAsync(AppPermissions.QuestionnairesView.ToString()).ConfigureAwait(true);
                    break;
                case 2:
                    await CurrentPage.NavigateToAsync(AppPermissions.QuestionnaireAddEdit.ToString(),
                    AppState.webEssentials.GetPreferenceValue(StorageConstants.PR_QUESTIONNAIRE_ID_KEY, 0).ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
                    break;
                case 3:
                    if (AppState.UserDetails.User.IsViewedFromOther && _headerCompName == AppPermissions.UserTabView.ToString())
                    {
                        await CurrentPage.NavigateToAsync(AppPermissions.ManageOrganisationView.ToString()
                            , Convert.ToString(AppState.UserDetails.User.SelectedOrganisationID, CultureInfo.InvariantCulture)
                        ).ConfigureAwait(true);
                    }
                    else
                    {
                        await CurrentPage.NavigateToAsync(NavigateToUsersPage()).ConfigureAwait(true);
                    }
                    break;
                case 4:
                    UserDTO user = new UserDTO
                    {
                        User = new UserModel
                        {
                            UserID = AppState.UserDetails.User.UserID,
                            IsUser = AppState.UserDetails.User.RoleID != 4,
                            OrganisationDomain = AppState.MasterData.OrganisationDomain
                        }
                    };
                    await SendServiceRequestAsync(new UserService(AppState.webEssentials).ResendActivationAsync(user, new CancellationToken()), user).ConfigureAwait(true);
                    await OnActionClickAsync(user.ErrCode).ConfigureAwait(true);
                    break;
                case 5:
                    _deleteConfirmationOptions = new List<ButtonActionModel>{
                         new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
                         new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
                    };
                    _hideDeleteConfirmation = false;
                    StateHasChanged();
                    break;
                default:
                    // do not act
                    break;
            }
        }
    }

    private string NavigateToUsersPage()
    {
        return _headerCompName == AppPermissions.PatientTabView.ToString() ? AppPermissions.PatientsView.ToString() : AppPermissions.UsersView.ToString();
    }

    private async Task PopUpCallbackAsync(object e)
    {
        _hideDeleteConfirmation = true;
        if (e != null && e.ToString() == "1")
        {
            UserDTO userData = AppState.UserDetails;
            userData.User.IsActive = false;
            await SendServiceRequestAsync(new UserService(AppState.webEssentials).SyncUserToServerAsync(userData, CancellationToken.None), userData).ConfigureAwait(true);
            if (userData.ErrCode == ErrorCode.OK)
            {
                if (AppState.UserDetails.User.IsViewedFromOther && _headerCompName == AppPermissions.UserTabView.ToString())
                {
                    await CurrentPage.NavigateToAsync(AppPermissions.ManageOrganisationView.ToString()
                        , Convert.ToString(AppState.UserDetails.User.SelectedOrganisationID, CultureInfo.InvariantCulture)).ConfigureAwait(true);
                }
                else
                {
                    await CurrentPage.NavigateToAsync(NavigateToUsersPage()).ConfigureAwait(true);
                }
            }
            await OnActionClickAsync(userData.ErrCode).ConfigureAwait(true);
        }
    }

    private async Task ResetErrorAsync()
    {
        await Task.Delay(Constants.DELAY_IN_MILLISECONDS).ConfigureAwait(true);
        await ErrorChanged.InvokeAsync(string.Empty).ConfigureAwait(false);
        await SuccessChanged.InvokeAsync(string.Empty).ConfigureAwait(false);
        StateHasChanged();
    }

    private void CheckErrorMessage(ref string message, ref bool isSuccess)
    {
        if (!string.IsNullOrWhiteSpace(Success))
        {
            message = LibResources.GetResourceValueByKey(PageResources, Success);
            if (string.IsNullOrWhiteSpace(message))
            {
                message = Success;
            }
            isSuccess = true;
        }
        else if (!string.IsNullOrWhiteSpace(Error))
        {
            message = LibResources.GetResourceValueByKey(PageResources, Error);
            if (string.IsNullOrWhiteSpace(message))
            {
                message = Error;
            }
        }
    }

    protected async Task OnCancelClickAsync()
    {
        IsPopup = false;
        ShowPopup = false;
        await ShowPopupChanged.InvokeAsync(false);
        popupStyle = "modal";
        StateHasChanged();
        await OnClose.InvokeAsync("").ConfigureAwait(true);
    }

    private async Task OnActionClickAsync(ErrorCode errorCode)
    {
        if (errorCode == ErrorCode.OK)
        {
            Success = errorCode.ToString();
        }
        else
        {
            Error = errorCode.ToString();
        }
        StateHasChanged();
        await Task.Delay(Constants.DELAY_IN_MILLISECONDS).ConfigureAwait(true);
        Success = string.Empty;
        Error = string.Empty;
        StateHasChanged();
    }

    private void ApplyPopupStyle()
    {
        if (PopupPagePostion != PopupPosition.Center)
        {
            switch (PopupPagePostion)
            {
                case PopupPosition.End:
                    popupStyle = "modal show model-edit-end";
                    break;
                case PopupPosition.CenterLarge:
                    popupStyle = "modal-large show";
                    break;
                default:
                    //DefaultCase
                    break;
            }
        }
    }

    private string GetPopupStyle()
    {
        string style = "height: auto !important;";
        style += PopupPagePostion switch
        {
            PopupPosition.Center => "max-height:calc(90vh - 150px)!important;",
            PopupPosition.CenterLarge => "max-height:calc(100vh - 70px) !important;",
            _ => "max-height:calc(100vh - 150px) !important;",
        };
        style += "overflow-y: auto; overflow-x: hidden; ";
        return style;
    }

    private void SetPageTitle()
    {
        var addEditText = (
                PageOperation == PageOperationType.Add
                ? LibResources.GetResourceValueByKey(AppState.MasterData?.Resources, ResourceConstants.R_ADD_ACTION_KEY)
                : PageOperation == PageOperationType.Edit
                ? LibResources.GetResourceValueByKey(AppState.MasterData?.Resources, ResourceConstants.R_EDIT_ACTION_KEY)
                : LibResources.GetResourceValueByKey(AppState.MasterData?.Resources, ResourceConstants.R_VIEW_BUTTON_KEY)
            ) ?? string.Empty;
        if (ShowPageTitle)
        {
            _pageTitle = string.IsNullOrWhiteSpace(PageTitle)
                ? AppState.RouterData?.SelectedRoute?.FeatureText
                : PageTitle;

            if (!string.IsNullOrWhiteSpace(_pageTitle))
            {
                _pageTitle = _pageTitle.Contains("{0}")
                   ? string.Format(CultureInfo.InvariantCulture, _pageTitle, addEditText)
                   : _pageTitle;
            }
            else
            {
                _pageTitle = addEditText;
            }
        }
        else
        {
            _pageTitle = string.Empty;
        }
    }

    private List<OptionModel> GetTabOptions()
    {
        var options = new List<OptionModel>();
        if (AppState?.Tabs != null)
        {
            foreach (var (item, index) in AppState?.Tabs?.Select((value, i) => (value, i)))
            {
                var option = new OptionModel
                {
                    ParentOptionID = item.FeatureParentID,
                    OptionID = item.FeatureID,
                    OptionText = item.FeatureText,
                    ParentOptionText = item.FeatureCode,
                    IsSelected = item.FeatureCode == AppState.SelectedTab
                };


                if (IsPatientMobileView)
                {
                    if (index > 4)
                    {
                        option.IsActive = false;
                    }
                    //break;
                }
                options.Add(option);
            }
        }
        return options;
    }

    public async Task OnTabChangeAsync(object value)
    {
        if (value != null && value is long)
        {
            var selectedFeatureID = (long)value;
            var selectedTab = AppState.Tabs?.FirstOrDefault(x => x.FeatureID == selectedFeatureID);
            if (selectedTab != null && AppState.SelectedTab != selectedTab.FeatureCode)
            {
                AppState.SelectedTab = selectedTab.FeatureCode;
                StateHasChanged();
                if (CurrentPage != null)
                {
                    await CurrentPage.NavigateToAsync(selectedTab.FeatureCode);
                }
            }
        }
    }

    /// <summary>
    /// Fetch parameter detais by name
    /// </summary>
    /// <param name="name">name of parameter</param>
    /// <returns>feature parameter</returns>
    protected SystemFeatureParameterModel GetParameter(string name)
    {
        return Parameters?.FirstOrDefault(x => x.ParameterName == name);
    }

    /// <summary>
    /// Fetch parameter value by name
    /// </summary>
    /// <param name="name">name of parameter</param>
    /// <returns>parameter value</returns>
    protected string GetParameterValue(string name)
    {
        return GetParameter(name)?.ParameterValue;
    }

    /// <summary>
    /// Handles based on the error response
    /// </summary>
    /// <param name="result">Result obtained from service</param>
    /// <returns>Status based on whether the response required any handling</returns>
    public async Task ExecuteTaskResultAsync(BaseDTO result)
    {
        switch (result?.ErrCode)
        {
            case ErrorCode.PinCodeLogin:
                result.ErrCode = ErrorCode.HandledRedirection;
                await NavigateToAsync(AppPermissions.PincodeLoginView.ToString(), true).ConfigureAwait(false);
                break;
            case ErrorCode.Unauthorized:
            case ErrorCode.TokenExpired:
                await new AuthService(AppState.webEssentials).ClearAccountTokensAndIdAsync().ConfigureAwait(false);
                result.ErrCode = ErrorCode.HandledRedirection;
                if (!string.IsNullOrWhiteSpace(new AuthService(AppState.webEssentials).GetSecuredValueAsync(nameof(TempSessionModel.TokenIdentifier)).Result))
                {
                    var selectedRoute = AppState.RouterData.SelectedRoute != null ? AppState.RouterData.SelectedRoute.Page : string.Empty;
                    await JSRuntime.InvokeVoidAsync("invokeWebviewMethod", "tokenexpired", selectedRoute);
                }
                else
                {
                    await NavigateToAsync(AppPermissions.LoginView.ToString(), true).ConfigureAwait(false);
                }
                break;
            case ErrorCode.Forbidden:
                result.ErrCode = ErrorCode.HandledRedirection;
                await NavigateToAsync(AppPermissions.DashboardView.ToString(), true).ConfigureAwait(false);
                break;
            case ErrorCode.InActiveUser:
                await new AuthService(AppState.webEssentials).ClearAccountTokensAndIdAsync().ConfigureAwait(true);
                result.ErrCode = ErrorCode.HandledRedirection;
                await AppState.NavigateToAsync(AppState.GetDefaultRoute(), true).ConfigureAwait(false);
                break;
            case ErrorCode.PlanExpired:
            case ErrorCode.RenewPlan:
                result.ErrCode = ErrorCode.HandledRedirection;
                break;
            case ErrorCode.OrganisationSetup:
                await NavigateToAsync(AppPermissions.OrganisationSetup.ToString(), true).ConfigureAwait(false);
                result.ErrCode = ErrorCode.HandledRedirection;
                break;
            case ErrorCode.SetPinCode:
                result.ErrCode = ErrorCode.HandledRedirection;
                await NavigateToAsync(AppPermissions.PinCodeView.ToString(), true).ConfigureAwait(false);
                break;
            case ErrorCode.MultipleUsers:
                result.ErrCode = ErrorCode.HandledRedirection;
                await NavigateToAsync(AppPermissions.MultipleUsersView.ToString(), true).ConfigureAwait(false);
                break;
            case ErrorCode.ConsentRequired:
                result.ErrCode = ErrorCode.HandledRedirection;
                await NavigateToAsync(AppPermissions.UserConsentsView.ToString()).ConfigureAwait(false);
                break;
            case ErrorCode.ServiceUnavailable:
            case default(ErrorCode):
                await NavigateToRouteAsync(AppPermissions.StaticMessageView.ToString(), false, ErrorCode.ServiceUnavailable.ToString()).ConfigureAwait(false);
                result.ErrCode = ErrorCode.HandledRedirection;
                break;
            default:
                //will use for future implementation
                break;
        }
    }

    protected async Task LoadMasterPageDataAsync(long accountID)
    {
        try
        {
            // To do: Ask local web notification permission
            AppState.Loader.ShowLoader(true);
            await SetupLocalStorageAsync().ConfigureAwait(true);
            FetchSystemDateTimeSetting();
            MasterDTO masterData = await new MasterService(AppState.webEssentials).GetMasterDataAsync(NavigationManager.Uri, accountID).ConfigureAwait(true);
            await CallIPDataApiForGeoLocationAsync(masterData);
            AppState.RenderAppState(masterData, new Uri(NavigationManager.BaseUri).Host);
            AppState.webEssentials.SetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, masterData.PermissionAtLevelID);
            await ExecuteTaskResultAsync(masterData).ConfigureAwait(true);
            //await SetupSignalRAsync().ConfigureAwait(true);
            AppState.webEssentials.SetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, masterData.LanguageID);
            AppState.webEssentials.SetPreferenceValue<long>(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, masterData.OrganisationID);

            AppState.Loader.ShowLoader(true);
        }
        catch (Exception ex)
        {
        }
    }

    private void FetchSystemDateTimeSetting()
    {
        var localizationOptions = new RequestLocalizationOptions().DefaultRequestCulture.UICulture.DateTimeFormat;
        bool is24HourFormat = localizationOptions.ShortTimePattern.StartsWith(Constants.DEFAULT_TIME_FORMAT);
        AppState.webEssentials.SetPreferenceValue(StorageConstants.PR_IS_24_HOUR_FORMAT, is24HourFormat);
    }

    private async Task SetupLocalStorageAsync()
    {
        string storageKey = StorageConstants.PR_LOCAL_STORAGE_KEY;
        if (await LocalStorageService.ContainKeyAsync(storageKey).ConfigureAwait(false))
        {
            StorageState storageState = await LocalStorageService.GetItemAsync<StorageState>(storageKey).ConfigureAwait(false);
            LocalStorage.DeviceModel = storageState.DeviceModel;
            LocalStorage.DeviceManufacturer = storageState.DeviceManufacturer;
            LocalStorage.DeviceName = storageState.DeviceName;
            LocalStorage.DeviceOS = storageState.DeviceOS;
            LocalStorage.DeviceOSVersionString = storageState.DeviceOSVersionString;
            LocalStorage.SecuredStorage = storageState.SecuredStorage;
            LocalStorage.Preferences = storageState.Preferences;
        }
        else
        {
            LocalStorage.DeviceModel = DetectionService.Browser.Name.ToString();
            LocalStorage.DeviceManufacturer = LocalStorage.DeviceModel;
            LocalStorage.DeviceName = LocalStorage.DeviceModel;
            LocalStorage.DeviceOS = DetectionService.Platform.Name.ToString();
            LocalStorage.DeviceOSVersionString = DetectionService.Browser.Version.ToString();
            LocalStorage.SecuredStorage = new Dictionary<string, string>();
            LocalStorage.Preferences = new Dictionary<string, object>();
            await LocalStorageService.SetItemAsync(storageKey, LocalStorage).ConfigureAwait(false);
        }
        AppState.webEssentials = new WebEssentials(LocalStorage);
    }

    private async Task SetupSignalRAsync()
    {
        //todo:
        //if (AppState != null)
        //{
        //    AppState.SignalRConnection = new HubConnectionBuilder().WithUrl(new Uri(await new AccountLibService().GetSelectedBaseUrlAsync(UrlConstants.DEFAULT_ENVIRONMENT_KEY_VALUE).ConfigureAwait(false) + Constants.SE_SIGNALR_ENDPOINT)).Build();
        //    AppState.SignalRConnection.On("ReceiveNotification", (string notificationMessageType, string notificationID, string notificationFromID, bool isSilent) =>
        //    {
        //        if (AppState.RouterData.SelectedRoute.Page == AppPermissions.ChatsView.ToString() || AppState.RouterData.SelectedRoute.Page == AppPermissions.ChatView.ToString() || AppState.RouterData.SelectedRoute.Page == AppPermissions.ChatAddEdit.ToString())
        //        {
        //            AppState.InvokeReceiveNotification(new SignalRNotificationEventArgs { NotificationID = notificationID, NotificationMessageType = notificationMessageType, NotificationFromID = notificationFromID, IsSilent = isSilent });
        //        }
        //        else
        //        {
        //            // To do: Show local web notification
        //        }
        //    });
        //    await AppState.SignalRConnection.StartAsync();
        //}
    }

    protected async Task CallIPDataApiForGeoLocationAsync(MasterDTO masterData)
    {
        string defaultCountryCodeValue = masterData.Settings?.FirstOrDefault(x => x.SettingKey == SettingsConstants.S_DEFAULT_COUNTRY_CODE_PATH_KEY)?.SettingValue;
        if (!string.IsNullOrWhiteSpace(defaultCountryCodeValue))
        {
            await JsRuntime.InvokeAsync<string>("getCountryCode", DotNetObjectReference.Create(this), defaultCountryCodeValue).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Sets selected page along with tabs if required
    /// </summary>
    /// <param name="selectedMenu">Selected route data</param>
    /// <param name="e">parameter</param>
    /// <returns>sets selected page along with tabs if required</returns>
    public async Task SetSelectedPageAsync(RouterData selectedMenu, long e)
    {
        await SetSelectedPageAsync(selectedMenu, e, true);
    }

    /// <summary>
    /// Sets selected page along with tabs if required
    /// </summary>
    /// <param name="selectedMenu">Selected route data</param>
    /// <param name="e">parameter</param>
    /// <returns>sets selected page along with tabs if required</returns>
    public async Task SetSelectedPageAsync(RouterData selectedMenu, long e, bool isMenuClicked)
    {
        if (AppState != null)
        {
            AppState.SelectedTabTitle = selectedMenu.FeatureText;
            ////Analytics.TrackEvent("PageHit", selectedMenu?.Page);
            AppState.Tabs = AppState.MasterData.FeaturePermissions.Where(x => x.FeatureGroupID == selectedMenu.FeatureId)?.OrderBy(x => x.SequenceNo).ToList();
            if (AppState.Tabs?.Count > 0)
            {
                AppState.SelectedTab = AppState.Tabs[0].FeatureCode;
                AppState.IsChanged = !AppState.IsChanged;
                StateHasChanged();
                if (AppState.RouterData.Routes.Any(x => x.Page == AppState.SelectedTab))
                {
                    if (e > 0)
                    {
                        await NavigateToAsync(isMenuClicked, AppState.SelectedTab, Convert.ToString(e, CultureInfo.InvariantCulture)).ConfigureAwait(false);
                    }
                    else
                    {
                        await NavigateToAsync(isMenuClicked, AppState.SelectedTab).ConfigureAwait(false);
                    }
                }
                else
                {
                    await NavigateToAsync(isMenuClicked, AppPermissions.StaticMessageView.ToString(), ErrorCode.NotImplemented.ToString()).ConfigureAwait(false);
                }
            }
            else
            {
                AppState.SelectedTab = string.Empty;
                AppState.Tabs = new List<OrganizationFeaturePermissionModel>();
                StateHasChanged();
                await NavigateToAsync(isMenuClicked, selectedMenu.Page).ConfigureAwait(false);
            }
        }
    }

    protected async Task SetLoginData(AuthDTO authData)
    {
        BaseService baseServices = new BaseService(AppState.webEssentials);
        await baseServices.SaveSecuredValueAsync(StorageConstants.SS_USER_NAME_KEY, authData.AuthenticationData.UserName).ConfigureAwait(true);
        AppState.webEssentials.SetPreferenceValue(StorageConstants.PR_REMEMBER_ME_KEY, authData.AuthenticationData.RememberMe);
        await baseServices.SaveSecuredValueAsync(StorageConstants.PR_USER_CRED_KEY, authData.AuthenticationData.AccountPassword).ConfigureAwait(true);
    }

    /// <summary>
    /// Navigate to a input route
    /// </summary>
    /// <param name="route">Page Route for Navigation</param>
    /// <param name="forceLoad">Should App Refresh</param>
    /// <param name="queryParams">Page Input Query Parameters</param>
    protected async Task RefreshAndNavigateToAsync(AuthDTO authData, bool isOrganizationSwitched, string route)
    {
        if (isOrganizationSwitched)
        {
            await LoadMasterPageDataAsync(authData.AccountID).ConfigureAwait(true);
        }
        await SetLoginData(authData).ConfigureAwait(true);
        await NavigateToAsync(route, true).ConfigureAwait(true);
    }

    /// <summary>
    /// Navigate to a input route
    /// </summary>
    /// <param name="route">Page Route for Navigation</param>
    /// <param name="forceLoad">Should App Refresh</param>
    /// <param name="queryParams">Page Input Query Parameters</param>
    public async Task NavigateToAsync(string route, bool forceLoad, params string[] queryParams)
    {
        if (AppState != null)
        {
            if (route == AppPermissions.LoginView.ToString())
            {
                AppState.MasterData.IsConsentAccepted = true;
                AppState.MasterData.IsConsentAccepted = true;
                AppState.MasterData.IsSubscriptionRequired = false;
                AppState.MasterData.IsProfileCompleted = true;
                AppState.MasterData.HasWelcomeScreens = false;
            }

            if (AppState?.MasterData?.HasWelcomeScreens ?? false
                && AppState.RouterData?.SelectedRoute?.Page != AppPermissions.UserWelcomeScreensView.ToString())
            {
                route = AppPermissions.UserWelcomeScreensView.ToString();
                forceLoad = true;
            }
            else if (!AppState?.MasterData?.IsConsentAccepted ?? false
                && AppState.RouterData?.SelectedRoute?.Page != AppPermissions.UserConsentsView.ToString())
            {
                route = AppPermissions.UserConsentsView.ToString();
                forceLoad = true;
            }
            else if (AppState?.MasterData?.IsSubscriptionRequired ?? false
                && AppState.RouterData?.SelectedRoute?.Page != AppPermissions.SubscriptionPlansView.ToString())
            {
                route = AppPermissions.SubscriptionPlansView.ToString();
                forceLoad = true;
            }
            else if (!AppState?.MasterData?.IsProfileCompleted ?? false
                && AppState.RouterData?.SelectedRoute?.Page != AppPermissions.ProfileView.ToString())
            {
                route = AppPermissions.ProfileView.ToString();
                forceLoad = true;
            }
            var navigationString = AppState.NavigationString(route, queryParams);
            if (!string.IsNullOrWhiteSpace(navigationString))
            {
                await AppState.NavigateToAsync(navigationString, forceLoad).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Navigate to a input route
    /// </summary>
    /// <param name="route">Page Route for Navigation</param>
    /// <param name="queryParams">Page Input Query Parameters</param>
    public async Task NavigateToAsync(string route, params string[] queryParams)
    {
        await NavigateToAsync(false, route, queryParams);
    }

    /// <summary>
    /// Navigate to a input route
    /// </summary>
    /// <param name="route">Page Route for Navigation</param>
    /// <param name="queryParams">Page Input Query Parameters</param>
    public async Task NavigateToAsync(bool isMenuClicked, string route, params string[] queryParams)
    {
        if (isMenuClicked && route == AppPermissions.AppointmentsView.ToString() && (queryParams == null || queryParams.Length < 1))
        {
            queryParams = new[] { "true" };
        }
        var selectedMenu = AppState.RouterData.Routes.FirstOrDefault(x => x.Page == route);
        if (selectedMenu != null)
        {
            //checked requested component belongs to same tab or not
            if (await IsComponentInSameTabAsync(route, queryParams).ConfigureAwait(true))
            {
                return;
            }
            // Check selected route is tab group or not
            await NavigateAsync(CheckFeatureIsTab(isMenuClicked, route, selectedMenu), queryParams).ConfigureAwait(false);
        }
        else
        {
            await NavigateAsync(AppPermissions.StaticMessageView.ToString(), ErrorCode.NotImplemented.ToString()).ConfigureAwait(false);
        }
    }

    private string CheckFeatureIsTab(bool isMenuClicked, string route, RouterData selectedMenu)
    {
        if (AppState != null)
        {
            OrganizationFeaturePermissionModel tab = FilterFeatureTab(isMenuClicked, selectedMenu);
            // Apply features tab
            if (AppState.Tabs?.Count > 0)
            {
                AppState.SelectedTab = tab == null ? AppState.Tabs[0].FeatureCode : route;
                AppState.IsChanged = !AppState.IsChanged;
                return AppState.SelectedTab;
            }
        }
        return route;
    }

    private OrganizationFeaturePermissionModel FilterFeatureTab(bool isMenuClicked, RouterData selectedMenu)
    {
        if (AppState != null)
        {
            //// Check feature is a group or not
            AppState.Tabs = AppState.MasterData.FeaturePermissions?.Where(x => x.FeatureGroupID == selectedMenu.FeatureId)?.OrderBy(x => x.SequenceNo).ToList();
            if (!isMenuClicked && (AppState.Tabs == null || AppState.Tabs.Count < 1))
            {
                //// Check Feature is a child page of any group or not
                var tab = AppState.MasterData.FeaturePermissions?.FirstOrDefault(x => x.FeatureID == selectedMenu.FeatureId);
                if (tab != null)
                {
                    AppState.Tabs = AppState.MasterData.FeaturePermissions.Where(x => x.FeatureGroupID == tab.FeatureGroupID)?.OrderBy(x => x.SequenceNo).ToList();
                }
                return tab;
            }
        }
        return null;
    }

    private async Task<bool> IsComponentInSameTabAsync(string route, params string[] queryParams)
    {
        if (AppState != null)
        {
            if (AppState.Tabs?.Count > 0 && AppState.Tabs.FirstOrDefault(x => x.FeatureCode == route) != null)
            {
                AppState.SelectedTab = route;
                AppState.IsChanged = !AppState.IsChanged;
                await NavigateAsync(route, queryParams).ConfigureAwait(false);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Navigate to a input route
    /// </summary>
    /// <param name="route">Page Route for Navigation</param>
    /// <param name="queryParams">Page Input Query Parameters</param>
    private Task NavigateAsync(string route, params string[] queryParams)
    {
        return NavigateToAsync(route, false, queryParams);
    }

    /// <summary>
    /// Navigate to a input string route
    /// </summary>
    /// <param name="route">Page Route for Navigation</param>
    /// <param name="forceLoad">Should App Refresh</param>
    /// <param name="queryParams">Page Input Query Parameters</param>
    public async Task NavigateToRouteAsync(string route, bool forceLoad, params string[] queryParams)
    {
        if (!string.IsNullOrWhiteSpace(route) && AppState != null)
        {
            var routeString = route;
            if (queryParams?.Length > 0)
            {
                routeString += Constants.SYMBOL_SLASH + string.Join(Constants.SYMBOL_SLASH, queryParams.Where(x => !string.IsNullOrWhiteSpace(x)));
            }
            await AppState.NavigateToAsync(routeString, forceLoad).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Sends service request and executes task
    /// </summary>
    /// <param name="serviceTask">service call task</param>
    /// <param name="request">response error code</param>
    /// <returns>Executes task and returns result</returns>
    public async Task SendServiceRequestAsync(Task serviceTask, BaseDTO request)
    {
        AppState.Loader.ShowLoader(true);
        AppState.TaskCount = AppState.TaskCount + 1;
        await serviceTask.ConfigureAwait(true);
        await ExecuteTaskResultAsync(request).ConfigureAwait(true);
        AppState.TaskCount = AppState.TaskCount - 1;
        if (AppState.TaskCount == 0)
        {
            AppState.Loader.ShowLoader(false);
        }
    }

    protected long GetSelectedID(List<OptionModel> options)
    {
        return options?.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0;
    }

    #region Validation Methods

    /// <summary>
    /// Page operation based on parameter
    /// </summary>
    /// <param name="isEditable">Is page editable</param>
    /// <param name="isAdd">isAdd Page</param>
    public PageOperationType GetPageOperationType(bool isEditable, bool isAdd)
    {
        if (isEditable)
        {
            return isAdd
            ? PageOperationType.Add
            : PageOperationType.Edit;
        }
        return PageOperationType.View;
    }

    /// <summary>
    /// use to Register control
    /// </summary>
    /// <param name="control">control</param>
    public void RegisterControl(KeyValuePair<string, object> control)
    {
        if (!string.IsNullOrWhiteSpace(control.Key))
        {
            if (control.Key.Contains("LanguagesField"))
            {
                RemoveControlByKey(control.Key);
            }
            if (!_controls.ContainsKey(control.Key))
            {
                _controls.Add(control.Key, control.Value);
            }
        }
    }

    /// <summary>
    /// Removes registered control from list based on received key
    /// </summary>
    /// <param name="key">Control key</param>
    public void RemoveControlContainsKey(string key)
    {
        var controls = _controls.Keys.Where(k => k.Contains(key));
        if (controls.Count() > 0)
        {
            foreach (var item in controls)
            {
                _controls.Remove(item);
            }
        }
    }

    /// <summary>
    /// Removes registered control from list based on received key
    /// </summary>
    /// <param name="key">Control key</param>
    public void RemoveControlByKey(string key)
    {
        if (_controls.ContainsKey(key))
        {
            _controls.Remove(key);
        }
    }

    /// <summary>
    /// Removes registered control from list based on received object
    /// </summary>
    /// <param name="control">Control object</param>
    public void RemoveControl(object control)
    {
        foreach (var item in _controls)
        {
            if (item.Value.GetType() == (Type)control)
            {
                _controls.Remove(item.Key);
            }
        }
    }

    public void ClearControls()
    {
        _controls.Clear();
    }

    /// <summary>
    /// use to validate controls
    /// </summary>
    /// <returns>valid true/false</returns>
    public bool IsValid()
    {
        bool isFormValid = true;
        bool controlValid;
        foreach (KeyValuePair<string, object> control in _controls)
        {
            if (control.Value is AmhBaseControl)
            {
                if (control.Key.Contains("LanguagesTab"))
                {
                    controlValid = ((AmhBaseControl)control.Value).ValidateControl(true, _controls);
                }
                else
                {
                    controlValid = ((AmhBaseControl)control.Value).ValidateControl(true);
                }
                isFormValid = isFormValid && controlValid;
            }
        }
        return isFormValid;
    }

    #endregion
}