using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior for handling drag and drop operations
    /// </summary>
    public static class DragAndDropBehavior
    {
        public static bool GetAcceptsDrop(DependencyObject obj)
        {
            return (bool)obj.GetValue(AcceptsDropProperty);
        }

        public static void SetAcceptsDrop(DependencyObject obj, bool value)
        {
            obj.SetValue(AcceptsDropProperty, value);
        }

        public static readonly DependencyProperty AcceptsDropProperty =
            DependencyProperty.RegisterAttached(
                "AcceptsDrop",
                typeof(bool),
                typeof(DragAndDropBehavior),
                new PropertyMetadata(false, OnAcceptsDropChanged));

        public static event EventHandler<DropEventArgs>? FilesDropped;

        private static void OnAcceptsDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element && (bool)e.NewValue)
            {
                element.AllowDrop = true;

                element.DragOver += (s, args) =>
                {
                    args.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
                    args.Handled = true;
                };

                element.Drop += async (s, args) =>
                {
                    var items = args.DataView.GetStorageItems();
                    await items.ContinueWith(async task =>
                    {
                        var storageItems = await task;
                        var filePaths = new System.Collections.Generic.List<string>();

                        foreach (var item in storageItems)
                        {
                            if (item is Windows.Storage.StorageFile file)
                            {
                                filePaths.Add(file.Path);
                            }
                        }

                        FilesDropped?.Invoke(element, new DropEventArgs { FilePaths = filePaths });
                    });
                };
            }
        }
    }

    public class DropEventArgs : EventArgs
    {
        public System.Collections.Generic.List<string> FilePaths { get; set; } = new();
    }
}
