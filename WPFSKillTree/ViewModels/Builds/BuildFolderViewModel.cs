using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using POESKillTree.Model.Builds;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.ViewModels.Builds
{
    public class BuildFolderViewModel : AbstractBuildViewModel<BuildFolder>, IBuildFolderViewModel
    {
        private readonly Action<IBuildFolderViewModel> _collectionChangedCallback;

        public ObservableCollection<IBuildViewModel> Children { get; } =
            new ObservableCollection<IBuildViewModel>();

        private bool _isInCollectionChanged;

        public BuildFolderViewModel(BuildFolder buildFolder, Predicate<IBuildViewModel> filterPredicate,
            Action<IBuildFolderViewModel> collectionChangedCallback)
            : base(buildFolder, filterPredicate)
        {
            _collectionChangedCallback = collectionChangedCallback;
            CreateChildren();

            Build.PropertyChanging += (sender, args) =>
            {
                if (args.PropertyName == nameof(BuildFolder.Builds))
                {
                    Build.Builds.CollectionChanged -= BuildsOnCollectionChanged;
                }
            };
            Build.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(BuildFolder.Builds))
                {
                    Build.Builds.CollectionChanged += BuildsOnCollectionChanged;
                    RecreateChildren();
                }
            };
            Build.Builds.CollectionChanged += BuildsOnCollectionChanged;
        }

        public override void ApplyFilter()
        {
            Children.ForEach(c => c.ApplyFilter());
        }

        private void CreateChildren()
        {
            foreach (var build in Build.Builds)
            {
                if (build is PoEBuild)
                    Children.Add(new BuildViewModel((PoEBuild)build, FilterPredicate));
                else if (build is BuildFolder)
                    Children.Add(new BuildFolderViewModel((BuildFolder)build, FilterPredicate, _collectionChangedCallback));
                else
                    throw new InvalidOperationException($"Unknown IBuild implementation {build.GetType()}");
            }
            Children.CollectionChanged += ChildrenOnCollectionChanged;
            SetParents();
        }

        private void RecreateChildren()
        {
            Children.CollectionChanged -= ChildrenOnCollectionChanged;
            UnsetParents();
            Children.Clear();
            CreateChildren();
        }

        private void UnsetParents()
        {
            UnsetParents(Children);
        }
        private void UnsetParents(IEnumerable<IBuildViewModel> builds)
        {
            builds.ForEach(UnsetParent);
        }
        private void UnsetParent(IBuildViewModel build)
        {
            build.Parent = null;
        }

        private void SetParents()
        {
            SetParents(Children);
        }
        private void SetParents(IEnumerable<IBuildViewModel> builds)
        {
            builds.ForEach(SetParent);
        }
        private void SetParent(IBuildViewModel build)
        {
            build.Parent = this;
        }

        private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (_isInCollectionChanged)
            {
                _collectionChangedCallback(this);
                return;
            }
            _isInCollectionChanged = true;
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                SetParents();
                Build.Builds.Clear();
                Children.Select(c => c.Build).ForEach(b => Build.Builds.Add(b));
            }
            else
            {
                CollectionChanged<IBuildViewModel, IBuild>(notifyCollectionChangedEventArgs, Build.Builds, b => b.Build);
            }
            _isInCollectionChanged = false;
            _collectionChangedCallback(this);
        }
        
        private void BuildsOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (_isInCollectionChanged)
                return;
            _isInCollectionChanged = true;
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                RecreateChildren();
            }
            else
            {
                CollectionChanged<IBuild, IBuildViewModel>(notifyCollectionChangedEventArgs, Children, GetChildForBuild);
            }
            _isInCollectionChanged = false;
        }

        private IBuildViewModel GetChildForBuild(IBuild build)
        {
            var ret = Children.FirstOrDefault(vm => vm.Build == build);
            if (ret != null) return ret;

            if (build is PoEBuild)
                return new BuildViewModel((PoEBuild)build, FilterPredicate);
            if (build is BuildFolder)
                return new BuildFolderViewModel((BuildFolder)build, FilterPredicate, _collectionChangedCallback);
            return null;
        }

        private void CollectionChanged<TSource, TTarget>(
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs,
            ObservableCollection<TTarget> targetCollection, Func<TSource, TTarget> toTargetFunc)
        {
            Func<TSource, TTarget, IBuildViewModel> toVmFunc;
            if (typeof(TSource) == typeof(IBuildViewModel))
                toVmFunc = (s, t) => (IBuildViewModel) s;
            else if (typeof(TTarget) == typeof(IBuildViewModel))
                toVmFunc = (s, t) => (IBuildViewModel) t;
            else
                throw new ArgumentException("One type parameter must be IBuildViewModel");
            var news = (notifyCollectionChangedEventArgs.NewItems?.Cast<TSource>() ?? new TSource[0]).Select(b =>
            {
                var target = toTargetFunc(b);
                return new { Target = target, ViewModel = toVmFunc(b, target)};
            });
            var olds = (notifyCollectionChangedEventArgs.OldItems?.Cast<TSource>() ?? new TSource[0]).Select(b =>
            {
                var target = toTargetFunc(b);
                return new { Target = target, ViewModel = toVmFunc(b, target) };
            });

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var i = notifyCollectionChangedEventArgs.NewStartingIndex;
                    foreach (var n in news)
                    {
                        SetParent(n.ViewModel);
                        if (i < 0)
                        {
                            targetCollection.Add(n.Target);
                        }
                        else
                        {
                            targetCollection.Insert(i, n.Target);
                            i++;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var o in olds)
                    {
                        UnsetParent(o.ViewModel);
                        targetCollection.Remove(o.Target);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (notifyCollectionChangedEventArgs.NewStartingIndex < 0)
                    {
                        foreach (var o in olds)
                        {
                            UnsetParent(o.ViewModel);
                            targetCollection.Remove(o.Target);
                        }
                        foreach (var n in news)
                        {
                            SetParent(n.ViewModel);
                            targetCollection.Add(n.Target);
                        }
                    }
                    else
                    {
                        var start = notifyCollectionChangedEventArgs.NewStartingIndex;
                        var oldList = olds.ToList();
                        var newList = news.ToList();
                        var minCount = Math.Min(newList.Count, oldList.Count);
                        for (var j = 0; j < minCount; j++)
                        {
                            UnsetParent(oldList[j].ViewModel);
                            SetParent(newList[j].ViewModel);
                            targetCollection.RemoveAt(start + j);
                            targetCollection.Insert(start + j, newList[j].Target);
                        }
                        for (var j = minCount; j < oldList.Count; j++)
                        {
                            UnsetParent(oldList[j].ViewModel);
                            targetCollection.Remove(oldList[j].Target);
                        }
                        for (var j = minCount; j < newList.Count; j++)
                        {
                            SetParent(newList[j].ViewModel);
                            targetCollection.Insert(start + j, newList[j].Target);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    foreach (var o in olds)
                    {
                        targetCollection.Remove(o.Target);
                    }
                    i = notifyCollectionChangedEventArgs.NewStartingIndex;
                    foreach (var n in news)
                    {
                        targetCollection.Insert(i, n.Target);
                        i++;
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}