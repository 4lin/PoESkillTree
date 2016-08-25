﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using POESKillTree.Common.ViewModels;
using POESKillTree.Utils;

namespace POESKillTree.Controls.Dialogs.ViewModels
{
    public class FileSelectorViewModel : ErrorInfoViewModel<string>
    {
        private string _filePath;
        private string _sanitizedFilePath;
        private readonly bool _isFolderPicker;
        private readonly string _validationSubPath;
        private readonly Func<string, string> _additionalValidationFunc;

        public string Message { get; }

        public string FilePath
        {
            get { return _filePath; }
            set { SetProperty(ref _filePath, value); }
        }

        public string SanitizedFilePath
        {
            get { return _sanitizedFilePath; }
            set { SetProperty(ref _sanitizedFilePath, value); }
        }

        public ICommand SelectFileCommand { get; }

        public bool IsCancelable { get; }

        public FileSelectorViewModel(string title, string message, FileSelectorDialogSettings settings)
        {
            if (!settings.IsFolderPicker && !string.IsNullOrEmpty(settings.ValidationSubPath))
                throw new ArgumentException("ValidationSubPath may only be given if IsFolderPicker is true",
                    nameof(settings));
            DisplayName = title;
            Message = message;
            IsCancelable = settings.IsCancelable;
            _isFolderPicker = settings.IsFolderPicker;
            _validationSubPath = settings.ValidationSubPath;
            _additionalValidationFunc = settings.AdditionalValidationFunc;
            FilePath = settings.DefaultPath;
            SelectFileCommand = new RelayCommand(SelectFile);
        }

        private void SelectFile()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = _isFolderPicker,
                InitialDirectory = Path.GetDirectoryName(SanitizedFilePath),
                DefaultFileName = Path.GetFileName(SanitizedFilePath)
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FilePath = dialog.FileName;
            }
        }

        protected override IEnumerable<string> ValidateProperty(string propertyName)
        {
            if (propertyName != nameof(FilePath))
                return null;
            string message;
            var trimmed = PathEx.TrimTrailingDirectorySeparators(FilePath);
            if (PathEx.IsPathValid(trimmed, out message, mustBeDirectory: _isFolderPicker, mustBeFile: !_isFolderPicker))
            {
                if (!string.IsNullOrEmpty(_validationSubPath))
                {
                    PathEx.IsPathValid(Path.Combine(trimmed, _validationSubPath), out message);
                }
                if (message == null)
                {
                    message = _additionalValidationFunc(trimmed);
                }
                SanitizedFilePath = trimmed;
            }
            return new[] {message};
        }
    }
}