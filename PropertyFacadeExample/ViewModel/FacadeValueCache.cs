using PropertyFacadeExample.Domain;
using System;
using System.Reactive.Subjects;

namespace PropertyFacadeExample.ViewModel
{
    /// <summary>
    /// Encapsulates an observable or subject to provide an efficient way to get or set the latest value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FacadeValueCache<T> : IObservable<T>, IDisposable
    {
        private IDisposable _disposable;
        private IObservable<T> _observable;
        private Func<T> _getValueFunc;
        private Action<T> _setValueFunc;

        public bool CanSet => !(_setValueFunc is null);

        public bool HasObservable => !(_observable is null);

        public void Attach(IObservable<T> obs)
        {
            if (obs is null)
            {
                throw new ArgumentNullException(nameof(obs));
            }

            Dispose();

            if (obs is IReadOnlyValueObservable<T> iObsVal)
            {
                _observable = iObsVal;
                _getValueFunc = () => iObsVal.Value;
            }
            else if (obs is BehaviorSubject<T> behSub)
            {
                _observable = behSub;
                _getValueFunc = () => behSub.Value;
            }
            else
            {
                var cached = obs.ToCached();

                _observable = cached;
                _disposable = cached;

                _getValueFunc = () => cached.Value;
            }

            if (obs is IObserver<T> iSub)
            {
                _setValueFunc = value => iSub.OnNext(value);
            }
        }

        private void AssertHasObservable()
        {
            if (_observable is null)
            {
                throw new InvalidOperationException("No observable has been attached.");
            }
        }

        public T GetValue()
        {
            AssertHasObservable();

            return _getValueFunc.Invoke();
        }

        public void SetValue(T value)
        {
            AssertHasObservable();

            if (CanSet)
            {
                _setValueFunc.Invoke(value);
            }
            else
            {
                throw new InvalidOperationException("The observable does not implement IObserver.");
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            AssertHasObservable();

            return _observable.Subscribe(observer);
        }

        public void Dispose()
        {
            _disposable?.Dispose();
            _observable = null;
            _getValueFunc = null;
            _setValueFunc = null;
        }
    }
}
