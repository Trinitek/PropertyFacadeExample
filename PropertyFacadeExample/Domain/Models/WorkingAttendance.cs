using System.Linq;
using System.Reactive.Linq;
using static PropertyFacadeExample.Domain.RxDomain;

namespace PropertyFacadeExample.Domain.Models
{
    /// <summary>
    /// An example domain model that encapsulates working time in hours.
    /// </summary>
    public sealed class WorkingAttendance
    {
        public IValueObservable<decimal> AdminATime { get; }
        public IValueObservable<decimal> AdminBTime { get; }
        public IValueObservable<decimal> NonSalaryTime { get; }
        public IValueObservable<decimal> SalaryTime { get; }
        public IValueObservable<decimal> TravelTime { get; }
        public IValueObservable<decimal> LeaveTime { get; }

        public IReadOnlyValueObservable<decimal> TotalTime { get; }

        public WorkingAttendance(
            decimal administrativeATime,
            decimal administrativeBTime,
            decimal nonSalaryTime,
            decimal salaryTime,
            decimal travelTime,
            decimal leaveTime)
        {
            AdminATime = ObservableProperty(administrativeATime, ClipNegativeHoursToZero);
            AdminBTime = ObservableProperty(administrativeBTime, ClipNegativeHoursToZero);
            NonSalaryTime = ObservableProperty(nonSalaryTime);
            SalaryTime = ObservableProperty(salaryTime);
            TravelTime = ObservableProperty(travelTime);
            LeaveTime = ObservableProperty(leaveTime, ClipNegativeHoursToZero);

            TotalTime = Observable.CombineLatest(
                AdminATime,
                AdminBTime,
                NonSalaryTime,
                SalaryTime,
                TravelTime,
                LeaveTime)
                .Select(hours => hours.Sum())
                .ToCached();
        }

        private static decimal ClipNegativeHoursToZero(decimal _, decimal newTime) => newTime < 0 ? 0 : newTime;
    }
}
