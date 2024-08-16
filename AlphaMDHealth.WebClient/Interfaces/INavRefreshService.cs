using System;
namespace AlphaMDHealth.WebClient
{
    public interface INavRefreshService
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3906:Event Handlers should have the correct signature", Justification = "Not required as no parameter is used")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3908:Generic event handlers should be used", Justification = "Not reqired")]
        public event Action RefreshRequested;
        public void CallRequestRefresh();
    }
}
