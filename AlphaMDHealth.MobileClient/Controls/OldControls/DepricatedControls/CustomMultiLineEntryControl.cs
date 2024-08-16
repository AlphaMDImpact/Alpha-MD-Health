using System.Globalization;
using System.Runtime.CompilerServices;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represemnts Custom MultiLine Entry Control
    /// </summary>
    public class CustomMultiLineEntryControl : BaseContentView
    {
        private readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(CustomMultiLineEntryControl), defaultValue: string.Empty, BindingMode.TwoWay);
        private CustomMultiLineEntry _editor;
        private readonly CustomLabelControl _errorLabel;
        private readonly Grid _mainLayout;
        private readonly Border _borderFrame;
        private readonly BoxView _seprator;
        private bool _isApplyHeightToError = true;

        /// <summary>
        /// OnEditorSizeChanged method
        /// </summary>
        public event EventHandler<EventArgs> OnEditorSizeChanged;
        private readonly EditorValidationBehavior _validationError = new EditorValidationBehavior();
        private bool _isButtonClick;

        /// <summary>
        /// is Readonly only editor
        /// </summary>
        public bool IsEditable
        {
            get => _editor.IsReadOnly;
            set
            {
                if (!value)
                {
                    if (MobileConstants.IsIosPlatform)
                    {
                        _editor.ReadOnly = true;
                    }
                    else
                    {
                        _editor.IsReadOnly = true;
                    }

                }
            }
        }

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
            }
        }

        /// <summary>
        /// transparent border set it befor control type
        /// </summary>
        public bool ControlWithoutBorder { get; set; }

        /// <summary>
        /// get and set Editor Text value
        /// </summary>
        public string Value
        {
            get => _editor?.Text;
            set => _editor.Text = value;
        }

        /// <summary>
        /// editor height request
        /// </summary>
        public EditorHeight EditorHeightRequest { get; set; } = EditorHeight.Default;


        /// <summary>
        /// regex parttern
        /// </summary>
        public string RegexPattern
        {
            get;
            set;
        }
        /// <summary>
        /// IsBackGroundTransparent
        /// </summary>
        public bool IsBackGroundTransparent
        {
            get; set;
        }

        /// <summary>
        /// validate Entry Control 
        /// </summary>
        /// <param name="isButtonClick">validation fired after button is clicked</param>
        public override void ValidateControl(bool isButtonClick)
        {
            if (!_isButtonClick)
            {
                _isButtonClick = isButtonClick;
            }
            _validationError.ValidateControl(_editor);
            if (IsValid)
            {
                if (_errorLabel != null)
                {
                    _errorLabel.IsVisible = false;
                    _errorLabel.HeightRequest = 0;
                }
            }
            else
            {
                if (!IsValid && _isButtonClick && _errorLabel != null)
                {
                    _errorLabel.IsVisible = !_editor.IsReadOnly;
                    _errorLabel.HeightRequest = _editor.IsReadOnly ? 0 : (IsApplyHeightToError ? _errorLabelHeight : _errorLabel.GetLineCount * _errorLabelHeight);
                }
            }
            if (_errorLabel?.IsVisible == true)
            {
                _seprator.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_ERROR_COLOR];
                _borderFrame.Stroke = (Color)Application.Current.Resources[StyleConstants.ST_ERROR_COLOR];
            }
            else
            {
                _seprator.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE];
                _borderFrame.Stroke = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE];
            }
        }

        /// <summary>
        /// This method Invoked on property chnaged
        /// </summary>
        /// <param name="propertyName">Name Of the property changed</param>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case _VALUE:
                    _editor.Text = Value;
                    ValidateControl(_isButtonClick);
                    break;
                case _RENDERER:
                    _editor.HeightRequest = Convert.ToDouble(EditorHeightRequest, CultureInfo.InvariantCulture);
                    if (EditorHeightRequest == EditorHeight.Chat)
                    {
                        _editor.AddConstraint = true;
                    }
                    else
                    {
                        _editor.AddConstraint = false;
                    }
                    RenderBorder(IsUnderLine);
                    break;
                default:
                    //to be implemented
                    break;
            }
        }

        private void CreateEditor()
        {
            _editor = new CustomMultiLineEntry
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_SMALL_EDITOR_STYLE],
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };

            _editor.SizeChanged += Editor_SizeChanged;
        }

        private void Editor_SizeChanged(object sender, EventArgs e)
        {
            OnEditorSizeChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// method to Apply Resource Value
        /// </summary>
        protected override async void ApplyResourceValue()
        {
            if (_resourceData != null)
            {
                _editor.ClassId = _resourceData.ResourceValue;
                if (string.IsNullOrWhiteSpace(_editor.AutomationId))
                {
                    _editor.AutomationId = $"{ShellMasterPage.CurrentShell.CurrentPage.GetType().Name}{_resourceData.ResourceValue}";
                }
                if (string.IsNullOrWhiteSpace(_borderFrame.AutomationId))
                {
                    _borderFrame.AutomationId = $"{ShellMasterPage.CurrentShell.CurrentPage.GetType().Name}{Constants.AUTOMATION_ID_SEPRATOR}{_resourceData.ResourceValue}";
                }
                AutomationProperties.SetName(_editor, _editor.AutomationId);
                AutomationProperties.SetName(_borderFrame, _borderFrame.AutomationId);
                AutomationProperties.SetName(_mainLayout, _borderFrame.AutomationId);
                var htmlPrefixText = await GetSettingsValueAsync().ConfigureAwait(true);
                if (_resourceData.IsRequired)
                {
                    _editor.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.IsRequired, string.Format(CultureInfo.InvariantCulture, htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY))));
                }
                if (_resourceData.MinLength > 0)
                {
                    _editor.StyleId = Convert.ToString(_resourceData.MinLength, CultureInfo.CurrentCulture);
                    _editor.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.MinimumLength, string.Format(CultureInfo.InvariantCulture, htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_MINIMUM_LENGTH_VALIDATION_KEY))));
                }
                if (_resourceData.MaxLength > 0)
                {
                    _editor.MaxLength = (int)_resourceData.MaxLength;
                }
                if (!string.IsNullOrWhiteSpace(RegexPattern))
                {
                    _editor.Pattern = RegexPattern;
                    _editor.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.ValidationRegxString, string.Format(CultureInfo.InvariantCulture, htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_INVALID_DATA_KEY))));
                }
                if (_editor.Validations.Count > 0)
                {
                    SetBinding(IsValidProperty, new Binding { Source = _validationError, Path = Constants.IS_VALID });
                    _editor.Behaviors.Add(_validationError);
                }
                else
                {
                    IsValid = true;
                }
                ApplyPlaceHolder();
            }
        }

        private void ApplyPlaceHolder()
        {
            _editor.Placeholder = this.IsEnabled && !string.IsNullOrWhiteSpace(_resourceData.PlaceHolderValue) ? _resourceData.PlaceHolderValue : string.Empty;

            FormattedString fs = new FormattedString();
            fs.Spans.Add(new Span { Text = _resourceData.ResourceValue });
            if (_resourceData.IsRequired && this.IsEnabled)
            {
                fs.Spans.Add(new Span { TextColor = (Color)Application.Current.Resources[StyleConstants.ST_ACCENT_COLOR], Text = Constants.IS_REQUIRED_FEILD_INDICATOR });
            }
            _headerLabel.FormattedText = fs;
            _headerLabel.Margin = new Thickness(0, 0, 0, 10);
            if (ShowHeader && !string.IsNullOrWhiteSpace(_resourceData.InfoValue))
            {
                _infoIcon.IsVisible = true;
                _infoIcon.VerticalOptions = LayoutOptions.Start;
                _infoIcon.Clicked += InfoIcon_Clicked;
            }
        }

        private void InfoIcon_Clicked(object sender, EventArgs e)
        {
            DisplayInfoPopup(_resourceData.InfoValue);
        }

        /// <summary>
        /// Method to render header
        /// </summary>
        protected override void RenderHeader()
        {
            AddHeaderInView(_mainLayout);
        }

        /// <summary>
        /// Method to set rendere value
        /// </summary>
        /// <param name="value">border thikness</param>
        protected override void RenderBorder(bool value)
        {
            if (_editor != null && _borderFrame != null)
            {
                if (value)
                {
                    _borderFrame.StrokeThickness = 0;
                    _borderFrame.Padding = new Thickness(0);
                    _seprator.IsVisible = true;
                    _editor.Margin = new Thickness(-3, -10, 0, 0);
                }
                else
                {
                    _editor.Margin = new Thickness(GenericMethods.GetPlatformSpecificValue(0, 8, 0), 0, 0, 0);
                    _seprator.IsVisible = false;
                    _borderFrame.Padding = new Thickness(5, 0, 5, 0);
                    _borderFrame.StrokeThickness = 1;
                }
                if (IsBackGroundTransparent)
                {
                    _borderFrame.BackgroundColor = Colors.Transparent;
                }
                else
                {
                    _borderFrame.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                }
                if (ControlWithoutBorder)
                {
                    _borderFrame.StrokeThickness = 0;
                }
            }
        }

        /// <summary>
        /// Method to Enable value
        /// </summary>
        /// <param name="value"></param>
        protected override void EnabledValue(bool value)
        {
            _editor.IsEnabled = value;
        }

        /// <summary>
        ///  Custom MultiLine Entry Control class costructor
        /// </summary>
        public CustomMultiLineEntryControl()
        {
            CreateEditor();
            _errorLabel = new CustomLabelControl(LabelType.ClientErrorLabel)
            {
                IsVisible = false
            };
            AutomationProperties.SetIsInAccessibleTree(_editor, true);
            _borderFrame = new Border
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_CONTROL_STYLE],
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Content = _editor
            };
            AutomationProperties.SetIsInAccessibleTree(_borderFrame, true);
            _mainLayout = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                ColumnSpacing = 10,
                Margin = new Thickness(0, 0, 0, Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.CurrentCulture)),
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto)},
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star)},
                },
            };
            AutomationProperties.SetIsInAccessibleTree(_mainLayout, true);
            _mainLayout.Add(_errorLabel, 0, 2);
            Grid.SetColumnSpan(_errorLabel, 2);
            _mainLayout.Add(_borderFrame, 0, 1);
            Grid.SetColumnSpan(_borderFrame, 2);
            _seprator = new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE],// LibGenericMethods.GetPlatformSpecificValue((Color)Application.Current.Resources[LibStyleConstants.ST_SEPERATOR_COLOR_STYLE], (Color)Application.Current.Resources[LibStyleConstants.ST_TERTIARY_TEXT_COLOR], Color.Default),
                VerticalOptions = LayoutOptions.End,
            };

            _mainLayout.Add(_seprator, 0, 1);
            Grid.SetColumnSpan(_seprator, 2);
            SetBinding(ValueProperty, new Binding { Source = _editor, Path = _TEXT, });
            _errorLabel.SetBinding(CustomLabel.TextProperty, new Binding { Source = _validationError, Path = _VALIDATION_ERROR });
            _errorLabel.SetBinding(CustomLabel.GetLineCountProperty, _VALIDATION_ERROR);

            IsUnderLine = true;
            ShowHeader = true;
            Content = _mainLayout;
        }
    }
}


