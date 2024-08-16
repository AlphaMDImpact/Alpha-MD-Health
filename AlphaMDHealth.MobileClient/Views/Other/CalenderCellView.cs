//using AlphaMDHealth.Model;
//using AlphaMDHealth.Utility;

//namespace AlphaMDHealth.MobileClient;

//public class CalenderCellView : Grid
//{
//    public CalenderCellView()
//    {
//        var appointmentHeader = new CustomLabelControl(LabelType.ListHeaderStyle);
//        appointmentHeader.SetBinding(CustomLabelControl.TextProperty, nameof(AppointmentModel.PageHeading));
//        var appointmentDescription = new CustomLabelControl(LabelType.PrimarySmallLeft);

//        appointmentDescription.SetBinding(CustomLabelControl.TextProperty, nameof(AppointmentModel.PageData));
//        var appoinmentBoxView = new BoxView
//        {
//            WidthRequest = 10,
//            VerticalOptions = LayoutOptions.FillAndExpand,
//            BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR]
//        };
//        var seprator = new BoxView
//        {
//            HeightRequest = 1,
//            HorizontalOptions = LayoutOptions.FillAndExpand,
//            BackgroundColor = Color.FromArgb(StyleConstants.SEPERATOR_N_DISABLED_COLOR)
//        };
//        Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE];
//        VerticalOptions = LayoutOptions.StartAndExpand;
//        RowSpacing = 0;
//        ColumnSpacing = (double)App.Current.Resources[StyleConstants.ST_APP_PADDING];
//        BackgroundColor = Color.FromArgb("#190078D3");
//        RowDefinitions.Add(new RowDefinition { Height = new GridLength(15, GridUnitType.Absolute) });
//        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
//        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
//        RowDefinitions.Add(new RowDefinition { Height = new GridLength(15, GridUnitType.Absolute) });
//        RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Absolute) });
//        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
//        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
//        this.Add(appointmentHeader, 1, 1);
//        this.Add(appointmentDescription, 1, 2);
//        this.Add(appoinmentBoxView, 0, 0);
//        Grid.SetRowSpan(appoinmentBoxView, 5);
//        this.Add(seprator, 0, 4);
//        Grid.SetColumnSpan(seprator, 2);
//    }
//}