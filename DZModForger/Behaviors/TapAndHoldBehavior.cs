using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior that executes command on tap and hold
    /// </summary>
    public static class TapAndHoldBehavior
    {
        public static ICommand GetTapAndHoldCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(TapAndHoldCommandProperty);
        }

        public static void SetTapAndHoldCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(TapAndHoldCommandProperty, value);
        }

        public static readonly DependencyProperty TapAndHoldCommandProperty =
            DependencyProperty.RegisterAttached(
                "TapAndHoldCommand",
                typeof(ICommand),
                typeof(TapAndHoldBehavior),
                new PropertyMetadata(null, OnTapAndHoldCommandChanged));

        public static int GetHoldDuration(DependencyObject obj)
        {
            return (int)obj.GetValue(HoldDurationProperty);
        }

        public static void SetHoldDuration(DependencyObject obj, int value)
        {
            obj.SetValue(HoldDurationProperty, value);
        }

        public static readonly DependencyProperty HoldDurationProperty =
            DependencyProperty.RegisterAttached(
                "HoldDuration",
                typeof(int),
                typeof(TapAndHoldBehavior),
                new PropertyMetadata(500));

        private static void OnTapAndHoldCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element && e.NewValue is ICommand command)
            {
                Stopwatch holdTimer = new Stopwatch();
                int holdDuration = GetHoldDuration(d);

                element.PointerPressed += async (s, args) =>
                {
                    holdTimer.Restart();
                    await Task.Delay(holdDuration);

                    if (holdTimer.ElapsedMilliseconds >= holdDuration)
                    {
                        if (command.CanExecute(null))
                        {
                            command.Execute(null);
                        }
                    }
                };

                element.PointerReleased += (s, args) =>
                {
                    holdTimer.Stop();
                };
            }
        }
    }

    public interface ICommand
    {
        void Execute(object parameter);
        bool CanExecute(object parameter);
    }
}
