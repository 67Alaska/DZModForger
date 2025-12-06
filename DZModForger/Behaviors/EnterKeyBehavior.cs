using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior that executes command when Enter key is pressed
    /// </summary>
    public static class EnterKeyBehavior
    {
        public static ICommand GetEnterCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(EnterCommandProperty);
        }

        public static void SetEnterCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(EnterCommandProperty, value);
        }

        public static readonly DependencyProperty EnterCommandProperty =
            DependencyProperty.RegisterAttached(
                "EnterCommand",
                typeof(ICommand),
                typeof(EnterKeyBehavior),
                new PropertyMetadata(null, OnEnterCommandChanged));

        private static void OnEnterCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox && e.NewValue is ICommand command)
            {
                textBox.KeyDown += (s, args) =>
                {
                    if (args.Key == Windows.System.VirtualKey.Enter)
                    {
                        if (command.CanExecute(textBox.Text))
                        {
                            command.Execute(textBox.Text);
                            args.Handled = true;
                        }
                    }
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
