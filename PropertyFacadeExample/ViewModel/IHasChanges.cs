using PropertyFacadeExample.Domain;

namespace PropertyFacadeExample.ViewModel
{
    public interface IHasChanges
    {
        IReadOnlyValueObservable<bool> HasChanges { get; }
        void ResetValue();
        void UpdateOriginalValue();
    }
}
