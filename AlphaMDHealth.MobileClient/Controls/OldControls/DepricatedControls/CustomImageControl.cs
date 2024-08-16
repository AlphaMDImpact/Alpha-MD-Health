using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents custom image control
    /// </summary>
    public class CustomImageControl : ContentView
    {
        private readonly AppImageSize _imageWidth;
        private readonly AppImageSize _imageHeight;
        private readonly bool _isCircle = true;

        /// <summary>
        /// Create image Control
        /// </summary>
        /// <param name="imageWidth">Width of image</param>
        /// <param name="imageHeight">height of image</param>
        /// <param name="source">Base64 Image string</param>
        /// <param name="defaultValue">In case the source is not present, default image or characters to be displayed in Image</param>
        /// <param name="isCircle">True, if you need circular Image</param>
        public CustomImageControl(AppImageSize imageWidth, AppImageSize imageHeight, ImageSource source, string defaultValue, bool isCircle)
        {
            _imageWidth = imageWidth;
            _imageHeight = imageHeight;
            Source = source;
            DefaultValue = defaultValue;
            _isCircle = isCircle;
            RenderImage(_imageWidth, _imageHeight, Source, DefaultValue, _isCircle, Colors.Transparent);
        }

        /// <summary>
        /// To draw a Images with size of parent container 
        /// </summary>
        /// <param name="source">Base64 Image string</param>
        /// <param name="defaultValue">In case the source is not present, default image or characters to be displayed in Image</param>
        public CustomImageControl(ImageSource source, string defaultValue)
        {
            Source = source;
            DefaultValue = defaultValue;
            RenderImage(AppImageSize.ImageNone, AppImageSize.ImageNone, Source, DefaultValue, false, Colors.Transparent);
        }

        /// <summary>
        /// image source for image view
        /// </summary>
        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        /// <summary>
        /// image source for image view
        /// </summary>
        public string ImagePathSource
        {
            get => (string)GetValue(ImageUrlSourceProperty);
            set => SetValue(ImageUrlSourceProperty, value);
        }

        /// <summary>
        /// source property
        /// </summary>
        public static readonly BindableProperty ImageUrlSourceProperty = BindableProperty.
            Create(nameof(ImagePathSource), typeof(string), typeof(CustomImageControl),
            propertyChanged: CustomImageControl.ImageUrlSourcePropertyChange, defaultBindingMode: BindingMode.OneWay);

        /// <summary>
        /// is used for svg image and intial view both
        /// </summary>
        public string DefaultValue
        {
            get => (string)GetValue(DefaultValueProperty);
            set => SetValue(DefaultValueProperty, value);
        }

        /// <summary>
        /// source property
        /// </summary>
        public static readonly BindableProperty TintColorProperty = BindableProperty.
            Create(nameof(TintColor), typeof(Color), typeof(CustomImageControl), Colors.Transparent,
            propertyChanged: CustomImageControl.TintColorPropertyChanged);

        /// <summary>
        /// is used for svg image Tint Color 
        /// </summary>
        public Color TintColor
        {
            get => (Color)GetValue(TintColorProperty);
            set => SetValue(TintColorProperty, value);
        }

        /// <summary>
        /// source property
        /// </summary>
        public static readonly BindableProperty SourceProperty = BindableProperty.
            Create(nameof(Source), typeof(ImageSource), typeof(CustomImageControl),
            propertyChanged: CustomImageControl.SourcePropertyChanged, defaultBindingMode: BindingMode.TwoWay);

        /// <summary>
        /// default value property
        /// </summary>
        public static readonly BindableProperty DefaultValueProperty =
            BindableProperty.Create(nameof(DefaultValue), typeof(string),
                typeof(CustomImageControl), string.Empty, propertyChanged: CustomImageControl.DefaultValuePropertyChanged, defaultBindingMode: BindingMode.TwoWay);

        private static void SourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            CustomImageControl control = (CustomImageControl)bindable;

            control.RenderImage(control._imageWidth, control._imageHeight, newValue != null ? (ImageSource)newValue : null, control.DefaultValue, control._isCircle, control.TintColor);
        }

        private static void TintColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            CustomImageControl control = (CustomImageControl)bindable;

            control.RenderImage(control._imageWidth, control._imageHeight, control.Source, control.DefaultValue, control._isCircle, (newValue == null || (Color)newValue == Colors.Transparent) ? Colors.Transparent : (Color)newValue);

        }
        private static void DefaultValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            CustomImageControl control = (CustomImageControl)bindable;
            control.RenderImage(control._imageWidth, control._imageHeight, control.Source, !string.IsNullOrWhiteSpace((string)newValue) ? (string)newValue : null, control._isCircle, control.TintColor);
        }

        private static void ImageUrlSourcePropertyChange(BindableObject bindable, object oldValue, object newValue)
        {
            CustomImageControl control = (CustomImageControl)bindable;
            control.RenderUrlImage(control._imageWidth, control._imageHeight, newValue != null ? (string)newValue : null, control._isCircle, control.TintColor);
        }

        private void RenderImage(AppImageSize imageWidth, AppImageSize imageHeight, ImageSource source, string defaultValue, bool isCircle, Color tintColor)
        {
            var prefix = AppStyles.NameSpaceImage;
            if (!string.IsNullOrWhiteSpace(defaultValue) && defaultValue.Length > 3)
            {
                DefaultValue = prefix + defaultValue.Replace(prefix, string.Empty);
            }

            View mainContent = null;
            if (!(source == null || (source.ToString() == Constants.FILE_PATH_EXTRA || source.ToString() == Constants.FILE_PATH)))
            {
                mainContent = SourceImage(imageWidth, imageHeight, source, isCircle);
            }
            else
            {
                DefaultImage(ref imageWidth, imageHeight, DefaultValue, isCircle, tintColor, ref mainContent);
            }

            Content = mainContent;
           
        }

        private void DefaultImage(ref AppImageSize imageWidth, AppImageSize imageHeight, string defaultValue, bool isCircle, Color tintColor, ref View mainContent)
        {
            if (!string.IsNullOrWhiteSpace(defaultValue) && defaultValue.Length > 0)
            {
                if (defaultValue.Length <= 2)
                {
                    mainContent = imageWidth == AppImageSize.ImageNone ? new InitialImageView(defaultValue) : new InitialImageView(defaultValue, imageWidth, imageHeight, isCircle);
                }
                else
                {
                    mainContent = new SvgImageView(defaultValue, imageWidth, imageHeight, tintColor);
                }
            }
        }

        private void RenderUrlImage(AppImageSize imageWidth, AppImageSize imageHeight, string source, bool isCircle, Color tintColor)
        {
            View mainContent = null;
            if (string.IsNullOrWhiteSpace(source))
            {
                DefaultImage(ref imageWidth, imageHeight, DefaultValue, isCircle, tintColor, ref mainContent);                
            }
            else
            {
                mainContent = UrlSourceImage(imageWidth, imageHeight, source);
            }
            Content = mainContent;
            
        }

        private View SourceImage(AppImageSize imageWidth, AppImageSize imageHeight, ImageSource source, bool isCircle)
        {
            View mainContent;
            ////if (Device.RuntimePlatform == Device.Android && App._essentials.GetPreferenceValue(LibStorageConstants.PR_APPLICATION_SDK_VERSION_KEY, 0) < 23)
            ////{
            ////    mainContent = imageWidth == AppImageSize.ImageNone ? new RoundedImageView(source, Aspect.AspectFill) : new RoundedImageView(source, imageWidth, imageHeight, isCircle, Aspect.AspectFill);
            ////}
            ////else
            ////{
            mainContent = imageWidth == AppImageSize.ImageNone ? new CircleImageView(source, Aspect.AspectFill) : new CircleImageView(source, imageWidth, imageHeight, isCircle, Aspect.AspectFill);
            //// }
            return mainContent;
        }

        private View UrlSourceImage(AppImageSize imageWidth, AppImageSize imageHeight, string imagePathSource)
        {
            View mainContent;
            mainContent = imageWidth == AppImageSize.ImageNone ? new CircleImageView(imagePathSource, Aspect.AspectFill) : new CircleImageView(Aspect.AspectFill, imagePathSource, imageWidth, imageHeight);
            return mainContent;
        }
    }
}