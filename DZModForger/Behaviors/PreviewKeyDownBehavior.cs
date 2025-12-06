using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior that handles preview key down events
    /// </summary>
    public static class PreviewKeyDownBehavior
    {
        public static KeyEventHandler GetPreviewKeyDown(DependencyObject obj)
        {
            return (KeyEventHandler)obj.GetValue(PreviewKeyDownProperty);
        }

        public static void SetPreviewKeyDown(DependencyObject obj, KeyEventHandler value)
        {
            obj.SetValue(PreviewKeyDownProperty, value);
        }

        public static readonly DependencyProperty PreviewKeyDownProperty =
            DependencyProperty.RegisterAttached(
                "PreviewKeyDown",
                typeof(KeyEventHandler),
                typeof(PreviewKeyDownBehavior),
                new PropertyMetadata(null, OnPreviewKeyDownChanged));

        private static void OnPreviewKeyDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element && e.NewValue is KeyEventHandler handler)
            {
                element.PreviewKeyDown += handler;
            }
        }
    }

    public delegate void KeyEventHandler(object sender, KeyRoutedEventArgs e);
}
