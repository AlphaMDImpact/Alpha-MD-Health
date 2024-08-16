using AlphaMDHealth.Utility;
using DevExpress.Maui.Controls;
using Microsoft.Maui.Controls;
using DevExpress.Maui.Editors;
using Microsoft.Maui.Controls.Shapes;

namespace AlphaMDHealth.MobileClient.Controls;

internal class AmhShimmerViewControl : AmhBaseControl
{
    /// <summary>
    /// default constructor
    /// </summary>
    public AmhShimmerViewControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
	internal AmhShimmerViewControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
	}

    protected override void ApplyResourceValue()
    {
        throw new NotImplementedException();
    }

    protected override void EnabledDisableField(bool value)
    {
        throw new NotImplementedException();
    }

    private object GetControlValue()
    {
        throw new NotImplementedException();
    }

    protected override void RenderControl()
    {
        //var scrollView = new ScrollView
        //{
        //    BackgroundColor = (Color)Application.Current.Resources["BackgroundThemeColor"],
        //    Content = new VerticalStackLayout
        //    {
        //        Children =
        //            {
        //                // Create ShimmerView
        //                new ShimmerView
        //                {
        //                    Style = (Style)Application.Current.Resources["ShimmerEffect"],
        //                    //IsLoading = "{Binding DealersLoading}",
        //                    Content = new Grid
        //                    {
        //                        ColumnDefinitions = new ColumnDefinitionCollection
        //                        {
        //                            new ColumnDefinition { Width = GridLength.Star },
        //                            new ColumnDefinition { Width = GridLength.Star },
        //                            new ColumnDefinition { Width = GridLength.Star }
        //                        },
        //                        HeightRequest = 120,
        //                        Margin = new Thickness(0, 24, 0, 0),
        //                        Children =
        //                        {
        //                            // First Grid
        //                            new Grid
        //                            {
        //                                RowDefinitions = new RowDefinitionCollection
        //                                {
        //                                    new RowDefinition { Height = GridLength.Star },
        //                                    new RowDefinition { Height = GridLength.Auto }
        //                                },
        //                                ColumnSpacing = 0,
        //                                Margin = new Thickness(24, 0, 0, 0),
        //                                Children =
        //                                {
        //                                    new Border
        //                                    {
        //                                        Style = (Style)Application.Current.Resources["ElementAction"],
        //                                        BackgroundColor = (Color)Application.Current.Resources["LightThemeColor1"]
        //                                    },
        //                                    new Image { Source = "bed", Margin = new Thickness(28) },
        //                                    new Label
        //                                    {
        //                                        Style = (Style)Application.Current.Resources["TextAction"],
        //                                        //GridRow = 1,
        //                                        Text = "Hotels"
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    },
        //                    LoadingView = new ShimmerView
        //                    {
        //                        Content = new Grid
        //                        {
        //                            ColumnDefinitions = new ColumnDefinitionCollection
        //                            {
        //                                new ColumnDefinition { Width = GridLength.Star },
        //                                new ColumnDefinition { Width = GridLength.Star },
        //                                new ColumnDefinition { Width = GridLength.Star }
        //                            },
        //                            HeightRequest = 100,
        //                            Margin = new Thickness(0, 24, 0, 0),
        //                            Children =
        //                            {
        //                                new Border
        //                                {
        //                                    Style = (Style)Application.Current.Resources["ShimmerElementAction"],
        //                                    //Col = 0,
        //                                    Margin = new Thickness(24, 0, 0, 0)
        //                                },
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //    }
        //};

        //// Set ContentPage Content to ScrollView
        //Content = scrollView;



        //var shimmerView = new ShimmerView
        //{
        //    LoadingView = new Grid
        //    {
        //        RowDefinitions = new RowDefinitionCollection
        //            {
        //                new RowDefinition(40),
        //                new RowDefinition(100),
        //                new RowDefinition(400),
        //                new RowDefinition(180)
        //            },
        //        ColumnDefinitions = new ColumnDefinitionCollection
        //            {
        //                new ColumnDefinition(),
        //                new ColumnDefinition(),
        //                new ColumnDefinition()
        //            },
        //        Padding = new Thickness(10)
        //    }
        //};

        //var loadingViewGrid = (Grid)shimmerView.LoadingView;

        //loadingViewGrid.Children.Add(
        //    new Label
        //    {
        //        Text = "Content is being loaded...",
        //        //Row = 0,
        //        //ColumnSpan = 3,
        //        FontSize = 16
        //    }
        //);

        //loadingViewGrid.Children.Add(
        //    new Ellipse { Fill = Colors.Gray, HeightRequest = 80, WidthRequest = 80 }
        //);

        //loadingViewGrid.Children.Add(
        //    new Ellipse { Fill = Colors.Gray, HeightRequest = 80, WidthRequest = 80 }
        //    //1, 1
        //);

        //loadingViewGrid.Children.Add(
        //    new Ellipse { Fill = Colors.Gray, HeightRequest = 80, WidthRequest = 80 }
        //    //1, 2
        //);

        //loadingViewGrid.Children.Add(
        //    new Rectangle { Fill = Colors.Gray, HeightRequest = 380 }
        //    //2, 0
        //);

        //loadingViewGrid.Children.Add(
        //    new Rectangle { Fill = Colors.Gray, HeightRequest = 150 }
        //    //3, 0
        //);

        //Content = shimmerView;
    }

    private void SetControlValue()
    {
        throw new NotImplementedException();
    }
}