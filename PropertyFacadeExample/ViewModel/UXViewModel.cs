using System.ComponentModel;

namespace PropertyFacadeExample.ViewModel
{
    /// <summary>
    /// A base view model implementation. If we were using ReactiveUI, this would also extend from ReactiveObject.
    /// </summary>
    public abstract class UXViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        public void RaisePropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);
        public void RaisePropertyChanged(string propertyName) => RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));

        public void RaisePropertyChanging(PropertyChangingEventArgs args) => PropertyChanging?.Invoke(this, args);
        public void RaisePropertyChanging(string propertyName) => RaisePropertyChanging(new PropertyChangingEventArgs(propertyName));

        public DisposableTracker DisposableTracker { get; } = new DisposableTracker();
    }
}
