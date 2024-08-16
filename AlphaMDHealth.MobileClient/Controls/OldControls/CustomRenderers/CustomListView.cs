namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents Custom List View
    /// </summary>
    public class CustomListView : ListView
    {
        /// <summary>
        /// Parameterized constructor for  Custom List View class
        /// </summary>
        /// <param name="cachingStrategy"></param>
        public CustomListView(ListViewCachingStrategy cachingStrategy) : base(cachingStrategy)
        {
        }

        /// <summary>
        /// Nul constructor for  Custom List View class
        /// </summary>
        public CustomListView()
        {
        }

        /// <summary>
        ///  Is Selected Property
        /// </summary>
        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(CustomListView), true);

        /// <summary>
        ///  Gets or sets Is Selected
        /// </summary>
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        /// <summary>
        ///  Is Scroll Enabled Property
        /// </summary>
        public static readonly BindableProperty IsScrollEnabledProperty = BindableProperty.Create(nameof(IsScrollEnabled), typeof(bool), typeof(CustomListView), true);

        /// <summary>
        ///  Is Scroll Enabled Property
        /// </summary>
        public bool IsScrollEnabled
        {
            get => (bool)GetValue(IsScrollEnabledProperty);
            set => SetValue(IsScrollEnabledProperty, value);
        }
    }
}
