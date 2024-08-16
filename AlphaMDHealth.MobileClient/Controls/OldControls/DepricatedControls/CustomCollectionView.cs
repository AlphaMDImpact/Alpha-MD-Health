namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomCollectionView : CollectionView
    {
        /// <summary>
        /// 
        /// </summary>
        public int FirstVisibleItemIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int LastVisibleItemIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler Appeared;

        /// <summary>
        /// 
        /// </summary>
        public void RaiseAppeared(object element)
        {
            Appeared?.Invoke(element, EventArgs.Empty);
        }
    }
}
