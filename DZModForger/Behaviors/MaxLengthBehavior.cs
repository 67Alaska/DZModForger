using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior that enforces maximum text length (WinUI 3 TextBox doesn't have MaxLength)
    /// </summary>
    public static class MaxLengthBehavior
    {
        public static int GetMaxLength(DependencyObject obj)
        {
            return (int)obj.GetValue(MaxLengthProperty);
        }

        public static void SetMaxLength(DependencyObject obj, int value)
        {
            obj.SetValue(MaxLengthProperty, value);
        }

        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.RegisterAttached(
                "MaxLength",
                typeof(int),
                typeof(MaxLengthBehavior),
                new PropertyMetadata(0, OnMaxLengthChanged));

        private static void OnMaxLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox && e.NewValue is int maxLength && maxLength > 0)
            {
                textBox.TextChanged += (s, args) =>
                {
                    if (textBox.Text.Length > maxLength)
                    {
                        int cursorPos = textBox.SelectionStart;
                        textBox.Text = textBox.Text.Substring(0, maxLength);
                        textBox.Select(System.Math.Min(cursorPos, maxLength), 0);
                    }
                };
            }
        }
    }
}
