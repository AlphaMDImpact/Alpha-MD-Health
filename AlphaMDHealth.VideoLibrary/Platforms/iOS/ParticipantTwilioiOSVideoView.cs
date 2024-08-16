using CoreGraphics;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;
using System.ComponentModel;
using UIKit;

namespace AlphaMDHealth.VideoLibrary.Platforms.iOS
{
    /// <summary>
    /// Participant Twilio view
    /// </summary>
    public class ParticipantTwilioiOSVideoView : ViewRenderer<ParticipantTwilioVideoView, UIView>
    {
        private UIView _uiView;

        /// <summary>
        /// Native Twilio control
        /// </summary>
        public Twilio.TVIVideoView ControlVideoView { get; private set; }

        /// <summary>
        /// Participant Twilio view
        /// </summary>
        public ParticipantTwilioiOSVideoView()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<ParticipantTwilioVideoView> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                _uiView = new UIView { ContentMode = UIViewContentMode.ScaleToFill };
            }
            if (e.NewElement != null)
            {
                ControlVideoView = new Twilio.TVIVideoView();
                ControlVideoView.ContentMode = UIViewContentMode.ScaleAspectFill;
                _uiView.AddSubview(ControlVideoView);
            }
            SetNativeControl(_uiView);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            switch (e.PropertyName)
            {
                case nameof(Element.Width):
                case nameof(Element.Height):
                    _uiView.Frame = new CGRect(0f, 0f, Element.Width, Element.Height);
                    break;
                case nameof(Element.IsVisible):
                    Hidden = Element.IsVisible;
                    ControlVideoView.Hidden = Element.IsVisible;
                    break;
                default:
                    // Update property change handling
                    break;
            }
        }
    }
}