namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Security Configuration Model
    /// </summary>
    public class SecurityConfigurationModel
    {
        /// <summary>
        /// true if jail broken devices are to be blocked
        /// </summary>
        public bool BlockJailbrokenDevice { get; set; }

        /// <summary>
        /// true if screenshots are to be allowed in app else false
        /// </summary>
        public bool IsScreenshotAllowed { get; set; }

        /// <summary>
        /// true if tapping jacking is allowed else false
        /// </summary>
        public bool BlockTapJacking { get; set; }

        /// <summary>
        /// true if copy paste text is allowed in app else false
        /// </summary>
        public bool IsReadCopyClipboardAllowed { get; set; }

        /// <summary>
        /// true if certificate pinning is enabled else false
        /// </summary>
        public bool IsCertifiatePiningAllowed { get; set; }
    }
}
