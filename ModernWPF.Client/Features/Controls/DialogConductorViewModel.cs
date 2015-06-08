using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using Autofac.Core;
using Caliburn.Micro;
using ModernWPF.Client.Extensions;
using ModernWPF.Client.Features.Actions;
using Serilog;
using Serilog.Context;

namespace ModernWPF.Client.Features.Controls
{
    public interface IDialogConductor : IConductor
    {
        IDialogConductor ShowDialog<T>(params object[] args) where T : IScreenWithClose;
        IDialogConductor ShowDialog(Type type, params object[] args);
        IDialogConductor ShowDialog<TFactory, T>(Func<TFactory, T> factory, bool duplicateCheck = true) where T : IScreenWithClose;
        Task<TResult> ShowDialog<TView, TResult>(params object[] args) where TView : IReturnOnClose<TResult>;
        void WithoutClose();

        void TryCloseActiveItem();
        object GetActiveItem();
        void RefreshActiveItem(object[] args);
        bool HasActiveItem { get; }
    }

    public class DialogConductorViewModel : PropertyChangedBase,
        IDialogConductor
    {
        private readonly ICommand _tryCloseCommand;
        private readonly BindableCollection<IScreenWithClose> _activeItems = new BindableCollection<IScreenWithClose>();
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IEventAggregatorSubscriptionTracker _eventAggregatorSubscriptionTracker;
        private readonly IDictionary<object, ILifetimeScope> _dialogLifetimeScopes = new Dictionary<object, ILifetimeScope>();
        private bool _isBusy;
        private LastActivation _lastActivation;

        public DialogConductorViewModel(ILifetimeScope lifetimeScope, IEventAggregatorSubscriptionTracker eventAggregatorSubscriptionTracker)
        {
            _lifetimeScope = lifetimeScope;
            _eventAggregatorSubscriptionTracker = eventAggregatorSubscriptionTracker;
            _tryCloseCommand = new DelegateCommand(x => TryCloseActiveItem());

            ActiveItems.CollectionChanged += (s, e) =>
            {
                NotifyOfPropertyChange(() => ActiveItem);
                NotifyOfPropertyChange(() => HasActiveItem);
            };
        }

        public BindableCollection<IScreenWithClose> ActiveItems { get { return _activeItems; } }

        public IScreenWithClose ActiveItem { get { return _activeItems.Any() ? _activeItems.Last() : null; } }

        public bool HasActiveItem { get { return ActiveItems.Any(); } }

        public ICommand TryCloseCommand { get { return _tryCloseCommand; } }

        public event EventHandler<ActivationProcessedEventArgs> ActivationProcessed = delegate { };

        public bool IsBusy
        {
            get { return _isBusy; }
            private set
            {
                if (value.Equals(_isBusy)) return;
                _isBusy = value;
                NotifyOfPropertyChange(() => IsBusy);
            }
        }

        private bool IsDuplicateActivation(Type screenType)
        {
            var isDuplicateActivation =
                _lastActivation != null
                && _lastActivation.IsWithinDuplicateThresold
                && _lastActivation.LastActivatedScreenType == screenType;

            if (isDuplicateActivation == false)
            {
                _lastActivation = new LastActivation(screenType);
            }
            else
            {
                Log.Debug("Duplicate screen activation detected {screenType}", screenType.Name);
            }

            return isDuplicateActivation;
        }

        public IDialogConductor ShowDialog(Type type, params object[] args)
        {
            if (IsDuplicateActivation(type)) return this;

            var lifetimeScope = _lifetimeScope.BeginLifetimeScope(type);

            var target = lifetimeScope.Resolve(type, args.Select(ConvertParameter));

            _dialogLifetimeScopes.Add(target, lifetimeScope);

            ActivateItem(target);

            return this;
        }

        public IDialogConductor ShowDialog<T>(params object[] args) where T : IScreenWithClose
        {
            if (IsDuplicateActivation(typeof(T))) return this;

            var lifetimeScope = _lifetimeScope.BeginLifetimeScope(typeof(T));
            var target = lifetimeScope.Resolve<T>(args.Select(ConvertParameter));

            _dialogLifetimeScopes.Add(target, lifetimeScope);

            ActivateItem(target);

            return this;
        }

        public IDialogConductor ShowDialog<TFactory, T>(Func<TFactory, T> factoryCallback, bool duplicateCheck = true) where T : IScreenWithClose
        {
            if (duplicateCheck && IsDuplicateActivation(typeof(T))) return this;

            var lifetimeScope = _lifetimeScope.BeginLifetimeScope(typeof(T));
            var factory = lifetimeScope.Resolve<TFactory>();
            var target = factoryCallback(factory);

            _dialogLifetimeScopes.Add(target, lifetimeScope);

            ActivateItem(target);

            return this;
        }

        public async Task<TResult> ShowDialog<TView, TResult>(params object[] args) where TView : IReturnOnClose<TResult>
        {
            if (IsDuplicateActivation(typeof(TView))) return default(TResult);

            var lifetimeScope = _lifetimeScope.BeginLifetimeScope(typeof(TView));
            var target = lifetimeScope.Resolve<TView>(args.Select(ConvertParameter));

            _dialogLifetimeScopes.Add(target, lifetimeScope);

            var tcs = new TaskCompletionSource<TResult>();
            target.ResultProcessed += (s, r) => tcs.TrySetResult(r);

            ActivateItem(target);
            await tcs.Task;

            TryCloseActiveItem();
            return tcs.Task.Result;
        }

        public void WithoutClose()
        {
            CloseIsVisible = false;
        }

        public bool CloseIsVisible { get; private set; }

        private static ConstantParameter ConvertParameter(object a)
        {
            var p = a as NamedParameter;
            if (p != null) return p;

            var t = a as TypedParameter;
            if (t != null) return t;

            return new TypedParameter(a.GetType(), a);
        }

        public IEnumerable GetChildren()
        {
            return ActiveItems;
        }

        public async void ActivateItem(object item)
        {
            var child = item as IChild;
            if (child != null)
                child.Parent = this;

            ActiveItems.Add((IScreenWithClose)item);

            ((IScreenWithClose)item).Activate();

            ActivationProcessed(this, new ActivationProcessedEventArgs { Item = item, Success = true });
        }

        public void DeactivateItem(object item, bool close)
        {
            var guard = item as IGuardClose;
            if (guard != null)
            {
                guard.CanClose(result =>
                {
                    if (result)
                        CloseActiveItem(ActiveItems.Single(x => x == (IScreenWithClose)item));
                });
            }

            else CloseActiveItem(ActiveItems.Single(x => x == (IScreenWithClose)item));
        }

        private readonly object _closeLock = new object();
        private bool _isClosing;
        public void TryCloseActiveItem()
        {
            if (_isClosing) return;

            lock (_closeLock)
            {
                _isClosing = true;

                if (ActiveItems.None()) return;

                var item = ActiveItems.Last();
                var screen = item;

                var busyScreen = screen as IGetBusy;
                if (busyScreen != null && busyScreen.IsBusy)
                {
                    Task.Run(async () => await CloseWaitingItem(new WaitingToClose(item)));
                    _isClosing = false;
                    return;
                }

                screen.TryClose();

                var disposable = screen as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }

                _isClosing = false;
            }
        }

