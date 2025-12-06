using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior that auto-focuses next control on Enter/Tab
    /// </summary>
    public static class AutoFocusNextBehavior
    {
        public static Control GetNextControl(DependencyObject obj)
        {
            return (Control)obj.GetValue(NextControlProperty);
        }

        public static void SetNextControl(DependencyObject obj, Control value)
        {
            obj.SetValue(NextControlProperty, value);
        }

        public static readonly DependencyProperty NextControlProperty =
            DependencyProperty.RegisterAttached(
                "NextControl",
                typeof(Control),
                typeof(AutoFocusNextBehavior),
                new PropertyMetadata(null, OnNextControlChanged));

        private static void OnNextControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && e.NewValue is Control nextControl)
            {
                control.KeyDown += (s, args) =>
                {
                    if (args.Key == Windows.System.VirtualKey.Tab ||
                        args.Key == Windows.System.VirtualKey.Enter)
                    {
                        nextControl.Focus(FocusState.Keyboard);
                        args.Handled = true;
                    }
                };
            }
        }
    }
}
