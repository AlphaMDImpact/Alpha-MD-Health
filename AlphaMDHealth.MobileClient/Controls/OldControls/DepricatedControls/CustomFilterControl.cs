using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Runtime.Serialization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Custom Filter Control
/// </summary>
public class CustomFilterControl : ContentView
{
    private readonly Grid _filterLayout;
    private IList<OptionModel> _itemsSource;

    /// <summary>
    /// Selected Values Changed event
    /// </summary>
    public event EventHandler<SelectedItemChangedEventArgs> SelectedItemChanged;

    /// <summary>
    /// Item source
    /// </summary>
    [DataMember]
    public IList<OptionModel> ItemSource
    {
        get { return _itemsSource; }
        set
        {
            _itemsSource = value;
            SetListSource();
        }
    }

    /// <summary>
    /// Custom Filter Control
    /// </summary>
    public CustomFilterControl(bool isScroll)
    {
        _filterLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = GridLength.Auto } },
            ColumnSpacing = 20
        };
        if (isScroll)
        {
            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                Content = _filterLayout,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Always
            };
        }
        else
        {
            Content = _filterLayout;
        }
    }

    private void SetListSource()
    {
        _filterLayout.Children.Clear();
        _filterLayout.ColumnDefinitions.Clear();
        foreach (var item in _itemsSource)
        {
            _filterLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _filterLayout.Add(GetContentItem(item), _filterLayout.Children.Count, 0);
        }
    }

    private Grid GetContentItem(OptionModel item)
    {
        Grid contentItem = new()
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            RowSpacing = 10,
            HorizontalOptions = LayoutOptions.StartAndExpand,
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } },
            Opacity = item.IsSelected ? 1 : 0.3,
            BindingContext = item
        };
        contentItem.Add(new SvgImageView(item.GroupName + Constants.PNG_EXTENSION, AppImageSize.ImageSizeL, AppImageSize.ImageSizeL, Colors.Transparent), 0, 0);
        contentItem.Add(new CustomLabelControl(LabelType.PrimarySmallCenter) { LineBreakMode = LineBreakMode.TailTruncation, Text = item.OptionText }, 0, 1);
        TapGestureRecognizer tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += TapGesture_Tapped;
        contentItem.GestureRecognizers.Add(tapGesture);
        return contentItem;
    }

    private void TapGesture_Tapped(object sender, EventArgs e)
    {
        if (sender is Grid contentItem)
        {
            foreach (var child in _filterLayout.Children)
            {
                (child as Grid).Opacity = 0.5;
            }
            contentItem.Opacity = 1;
            SelectedItemChanged?.Invoke(this, new SelectedItemChangedEventArgs(contentItem.BindingContext, 0));
        }
    }
}
