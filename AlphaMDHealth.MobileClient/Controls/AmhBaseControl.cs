using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Controls;
using DevExpress.Maui.Editors;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

internal abstract class AmhBaseControl : ContentView
{
    protected bool _isButtonClick;
    protected ResourceModel _resource;
    protected DXPopup infoPopup;

    #region Control Properties 

    /// <summary>
    /// display header or not
    /// </summary>
    internal bool ShowHeader { get; set; } = true;

    protected BaseDTO _pageResources;
    /// <summary>
    /// resource data for control
    /// </summary>
    internal BaseDTO PageResources
    {
        get => _pageResources;
        set
        {
            _pageResources = value;
            ApplyResources();
        }
    }

    protected string _resourceKey;
    /// <summary>
    /// resource key of Control
    /// </summary>
    internal string ResourceKey
    {
        get => _resourceKey;
        set
        {
            _resourceKey = value;
            ApplyResources();
        }
    }

    /// <summary>
    /// Unique ID of Control
    /// </summary>
    public string UniqueID { get; set; }

    /// <summary>
    /// Bindable ResourceKey
    /// </summary>
    internal static readonly BindableProperty ResourceKeyProperty = BindableProperty.Create(nameof(ResourceKeyProperty), typeof(string), typeof(AmhBaseControl), propertyChanged: ResourceKeyPropertyChange);
    private static void ResourceKeyPropertyChange(BindableObject bindable, object oldValue, object newValue)
    {
        AmhBaseControl control = (AmhBaseControl)bindable;
        if (newValue != null && (string)newValue != string.Empty)
        {
            control._resourceKey = (string)newValue;
        }
    }

    protected FieldTypes _fieldType;
    /// <summary>
    /// Get/ set Control type to Date and time
    /// </summary>
    internal FieldTypes FieldType
    {
        get => _fieldType;
        set
        {
            if (_fieldType != value)
            {
                _fieldType = value;
                RenderControl();
            }
        }
    }

