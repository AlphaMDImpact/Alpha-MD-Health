namespace AlphaMDHealth.MobileClient.Controls.DevExpress;
using Microsoft.Maui.Controls;
//using DevExpress.Maui.Controls;
//using DevExpress.Xpo.DB.Helpers;
using global::DevExpress.Maui.Controls;

public class TabMenuControl : ContentPage
{
    private TabViewItem moreTab;
    bool subMenuLblCreated = true;
    bool settingLabelTabCreated = true;
    private StackLayout settingLabelTab = null;
    private Label label;
    // private StackLayout settingLabelTab;
    //private StackLayout settingLabelTab;
    string Appointmentdata = "Welcome to your Appointments dashboard! Here, you can easily manage and keep track of all your upcoming appointments. ";
    string profileData = "Welcome to your Profile! This is your space to showcase your identity and manage your personal information";

    public TabMenuControl()
	{
        Resources = new ResourceDictionary();
        Resources["unselectedItemColor"] = Color.FromHex("#757575");
        Resources["DashboardColor"] = Color.FromHex("#1e88e5");
        Resources["TasksColor"] = Color.FromHex("#1e88e5");
        Resources["ReadingsColor"] = Color.FromHex("#1e88e5");
        Resources["EducationsColor"] = Color.FromHex("#1e88e5");
        Resources["MoreInfoColor"] = Color.FromHex("#1e88e5");

        var tabView = new TabView
        {
            HeaderPanelPosition = HeaderContentPosition.Bottom,
            HeaderPanelWidth = 20,
            IsSelectedItemIndicatorVisible = false,
            ItemHeaderTextColor = (Color)Resources["unselectedItemColor"],
            ItemHeaderIconColor = (Color)Resources["unselectedItemColor"],
            ItemHeaderFontSize = 6
        };


        AddTabOrSubmenu(tabView, null, "Dashboard", "dashboard", (Color)Resources["DashboardColor"], "Dashboard List here");
        AddTabOrSubmenu(tabView, null, "Tasks", "calendar", (Color)Resources["TasksColor"], "Tasks List Here");
        AddTabOrSubmenu(tabView, null, "Readings", "people", (Color)Resources["ReadingsColor"], "See Readings Here");
        AddTabOrSubmenu(tabView, null, "Educations", "people", (Color)Resources["EducationsColor"], "See Educations Info Here");
        AddTabOrSubmenu(tabView, null, "My Appointments", "people", (Color)Resources["EducationsColor"], "See Educations Info Here");
        AddTabOrSubmenu(tabView, null, "My Appointments", "schedule", (Color)Resources["EducationsColor"], "See Educations Info Here");
        AddTabOrSubmenu(tabView, null, "Chats", "chat", (Color)Resources["EducationsColor"], "See Educations Info Here");
        AddTabOrSubmenu(tabView, null, "Medications", "medications", (Color)Resources["EducationsColor"], "See Educations Info Here");
        AddTabOrSubmenu(tabView, null, "Files And Documents", "document", (Color)Resources["EducationsColor"], "See Educations Info Here");
        AddTabOrSubmenu(tabView, null, "Devices", "device", (Color)Resources["EducationsColor"], "See Educations Info Here");
        AddTabOrSubmenu(tabView, "Settings", "Profile", "profile", (Color)Resources["EducationsColor"], "See Educations Info Here");
        AddTabOrSubmenu(tabView, "Settings", "My Programs", "programs", (Color)Resources["EducationsColor"], "See Educations Info Here");
        AddTabOrSubmenu(tabView, "Settings", "Select Language", "language", (Color)Resources["EducationsColor"], "See Educations Info Here");


        void AddTabOrSubmenu(TabView tabView, string subMenu, string headerText, string headerIcon, Color selectedHeaderColor, string contentText)
        {
            if (tabView.Items.Count < 4)
            {
                var tabItem = CreateTab(headerText, headerIcon, selectedHeaderColor, contentText);
                tabView.Items.Add(tabItem);
            }
            else if (tabView.Items.Count >= 3)
            {
                if (tabView.Items.Count == 4)
                {
                    moreTab = new TabViewItem
                    {
                        HeaderText = "More",
                        HeaderIcon = "people",
                        SelectedHeaderTextColor = (Color)Resources["MoreInfoColor"],
                        SelectedHeaderIconColor = (Color)Resources["MoreInfoColor"],
                        //Content = CreateMoreTabContent(headerText, headerIcon, selectedHeaderColor, contentText)
                        //Content = CreateMoreTabContent()
                    };
                    moreTab.Content = new StackLayout();
                    tabView.Items.Add(moreTab);

                }
                else if (tabView.Items.Count >= 4)
                {
                    var moreTabContent = moreTab.Content as StackLayout;
                    tabView.Items.Remove(moreTab);

                    // Modify the content of moreTab
                    moreTabContent.Children.Add(CreateMoreTabContent(headerText, subMenu, headerIcon, selectedHeaderColor, contentText));

                    // Add the modified moreTab back to tabView
                    tabView.Items.Add(moreTab);
                }

            }
        }

        Content = tabView;
    }

