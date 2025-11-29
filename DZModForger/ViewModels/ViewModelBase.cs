using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DZModForger.ViewModels
{
    /// <summary>
    /// Base class for all ViewModels in DZModForger
    /// Implements INotifyPropertyChanged for MVVM data binding
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// ViewModel display name for debugging and logging
        /// </summary>
        protected string DisplayName { get; set; }

        protected ViewModelBase()
        {
            DisplayName = GetType().Name;
            Debug.WriteLine($"[{DisplayName}] ViewModel created");
        }

        /// <summary>
        /// Raises PropertyChanged event for a specific property
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                Debug.WriteLine($"[{DisplayName}] PropertyChanged: propertyName is null");
                return;
            }

            Debug.WriteLine($"[{DisplayName}] PropertyChanged: {propertyName}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets a property value and raises PropertyChanged if value changed
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="field">Reference to the backing field</param>
        /// <param name="value">New value</param>
        /// <param name="propertyName">Name of the property (auto-captured)</param>
        /// <returns>True if value was changed, false otherwise</returns>
        protected bool SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName] string propertyName = null)
        {
            // Check if value actually changed
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            // Update field
            field = value;

            // Raise PropertyChanged event
            OnPropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Sets a property value and raises PropertyChanged for multiple properties
        /// Useful when setting one property affects multiple UI elements
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="field">Reference to the backing field</param>
        /// <param name="value">New value</param>
        /// <param name="propertyNames">Names of properties to notify about</param>
        /// <returns>True if value was changed, false otherwise</returns>
        protected bool SetProperty<T>(
            ref T field,
            T value,
            params string[] propertyNames)
        {
            // Check if value actually changed
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            // Update field
            field = value;

            // Raise PropertyChanged for all specified properties
            foreach (var propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }

            return true;
        }

        /// <summary>
        /// Executes an action with error handling and logging
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="actionName">Descriptive name for logging</param>
        protected void ExecuteAction(Action action, string actionName = "Action")
        {
            try
            {
                Debug.WriteLine($"[{DisplayName}] Executing: {actionName}");
                action?.Invoke();
                Debug.WriteLine($"[{DisplayName}] Completed: {actionName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DisplayName}] Exception in {actionName}: {ex.Message}");
                Debug.WriteLine($"[{DisplayName}] Stack trace: {ex.StackTrace}");
                OnActionError(actionName, ex);
            }
        }

        /// <summary>
        /// Executes an action with a parameter and error handling
        /// </summary>
        /// <typeparam name="T">Type of the parameter</typeparam>
        /// <param name="action">Action to execute</param>
        /// <param name="parameter">Parameter to pass</param>
        /// <param name="actionName">Descriptive name for logging</param>
        protected void ExecuteAction<T>(Action<T> action, T parameter, string actionName = "Action")
        {
            try
            {
                Debug.WriteLine($"[{DisplayName}] Executing: {actionName}");
                action?.Invoke(parameter);
                Debug.WriteLine($"[{DisplayName}] Completed: {actionName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DisplayName}] Exception in {actionName}: {ex.Message}");
                Debug.WriteLine($"[{DisplayName}] Stack trace: {ex.StackTrace}");
                OnActionError(actionName, ex);
            }
        }

        /// <summary>
        /// Executes a function with error handling and returns the result
        /// </summary>
        /// <typeparam name="TResult">Type of the return value</typeparam>
        /// <param name="func">Function to execute</param>
        /// <param name="functionName">Descriptive name for logging</param>
        /// <returns>Result of function execution or default if exception occurred</returns>
        protected TResult ExecuteFunction<TResult>(
            Func<TResult> func,
            string functionName = "Function")
        {
            try
            {
                Debug.WriteLine($"[{DisplayName}] Executing: {functionName}");
                var result = func.Invoke();
                Debug.WriteLine($"[{DisplayName}] Completed: {functionName}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DisplayName}] Exception in {functionName}: {ex.Message}");
                Debug.WriteLine($"[{DisplayName}] Stack trace: {ex.StackTrace}");
                OnFunctionError(functionName, ex);
                return default;
            }
        }

        /// <summary>
        /// Executes a function with a parameter and error handling
        /// </summary>
        /// <typeparam name="TParameter">Type of the parameter</typeparam>
        /// <typeparam name="TResult">Type of the return value</typeparam>
        /// <param name="func">Function to execute</param>
        /// <param name="parameter">Parameter to pass</param>
        /// <param name="functionName">Descriptive name for logging</param>
        /// <returns>Result of function execution or default if exception occurred</returns>
        protected TResult ExecuteFunction<TParameter, TResult>(
            Func<TParameter, TResult> func,
            TParameter parameter,
            string functionName = "Function")
        {
            try
            {
                Debug.WriteLine($"[{DisplayName}] Executing: {functionName}");
                var result = func.Invoke(parameter);
                Debug.WriteLine($"[{DisplayName}] Completed: {functionName}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DisplayName}] Exception in {functionName}: {ex.Message}");
                Debug.WriteLine($"[{DisplayName}] Stack trace: {ex.StackTrace}");
                OnFunctionError(functionName, ex);
                return default;
            }
        }

        /// <summary>
        /// Called when an action throws an exception
        /// Override to handle action errors
        /// </summary>
        /// <param name="actionName">Name of the action</param>
        /// <param name="ex">Exception that was thrown</param>
        protected virtual void OnActionError(string actionName, Exception ex)
        {
            // Default implementation: just log the error
            Debug.WriteLine($"[{DisplayName}] Action error in {actionName}: {ex.Message}");
        }

        /// <summary>
        /// Called when a function throws an exception
        /// Override to handle function errors
        /// </summary>
        /// <param name="functionName">Name of the function</param>
        /// <param name="ex">Exception that was thrown</param>
        protected virtual void OnFunctionError(string functionName, Exception ex)
        {
            // Default implementation: just log the error
            Debug.WriteLine($"[{DisplayName}] Function error in {functionName}: {ex.Message}");
        }

        /// <summary>
        /// Logs a message with the ViewModel's display name
        /// </summary>
        /// <param name="message">Message to log</param>
        protected void Log(string message)
        {
            Debug.WriteLine($"[{DisplayName}] {message}");
        }

        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Format arguments</param>
        protected void LogDebug(string format, params object[] args)
        {
            Debug.WriteLine($"[{DisplayName}] {string.Format(format, args)}");
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Format arguments</param>
        protected void LogWarning(string format, params object[] args)
        {
            Debug.WriteLine($"[{DisplayName}] WARNING: {string.Format(format, args)}");
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Format arguments</param>
        protected void LogError(string format, params object[] args)
        {
            Debug.WriteLine($"[{DisplayName}] ERROR: {string.Format(format, args)}");
        }

        /// <summary>
        /// Verifies that a property name is valid for this ViewModel type
        /// Helps catch typos in property names at development time
        /// </summary>
        /// <param name="propertyName">Name of the property to verify</param>
        /// <returns>True if property exists, false otherwise</returns>
        [Conditional("DEBUG")]
        protected void VerifyPropertyName(string propertyName)
        {
            var type = GetType();
            var property = type.GetProperty(propertyName);

            if (property == null)
            {
                Debug.WriteLine($"[{DisplayName}] WARNING: Invalid property name '{propertyName}' for type '{type.Name}'");
            }
        }

        /// <summary>
        /// Cleanup method - override to dispose resources
        /// </summary>
        public virtual void Cleanup()
        {
            Debug.WriteLine($"[{DisplayName}] Cleanup called");
        }

        /// <summary>
        /// Destructor for cleanup
        /// </summary>
        ~ViewModelBase()
        {
            Debug.WriteLine($"[{DisplayName}] ViewModel destroyed");
        }
    }

    /// <summary>
    /// Observable collection wrapper with change notifications
    /// </summary>
    public class NotifyCollectionChangedBehavior
    {
        public static void OnCollectionChanged(
            string displayName,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                Debug.WriteLine($"[{displayName}] Collection: Added {e.NewItems?.Count} items");
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                Debug.WriteLine($"[{displayName}] Collection: Removed {e.OldItems?.Count} items");
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                Debug.WriteLine($"[{displayName}] Collection: Replaced items");
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                Debug.WriteLine($"[{displayName}] Collection: Moved items");
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                Debug.WriteLine($"[{displayName}] Collection: Cleared");
            }
        }
    }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Editor modes for viewport interaction
    /// </summary>
    public enum EditorMode
    {
        Object,
        Edit,
        Sculpt,
        Animation
    }

    /// <summary>
    /// Viewport shading modes
    /// </summary>
    public enum ShadeMode
    {
        Wireframe,
        Solid,
        Material,
        Render
    }
}
