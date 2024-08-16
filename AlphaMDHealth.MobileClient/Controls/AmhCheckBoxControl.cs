using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Core.Internal;
using DevExpress.Maui.Editors;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

internal class AmhCheckBoxControl : AmhBaseControl
{
    private AmhLabelControl _headerLabel;
    private AmhLabelControl _placeHolderLabel;
    private AmhLabelControl _infoLabel;
    private AmhLabelControl _errorLabel;
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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(AmhCheckBoxControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhCheckBoxControl control = (AmhCheckBoxControl)bindable;
        if (newValue != null)
        {
            control.Value = newValue as string;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhCheckBoxControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhCheckBoxControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    private string GetControlValue()
    {
        _value = string.Empty;
        if (_container.Children.Count > 0)
        {
            _value = string.Join(Constants.PIPE_SEPERATOR, GetSelectedIDs());
        }
        return _value;
    }

    private void SetControlValue()
    {
        if (_container.Children.Count > 0)
        {
            var selectedIds = FetchOptionIDsFromValue(_value);
            _container.Children.ForEach(x =>
            {
                if (x is CheckEdit)
                {
                    var id = Convert.ToInt64((x as CheckEdit).StyleId);
                    (x as CheckEdit).IsChecked = selectedIds?.Contains(id);
                }
            });
        }
    }

    private void OnCheckedChanged(object sender, EventArgs e)
    {
        OnValueChangedAction(sender, e);
    }

    protected override void EnabledDisableField(bool value)
    {
        SetFieldIsEnabled(_container, value);
    }

    protected override void RenderControl()
    {
        if (_container == null)
        {
            CreateWrapperControls(out _container, out _headerLabel, out _placeHolderLabel, out _infoLabel, out _errorLabel);
        }
        AddColumnDefinition();
        AddChildsInContainer();
        Content = _container;
    }

    protected override void ApplyResourceValue()
    {
        ApplyResource(_headerLabel, _placeHolderLabel, _infoLabel);
    }

    protected override void ApplyOptions()
    {
        AddChildsInContainer();
    }

    internal override void ValidateControl(bool isButtonClick)
    {
        _errorLabel.Value = string.Empty;
        if (IsControlEnabled && _resource != null)
        {
            double? checkedCount = GetSelectedIDs()?.Count();
            if (Validator.HasRequiredValidationError(_resource, checkedCount.HasValue && checkedCount > 0))
            {
                _errorLabel.Value = GetRequiredResourceValue();
            }
            else if (checkedCount.HasValue && checkedCount > 0)
            {
                if (Validator.HasMinLengthValidationError(_resource, checkedCount.Value))
                {
                    _errorLabel.Value = string.Format(CultureInfo.CurrentCulture
                        , LibResources.GetResourceValueByKey(PageResources?.Resources, ResourceConstants.R_MINIMUM_LENGTH_VALIDATION_KEY)
                        , _resource.MinLength);
                }
                else if (Validator.HasRangeValidationError(_resource, checkedCount.Value))
                {
                    _errorLabel.Value = string.Format(CultureInfo.CurrentCulture
                        , LibResources.GetResourceValueByKey(PageResources?.Resources, ResourceConstants.R_RANGE_LENGTH_VALIDATION_KEY)
                        , _resource.MinLength, _resource.MaxLength);
                }
            }
        }
        SetValidationResult(isButtonClick, _errorLabel.Value);
    }

    private IEnumerable<string> GetSelectedIDs()
    {
        return _container.Children.Where(x =>
            x is CheckEdit &&
            ((x as CheckEdit).IsChecked ?? false)
        ).Select(x => (x as CheckEdit).StyleId);
    }

    private void AddColumnDefinition()
    {
        if (_fieldType == FieldTypes.ColorBoxCheckBoxControl)
        {
            _container.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
            };
            SetColSpan(2);
        }
        else
        {
            _container.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star }
            };
            SetColSpan(1);
        }
    }

    private void AddChildsInContainer()
    {
        _container.Children?.Clear();
        _container.RowDefinitions = new RowDefinitionCollection();

        AddView(_container, _headerLabel, 0, 0);
        AddView(_container, _placeHolderLabel, 0, 1);
        AddView(_container, _infoLabel, 0, 2);
        var row = 3;
        if (GenericMethods.IsListNotEmpty(_options))
        {
            var selectedIds = FetchOptionIDsFromValue(_value);
            _options.ForEach(item =>
            {
                AddCheckBox(item, selectedIds, row);
                AddColorBox(item, row);
                row++;
            });
        }
        AddView(_container, _errorLabel, 0, row);
    }

    private void AddCheckBox(OptionModel item, List<long> selectedIds, int row)
    {
        var checkBox = new CheckEdit
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_CHECK_EDIT_STYLE],
            StyleId = item.OptionID.ToString(),
            Label = item.OptionText,
            IsChecked = item.IsSelected || (selectedIds?.Any(x => x == item.OptionID) ?? false)
        };
        checkBox.CheckedChanged += OnCheckedChanged;
        AddView(_container, checkBox, 0, row);
    }

    private void AddColorBox(OptionModel item, int row)
    {
        if (_fieldType == FieldTypes.ColorBoxCheckBoxControl)
        {
            var colorView = new BoxView
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_COLOR_BOX_STYLE],
                BackgroundColor = string.IsNullOrWhiteSpace(item.ParentOptionText) ? default : Color.FromArgb(item.ParentOptionText),
            };
            AddView(_container, colorView, 1, row);
        }
    }

    private void SetColSpan(int row)
    {
        Grid.SetColumnSpan(_headerLabel, row);
        Grid.SetColumnSpan(_placeHolderLabel, row);
        Grid.SetColumnSpan(_infoLabel, row);
        Grid.SetColumnSpan(_errorLabel, row);
    }
}