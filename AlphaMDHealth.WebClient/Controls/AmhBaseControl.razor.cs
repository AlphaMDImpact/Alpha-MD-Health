using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AlphaMDHealth.WebClient;

public partial class AmhBaseControl
{
    protected ResourceModel _resource;

    #region Control Properties

    /// <summary>
    /// resource data for control
    /// </summary>
    [CascadingParameter]
    public IEnumerable<ResourceModel> PageResources { get; set; }

    [CascadingParameter]
    public EventCallback<KeyValuePair<string, object>> RegisterComponent { get; set; }

    /// <summary>
    /// Flag to avoid base structure
    /// </summary>
    [Parameter]
    public bool ApplyStructure { get; set; } = true;

    /// <summary>
    /// resource key of Control
    /// </summary>
    [Parameter]
    public string UniqueID { get; set; }

    /// <summary>
    /// resource key of Control
    /// </summary>
    [Parameter]
    public string Title { get; set; }

    protected string _resourceKey;
    /// <summary>
    /// resource key of Control
    /// </summary>
    [Parameter]
    public string ResourceKey
    {
        get => _resourceKey;
        set
        {
            if (value != _resourceKey)
            {
                if (!string.IsNullOrWhiteSpace(_resourceKey))
                {
                    // remove old key
                }
                _resourceKey = value;
                RegisterControl();
            }
            ApplyResources();
        }
    }

    /// <summary>
    /// display header or not
    /// </summary>
    [Parameter]
    public bool ShowHeader { get; set; } = true;

    private FieldTypes _fieldType;

    /// <summary>
    /// Get/ set Control type to Date and time
    /// </summary>
    [Parameter]
    public FieldTypes FieldType
    {
        get { return _fieldType; }
        set
        {
            if (_fieldType != value)
            {
                _fieldType = value;
                OnFieldTypeChanged();
            }
        }
    }

    protected string _leftIcon;
    protected string _rightIcon;
    private string _icon;
    /// <summary>
    /// Left icon should be set in svg path property
    /// </summary>
    [Parameter]
    public string Icon
    {
        get { return _icon; }
        set
        {
            if (_icon != value)
            {
                _leftIcon = string.Empty;
                _rightIcon = string.Empty;
                _icon = value;
                if (!string.IsNullOrWhiteSpace(_icon))
                {
                    var icons = _icon.Split("|");
                    if (icons.Length > 0 && !string.IsNullOrWhiteSpace(icons[0]))
                    {
                        _leftIcon = ImageConstants.WEB_IMAGE_PATH + icons[0];
                    }
                    if (icons.Length > 1 && !string.IsNullOrWhiteSpace(icons[1]))
                    {
                        _rightIcon = ImageConstants.WEB_IMAGE_PATH + icons[1];
                    }
                }
            }
        }
    }

    /// <summary>
    /// byte array to display image
    /// </summary>
    [Parameter]
    public byte[] Source { get; set; }

    /// <summary>
    /// enable disable control
    /// </summary>
    [Parameter]
    public bool IsControlEnabled { get; set; } = true;

    /// <summary>
    /// is valid to return whether control is validated
    /// </summary>
    [Parameter]
    public bool IsValid { get; set; } = true;

    private List<OptionModel> _options;
    /// <summary>
    /// Control value as object
    /// </summary>
    [Parameter]
    public List<OptionModel> Options
    {
        get { return _options; }
        set
        {
            if (value != _options)
            {
                _options = value;
                OnOptionsChanged();
                OptionsChanged.InvokeAsync(_options);
            }
        }
    }

    /// <summary>
    /// Bindable property of Options
    /// </summary>
    [Parameter]
    public EventCallback<List<OptionModel>> OptionsChanged { get; set; }

    private string _errorMessage;
    /// <summary>
    /// is valid to return whether control is validated
    /// </summary>
    [Parameter]
    public string ErrorMessage
    {
        get { return _errorMessage; }
        set
        {
            if (value != _errorMessage)
            {
                _errorMessage = GenericMethods.HtmlToPlainText(Regex.Replace(value,Constants.HTML_TAGS, string.Empty));
                ErrorMessageChanged.InvokeAsync(_errorMessage);
            }
        }
    }

