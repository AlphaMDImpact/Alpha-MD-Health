namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// SwitchColorEffect
    /// </summary>
    public class SwitchColorEffect : RoutingEffect
    {
        /// <summary>
        /// FalseColor
        /// </summary>
        public static readonly BindableProperty FalseColorProperty = BindableProperty.CreateAttached("FalseColor", typeof(Color), typeof(SwitchColorEffect),
        Colors.Transparent, propertyChanged: OnColorChanged);

        /// <summary>
        /// TrueColor
        /// </summary>
        public static readonly BindableProperty TrueColorProperty = BindableProperty.CreateAttached("TrueColor", typeof(Color), typeof(SwitchColorEffect),
            Colors.Transparent, propertyChanged: OnColorChanged);

        /// <summary>
        /// The colour of the "thumb", the thing you tap or drag to toggle the switch
        /// </summary>
        public static readonly BindableProperty ThumbColorProperty = BindableProperty.CreateAttached("ThumbColor", typeof(Color), typeof(SwitchColorEffect),
            Colors.Silver, propertyChanged: OnColorChanged);

        ///// <summary>
        ///// [assembly: ResolutionGroupName("xamformsdemo")] moved to the AssemblyInfo
        ///// classes in the platform projects
        ///// </summary>
        //public SwitchColorEffect() : base("CommonLibrary.SwitchColorEffect")
        //{
        //}

        private static void OnColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as Switch;
            if (control == null)
            {
                return;
            }

            var color = (Color)newValue;

            var attachedEffect = control.Effects.FirstOrDefault(e => e is SwitchColorEffect);
            if (color != Colors.Transparent && attachedEffect == null)
            {
                control.Effects.Add(new SwitchColorEffect());
            }
            else
            {
                if (color == Colors.Transparent && attachedEffect != null)
                {
                    control.Effects.Remove(attachedEffect);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static Color GetFalseColor(BindableObject view)
        {
            return (Color)view.GetValue(FalseColorProperty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="color"></param>
        public static void SetFalseColor(BindableObject view, string color)
        {
            view.SetValue(FalseColorProperty, color);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static Color GetTrueColor(BindableObject view)
        {
            return (Color)view.GetValue(TrueColorProperty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="color"></param>
        public static void SetTrueColor(BindableObject view, Color color)
        {
            view.SetValue(TrueColorProperty, color);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static Color GetThumbColor(BindableObject view)
        {
            return (Color)view.GetValue(ThumbColorProperty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="color"></param>
        public static void SetThumbColor(BindableObject view, Color color)
        {
            view.SetValue(ThumbColorProperty, color);
        }
    }
}

