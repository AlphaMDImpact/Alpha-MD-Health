using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Runtime.Serialization;

namespace AlphaMDHealth.WebClient;

public partial class StaticMessagePage : BasePage
{
    private BaseDTO _pageData = new BaseDTO();
    private List<ButtonActionModel> _actionsForBasePage;
    private List<ButtonActionModel> _actionsForConsent;

    [DataMember]
    private List<ButtonActionModel> _actionList;
    private PageType _messageType = PageType.Default;

    [Parameter]
    public string Title
    {
        get;
        set;
    }

    /// <summary>
    /// Resource key to display message into page
    /// </summary>
    [Parameter]
    public string Key
    {
        get;
        set;
    }

    /// <summary>
    /// Target to take action on click of action button
    /// </summary>
    [Parameter]
    public string TargetPage
    {
        get;
        set;
    }

    /// <summary>
    /// Target page parameters to pass during navigation on target page click of action button
    /// </summary>
    [Parameter]
    public string[] TargetParameters
    {
        get;
        set;
    }

    /// <summary>
    /// Type of message data source to fetch data from it
    /// </summary>
    [Parameter]
    public string Type { get => _messageType.ToString(); set { _messageType = value.ToEnum<PageType>(); } }

    /// <summary>
    /// Callback action event 
    /// </summary>
    [Parameter]
    public EventCallback<long> OnActionClicked { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetPageDataAsync().ConfigureAwait(true);
    }

    protected override async Task OnParametersSetAsync()
    {
        await GetPageDataAsync().ConfigureAwait(true);
        if (_isDataFetched)
        {
            StateHasChanged();
        }
        await base.OnParametersSetAsync();
    }

    private async Task GetPageDataAsync()
    {
        if (string.IsNullOrWhiteSpace(Key))
        {
            Key = ErrorCode.NotFound.ToString();
        }

        await RenderContentAsync().ConfigureAwait(true);
        if (_messageType == PageType.ConsentPage)
        {
            var actionResource = AppState.MasterData.Resources.FirstOrDefault(x => x.ResourceKey == (Convert.ToInt32(TargetParameters[0]) == 2
                ? ResourceConstants.R_DECLINE_KEY
                : ResourceConstants.R_I_ACCEPT_TEXT_KEY));
             
            if (actionResource != null)
            {
                _actionList = new List<ButtonActionModel>();

                var cancelActionResource = AppState.MasterData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_CANCEL_ACTION_KEY);
                _pageData.Resources.Add(cancelActionResource);
                var cancelButtonAction = new ButtonActionModel
                {
                    ButtonID = Constants.NUMBER_ZERO,
                    ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY,
                    ButtonAction = () => { OnMessageActionClickedAsync(Constants.NUMBER_ZERO); },
                    Icon = ImageConstants.PATIENT_MOBILE_BACK_SVG
                };
                _actionList.Add(cancelButtonAction);

                _pageData.Resources.Add(actionResource);
                var specificButtonAction = new ButtonActionModel
                {
                    ButtonID = TargetParameters[0],
                    ButtonResourceKey = actionResource.ResourceKey,
                    FieldType = FieldTypes.PrimaryButtonControl,
                    ButtonAction = () => { OnMessageActionClickedAsync(TargetParameters[0]); },
                    ButtonClass = IsPatientMobileView ? "mobile-view-button" : "",
                };
                _actionList.Add(specificButtonAction);

                _actionsForBasePage = new List<ButtonActionModel>();
                _actionsForConsent = new List<ButtonActionModel>();

                if (IsPatientMobileView)
                {
                    _actionsForBasePage.Add(cancelButtonAction);
                    _actionsForConsent.Add(specificButtonAction);
                }
                else
                {
                    _actionsForBasePage.AddRange(_actionList);
                }
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(TargetPage))
            {
                _actionsForBasePage = new List<ButtonActionModel>
                {
                    new ButtonActionModel{ ButtonID = Constants.NUMBER_TWO, ButtonResourceKey =ResourceConstants.R_NEXT_ACTION_KEY  },
                };
            }
        }
        _isDataFetched = true;
        StateHasChanged();
    }

    private async Task RenderContentAsync()
    {
        if (_messageType == PageType.ContentPage || _messageType == PageType.ConsentPage)
        {
            _pageData.AddedBy = Key;
            _pageData.LastModifiedBy = PageType.ContentPage.ToString();
            _pageData.LanguageID = AppState.SelectedLanguageID;
            await SendServiceRequestAsync(new ContentPageService(AppState.webEssentials).GetContentDetailsAsync(_pageData), _pageData).ConfigureAwait(true);
            Key = _pageData.AddedBy;
            if(IsPatientMobileView)
            {
                _pageData.Resources.FirstOrDefault(x => x.ResourceKey == Key).ResourceValue = null;
            }          
        }
        else
        {
            if (AppState?.MasterData?.Resources?.Count > 0)
            {
                _pageData.Resources = AppState.MasterData.Resources;
            }
        }
        if (_pageData == null || _pageData.Resources == null || _pageData.Resources.Count < 1)
        {
            new BaseService(AppState.webEssentials).GetDefaultText(_pageData, Key, AppState.SelectedLanguageID);
        }
    }

    /// <summary>
    /// Navigate on given target on click of action button
    /// </summary>
    /// <param name="index">Button index</param>
    private async Task OnMessageActionClickedAsync(object index)
    {
        if (_messageType == PageType.ConsentPage)
        {
            await OnActionClicked.InvokeAsync(Convert.ToInt64(index)).ConfigureAwait(true);
        }
        else
        {
            await NavigateToAsync(TargetPage, TargetParameters).ConfigureAwait(true);
        }
    }
}