using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.CollectionView;
using System.Collections;

namespace AlphaMDHealth.MobileClient;

internal class AmhCardsControl : AmhBaseControl
{
    private string _value;
    private DXCollectionView _collectionView;

    private void ApplyDataSource()
    {
        if (_options != null)
        {
            _collectionView.ItemsSource = _options;
        }

        Content = _collectionView;
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhCardsControl() : this(FieldTypes.Default) {  }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhCardsControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    protected override void ApplyOptions()
    {
        ApplyDataSource();
    }

    protected override void ApplyResourceValue() { }

    protected override void EnabledDisableField(bool value)
    {
        _collectionView.IsEnabled = value;
    }

    protected override void RenderControl()
    {
        _collectionView = new DXCollectionView
        {
            Orientation = LayoutOrientation.Horizontal,
            HeightRequest =130,
            ItemTemplate = new DataTemplate(() =>
            {
                var content = new ContentView
                {
                    Content = new Frame
                    {                             
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Start,
                        Style = (Style)Application.Current.Resources[StyleConstants.ST_CARD_FRAME],
                        Content = new AmhCardsViewCell()
                    }
                };
                return content;
            }),
        };
    }

    /// <summary>
    /// Control value as object
    /// </summary>
    internal string Value
    {
        get
        {
            return GetControlValue();
        }
        set
        {
            if (_value != value)
            {
                _value = value;
                SetControlValue();
            }
        }
    }

    private string GetControlValue()
    {
        return null;
    }

    private void SetControlValue()
    {

    }
}

