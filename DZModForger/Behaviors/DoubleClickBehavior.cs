using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior that executes a command on double-click
    /// </summary>
    public static class DoubleClickBehavior
    {
        public static ICommand GetDoubleClickCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(DoubleClickCommandProperty);
        }

        public static void SetDoubleClickCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(DoubleClickCommandProperty, value);
        }

        public static readonly DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.RegisterAttached(
                "DoubleClickCommand",
                typeof(ICommand),
                typeof(DoubleClickBehavior),
                new PropertyMetadata(null, OnDoubleClickCommandChanged));

        private static void OnDoubleClickCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if (e.NewValue is ICommand command)
                {
                    var stopwatch = new Stopwatch();
                    const int DOUBLE_CLICK_DELAY = 300;

                    element.PointerPressed += (s, args) =>
                    {
                        if (stopwatch.ElapsedMilliseconds <= DOUBLE_CLICK_DELAY)
                        {
                            if (command.CanExecute(null))
                            {
                                command.Execute(null);
                            }
                            stopwatch.Stop();
                            stopwatch.Reset();
                        }
                        else
                        {
                            stopwatch.Restart();
                        }
                    };
                }
            }
        }
    }

    public interface ICommand
    {
        void Execute(object parameter);
        bool CanExecute(object parameter);
    }
}
