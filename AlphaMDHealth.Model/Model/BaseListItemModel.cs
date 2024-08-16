using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Base model for list items
    /// </summary>
    public class BaseListItemModel
    {
        //todo:
        ///// <summary>
        ///// Left icon source 
        ///// </summary>
        [Ignore]
        public string LeftSourceIcon { get; set; }

        /// <summary>
        /// Left default icon
        /// </summary>
        public string LeftDefaultIcon { get; set; }

        /// <summary>
        /// Background color of image
        /// </summary>
        [Ignore]
        public string ImageBackgroundColor { get; set; }
    }
}
