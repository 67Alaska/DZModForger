using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior that auto-scrolls to end of ScrollViewer when content changes
    /// </summary>
    public static class ScrollToEndBehavior
    {
        public static bool GetScrollToEnd(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollToEndProperty);
        }

        public static void SetScrollToEnd(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollToEndProperty, value);
        }

        public static readonly DependencyProperty ScrollToEndProperty =
            DependencyProperty.RegisterAttached(
                "ScrollToEnd",
                typeof(bool),
                typeof(ScrollToEndBehavior),
                new PropertyMetadata(false, OnScrollToEndChanged));

        private static void OnScrollToEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer && (bool)e.NewValue)
            {
                scrollViewer.Loaded += (s, args) =>
                {
                    scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null, true);
                };
            }
        }
    }
}
