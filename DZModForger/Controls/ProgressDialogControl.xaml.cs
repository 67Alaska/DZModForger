using Microsoft.UI.Xaml.Controls;
using System;

namespace DZModForger.Controls
{
    public sealed partial class ProgressDialogControl : ContentDialog
    {
        public event EventHandler? CancelRequested;

        private bool _isClosed = false;

        public ProgressDialogControl()
        {
            this.InitializeComponent();
            this.PrimaryButtonClick += ProgressDialogControl_PrimaryButtonClick;
        }

        /// <summary>
        /// Update progress
        /// </summary>
        public void UpdateProgress(string message, int percentage)
        {
            StatusMessage.Text = message;
            ProgressValue.Value = Math.Clamp(percentage, 0, 100);
            ProgressText.Text = $"{percentage}%";
        }

        /// <summary>
        /// Set to complete
        /// </summary>
        public void SetComplete(string message = "Complete!")
        {
            StatusMessage.Text = message;
            ProgressValue.Value = 100;
            ProgressText.Text = "100%";
            IsPrimaryButtonEnabled = false;
        }

        /// <summary>
        /// Set to error
        /// </summary>
        public void SetError(string errorMessage)
        {
            StatusMessage.Text = $"Error: {errorMessage}";
            ProgressValue.Value = 100;
            ProgressText.Text = "Error";
            IsPrimaryButtonEnabled = true;
        }

        private void ProgressDialogControl_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        public new async void Hide()
        {
            if (!_isClosed)
            {
                _isClosed = true;
                await this.HideAsync();
            }
        }
    }
}
