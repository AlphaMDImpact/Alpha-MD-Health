namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Effect for round corner 
/// </summary>
public class RoundCornersEffect : RoutingEffect
{
    ///// <summary>
    ///// Effect for round corner 
    ///// </summary>
    //public RoundCornersEffect() : base($"CommonLibrary.{nameof(RoundCornersEffect)}")
    //{
    //}

    /// <summary>
    /// Corner Radius Bindable property
    /// </summary>
    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.CreateAttached(
            "CornerRadius",
            typeof(int),
            typeof(RoundCornersEffect),
            0,
            propertyChanged: OnCornerRadiusChanged);

    /// <summary>
    /// Get Corner Radius 
    /// </summary>
    /// <param name="view"></param>
    /// <returns></returns>
    public static int GetCornerRadius(BindableObject view) =>
        (int)view.GetValue(CornerRadiusProperty);

    /// <summary>
    /// Set Corner Radius
    /// </summary>
    /// <param name="view">Current iew</param>
    /// <param name="value">Value of radius</param>
    public static void SetCornerRadius(BindableObject view, int value) =>
        view.SetValue(CornerRadiusProperty, value);

    private static void OnCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (!(bindable is View view))
        {
            return;
        }

        var cornerRadius = (int)newValue;
        var effect = view.Effects.FirstOrDefault(e => e is SwitchColorEffect);
        //// var effect = view.Effects.OfType<RoundCornersEffect>().FirstOrDefault();

        if (cornerRadius > 0 && effect == null)
        {
            view.Effects.Add(new RoundCornersEffect());
        }

        if (cornerRadius == 0 && effect != null)
        {
            view.Effects.Remove(effect);
        }
    }
}