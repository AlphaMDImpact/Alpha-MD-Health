using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Editors;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

internal class AmhMultilineEntryControl : AmhBaseControl
{
    private MultilineEdit _editorControl;

    private string _value;
    /// <summary>
    /// Control value as string
    /// </summary>
    internal string Value
    {
        get
        {
            return GetControlValue();
        }
        set
        {
            if (_value != value)
            {
                _value = value;
                SetControlValue();
            }
        }
    }

    /// <summary>
    /// value property
    /// </summary>
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(AmhMultilineEntryControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhMultilineEntryControl control = (AmhMultilineEntryControl)bindable;
        if (newValue != null)
        {
            control.Value = newValue as string;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhMultilineEntryControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhMultilineEntryControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    private string GetControlValue()
    {
        return _editorControl?.Text.Trim();
    }

    private void SetControlValue()
    {
        _editorControl.Text = _value;
    }

    private void CreateTextEditor()
    {
        _editorControl = new MultilineEdit()
        {
            Text = Value as string,
        };
        _editorControl.TextChanged += OnTextChanged;
    }

    private void OnTextChanged(object sender, EventArgs e)
    {
        OnValueChangedAction(sender, e);
    }

    protected override void EnabledDisableField(bool value)
    {
        SetFieldIsEnabled(_editorControl, value);
    }

    protected override void RenderControl()
    {
        if (_editorControl != null)
        {
            _editorControl.TextChanged -= OnTextChanged;
            _editorControl = null;
        }
        CreateTextEditor();
        Content = _editorControl;
    }

    protected override void ApplyResourceValue()
    {
        if (_editorControl != null)
        {
            SetIcon(_editorControl);
            ApplyResource(_editorControl);
            if (IsControlEnabled)
            {
                _editorControl.MinLineCount = 2;
                //_editorControl.MaxLineCount = 4;
                _editorControl.MaxCharacterCount = (int)_resource.MaxLength;
                _editorControl.MaxCharacterCountOverflowMode = OverflowMode.LimitInput;
            }
        }
    }

    internal override void ValidateControl(bool isButtonClick)
    {
        if (_editorControl != null)
        {
            SetFieldError(_editorControl, false, string.Empty);
            if (IsControlEnabled)
            {
                if (_resource.IsRequired && string.IsNullOrWhiteSpace(_editorControl.Text))
                {
                    SetFieldError(_editorControl, true, GetRequiredResourceValue());
                }
                else if (!string.IsNullOrWhiteSpace(_editorControl.Text))
                {
                    if (_resource.MinLength != 0
                        && _editorControl.Text.Length < _resource.MinLength)
                    {
                        SetFieldError(_editorControl, true, string.Format(CultureInfo.CurrentCulture
                            , LibResources.GetResourceValueByKey(PageResources.Resources, ResourceConstants.R_MINIMUM_LENGTH_VALIDATION_KEY)
                            , _resource.MinLength));

                    }
                    else if (!_resource.MinLength.Equals(_resource.MaxLength)
                        && _resource.MinLength < _resource.MaxLength
                        && _editorControl.Text.Length > _resource.MaxLength)
                    {
                        SetFieldError(_editorControl, true, string.Format(CultureInfo.CurrentCulture
                            , LibResources.GetResourceValueByKey(PageResources.Resources, ResourceConstants.R_RANGE_LENGTH_VALIDATION_KEY)
                            , _resource.MinLength, _resource.MaxLength));
                    }
                }
            }
            IsValid = !_editorControl.HasError;
        }
        if (!_isButtonClick)
        {
            _isButtonClick = isButtonClick;
        }
    }
}
