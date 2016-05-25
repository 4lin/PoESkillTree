﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using POESKillTree.Model;

namespace POESKillTree.ViewModels
{
    /// <summary>
    /// Abstract class for ViewModels that can be closed.
    /// </summary>
    public abstract class CloseableViewModel : ViewModelBase
    {
        private readonly RelayCommand _closeCommand;
        /// <summary>
        /// Gets the command that closes this ViewModel.
        /// </summary>
        public ICommand CloseCommand
        {
            get { return _closeCommand; }
        }

        /// <summary>
        /// Raised when <see cref="CloseCommand"/> is executed.
        /// </summary>
        public event Action RequestsClose;

        private readonly TaskCompletionSource<object> _closeCompletionSource =
            new TaskCompletionSource<object>();

        protected CloseableViewModel()
        {
            _closeCommand = new RelayCommand(x =>
            {
                if (RequestsClose != null)
                    RequestsClose();
                _closeCompletionSource.TrySetResult(null);
            });
        }

        /// <summary>
        /// Returns a task that completes once <see cref="CloseCommand"/> is executed
        /// and all handlers for <see cref="RequestsClose"/> returned.
        /// </summary>
        public Task WaitForCloseAsync()
        {
            return _closeCompletionSource.Task;
        }
    }
}