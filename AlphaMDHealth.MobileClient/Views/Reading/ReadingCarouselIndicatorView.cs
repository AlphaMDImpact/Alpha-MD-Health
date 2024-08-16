using AlphaMDHealth.Utility;
using System.Collections;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// ReadingCarouselIndicatorView
/// </summary>
public class ReadingCarouselIndicatorView : Grid
{
    private string _unselectedImageSource;
    private string _selectedImageSource;
    private readonly StackLayout _indicators = new StackLayout
    {
        Orientation = StackOrientation.Horizontal,
        HorizontalOptions = LayoutOptions.CenterAndExpand
    };

    /// <summary>
    /// ReadingCarouselIndicatorView constructor
    /// </summary>
    public ReadingCarouselIndicatorView()
    {
        this.HorizontalOptions = LayoutOptions.Center;
        this.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        this.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        this.Add(_indicators);
    }

    /// <summary>
    /// SelectedIndicatorProperty
    /// </summary>
    public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(int), typeof(ReadingCarouselIndicatorView), 0, BindingMode.TwoWay, propertyChanging: PositionChanging);
    /// <summary>
    /// 
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(ReadingCarouselIndicatorView), Enumerable.Empty<object>(), propertyChanged: ItemsChanged);
    /// <summary>
    /// 
    /// </summary>
    public static readonly BindableProperty SelectedIndicatorProperty = BindableProperty.Create(nameof(SelectedIndicator), typeof(string), typeof(ReadingCarouselIndicatorView), "");

    /// <summary>
    /// UnselectedIndicatorProperty
    /// </summary>
    public static readonly BindableProperty UnselectedIndicatorProperty = BindableProperty.Create(nameof(UnselectedIndicator), typeof(string), typeof(ReadingCarouselIndicatorView), "");

    /// <summary>
    /// IndicatorWidthProperty
    /// </summary>
    public static readonly BindableProperty IndicatorWidthProperty = BindableProperty.Create(nameof(IndicatorWidth), typeof(double), typeof(ReadingCarouselIndicatorView), 0.0);

    /// <summary>
    /// IndicatorHeightProperty 
    /// </summary>
    public static readonly BindableProperty IndicatorHeightProperty = BindableProperty.Create(nameof(IndicatorHeight), typeof(double), typeof(ReadingCarouselIndicatorView), 0.0);

    /// <summary>
    /// SelectedIndicator property
    /// </summary>
    public string SelectedIndicator
    {
        get { return (string)this.GetValue(SelectedIndicatorProperty); }
        set { this.SetValue(SelectedIndicatorProperty, value); }
    }

    /// <summary>
    /// UnselectedIndicator property
    /// </summary>
    public string UnselectedIndicator
    {
        get { return (string)this.GetValue(UnselectedIndicatorProperty); }
        set { this.SetValue(UnselectedIndicatorProperty, value); }
    }

    /// <summary>
    /// IndicatorWidth property
    /// </summary>
    public double IndicatorWidth
    {
        get { return (double)this.GetValue(IndicatorWidthProperty); }
        set { this.SetValue(IndicatorWidthProperty, value); }
    }

    /// <summary>
    /// IndicatorHeight property
    /// </summary>
    public double IndicatorHeight
    {
        get { return (double)this.GetValue(IndicatorHeightProperty); }
        set { this.SetValue(IndicatorHeightProperty, value); }
    }

    /// <summary>
    /// Position property
    /// </summary>
    public int Position
    {
        get { return (int)this.GetValue(PositionProperty); }
        set { this.SetValue(PositionProperty, value); }
    }

    /// <summary>
    /// ItemsSource property
    /// </summary>
    public IEnumerable ItemsSource
    {
        get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
        set { this.SetValue(ItemsSourceProperty, value); }
    }

    private void Clear()
    {
        _indicators.Children?.Clear();
    }

    private void Init(int position)
    {
        _unselectedImageSource = _unselectedImageSource ?? UnselectedIndicator;
        _selectedImageSource = _selectedImageSource ?? SelectedIndicator;
        if (_indicators.Children.Count > 0)
        {
            UpdateIndicators(position);
        }
        else
        {
            var enumerator = ItemsSource.GetEnumerator();
            int count = 0;
            while (enumerator.MoveNext())
            {
                CustomImageControl image = position == count ? BuildImage(State.Selected, count) : BuildImage(State.Unselected, count);
                _indicators.Add(image);
                count++;
            }
        }
    }

    private void UpdateIndicators(int position)
    {
        for (int i = 0; i < _indicators.Children.Count; i++)
        {
            if (((CustomImageControl)_indicators.Children[i]).ClassId == nameof(State.Selected) && i != position)
            {
                _indicators.Children[i] = BuildImage(State.Unselected, i);
            }
            else
            {
                if (((CustomImageControl)_indicators.Children[i]).ClassId == nameof(State.Unselected) && i == position)
                {
                    _indicators.Children[i] = BuildImage(State.Selected, i);
                }
            }
        }
    }

    private CustomImageControl BuildImage(State state, int position)
    {
        CustomImageControl image = new CustomImageControl(AppImageSize.ImageSizeXXXS, AppImageSize.ImageSizeXXXS, null, null, true)
        {
            ClassId = state.ToString(),
            HorizontalOptions = LayoutOptions.End,
        };
        switch (state)
        {
            case State.Selected:
                image.DefaultValue = _selectedImageSource;
                break;
            case State.Unselected:
                image.DefaultValue = _unselectedImageSource;
                break;
            default:
                return image;
        }
        image.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { Position = position; }) });
        return image;
    }

    private static void PositionChanging(object bindable, object oldValue, object newValue)
    {
        var carouselIndicators = bindable as ReadingCarouselIndicatorView;
        carouselIndicators.Init(Convert.ToInt32(newValue, CultureInfo.InvariantCulture));
    }

    private static void ItemsChanged(object bindable, object oldValue, object newValue)
    {
        var carouselIndicators = bindable as ReadingCarouselIndicatorView;
        carouselIndicators.Clear();
        carouselIndicators.Init(0);
    }

    /// <summary>
    /// State Enums
    /// </summary>
    public enum State
    {
        /// <summary>
        /// Selected
        /// </summary>
        Selected,

        /// <summary>
        /// Unselected
        /// </summary>
        Unselected
    }
}