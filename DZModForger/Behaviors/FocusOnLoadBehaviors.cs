using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior that sets focus to a control when it loads
    /// </summary>
    public static class FocusOnLoadBehavior
    {
        /// <summary>
        /// Gets or sets whether the control should focus on load
        /// </summary>
        public static bool GetFocusOnLoad(DependencyObject obj)
        {
            return (bool)obj.GetValue(FocusOnLoadProperty);
        }

        public static void SetFocusOnLoad(DependencyObject obj, bool value)
        {
            obj.SetValue(FocusOnLoadProperty, value);
        }

        public static readonly DependencyProperty FocusOnLoadProperty =
            DependencyProperty.RegisterAttached(
                "FocusOnLoad",
                typeof(bool),
                typeof(FocusOnLoadBehavior),
                new PropertyMetadata(false, OnFocusOnLoadChanged));

        private static void OnFocusOnLoadChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element && (bool)e.NewValue)
            {
                element.Loaded += (s, args) =>
                {
                    element.Focus(FocusState.Programmatic);
                };
            }
        }
    }
}
