namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Keyboard event arguments
    /// </summary>
    public class KeyboardEventArgs : EventArgs
    {
        /// <summary>
        /// Gets whether keyboard is displayed
        /// </summary>
        public bool IsKeyboardDisplayed { get; set; }

        /// <summary>
        /// Gets keyboard height
        /// </summary>
        public double KeyboardHeight { get; set; }
    }
}
