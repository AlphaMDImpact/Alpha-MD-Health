//using AlphaMDHealth.Model;
//using AlphaMDHealth.Utility;
//using System.Globalization;

//namespace AlphaMDHealth.MobileClient;

//public class SampleDashboardPage : BasePage
//{
//    private readonly SampleCodesView _sampleListView;
//    private readonly SvgImageButtonView _addButton;

//    public SampleDashboardPage()
//    {
//        _addButton = new SvgImageButtonView(ImageConstants.I_ADD_ICON_SVG, AppImageSize.ImageSizeL, AppImageSize.ImageSizeL);
//        _sampleListView = new SampleCodesView(this, AddParameters(CreateParameter(nameof(BaseDTO.RecordCount), 2.ToString())));
//        var mainLayout = new Grid
//        {
//            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
//            ColumnSpacing = Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture),
//            RowDefinitions =
//            {
//                new RowDefinition { Height = GridLength.Auto },
//                new RowDefinition { Height = GridLength.Auto }
//            },
//            ColumnDefinitions =
//            {
//               new ColumnDefinition{Width= new GridLength(1, GridUnitType.Star)},
//           }
//        };
//        mainLayout.Add(_addButton, 0, 0);
//        mainLayout.Add(_sampleListView, 0, 1);
//        Content = mainLayout;
//    }

//    protected override async void OnAppearing()
//    {
//        base.OnAppearing();
//        await _sampleListView.LoadUIAsync(false).ConfigureAwait(true);
//        _addButton.Clicked += AddButton_Clicked;
//        AppHelper.ShowBusyIndicator = false;
//    }

//    protected override async void OnDisappearing()
//    {
//        await _sampleListView.UnloadUIAsync().ConfigureAwait(true);
//        _addButton.Clicked -= AddButton_Clicked;
//        base.OnDisappearing();
//    }

//    private async void AddButton_Clicked(object sender, EventArgs e)
//    {
//        await _sampleListView.EmployeeClickAsync(Guid.Empty.ToString()).ConfigureAwait(false);
//    }
//}
