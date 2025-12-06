using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior that selects all text in a TextBox when it gains focus
    /// </summary>
    public static class SelectAllOnFocusBehavior
    {
        public static bool GetSelectAllOnFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectAllOnFocusProperty);
        }

        public static void SetSelectAllOnFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectAllOnFocusProperty, value);
        }

        public static readonly DependencyProperty SelectAllOnFocusProperty =
            DependencyProperty.RegisterAttached(
                "SelectAllOnFocus",
                typeof(bool),
                typeof(SelectAllOnFocusBehavior),
                new PropertyMetadata(false, OnSelectAllOnFocusChanged));

        private static void OnSelectAllOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox && (bool)e.NewValue)
            {
                textBox.GotFocus += (s, args) =>
                {
                    textBox.SelectAll();
                };
            }
        }
    }
}
