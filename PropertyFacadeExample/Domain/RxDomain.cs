using System;

namespace PropertyFacadeExample.Domain
{
    internal static class RxDomain
    {
        public static IValueObservable<T> ObservableProperty<T>()
            => new ValueObservable<T>();

        public static IValueObservable<T> ObservableProperty<T>(T defaultValue, CoerceValueHandler<T> coerceValue = null)
            => new ValueObservable<T>(defaultValue, coerceValue);

        public static IValueObservable<T> ObservableProperty<T>(T defaultValue, CoerceValueHandler<T> coerceValue, out IDisposable coercerDelayToken)
        {
            var obs = new ValueObservable<T>(defaultValue, coerceValue, delayCoercer: true);
            coercerDelayToken = obs.GetCoercerDelayToken();
            return obs;
        }

        public static IValueObservable<T> ObservableProperty<T>(CoerceValueHandler<T> coerceValue)
            => new ValueObservable<T>(default, coerceValue);

        public static IReadOnlyValueObservable<T> ReadOnlyConstant<T>(T value)
            => new ReadOnlyConstantValueObservable<T>(value);
    }
}
