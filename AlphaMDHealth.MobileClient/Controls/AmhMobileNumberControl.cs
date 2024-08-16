using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

internal class AmhMobileNumberControl : AmhBaseControl
{
    private AmhSingleSelectDropDownControl _countryCodeDropDown;
    private AmhNumericEntryControl _mobileEntry;
    private Grid _container;

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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(AmhMobileNumberControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhMobileNumberControl control = (AmhMobileNumberControl)bindable;
        if (newValue != null)
        {
            control.Value = (string)newValue;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhMobileNumberControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhMobileNumberControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    /// <summary>
    /// Get value of control
    /// </summary>
    /// <returns>value of control</returns>
    private string GetControlValue()
    {
        var countrycode = _countryCodeDropDown.Options.FirstOrDefault(x => x.OptionID == Convert.ToInt64(_countryCodeDropDown.Value)).OptionText;
        return countrycode + Constants.SYMBOL_DASH + _mobileEntry.Value;
    }

    /// <summary>
    /// Set value to control
    /// </summary>
    private void SetControlValue()
    {
        if (!string.IsNullOrWhiteSpace(_value))
        {
            var mobileNumberParts = (_value)?.Split('-');
            if (mobileNumberParts.Length == 2)
            {
                if (!string.IsNullOrWhiteSpace(mobileNumberParts[0]))
                {
                    _countryCodeDropDown.Value = _countryCodeDropDown.Options?.FirstOrDefault(x => x.OptionText == mobileNumberParts[0])?.OptionID ?? default;
                }
                if (!string.IsNullOrWhiteSpace(mobileNumberParts[1]))
                {
                    decimal.TryParse(mobileNumberParts[1], CultureInfo.InvariantCulture, out decimal val);
                    _mobileEntry.Value = val;
                }
            }
        }
    }

    /// <summary>
    /// enable/disable control
    /// </summary>
    /// <param name="value"></param>
    protected override void EnabledDisableField(bool value)
    {
        _countryCodeDropDown.IsControlEnabled = value;
        _mobileEntry.IsControlEnabled = value;
    }

    /// <summary>
    /// Render Control based on Specified type
    /// </summary>
    protected override void RenderControl()
    {
        _container?.Children.Clear();
        _container = CreateGridContainer(false);
        _container.ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
        };

        _countryCodeDropDown = new AmhSingleSelectDropDownControl(FieldTypes.SingleSelectWithoutBorderDropDownControl)
        {
            ZIndex = 1,
        };
        _container.Add(_countryCodeDropDown, 0, 0);

        _mobileEntry = new AmhNumericEntryControl(FieldTypes.NumericEntryControl)
        {
            ResourceKey = _resourceKey,
            PageResources = _pageResources,
        };
        _mobileEntry.OnValueChanged += OnTextChanged;
        _mobileEntry.OnValueChanged += OnSelectionChanged;
        _container.Add(_mobileEntry, 0, 0);
        Grid.SetColumnSpan(_mobileEntry, 2);

        Content = _container;
    }



    /// <summary>
    /// Applies Resource Value Method implementation
    /// </summary>
    protected override void ApplyResourceValue()
    {
        _countryCodeDropDown.PageResources = _pageResources;
        _countryCodeDropDown.Options = GetCountryCodeOptions();
        if (_mobileEntry != null)
        {
            _mobileEntry.Icon = Icon;
            _mobileEntry.ResourceKey = _resourceKey;
            _mobileEntry.PageResources = _pageResources;
        }
        _countryCodeDropDown.Value = _countryCodeDropDown.Options.Select(x => x.OptionID).FirstOrDefault();
    }

    /// <summary>
    /// Validates control
    /// </summary>
    /// <param name="isButtonClick"></param>
    internal override void ValidateControl(bool isButtonClick)
    {
        if (_mobileEntry != null && _countryCodeDropDown != null)
        {
            if (_resource.IsRequired)
            {
                if (_countryCodeDropDown.Value == 0)
                {
                    SetFieldError(_mobileEntry._numericEntry, true, "Country Code must be selected");
                }
                else
                {
                    _mobileEntry.ValidateControl(isButtonClick);
                }
            }
            IsValid = _mobileEntry.IsValid;
        }
        if (!_isButtonClick)
        {
            _isButtonClick = isButtonClick;
        }
    }

    private void OnTextChanged(object sender, EventArgs e)
    {
        //_mobileEntry.Value = Convert.ToDecimal(new string(_mobileEntry.Value.ToString().Where(char.IsDigit).ToArray()), CultureInfo.InvariantCulture);
        OnValueChangedAction(sender, e);
    }

    private void OnSelectionChanged(object sender, EventArgs e)
    {
        (_resource.MaxLength, _resource.MinLength) = GetMobileNumberLengths();
        _pageResources.Resources.ForEach(x =>
        {
            if (x.ResourceKey == ResourceKey)
            {
                x.MaxLength = _resource.MaxLength;
                x.MinLength = _resource.MinLength;
            }
        });
        PageResources = _pageResources;
    }

    private List<OptionModel> GetCountryCodeOptions()
    {
        var options = new List<OptionModel>();
        if (_pageResources.CountryCodes?.Count > 0)
        {
            for (int i = 0; i < _pageResources.CountryCodes.Count; i++)
            {
                options.Add(new OptionModel
                {
                    OptionID = i + 1,
                    OptionText = _pageResources.CountryCodes[i].CountryCode,
                    SequenceNo = _pageResources.CountryCodes[i].MobileNumberLength,
                    ParentOptionID = _pageResources.CountryCodes[i].MobileNumberLengthMax,
                });
            }
        }

        return options;
    }

    private (long maxLength, long minLength) GetMobileNumberLengths()
    {
        long maxLength = default;
        long minLength = default;

        if (_pageResources?.CountryCodes?.Count > 0)
        {
            var selectedOption = _countryCodeDropDown.Options?.FirstOrDefault(item => item.OptionID == _countryCodeDropDown.Value);

            if (selectedOption != null)
            {
                string maxLengthStr = "";
                for (var i = 0; i < selectedOption.ParentOptionID; i++)
                {
                    maxLengthStr += "9";
                }
                selectedOption.ParentOptionID = maxLengthStr != null && !string.IsNullOrWhiteSpace(maxLengthStr) ? long.Parse(maxLengthStr) : default;
                maxLength = selectedOption.ParentOptionID;

                // string minLengthStr = "";
                //for (var i = 0; i < selectedOption.SequenceNo; i++)
                //{
                //    minLengthStr += "9";
                //}
                //  selectedOption.SequenceNo = minLengthStr != null && !string.IsNullOrWhiteSpace(minLengthStr) ? long.Parse(minLengthStr) : default;
                //   minLength = selectedOption.SequenceNo;
            }
        }

        return (maxLength, 0);
    }
}
