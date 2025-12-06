using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Windows.Input;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior for passing custom command parameter
    /// </summary>
    public static class CommandParameterBehavior
    {
        public static object GetCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(CommandParameterProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "CommandParameter",
                typeof(object),
                typeof(CommandParameterBehavior),
                new PropertyMetadata(null, OnCommandParameterChanged));

        private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ButtonBase button && button.Command is ICommand command)
            {
                button.Click += (s, args) =>
                {
                    if (command.CanExecute(e.NewValue))
                    {
                        command.Execute(e.NewValue);
                    }
                };
            }
        }
    }
}
