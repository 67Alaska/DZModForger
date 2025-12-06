using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior that applies validation styling to controls
    /// </summary>
    public static class ValidationBehavior
    {
        public static bool GetIsValidating(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsValidatingProperty);
        }

        public static void SetIsValidating(DependencyObject obj, bool value)
        {
            obj.SetValue(IsValidatingProperty, value);
        }

        public static readonly DependencyProperty IsValidatingProperty =
            DependencyProperty.RegisterAttached(
                "IsValidating",
                typeof(bool),
                typeof(ValidationBehavior),
                new PropertyMetadata(false, OnIsValidatingChanged));

        public static bool GetIsValid(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsValidProperty);
        }

        public static void SetIsValid(DependencyObject obj, bool value)
        {
            obj.SetValue(IsValidProperty, value);
        }

        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.RegisterAttached(
                "IsValid",
                typeof(bool),
                typeof(ValidationBehavior),
                new PropertyMetadata(true, OnIsValidChanged));

        public static string GetErrorMessage(DependencyObject obj)
        {
            return (string)obj.GetValue(ErrorMessageProperty);
        }

        public static void SetErrorMessage(DependencyObject obj, string value)
        {
            obj.SetValue(ErrorMessageProperty, value);
        }

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.RegisterAttached(
                "ErrorMessage",
                typeof(string),
                typeof(ValidationBehavior),
                new PropertyMetadata("", OnErrorMessageChanged));

        private static void OnIsValidatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Validation state changed
        }

        private static void OnIsValidChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control)
            {
                bool isValid = (bool)e.NewValue;

                if (isValid)
                {
                    VisualStateManager.GoToState(control, "Valid", true);
                }
                else
                {
                    VisualStateManager.GoToState(control, "Invalid", true);
                }
            }
        }

        private static void OnErrorMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Error message updated - could be displayed in tooltip
        }
    }
}