    /// <summary>
    /// source property
    /// </summary>
    internal static readonly BindableProperty FieldTypeProperty = BindableProperty.Create(nameof(FieldType), typeof(FieldTypes), typeof(AmhBaseControl), propertyChanged: FieldTypePropertyChanged, defaultBindingMode: BindingMode.TwoWay);
    private static void FieldTypePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhBaseControl control = (AmhBaseControl)bindable;
        if (newValue != null && newValue is FieldTypes)
        {
            control.FieldType = (FieldTypes)newValue;
        }
    }

    protected string _icon;
    /// <summary>
    /// Left icon should be set in svg path property
    /// </summary>
    internal string Icon
    {
        get { return _icon; }
        set
        {
            if (_icon != value)
            {
                _icon = value;
                SetIcon();
            }
        }
    }

    /// <summary>
    /// source property
    /// </summary>
    internal static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(AmhBaseControl), propertyChanged: IconPropertyChanged, defaultBindingMode: BindingMode.TwoWay);
    private static void IconPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhBaseControl control = (AmhBaseControl)bindable;
        if (newValue != null)
        {
            control.Icon = newValue as string;
        }
    }

    protected byte[] _source;
    /// <summary>
    /// byte array to display image
    /// </summary>
    internal byte[] Source
    {
        get { return _source; }
        set
        {
            if (_source != value)
            {
                _source = value;
                SetIcon();
            }
        }
    }

    /// <summary>
    /// source property
    /// </summary>
    public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(byte[]), typeof(AmhBaseControl), propertyChanged: AmhBaseControl.SourcePropertyChanged, defaultBindingMode: BindingMode.TwoWay);
    private static void SourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhBaseControl control = (AmhBaseControl)bindable;
        if (newValue != null)
        {
            control.Source = newValue as byte[];
        }
    }

    private bool _isControlEnabled = true;
    /// <summary>
    /// enable disable control
    /// </summary>
    internal bool IsControlEnabled
    {
        get { return _isControlEnabled; }
        set
        {
            if (_isControlEnabled != value)
            {
                _isControlEnabled = value;
                EnabledDisableField(value);
            }
        }
    }

    /// <summary>
    /// IsControlEnabled property
    /// </summary>
    internal static readonly BindableProperty IsControlEnabledProperty = BindableProperty.Create(nameof(IsControlEnabled), typeof(bool), typeof(AmhBaseControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: AmhBaseControl.IsControlEnabledPropertyChanged);
    private static void IsControlEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhBaseControl control = (AmhBaseControl)bindable;
        if (newValue != null)
        {
            control.IsControlEnabled = (bool)newValue;
        }
    }

    internal List<OptionModel> _options;
    /// <summary>
    /// Control value as object
    /// </summary>
    internal List<OptionModel> Options
    {
        get
        {
            return _options;
        }
        set
        {
            if (_options != value)
            {
                _options = value;
                ApplyOptions();
            }
        }
    }

    /// <summary>
    /// value property
    /// </summary>
    internal static readonly BindableProperty OptionsProperty = BindableProperty.Create(nameof(Options), typeof(List<OptionModel>), typeof(AmhBaseControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: AmhBaseControl.OptionsPropertyChanged);
    private static void OptionsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhBaseControl control = (AmhBaseControl)bindable;
        if (newValue != null)
        {
            control.Options = newValue as List<OptionModel>;
        }
    }

    protected string _styleName;
    /// <summary>
    /// name of the style to apply in control
    /// </summary>
    internal string StyleName
    {
        get { return _styleName; }
        set
        {
            if (_styleName != value)
            {
                _styleName = value;
                ApplyStyle();
            }
        }
    }

    /// <summary>
    /// source property
    /// </summary>
    internal static readonly BindableProperty StyleNameProperty = BindableProperty.Create(nameof(StyleName), typeof(string), typeof(AmhBaseControl), propertyChanged: AmhBaseControl.StyleNamePropertyChanged, defaultBindingMode: BindingMode.TwoWay);
    private static void StyleNamePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhBaseControl control = (AmhBaseControl)bindable;
        if (newValue != null)
        {
            control.StyleName = newValue as string;
        }
    }

    /// <summary>
    /// is valid to return whether control is validated
    /// </summary>
    internal bool IsValid
    {
        get => (bool)GetValue(IsValidProperty);
        set => SetValue(IsValidProperty, value);
    }

    /// <summary>
    /// IsValid Property
    /// </summary>
    internal readonly BindableProperty IsValidProperty = BindableProperty.Create(nameof(IsValid), typeof(bool), typeof(AmhBaseControl), false);

    #endregion

    /// <summary>
    /// On advance value change event
    /// </summary>
    internal event EventHandler<EventArgs> OnValueChanged;

    /// <summary>
    /// MyBaseControl initializer
    /// </summary>
    internal AmhBaseControl()
    {
    }

    /// <summary>
    /// MyBaseControl initializer
    /// </summary>
    /// <param name="fieldType">type of control to render</param>
    internal AmhBaseControl(FieldTypes fieldType)
    {
        _fieldType = fieldType;
    }

    #region Methods to override in specific control


    /// <summary>
    /// Render Control based on Specified type
    /// </summary>
    protected abstract void RenderControl();

    /// <summary>
    /// Applies Resource Value Abstract Method
    /// </summary>
    protected abstract void ApplyResourceValue();

    /// <summary>
    /// Enabled Value Abstract Method
    /// </summary>
    /// <param name="value">flag representing control needs to enable or disable</param>
    protected abstract void EnabledDisableField(bool value);

    /// <summary>
    /// Validates a Control Abstract Method
    /// </summary>
    /// <param name="isButtonClick"></param>
    internal virtual void ValidateControl(bool isButtonClick)
    {
    }

    /// <summary>
    /// set icon
    /// </summary>
    protected virtual void SetIcon()
    {
    }

    /// <summary>
    /// Set Item source to control
    /// </summary>
    protected virtual void ApplyOptions()
    {
    }

    /// <summary>
    /// Update control style
    /// </summary>
    protected virtual void ApplyStyle()
    {
    }

    #endregion

    #region Common Methods across controls

    /// <summary>
    /// value change event
    /// </summary>
    /// <param name="sender">sender control object</param>
    /// <param name="e">event argument</param>
    protected void OnValueChangedAction(object sender, EventArgs e)
    {
        if (_isButtonClick)
        {
            ValidateControl(_isButtonClick);
        }
        OnValueChanged?.Invoke(sender, e);
    }

    /// <summary>
    /// initialize resources after assigning control resources
    /// </summary>
    protected void ApplyResources()
    {
        if (!string.IsNullOrWhiteSpace(_resourceKey) && GenericMethods.IsListNotEmpty(_pageResources?.Resources))
        {
            _resource = LibResources.GetResourceByKey(_pageResources?.Resources, _resourceKey);
            if (_resource != null && FieldType == FieldTypes.Default && !string.IsNullOrWhiteSpace(_resource.FieldType))
            {
                FieldType = _resource.FieldType.ToEnum<FieldTypes>();
            }
            ApplyResourceValue();
        }
    }

    protected void ApplyResource(EditBase field)
    {
        if (field != null && _resource != null)
        {
            field.PlaceholderText = GetPlaceholderText(_resource.PlaceHolderValue);
            if (ShowHeader)
            {
                field.LabelText = string.Join(Constants.CHAR_SPACE, _resource.ResourceValue, Constants.IS_REQUIRED_FEILD_INDICATOR);
            }
            SetInfoField(field);
        }
    }

    private void SetInfoField(EditBase field)
    {
        if (!string.IsNullOrWhiteSpace(_resource?.InfoValue))
        {
            if (SetIcon(field))
            {
                field.HelpText = _resource.InfoValue;
            }
            else
            {
                infoPopup ??= new DXPopup()
                {
                    HorizontalAlignment = PopupHorizontalAlignment.Stretch,
                    CloseOnScrimTap = true,
                    PlacementTarget = field,
                };
                infoPopup.Content = new StackLayout
                {
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_STACK_LAYOUT_KEY],
                    Children =
                    {
                        new AmhLabelControl(FieldTypes.HtmlPrimaryCenterLabelControl) { Value = _resource.InfoValue }
                    }
                };
                field.HelpText = string.Empty;
                field.EndIcon = ImageSource.FromFile(ImageConstants.I_INFO_ICON_PNG);
                field.EndIconClicked += OnInfoEndIconClicked;
            }
        }
    }

    private void OnInfoEndIconClicked(object sender, EventArgs e)
    {
        if (infoPopup.IsOpen)
        {
            infoPopup.IsOpen = false;
        }
        else
        {
            infoPopup.Show();
        }
    }

    protected void ApplyResource(AmhLabelControl header, AmhLabelControl placeHolderLabel, AmhLabelControl info)
    {
        if (_resource != null)
        {
            header.Value = _resource.ResourceValue;
            placeHolderLabel.Value = _resource.PlaceHolderValue;
            info.Value = _resource.InfoValue;
        }
    }

    protected void CreateWrapperControls(out Grid container, out AmhLabelControl header, out AmhLabelControl placeholder, out AmhLabelControl info, out AmhLabelControl error, bool withRowSpacing = true)
    {
        container = CreateGridContainer(withRowSpacing);
        CreateControlLabels(out header, out placeholder, out info, out error);
    }

    protected Grid CreateGridContainer(bool withRowSpacing)
    {
        Grid container = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
        };
        if (withRowSpacing)
        {
            container.RowSpacing = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING] / 2;
        }
        return container;
    }

    protected void CreateControlLabels(out AmhLabelControl header, out AmhLabelControl placeholder, out AmhLabelControl info, out AmhLabelControl error)
    {
        header = new AmhLabelControl(FieldTypes.PrimarySmallHStartVCenterLabelControl);
        placeholder = new AmhLabelControl(FieldTypes.SecondarySmallHStartVCenterLabelControl);
        info = new AmhLabelControl(FieldTypes.TertiarySmallHStartVCenterLabelControl);
        error = new AmhLabelControl(FieldTypes.ErrorHStartVCenterLabelControl);
    }

    protected void AddView(Grid container, View view, int col, int row)
    {
        if (col == 0)
        {
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }
        container.Add(view, col, row);
    }

    protected bool SetIcon(EditBase field)
    {
        if (!string.IsNullOrWhiteSpace(Icon))
        {
            var icons = Icon.Split("|");
            if (icons.Length > 0 && !string.IsNullOrWhiteSpace(icons[0]))
            {
                field.StartIcon = ImageSource.FromFile(icons[0]);
            }
            if (icons.Length > 1 && !string.IsNullOrWhiteSpace(icons[1]))
            {
                field.EndIcon = ImageSource.FromFile(icons[1]);
                return true;
            }
        }
        return false;
    }

    protected void ApplyOptions(ItemsEditBase field)
    {
        var itemsSource = Options.Select(item => item.OptionText).ToList();
        field.ItemsSource = itemsSource;
    }

    protected string GetRequiredResourceValue()
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            LibResources.GetResourceValueByKey(PageResources.Resources, ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY),
            _resource?.ResourceValue
        );
    }

    /// <summary>
    /// get resource value by resource key
    /// </summary>
    /// <param name="resourceKey"></param>
    /// <returns></returns>
    public string GetResourceValueByKey(string resourceKey)
    {
        return LibResources.GetResourceValueByKey(PageResources.Resources, resourceKey);
    }

    protected string GetPlaceholderText(string placeholderTxt)
    {
        return IsControlEnabled && !string.IsNullOrWhiteSpace(placeholderTxt)
            ? placeholderTxt
            : string.Empty;
    }

    protected void SetFieldIsEnabled(VisualElement field, bool value)
    {
        if (field != null)
        {
            field.IsEnabled = value;
        }
    }

    protected void SetFieldError(EditBase field, bool hasError, string errorMessage)
    {
        field.HasError = hasError;
        field.ErrorText = errorMessage;
    }

    protected void SetFieldError(AmhLabelControl errorLabel, string errorMessage)
    {
        errorLabel.Value = errorMessage;
    }

    public void SetValidationResult(bool isButtonClick, string errorMessage)
    {
        IsValid = string.IsNullOrWhiteSpace(errorMessage);
        if (!isButtonClick)
        {
            _isButtonClick = isButtonClick;
        }
        if (!_isButtonClick)
        {
            _isButtonClick = isButtonClick;
        }
    }

    protected List<long> FetchOptionIDsFromValue(string value)
    {
        return !string.IsNullOrWhiteSpace(value)
            ? value.Split(Constants.PIPE_SEPERATOR)?.Select(x => Convert.ToInt64(x)).ToList()
            : new List<long>();
    }

    #endregion
}