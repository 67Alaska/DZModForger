using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using System;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior for clipboard operations (copy/paste)
    /// </summary>
    public static class ClipboardBehavior
    {
        public static bool GetCopyOnClick(DependencyObject obj)
        {
            return (bool)obj.GetValue(CopyOnClickProperty);
        }

        public static void SetCopyOnClick(DependencyObject obj, bool value)
        {
            obj.SetValue(CopyOnClickProperty, value);
        }

        public static readonly DependencyProperty CopyOnClickProperty =
            DependencyProperty.RegisterAttached(
                "CopyOnClick",
                typeof(bool),
                typeof(ClipboardBehavior),
                new PropertyMetadata(false, OnCopyOnClickChanged));

        public static string GetCopyText(DependencyObject obj)
        {
            return (string)obj.GetValue(CopyTextProperty);
        }

        public static void SetCopyText(DependencyObject obj, string value)
        {
            obj.SetValue(CopyTextProperty, value);
        }

        public static readonly DependencyProperty CopyTextProperty =
            DependencyProperty.RegisterAttached(
                "CopyText",
                typeof(string),
                typeof(ClipboardBehavior),
                new PropertyMetadata("", OnCopyTextChanged));

        private static void OnCopyOnClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button button && (bool)e.NewValue)
            {
                button.Click += async (s, args) =>
                {
                    string copyText = GetCopyText(d);
                    if (!string.IsNullOrEmpty(copyText))
                    {
                        var dataPackage = new DataPackage();
                        dataPackage.SetText(copyText);
                        Clipboard.SetContent(dataPackage);
                    }
                };
            }
        }

        private static void OnCopyTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Copy text updated
        }
    }
}
