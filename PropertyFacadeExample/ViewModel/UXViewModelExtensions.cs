using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PropertyFacadeExample.ViewModel
{
    public static class UXViewModelExtensions
    {
        public static PropertyFacade<T> PropFacade<T, TViewModel>(this TViewModel vm, Expression<Func<TViewModel, T>> property)
            where TViewModel : UXViewModel
        {
            if (vm is null)
            {
                throw new ArgumentNullException(nameof(vm));
            }

            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            var memberExpr = (MemberExpression)property.Body;

            string propName = memberExpr.Member.Name;

            return new PropertyFacade<T>(vm, propName);
        }

        public static ReadOnlyPropertyFacade<T> ReadOnlyPropFacade<T, TViewModel>(this TViewModel vm, Expression<Func<TViewModel, T>> property)
            where TViewModel : UXViewModel
        {
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            var memberExpr = (MemberExpression)property.Body;

            string propName = memberExpr.Member.Name;

            return new ReadOnlyPropertyFacade<T>(vm, propName);
        }

        public static void ToProperty<T>(this IObservable<T> observable, UXViewModel viewModel, ReadOnlyPropertyFacade<T> propertyFacade, IEqualityComparer<T> equalityComparer = null)
        {
            if (observable is null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            if (viewModel is null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            if (propertyFacade is null)
            {
                throw new ArgumentNullException(nameof(propertyFacade), "The property facade has not been initialized yet.");
            }

            propertyFacade.Observe(observable, equalityComparer);
            viewModel.DisposableTracker.Add(propertyFacade);
        }

        public static T DisposeWith<T>(this T disposable, UXViewModel viewModel) where T : class, IDisposable
        {
            if (disposable is null)
            {
                throw new ArgumentNullException(nameof(disposable));
            }

            if (viewModel is null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            viewModel.DisposableTracker.Add(disposable);

            return disposable;
        }

        public static T TrackChanges<T>(this T subject, ChangeTracker changeTracker)
            where T : class, IHasChanges
        {
            if (subject is null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (changeTracker is null)
            {
                throw new ArgumentNullException(nameof(changeTracker));
            }

            changeTracker.Add(subject);

            return subject;
        }

        public static T TrackChanges<T>(this T subject, ITracksChanges trackerOwner)
            where T : class, IHasChanges
        {
            if (subject is null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (trackerOwner is null)
            {
                throw new ArgumentNullException(nameof(trackerOwner));
            }

            return TrackChanges(subject, trackerOwner.ChangeTracker);
        }
    }
}
