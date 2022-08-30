using PropertyFacadeExample.Domain;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace PropertyFacadeExample.ViewModel
{
    public sealed class ChangeTracker : IHasChanges
    {
        private readonly List<IHasChanges> _subjects = new List<IHasChanges>();

        public IReadOnlyValueObservable<bool> HasChanges { get; }
        private readonly BehaviorSubject<bool> _hasChangesSubject = new BehaviorSubject<bool>(default);

        private int TrueCount
        {
            get => _trueCount;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), $"Value must not be negative ({value}).");
                }

                _trueCount = value;
            }
        }
        private int _trueCount;

        public ChangeTracker()
        {
            HasChanges = _hasChangesSubject.ToCached();
        }

        public void Add(IHasChanges subject)
        {
            if (subject is null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            _subjects.Add(subject);

            bool init = true;

            subject.HasChanges.Subscribe(hasChanges =>
            {
                // Do not count no-changes when attaching to the observable.
                if (init && !hasChanges)
                {
                    return;
                }

                TrueCount += hasChanges ? 1 : -1;

                if (TrueCount == 0)
                {
                    _hasChangesSubject.OnNext(false);
                }
                else if (TrueCount == 1 && hasChanges)
                {
                    _hasChangesSubject.OnNext(true);
                }
            });

            init = false;
        }

        public void ResetValue()
        {
            foreach (var facade in _subjects)
            {
                facade.ResetValue();
            }
        }

        public void UpdateOriginalValue()
        {
            foreach (var facade in _subjects)
            {
                facade.UpdateOriginalValue();
            }
        }
    }
}
