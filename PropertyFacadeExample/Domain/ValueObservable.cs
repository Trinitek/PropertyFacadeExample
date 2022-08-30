using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace PropertyFacadeExample.Domain
{

    public interface IReadOnlyValueObservable<T> : IObservable<T>, IDisposable
    {
        T Value { get; }
    }

    public interface IValueObservable<T> : IReadOnlyValueObservable<T>, ISubject<T>
    {
        new T Value { get; set; }
    }

    /// <summary>
    /// Same thing as Observable.Return() except as a <see cref="IReadOnlyValueObservable{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ReadOnlyConstantValueObservable<T> : IReadOnlyValueObservable<T>
    {
        public T Value { get; }

        public ReadOnlyConstantValueObservable(T value) => Value = value;

        public void Dispose()
        { }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer is null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            observer.OnNext(Value);

            return Disposable.Empty;
        }
    }

    public delegate T CoerceValueHandler<T>(T oldValue, T newValue);

    /// <summary>
    /// Represents a reactive subject that caches the latest value. It exhibits the same behavior as
    /// <see cref="BehaviorSubject{T}"/> except it has a settable <see cref="Value"/> property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("(ValueObservable) {Value}")]
    public sealed class ValueObservable<T> : IValueObservable<T>
    {
        private readonly BehaviorSubject<T> _subject;
        private readonly CoerceValueHandler<T> _coerceValue;

        public ValueObservable(T initialValue = default, CoerceValueHandler<T> coerceValue = default, bool delayCoercer = false)
        {
            _coerceValue = coerceValue ?? ((_, newValue) => newValue);
            _subject = new BehaviorSubject<T>(delayCoercer ? initialValue : _coerceValue.Invoke(default, initialValue));
        }

        public T Value
        {
            get => _subject.Value;
            set => OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer) => _subject.SubscribeSafe(observer);

        public void OnCompleted() => _subject.OnCompleted();

        public void OnError(Exception error) => _subject.OnError(error);

        public void OnNext(T newValue) => _subject.OnNext(_coerceValue.Invoke(Value, newValue));

        public void Dispose() => _subject.Dispose();

        public override string ToString() => $"(ValueObservable<{typeof(T).Name}>) {Value}";

        public void CoerceCurrent() => _coerceValue.Invoke(Value, Value);

        public IDisposable GetCoercerDelayToken() => Disposable.Create(() => CoerceCurrent());
    }

    /// <summary>
    /// Allows you to build custom observer logic for complex coercion scenarios.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ValueObservableFacade<T> : IValueObservable<T>
    {
        private readonly IObservable<T> _observable;
        private readonly IObserver<T> _observer;
        private readonly Func<T> _getValue;
        private readonly Action<T> _setValue;

        public ValueObservableFacade(IObservable<T> observable, IObserver<T> observer, Func<T> getValue, Action<T> setValue = null)
        {
            _observable = observable ?? throw new ArgumentNullException(nameof(observable));
            _observer = observer ?? throw new ArgumentNullException(nameof(observer));
            _getValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
            _setValue = setValue;

            _observable.Subscribe(next => Value = next);
        }

        /// <summary>
        /// Gets or sets the value. If the value is the same as the one stored, observers will not be notified. Use <see cref="OnNext(T)"/> to override this.
        /// </summary>
        public T Value
        {
            get => _getValue();
            set
            {
                if (!Equals(value, _getValue()))
                {
                    OnNext(value);
                }
            }
        }

        public void Dispose()
        {
            if (_observer is IDisposable d)
            {
                d.Dispose();
            }
        }

        public void OnCompleted() => _observer.OnCompleted();

        public void OnError(Exception error) => _observer.OnError(error);

        public void OnNext(T value)
        {
            _setValue?.Invoke(value);
            _observer.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer) => _observable.SubscribeSafe(observer);
    }
}
