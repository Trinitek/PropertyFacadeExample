using PropertyFacadeExample.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PropertyFacadeExample.ViewModel
{
    [DebuggerDisplay("(PropertyFacade) {Value}")]
    public class ReadOnlyPropertyFacade<T> : IDisposable
    {
        private readonly UXViewModel _viewModel;
        private readonly string _propertyName;
        protected FacadeValueCache<T> _valueCache;
        private IDisposable _subscription;
        private T _oldValue;

        public bool IsDisposed { get; private set; }

        public T Value => _valueCache.HasObservable ? _valueCache.GetValue() : default;

        public T OriginalValue { get; protected set; }

        public IReadOnlyValueObservable<bool> HasChanges { get; }
        private readonly BehaviorSubject<bool> _hasChangesSubject = new BehaviorSubject<bool>(false);

        public bool SuppressDebugOutput { get; set; }

        public ReadOnlyPropertyFacade(UXViewModel viewModel, string propertyName)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _propertyName = propertyName;
            _valueCache = new FacadeValueCache<T>();

            HasChanges = _hasChangesSubject.ToCached();
        }

        public ReadOnlyPropertyFacade(UXViewModel viewModel, IReadOnlyValueObservable<T> observable, string propertyName)
            : this(viewModel, propertyName)
        {
            Observe(observable);
        }

        /// <summary>
        /// Attach the facade to an observable. If an observable is already attached, the subscription is disposed and the new one is attached in its place.
        /// </summary>
        /// <param name="newObservable"></param>
        /// <param name="equalityComparer"></param>
        public void Observe(IObservable<T> newObservable, IEqualityComparer<T> equalityComparer = null)
        {
            if (newObservable is null)
            {
                throw new ArgumentNullException(nameof(newObservable));
            }

            Dispose();

            _valueCache.Attach(newObservable);

            OriginalValue = _valueCache.GetValue();

            DebugMessage("PropertyFacade: New observable mounted on viewmodel='{0}', property='{1}', OriginalValue='{2}'", _viewModel, _propertyName, OriginalValue);

            equalityComparer = equalityComparer ?? GetEqualityComparer<T>();

            IsDisposed = false;

            _subscription = _valueCache.Subscribe(newValue =>
            {
                DebugMessage("PropertyFacade: Received value on viewmodel='{0}', property='{1}', value='{2}'", _viewModel, _propertyName, newValue);

                if (!equalityComparer.Equals(_oldValue, newValue))
                {
                    // Only pump HasChanges if the new HasChanges value is different.

                    bool changed = !equalityComparer.Equals(OriginalValue, newValue);

                    if (_hasChangesSubject.Value != changed)
                    {
                        _hasChangesSubject.OnNext(changed);
                    }

                    RaisePropertyChanged(newValue);
                }
            });
        }

        private IEqualityComparer<TValue> GetEqualityComparer<TValue>()
        {
            if (typeof(TValue) == typeof(string))
            {
                return (IEqualityComparer<TValue>)(IEqualityComparer<string>)NullOrEmptyStringEqualityComparer.Default;
            }
            else
            {
                return EqualityComparer<TValue>.Default;
            }
        }

        private void RaisePropertyChanged(T newValue)
        {
            DebugMessage("PropertyFacade: RaisePropertyChanging: '{0}', property='{1}', old='{2}', new='{3}'", _viewModel, _propertyName, _oldValue, newValue);
            _viewModel.RaisePropertyChanging(_propertyName);

            var oldTemp = _oldValue;
            _oldValue = newValue;

            DebugMessage("PropertyFacade: RaisePropertyChanged: '{0}', property='{1}', old='{2}', new='{3}'", _viewModel, _propertyName, oldTemp, newValue);
            _viewModel.RaisePropertyChanged(_propertyName);
        }

        public void UpdateOriginalValue()
        {
            OriginalValue = Value;

            if (HasChanges.Value)
            {
                _hasChangesSubject.OnNext(false);
            }
        }

        /// <summary>
        /// Disconnects any existing subscription. If none exists, no action is taken.
        /// </summary>
        public void Dispose()
        {
            IsDisposed = true;
            _subscription?.Dispose();
            _valueCache.Dispose();
        }

        [DebuggerStepThrough]
        protected void DebugMessage(string message, params object[] args)
        {
            if (!SuppressDebugOutput && Debugger.IsAttached)
            {
                Debug.WriteLine(message, args);
            }
        }
    }
}
