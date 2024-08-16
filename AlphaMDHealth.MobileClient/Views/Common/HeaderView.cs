using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Headers view used to render as heading on anywhere
/// </summary>
public class HeaderView : Grid
{
    /// <summary>
    /// Node id on which navigate on click of it
    /// </summary>
    private readonly long _nodeID;

    /// <summary>
    /// featureCode to navigate on click of Header click of it
    /// </summary>
    private readonly string _featureCode;

    /// <summary>
    /// Event register with grid to navigate
    /// </summary>
    private TapGestureRecognizer _tapGestureRecognizer;

    /// <summary>
    /// on click event of OnHederClicked
    /// </summary>
    public event EventHandler<EventArgs> OnMoreOptionClicked;

    /// <summary>
    /// Header view constructor
    /// </summary>
    /// <param name="headerLabelText">Heading to display in header</param>
    /// <param name="moreOptionText">Navigation text message</param>
    /// <param name="nodeID">Node id to navigate on click of it</param>
    public HeaderView(string headerLabelText, string moreOptionText, long nodeID)
    {
        _nodeID = nodeID;
        LoadUI(headerLabelText, moreOptionText);
    }

    /// <summary>
    /// Header view constructor
    /// </summary>
    /// <param name="headerLabelText">Heading to display in header</param>
    /// <param name="moreOptionText">Navigation text message</param>
    /// <param name="featureCode">feature Code to navigate on click of it</param>
    public HeaderView(string headerLabelText, string moreOptionText, string featureCode)
    {
        _featureCode = featureCode;
        LoadUI(headerLabelText, moreOptionText);
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
        if (!string.IsNullOrWhiteSpace(_featureCode) && OnMoreOptionClicked != null)
        {
            OnMoreOptionClicked.Invoke(_featureCode, new EventArgs());
        }
        else
        {
            AppHelper.ShowBusyIndicator = true;
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByNodeIDAsync(_nodeID, false, "").ConfigureAwait(true);
        }
    }

    private void LoadUI(string headerLabelText, string moreOptionText)
    {
        Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE];
        RowDefinitions = new RowDefinitionCollection();
        ColumnDefinitions = new ColumnDefinitionCollection();
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        SetgroupTitleLabel(headerLabelText);
        if (_nodeID > 0 || !string.IsNullOrWhiteSpace(_featureCode))
        {
            ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            if (string.IsNullOrWhiteSpace(moreOptionText))
            {
                this.Add(new AmhImageControl
                {
                    Icon = ImageConstants.I_MORE_NAV_WHITE_BACK_PNG,
                    ImageHeight = AppImageSize.ImageSizeS,
                    ImageWidth = AppImageSize.ImageSizeS,
                    HorizontalOptions = LayoutOptions.EndAndExpand,
                    // If language is RTL and is back menu then rotate
                    RotationY = FlowDirection == FlowDirection.RightToLeft ? 180 : 0
                }, 1, 0);
            }
            else
            {
                this.Add(new AmhLabelControl(FieldTypes.PrimaryAppMediumHEndVCenterBoldLabelControl)
                {
                    Value = moreOptionText,
                }, 1, 0);
            }
            _tapGestureRecognizer = new TapGestureRecognizer();
            _tapGestureRecognizer.Tapped += OnHeaderClicked;
            GestureRecognizers.Add(_tapGestureRecognizer);
        }
    }

    private void SetgroupTitleLabel(string headerLabelText)
    {
        AmhLabelControl groupTitleLabel = new AmhLabelControl(FieldTypes.PrimaryLargeHStartVCenterBoldLabelControl)
        {
            Value = headerLabelText ?? string.Empty
        };
        this.Add(groupTitleLabel, 0, 0);
    }
}