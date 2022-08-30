using PropertyFacadeExample.Domain;
using System;

namespace PropertyFacadeExample.ViewModel
{
    public class PropertyFacade<T> : ReadOnlyPropertyFacade<T>, IHasChanges
    {
        public new T Value
        {
            get => base.Value;
            set
            {
                if (IsDisposed)
                {
                    throw new InvalidOperationException("The subscription for this observable has been disposed. Attach a new subscription by calling Observe.");
                }

                if (_valueCache is null)
                {
                    throw new InvalidOperationException("The observable has not been set yet. Attach a new subscription by calling Observe.");
                }

                _valueCache.SetValue(value);
            }
        }

        public PropertyFacade(UXViewModel viewModel, string propertyName)
            : base(viewModel, propertyName)
        { }

        public PropertyFacade(UXViewModel viewModel, IValueObservable<T> observable, string propertyName)
            : base(viewModel, observable, propertyName)
        { }

        public void ResetValue() => Value = OriginalValue;
    }
}
