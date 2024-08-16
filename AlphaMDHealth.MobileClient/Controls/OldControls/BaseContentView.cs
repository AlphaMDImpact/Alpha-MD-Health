using System.ComponentModel;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Device = Microsoft.Maui.Controls.Device;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Base Content View
    /// </summary>
    public abstract class BaseContentView : ContentView
    {
        private const string _HEADER_LABEL = "HeaderLabel";
        private const string _INFO_ICON = "InfoIcon";

        /// <summary>
        /// project namespace
        /// </summary>
        protected readonly string _prefix = AppStyles.NameSpaceImage;

        /// <summary>
        /// Validation Error Constant String
        /// </summary>
        protected const string _VALIDATION_ERROR = "ValidationError";

        /// <summary>
        /// Text Constant String
        /// </summary>
        protected const string _TEXT = "Text";

        /// <summary>
        /// Value Constant String
        /// </summary>
        protected const string _VALUE = "Value";

        /// <summary>
        /// Renderer Constant String
        /// </summary>
        protected const string _RENDERER = "Renderer";

        /// <summary>
        /// Line Constant String
        /// </summary>
        protected const string _LINE = "Line";

        /// <summary>
        /// Resource Model Reference property
        /// </summary>
        protected ResourceModel _resourceData;

        /// <summary>
        /// Resource DTO
        /// </summary>
        protected BaseDTO _pageData;

        /// <summary>
        /// resource Key
        /// </summary>
        protected string _resourceKey;

        /// <summary>
        /// error label height
        /// </summary>
        protected double _errorLabelHeight = Device.GetNamedSize(NamedSize.Small, typeof(CustomLabel)) + 10;

        /// <summary>
        /// IsValid Property
        /// </summary>
        protected readonly BindableProperty IsValidProperty = BindableProperty.Create(nameof(IsValid), typeof(bool), typeof(BaseContentView), false);

        /// <summary>
        /// Bindable ResourceKey
        /// </summary>
        public static readonly BindableProperty BindableControlResourceKeyProperty =
          BindableProperty.Create(nameof(BindableControlResourceKey), typeof(string), typeof(BaseContentView), propertyChanged: BaseContentView.BindableResourceKeyChangeProperty);

        private static void BindableResourceKeyChangeProperty(BindableObject bindable, object oldValue, object newValue)
        {
            BaseContentView control = (BaseContentView)bindable;
            if (newValue != null && (string)newValue != string.Empty)
            {
                control._resourceKey = (string)newValue;
            }
        }


        private bool _hasHeader;
        private bool _isEnabled;
        private bool _isUnderLine = true;

        /// <summary>
        /// Custom Header Label
        /// </summary>
        protected Label _headerLabel;

        /// <summary>
        /// Info Icon
        /// </summary>
        protected SvgImageButtonView _infoIcon;
        private CustomMessageControl _messageView;

        /// <summary>
        /// resource data for control
        /// </summary>
        public BaseDTO PageResources
        {
            get => _pageData;
            set
            {
                _pageData = value;
                InitialiseResoures();
            }
        }

        /// <summary>
        /// resource key of Control
        /// </summary>
        public string ControlResourceKey
        {
            get => _resourceKey;
            set
            {
                _resourceKey = value;
                InitialiseResoures();
            }
        }

        /// <summary>
        /// enable disable control
        /// </summary>
        public bool IsControlEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                EnabledValue(_isEnabled);
            }
        }

        /// <summary>
        ///display underline in control
        /// </summary>
        [DefaultValue(true)]
        public bool IsUnderLine
        {
            get => _isUnderLine;
            set
            {
                _isUnderLine = value;
                RenderBorder(_isUnderLine);
            }
        }

        /// <summary>
        /// display header or not
        /// </summary>
        public bool ShowHeader
        {
            get => _hasHeader;
            set
            {
                _hasHeader = value;
                RenderHeader();
            }
        }

        /// <summary>
        /// is valid to return whether control is validated
        /// </summary>
        public bool IsValid
        {
            get => (bool)GetValue(IsValidProperty);
            set => SetValue(IsValidProperty, value);
        }

        /// <summary>
        /// BindableControlResourceKeyProperty 
        /// </summary>
        public string BindableControlResourceKey
        {
            get => (string)GetValue(BindableControlResourceKeyProperty);
            set => SetValue(BindableControlResourceKeyProperty, value);
        }

        /// <summary>
        /// To get settings value from database based on settingKey
        /// </summary>
        /// <returns>Setting value of specified Key</returns>
        public async Task<string> GetSettingsValueAsync()
        {
            try
            {
                //to do
                 return (await new SettingLibDatabase().GetSettingAsync(SettingsConstants.S_HTML_PREFIX_FOR_VALIDATION_MESSAGE_TEXT_KEY).ConfigureAwait(false))?.SettingValue ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets a Resource Value By Key
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        protected string GetResourceValueByKey(string resourceKey)
        {
            if (GenericMethods.IsListNotEmpty(_pageData?.Resources))
            {
                return _pageData?.Resources.FirstOrDefault(x => x.ResourceKey == resourceKey)?.ResourceValue?.Trim();
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets a Resource By Key
        /// </summary>
        /// <param name="resourceKey">resource key</param>
        /// <returns>resource model for given key</returns>
        protected ResourceModel GetResourceByKey(string resourceKey)
        {
            if (GenericMethods.IsListNotEmpty(_pageData?.Resources))
            {
                return _pageData?.Resources.FirstOrDefault(x => x.ResourceKey == resourceKey);
            }
            return null;
        }

        /// <summary>
        /// Finds a particular setting based on key from settingData.
        /// </summary>
        /// <param name="settingKey">key for which setting to be fetched.</param>
        /// <returns>setting value for specified setting key.</returns>
        public string GetSettingsValueByKey(string settingKey)
        {
            if (GenericMethods.IsListNotEmpty(_pageData?.Settings))
            {
                return _pageData.Settings.FirstOrDefault(x => x.SettingKey == settingKey)?.SettingValue?.Trim() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// initialise resources after assigning control resources
        /// </summary>
        protected void InitialiseResoures()
        {
            if (!string.IsNullOrWhiteSpace(_resourceKey) && GenericMethods.IsListNotEmpty(_pageData?.Resources))
            {
                _resourceData = _pageData?.Resources.FirstOrDefault(x => x.ResourceKey == _resourceKey);
                ApplyResourceValue();
            }
        }

        /// <summary>
        /// initialise resource after assigning control resource
        /// </summary>
        protected void InitialiseResours()
        {
            if (!string.IsNullOrWhiteSpace(_resourceKey) && _resourceData != null)
            {
                _resourceData = _pageData?.Resources.FirstOrDefault(x => x.ResourceKey == _resourceKey);
                ApplyResourceValue();
            }
        }

        /// <summary>
        /// add header Label to every control
        /// </summary>
        /// <param name="mainLayout">the layout in which header item is added</param>
        protected void AddHeaderInView(Grid mainLayout)
        {
            AddHeaderInView(mainLayout, false);
        }

        /// <summary>
        /// add header Label to every control
        /// </summary>
        /// <param name="mainLayout">the layout in which header item is added</param>
        /// <param name="isApplyStyle">apply header style for different label style added</param>
        protected void AddHeaderInView(Grid mainLayout, bool isApplyStyle)
        {
            if (_hasHeader)
            {
                if (isApplyStyle)
                {
                    _headerLabel = new CustomLabelControl(LabelType.HeaderPrimaryMediumBoldLeftWithoutPadding)
                    {
                        StyleId = _HEADER_LABEL
                    };
                }
                else
                {
                    _headerLabel = new Label
                    {
                        Style = (Style)Application.Current.Resources[StyleConstants.ST_FORMATTED_LABEL_SECONDARY_TEXT_COLOR_SMALL_STYLE],
                        StyleId = _HEADER_LABEL
                    };
                }
                AutomationProperties.SetIsInAccessibleTree(_headerLabel, true);
                _infoIcon = new SvgImageButtonView(ImageConstants.I_INFO_ICON_PNG, AppImageSize.ImageSizeS, AppImageSize.ImageSizeS) { IsVisible = false, StyleId = _INFO_ICON };
                AutomationProperties.SetIsInAccessibleTree(_infoIcon, true);
                mainLayout?.Add(_headerLabel, 0, 0);
                if (isApplyStyle)
                {
                    Grid.SetColumnSpan(_headerLabel, 2);
                }
                else
                {
                    mainLayout?.Add(_infoIcon, 1, 0);
                }
            }
            else
            {
                RemoveExistingHeader(mainLayout);
            }
        }

        /// <summary>
        /// add header Label to every control
        /// </summary>
        /// <param name="infoValue">the layout in which header item is added</param>
        protected void DisplayInfoPopup(string infoValue)
        {
            _messageView = new CustomMessageControl(false) { ShowInPopup = true, ShowCloseButton = true, ShowIcon = false, ShowDiscription = false, ShowInfoValueInHtmlLabel = true, ApplyButtomMarginToHtmlLabel = true };
            AutomationProperties.SetIsInAccessibleTree(_messageView, true);
            _messageView.ControlResourceKey = ControlResourceKey;
            _messageView.PageResources = PageResources;
            _messageView.PageResources.Resources[0].PlaceHolderValue = infoValue;
            _messageView.OnActionClicked += OnMessgeViewActionClicked;
        }

        private void OnMessgeViewActionClicked(object sender, int e)
        {
            _messageView.ShowInPopup = false;
        }

        private static void RemoveExistingHeader(Grid mainLayout)
        {
            if (mainLayout != null && mainLayout.Count > 0)
            {
                foreach (View child in mainLayout)
                {
                    RemoveHeaderFromMainlayout(mainLayout, child);
                }
            }
        }

        private static void RemoveHeaderFromMainlayout(Grid mainLayout, View child)
        {
            if (child is Label && child.StyleId == _HEADER_LABEL)
            {
                mainLayout.Children.Remove(child);
            }
            if (child is CustomLabel && child.StyleId == _HEADER_LABEL)
            {
                mainLayout.Children.Remove(child);
            }
            if (child is SvgImageButtonView && child.StyleId == _INFO_ICON)
            {
                mainLayout.Children.Remove(child);
            }
        }

        /// <summary>
        /// Validates a Control Abstract Method
        /// </summary>
        /// <param name="isButtonClick"></param>
        public abstract void ValidateControl(bool isButtonClick);

        /// <summary>
        /// Apply's Resource Value Abstract Method
        /// </summary>
        protected abstract void ApplyResourceValue();

        /// <summary>
        /// Render Header Abstract Method
        /// </summary>
        protected abstract void RenderHeader();

        /// <summary>
        /// Render Border Abstract Method
        /// </summary>
        /// <param name="value"></param>
        protected abstract void RenderBorder(bool value);

        /// <summary>
        /// Enabled Value Abstratc Method
        /// </summary>
        /// <param name="value"></param>
        protected abstract void EnabledValue(bool value);
    }
}