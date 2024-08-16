using AlphaMDHealth.Utility;
using DevExpress.Maui.CollectionView;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Represents App List Styles
/// </summary>
public partial class AppStyles
{
    private void CreateListStyle()
    {
        AddDevExpressListStyle();
        OldListControl();
    }

    private void OldListControl()
    {
        //Style defaultCustomListViewStyle = new Style(typeof(CustomListView))
        //{
        //    Setters =
        //    {
        //        new Setter { Property = CustomListView.SeparatorColorProperty, Value =  _separatorAndDisableColor },
        //        new Setter { Property = CustomListView.SeparatorVisibilityProperty, Value = SeparatorVisibility.None},
        //        new Setter { Property = CustomListView.FlowDirectionProperty , Value = DefaultFlowDirection},
        //        new Setter { Property = CustomListView.IsPullToRefreshEnabledProperty , Value = false},
        //        //new Setter { Property = CustomListView.BackgroundColorProperty, Value = Color.Transparent }, //todo:
        //        new Setter { Property = CustomListView.SelectionModeProperty , Value = ListViewSelectionMode.None}
        //    }
        //};
        //Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_CUSTOMLISTVIEW_STYLE, defaultCustomListViewStyle);


        //Style CustomListView1RowStyle = new Style(typeof(CustomListView))
        //{
        //    BasedOn = defaultCustomListViewStyle,
        //    Setters =
        //    {
        //        new Setter { Property = CustomListView.SelectionModeProperty , Value = ListViewSelectionMode.Single},
        //        //new Setter { Property = CustomListView.BackgroundColorProperty, Value =  Color.Transparent},//todo:
        //        new Setter { Property = CustomListView.RowHeightProperty, Value = Convert.ToInt32(_singleRowHeight)},
        //        new Setter { Property = CustomListView.VerticalOptionsProperty, Value = LayoutOptions.CenterAndExpand},
        //    }
        //};

        //Style CustomListView2RowStyle = new Style(typeof(CustomListView))
        //{
        //    BasedOn = defaultCustomListViewStyle,
        //    Setters =
        //    {
        //        new Setter { Property = CustomListView.RowHeightProperty,Value = new OnIdiom<int>{Phone=Convert.ToInt32(_doubleRowHeight),Tablet=Convert.ToInt32(_doubleRowHeight) } },
        //        new Setter { Property = CustomListView.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
        //        new Setter { Property = CustomListView.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
        //    }
        //};

        //Style CustomListView3RowStyle = new Style(typeof(CustomListView))
        //{
        //    BasedOn = defaultCustomListViewStyle,
        //    Setters =
        //    {
        //        new Setter { Property = CustomListView.RowHeightProperty,Value = Convert.ToInt32(_tripleRowHeight)},
        //        new Setter { Property = CustomListView.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
        //        new Setter { Property = CustomListView.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
        //    }
        //};

        //Style CustomListView5RowStyle = new Style(typeof(CustomListView))
        //{
        //    BasedOn = defaultCustomListViewStyle,
        //    Setters =
        //    {
        //        new Setter { Property = CustomListView.SeparatorVisibilityProperty, Value = SeparatorVisibility.None},
        //        new Setter { Property = CustomListView.RowHeightProperty,Value = Convert.ToInt32(_fiveRowHeight)},
        //    }
        //};
        //Style CustomListView6RowStyle = new Style(typeof(CustomListView))
        //{
        //    BasedOn = defaultCustomListViewStyle,
        //    Setters =
        //    {
        //        new Setter { Property = CustomListView.RowHeightProperty,Value = Convert.ToInt32(_sixRowHeight)},
        //        new Setter { Property = CustomListView.BackgroundColorProperty,Value = _genericBackgroundColor},
        //        new Setter { Property = CustomListView.HorizontalOptionsProperty,Value = LayoutOptions.FillAndExpand },
        //        new Setter { Property = CustomListView.VerticalOptionsProperty,Value =  LayoutOptions.FillAndExpand }
        //    },
        //};
        //Application.Current.Resources.Add(StyleConstants.ST_CustomListView_1_ROW_HEIGHT_STYLE, CustomListView1RowStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_CustomListView_2_ROW_HEIGHT_STYLE, CustomListView2RowStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_CustomListView_3_ROW_HEIGHT_STYLE, CustomListView3RowStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_CustomListView_5_ROW_HEIGHT_STYLE, CustomListView5RowStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_CustomListView_6_ROW_HEIGHT_STYLE, CustomListView6RowStyle);
    }

    private void AddDevExpressListStyle()
    {

        //var SelectedItemAppearance = new CollectionViewSelectedItemAppearance { BackgroundColor = Color.From },

        Style defaultDXCollectionViewStyle = new Style(typeof(DXCollectionView))
        {
            Setters =
            {
                new Setter { Property = DXCollectionView.FlowDirectionProperty , Value = DefaultFlowDirection},
                new Setter { Property = DXCollectionView.SelectionModeProperty, Value =  SelectionMode.Single },
                //new Setter { Property = DXCollectionView.SeparatorVisibilityProperty, Value = SeparatorVisibility.None},
                new Setter { Property = DXCollectionView.IsPullToRefreshEnabledProperty , Value = false },
                new Setter { Property = DXCollectionView.MarginProperty, Value =  new Thickness(0) },
                new Setter { Property = DXCollectionView.ItemSpacingProperty, Value =  5 },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_DX_COLLECTION_VIEW_STYLE, defaultDXCollectionViewStyle);
    }
}