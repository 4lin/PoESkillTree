﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Linq;
using POESKillTree.Controls;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Model.Items;

namespace POESKillTree.ViewModels
{
    public static class DialogCoordinatorExtensions
    {
        public static Task<MessageDialogResult> ShowErrorMessageAsync(this IDialogCoordinator dialogCoordinator, object context, string message, string details = null)
        {
            return dialogCoordinator.ShowMessageAsync(context, L10n.Message("Error"), Format(message, details));
        }

        public static Task<MessageDialogResult> ShowWarningMessageAsync(this IDialogCoordinator dialogCoordinator, object context, string message, string details = null)
        {
            return dialogCoordinator.ShowMessageAsync(context, L10n.Message("Warning"), Format(message, details));
        }

        private static string Format(string message, string details)
        {
            if (!string.IsNullOrEmpty(details))
                message += "\n\n" + L10n.Message("Details:") + "\n" + details;

            return message;
        }
    }

    public class DownloadStashViewModel : CloseableViewModel
    {
        private readonly Stash _stash;
        private readonly IDialogCoordinator _dialogCoordinator;

        private PoEBuild _build;
        public PoEBuild Build
        {
            get { return _build; }
            private set { SetProperty(ref _build, value); }
        }

        private string _tabsLink;
        public string TabsLink
        {
            get { return _tabsLink; }
            private set { SetProperty(ref _tabsLink, value);}
        }

        private string _tabLink;
        public string TabLink
        {
            get { return _tabLink; }
            private set { SetProperty(ref _tabLink, value); }
        }

        private readonly List<StashBookmark> _tabs = new List<StashBookmark>();

        public ICollectionView TabsView { get; private set; }

        public IReadOnlyList<string> CurrentLeagues { get; private set; }

        private RelayCommand<string> _openInBrowserCommand;
        public ICommand OpenInBrowserCommand
        {
            get
            {
                return _openInBrowserCommand ??
                       (_openInBrowserCommand = new RelayCommand<string>(param => Process.Start(param)));
            }
        }

        private RelayCommand _loadTabsCommand;
        public ICommand LoadTabsCommand
        {
            get { return _loadTabsCommand ?? (_loadTabsCommand = new RelayCommand(o => LoadTabs())); }
        }

        private RelayCommand _loadTabContentsCommand;
        public ICommand LoadTabContentsCommand
        {
            get { return _loadTabContentsCommand ?? (_loadTabContentsCommand = new RelayCommand(o => LoadTabContents())); }
        }

        public DownloadStashViewModel(PoEBuild build, Stash stash, IDialogCoordinator dialogCoordinator)
        {
            _stash = stash;
            _dialogCoordinator = dialogCoordinator;
            CurrentLeagues = LoadCurrentLeagues();
            DisplayName = L10n.Message("Download & Import Stash");

            TabsView = new ListCollectionView(_tabs);
            TabsView.CurrentChanged += (sender, args) => UpdateTabLink();

            Build = build;
            Build.PropertyChanged += BuildOnPropertyChanged;
            BuildOnPropertyChanged(this, null);
            RequestsClose += () => Build.PropertyChanged -= BuildOnPropertyChanged;
        }

        private IReadOnlyList<string> LoadCurrentLeagues()
        {
            string file;
            using (var client = new WebClient())
            {
                file = client.DownloadString("http://api.pathofexile.com/leagues?type=main&compact=1");
            }
            try
            {
                return JArray.Parse(file).Select(t => t["id"].Value<string>()).ToList();
            }
            catch (Exception e)
            {
                _dialogCoordinator.ShowWarningMessageAsync(this,
                    L10n.Message("Could not load the currently running leagues."), e.Message);
                return new List<string>();
            }
        }

        private void BuildOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            TabsLink =
                string.Format(
                    "https://www.pathofexile.com/character-window/get-stash-items?tabs=1&tabIndex=0&league={0}&accountName={1}",
                    Build.League, Build.AccountName);
            UpdateTabLink();
        }

        private void UpdateTabLink()
        {
            var selectedTab = TabsView.CurrentItem as StashBookmark;
            if (selectedTab == null)
            {
                TabLink = "";
                return;
            }
            TabLink =
                string.Format(
                    "https://www.pathofexile.com/character-window/get-stash-items?tabs=0&tabIndex={2}&league={0}&accountName={1}",
                    Build.League, Build.AccountName, selectedTab.Position);
        }

        private void LoadTabs()
        {
            var stashData = Clipboard.GetText();
            _tabs.Clear();
            try
            {
                var tabs = (JArray)JObject.Parse(stashData)["tabs"];
                foreach (var tab in tabs)
                {
                    if (tab["hidden"].Value<bool>())
                        continue;
                    var name = tab["n"].Value<string>();
                    var index = tab["i"].Value<int>();
                    var c = tab["colour"].Value<JObject>();
                    var color = Color.FromArgb(0xFF, c["r"].Value<byte>(), c["g"].Value<byte>(), c["b"].Value<byte>());
                    _tabs.Add(new StashBookmark(name, index, color));
                }
            }
            catch (Exception e)
            {
                _dialogCoordinator.ShowErrorMessageAsync(this,
                    L10n.Message("An error occurred while attempting to load stash data."), e.Message);
            }
            TabsView.Refresh();
            TabsView.MoveCurrentToFirst();
        }

        private async void LoadTabContents()
        {
            var tabContents = Clipboard.GetText();
            IntRange highlightrange = null;
            try
            {
                var json = JObject.Parse(tabContents);
                var items = json["items"].Select(i => new Item((JObject)i)).ToArray();

                var yStart = _stash.LastOccupiedLine + 3;

                var selectedBookmark = TabsView.CurrentItem as StashBookmark;
                var sb = selectedBookmark != null
                    ? new StashBookmark(selectedBookmark.Name, yStart, selectedBookmark.Color)
                    : new StashBookmark("imported", yStart);

                _stash.BeginUpdate();
                _stash.AddBookmark(sb);

                var yOffsetInImported = items.Min(i => i.Y);
                var yEnd = yStart;
                foreach (var item in items)
                {
                    item.Y += yStart - yOffsetInImported;
                    yEnd = Math.Max(yEnd, item.Y + item.Height);
                    if (item.X + item.Width > 12)
                    {
                        await _dialogCoordinator.ShowWarningMessageAsync(this, "Skipping item because it is too wide.");
                        continue;
                    }
                    _stash.Items.Add(item);
                }

                await _dialogCoordinator.ShowMessageAsync(this, L10n.Message("New tab added"),
                    string.Format(L10n.Message("New tab with {0} items was added to stash."), items.Length));
                highlightrange = new IntRange { From = yStart, Range = yEnd - yStart };
            }
            catch (Exception e)
            {
                _dialogCoordinator.ShowErrorMessageAsync(this,
                    L10n.Message("An error occurred while attempting to load stash data."), e.Message);
            }
            finally
            {
                _stash.EndUpdate();
            }

            if (highlightrange != null)
                _stash.AddHighlightRange(highlightrange);
        }
    }
}