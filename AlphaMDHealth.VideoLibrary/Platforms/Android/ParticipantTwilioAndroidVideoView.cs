using Android.Content;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using System.ComponentModel;


//[assembly: ExportRenderer(typeof(ParticipantTwilioVideoView), typeof(ParticipantTwilioAndroidVideoView))]
namespace AlphaMDHealth.VideoLibrary.Platforms.Android
{
    /// <summary>
    /// Native participant view for Twilio
    /// </summary>
    public class ParticipantTwilioAndroidVideoView : FrameLayout, IVisualElementRenderer, IViewRenderer
    {
        private readonly Context _context;
        private bool disposed;
        int? defaultLabelFor;
        private VisualElementTracker _visualElementTracker;
        private VisualElementRenderer _visualElementRenderer;
        private ParticipantTwilioVideoView _element;

        /// <summary>
        /// Native Twilio Video control
        /// </summary>
        public Twilio.Video.VideoView ControlVideoView { get; private set; }

        /// <summary>
        /// Xamarin element
        /// </summary>
        public ParticipantTwilioVideoView Element
        {
            get => _element;
            set
            {
                if (_element == value)
                {
                    return;
                }

                var oldElement = _element;
                _element = value;
                OnElementChanged(new ElementChangedEventArgs<ParticipantTwilioVideoView>(oldElement, _element));
            }
        }

        /// <summary>
        /// Event called when element is changed
        /// </summary>
        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

        /// <summary>
        /// Event called when element property is changed
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

        /// <summary>
        /// Native participant view for Twilio
        /// </summary>
        /// <param name="context">Android Context</param>
        public ParticipantTwilioAndroidVideoView(Context context) : base(context)
        {
            _context = context;
            _visualElementRenderer = new VisualElementRenderer(this);
        }

        private void OnElementChanged(ElementChangedEventArgs<ParticipantTwilioVideoView> e)
        {
            if (e.OldElement != null)
            {
                e.OldElement.PropertyChanged -= OnElementPropertyChanged;
            }
            if (e.NewElement != null)
            {
                this.EnsureId();
                e.NewElement.PropertyChanged += OnElementPropertyChanged;
                ElevationHelper.SetElevation(this, e.NewElement);
                ControlVideoView = new Twilio.Video.VideoView(_context);
                ControlVideoView.VideoScaleType = Twilio.Video.VideoScaleType.AspectFit;
                AddView(ControlVideoView);
            }
            ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
        }

        private void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ElementPropertyChanged?.Invoke(this, e);
            switch (e.PropertyName)
            {
                case nameof(Width):
                case nameof(Height):
                    ControlVideoView.LayoutParameters.Width = (int)Context.ToPixels(Element.Width);
                    ControlVideoView.LayoutParameters.Height = (int)Context.ToPixels(Element.Height);
                    break;
                case nameof(Element.IsVisible):
                    Visibility = Element.IsVisible ? ViewStates.Visible : ViewStates.Invisible;
                    ControlVideoView.Visibility = Visibility;
                    break;
                default:
                    //
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            if (disposing)
            {
                SetOnClickListener(null);
                SetOnTouchListener(null);

                if (_visualElementTracker != null)
                {
                    _visualElementTracker.Dispose();
                    _visualElementTracker = null;
                }

                if (_visualElementRenderer != null)
                {
                    _visualElementRenderer.Dispose();
                    _visualElementRenderer = null;
                }

                if (Element != null)
                {
                    Element.PropertyChanged -= OnElementPropertyChanged;

                    if (Microsoft.Maui.Controls.Compatibility.Platform.Android.Platform.GetRenderer(Element) == this)
                    {
                        Microsoft.Maui.Controls.Compatibility.Platform.Android.Platform.SetRenderer(Element, null);
                    }
                }
            }

            base.Dispose(disposing);
        }

        #region IViewRenderer

        /// <summary>
        /// Measures the width and height of the view
        /// </summary>
        //public void MeasureExactly() => MeasureExactly(this, Element, Context);

        //private void MeasureExactly(Microsoft.Maui.Controls.View control, VisualElement element, Context context)
        //{
        //    if (control == null || element == null)
        //    {
        //        return;
        //    }

        //    double width = element.Width;
        //    double height = element.Height;

        //    if (width <= 0 || height <= 0)
        //    {
        //        return;
        //    }

        //    int realWidth = (int)context.ToPixels(width);
        //    int realHeight = (int)context.ToPixels(height);

        //    int widthMeasureSpec = MakeMeasureSpec(realWidth, MeasureSpecMode.Exactly);
        //    int heightMeasureSpec = MakeMeasureSpec(realHeight, MeasureSpecMode.Exactly);

        //    control.Measure(widthMeasureSpec, heightMeasureSpec);
        //}

        public void MeasureExactly()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IVisualElementRenderer

        VisualElement IVisualElementRenderer.Element => Element;

        /// <summary>
        /// Android element tracker
        /// </summary>
        public VisualElementTracker Tracker => _visualElementTracker;

        /// <summary>
        /// Android view group
        /// </summary>
        public ViewGroup ViewGroup => null;

        /// <summary>
        /// Android view
        /// </summary>
        //public Android.Views.View View => this;

        global::Android.Views.View IVisualElementRenderer.View => this;

        /// <summary>
        /// Gets the desired size
        /// </summary>
        /// <param name="widthConstraint">Width constraint for view</param>
        /// <param name="heightConstraint"></param>
        /// <returns>Desired size based on constraints</returns>
        public SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
        {
            Measure(widthConstraint, heightConstraint);
            SizeRequest result = new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), new Size(Context.ToPixels(20), Context.ToPixels(20)));
            return result;
        }

        /// <summary>
        /// Sets the element to the UI
        /// </summary>
        /// <param name="element">Element to be added</param>
        public void SetElement(VisualElement element)
        {
            if (!(element is ParticipantTwilioVideoView camera))
            {
                throw new ArgumentException($"{nameof(element)} must be of type {nameof(ParticipantTwilioVideoView)}");
            }

            if (_visualElementTracker == null)
            {
                _visualElementTracker = new VisualElementTracker(this);
            }
            Element = camera;
        }

        /// <summary>
        /// Sets the label id
        /// </summary>
        /// <param name="id">id of the view</param>
        public void SetLabelFor(int? id)
        {
            if (defaultLabelFor == null)
            {
                defaultLabelFor = LabelFor;
            }
            LabelFor = (int)(id ?? defaultLabelFor);
        }

        /// <summary>
        /// Updates the layout
        /// </summary>
        public void UpdateLayout() => _visualElementTracker?.UpdateLayout();

        #endregion

        private int MakeMeasureSpec(int size, MeasureSpecMode mode) => size + (int)mode;

    }
}