    /// <summary>
    /// Bindable property of ErrorMessage
    /// </summary>
    [Parameter]
    public EventCallback<string> ErrorMessageChanged { get; set; }

    [Parameter]
    public bool IsButtonClick { get; set; }

    /// <summary>
    /// Bindable property of IsButtonClick
    /// </summary>
    [Parameter]
    public EventCallback<bool> IsButtonClickChanged { get; set; }

    /// <summary>
    /// It is used for style apply
    /// </summary>
    [Parameter]
    public string Class { get; set; } = string.Empty;

    /// <summary>
    /// It is used for style apply
    /// </summary>
    [Parameter]
    public string BaseClass { get; set; } = string.Empty; 

    /// <summary>
    /// On advance value change event
    /// </summary>
    [Parameter]
    public EventCallback<object?> OnValueChanged { get; set; }

    #endregion

    protected override Task OnInitializedAsync()
    {
        RegisterControl();
        return base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        ApplyResources();
        base.OnParametersSet();
    }

    /// <summary>
    /// value change event
    /// </summary>
    protected void OnValueChangedAction(object sender)
    {
        if (IsButtonClick)
        {
            ValidateControl(IsButtonClick);
        }
        OnValueChanged.InvokeAsync(sender);
    }

    /// <summary>
    /// method to invoke after option changed
    /// </summary>
    public virtual void OnOptionsChanged()
    {
    }

    /// <summary>
    /// method to invoke after field type changed
    /// </summary>
    public virtual void OnFieldTypeChanged()
    {
    }

    /// <summary>
    /// Validates a Control Abstract Method
    /// </summary>
    /// <param name="isButtonClick"></param>
    public virtual bool ValidateControl(bool isButtonClick)
    {
        return true;
    }

    public virtual bool ValidateControl(bool isButtonClick, Dictionary<string, object> controls)
    {
        return true;
    }

    #region Common Methods across controls

    public void SetValidationResult(bool isButtonClick)
    {
        IsValid = string.IsNullOrWhiteSpace(ErrorMessage);
        if (!IsButtonClick)
        {
            IsButtonClick = isButtonClick;
        }
    }

    protected string GetResourceValue(string resourceKey)
    {
        return LibResources.GetResourceValueByKey(PageResources, resourceKey);
    }

    /// <summary>
    /// initialize resources after assigning control resources
    /// </summary>
    private void ApplyResources()
    {
        if (!string.IsNullOrWhiteSpace(_resourceKey) && PageResources?.Count() > 0)
        {
            _resource = LibResources.GetResourceByKey(PageResources, _resourceKey);
            if (_resource != null && FieldType == FieldTypes.Default && !string.IsNullOrWhiteSpace(_resource.FieldType))
            {
                FieldType = _resource.FieldType.ToEnum<FieldTypes>();
            }
        }
    }

    protected string GetRequiredResourceValue()
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            GetResourceValue(ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY),
            _resource?.ResourceValue
        );
    }

    protected string GetPlaceholderText(string placeholderTxt)
    {
        return IsControlEnabled && !string.IsNullOrWhiteSpace(placeholderTxt)
            ? placeholderTxt
            : string.Empty;
    }

    protected List<long> FetchOptionIDsFromValue(string value)
    {
        return (!string.IsNullOrWhiteSpace(value))
            ? value.Split(Constants.PIPE_SEPERATOR)?.Select(x => Convert.ToInt64(x)).ToList() ?? new List<long>()
            : new List<long>();
    }

    #endregion

    private void RegisterControl()
    {
        if (!string.IsNullOrWhiteSpace(_resourceKey))
        {
            //register new key
            RegisterComponent.InvokeAsync(new KeyValuePair<string, object>(UniqueID + _resourceKey, this));
        }
    }
}