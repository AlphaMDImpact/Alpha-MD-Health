using AlphaMDHealth.Utility;
using CommunityToolkit.Maui.Views;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateWebViewStyle()
    {
        Style videoGridStyle = new Style(typeof(Grid))
        {
            Setters = 
            {
                new Setter { Property = VisualElement.IsVisibleProperty, Value = false },
                new Setter { Property = View.MarginProperty, Value = new Thickness(0, 10, 0, 0) },
                new Setter { Property = View.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                new Setter { Property = View.VerticalOptionsProperty, Value = LayoutOptions.StartAndExpand }
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_VIDEO_GRID, videoGridStyle);


        Style mediaElementStyle = new Style(typeof(MediaElement))
        {
            Setters = 
            {
                new Setter { Property = MediaElement.AspectProperty, Value = Aspect.AspectFit },
                new Setter { Property = VisualElement.BackgroundColorProperty, Value = Color.FromHex(StyleConstants.DEFAULT_COLOR) },
                new Setter { Property = VisualElement.HeightRequestProperty, Value = 300 },
                new Setter { Property = View.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                //new Setter { Property = MediaElement.ShouldAutoPlayProperty, Value = true },
                new Setter { Property = View.VerticalOptionsProperty, Value = LayoutOptions.Fill }
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_MEDIA_STYLE, mediaElementStyle);

        Style buttonStyle = new Style(typeof(ImageButton))
        {
            Setters = 
            {
                new Setter { Property = ImageButton.MarginProperty, Value = new Thickness(10) },
                new Setter { Property = VisualElement.HeightRequestProperty, Value = 40 },
                new Setter { Property = View.HorizontalOptionsProperty, Value = LayoutOptions.End },
                new Setter { Property = ImageButton.SourceProperty, Value = ImageConstants.I_FullScreen_PNG },
                new Setter { Property = View.VerticalOptionsProperty, Value = LayoutOptions.Start },
                new Setter { Property = VisualElement.WidthRequestProperty, Value = 40 }
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_IMAGE_BUTTON, buttonStyle);

        Style backButtonStyle = new Style(typeof(ImageButton))
        {
            Setters = 
            {
                new Setter { Property = ImageButton.MarginProperty, Value = new Thickness(0, 0, 20, 0) },
                new Setter { Property = ImageButton.HeightRequestProperty, Value = 30 },
                new Setter { Property = ImageButton.HorizontalOptionsProperty, Value = LayoutOptions.StartAndExpand },
                new Setter { Property = ImageButton.SourceProperty, Value = "iconback.png" },
                new Setter { Property = ImageButton.WidthRequestProperty, Value = 30 }
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_BACK_IMAGE_BUTTON, backButtonStyle);

        Style aspectButtonStyle = new Style(typeof(ImageButton))
        {
            Setters = 
            {
                new Setter { Property = ImageButton.HeightRequestProperty, Value = 30 },
                new Setter { Property = ImageButton.HorizontalOptionsProperty, Value = LayoutOptions.CenterAndExpand },
                new Setter { Property = ImageButton.SourceProperty, Value = "iconaspect.png" },
                new Setter { Property = ImageButton.WidthRequestProperty, Value = 30 }
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_ASPECT_IMAGE_BUTTON, aspectButtonStyle);

        Style orientationButtonStyle = new Style(typeof(ImageButton))
        {
            Setters = 
            {
                new Setter { Property = ImageButton.HeightRequestProperty, Value = 30 },
                new Setter { Property = ImageButton.HorizontalOptionsProperty, Value = LayoutOptions.EndAndExpand },
                new Setter { Property = ImageButton.SourceProperty, Value = "iconorientation.png" },
                new Setter { Property = ImageButton.WidthRequestProperty, Value = 30 }
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_ORIENTATION_IMAGE_BUTTON, orientationButtonStyle);
    }
}