    private TabViewItem CreateTab(string headerText, string headerIcon, Color selectedHeaderColor, string contentText)
    {
        var tabItem = new TabViewItem
        {
            HeaderText = headerText,
            HeaderIcon = headerIcon,
            SelectedHeaderTextColor = selectedHeaderColor,
            SelectedHeaderIconColor = selectedHeaderColor
        };

        if (!string.IsNullOrEmpty(contentText))
        {
            tabItem.Content = new Grid
            {
                Children =
                    {
                        new Label
                        {
                            Text = contentText,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.CenterAndExpand
                        }
                    }
            };
        }

        return tabItem;
    }



    private View CreateMoreTabContent(string headerText, string subMenu, string headerIcon, Color selectedHeaderColor, string contentText)
    {
        if (subMenu == "Settings")
        {
            return CreateSettingLabelTab(headerText, headerText, headerIcon);
        }
        else
        {
            return CreateRegularMoreTabContent(headerText, headerIcon, selectedHeaderColor, contentText);
        }
    }

    private View CreateSettingLabelTab(string labelText, string headerText, string headerIcon)
    {
        settingLabelTab = new StackLayout
        {
            Orientation = StackOrientation.Vertical,
            Padding = new Thickness(-7),
        };

        if (subMenuLblCreated)
        {
            label = new Label
            {
                Text = "Settings",
                FontSize = 24,
                Padding = new Thickness(5),
                Margin = new Thickness(5),
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromRgb(0, 0, 0),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.CenterAndExpand,
            };
            subMenuLblCreated = false;
            settingLabelTab.Children.Add(label);
        }

        //foreach (var item in headerText)
        //{
        settingLabelTab.Children.Add(CreateGridTab(headerText, headerText, headerIcon));
        //}

        return settingLabelTab;
    }

    private View CreateRegularMoreTabContent(string headerText, string headerIcon, Color selectedHeaderColor, string contentText)
    {
        var moreTabContent = new ScrollView
        {
            Orientation = ScrollOrientation.Vertical,
            Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                BackgroundColor = Color.FromRgb(245, 245, 245),
                Children =
                    {
                        CreateGridTab(headerText, headerText,headerIcon),
                    }
            }
        };

        return moreTabContent;
    }

    private View CreateGridTab(string headerText, string labelText, string headerIcon)
    {
        var tabGrid = new Grid
        {
            Padding = new Thickness(10),
            RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto }
                }
        };

        var innerGrid = new Grid
        {
            BackgroundColor = Color.FromRgb(245, 245, 245),
            Padding = new Thickness(5),
            ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
        };

        var tabImage = new Image
        {
            Source = headerIcon,
            HeightRequest = 30,
            WidthRequest = 30,
            HorizontalOptions = LayoutOptions.Start
        };

        var tabLabel = new Label
        {
            Text = labelText,
            FontSize = 18,
            TextColor = Color.FromRgb(0, 0, 0),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.CenterAndExpand,
        };

        Grid.SetColumn(tabLabel, 1);

        var rightImage = new Image
        {
            Source = "right.svg",
            HeightRequest = 20,
            WidthRequest = 20,
            HorizontalOptions = LayoutOptions.Start,
        };
        Grid.SetColumn(rightImage, 2);

        var tapGestureRecognizer = new TapGestureRecognizer
        {
            CommandParameter = headerText,
        };

        tapGestureRecognizer.Tapped += TabGestureRecognizer_Tapped; // Attach the event handler
        tabGrid.GestureRecognizers.Add(tapGestureRecognizer);

        innerGrid.Children.Add(tabImage);
        innerGrid.Children.Add(tabLabel);
        innerGrid.Children.Add(rightImage);

        tabGrid.Children.Add(innerGrid);

        return tabGrid;
    }

    private async void TabGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        string data = "";
        var tabName = ((sender as Grid)?.GestureRecognizers[0] as TapGestureRecognizer)?.CommandParameter;

        if (tabName == "My Appointments")
        {
            data = Appointmentdata;
        }

        if (tabName == "Profile")
        {
            data = profileData;
        }

        switch (tabName)
        {
            case "My Appointments":
                //await Navigation.PushAsync(new TabViewExample(data));
                break;
            case "Chats":
                //await Navigation.PushAsync(new TabViewExample5(data));
                break;
            case "Medications":
                //await Navigation.PushAsync(new TabViewExample4(data));
                break;
            case "Files And Documents":
                //await Navigation.PushAsync(new TabViewExample3(data));
                break;
            case "Devices":
                //await Navigation.PushAsync(new TabViewExample2(data));
                break;
            case "Tab6":
                //await Navigation.PushAsync(new TabViewExample1(data));
                break;
            case "Profile":
                //await Navigation.PushAsync(new TabViewExampleProfile(data));
                break;
            case "My Programs":
                //await Navigation.PushAsync(new TabViewExamplePrograms(data));
                break;
            case "Select Language":
                //await Navigation.PushAsync(new TabViewExamplelan(data));
                break;
            default:
                break;
        }
    }
}
