using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Link View
/// </summary>
public class LinkView : Grid
{
    /// <summary>
    /// Event register with grid to navigate
    /// </summary>
    readonly TapGestureRecognizer _tapGestureRecognizer;

    /// <summary>
    /// Node id on which navigate on click of it
    /// </summary>
    private readonly long _nodeID;

    /// <summary>
    /// Header view constructor
    /// </summary>
    /// <param name="headerLabelText">Heading to display in header</param>
    /// <param name="moreOptionText">Navigation text message</param>
    /// <param name="nodeID">Node id to navigate on click of it</param>
    public LinkView(string headerLabelText, string moreOptionText, long nodeID)
    {
        _nodeID = nodeID;
        Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE];
        BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
        RowDefinitions = new RowDefinitionCollection();
        ColumnDefinitions = new ColumnDefinitionCollection();
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        var groupTitleLabel = new CustomLabelControl(LabelType.HeaderPrimarySmallLeftWithoutPadding)
        {
            Text = headerLabelText,
            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR]
        };
        this.Add(groupTitleLabel, 0, 0);
        if (_nodeID > 0)
        {
            ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            if (string.IsNullOrWhiteSpace(moreOptionText))
            {
                var _navIconView = new SvgImageView(ImageConstants.I_MORE_NAV_PNG, AppImageSize.ImageSizeS, AppImageSize.ImageSizeS, default)
                {
                    HorizontalOptions = LayoutOptions.EndAndExpand,
                    // If language is RTL and is back menu then rotate
                    RotationY = FlowDirection == Microsoft.Maui.FlowDirection.RightToLeft ? 180 : 0
                };
                this.Add(_navIconView, 1, 0);
            }
            else
            {
                this.Add(new CustomLabelControl(LabelType.PrimaryAppExtraSmallRight)
                {
                    TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR],
                    Text = moreOptionText,
                }, 1, 0);
            }
            _tapGestureRecognizer = new TapGestureRecognizer();
            _tapGestureRecognizer.Tapped += OnHeaderClicked;
            GestureRecognizers.Add(_tapGestureRecognizer);
        }
    }

    /// <summary>
    /// Unregister click event of header
    /// </summary>
    public void UnloadUI()
    {
        if (_tapGestureRecognizer != null)
        {
            _tapGestureRecognizer.Tapped -= OnHeaderClicked;
        }
    }

    private async void OnHeaderClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.CurrentPage.PushPageByNodeIDAsync(_nodeID, false, "").ConfigureAwait(true);
    }
}