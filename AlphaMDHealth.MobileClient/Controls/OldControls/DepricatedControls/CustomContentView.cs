namespace AlphaMDHealth.MobileClient;

public class CustomContentView : ContentView
{
    /// <summary>
    /// event for Long press
    /// </summary>
    public event EventHandler LongPressed;
    /// <summary>
    /// mothod for Long press
    /// </summary>
    public void InvokeLongPressedEvent(EventArgs e)
    {
        LongPressed(this, e);
    }

    /// <summary>
    /// event for Long press
    /// </summary>
    public event EventHandler Pressed;

    /// <summary
    /// mothod for Long press
    /// </summary>
    public void InvokePressedEvent(EventArgs e)
    {
        Pressed(this, e);
    }
}