using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Mopups.Services;

namespace AlphaMDHealth.MobileClient;

internal class AmhMessageControl : AmhBaseControl
{
    private List<ButtonActionModel> _actions = new List<ButtonActionModel>();
    private AmhMessagePopupPage _messagePopup;
    private Grid _mainLayout;
    //private StackLayout _container;
    private AmhImageControl _imageIcon;
    private AmhImageControl _closeButton;
    private AmhLabelControl _heading;
    private AmhLabelControl _content;
    private AmhLabelControl _placeholder;

    internal List<ButtonActionModel> Actions
    {
        get { return _actions; }
        set
        {
            if (_actions != value)
            {
                _actions = value;
            }
        }
    }

    private bool IsPopup => _fieldType.ToString().ToLower().Contains("popup");
    private bool _showPopup;

    /// <summary>
    /// ShowPopup
    /// </summary>
    internal bool ShowPopup
    {
        get { return _showPopup; }
        set
        {
            _showPopup = IsPopup || value;
            if (value)
            {
                ShowInPopup();
            }
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhMessageControl() : this(FieldTypes.Default)
    {
    }

    internal AmhMessageControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    private async void ShowInPopup()
    {
        _messagePopup = _messagePopup ?? new AmhMessagePopupPage(_mainLayout, _actions, ShellMasterPage.CurrentShell.CurrentPage);
        await MopupService.Instance.PushAsync(_messagePopup).ConfigureAwait(false);
        _messagePopup.OnActionClicked += _messagePopup_OnActionButtonClicked;
    }

    private void _messagePopup_OnActionButtonClicked(object sender, EventArgs e)
    {
        OnPopupClose(sender, e);
    }

    protected override void ApplyResourceValue()
    {
        _heading.Value = _resource.ResourceValue;
        _placeholder.Value = _resource.PlaceHolderValue;
        _content.Value = _resource.InfoValue;
        if (!string.IsNullOrWhiteSpace(_resource.KeyDescription))
        {
            _imageIcon.IsVisible = true;
            _imageIcon.Icon = _resource.KeyDescription;
            _imageIcon.ImageHeight = AppImageSize.ImageSizeXXXL;
           // _imageIcon.ImageWidth = AppImageSize.ImageSizeM;
        }
    }

    protected override void RenderControl()
    {
        _mainLayout = new Grid()
        {
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto } },
        };
        _heading = new AmhLabelControl(FieldTypes.PrimaryMediumHVCenterBoldLabelControl);
        _placeholder = new AmhLabelControl(FieldTypes.HtmlSecondaryCenterLabelControl);
        _imageIcon = new AmhImageControl(FieldTypes.ImageControl);
        _imageIcon.IsVisible = false;
        _content = new AmhLabelControl(FieldTypes.HtmlPrimaryCenterLabelControl);

        switch (_fieldType)
        {
            case FieldTypes.TopHeadingMessageControl:
            case FieldTypes.TopHeadingPopupMessageControl:
            case FieldTypes.TopHeadingWithCloseButtonMessageControl:
            case FieldTypes.TopHeadingWithCloseButtonPopupMessageControl:
                AddView(_mainLayout, _heading, 0, 1);
                AddView(_mainLayout, _imageIcon, 0, 2);              
                break;
            case FieldTypes.MessageControl:
            case FieldTypes.PopupMessageControl:
            case FieldTypes.CloseButtonMessageControl:
            case FieldTypes.CloseButtonPopupMessageControl:
            default:
                AddView(_mainLayout, _imageIcon, 0, 1);
                AddView(_mainLayout, _heading, 0, 2);
                Grid.SetColumnSpan(_heading, 2);
                break;
        }

        AddView(_mainLayout, _placeholder, 0, 3);
        AddView(_mainLayout, _content, 0, 4);
        Grid.SetColumnSpan(_placeholder, 2);
        Grid.SetColumnSpan(_imageIcon, 2);
        Grid.SetColumnSpan(_content, 2);

        if (_fieldType.ToString().ToLower().Contains("close") || IsPopup || _showPopup)
        {
            _closeButton = new AmhImageControl(FieldTypes.ImageControl)
            {
                Icon = ImageConstants.I_CLOSE_PNG,
                ImageWidth = AppImageSize.ImageSizeS,
                ImageHeight = AppImageSize.ImageSizeS,
            };
            _closeButton.OnValueChanged += OnPopupClose;
            AddView(_mainLayout, _closeButton, 1, 0);
        }

        Content = IsPopup ? null : _mainLayout;
    }

    private async void OnPopupClose(object sender, EventArgs e)
    {
        if (_showPopup)
        {
            await MopupService.Instance.PopAsync();
            _showPopup = false;
            OnValueChangedAction(sender, e);
        }
        else
        {
            _mainLayout.Clear();
            Content = null;
        }
    }

    protected override void EnabledDisableField(bool value) { }
}