using System;
using System.Diagnostics;
using System.Reactive;

namespace PropertyFacadeExample.Domain
{
    public interface ICachedObservable<T> : IReadOnlyValueObservable<T>
    {
        bool HasValue { get; }
    }

    /// <summary>
    /// Represents an observable that caches the latest value of another observable.
    /// This observable has the same subscription semantics as ReplaySubject(1): if there is a value present, the value will replay when a subscription is made.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("(CachedObservable) {Value}")]
    public sealed class CachedObservable<T> : ObservableBase<T>, ICachedObservable<T>
    {
        private readonly IDisposable _registration;
        private readonly IObservable<T> _source;

        public bool HasValue { get; private set; }

        public T Value { get; private set; }

        public CachedObservable(IObservable<T> source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));

            _registration = _source.Subscribe(x =>
            {
                HasValue = true;
                Value = x;
            });
        }

        public void Dispose() => _registration.Dispose();

        protected override IDisposable SubscribeCore(IObserver<T> observer)
        {
            if (observer is null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            var sub = _source.SubscribeSafe(observer);

            if (HasValue)
            {
                observer.OnNext(Value);
            }

            return sub;
        }

        public override string ToString() => $"(CachedObservable) {Value}";
    }

    public static class CachedObservableExtensions
    {
        public static ICachedObservable<T> ToCached<T>(this IObservable<T> source) => new CachedObservable<T>(source);
    }
}
