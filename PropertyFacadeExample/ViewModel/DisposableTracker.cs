using System;
using System.Collections.Generic;

namespace PropertyFacadeExample.ViewModel
{
    /// <summary>
    /// Tracks a list of <see cref="IDisposable"/> objects. Prefer this over <see cref="CompositeDisposable"/>
    /// to avoid unnecessary allocations where no items need to be tracked.
    /// </summary>
    public sealed class DisposableTracker
    {
        private List<IDisposable> Disposables { get; set; }

        public void Add(IDisposable disposable)
        {
            if (disposable is null)
            {
                throw new ArgumentNullException(nameof(disposable));
            }

            if (Disposables is null)
            {
                Disposables = new List<IDisposable>();
            }

            Disposables.Add(disposable);
        }

        /// <summary>
        /// Disposes all tracked items and removes them from the list.
        /// </summary>
        public void Dispose()
        {
            if (!(Disposables is null))
            {
                foreach (var disposable in Disposables)
                {
                    disposable.Dispose();
                }

                Disposables.Clear();
            }
        }
    }
}