        public object GetActiveItem()
        {
            return ActiveItems.Any() ? ActiveItems.Last() : null;
        }

        public void RefreshActiveItem(object[] args)
        {
            if (ActiveItems.None()) return;

            var item = ActiveItems.Last();
            var activeItemType = item.GetType();

            CloseActiveItem(item);

            ShowDialog(activeItemType, args);
        }

        private void CloseActiveItem(IScreenWithClose item)
        {
            if (item == null) return;

            RemoveItem(item);
            DisposeItem(item);
        }

        private void DisposeItem(IScreenWithClose oldItem)
        {
            ILifetimeScope lifetimeScope;
            _dialogLifetimeScopes.TryGetValue(oldItem, out lifetimeScope);
            if (lifetimeScope != null)
            {
                _dialogLifetimeScopes.Remove(oldItem);

                _eventAggregatorSubscriptionTracker.EndScope(lifetimeScope);
                lifetimeScope.Dispose();
            }
        }

        private void RemoveItem(IScreenWithClose oldItem)
        {
            if (ActiveItems.Contains(oldItem) == false) return;

            oldItem.Deactivate(true);

            Task.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(300));
                ActiveItems.Remove(oldItem);
                NotifyOfPropertyChange(() => HasActiveItem);
            });
        }

        private async Task CloseWaitingItem(WaitingToClose item)
        {
            using (LogContext.PushProperty("OperationId", Guid.NewGuid()))
            {
                Log.Debug("Removing screen {screenType}", item.WaitingItem.GetType());
                RemoveItem(item.WaitingItem);

                while (true)
                {
                    if (item.IsStillBusy == false)
                    {
                        break;
                    }

                    Log.Debug("Waiting to dispose screen {0}...", item.WaitingItem.GetType());
                    item.RecordAttempt();
                    if (item.HasReachedAttemptThreshold)
                    {
                        Log.Error("Screen {0} is still busy after 60 seconds. It will now be forcibly closed. This may result in subsequent errors being reported.", item.WaitingItem.GetType());
                        break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                Log.Debug("Disposing screen {0}.", item.WaitingItem.GetType());
                DisposeItem(item.WaitingItem);
            }
        }

        private class WaitingToClose
        {
            private readonly IGetBusy _busyScreen;
            private int _closeAttempts;

            public WaitingToClose(IScreenWithClose waitingItem)
            {
                var busyScreen = waitingItem as IGetBusy;

                if (busyScreen == null)
                    throw new ArgumentException("screen");

                _busyScreen = busyScreen;
                WaitingItem = waitingItem;
            }

            public IScreenWithClose WaitingItem { get; private set; }
            public bool IsStillBusy { get { return _busyScreen.IsBusy; } }
            public bool HasReachedAttemptThreshold { get { return _closeAttempts == 60; } }

            public void RecordAttempt()
            {
                _closeAttempts++;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((WaitingToClose)obj);
            }

            protected bool Equals(WaitingToClose other)
            {
                return Equals(WaitingItem, other.WaitingItem);
            }

            public override int GetHashCode()
            {
                return (WaitingItem != null ? WaitingItem.GetHashCode() : 0);
            }
        }

        private class LastActivation
        {
            public LastActivation(Type lastActivatedScreenType)
            {
                LastActivatedScreenType = lastActivatedScreenType;
                LastActivationTime = DateTimeOffset.UtcNow;
            }

            public Type LastActivatedScreenType { get; private set; }
            public DateTimeOffset LastActivationTime { get; private set; }
            public bool IsWithinDuplicateThresold { get { return DateTimeOffset.UtcNow - LastActivationTime <= TimeSpan.FromSeconds(2); } }
        }
    }
}