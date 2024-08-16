using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;
using System.Runtime.Serialization;

namespace AlphaMDHealth.MobileClient;

public class MedicinesSearchPopupPage : BasePopupPage
{
    private readonly CollectionView _medicineList;
    private readonly CustomSearchControl _customSearch;
    private List<MedicineModel> _searchedMedicineList;
    private PatientMedicationDTO _medicationData;

    /// <summary>
    /// List of serched medicines
    /// </summary>
    [DataMember]
    public List<MedicineModel> SearchedMedicineList
    {
        get => _searchedMedicineList;
        set
        {
            _medicineList.ItemsSource = _searchedMedicineList = value;
        }
    }

    /// <summary>
    /// on click event of Send Button
    /// </summary>
    public event EventHandler<EventArgs> OnSelectMedicineItem;

    /// <summary>
    /// on click event of Close Button
    /// </summary>
    public event EventHandler<EventArgs> OnCloseSearchMedicine;

    /// <summary>
    /// Default constructor 
    /// </summary>
    public MedicinesSearchPopupPage(PatientMedicationDTO medicationData) : base(new BasePage(PageLayoutType.EndToEndPageLayout, false))
    {
        _medicationData = medicationData;
        _parentPage.PageService = new MedicationSevice(App._essentials);
        //todo: CloseWhenBackgroundIsClicked = false;
        Padding = new OnIdiom<Thickness>
        {
            Phone = new Thickness(0, Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture) * 3, 0, 0),
            Tablet = _parentPage.GetPopUpPagePadding(PopUpPageType.Long)
        };
        _parentPage.PageLayout.Padding = new Thickness((double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], 10);
        _parentPage.AddRowColumnDefinition(GridLength.Auto, 1, true);
        _parentPage.AddRowColumnDefinition(GridLength.Star, 1, true);
        _customSearch = new CustomSearchControl
        {
            ControlResourceKey = ResourceConstants.R_SEARCH_TEXT_KEY,
            SearchedOn = SearchedOnType.Both,
            SearchOutSideViewHide = false,
            IsAppliedMargin = false,
        };
        CustomCellModel customCellModel = new CustomCellModel
        {
            CellHeader = nameof(MedicineModel.ShortName),
        };
        _medicineList = new CollectionView
        {
            SelectionMode = SelectionMode.Single,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                SnapPointsType = SnapPointsType.None,
                SnapPointsAlignment = SnapPointsAlignment.Center,
                ItemSpacing = new OnIdiom<double> { Phone = 10, Tablet = 0 }
            },
            ItemTemplate = new DataTemplate(() =>
            {
                if (MobileConstants.IsTablet)
                {
                    return new ContentView { Style = (Style)Application.Current.Resources[StyleConstants.ST_SELECTED_CONTENT_STYLE], Content = new ResponsiveView(customCellModel), Padding = GenericMethods.GetPlatformSpecificValue(new Thickness(1, 1, 1, 0), new Thickness(0), new Thickness(1, 1, 1, 0)) };
                }
                return new ContentView { Style = (Style)Application.Current.Resources[StyleConstants.ST_SELECTED_CONTENT_STYLE], Content = new ResponsiveView(customCellModel) };
            })
        };
        _parentPage.PageLayout.Add(_customSearch, 0, 0);
        _parentPage.PageLayout.Add(_medicineList, 0, 1);
    }

    /// <summary>
    /// Appearing page
    /// </summary>
    protected override async void OnAppearing()
    {
        AppHelper.ShowBusyIndicator = true;
        base.OnAppearing();
        await _parentPage.GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_MEDICATION_GROUP).ConfigureAwait(true);
        _customSearch.PageResources = _parentPage.PageData;
        _medicineList.SelectedItem = null;
        if (GenericMethods.IsListNotEmpty(_searchedMedicineList))
        {
            _customSearch.Value = string.Empty;
            SetTitle(GetHeaderText(_searchedMedicineList.Count));
            _medicineList.ItemsSource = _searchedMedicineList;
            _customSearch.OnSearchTextChanged += OnSearchMedicine;
        }
        else
        {
            if (!GenericMethods.IsListNotEmpty(_searchedMedicineList))
            {
                _searchedMedicineList = new List<MedicineModel>();
                SetTitle(GetHeaderText(_searchedMedicineList.Count));
                _customSearch.Value = string.Empty;
                _medicineList.EmptyView = new CustomLabelControl(LabelType.SecondrySmallCenter)
                {
                    Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_NO_DATA_FOUND_KEY),
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                };
                _customSearch.OnSearchTextChanged -= OnSearchMedicine;
            }
        }
        if (MobileConstants.IsTablet)
        {
            DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
            OnLeftHeaderClickedEvent += OnCancelClicked;
        }
        else
        {
            ShowCloseButton(true);
            OnCloseButtonClickedEvent += OnCancelClicked;
        }

        
        _medicineList.SelectionChanged += OnMedicineSelect;
        AppHelper.ShowBusyIndicator = false;
    }

    private string GetHeaderText(int count)
    {
        return string.Concat(_parentPage.GetResourceValueByKey(ResourceConstants.R_SEARCH_MEDICINE_TEXT_KEY), $" ({count})");
    }

    /// <summary>
    /// Disappearing page
    /// </summary>
    protected override void OnDisappearing()
    {
        _medicineList.SelectionChanged -= OnMedicineSelect;
        if (MobileConstants.IsTablet)
        {
            OnLeftHeaderClickedEvent -= OnCancelClicked;
        }
        else
        {
            OnCloseButtonClickedEvent -= OnCancelClicked;
        }
        base.OnDisappearing();
    }

    private async void OnMedicineSelect(object sender, SelectionChangedEventArgs e)
    {
        var selectedMedicine = (sender as CollectionView).SelectedItem;
        OnSelectMedicineItem.Invoke(selectedMedicine, new EventArgs());
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        OnCloseSearchMedicine.Invoke(sender, new EventArgs());
        await Task.Delay(2);
        _searchedMedicineList.Clear();
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnSearchMedicine(object sender, EventArgs e)
    {
        var searchBar = sender as CustomSearchBar;
        _medicineList.Footer = null;
        if (string.IsNullOrWhiteSpace(searchBar.Text))
        {
            _medicineList.ItemsSource = _searchedMedicineList;
            SetTitle(GetHeaderText(_searchedMedicineList.Count));
        }
        else
        {
            await new MedicineService(App._essentials).SearchMedicineAsync(_medicationData, searchBar.Text);
            SearchedMedicineList = _medicationData.Medicines;
            var searchedMedicine = _searchedMedicineList.FindAll(y =>
            {
                return y.FullName.ToLowerInvariant().Contains(searchBar.Text.Trim().ToLowerInvariant()) 
                    || y.ShortName.ToLowerInvariant().Contains(searchBar.Text.Trim().ToLowerInvariant());
            });
            _medicineList.ItemsSource = searchedMedicine;
            SetTitle(GetHeaderText(searchedMedicine.Count));
            if (searchedMedicine.Count <= 0 && _medicineList.EmptyView == null)
            {
                _medicineList.EmptyView = new CustomLabelControl(LabelType.SecondrySmallCenter)
                {
                    Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_NO_DATA_FOUND_KEY),
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                };
            }
        }
    }
}