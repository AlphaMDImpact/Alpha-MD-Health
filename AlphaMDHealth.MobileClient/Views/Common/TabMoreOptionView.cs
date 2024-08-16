using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Tab more option view
/// </summary>
public class TabMoreOptionView : BaseLibCollectionView
{
    private List<OptionModel> _tabOptions;
    /// <summary>
    /// Tab more option view
    /// </summary>
    public TabMoreOptionView()
    {
        CustomCellModel customCellModel = new CustomCellModel
        {
            CellID = nameof(OptionModel.GroupName),
            CellHeader = nameof(OptionModel.OptionText),
            NoMarginNoSeprator = true,
            IsList = true,
        };
        Grid mainLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR],
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };
        AddListView(mainLayout, customCellModel, 0, 0, null, ListViewCachingStrategy.RetainElement, StyleConstants.TRANSPARENT_COLOR_STRING);
        Content = mainLayout;
    }


    /// <summary>
    /// Loads menus to be shown in list
    /// </summary>
    /// <returns>Returns ContentView with menu list</returns>
    internal async Task LoadUIDataAsync(List<OptionModel> tabOptions)
    {
        _tabOptions = tabOptions;
        ListViewField.SelectedItem = _tabOptions.FirstOrDefault(Tab => Tab.IsSelected);
        ListViewField.ItemTapped += OnSelectionChanged;
        await Task.CompletedTask;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ListViewField.HasUnevenRows = true;
            ListViewField.ItemsSource = _tabOptions;
        });
    }

    private void OnSelectionChanged(object sender, ItemTappedEventArgs e)
    {
        if (ListViewField.SelectedItem != null && ListViewField.SelectedItem != _tabOptions.FirstOrDefault(Tab => Tab.IsSelected))
        {
            var item = ListViewField.SelectedItem as OptionModel;
            if (!string.IsNullOrWhiteSpace(item.GroupName))
            {
                InvokeListRefresh(item, new System.EventArgs());
            }
        }
    }

    /// <summary>
    /// Unregister events attached in view
    /// </summary>
    internal void UnLoadUIData()
    {
        ListViewField.ItemTapped -= OnSelectionChanged;
    }
}