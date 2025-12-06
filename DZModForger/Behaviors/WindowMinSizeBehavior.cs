using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior that enforces minimum window size
    /// </summary>
    public static class WindowMinSizeBehavior
    {
        public static int GetMinWidth(DependencyObject obj)
        {
            return (int)obj.GetValue(MinWidthProperty);
        }

        public static void SetMinWidth(DependencyObject obj, int value)
        {
            obj.SetValue(MinWidthProperty, value);
        }

        public static readonly DependencyProperty MinWidthProperty =
            DependencyProperty.RegisterAttached(
                "MinWidth",
                typeof(int),
                typeof(WindowMinSizeBehavior),
                new PropertyMetadata(800, OnMinWidthChanged));

        public static int GetMinHeight(DependencyObject obj)
        {
            return (int)obj.GetValue(MinHeightProperty);
        }

        public static void SetMinHeight(DependencyObject obj, int value)
        {
            obj.SetValue(MinHeightProperty, value);
        }

        public static readonly DependencyProperty MinHeightProperty =
            DependencyProperty.RegisterAttached(
                "MinHeight",
                typeof(int),
                typeof(WindowMinSizeBehavior),
                new PropertyMetadata(600, OnMinHeightChanged));

        private static void OnMinWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && e.NewValue is int minWidth)
            {
                window.Loaded += (s, args) =>
                {
                    if (window.AppWindow != null)
                    {
                        var appWindow = window.AppWindow;
                        appWindow.ResizableWindowMinWidth = minWidth;
                    }
                };
            }
        }

        private static void OnMinHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && e.NewValue is int minHeight)
            {
                window.Loaded += (s, args) =>
                {
                    if (window.AppWindow != null)
                    {
                        var appWindow = window.AppWindow;
                        appWindow.ResizableWindowMinHeight = minHeight;
                    }
                };
            }
        }
    }
}
