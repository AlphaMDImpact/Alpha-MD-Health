using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Custom checkbox list Control
    /// </summary>
    public class CustomCheckBoxListControl : BaseContentView
    {
        private readonly CustomCheckBoxList _multiSelect;
        private string _htmlPrefixText;
        private Grid _mainLayout;
        private CustomLabelControl _errorLabel;
        /// <summary>
        /// Gets or Sets the Selected Index list
        /// </summary>  
        public ListStyleType CheckBoxType { get; set; }

        /// <summary>
        /// Viewes to display with checkbox list
        /// </summary>
        public Dictionary<long, View> RightViewes { get; private set; }

        /// <summary>
        /// Item source property with property changed event handler
        /// </summary>
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(CustomCheckBoxList), propertyChanged: OnItemsSourceChanged);

        /// <summary>
        /// ItemSource property
        /// </summary>
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private bool _isApplyHeightToError = true;
        /// <summary>
        /// IsApplyHeightToError
        /// </summary>
        public bool IsApplyHeightToError
        {
            get => _isApplyHeightToError;
            set
            {
                _isApplyHeightToError = value;
                _errorLabel.HeightRequest = value ? _errorLabelHeight : -1;
                _errorLabel.WidthRequest = -1;
            }
        }

        /// <summary>
        /// Gets or Sets the Selected Index list
        /// </summary>       
        [DataMember]
        public List<string> SelectedIndexValues
        {
            get
            {
                return _multiSelect?.SelectedIndexValues;
            }
            set
            {
                if (_multiSelect != null)
                {
                    _multiSelect.SelectedIndexValues = value;
                }
            }
        }
        /// <summary>
        /// aplly margin
        /// </summary>
        public bool ApplyMargin
        {
            get { return false; }
            set
            {
                if (_multiSelect != null)
                {
                    _multiSelect.AppliyMarginToCheckBox = value;
                }
            }
        }

        private static void OnItemsSourceChanged(BindableObject bindable, object oldvalue, object newValue)
        {
            CustomCheckBoxListControl control = (CustomCheckBoxListControl)bindable;
            if (newValue != null)
            {
                control.SetOptions((List<OptionSelectModel>)newValue);
                // control.ItemsSource = (IEnumerable)newValue;
            }
        }

        /// <summary>
        /// Custom checkbox list Control
        /// </summary>
        public CustomCheckBoxListControl()
        {
            _multiSelect = new CustomCheckBoxList(App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, 0.0))
            {
                SelectedIndexValues = new List<string>(),
            };
            ApplyMarginToConntrol(0);
            RightViewes = new Dictionary<long, View>();
            CreateView(false);
            // Content = _mainLayout;
        }

        private int ApplyMarginToConntrol(int leftmargin)
        {
            if (MobileConstants.IsAndroidPlatform && DeviceInfo.Idiom == DeviceIdiom.Phone)
            {
                leftmargin = -5;
            }
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                if (MobileConstants.IsTablet)
                {
                    leftmargin = 10;
                }
            }
            if (MobileConstants.IsAndroidPlatform)
            {
                if (MobileConstants.IsTablet)
                {
                    leftmargin = 15;
                }
                if (DeviceInfo.Idiom == DeviceIdiom.Phone)
                {
                    leftmargin = 5;
                }
            }
            _multiSelect.Margin = new Thickness(leftmargin, 10, 0, 0);
            return leftmargin;
        }

        /// <summary>
        /// Custom checkbox list Control
        /// </summary>
        /// <param name="showHeader">for ShowHeader Property to set earlier</param>
        public CustomCheckBoxListControl(bool showHeader)
        {
            _multiSelect = new CustomCheckBoxList(App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, 0.0))
            {
                SelectedIndexValues = new List<string>(),
            };
            ApplyMarginToConntrol(0);
            RightViewes = new Dictionary<long, View>();
            CreateView(showHeader);
        }

        /// <summary>
        /// Custom checkbox list Control
        /// </summary>
        /// <param name="options">options</param>
        public CustomCheckBoxListControl(List<OptionSelectModel> options)
        {
            _multiSelect = new CustomCheckBoxList(App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, 0.0))
            {
                SelectedIndexValues = new List<string>(),
            };
            CreateView(false);
            SetOptions(options);
            //  Content = _mainLayout;
        }

        private void CreateView(bool showHeader)
        {
            if (showHeader)
            {
                _mainLayout = new Grid
                {
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                    ColumnSpacing = 10,
                    Margin = new Thickness(0, 0, 0, Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.CurrentCulture)),
                    RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                },
                    ColumnDefinitions =
                {
                    new ColumnDefinition { Width =  GridLength.Auto},
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                },
                };
                _errorLabel = new CustomLabelControl(LabelType.ClientErrorLabel)
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    WidthRequest = -1,
                    IsHtmlLabelLineCount = true,
                    IsVisible = false
                };
                _mainLayout.Add(_multiSelect, 0, 1);
                Grid.SetColumnSpan(_multiSelect, 2);
                _mainLayout.Add(_errorLabel, 0, 2);
                Grid.SetColumnSpan(_errorLabel, 2);
                Content = _mainLayout;
            }
            else
            {
                Content = _multiSelect;

            }
        }
        /// <summary>
        /// Sets options in check box list 
        /// </summary>
        /// <param name="options"></param>
        public void SetOptions(List<OptionSelectModel> options)
        {
            List<CustomCheckBox> checkBoxes = new List<CustomCheckBox>();
            for (int i = 0; i < options.Count; i++)
            {
                CustomCheckBox checkBox = new CustomCheckBox
                {
                    Style = options[i].IsEnabled
                        ? (options[i].IsSelected
                            ? (Style)Application.Current.Resources[StyleConstants.ST_SELECTED_CHECKBOX_KEY]
                            : (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_CHECKBOX_KEY])
                        : (Style)Application.Current.Resources[StyleConstants.ST_DISABLED_CHECKBOX_KEY],
                    Text = options[i].DisplayText,
                    CheckBoxId = options[i].Value,
                    IsChecked = options[i].IsSelected,
                    IsEnabled = options[i].IsEnabled,
                    AutomationId = $"{ShellMasterPage.CurrentShell.CurrentPage.GetType().Name}{Constants.AUTOMATION_ID_SEPRATOR}{options[i].Value}"
                };
                AutomationProperties.SetIsInAccessibleTree(checkBox, true);
                AutomationProperties.SetName(checkBox, checkBox.AutomationId);
                checkBoxes.Add(checkBox);
                if (checkBox.IsChecked)
                {
                    _multiSelect.SelectedIndexValues.Add(checkBox.CheckBoxId);
                }
            }
            _multiSelect.CheckBoxType = CheckBoxType;
            _multiSelect.RightViewes = RightViewes;
            _multiSelect.ItemsSource = checkBoxes;
        }

        public override void ValidateControl(bool isButtonClick)
        {
            if (_resourceData != null && _resourceData.IsRequired)
            {
                if (_multiSelect.SelectedIndexValues == null || _multiSelect.SelectedIndexValues?.Count < 1)
                {
                    string errorlabel = string.Format(CultureInfo.InvariantCulture, _htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_DROPDOWN_SELECTION_VALIDATION_KEY));
                    _errorLabel.Text = errorlabel;
                    _errorLabel.IsVisible = true;
                    _errorLabel.HeightRequest = IsApplyHeightToError ? _errorLabelHeight : _errorLabel.GetLineCount * _errorLabelHeight;
                    IsValid = false;
                }
                else
                {
                    _errorLabel.Text = string.Empty;
                    _errorLabel.IsVisible = false;
                    IsValid = true;
                }
            }
            else
            {
                IsValid = true;
            }

        }

        protected async override void ApplyResourceValue()
        {
            if (_resourceData != null)
            {
                _htmlPrefixText = await GetSettingsValueAsync().ConfigureAwait(true);
                if (_headerLabel != null)
                {
                    FormattedString fs = new FormattedString();
                    fs.Spans.Add(new Span { Text = _resourceData?.ResourceValue });
                    if (_resourceData.IsRequired && this.IsEnabled)
                    {
                        fs.Spans.Add(new Span { TextColor = (Color)Application.Current.Resources[StyleConstants.ST_ACCENT_COLOR], Text = Constants.IS_REQUIRED_FEILD_INDICATOR });
                    }
                    _headerLabel.FormattedText = fs;
                }
                if (ShowHeader && !string.IsNullOrWhiteSpace(_resourceData.InfoValue))
                {
                    _infoIcon.IsVisible = true;
                    _infoIcon.Clicked += InfoIcon_Clicked;
                }
            }
        }

        private void InfoIcon_Clicked(object sender, EventArgs e)
        {
            DisplayInfoPopup(_resourceData.InfoValue);
        }

        protected override void RenderHeader()
        {
            AddHeaderInView(_mainLayout);
        }

        protected override void RenderBorder(bool value)
        {
            throw new NotImplementedException();
        }

        protected override void EnabledValue(bool value)
        {
            _multiSelect.IsEnabled = value;
        }
    }
}
