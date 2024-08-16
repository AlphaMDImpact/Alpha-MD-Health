using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Editors;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AlphaMDHealth.MobileClient;

internal class AmhEntryControl : AmhBaseControl
{
    internal TextEditBase _entry;

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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(AmhEntryControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhEntryControl control = (AmhEntryControl)bindable;
        if (newValue != null)
        {
            control.Value = newValue as string;
        }
    }

    /// <summary>
    /// Regex Value
    /// </summary>
    public string RegexExpression { get; set; }

    /// <summary>
    /// custom entry control initializer
    /// </summary>
    public AmhEntryControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// custom entry control initializer
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    public AmhEntryControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    /// <summary>
    /// Get value of control
    /// </summary>
    /// <returns>value of control</returns>
    private string GetControlValue()
    {
        return _entry?.Text;
    }

    /// <summary>
    /// Set value to control
    /// </summary>
    private void SetControlValue()
    {
        _entry.Text = _value;
    }

    private void OnEntryTextChanged(object sender, EventArgs e)
    {
        OnValueChangedAction(sender, e);
    }

    /// <summary>
    /// Render Control baseed on Specified type
    /// </summary>
    protected override void RenderControl()
    {
        if (_entry != null)
        {
            _entry.TextChanged -= OnEntryTextChanged;
            _entry = null;
        }
        _entry = _fieldType switch
        {
            FieldTypes.EmailEntryControl => new TextEdit { Keyboard = Keyboard.Email },
            FieldTypes.PasswordEntryControl => new PasswordEdit { Keyboard = Keyboard.Text },
            FieldTypes.NumericTextEntryControl => new TextEdit { Keyboard = Keyboard.Numeric },
            _ => new TextEdit { Keyboard = Keyboard.Text },
        };
        _entry.Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_MY_ENTRY_STYLE_KEY];
        _entry.Text = Value as string;
        _entry.TextChanged += OnEntryTextChanged;
        ApplyCopyPasteSetting();
        Content = _entry;
    }

    /// <summary>
    /// Apply's Resource Value Method implementation
    /// </summary>
    protected override void ApplyResourceValue()
    {
        if (_entry != null)
        {
            SetIcon(_entry);
            ApplyResource(_entry);
            if (IsControlEnabled && _resource != null)
            {
                _entry.MaxCharacterCount = (int)_resource.MaxLength;
                _entry.MaxCharacterCountOverflowMode = OverflowMode.LimitInput;
            }
        }
    }

    /// <summary>
    /// Method implementation to Enable Disable control
    /// </summary>
    /// <param name="value">flag representing control needs to enable or disable</param>
    protected override void EnabledDisableField(bool value)
    {
        SetFieldIsEnabled(_entry, value);
    }

    internal override void ValidateControl(bool isButtonClick)
    {
        if (_entry != null)
        {
            SetFieldError(_entry, false, string.Empty);
            if (IsControlEnabled && _resource != null)
            {
                if (_resource.IsRequired && string.IsNullOrWhiteSpace(_entry.Text))
                {
                    SetFieldError(_entry, true, GetRequiredResourceValue());
                }
                else if (!string.IsNullOrWhiteSpace(_entry.Text))
                {
                    if (_resource.MinLength != 0
                        && _entry.Text.Length < _resource.MinLength)
                    {
                        SetFieldError(_entry, true, string.Format(CultureInfo.CurrentCulture
                            , LibResources.GetResourceValueByKey(PageResources.Resources, ResourceConstants.R_MINIMUM_LENGTH_VALIDATION_KEY)
                            , _resource.MinLength));

                    }
                    else if (!_resource.MinLength.Equals(_resource.MaxLength)
                        && _resource.MinLength < _resource.MaxLength
                        && _entry.Text.Length > _resource.MaxLength)
                    {
                        SetFieldError(_entry, true, string.Format(CultureInfo.CurrentCulture
                            , LibResources.GetResourceValueByKey(PageResources.Resources, ResourceConstants.R_RANGE_LENGTH_VALIDATION_KEY)
                            , _resource.MinLength, _resource.MaxLength));
                    }
                    else if (!string.IsNullOrWhiteSpace(RegexExpression)
                        && !new Regex(RegexExpression).IsMatch(_entry.Text))
                    {
                        SetFieldError(_entry, true, string.Format(CultureInfo.CurrentCulture
                            , LibResources.GetResourceValueByKey(PageResources.Resources, ResourceConstants.R_INVALID_DATA_KEY)
                            , _resource.ResourceValue));
                    }
                }
            }
            IsValid = !_entry.HasError;
        }
        if (!_isButtonClick)
        {
            _isButtonClick = isButtonClick;
        }
    }

    private void ApplyCopyPasteSetting()
    {
        if (App._essentials.GetPreferenceValue(StorageConstants.PR_ALLOW_READ_COPY_CLIPBOARD_KEY, false))
        {
            var preventPasteBehavior = GetPreventPasteBehavior();
            _entry.Behaviors.Add(preventPasteBehavior);
        }
    }

    public PreventPasteBehavior GetPreventPasteBehavior()
    {
        return new PreventPasteBehavior();
    }
}

public class PreventPasteBehavior : Behavior<TextEditBase>
{
    private string _lastValue = "";

    protected override void OnAttachedTo(TextEditBase editor)
    {
        base.OnAttachedTo(editor);
        _lastValue = editor.Text;
        editor.TextChanged += OnTextChanged;
    }

    protected override void OnDetachingFrom(TextEditBase editor)
    {
        editor.TextChanged -= OnTextChanged;
        base.OnDetachingFrom(editor);
    }

    private void OnTextChanged(object sender, EventArgs e)
    {
        var editor = sender as TextEditBase;
        if (editor == null) return;

        // If the change in text is significant, it might be a paste operation.
        if (Math.Abs(editor.Text.Length - _lastValue.Length) > 1)
        {
            // Update the text indirectly to prevent recursive calls
            editor.SetValue(TextEditBase.TextProperty, _lastValue);
        }
        else
        {
            _lastValue = editor.Text;
        }
    }
}
