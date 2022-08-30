using System;

namespace PropertyFacadeExample.ViewModel.Features
{
    public sealed class EditWorkingAttendanceViewModel : UXViewModel, ITracksChanges
    {
        public ChangeTracker ChangeTracker { get; } = new ChangeTracker();

        public EditWorkingAttendanceViewModel()
        {
            // Also includes change tracking support, which is optional.

            // DisposeWith registers the subscription with DisposableTracker.
            // This is necessary if you expect the domain model to live longer than the viewmodel.
            // When you close the viewmodel, call DisposableTracker.Dispose().
            //
            // You can dispose a PropertyFacade as many times as you need; this only disposes the underlying
            // subscription if one is present. Likewise you can re-use and re-dispose the tracker.
            //
            // Take care when creating and registering regular Rx subscriptions here in the constructor if you
            // intend to reuse the viewmodel after Dispose. If you dispose those subscriptions, they're gone.

            _adminA = this.PropFacade(vm => vm.AdminA).TrackChanges(this).DisposeWith(this);
            _adminB = this.PropFacade(vm => vm.AdminB).TrackChanges(this).DisposeWith(this);
            _nonSalary = this.PropFacade(vm => vm.NonSalary).TrackChanges(this).DisposeWith(this);
            _salary = this.PropFacade(vm => vm.Salary).TrackChanges(this).DisposeWith(this);
            _travel = this.PropFacade(vm => vm.Travel).TrackChanges(this).DisposeWith(this);
            _leave = this.PropFacade(vm => vm.Leave).TrackChanges(this).DisposeWith(this);
            _total = this.ReadOnlyPropFacade(vm => vm.Total).DisposeWith(this);
        }

        public decimal AdminA { get => _adminA.Value; set => _adminA.Value = value; }
        private readonly PropertyFacade<decimal> _adminA;

        public decimal AdminB { get => _adminB.Value; set => _adminB.Value = value; }
        private readonly PropertyFacade<decimal> _adminB;

        public decimal NonSalary { get => _nonSalary.Value; set => _nonSalary.Value = value;  }
        private readonly PropertyFacade<decimal> _nonSalary;

        public decimal Salary { get => _salary.Value; set => _salary.Value = value; }
        private readonly PropertyFacade<decimal> _salary;

        public decimal Travel { get => _travel.Value; set => _travel.Value = value; }
        private readonly PropertyFacade<decimal> _travel;

        public decimal Leave { get => _leave.Value; set => _leave.Value = value; }
        private readonly PropertyFacade<decimal> _leave;

        public decimal Total => _total.Value;
        private readonly ReadOnlyPropertyFacade<decimal> _total;

        public void Load(Domain.Models.WorkingAttendance model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // PropertyFacade supports hot-swapping subscriptions.
            // If we want to treat the domain model as more-or-less discardable while keeping the viewmodel alive,
            // we can replace an older model with a new one.

            // We can also use ConvertOneWay or ConvertTwoWay extension methods from FacadeConverters.cs to convert value types
            // if the PropertyFacade type and the domain property type do not match. We may want to do this if we
            // need to "play nice" with a UI control that doesn't like the property on the domain.
            // To use this, place ConvertTwoWay() before ToProperty().

            model.AdminATime.ToProperty(this, _adminA);
            model.AdminBTime.ToProperty(this, _adminB);
            model.NonSalaryTime.ToProperty(this, _nonSalary);
            model.SalaryTime.ToProperty(this, _salary);
            model.TravelTime.ToProperty(this, _travel);
            model.LeaveTime.ToProperty(this, _leave);
            model.TotalTime.ToProperty(this, _total);
        }
    }
}
