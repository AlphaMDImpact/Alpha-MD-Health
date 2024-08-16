using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

internal class MessageControlDemo : BasePage
{
    private readonly AmhMessageControl _messageControl;
    private readonly AmhMessageControl _closeButtonMessageControl;
    private readonly AmhMessageControl _topHeadingMessageControl;
    private readonly AmhMessageControl _topHeadingCloseButtonMessageControl;

    private readonly AmhMessageControl _messagePopupControl;
    private readonly AmhMessageControl _closeButtonMessagePopupControl;
    private readonly AmhMessageControl _topHeadingMessagePopupControl;
    private readonly AmhMessageControl _topHeadingWithCloseButtonMessagePopupControl;

    private readonly AmhButtonControl _btnMessagePopupControl;
    private readonly AmhButtonControl _btnCloseButtonMessagePopupControl;
    private readonly AmhButtonControl _btnTopHeadingMessagePopupControl;
    private readonly AmhButtonControl _btnTopHeadingWithCloseButtonMessagePopupControl;

    private readonly AmhButtonControl _primaryButton;

    private List<ButtonActionModel> _actions;

    public MessageControlDemo() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.StartAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);

        _messageControl = new AmhMessageControl(FieldTypes.MessageControl)
        {
            ResourceKey = FieldTypes.MessageControl.ToString(),
        };
        AddView(_messageControl);

        _closeButtonMessageControl = new AmhMessageControl(FieldTypes.CloseButtonMessageControl)
        {
            ResourceKey = FieldTypes.CloseButtonMessageControl.ToString(),
        };
        AddView(_closeButtonMessageControl);

        _topHeadingMessageControl = new AmhMessageControl(FieldTypes.TopHeadingMessageControl)
        {
            ResourceKey = FieldTypes.TopHeadingMessageControl.ToString(),
        };
        AddView(_topHeadingMessageControl);

        _topHeadingCloseButtonMessageControl = new AmhMessageControl(FieldTypes.TopHeadingWithCloseButtonMessageControl)
        {
            ResourceKey = FieldTypes.TopHeadingWithCloseButtonMessageControl.ToString(),
        };
        AddView(_topHeadingCloseButtonMessageControl);

        _messagePopupControl = new AmhMessageControl(FieldTypes.PopupMessageControl)
        {
            ResourceKey = FieldTypes.PopupMessageControl.ToString(),
        };
        AddView(_messagePopupControl);

        _closeButtonMessagePopupControl = new AmhMessageControl(FieldTypes.CloseButtonPopupMessageControl)
        {
            ResourceKey = FieldTypes.CloseButtonPopupMessageControl.ToString(),
        };
        AddView(_closeButtonMessagePopupControl);

        _topHeadingMessagePopupControl = new AmhMessageControl(FieldTypes.TopHeadingPopupMessageControl)
        {
            ResourceKey = FieldTypes.TopHeadingPopupMessageControl.ToString(),
        };
        AddView(_topHeadingMessagePopupControl);

        _topHeadingWithCloseButtonMessagePopupControl = new AmhMessageControl(FieldTypes.TopHeadingWithCloseButtonPopupMessageControl)
        {
            ResourceKey = FieldTypes.TopHeadingWithCloseButtonPopupMessageControl.ToString(),
        };
        AddView(_topHeadingWithCloseButtonMessagePopupControl);

        _btnMessagePopupControl = new AmhButtonControl(FieldTypes.PrimaryButtonControl);
        AddView(_btnMessagePopupControl);

        _btnCloseButtonMessagePopupControl = new AmhButtonControl(FieldTypes.PrimaryButtonControl);
        AddView(_btnCloseButtonMessagePopupControl);

        _btnTopHeadingMessagePopupControl = new AmhButtonControl(FieldTypes.PrimaryButtonControl);
        AddView(_btnTopHeadingMessagePopupControl);

        _btnTopHeadingWithCloseButtonMessagePopupControl = new AmhButtonControl(FieldTypes.PrimaryButtonControl);
        AddView(_btnTopHeadingWithCloseButtonMessagePopupControl);

        _primaryButton = new AmhButtonControl(FieldTypes.PrimaryButtonControl)
        {
            Icon = ImageConstants.I_USER_ID_PNG,
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_primaryButton);

    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await new AuthService(App._essentials).GetAccountDataAsync(PageData).ConfigureAwait(true);

        var demoService = new ControlDemoService(App._essentials);
        demoService.GetControlDemoPageResources(PageData);

        _primaryButton.PageResources = PageData;

        _actions = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE,ButtonResourceKey = ResourceConstants.R_SAVE_ACTION_KEY, },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO,ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };

        _messageControl.PageResources = PageData;
        _closeButtonMessageControl.PageResources = PageData;
        _topHeadingMessageControl.PageResources = PageData;
        _topHeadingCloseButtonMessageControl.PageResources = PageData;

        _messagePopupControl.PageResources = PageData;
        _messagePopupControl.Actions = _actions;
        _messagePopupControl.OnValueChanged += _messagePopupControl_OnValueChanged1;


        _closeButtonMessagePopupControl.PageResources = PageData;
        _closeButtonMessagePopupControl.Actions = _actions;
        _topHeadingMessagePopupControl.PageResources = PageData;
        _topHeadingMessagePopupControl.Actions = _actions;
        _topHeadingWithCloseButtonMessagePopupControl.PageResources = PageData;
        _topHeadingWithCloseButtonMessagePopupControl.Actions = _actions;

        _btnMessagePopupControl.Value = "MessagePopupControl";
        _btnCloseButtonMessagePopupControl.Value = "CloseButtonMessagePopupControl";
        _btnTopHeadingMessagePopupControl.Value = "TopHeadingMessagePopupControl";
        _btnTopHeadingWithCloseButtonMessagePopupControl.Value = "TopHeadingWithCloseButtonMessagePopupControl";

        _btnMessagePopupControl.OnValueChanged += _messagePopupControl_OnValueChanged; ;
        _btnCloseButtonMessagePopupControl.OnValueChanged += _closeButtonMessagePopupControl_OnValueChanged;
        _btnTopHeadingMessagePopupControl.OnValueChanged += _topHeadingMessagePopupControl_OnValueChanged;
        _btnTopHeadingWithCloseButtonMessagePopupControl.OnValueChanged += _topHeadingWithCloseButtonMessagePopupControl_OnValueChanged;

        
        _primaryButton.OnValueChanged += _primaryButton_OnValueChanged; ;
        AppHelper.ShowBusyIndicator = false;
    }

    private void _messagePopupControl_OnValueChanged1(object sender, EventArgs e)
    {

    }

    private void _topHeadingWithCloseButtonMessagePopupControl_OnValueChanged(object sender, EventArgs e)
    {
        _topHeadingWithCloseButtonMessagePopupControl.ShowPopup = true;
    }

    private void _topHeadingMessagePopupControl_OnValueChanged(object sender, EventArgs e)
    {
        _topHeadingMessagePopupControl.ShowPopup = true;
    }

    private void _closeButtonMessagePopupControl_OnValueChanged(object sender, EventArgs e)
    {
        _closeButtonMessagePopupControl.ShowPopup = true;
    }

    private void _messagePopupControl_OnValueChanged(object sender, EventArgs e)
    {
        _messagePopupControl.ShowPopup = true;
    }

    private async void _primaryButton_OnValueChanged(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
    }

    private void AddView(View view)
    {
        int index = PageLayout.Children?.Count() ?? 0;
        PageLayout.Add(view, 0, index);
    }
}
