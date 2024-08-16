namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// 
    /// </summary>
    public class SwipeViewExt : SwipeView
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsOpened { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SwipeViewExt()
        {
            this.CloseRequested += OnCloseRequested;
            this.SwipeEnded += OnSwipeEnded;
            this.SwipeStarted += OnSwipeStarted;
            this.SwipeChanging += OnSwipeChanging;
        }

        /// <summary>
        /// 
        /// </summary>
        public EventHandler TappedHandler;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler Tapped
        {
            add { TappedHandler += value; }
            remove { TappedHandler -= value; }
        }

        private void OnCloseRequested(object sender, EventArgs e)
        {
            IsOpened = false;
        }

        private void OnSwipeEnded(object sender, SwipeEndedEventArgs e)
        {
            IsOpened = true;
        }

        private void OnSwipeStarted(object sender, SwipeStartedEventArgs e)
        {
            // to do
        }

        private void OnSwipeChanging(object sender, SwipeChangingEventArgs e)
        {
            // to do
        }
    }
}
